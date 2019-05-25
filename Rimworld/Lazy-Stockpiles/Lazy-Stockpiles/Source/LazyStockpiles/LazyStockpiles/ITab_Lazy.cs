using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace LazyStockpiles
{
    /// <summary>
    /// Defines the tab that appears when the user selects the tab for a lazy stockpile
    /// On top of the regular storage tab options, this tab has options for the type of the storage, Normal, Cache, or Buffer
    /// If Cache or Buffer are selected, a slider will be provided to allow the user to select the threshold for that type (Cache 0.0-0.75 Buffer 0.25-1.0)
    /// </summary>
    public class ITab_Lazy : ITab_Storage
    {
        private Vector2 scrollPosition;

        public static readonly Vector2 WinSize = new Vector2(300f, 575f);


        private float TopAreaHeight
        {
            get
            {
                return (float)((!this.IsPrioritySettingVisible) ? 20 : 35);
            }
        }

        public ITab_Lazy()
        {
            this.size = ITab_Lazy.WinSize;
            this.labelKey = "TabStorage";
            this.tutorTag = "Storage";
        }


        protected override void FillTab()
        {
            IStoreSettingsParent selStoreSettingsParent = this.SelStoreSettingsParent;
            StorageSettings settings = selStoreSettingsParent.GetStoreSettings();

            Rect position = new Rect(0f, 0f, ITab_Lazy.WinSize.x, ITab_Lazy.WinSize.y).ContractedBy(10f);
            GUI.BeginGroup(position);
            if (this.IsPrioritySettingVisible)
            {
                Text.Font = GameFont.Small;
                Rect rect = new Rect(0f, 0f, 160f, this.TopAreaHeight - 6f);
                if (Widgets.ButtonText(rect, "Priority".Translate() + ": " + settings.Priority.Label(), true, false, true))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (StoragePriority storagePriority in Enum.GetValues(typeof(StoragePriority)))
                    {
                        if (storagePriority != StoragePriority.Unstored)
                        {
                            StoragePriority localPr = storagePriority;
                            list.Add(new FloatMenuOption(localPr.Label().CapitalizeFirst(), delegate
                            {
                                settings.Priority = localPr;
                            }, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                UIHighlighter.HighlightOpportunity(rect, "StoragePriority");
            }
            //Lazy button & Threshold Sliders
            {
                Text.Font = GameFont.Small;
                Rect rect = new Rect(0f, 35f, 160f, this.TopAreaHeight - 6f);
                LazySettings stockpile = LazySettingsManager.AddOrGetSettings(settings);
                if (stockpile == null)
                {
                    Log.Error(string.Format($"Lazy Tab Error: Attempted to load {SelObject} settings as LazySettings, when it was of type {settings.GetType()}"));
                }
                else
                {
                    //Type Button
                    if (Widgets.ButtonText(rect, "Type: " + stockpile.type.ToString(), true, false, true))
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        foreach (LazyType type in Enum.GetValues(typeof(LazyType)))
                        {
                            LazyType localTy = type;
                            list.Add(new FloatMenuOption(type.ToString(), delegate
                            {
                                    stockpile.type = type;
                            }, MenuOptionPriority.Default, null, null, 0f, null, null));
                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                    }
                    //Cache Threshold Slider
                    if (stockpile.type == LazyType.Cache)
                    {
                        Rect sliderRect = new Rect(0f, 66f, WinSize.x - 20f, 70f);
                        Listing_Standard stand = new Listing_Standard();
                        stand.Begin(sliderRect);
                        stand.Label(string.Format($"Cache Threshold: {stockpile.CacheThreshold * 100:0}%"));
                        stockpile.CacheThreshold = stand.Slider(stockpile.CacheThreshold, 0f, 0.75f);
                        stand.End();
                    }
                    //Buffer Threshold Slider
                    else if (stockpile.type == LazyType.Buffer)
                    {
                        Rect sliderRect = new Rect(0f, 66f, WinSize.x - 20f, 70f);
                        Listing_Standard stand = new Listing_Standard();
                        stand.Begin(sliderRect);
                        stand.Label(string.Format($"Buffer Threshold: {stockpile.BufferThreshold * 100:0}%"));
                        stockpile.BufferThreshold = stand.Slider(stockpile.BufferThreshold, 0.25f, 1f);
                        stand.End();
                    }
                }    
            }


            ThingFilter parentFilter = null;
            if (selStoreSettingsParent.GetParentStoreSettings() != null)
            {
                parentFilter = selStoreSettingsParent.GetParentStoreSettings().filter;
            }
            Rect rect2 = new Rect(0f, (this.TopAreaHeight * 3) + 5, position.width, position.height - (this.TopAreaHeight * 2));

            Bill[] first = (from b in BillUtility.GlobalBills()
                            where b is Bill_Production && b.GetStoreZone() == selStoreSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
                            select b).ToArray();


            ThingFilterUI.DoThingFilterConfigWindow(rect2, ref this.scrollPosition, settings.filter, parentFilter, 8, null, null, false, null, null);

            Bill[] second = (from b in BillUtility.GlobalBills()
                             where b is Bill_Production && b.GetStoreZone() == selStoreSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
                             select b).ToArray();

            IEnumerable<Bill> enumerable = first.Except(second);
            foreach (Bill item in enumerable)
            {
                Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(item.LabelCap, item.billStack.billGiver.LabelShort.CapitalizeFirst(), item.GetStoreZone().label), item.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, false);
            }

            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
            GUI.EndGroup();
        }


    }
}
