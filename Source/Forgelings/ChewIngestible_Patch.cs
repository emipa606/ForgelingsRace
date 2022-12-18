using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Forgelings;

[HarmonyPatch(typeof(Toils_Ingest), "ChewIngestible")]
public static class ChewIngestible_Patch
{
    public static bool Prefix(ref Toil __result, Pawn chewer, float durationMultiplier, TargetIndex ingestibleInd,
        TargetIndex eatSurfaceInd = TargetIndex.None)
    {
        if (chewer.def != FDefOf.Forge_Forgeling_Race)
        {
            return true;
        }

        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var thing4 = actor.CurJob.GetTarget(ingestibleInd).Thing;
            toil.actor.pather.StopDead();
            actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt(500 * durationMultiplier);
            if (thing4.Spawned)
            {
                thing4.Map.physicalInteractionReservationManager.Reserve(chewer, actor.CurJob, thing4);
            }
        };
        toil.tickAction = delegate
        {
            if (chewer != toil.actor)
            {
                toil.actor.rotationTracker.FaceCell(chewer.Position);
            }
            else
            {
                var thing3 = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
                if (thing3 is { Spawned: true })
                {
                    toil.actor.rotationTracker.FaceCell(thing3.Position);
                }
                else if (eatSurfaceInd != 0 && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid)
                {
                    toil.actor.rotationTracker.FaceCell(toil.actor.CurJob.GetTarget(eatSurfaceInd).Cell);
                }
            }

            toil.actor.GainComfortFromCellIfPossible();
        };
        toil.WithProgressBar(ingestibleInd, delegate
        {
            var thing2 = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
            return thing2 == null
                ? 1f
                : 1f - (toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round(500 * durationMultiplier));
        });
        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.FailOnDestroyedOrNull(ingestibleInd);
        toil.AddFinishAction(delegate
        {
            if (chewer.CurJob == null)
            {
                return;
            }

            var thing = chewer.CurJob.GetTarget(ingestibleInd).Thing;
            if (thing != null && chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, thing))
            {
                chewer.Map.physicalInteractionReservationManager.Release(chewer, toil.actor.CurJob, thing);
            }
        });
        toil.handlingFacing = true;
        Toils_Ingest.AddIngestionEffects(toil, chewer, ingestibleInd, eatSurfaceInd);
        __result = toil;
        return false;
    }
}