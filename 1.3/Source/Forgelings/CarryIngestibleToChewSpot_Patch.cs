using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Forgelings
{
    [HarmonyPatch(typeof(Toils_Ingest), "CarryIngestibleToChewSpot")]
    public static class CarryIngestibleToChewSpot_Patch
    {
        public static bool Prefix(ref Toil __result, Pawn pawn, TargetIndex ingestibleInd)
        {
            if (pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                __result = CarryIngestibleToChewSpot(pawn, ingestibleInd);
                return false;
            }
            return true;
        }
        public static Toil CarryIngestibleToChewSpot(Pawn pawn, TargetIndex ingestibleInd)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                IntVec3 cell2 = IntVec3.Invalid;
                Thing thing = null;
                Thing thing2 = actor.CurJob.GetTarget(ingestibleInd).Thing;
                Predicate<Thing> baseChairValidator = delegate (Thing t)
                {
                    if (t.def.building == null || !t.def.building.isSittable)
                    {
                        return false;
                    }
                    if (!TryFindFreeSittingSpotOnThing(t, out var _))
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
                    bool flag = false;
                    for (int i = 0; i < 4; i++)
                    {
                        Building edifice = (t.Position + GenAdj.CardinalDirections[i]).GetEdifice(t.Map);
                        if (edifice != null && edifice.def.surfaceType == SurfaceType.Eat)
                        {
                            flag = true;
                            break;
                        }
                    }
                    return flag ? true : false;
                };

                thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell,
                    TraverseParms.For(actor), 32f, (Thing t) => baseChairValidator(t) && t.Position.GetDangerFor(pawn, t.Map) == Danger.None);

                if (thing == null)
                {
                    cell2 = RCellFinder.SpotToChewStandingNear(actor, actor.CurJob.GetTarget(ingestibleInd).Thing, (IntVec3 c) => actor.CanReserveSittableOrSpot(c));
                    Danger chewSpotDanger = cell2.GetDangerFor(pawn, actor.Map);
                    if (chewSpotDanger != Danger.None)
                    {
                        thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                            PathEndMode.OnCell, TraverseParms.For(actor), 32f, (Thing t) => baseChairValidator(t) && (int)t.Position.GetDangerFor(pawn, t.Map) <= (int)chewSpotDanger);
                    }
                }
                if (thing != null && !TryFindFreeSittingSpotOnThing(thing, out cell2))
                {
                    Log.Error("Could not find sitting spot on chewing chair! This is not supposed to happen - we looked for a free spot in a previous check!");
                }
                actor.ReserveSittableOrSpot(cell2, actor.CurJob);
                actor.Map.pawnDestinationReservationManager.Reserve(actor, actor.CurJob, cell2);
                actor.pather.StartPath(cell2, PathEndMode.OnCell);
                bool TryFindFreeSittingSpotOnThing(Thing t, out IntVec3 cell)
                {
                    foreach (IntVec3 item in t.OccupiedRect())
                    {
                        if (actor.CanReserveSittableOrSpot(item))
                        {
                            cell = item;
                            return true;
                        }
                    }
                    cell = default(IntVec3);
                    return false;
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
    }
}
