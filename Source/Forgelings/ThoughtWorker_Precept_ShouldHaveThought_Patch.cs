using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(ThoughtWorker_Precept_NoRecentHumanMeat), "ShouldHaveThought")]
public static class ThoughtWorker_Precept_ShouldHaveThought_Patch
{
    public static void Postfix(Pawn p, ref ThoughtState __result)
    {
        if (p.def == FDefOf.Forge_Forgeling_Race)
        {
            __result = false;
        }
    }
}