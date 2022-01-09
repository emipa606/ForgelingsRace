using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(FoodUtility), "TryFindBestFoodSourceFor")]
    public static class TryFindBestFoodSourceFor_Patch
    {
        public static void Prefix(Pawn getter, Pawn eater, out Dictionary<ThingDef, OverridenValues> __state)
        {
            if (eater?.def == FDefOf.Forge_Forgeling_Race)
            {
                __state = Utils.AlterStats();

            }
            else
            {
                __state = null;
            }
        }
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
        }
    }
}
