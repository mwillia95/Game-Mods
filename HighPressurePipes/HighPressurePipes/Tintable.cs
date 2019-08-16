
using UnityEngine;
namespace HighPressurePipes
{
    public class Tintable : KMonoBehaviour, ISaveLoadable
    {
        [MyCmpGet]
        private KAnimControllerBase controller;
        [SerializeField]
        public Color32 TintColour = new Color32(255, 255, 255, 255);

        protected override void OnSpawn()
        {
            base.OnSpawn();
            controller.TintColour = TintColour;
        }
        public void ResetTint()
        {
            controller.TintColour = TintColour;
        }

        private void SetTintColour()
        {
            //string id = GetComponent<Building>()?.Def.PrefabID;
            //if(id == )
        }
    }
}
