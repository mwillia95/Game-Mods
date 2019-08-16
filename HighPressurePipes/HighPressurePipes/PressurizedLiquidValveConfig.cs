using TUNING;
using UnityEngine;

namespace HighPressurePipes
{
    public class PressurizedLiquidValveConfig : IBuildingConfig
    {
        public const string ID = "PressurizedLiquidValve";

        private const ConduitType CONDUIT_TYPE = ConduitType.Liquid;

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 1;
            int height = 2;
            string anim = "valveliquid_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] tIER = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0], BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0] }; //100KG, 100KG
            string[] constructionMaterial = { SimHashes.Steel.ToString(), MATERIALS.PLASTIC };
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER1;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, constructionMaterial, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER0, tIER2);
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.PermittedRotations = PermittedRotations.R360;
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 1);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            ValveBase valveBase = go.AddOrGet<ValveBase>();
            valveBase.conduitType = ConduitType.Liquid;
            valveBase.maxFlow = 30f;
            valveBase.animFlowRanges = new ValveBase.AnimRangeInfo[3]
            {
            new ValveBase.AnimRangeInfo(9f, "lo"),
            new ValveBase.AnimRangeInfo(21f, "med"),
            new ValveBase.AnimRangeInfo(30f, "hi")
            };
            go.AddOrGet<Valve>();
            Tintable tint = go.AddOrGet<Tintable>();
            tint.TintColour = new Color32(255, 160, 120, 255);
            Workable workable = go.AddOrGet<Workable>();
            workable.workTime = 5f;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Object.DestroyImmediate(go.GetComponent<RequireInputs>());
            Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
            Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
            go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
        }
    }
}
