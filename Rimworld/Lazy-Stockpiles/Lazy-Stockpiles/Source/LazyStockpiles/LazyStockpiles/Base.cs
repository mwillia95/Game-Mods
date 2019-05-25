using System.Collections.Generic;
using HugsLib;
using RimWorld;
using Verse;

namespace LazyStockpiles
{
    //Initializes HugsLib to allow for HarmonyPatches to execute automatically
    public class Base : ModBase
    {
        public override string ModIdentifier
        {
            get
            {
                return "LazyStockpileSettings";
            }
        }

        public Base()
        {
        }

    }
}
