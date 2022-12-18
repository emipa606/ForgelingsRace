using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
public static class FoodOptimality_Patch
{
    public static bool Prefix(ref float __result, out Dictionary<ThingDef, OverridenValues> __state, Pawn eater,
        Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
    {
        if (eater?.def == FDefOf.Forge_Forgeling_Race)
        {
            if (!Utils.FoodEdibleForgeling.ContainsKey(foodSource.def))
            {
                __result = -9999999f;
                __state = null;
                return false;
            }

            __state = Utils.AlterStats();
        }
        else
        {
            __state = null;
        }

        if (foodSource.def.ingestible is not null)
        {
            return true;
        }

        __result = -9999999f;
        return false;
    }

    public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
    {
        if (__state != null)
        {
            Utils.RestoreStats(__state);
        }
    }
}