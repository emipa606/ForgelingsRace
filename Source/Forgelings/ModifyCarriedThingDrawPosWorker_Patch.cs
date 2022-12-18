using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(JobDriver_Ingest), "ModifyCarriedThingDrawPosWorker")]
public static class ModifyCarriedThingDrawPosWorker_Patch
{
    public static bool Prefix(Pawn pawn)
    {
        return pawn.def != FDefOf.Forge_Forgeling_Race;
    }
}