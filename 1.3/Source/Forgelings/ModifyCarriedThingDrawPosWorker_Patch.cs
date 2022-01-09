using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(JobDriver_Ingest), "ModifyCarriedThingDrawPosWorker")]
    public static class ModifyCarriedThingDrawPosWorker_Patch
    {
        public static bool Prefix(JobDriver_Ingest __instance, ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
        {
            if (pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                return false;
            }
            return true;
        }
    }
}
