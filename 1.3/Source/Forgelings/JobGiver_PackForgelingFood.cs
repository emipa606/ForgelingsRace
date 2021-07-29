using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Forgelings
{
	public class JobGiver_PackForgelingFood : ThinkNode_JobGiver
	{
		public const FoodPreferability MinFoodPreferability = FoodPreferability.MealAwful;
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.inventory == null)
			{
				return null;
			}
			float invNutrition = GetInventoryPackableFoodNutrition(pawn);
			if (invNutrition > 0.4f)
			{
				return null;
			}
			var state = Utils.AlterStats();
			Thing thing = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), 
				PathEndMode.ClosestTouch, TraverseParms.For(pawn), 20f, delegate (Thing t)
			{
				if (!IsGoodPackableFoodFor(t, pawn) || t.IsForbidden(pawn) || !pawn.CanReserve(t) || !t.IsSociallyProper(pawn))
				{
					return false;
				}
				if (invNutrition + Utils.foodEdibleForgeling[t.def] * (float)t.stackCount < 0.8f)
				{
					return false;
				}
				return true;
			}, (Thing x) => FoodUtility.FoodOptimality(pawn, x, FoodUtility.GetFinalIngestibleDef(x), 0f));
			Utils.RestoreStats(state);
			if (thing == null)
			{
				return null;
			}
			int a = Mathf.FloorToInt((pawn.needs.food.MaxLevel - invNutrition) / Utils.foodEdibleForgeling[thing.def]);
			a = Mathf.Min(a, thing.stackCount);
			a = Mathf.Max(a, 1);
			Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
			job.count = a;
			return job;
		}

		private float GetInventoryPackableFoodNutrition(Pawn pawn)
		{
			ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
			float num = 0f;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (IsGoodPackableFoodFor(innerContainer[i], pawn))
				{
					num += Utils.foodEdibleForgeling[innerContainer[i].def] * (float)innerContainer[i].stackCount;
				}
			}
			return num;
		}

		private bool IsGoodPackableFoodFor(Thing food, Pawn forPawn)
		{
			if (Utils.foodEdibleForgeling.ContainsKey(food.def))
            {
				return true;
            }
			return false;
		}
	}
}
