using System;
using System.Collections;
using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Linq;
namespace LazyStockpiles
{

    /// <summary>
    /// Various methods and classes that utilize Harmony library to inject code after specific methods run/before they start
    /// The goal of these methods is to implement the Cache/Buffer behavior to hauling jobs, which includes
    /// 
    /// Cache: A cache functions similar to a crafting job's "Until you have X" condition.
    /// If a stockpile is a cache, the entire stockpile has a single stack threshold, for example, 33%.
    /// If a cell within the stockpile has a stack of items, and that stack meets or exceeds that threshold,
    ///     that specific cell will no longer accept hauling jobs to it.
    /// A cache is useful as a high priority stockpile that supplies a nearby crafting station.
    /// 
    /// Buffer: A buffer is intended to store recently created products nearby until a full (or near full) stack can be hauled to another, more distant stockpile.
    /// If a stockpils is a buffer, the entire stockpile has a single stack threshold, for example, 75%.
    /// If a cell within the stockpile has a stack of items, and that stack meets or exceeds that threshold,
    ///     that specific cell will attempt to force the stack out of the stockpile entirely.
    ///     This includes hauling the stack to a lower priority stockpile.
    /// Additionally, a buffer will never accept items that are coming from another stockpile
    ///
    /// </summary>
    public static class ThingExtension
    {
        public static float Ratio(this Thing t)
        {
            if (t.def.IsApparel || t.def.IsWeapon)
            {
                return 1f;
            }
            return (float)t.stackCount / (float)t.def.stackLimit;
        }
    }

    [HarmonyPatch(typeof(StorageSettings), "ExposeData")]
    public static class StorageSettings_ExposeData_Patch
    {
        public static void Postfix(StorageSettings __instance)
        {
            LazySettings settings = LazySettingsManager.AddOrGetSettings(__instance);
            {
                Scribe_Values.Look(ref settings.type, "settings.type", LazyType.Normal);
                Scribe_Values.Look(ref settings.CacheThreshold, "settings.CacheThreshold", LazySettings.DefaultCache);
                Scribe_Values.Look(ref settings.BufferThreshold, "settings.BufferThreshold", LazySettings.DefaultBuffer);
            }
        }
    }

    //PURPOSE: When ITab_Storage is instantiated, a protected variable (size) is set equal to a private readonly variable (WinSize)
    //Bypass the code to set size to my own custom WinSize (found in LazySettingsManager
    [HarmonyPatch(typeof(ITab_Storage), MethodType.Constructor)]
    public static class ITabStorage_Constructor_Patch
    {
        public static void Postfix(ITab_Storage __instance)
        {
            //AccessTools.Field(typeof(ITab_Storage), "size").SetValue(__instance, LazySettingsManager.WinSize);
        }
    }

    [HarmonyPatch(typeof(InspectTabBase), "TabRect", MethodType.Getter)]
    public static class InspectTabBase_TabRectGetter_Patch
    {
        public static void Postfix(InspectTabBase __instance, ref Rect __result)
        {
            if(__instance as ITab_Storage != null)
            {
                //Log.Message("Patching ITab_Storage Rectangle");
                __result.y -= 95f;
                __result.height += 95f;
            }
        }
    }

    //[HarmonyPatch(typeof(ITab), "PaneTopY", MethodType.Getter)]
    //public static class ITab_PaneTopY_Patch
    //{
    //    public static void Postfix(ITab __instance, ref float __result)
    //    {
    //        if (__instance as ITab_Storage != null)
    //            __result -= 95f;
    //    }
    //}

