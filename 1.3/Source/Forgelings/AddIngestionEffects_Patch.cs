using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Forgelings
{
    [HarmonyPatch(typeof(Toils_Ingest), "AddIngestionEffects")]
    public static class AddIngestionEffects_Patch
    {
        public static bool Prefix(ref Toil __result, Toil toil, Pawn chewer, TargetIndex ingestibleInd, TargetIndex eatSurfaceInd)
        {
            if (chewer.def == FDefOf.Forge_Forgeling_Race)
            {
                toil.WithEffect(delegate
                {
                    LocalTargetInfo target2 = toil.actor.CurJob.GetTarget(ingestibleInd);
                    if (!target2.HasThing)
                    {
                        return null;
                    }
                    EffecterDef result = EffecterDefOf.EatMeat;
                    return result;
                }, delegate
                {
                    if (!toil.actor.CurJob.GetTarget(ingestibleInd).HasThing)
                    {
                        return null;
                    }
                    Thing thing = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
                    if (chewer != toil.actor)
                    {
                        return chewer;
                    }
                    return (eatSurfaceInd != 0 && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid) ? toil.actor.CurJob.GetTarget(eatSurfaceInd) : ((LocalTargetInfo)thing);
                });
                toil.PlaySustainerOrSound(delegate
                {
                    if (!chewer.RaceProps.Humanlike)
                    {
                        return null;
                    }
                    LocalTargetInfo target = toil.actor.CurJob.GetTarget(ingestibleInd);
                    return SoundDefOf.RawMeat_Eat;
                });
                __result = toil;
                return false;
            }
            return true;
        }
    }
}
