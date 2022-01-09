using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(StatPart_WorkTableTemperature), "Applies", new Type[] { typeof(ThingDef), typeof(Map), typeof(IntVec3) })]
    public static class StatPart_WorkTableTemperature_Patch_Applies
    {
        private static void Postfix(ref bool __result, ThingDef tDef, Map map, IntVec3 c)
        {
            if (tDef == FDefOf.Forge_ForgelingSpot)
            {
                __result = false;
            }
        }
    }
}
