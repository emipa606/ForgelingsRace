using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(StatPart_WorkTableTemperature), "Applies", typeof(ThingDef), typeof(Map), typeof(IntVec3))]
public static class StatPart_WorkTableTemperature_Patch_Applies
{
    private static void Postfix(ref bool __result, ThingDef tDef)
    {
        if (tDef == FDefOf.Forge_ForgelingSpot)
        {
            __result = false;
        }
    }
}