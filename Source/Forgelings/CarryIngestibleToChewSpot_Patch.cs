using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Forgelings;

[HarmonyPatch(typeof(Toils_Ingest), "CarryIngestibleToChewSpot")]
public static class CarryIngestibleToChewSpot_Patch
{
    public static bool Prefix(ref Toil __result, Pawn pawn, TargetIndex ingestibleInd)
    {
        if (pawn.def != FDefOf.Forge_Forgeling_Race)
        {
            return true;
        }

        __result = CarryIngestibleToChewSpot(pawn, ingestibleInd);
        return false;
    }

    public static Toil CarryIngestibleToChewSpot(Pawn pawn, TargetIndex ingestibleInd)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var cell2 = IntVec3.Invalid;
            _ = actor.CurJob.GetTarget(ingestibleInd).Thing;

            bool BaseChairValidator(Thing t)
            {
                if (t.def.building is not { isSittable: true })
                {
                    return false;
                }

                if (!TryFindFreeSittingSpotOnThing(t, out _))
                {
                    return false;
                }

                if (t.IsForbidden(pawn))
                {
                    return false;
                }

                if (!actor.CanReserve(t))
                {
                    return false;
                }

                if (!t.IsSociallyProper(actor))
                {
                    return false;
                }

                if (t.IsBurning())
                {
                    return false;
                }

                if (t.HostileTo(pawn))
                {
                    return false;
                }

                var foundEdifice = false;
                for (var i = 0; i < 4; i++)
                {
                    var edifice = (t.Position + GenAdj.CardinalDirections[i]).GetEdifice(t.Map);
                    if (edifice == null || edifice.def.surfaceType != SurfaceType.Eat)
                    {
                        continue;
                    }

                    foundEdifice = true;
                    break;
                }

                return foundEdifice;
            }

            var thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell,
                TraverseParms.For(actor), 32f,
                t => BaseChairValidator(t) && t.Position.GetDangerFor(pawn, t.Map) == Danger.None);

            if (thing == null)
            {
                cell2 = RCellFinder.SpotToChewStandingNear(actor, actor.CurJob.GetTarget(ingestibleInd).Thing,
                    c => actor.CanReserveSittableOrSpot(c));
                var chewSpotDanger = cell2.GetDangerFor(pawn, actor.Map);
                if (chewSpotDanger != Danger.None)
                {
                    thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map,
                        ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                        PathEndMode.OnCell, TraverseParms.For(actor), 32f,
                        t => BaseChairValidator(t) && (int)t.Position.GetDangerFor(pawn, t.Map) <= (int)chewSpotDanger);
                }
            }

            if (thing != null && !TryFindFreeSittingSpotOnThing(thing, out cell2))
            {
                Log.Error(
                    "Could not find sitting spot on chewing chair! This is not supposed to happen - we looked for a free spot in a previous check!");
            }

            actor.ReserveSittableOrSpot(cell2, actor.CurJob);
            actor.Map.pawnDestinationReservationManager.Reserve(actor, actor.CurJob, cell2);
            actor.pather.StartPath(cell2, PathEndMode.OnCell);

            bool TryFindFreeSittingSpotOnThing(Thing t, out IntVec3 cell)
            {
                foreach (var item in t.OccupiedRect())
                {
                    if (!actor.CanReserveSittableOrSpot(item))
                    {
                        continue;
                    }

                    cell = item;
                    return true;
                }

                cell = default;
                return false;
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        return toil;
    }
}