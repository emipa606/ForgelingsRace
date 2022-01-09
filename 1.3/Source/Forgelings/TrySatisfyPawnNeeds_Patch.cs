using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Forgelings
{

    [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
    public static class TrySatisfyPawnNeeds_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Caravan_NeedsTracker __instance, Pawn pawn)
        {
            if (pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                if (pawn.Dead)
                {
                    return false;
                }
                List<Need> allNeeds = pawn.needs.AllNeeds;
                for (int i = 0; i < allNeeds.Count; i++)
                {
                    Need need = allNeeds[i];
                    Need_Rest need_Rest = need as Need_Rest;
                    Need_Food need_Food = need as Need_Food;
                    Need_Chemical need_Chemical = need as Need_Chemical;
                    Need_Joy need_Joy = need as Need_Joy;
                    if (need_Rest != null)
                    {
                        __instance.TrySatisfyRestNeed(pawn, need_Rest);
                    }
                    else if (need_Food != null)
                    {
                        TrySatisfyFoodNeed(__instance.caravan, pawn, need_Food);
                    }
                    else if (need_Chemical != null)
                    {
                        __instance.TrySatisfyChemicalNeed(pawn, need_Chemical);
                    }
                    else if (need_Joy != null)
                    {
                        __instance.TrySatisfyJoyNeed(pawn, need_Joy);
                    }
                }
                Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
                if (psychicEntropy.Psylink != null)
                {
                    __instance.TryGainPsyfocus(psychicEntropy);
                }
                return false;
            }
            return true;
        }
        
        private static void TrySatisfyFoodNeed(Caravan caravan, Pawn pawn, Need_Food foodNeed)
        {
        	if ((int)foodNeed.CurCategory < 1)
        	{
        		return;
        	}
            var food = CaravanInventoryUtility.AllInventoryItems(caravan).Where(x => Utils.FoodEdibleForgeling.Keys.Contains(x.def)).RandomElementWithFallback();
        	if (food != null)
        	{
        		var owner = CaravanInventoryUtility.GetOwnerOf(caravan, food);
        		foodNeed.CurLevel += IngestedFood(food, pawn, foodNeed.MaxLevel - foodNeed.CurLevel);        
        		if (food.Destroyed)
        		{
        			if (owner != null)
        			{
        				owner.inventory.innerContainer.Remove(food);
        				caravan.RecacheImmobilizedNow();
        				caravan.RecacheDaysWorthOfFood();
        			}
        		}
        	}
        }
        
        private static float IngestedFood(Thing thing, Pawn pawn, float nutritionWanted)
        {
            var nutritionPerCount = Utils.FoodEdibleForgeling[thing.def];
            int stackConsumed = (int)Mathf.Min(nutritionWanted / nutritionPerCount, thing.stackCount);
        	if (thing.stackCount > stackConsumed)
        	{
        		thing.SplitOff(stackConsumed);
        	}
        	else
        	{
        		thing.Destroy();
        	}
        	return nutritionPerCount * stackConsumed;
        }
    }
}
