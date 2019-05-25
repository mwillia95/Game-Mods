using System.Collections.Generic;
using RimWorld;
using Verse;
//namespace LazyStockpiles
//{
    /// <summary>
    /// The class for a lazy stockpile, inheriting from the default stockpile
    /// Uses the "resource preset" for default filters, and by default is a normal type
    /// Constructor forces the settings to be of type LazySettings instead of StorageSettings
    /// ExposeData saves the LazySetting variables, since StorageSettings ExposeData is not virtual
    /// </summary>
//    public class Lazy_Stockpile : Zone_Stockpile
//    {
//        private static readonly ITab StorageTab = new ITab_Lazy();

//        public Lazy_Stockpile(Zone_Stockpile z)
//        {
//            this.zoneManager = z.zoneManager;

//            this.slotGroup = z.slotGroup;
//            this.slotGroup.parent = this;
//            //Change the settings to be LazySettings
//            this.settings = new LazySettings(z.settings);
//            this.settings.owner = this;
//            //Make the slotgroup belong to this stockpile

//            //Pass other variables to this object
//            this.cells = z.cells;
//            //this.slotGroup.CellsList.Clear();
//            this.color = z.color;
//            this.hidden = z.hidden;
//            this.label = z.label;



//            //this.zoneManager.DeregisterZone(z);
//            //this.zoneManager.RegisterZone(this);
//        }
//        public Lazy_Stockpile()
//        {
//            this.settings = new LazySettings();
//        }
//        public override IEnumerable<InspectTabBase> GetInspectTabs()
//        {
//            yield return Lazy_Stockpile.StorageTab;
//        }

//        public override void ExposeData()
//        {
//            base.ExposeData();
//            if(Scribe.mode == LoadSaveMode.LoadingVars)
//                this.settings = new LazySettings(this.settings);
//            LazySettings settings = this.settings as LazySettings;
//            if (settings == null)
//                Log.Error("ExposeData error: Lazy_Stockpile " + this.label + " does not have LazySettings");
//            else
//            {
//                Scribe_Values.Look(ref settings.type, "settings.type", LazyType.Normal);
//                Scribe_Values.Look(ref settings.CacheThreshold, "settings.CacheThreshold", LazySettings.DefaultCache);
//                Scribe_Values.Look(ref settings.BufferThreshold, "settings.BufferThreshold", LazySettings.DefaultBuffer);
//            }
//        }
//    }
//}
