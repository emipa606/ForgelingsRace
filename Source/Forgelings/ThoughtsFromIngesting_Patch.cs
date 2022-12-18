using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
public static class ThoughtsFromIngesting_Patch
{
    public static void Prefix(Pawn ingester, out Dictionary<ThingDef, OverridenValues> __state)
    {
        __state = ingester?.def == FDefOf.Forge_Forgeling_Race ? Utils.AlterStats() : null;
    }

    public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
    {
        if (__state != null)
        {
            Utils.RestoreStats(__state);
        }
    }
}