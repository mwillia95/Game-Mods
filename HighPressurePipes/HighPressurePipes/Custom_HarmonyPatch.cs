using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using System.Reflection;
using UnityEngine;
namespace HighPressurePipes
{
    public static class CustomPatcher
    {
        private static MethodInfo Original_Target = AccessTools.Method(RenderMeshTask, "Run", new Type[] { RenderMeshContext });
        private static MethodInfo Patch_Transpiler = typeof(CustomPatcher).GetMethod(nameof(Run_Transpiler));
        private static FieldInfo Context_Outer = AccessTools.Field(RenderMeshContext, "outer");
        private static readonly Type RenderMeshContext = AccessTools.TypeByName("ConduitFlowVisualizer+RenderMeshContext");
        private static Type RenderMeshTask = AccessTools.TypeByName("ConduitFlowVisualizer+RenderMeshTask");

        public static void OnLoad()
        {
            MethodInfo[] methodList = RenderMeshTask.GetMethods();
            foreach (MethodInfo method in methodList)
            {
                if (method.Name == "Run")
                {
                    Original_Target = method;
                }
            }

            Context_Outer = AccessTools.Field(RenderMeshContext, "outer");
            var harmony = HarmonyInstance.Create("Super_Corgi_PressurizedPipes_CustomHarmony");
            harmony.Patch(Original_Target, null, null, new HarmonyMethod(Patch_Transpiler));
        }
        //Purpose: To modify the size of the flowing contents to accomodate the potential increased capacity of some conduits
        //Normally, the flow caps off in scaling at 1KG/10KG (gas/liquid). Change the behaviour to consider if the contents are in a higher capacity conduit than is standard.
        //Addtionally, if flowing to differing capacities, progressively interlopate the scaling from the source conduit to the scaling of the receiving conduit
        public static IEnumerable<CodeInstruction> Run_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo target = typeof(ConduitFlowVisualizer).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == "CalculateMassScale");
            MethodBase massPatch = AccessTools.Method(typeof(CustomPatcher), nameof(ModifyApparentMass));
            MethodBase lerpPatch = AccessTools.Method(typeof(CustomPatcher), nameof(LerpMasses));
            FieldInfo flowManager = AccessTools.Field(typeof(ConduitFlowVisualizer), "flowManager");
            FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");
            FieldInfo conduitContents = AccessTools.Field(typeof(ConduitFlow.ConduitFlowInfo), "contents");
            MethodInfo conduitContentsMass = AccessTools.Property(typeof(ConduitFlow.ConduitContents), "mass").GetGetMethod();
            FieldInfo contextLerpPercent = AccessTools.Field(RenderMeshContext, "lerp_percent");
            if (target == default(MethodInfo))
            {
                Debug.LogWarning($"[Pressurized] Could not find ConduitFlowVisualizer.CalculateMassScale() MethodBase");
            }
            if (RenderMeshContext == null)
                Debug.LogWarning($"[Pressurized] Could not find Type for RenderMeshContext!!");
            if (Context_Outer == null)
            {
                Debug.LogWarning($"[Pressurized] Could not find FieldInfo for outer context!");
            }
            bool foundFirst = false;
            foreach (CodeInstruction original in instructions)
            {
                if (original.opcode == OpCodes.Callvirt && original.operand == target)
                {
                    if (!foundFirst)//moving
                    {
                        foundFirst = true;
                        //context.outer.flowManager
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 5); //cellFromDirection
                        yield return new CodeInstruction(OpCodes.Ldarg_1); //context.outer.flowManager.conduitType
                        yield return new CodeInstruction(OpCodes.Ldfld, Context_Outer);
                        yield return new CodeInstruction(OpCodes.Ldfld, flowManager);
                        yield return new CodeInstruction(OpCodes.Ldfld, conduitType);
                        yield return new CodeInstruction(OpCodes.Call, massPatch); //ModifyApparentMass()
                        yield return original;
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldfld, Context_Outer);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, 3);
                        yield return new CodeInstruction(OpCodes.Ldflda, conduitContents);
                        yield return new CodeInstruction(OpCodes.Call, conduitContentsMass);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6); //cellFromDirection
                        yield return new CodeInstruction(OpCodes.Ldarg_1); //context.outer.flowManager.conduitType
                        yield return new CodeInstruction(OpCodes.Ldfld, Context_Outer);
                        yield return new CodeInstruction(OpCodes.Ldfld, flowManager);
                        yield return new CodeInstruction(OpCodes.Ldfld, conduitType);
                        yield return new CodeInstruction(OpCodes.Call, massPatch); //ModifyApparentMass()
                        yield return original;
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldfld, contextLerpPercent);
                        yield return new CodeInstruction(OpCodes.Call, lerpPatch);
                    }
                    else//static
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 5); //cell
                        yield return new CodeInstruction(OpCodes.Ldarg_1); //context.outer.flowManager.conduitType
                        yield return new CodeInstruction(OpCodes.Ldfld, Context_Outer);
                        yield return new CodeInstruction(OpCodes.Ldfld, flowManager);
                        yield return new CodeInstruction(OpCodes.Ldfld, conduitType);
                        yield return new CodeInstruction(OpCodes.Call, massPatch); //ModifyApparentMass()
                        yield return original;
                    }
                }
                else
                {
                    yield return original;
                }
            }
        }
        private static float ModifyApparentMass(float originalMass, int cell, ConduitType type)
        {
            try
            {
                PressurizedInfo info = Integration.GetPressurizedAt(cell, type)?.Info;
                if (info != null && !info.IsDefault)
                {
                    return originalMass / info.IncreaseMultiplier;
                }
                return originalMass;
            }
            catch(Exception e)
            {
                Debug.LogError($"[Pressurized] Error caught in ModifyApparentMass ->\n{e.Message}");
            }
            return originalMass;
        }

        private static float LerpMasses(float originScale, float receiverScale, float lerpPercent)
        {
            return Mathf.Lerp(originScale, receiverScale, lerpPercent);
        }

    }
}
