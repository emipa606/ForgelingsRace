using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(ForbidUtility), "IsForbidden", typeof(Thing), typeof(Pawn))]
public static class Patch_IsForbidden
{
    private static void Postfix(ref bool __result, Thing t, Pawn pawn)
    {
        if (!__result && pawn != null && t?.def == FDefOf.Forge_ForgelingSpot &&
            pawn.def != FDefOf.Forge_Forgeling_Race)
        {
            __result = true;
        }
    }
}