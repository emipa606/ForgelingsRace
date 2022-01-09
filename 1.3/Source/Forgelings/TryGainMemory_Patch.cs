using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new Type[]
    {
        typeof(Thought_Memory),
        typeof(Pawn)
    })]
    public static class TryGainMemory_Patch
    {
        private static bool Prefix(MemoryThoughtHandler __instance, ref Thought_Memory newThought, Pawn otherPawn)
        {
            if (__instance.pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                if (newThought.def.defName.Contains("Ate") && newThought.def != ThoughtDefOf.AteLavishMeal)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
