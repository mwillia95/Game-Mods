using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Harmony;
using System.Reflection;

namespace LazyStockpiles
{
    public enum LazyType { Normal, Cache, Buffer }

    public class LazySettings
    {
        public StorageSettings settings;
        public LazyType type;
        public static float DefaultCache = (float)1 / (float)3;
        public static float DefaultBuffer = 0.75f;
        public float BufferThreshold;
        public float CacheThreshold;

        public LazySettings(StorageSettings s)
        {
            settings = s;
            BufferThreshold = DefaultBuffer;
            CacheThreshold = DefaultCache;
            type = LazyType.Cache;
        }
    }
    public class LazySettingsManager
    {
        public static List<LazySettings> LazySettings = new List<LazySettings>();
        //references to ITab_Storage.WinSize are to replaced with a reference to this variable
        public static Vector2 WinSize = new Vector2(300f, 575f); 
        public static LazySettings AddOrGetSettings(StorageSettings settings)
        {
            if (settings == null)
                return null;
            LazySettings result = null;
            foreach(LazySettings s in LazySettings)
            {
                if(s.settings == settings)
                {
                    result = s;
                    break;
                }
            }

            if(result == null)
            {
                result = AddSettings(settings);
            }
            return result;
        }

        public static float VariableTabHeight(StorageSettings settings)
        {
            LazySettings lazySett = LazySettingsManager.AddOrGetSettings(settings);
            float baseValue = 35f;
            if(lazySett?.type != LazyType.Normal)
            {
                baseValue += 40f;
            }
            return baseValue;
        }

        //public static float ModifyPaneTopY(float f, ITab instance)
        //{
        //    if(instance as ITab_Storage != null)
        //    {
        //        return f - 95f;
        //    }
        //    return f;
        //}

        private static LazySettings AddSettings(StorageSettings settings)
        {
            LazySettings result = new LazySettings(settings);
            LazySettings.Add(result);
            return result;
        }
        private static bool logged = false;
        public static void ITab_Storage_Patch(ITab_Storage instance, StorageSettings settings)
        {
            //Log.Message("ITab_Storage_Patch was called");
            //if (!logged)
            //{
            //    Log.Message($"ITab_Storage instance == null: {instance == null}");
            //    Log.Message($"StorageSettings settings == null: {settings == null}");
            //    logged = true;
            //}
            object o = AccessTools.Field(typeof(ITab_Storage), "size").GetValue(instance);
            Vector2 size = (Vector2)o;
            //Log.Message($"[NOTICE] ITab_Storage size -> x: {size.x} y: {size.y}");
            Vector2 WinSize = new Vector2(300f, 575f);
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 35f, 160f, 35f - 6f);
            LazySettings stockpile = LazySettingsManager.AddOrGetSettings(settings);
            if (stockpile == null)
            {
                Log.ErrorOnce(string.Format($"Lazy Tab Error: Attempted to load settings as LazySettings, when it was of type {settings.GetType()}"), 0);
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
    }
}
