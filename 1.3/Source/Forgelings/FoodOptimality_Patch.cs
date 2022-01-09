using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
    public static class FoodOptimality_Patch
    {
        public static bool Prefix(ref float __result, out Dictionary<ThingDef, OverridenValues> __state, Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
        {
            if (eater?.def == FDefOf.Forge_Forgeling_Race)
            {
                if (!Utils.FoodEdibleForgeling.ContainsKey(foodSource.def))
                {
                    __result = -9999999f;
                    __state = null;
                    return false;
                }
                else
                {
                    __state = Utils.AlterStats();
                }
            }
            else
            {
                __state = null;
            }
            if (foodSource.def.ingestible is null)
            {
                __result = -9999999f;
                return false;
            }
            return true;
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
