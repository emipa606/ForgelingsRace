using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(StatPart_WorkTableOutdoors), "Applies", typeof(ThingDef), typeof(Map), typeof(IntVec3))]
public static class Patch_Applies
{
    private static void Postfix(ref bool __result, ThingDef def)
    {
        if (def == FDefOf.Forge_ForgelingSpot)
        {
            __result = false;
        }
    }
}