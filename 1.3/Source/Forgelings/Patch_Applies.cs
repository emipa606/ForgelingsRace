using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(StatPart_WorkTableOutdoors), "Applies", new Type[] { typeof(ThingDef), typeof(Map), typeof(IntVec3) })]
    public static class Patch_Applies
    {
        private static void Postfix(ref bool __result, ThingDef def, Map map, IntVec3 c)
        {
            if (def == FDefOf.Forge_ForgelingSpot)
            {
                __result = false;
            }
        }
    }
}
