using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(FoodUtility), "TryFindBestFoodSourceFor_NewTemp")]
public static class TryFindBestFoodSourceFor_Patch
{
    public static void Prefix(Pawn eater, out Dictionary<ThingDef, OverridenValues> __state)
    {
        __state = eater?.def == FDefOf.Forge_Forgeling_Race ? Utils.AlterStats() : null;
    }

    public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
    {
        if (__state != null)
        {
            Utils.RestoreStats(__state);
        }
    }
}