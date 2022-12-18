using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(Thing), "Ingested")]
public static class Ingested_Patch
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