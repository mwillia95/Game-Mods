using System;
using System.Collections.Generic;
using Harmony;
using Klei.AI;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;


namespace ExposureNotification
{

    public class ShowLocationObject
    {
        public Vector3 Pos;
        public MinionIdentity Minion;
        public bool ShowLocation = false;
        public ShowLocationObject(MinionIdentity minion)
        {
            Pos = minion.transform.GetPosition();
            Minion = minion;
        }
    }

    public class ExposureNotificationHarmonyPatches
    {
        [HarmonyPatch(typeof(GermExposureMonitor.Instance))]
        [HarmonyPatch("SetExposureState")]
        public static class SetExposureStateHarmonyPatch
        {
            //When an exposure is set to exposed, it is the exact moment the Duplicant has become exposed
            //Take the germ id, find the sickness tied to the germ, and get that sicknesses name
            //Create a popfx (appears next to the Dupe) and add a notification (appears in the top left list)

            //It would also be possible to do this as a transpiler in the InjectDisease method
            //The method could easily bypass having to check exposure state and getting the sickness, but would be prone to breaking
            public static void Postfix(string germ_id, GermExposureMonitor.ExposureState exposure_state, GermExposureMonitor.Instance __instance)
            {
                if (exposure_state == GermExposureMonitor.ExposureState.Exposed)
                {
                    GermExposureMonitor.ExposureType exposure_type = null;
                    foreach (GermExposureMonitor.ExposureType type in GermExposureMonitor.exposureTypes)
                    {
                        if (germ_id == type.germ_id)
                        {
                            exposure_type = type;
                            break;
                        }
                    }
                    Sickness sickness = exposure_type?.sickness_id != null ? Db.Get().Sicknesses.Get(exposure_type.sickness_id) : null;
                    string sicknessName = sickness != null ? sickness.Name : "a disease";
                    string text = string.Format(Helper.DUPE_EXPOSED_TO_GERMS_POPFX, sicknessName);
                    PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, text, __instance.gameObject.transform, 3f, true);
                    Helper.CreateAndAddNotification(__instance, sicknessName);
                }
            }
        }

        //Need to find a better way to do this
        //When Update is first called, even postfix, the minions are not loaded into the list
        //Would be better to find a method that is called immediately after all the Duplicants have been loaded
        //or the method that puts all the minions into the list
     
        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Update")]
        public static class RunOnceMinionsLoadedPatch
        {
            public static void Postfix()
            {
                if (Helper.MinionsLoaded)
                    return;
                List<MinionIdentity> minions = Components.LiveMinionIdentities?.Items;

                if (minions == null || minions.Count == 0)
                {
                    return;
                }
                try
                {
                    ConfigSettings settings = ConfigReader<ConfigSettings>.GetConfig();
                    Helper.showLocation = settings == null ? false : settings.ShowLocation;
                }
                catch (Exception)
                {
                    Helper.showLocation = false;
                }
                try
                {
                    foreach (MinionIdentity minion in minions)
                    {
                        GermExposureMonitor.Instance monitor = minion.gameObject.GetSMI<GermExposureMonitor.Instance>();
                        foreach (GermExposureMonitor.ExposureType exposure in GermExposureMonitor.exposureTypes)
                        {
                            GermExposureMonitor.ExposureState state = monitor.GetExposureState(exposure.germ_id);
                            if (state == GermExposureMonitor.ExposureState.Contracted || state == GermExposureMonitor.ExposureState.Exposed)
                            {
                                Sickness sickness = Db.Get().Sicknesses.Get(exposure.sickness_id);
                                string sicknessName = sickness != null ? sickness.Name : "a disease";
                                Helper.CreateAndAddNotification(monitor, sicknessName);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Do nothing. Can't be sure why this error occurred, but hopefully will only happen this time.
                }
                Helper.MinionsLoaded = true;
            }
        }



        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Load")]
        public static class ResetMinionsLoadedPatch
        {
            public static void Prefix()
            {
                Helper.MinionsLoaded = false;
            }
        }

    }

}
