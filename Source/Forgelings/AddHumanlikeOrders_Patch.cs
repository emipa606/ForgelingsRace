using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
public static class AddHumanlikeOrders_Patch
{
    public static void Prefix(Pawn pawn, out Dictionary<ThingDef, OverridenValues> __state)
    {
        __state = pawn?.def == FDefOf.Forge_Forgeling_Race ? Utils.AlterStats() : null;
    }

    public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
    {
        if (__state != null)
        {
            Utils.RestoreStats(__state);
        }
    }
}