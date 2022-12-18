using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Forgelings;

public class JobGiver_PackForgelingFood : ThinkNode_JobGiver
{
    public const FoodPreferability MinFoodPreferability = FoodPreferability.MealAwful;

    public override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.inventory == null)
        {
            return null;
        }

        var invNutrition = GetInventoryPackableFoodNutrition(pawn);
        if (invNutrition > 0.4f)
        {
            return null;
        }

        var state = Utils.AlterStats();
        var thing = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(pawn.Position, pawn.Map,
            ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
            PathEndMode.ClosestTouch, TraverseParms.For(pawn), 20f, delegate(Thing t)
            {
                if (!IsGoodPackableFoodFor(t) || t.IsForbidden(pawn) || !pawn.CanReserve(t) ||
                    !t.IsSociallyProper(pawn))
                {
                    return false;
                }

                return !(invNutrition + (Utils.FoodEdibleForgeling[t.def] * t.stackCount) < 0.8f);
            }, x => FoodUtility.FoodOptimality(pawn, x, FoodUtility.GetFinalIngestibleDef(x), 0f));
        Utils.RestoreStats(state);
        if (thing == null)
        {
            return null;
        }

        var a = Mathf.FloorToInt((pawn.needs.food.MaxLevel - invNutrition) / Utils.FoodEdibleForgeling[thing.def]);
        a = Mathf.Min(a, thing.stackCount);
        a = Mathf.Max(a, 1);
        var job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
        job.count = a;
        return job;
    }

    private float GetInventoryPackableFoodNutrition(Pawn pawn)
    {
        var innerContainer = pawn.inventory.innerContainer;
        var num = 0f;
        foreach (var thing in innerContainer)
        {
            if (IsGoodPackableFoodFor(thing))
            {
                num += Utils.FoodEdibleForgeling[thing.def] * thing.stackCount;
            }
        }

        return num;
    }

    private bool IsGoodPackableFoodFor(Thing food)
    {
        return Utils.FoodEdibleForgeling.ContainsKey(food.def);
    }
}