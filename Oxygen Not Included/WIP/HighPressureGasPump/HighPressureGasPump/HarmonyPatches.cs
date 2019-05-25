using System.Collections.Generic;
using Harmony;
using Database;
using STRINGS;

namespace HighPressureGasPump
{
    public class HarmonyPatches
    {
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class PipedElectrolyzer_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                string prefix = "STRINGS.BUILDINGS.PREFABS." + HighPressureGasPumpConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "High Pressure Gas Pump");
                Strings.Add(prefix + ".DESC", "Piping a pump's intake to another building's output will send gas to that building.");
                Strings.Add(prefix + ".EFFECT", "Draws in a large amount of gas " + UI.FormatAsLink("Gas", "ELEMENTS_GAS") + " runs it through " + UI.FormatAsLink("Pipes", "GASPIPING") + ".\n\nMust be immersed in " + UI.FormatAsLink("Gas", "ELEMENTS_GAS") + ".");
                ModUtil.AddBuildingToPlanScreen("HVAC", HighPressureGasPumpConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class PipedElectrolyzer_Db_Initialize
        {
            public static void Prefix()
            {
                List<string> list = new List<string>(Techs.TECH_GROUPING["DirectedAirStreams"]);
                list.Add(HighPressureGasPumpConfig.ID);
                Techs.TECH_GROUPING["DirectedAirStreams"] = list.ToArray();
            }
        }
    }
}
