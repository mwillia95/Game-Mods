﻿using System.Collections.Generic;
using UnityEngine;
namespace PressurizedPipes
{
    public class PressurizedInfo
    {   
        public float Capacity;
        public float IncreaseMultiplier;
        public bool IsDefault;
        public Color32 KAnimTint;
        public Color32 OverlayTint;
        public Color32 FlowTint;
        public Color32 FlowOverlayTint;
    }

    public static class PressurizedTuning
    {
        public static PressurizedInfo GetPressurizedInfo(string id)
        {
            if (PressurizedLookup.ContainsKey(id))
                return PressurizedLookup[id];
            else
                return PressurizedLookup[""];
        }

        public static bool TryAddPressurizedInfo(string id, PressurizedInfo info)
        {
            if (PressurizedLookup.ContainsKey(id))
            {
                Debug.LogWarning($"[Pressurized] PressurizedTuning.TryAddPressurizedInfo(string, PressurizedInfo) -> Attempted to add an id that already exists.");
                return false;
            }
            else if (info == null || info.IsDefault == true || info.Capacity <= 0f)
            {
                Debug.LogWarning($"[Pressurized] PressurizedTuning.TryAddPressurizedInfo(string, PressurizedInfo) -> PressurizedInfo argument was invalid. Must not be null, have a Capacity > 0, and IsDefault must be false.");
                return false;
            }
            PressurizedLookup.Add(id, info);
            return true;
        }

        private static Dictionary<string, PressurizedInfo> PressurizedLookup = new Dictionary<string, PressurizedInfo>()
        {
            {
                PressurizedGasConduitConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 3f,
                    IncreaseMultiplier = 3f,
                    KAnimTint = new Color32(255, 120, 200, 255),
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(167, 56, 40, 255),
                    FlowOverlayTint = new Color32(201, 160, 160, 0),
                    IsDefault = false
                }
            },
            {

                PressurizedLiquidConduitConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 30f,
                    IncreaseMultiplier = 3f,
                    KAnimTint = new Color32(255, 120, 255, 255),
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(255, 70, 40, 255),
                    FlowOverlayTint = new Color32(201, 160, 160, 0),
                    IsDefault = false
                }
            },
            {
                PressurizedGasConduitBridgeConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 3f,
                    IncreaseMultiplier = 3f,
                    KAnimTint = new Color32(230, 120, 80, 255),
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(255, 255, 255, 255),
                    FlowOverlayTint = new Color32(0, 0, 0, 0),
                    IsDefault = false
                }
            },
            {
                PressurizedLiquidConduitBridgeConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 30f,
                    IncreaseMultiplier = 3f,
                    KAnimTint = new Color32(230, 120, 80, 255),
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(255, 255, 255, 255),
                    FlowOverlayTint = new Color32(0, 0, 0, 0),
                    IsDefault = false
                }
            },
            {
                "",
                new PressurizedInfo()
                {
                    Capacity = -1f,
                    IncreaseMultiplier = 1f,
                    KAnimTint = new Color32(255, 255, 255, 255),
                    FlowTint = new Color32(255, 255, 255, 255),
                    IsDefault = true
                }
            }
        };


    }
}