    //PURPOSE: Modify the storage tab to include custom button and sliders for Lazy Settings
    //Code is injected to adjust the size of the overall window and to replace references with the ITab_Storage.WinSize with LazySettingsManager.WinSize
    //Code is injected to create the stockpile type button and slider at the correct moment of execution
    //Code is injected to modify the height and starting position of the Filter Window to account for the stockpile type button and, if present, the threshold slider
    [HarmonyPatch(typeof(ITab_Storage), "FillTab")]
    public static class ITabStorage_FillTab_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase method)
        {
            Log.Message("Starting FillTab transpiler patch");
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo tabPatch = AccessTools.Method(typeof(LazySettingsManager), nameof(LazySettingsManager.ITab_Storage_Patch));
            MethodInfo getStoreSettings = AccessTools.Method(typeof(IStoreSettingsParent), nameof(IStoreSettingsParent.GetStoreSettings));
            MethodInfo varTabHeight = AccessTools.Method(typeof(LazySettingsManager), nameof(LazySettingsManager.VariableTabHeight));
            FieldInfo winSizeField = AccessTools.Field(typeof(LazySettingsManager), "WinSize");
            MethodInfo selStorageGetter = AccessTools.Property(typeof(ITab_Storage), "SelStoreSettingsParent").GetGetMethod(true);
           

            Log.Message($"Number of instructions: {codes.Count}");

            bool foundFilterRect = false;
            bool foundFirstCall = false;
            bool foundSecondCall = false;
            bool foundFirstWinSize = false;
            for (int i = 0; i < codes.Count; i++)
            {
                //Find the beginning of when the variable rect2 is being declared. Inject code to modify the start position and window height of this rectangle
                if (!foundFilterRect)
                {
                    //Very prone to break if the source code changes at all
                    if (codes[i].opcode == OpCodes.Ldloca_S && (codes[i].operand as LocalBuilder)?.LocalIndex == 11)
                    {
                        Log.Message("[NOTICE] Found beginning of filter rectangle declaration");
                        foundFilterRect = true;
                    }
                }

                else if (!foundFirstCall)
                {
                    //Load StorageSettings onto the stack and call LazySettingsManager.VariableTabHeight()
                    //Add the methods result to TopAreaHeight
                    //Modifies the starting position of the Filter Window
                    if (codes[i].opcode == OpCodes.Call && codes[i - 1].opcode == OpCodes.Ldarg_0)
                    {
                        Log.Message("[NOTICE] Modifiying first reference of TopAreaHeight");
                        foundFirstCall = true;
                        //value = TopAreaHeight + LazySettingsManager.VariableTabHeight(this.SelStoreSettingsParent.GetStoreSettings());
                        yield return codes[i];
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Callvirt, selStorageGetter);
                        yield return new CodeInstruction(OpCodes.Callvirt, getStoreSettings);
                        yield return new CodeInstruction(OpCodes.Call, varTabHeight);
                        yield return new CodeInstruction(OpCodes.Add);
                        continue;
                    }
                }
                else if (!foundSecondCall)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i - 1].opcode == OpCodes.Ldarg_0)
                    {
                        //Load StorageSettings onto the stack and call LazySettingsManager.VariableTabHeight()
                        //Add the methods result to TopAreaHeight, plus an additional 10
                        //Modifies the height of the Filter Window
                        Log.Message("[NOTICE] Modifiying second reference of TopAreaHeight");
                        foundSecondCall = true;
                        //value = this.TopAreaHeight + LazySettingsManager.VariableTabHeight(this.SelStoreSettingsParent.GetStoreSettings()) + 10f;
                        yield return codes[i];
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Callvirt, selStorageGetter);
                        yield return new CodeInstruction(OpCodes.Callvirt, getStoreSettings);
                        yield return new CodeInstruction(OpCodes.Call, varTabHeight);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 10f);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Add);
                        continue;
                    }
                }


                //Inject code to replace references of ITab_Storage.WinSize with a reference to LazySettingsManager.WinSize
                if (codes[i].opcode == OpCodes.Ldsfld)
                {
                    if (codes[i].operand.ToString().Contains("Vector2"))
                    {

                        Log.Message("[WINSIZE] Patching WinSize for ITab_Storage");
                        //replace
                        //ITab_Storage.WinSize
                        //with
                        //LazySettingsManager.WinSize
                        yield return new CodeInstruction(OpCodes.Ldsfld, winSizeField);
                        continue;

                    }
                }

                //Inject code to call LazySettingsManager.ITab_Storage_Patch just before the code `ThingFilter parentFilter = null;`
                //Load the instance of the class and StorageSettings onto the stack to be passed to the method
                if (codes[i] != null && codes[i].opcode == OpCodes.Ldnull)
                {
                    //Log.Message("Found Ldnull opcode");
                    if (codes[i + 1].opcode == OpCodes.Stloc_S)
                    {
                        Log.Message($"Found stloc.s opcode after ldnull\nOperand has type: {codes[i + 1].operand.GetType()}");
                        //LazySettingsManager.ITab_Storage_Patch(this, this.SelStoreSettingsParent.GetStorageSettings());
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Callvirt, selStorageGetter);
                        yield return new CodeInstruction(OpCodes.Callvirt, getStoreSettings);

                        yield return new CodeInstruction(OpCodes.Call, tabPatch);
                    }

                }
                yield return codes[i];
            }
        }
    }


    [HarmonyPatch(typeof(StoreUtility), "NoStorageBlockersIn")]
    public static class NoStorageBlockersIn_LazyPatch
    {
        //Consider a cache cell to be blocked if the stack size already in the cell is greater than 33% (or threshold)
        //Consider a buffer cell to be blocked if thing is already stored within the same stockpile
        [HarmonyPostfix]
        private static void NoStorageBlockersIn_Lazy_PostFix(IntVec3 c, Map map, Thing thing, ref bool __result)
        {
            Zone_Stockpile generic = map.zoneManager.ZoneAt(c) as Zone_Stockpile;
            if (__result && generic != null)
            {
                LazySettings stockPile = LazySettingsManager.AddOrGetSettings(generic.settings);
                if (stockPile == null)
                {
                    //Log.Error("The stockpile settings are not Lazy. Will not utilize lazy settings at all.");
                    return;
                }
                if (stockPile.type == LazyType.Cache)
                {
                    List<Thing> list = map.thingGrid.ThingsListAt(c);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Thing potentialBlocker = list[i];
                        if (potentialBlocker.def.EverStorable(false) && potentialBlocker.Ratio() > stockPile.CacheThreshold)
                        {
                            __result = false;
                            return;
                        }
                    }
                }
                else if (stockPile.type == LazyType.Buffer)
                {
                    //if the thing we are trying to place is trying to go into the same stockpile that it is already in, and the stockpile is a lazy buffer, do not let it place itself back in
                    //this should only be reached because the buffer is trying to get rid of the stack (greater than the threshold)
                    LazySettings thingStock = LazySettingsManager.AddOrGetSettings(thing.GetSlotGroup()?.Settings);
                    __result = stockPile != thingStock;
                }
            }
        }
    }


    //Consider a thing to be not be in a valid (and best) storage if its ratio >= 75% and it is stored in a lazy buffer
    //To force haul checks on that item
    [HarmonyPatch(typeof(StoreUtility), "IsInValidBestStorage")]
    public static class IsInValidBestStorage_BufferPatch
    {
        [HarmonyPostfix]
        public static void IsInValidBestStorage(Thing t, ref bool __result)
        {

            if (__result) //to prevent uneccessary work
            {
                StorageSettings test = t.GetSlotGroup()?.Settings;
                if (test != null)
                {
                    LazySettings settings = LazySettingsManager.AddOrGetSettings(test);
                    if (settings != null)
                    {
                        __result = !(settings.type == LazyType.Buffer && t.Ratio() >= settings.BufferThreshold);
                    }
                }

            }
        }
    }

    //modify whether a storage is allowed to accept an item
    //If the storage target is a buffer
    //Do not allow the item if it is coming from a different storage
    [HarmonyPatch(typeof(StorageSettings), "AllowedToAccept", new Type[] { typeof(Thing) })]
    public static class AllowedToAccept_BufferPatch
    {
        [HarmonyPostfix]
        public static void AllowedToAccept_Postfix(ref bool __result, ref StorageSettings __instance, Thing t)
        {
            if (__result)
            {
                LazySettings target = LazySettingsManager.AddOrGetSettings(__instance);

                if (target?.type == LazyType.Buffer)
                {

                    SlotGroup sourceGroup = t.GetSlotGroup();
                    if (sourceGroup == null)
                        return;
                    //if the item is targetting its own buffer and ratio is too high, IsInValidBestStorage harmony patch will take care of it
                    __result = sourceGroup.Settings == __instance;
                }
            }

        }
    }

    [HarmonyPatch(typeof(StoreUtility), "TryFindBestBetterStoreCellFor")]
    public static class TryFindBestBetterStoreCellFor_BufferPatch
    {
        //If a thing is stored within a lazy buffer, and the ratio of the thing is >= 75%, consider it to not have been stored at all(Unstored)
        [HarmonyPrefix]
        public static void TryFindBestBetterStoreCellFor_Prefix(Thing t, ref StoragePriority currentPriority)
        {
            LazySettings l = LazySettingsManager.AddOrGetSettings(t.GetSlotGroup()?.Settings);
            if (l != null)
            {
                if (l.type == LazyType.Buffer)
                {
                    if (t.Ratio() >= l.BufferThreshold)
                        currentPriority = StoragePriority.Unstored; //Have the method consider the thing to not be stored at all
                }
            }
        }
    }
}



