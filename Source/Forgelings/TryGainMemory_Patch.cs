using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", typeof(Thought_Memory), typeof(Pawn))]
public static class TryGainMemory_Patch
{
    private static bool Prefix(MemoryThoughtHandler __instance, ref Thought_Memory newThought)
    {
        if (__instance.pawn.def != FDefOf.Forge_Forgeling_Race)
        {
            return true;
        }

        return !newThought.def.defName.Contains("Ate") || newThought.def == ThoughtDefOf.AteLavishMeal;
    }
}