using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using HighPressurePipes;
using UnityEngine;
namespace ExtremePipes
{
    public class HarmonyPatches
    {
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class PipedElectrolyzer_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                string prefix = "STRINGS.BUILDINGS.PREFABS." + ExtremeGasConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Extreme Gas Pipe");
                Strings.Add(prefix + ".DESC", "Crikey.");
                Strings.Add(prefix + ".EFFECT", "WOAH! Carries a whole buncha bunch of gas.");
                ModUtil.AddBuildingToPlanScreen("HVAC", ExtremeGasConduitConfig.ID);
                PressurizedTuning.TryAddPressurizedInfo(ExtremeGasConduitConfig.ID, new PressurizedInfo()
                {
                    Capacity = 10f,
                    IncreaseMultiplier = 10f,
                    KAnimTint = new Color32(240, 40, 120, 255),
                    OverlayTint = new Color32(201, 0, 30, 0),
                    FlowTint = new Color32(200, 40, 30, 255),
                    FlowOverlayTint = new Color32(255, 40, 10, 0),
                    IsDefault = false
                });
                prefix = "STRINGS.BUILDINGS.PREFABS." + ExtremeLiquidConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Extreme Liquid Pipe");
                Strings.Add(prefix + ".DESC", "Crikey.");
                Strings.Add(prefix + ".EFFECT", "WOAH! Carries a whole buncha bunch of liquid.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", ExtremeLiquidConduitConfig.ID);
                PressurizedTuning.TryAddPressurizedInfo(ExtremeLiquidConduitConfig.ID, new PressurizedInfo()
                {
                    Capacity = 100f,
                    IncreaseMultiplier = 10f,
                    KAnimTint = new Color32(240, 40, 120, 255),
                    OverlayTint = new Color32(201, 0, 30, 0),
                    FlowTint = new Color32(200, 40, 30, 255),
                    FlowOverlayTint = new Color32(255, 40, 10, 0),
                    IsDefault = false
                });
            }
        }
    }
}
