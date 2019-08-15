using System.Collections.Generic;
using STRINGS;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;

namespace HighPressurePipes
{
    public static class Integration
    {
        private static readonly FieldInfo maxMass = AccessTools.Field(typeof(ConduitFlow), "MaxMass");
        private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");
        private static readonly MethodBase patch = AccessTools.Method(typeof(Integration), nameof(IntegratePressurized));
        private static readonly MethodBase overpressurePatch = AccessTools.Method(typeof(Integration), nameof(IntegrateOverpressure));
        private static readonly MethodBase conduitContentsAddMass = AccessTools.Method(typeof(ConduitFlow.ConduitContents), "AddMass");

        internal static IEnumerable<CodeInstruction> AddIntegrationIfNeeded(CodeInstruction original, CodeInstruction toGetCell, bool isUpdateConduit = false)
        {
            if (original.opcode == OpCodes.Ldfld && original.operand == maxMass)
            {
                yield return original;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, conduitType);
                yield return toGetCell;
                yield return new CodeInstruction(OpCodes.Call, patch);

            }
            else if (isUpdateConduit && original.opcode == OpCodes.Call && original.operand == conduitContentsAddMass)
            {
                yield return original;
                yield return new CodeInstruction(OpCodes.Ldloc_2); //gridenode grid_node
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, maxMass); //this.MaxMass
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, conduitType); //this.conduitType
                yield return toGetCell; //int cell2
                yield return new CodeInstruction(OpCodes.Call, overpressurePatch);
            }
            else
                yield return original;
        }
        internal static int[] layers = { 0, 12, 16, 0 };
        internal static int[] connectionLayers = { 0, 15, 19, 0 };


        private static void IntegrateOverpressure(ConduitFlow.GridNode sender, float standardMax, ConduitType conduitType, int cell)
        {
            Pressurized pressure = GetPressurizedAt(cell, conduitType);
            float receiverMax = Pressurized.IsDefault(pressure) ? standardMax : pressure.Info.Capacity;

            float senderMass = sender.contents.mass;
            if (senderMass >= receiverMax * 2f)
            {
                GameObject receiver = pressure != null ? pressure.gameObject : Grid.Objects[cell, layers[(int)conduitType]];
                BuildingHP.DamageSourceInfo damage = GetPressureDamage();
                queueDamages.Add(new QueueDamage(damage, receiver));
            }
        }

        public static BuildingHP.DamageSourceInfo GetPressureDamage()
        {
            BuildingHP.DamageSourceInfo damage = new BuildingHP.DamageSourceInfo
            {
                damage = 1,
                source = BUILDINGS.DAMAGESOURCES.LIQUID_PRESSURE,
                popString = STRINGS.UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.LIQUID_PRESSURE
            };
            return damage;
        }
        internal class QueueDamage
        {
            public BuildingHP.DamageSourceInfo Damage;
            public GameObject Receiver;

            public QueueDamage(BuildingHP.DamageSourceInfo dmg, GameObject rcvr)
            {
                Damage = dmg;
                Receiver = rcvr;
            }
        }

        internal static List<QueueDamage> queueDamages = new List<QueueDamage>();

        private static float IntegratePressurized(float standardMax, ConduitType conduitType, int cell)
        {
            Pressurized pressure = GetPressurizedAt(cell, conduitType);
            if(!Pressurized.IsDefault(pressure))
                return pressure.Info.Capacity;
            return standardMax;
        }

        public static Pressurized GetPressurizedAt(int cell, int layer)
        {

            return Grid.Objects[cell, layer]?.GetComponent<Pressurized>();
        }
        public static Pressurized GetPressurizedAt(int cell, ConduitType type, bool isBridge = false)
        {
            if (type != ConduitType.Gas && type != ConduitType.Liquid)
                return null;
            int layer = isBridge ? connectionLayers[(int)type] : layers[(int)type];
            return Grid.Objects[cell, layer]?.GetComponent<Pressurized>();
        }
    }
}
