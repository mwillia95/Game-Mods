using Harmony;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using System.Reflection;
using System;
using System.Threading;
using UnityEngine;

namespace DuplicantLifeExpectancy
{
    public class HarmonyPatches
    {
        [HarmonyPatch(typeof(MinionConfig))]
        [HarmonyPatch("CreatePrefab")]
        public static class LifeStage_MinionConfig_CreatePrefab_HarmonyPatch
        {
            public static void Postfix(ref GameObject __result)
            {
                __result.AddOrGet<LifeStages>();

            }
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Update")]
        public static class LifeStage_Game_Update_HarmonyPatch
        {
            public static void Prefix(Game __instance)
            {
                if (!__instance.IsLoading() && !LifeStages.postLoadChecked)
                {
                    Debugger.AddMessage("Inside Game.Update() post load");
                    List<MinionIdentity> minions = Components.LiveMinionIdentities.Items;
                    if (minions == null || minions.Count == 0)
                    {
                        Debugger.AddMessage("Post load minion identities still empty!");
                        return;
                    }
                    foreach (MinionIdentity minion in minions)
                    {
                        LifeStages stages = minion.GetComponent<LifeStages>();
                        stages.PostLoadCheck();
                    }
                    LifeStages.postLoadChecked = true;
                }
            }
        }


        [HarmonyPatch(typeof(Effects))]
        [HarmonyPatch("OnSpawn")]
        public static class Add_Effects_Before_Loading
        {
            public static void Prefix()
            {
                ResourceSet<Effect> effects = Db.Get().effects;
                foreach (Effect e in LifeStagePreset.GetGenericEffects())
                {
                    effects.Add(e);
                }
            }
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Load")]
        public static class ResetMinionsLoadedPatch
        {
            public static void Prefix()
            {
                TUNING.SKILLS.TARGET_SKILLS_CYCLE = 200;    
                LifeStages.postLoadChecked = false;
            }
        }


    }
}
