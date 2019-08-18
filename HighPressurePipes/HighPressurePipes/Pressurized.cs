using UnityEngine;
using System.Runtime.Serialization;
using KSerialization;
namespace PressurizedPipes
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class Pressurized : KMonoBehaviour, ISaveLoadable
    {
        [SerializeField]
        public ConduitType ConduitType;
        [SerializeField]
        public bool IsBridge = false;
        public PressurizedInfo Info;

        [MyCmpGet]
        private Building building;
        [MyCmpGet]
        private Conduit conduit;
        [MyCmpGet]
        private ConduitBridge bridge;

        private bool _loadedInfo = false;

        [OnDeserialized]
        private void OnDeserialized()
        {
            _loadedInfo = true;
            string id = building.Def.PrefabID;
            Info = PressurizedTuning.GetPressurizedInfo(id);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if(conduit == null && bridge == null)
                Debug.LogError($"[Pressurized] Pressurized component should not be added unless there is an accomponying Conduit or ConduitBridge component.");
            if (!_loadedInfo)
            {
                _loadedInfo = true;
                string id = building.Def.PrefabID;
                IsBridge = bridge != null;
                Info = PressurizedTuning.GetPressurizedInfo(id);
            }
            if (!Info.IsDefault)
            {
                KAnimControllerBase kAnim = this.GetComponent<KAnimControllerBase>();
                if (kAnim != null)
                    kAnim.TintColour = Info.KAnimTint;
                else
                    Debug.LogWarning($"[Pressurized] Conduit.OnSpawn() KAnimControllerBase component was null!");
            }
        }

        public int GetLayer()
        {
            if (IsBridge)
                return Integration.connectionLayers[(int)ConduitType];
            else
                return Integration.layers[(int)ConduitType];
        }

        public static bool IsDefault(Pressurized pressure)
        {
            return pressure == null || pressure.Info == null || pressure.Info.IsDefault || pressure.Info.Capacity <= 0f;
        }
        
    }
}
