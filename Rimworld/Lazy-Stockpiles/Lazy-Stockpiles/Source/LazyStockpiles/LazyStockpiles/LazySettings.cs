using RimWorld;
using Verse;

//namespace LazyStockpiles
//{
//    /// <summary>
//    /// Inherits from StorageSettings, while containing variables for LazyType and thresholds
//    /// </summary>

//    public class LazySettings : StorageSettings
//    {
//        public LazyType type;
//        public static float DefaultCache = (float)1 / (float)3;
//        public static float DefaultBuffer = 0.75f;
//        public float BufferThreshold;
//        public float CacheThreshold;

//        //When loading a save
//        public LazySettings() : base()
//        {

//        }

//        //When loading a save to override the default StorageSettings
//        public LazySettings(StorageSettings settings, LazyType t = LazyType.Normal)
//        {
//            if (settings != null)
//            {
//                this.owner = settings.owner;
//                this.filter = settings.filter;
//                this.Priority = settings.Priority;
//            }
//            this.type = t;
//            CacheThreshold = DefaultCache;
//            BufferThreshold = DefaultBuffer;
//        }

//        //When loading
//        public LazySettings(IStoreSettingsParent owner) : base(owner)
//        {

//        }
//    }
//  }
