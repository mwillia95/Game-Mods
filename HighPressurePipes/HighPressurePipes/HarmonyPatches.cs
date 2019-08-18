using Database;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Linq;
using STRINGS;
namespace HighPressurePipes
{
    internal static class HarmonyPatches
    {
        //MaxMass is used by:
        //ConduitFlow.UpdateConduit
        //ConduitFlow.AddElement
        //ConduitFlow.OnDeserialized
        //ConduitFlow.IsConduitFull
        //ConduitFlow.FreezeConduitContents
        //ConduitFlow.MeltConduitContents
        private static readonly FieldInfo maxMass = AccessTools.Field(typeof(ConduitFlow), "MaxMass");
        //private static readonly FieldInfo bridgeInputCell = AccessTools.Field(typeof(ConduitBridge), "inputCell");
        //private static readonly Color32 PressurizedColor = new Color32(201, 80, 142, 0);
        //private static readonly Color32 PressurizedConduitColor = new Color32(66, 15, 12, 255);
        //private static readonly Color32 PressurizedConduitKAnimTint = new Color32(180, 80, 255, 255);

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class PipedElectrolyzer_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                //PRESSURIZED GAS PIPE
                string prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedGasConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Gas Pipe");
                Strings.Add(prefix + ".DESC", "Able to contain significantly more gas than standard gas pipes.");
                Strings.Add(prefix + ".EFFECT", $"Carries {UI.FormatAsLink("Gas", "ELEMENTS_GAS")} between {UI.FormatAsLink("Outputs", "GASPIPING")} and {UI.FormatAsLink("Intakes", "GASPIPING")}.\n\nCan carry a maximum of 3KG of gasses. Can also connect to regular gas pipes, but may damage pipes with a lower maximum capacity if the flow is too strong, especially to bridges.");
                ModUtil.AddBuildingToPlanScreen("HVAC", PressurizedGasConduitConfig.ID);
                //PRESSURIZED GAS BRIDGE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedGasConduitBridgeConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Gas Bridge");
                Strings.Add(prefix + ".DESC", "Is able to bridge upwards of 3KG of gas without damaging itself.");
                Strings.Add(prefix + ".EFFECT", $"A gas bridge built with steel and plastics.\n\nCan carry a maximum of 3KG of {UI.FormatAsLink("Gas", "ELEMENTS_LIQUID")}. May damage connected output pipes if too much flow is in the input pipe.");
                ModUtil.AddBuildingToPlanScreen("HVAC", PressurizedGasConduitBridgeConfig.ID);
                //PRESSURIZED LIQUID PIPE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedLiquidConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Liquid Pipe");
                Strings.Add(prefix + ".DESC", "Able to contain significantly more liquid than standard liquid pipes");
                Strings.Add(prefix + ".EFFECT", $"Carries {UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID")} between {UI.FormatAsLink("Outputs", "LIQUIDPIPING")} and {UI.FormatAsLink("Intakes", "LIQUIDPIPING")}.\n\nCan carry a maximum of 30KG of {UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID")}. Can also connect to regular liquid pipes, but may damage pipes with a lower maximum capacity if the flow is too strong, especially to bridges.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", PressurizedLiquidConduitConfig.ID);
                //PRESSURIZED LIQUID BRIDGE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedLiquidConduitBridgeConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Liquid Bridge");
                Strings.Add(prefix + ".DESC", "Is able to bridge upwards of 3KG of gas without damaging itself.");
                Strings.Add(prefix + ".EFFECT", $"A liquid bridge built with steel and plastics.\n\nCan carry a maximum of 30KG of {UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID")}. May damage connected output pipes if too much flow is in the input pipe.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", PressurizedLiquidConduitBridgeConfig.ID);
                //PRESSURIZED GAS VALVE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedGasValveConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Gas Valve");
                Strings.Add(prefix + ".DESC", "A gas valve that can fully support the flow of a Pressurized Gas Pipe.");
                Strings.Add(prefix + ".EFFECT", "Can limit flow by up to 3KG. Will not be damaged by strong flows and will not damage connected outputs.");
                ModUtil.AddBuildingToPlanScreen("HVAC", PressurizedGasValveConfig.ID);
                //PRESSURIZED LIQUID VALVE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedLiquidValveConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Liquid Valve");
                Strings.Add(prefix + ".DESC", "A liquid valve that can fully support the flow of a Pressurized Liquid Pipe.");
                Strings.Add(prefix + ".EFFECT", "Can limit flow by up to 30KG. Will not be damaged by strong flows and will not damage connected outputs.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", PressurizedLiquidValveConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class PipedElectrolyzer_Db_Initialize
        {
            public static void Prefix()
            {
                //List<string> list = new List<string>(Techs.TECH_GROUPING["ImprovedOxygen"]);
                //list.Add(PressurizedGasConduitConfig.ID);
                //Techs.TECH_GROUPING["ImprovedOxygen"] = list.ToArray();
                List<string> hvac = new List<string>(Techs.TECH_GROUPING["HVAC"]) { PressurizedGasConduitConfig.ID, PressurizedGasConduitBridgeConfig.ID };
                Techs.TECH_GROUPING["HVAC"] = hvac.ToArray();

                List<string> imprLiqPiping = new List<string>(Techs.TECH_GROUPING["ImprovedLiquidPiping"]) { PressurizedLiquidValveConfig.ID };
                Techs.TECH_GROUPING["ImprovedLiquidPiping"] = imprLiqPiping.ToArray();

                List<string> igp = new List<string>(Techs.TECH_GROUPING["ImprovedGasPiping"]) { PressurizedGasValveConfig.ID };
                Techs.TECH_GROUPING["ImprovedGasPiping"] = igp.ToArray();

                List<string> lt = new List<string>(Techs.TECH_GROUPING["LiquidTemperature"]) { PressurizedLiquidConduitConfig.ID, PressurizedLiquidConduitBridgeConfig.ID };
                (Techs.TECH_GROUPING["LiquidTemperature"]) = lt.ToArray();
            }
        }

        //our conduits are tinted and ResetDisplayValues will reset the tint when closing a gas/liquid overlay. reapply the tint
        [HarmonyPatch(typeof(OverlayModes.Mode), "ResetDisplayValues", new Type[] { typeof(KBatchedAnimController) })]
        internal static class Patch_OverModesMode_ResetDisplayValues
        {
            internal static void Postfix(KBatchedAnimController controller)
            {
                Pressurized pressure = controller.GetComponent<Pressurized>();
                if (!Pressurized.IsDefault(pressure))
                    controller.TintColour = pressure.Info.KAnimTint;
                else
                    controller.GetComponent<Tintable>()?.SetTint();
            }
        }

        [HarmonyPatch(typeof(ConduitBridge), "OnPrefabInit")]
        internal static class Patch_ConduitBridge_OnPrefabInit
        {
            internal static void Postfix(ConduitBridge __instance)
            {
                __instance.gameObject.AddOrGet<Pressurized>();
            }
        }
        [HarmonyPatch(typeof(Conduit), "OnPrefabInit")]
        internal static class Patch_Conduit_OnPrefabInit
        {
            internal static void Postfix(Conduit __instance)
            {
                __instance.gameObject.AddOrGet<Pressurized>();
            }
        }

        //Cannot trigger building damage inside of the conduit updates (where the need to damage is discovered). Trigger all damage at the end of each tick safely.
        [HarmonyPatch(typeof(Game), "Update")]
        internal static class Patch_Game_Update
        {
            internal static void Postfix()
            {
                List<Integration.QueueDamage> damages = Integration.queueDamages;
                if (damages.Count > 0)
                {
                    foreach (Integration.QueueDamage info in damages)
                    {
                        info.Receiver.Trigger((int)GameHashes.DoBuildingDamage, info.Damage);
                    }
                    damages.Clear();
                }
            }
        }

        //To change the color of how our pressurized pipes are displayed in their respective overlay
        [HarmonyPatch(typeof(OverlayModes.ConduitMode), "Update")]
        internal static class Patch_OvererlayModesConduitMode_Update
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                MethodBase patch = AccessTools.Method(typeof(Patch_OvererlayModesConduitMode_Update), nameof(PatchThermalColor));
                int layerTargetIdx = 12;
                int tintColourIdx = 15;
                bool foundVar = false;
                LocalVariableInfo layerTargetInfo = original.GetMethodBody().LocalVariables.FirstOrDefault(x => x.LocalIndex == layerTargetIdx);
                foundVar = layerTargetInfo != default(LocalVariableInfo);
                if (!foundVar)
                    Debug.LogError($"[Pressurized] OverlayModes.ConduitMode.Update() Transpiler -> Local variable signatures did not match expected signatures");

                foreach (CodeInstruction code in instructions)
                {
                    if (foundVar && code.opcode == OpCodes.Stloc_S && (code.operand as LocalVariableInfo).LocalIndex == tintColourIdx)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, layerTargetIdx);
                        yield return new CodeInstruction(OpCodes.Call, patch);
                    }
                    yield return code;
                }
            }
            private static HashSet<int> cells = new HashSet<int>();
            private static HashSet<int> cells2 = new HashSet<int>();

            private static Color32 PatchThermalColor(Color32 original, SaveLoadRoot layerTarget)
            {
                Pressurized pressurized = layerTarget.GetComponent<Pressurized>();
                if (pressurized != null && pressurized.Info != null && !pressurized.Info.IsDefault)
                    return pressurized.Info.OverlayTint;
                else
                    return original;
            }
        }


        //Modify MaxMass if needed for pressurized pipes when adding elements to a pipe
        [HarmonyPatch(typeof(ConduitFlow), "AddElement")]
        internal static class Patch_ConduitFlow_AddElement
        {

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in Integration.AddIntegrationIfNeeded(code, new CodeInstruction(OpCodes.Ldarg_1)))
                    {
                        yield return result;
                    }
                }

            }
        }

        //When Deserializing the contents inside of Conduits, the method will normally prevent the deserialized data from being higher than the built-in ConduitFlow MaxMass.
        //Instead, replace the max mass with infinity so the serialized mass will always be used.
        //Must be done this way because OnDeserialized is called before the Conduits are spawned, so no information is available as to what the max mass is supposed to be

        [HarmonyPatch(typeof(ConduitFlow), "OnDeserialized")]
        internal static class Patch_ConduitFlow_OnDeserialized
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                FieldInfo maxMass = AccessTools.Field(typeof(ConduitFlow), "MaxMass");
                MethodInfo patch = AccessTools.Method(typeof(Patch_ConduitFlow_OnDeserialized), "ReplaceMaxMass");
                foreach (CodeInstruction original in instructions)
                {
                    if (original.opcode == OpCodes.Ldfld && original.operand == maxMass)
                    {
                        yield return original;
                        yield return new CodeInstruction(OpCodes.Call, patch);
                    }
                    else
                        yield return original;
                }
            }

            internal static float ReplaceMaxMass(float original)
            {
                return float.PositiveInfinity;
            }
        }
        //Similar to above, for when moving contents through one pipe to the next
        [HarmonyPatch(typeof(ConduitFlow), "UpdateConduit")]
        internal static class Patch_ConduitFlow_UpdateConduit
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in Integration.AddIntegrationIfNeeded(code, new CodeInstruction(OpCodes.Ldloc_S, 14), true))
                    {
                        yield return result;
                    }
                }

            }
        }
        //Similar to above
        [HarmonyPatch(typeof(ConduitFlow), "IsConduitFull")]
        internal static class Patch_ConduitFlow_IsConduitFull
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in Integration.AddIntegrationIfNeeded(code, new CodeInstruction(OpCodes.Ldarg_1)))
                    {
                        yield return result;
                    }
                }
            }
        }
        //prevent the game from marking our pipes as radiant or insulated
        [HarmonyPatch(typeof(ConduitFlowVisualizer), "AddThermalConductivity")]
        internal static class Patch_ConduitFlowVisualizer_AddThermalConductivity
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Prefix(ConduitFlowVisualizer __instance, int cell, ref float conductivity, ConduitFlow ___flowManager)
            {
                Pressurized pressure = Integration.GetPressurizedAt(cell, (ConduitType)conduitType.GetValue(___flowManager));
                if (!Pressurized.IsDefault(pressure))
                    conductivity = 1f;
            }
        }
        //if our pipe does not have a thermal conductivity of 1f, this method would originally attempt to remove it even though it does not exist because of the above patch
        [HarmonyPatch(typeof(ConduitFlowVisualizer), "RemoveThermalConductivity")]
        internal static class Patch_ConduitFlowVisualizer_RemoveThermalConductivity
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Prefix(ConduitFlowVisualizer __instance, int cell, ref float conductivity, ConduitFlow ___flowManager)
            {
                Pressurized pressure = Integration.GetPressurizedAt(cell, (ConduitType)conduitType.GetValue(___flowManager));
                if (!Pressurized.IsDefault(pressure))
                    conductivity = 1f;
            }
        }

        //specifically changes the color of the flowing contents that appear in conduits
        [HarmonyPatch(typeof(ConduitFlowVisualizer), "GetCellTintColour")]
        internal static class Patch_ConduitFlowVisualizer_GetCellTintColour
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Postfix(ConduitFlowVisualizer __instance, int cell, ConduitFlow ___flowManager, bool ___showContents, ref Color32 __result)
            {
                Pressurized pressure = Integration.GetPressurizedAt(cell, (ConduitType)conduitType.GetValue(___flowManager));
                if (!Pressurized.IsDefault(pressure))
                    __result = ___showContents ? pressure.Info.FlowOverlayTint : pressure.Info.FlowTint;
            }
        }


        [HarmonyPatch(typeof(ConduitBridge), "ConduitUpdate")]
        internal static class Patch_ConduitBridge_ConduitUpdate
        {
            private static FieldInfo bridgeOutputCell;
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo flowManagerGetContents = AccessTools.Method(typeof(ConduitFlow), "GetContents");
                MethodInfo setMaxFlowPatch = AccessTools.Method(typeof(Patch_ConduitBridge_ConduitUpdate), nameof(SetMaxFlow));
                bridgeOutputCell = AccessTools.Field(typeof(ConduitBridge), "outputCell");
                foreach (CodeInstruction original in instructions)
                {
                    if (original.opcode == OpCodes.Callvirt && original.operand == flowManagerGetContents)
                    {
                        yield return original;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Call, setMaxFlowPatch);
                    }
                    else
                        yield return original;
                }
            }

            private static ConduitFlow.ConduitContents SetMaxFlow(ConduitFlow.ConduitContents contents, ConduitBridge bridge, ConduitFlow manager)
            {
                if (bridge.GetComponent<BuildingHP>().HitPoints == 0)
                {
                    //does not actually remove mass from the conduit, just changes what the bridge sees
                    contents.RemoveMass(contents.mass);
                    return contents;
                }
                int outputCell = (int)bridgeOutputCell.GetValue(bridge);
                GameObject outputObject = Grid.Objects[outputCell, Integration.layers[(int)bridge.type]];
                if (outputObject == null)
                    return contents;
                Pressurized pressure = bridge.GetComponent<Pressurized>();
                float capacity;
                if (!Pressurized.IsDefault(pressure))
                    capacity = pressure.Info.Capacity;
                else
                    capacity = (float)maxMass.GetValue(manager);

                //If the ConduitBridge is not supposed to support the amount of fluid currently in the contents, only make the bridge's intended max visible
                //Also immediately deal damage if the current contents are higher than the intended max.
                if (capacity < contents.mass)
                {
                    float initial = contents.mass;
                    float removed = contents.RemoveMass(initial - capacity);
                    float ratio = removed / initial;
                    contents.diseaseCount = (int)((float)contents.diseaseCount * ratio);
                    BuildingHP.DamageSourceInfo damage = Integration.GetPressureDamage();
                    bridge.Trigger((int)GameHashes.DoBuildingDamage, damage);
                }
                {
                    float targetCapacity;
                    Pressurized outPressure = outputObject.GetComponent<Pressurized>();
                    if (!Pressurized.IsDefault(outPressure))
                        targetCapacity = outPressure.Info.Capacity;
                    else
                        targetCapacity = (float)maxMass.GetValue(manager);

                    if(contents.mass > targetCapacity * 2)
                    {
                        BuildingHP.DamageSourceInfo damage = Integration.GetPressureDamage();
                        outputObject.Trigger((int)GameHashes.DoBuildingDamage, damage);
                    }
                }
                return contents;
            }
        }
    }
}
