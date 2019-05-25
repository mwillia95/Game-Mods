using Harmony;
using Database;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace SkilledAndUnskilledTinkering
{
    public class SkilledOperateHarmonyPatches
    {
        public static ExtensionProperties properties = new ExtensionProperties();


        [HarmonyPatch(typeof(Database.SkillGroups), MethodType.Constructor, new Type[] { typeof(ResourceSet) })]
        public static class SkillGrouptsConstructorPatch
        {
            public static void Postfix(SkillGroups __instance)
            {
                properties.SkilledTechnicals = __instance.Add(new SkillGroup("SkilledTechnicals", properties.SkilledMachineOperating.Id, "Skilled"));
                properties.SkilledTechnicals.relevantAttributes = new List<string> { Db.Get().Attributes.Machinery.Id };
                properties.SkilledTechnicals.requiredChoreGroups = new List<string> { properties.SkilledMachineOperating.Id };

            }
        }

        [HarmonyPatch(typeof(ChoreGroups), MethodType.Constructor, new Type[] { typeof(ResourceSet) })]
        public static class ChoreGroupsConstructorPatch
        {
            public static void Postfix(ChoreGroups __instance)
            {
                ChoreGroup group = new ChoreGroup("Skilled", "Skilled", "Machinery", 3);
                properties.SkilledMachineOperating = __instance.Add(group);
            }
        }





    }
}
