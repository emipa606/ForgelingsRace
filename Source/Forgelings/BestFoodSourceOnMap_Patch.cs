﻿using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using static RimWorld.FoodUtility;

namespace Forgelings;

[HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap_NewTemp")]
public static class BestFoodSourceOnMap_Patch
{
    public static bool Prefix(ref Thing __result, Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef,
        FoodPreferability maxPref = FoodPreferability.MealLavish,
        bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true,
        bool allowDispenserEmpty = true,
        bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false,
        bool forceScanWholeMap = false, bool ignoreReservations = false,
        bool calculateWantedStackCount = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined,
        float? minNutrition = null)
    {
        if (eater?.def == FDefOf.Forge_Forgeling_Race)
        {
            var stats = Utils.AlterStats();
            __result = BestFoodSourceOnMapOverride(getter, eater, desperate, out foodDef, maxPref, allowPlant,
                allowDrug,
                allowCorpse, allowDispenserFull, allowDispenserEmpty, allowForbidden, allowSociallyImproper,
                allowHarvest, forceScanWholeMap, ignoreReservations, calculateWantedStackCount, minPrefOverride,
                minNutrition);
            Utils.RestoreStats(stats);
            return false;
        }

        foodDef = null;
        return true;
    }

    public static Thing BestFoodSourceOnMapOverride(Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef,
        FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true,
        bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true,
        bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false,
        bool forceScanWholeMap = false, bool ignoreReservations = false, bool calculateWantedStackCount = false,
        FoodPreferability minPrefOverride = FoodPreferability.Undefined, float? minNutrition = null)
    {
        foodDef = null;
        var getterCanManipulate = getter.RaceProps.ToolUser &&
                                  getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
        if (!getterCanManipulate && getter != eater)
        {
            Log.Error(string.Concat(getter, " tried to find food to bring to ", eater, " but ", getter,
                " is incapable of Manipulation."));
            return null;
        }

        FoodPreferability minPref;
        if (minPrefOverride == FoodPreferability.Undefined)
        {
            if (eater.NonHumanlikeOrWildMan())
            {
                minPref = FoodPreferability.NeverForNutrition;
            }
            else if (desperate)
            {
                minPref = FoodPreferability.DesperateOnly;
            }
            else
            {
                minPref = (int)eater.needs.food.CurCategory >= 2
                    ? FoodPreferability.RawBad
                    : FoodPreferability.MealAwful;
            }
        }
        else
        {
            minPref = minPrefOverride;
        }

        bool FoodValidator(Thing t)
        {
            var stackCount = 1;
            var statValue = t.GetStatValue(StatDefOf.Nutrition);
            if (minNutrition.HasValue)
            {
                stackCount = StackCountForNutrition(minNutrition.Value, statValue);
            }
            else if (calculateWantedStackCount)
            {
                stackCount = WillIngestStackCountOf(eater, t.def, statValue);
            }

            return (int)t.def.ingestible.preferability >= (int)minPref &&
                   (int)t.def.ingestible.preferability <= (int)maxPref && eater.WillEat_NewTemp(t, getter) &&
                   t.def.IsNutritionGivingIngestible && t.IngestibleNow && (allowCorpse || t is not Corpse) &&
                   (allowDrug || !t.def.IsDrug) && (allowForbidden || !t.IsForbidden(getter)) &&
                   (desperate || !t.IsNotFresh()) && !t.IsDessicated() &&
                   IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) &&
                   (getter.AnimalAwareOf(t) || forceScanWholeMap) &&
                   (ignoreReservations || getter.CanReserve(t, 10, stackCount));
        }

        foreach (var def in Utils.FoodEdibleForgeling.Keys.ToList().InRandomOrder())
        {
            var thingRequest = ThingRequest.ForDef(def);
            var bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position,
                getter.Map.listerThings.ThingsMatching(thingRequest),
                PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, FoodValidator);
            if (foodDef == null && bestThing != null)
            {
                foodDef = GetFinalIngestibleDef(bestThing);
            }

            if (bestThing != null)
            {
                return bestThing;
            }
        }

        return null;
    }
}