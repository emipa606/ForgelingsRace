using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
public static class TrySatisfyPawnNeeds_Patch
{
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(Caravan_NeedsTracker __instance, Pawn pawn)
    {
        if (pawn.def != FDefOf.Forge_Forgeling_Race)
        {
            return true;
        }

        if (pawn.Dead)
        {
            return false;
        }

        var allNeeds = pawn.needs.AllNeeds;
        foreach (var need in allNeeds)
        {
            switch (need)
            {
                case Need_Rest need_Rest:
                    __instance.TrySatisfyRestNeed(pawn, need_Rest);
                    break;
                case Need_Food need_Food:
                    TrySatisfyFoodNeed(__instance.caravan, need_Food);
                    break;
                case Need_Chemical need_Chemical:
                    __instance.TrySatisfyChemicalNeed(pawn, need_Chemical);
                    break;
                case Need_Joy need_Joy:
                    __instance.TrySatisfyJoyNeed(pawn, need_Joy);
                    break;
            }
        }

        var psychicEntropy = pawn.psychicEntropy;
        if (psychicEntropy.Psylink != null)
        {
            __instance.TryGainPsyfocus(psychicEntropy);
        }

        return false;
    }

    private static void TrySatisfyFoodNeed(Caravan caravan, Need_Food foodNeed)
    {
        if ((int)foodNeed.CurCategory < 1)
        {
            return;
        }

        var food = CaravanInventoryUtility.AllInventoryItems(caravan)
            .Where(x => Utils.FoodEdibleForgeling.Keys.Contains(x.def)).RandomElementWithFallback();
        if (food == null)
        {
            return;
        }

        var owner = CaravanInventoryUtility.GetOwnerOf(caravan, food);
        foodNeed.CurLevel += IngestedFood(food, foodNeed.MaxLevel - foodNeed.CurLevel);
        if (!food.Destroyed)
        {
            return;
        }

        if (owner == null)
        {
            return;
        }

        owner.inventory.innerContainer.Remove(food);
        caravan.RecacheImmobilizedNow();
        caravan.RecacheDaysWorthOfFood();
    }

    private static float IngestedFood(Thing thing, float nutritionWanted)
    {
        var nutritionPerCount = Utils.FoodEdibleForgeling[thing.def];
        var stackConsumed = (int)Mathf.Min(nutritionWanted / nutritionPerCount, thing.stackCount);
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