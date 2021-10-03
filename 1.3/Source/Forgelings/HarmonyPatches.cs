using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static RimWorld.FoodUtility;

namespace Forgelings
{
    public class OverridenValues
    {
        public float? originalNutrients;
        public IngestibleProperties originalIngestibleProps;
    }

    [DefOf]
    public static class FDefOf
    {
        public static ThingDef Forge_Forgeling_Race;
        public static ThingDef Forge_Apparel_CruciblePlate;
        public static ThingDef Forge_ForgelingSpot;
        public static ThingDef FueledSmithy;
    }
    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static Dictionary<ThingDef, float> foodEdibleForgeling = new Dictionary<ThingDef, float>
        {
            {ThingDefOf.WoodLog, 0.1f},
            {ThingDefOf.Chemfuel, 0.25f}
        };

        public static Harmony harmonyInstance;
        static Utils()
        {
            harmonyInstance = new Harmony("Forgelings.Mod");
            harmonyInstance.PatchAll();
            if (FDefOf.Forge_ForgelingSpot.recipes is null)
            {
                FDefOf.Forge_ForgelingSpot.recipes = new List<RecipeDef>();
            }
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                if (recipe.AllRecipeUsers.Contains(FDefOf.FueledSmithy))
                {
                    if (recipe.recipeUsers is null)
                    {
                        recipe.recipeUsers = new List<ThingDef>();
                    }
                    recipe.recipeUsers.Add(FDefOf.Forge_ForgelingSpot);
                }
            }
        }

        public static Dictionary<ThingDef, OverridenValues> AlterStats()
        {
            var dict = new Dictionary<ThingDef, OverridenValues>();
            foreach (var data in foodEdibleForgeling)
            {
                var def = data.Key;
                dict[def] = new OverridenValues();
                if (def.ingestible != null)
                {
                    dict[def].originalIngestibleProps = def.ingestible;
                }
                if (def.StatBaseDefined(StatDefOf.Nutrition))
                {
                    dict[def].originalNutrients = def.GetStatValueAbstract(StatDefOf.Nutrition);
                }
                def.ingestible = new IngestibleProperties
                {
                    foodType = FoodTypeFlags.Meal,
                    optimalityOffsetHumanlikes = 16,
                    preferability = FoodPreferability.MealFine,
                    tasteThought = ThoughtDefOf.AteLavishMeal,
                    ingestEffect = EffecterDefOf.EatMeat,
                    ingestSound = SoundDefOf.RawMeat_Eat,
                    maxNumToIngestAtOnce = 20,
                    baseIngestTicks = 500,
                    parent = def
                };
                def.SetStatBaseValue(StatDefOf.Nutrition, data.Value);
                //Log.Message("0 Setting: " + def);
            }
            //Log.Message("Altering stats" + new StackTrace().ToString());
            return dict;
        }

        public static void RestoreStats(Dictionary<ThingDef, OverridenValues> dict)
        {
            foreach (var data in dict)
            {
                if (data.Value.originalIngestibleProps != null)
                {
                    //Log.Message("Setting ingestible to " + data.Key);
                    data.Key.ingestible = data.Value.originalIngestibleProps;
                }
                else
                {
                    //Log.Message("Removing ingestible from " + data.Key);
                    data.Key.ingestible = null;
                }
                if (data.Value.originalNutrients.HasValue)
                {
                    //Log.Message("Setting stats to " + data.Key);
                    data.Key.SetStatBaseValue(StatDefOf.Nutrition, data.Value.originalNutrients.Value);
                }
                else
                {
                    //Log.Message("Removing stats from " + data.Key);
                    data.Key.statBases.RemoveAll(x => x.stat == StatDefOf.Nutrition);
                }
            }
            //Log.Message("Restoring stats: " + new StackTrace().ToString());

        }
    }

    [HarmonyPatch(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(Thing), typeof(Pawn) })]
    public static class Patch_IsForbidden
    {
        private static void Postfix(ref bool __result, Thing t, Pawn pawn)
        {
            if (!__result && pawn != null && t?.def == FDefOf.Forge_ForgelingSpot && pawn.def != FDefOf.Forge_Forgeling_Race)
            {
                __result = true;
            }
        }
    }
    [HarmonyPatch(typeof(StatPart_WorkTableOutdoors), "Applies", new Type[] { typeof(ThingDef), typeof(Map), typeof(IntVec3) })]
    public static class Patch_Applies
    {
        private static void Postfix(ref bool __result, ThingDef def, Map map, IntVec3 c)
        {
            if (def == FDefOf.Forge_ForgelingSpot)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StatPart_WorkTableTemperature), "Applies", new Type[] { typeof(ThingDef), typeof(Map), typeof(IntVec3) })]
    public static class StatPart_WorkTableTemperature_Patch_Applies
    {
        private static void Postfix(ref bool __result, ThingDef tDef, Map map, IntVec3 c)
        {
            if (tDef == FDefOf.Forge_ForgelingSpot)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
    public static class TryGiveJob_Patch
    {
        public static bool Prefix(JobGiver_GetFood __instance, Pawn pawn, ref Job __result, HungerCategory ___minCategory, float ___maxLevelPercentage, bool ___forceScanWholeMap, out Dictionary<ThingDef, OverridenValues> __state)
        {
            if (pawn?.def == FDefOf.Forge_Forgeling_Race)
            {
                __result = TryGiveJob(pawn, ___minCategory, ___maxLevelPercentage, ___forceScanWholeMap);
                __state = Utils.AlterStats();
                return false;
            }
            else
            {
                IngestibleNow_Patch.disableManually = true;
                __state = null;
            }
            return true;
        }
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
            IngestibleNow_Patch.disableManually = false;
    
        }
        public static Job TryGiveJob(Pawn pawn, HungerCategory minCategory, float maxLevelPercentage, bool forceScanWholeMap)
        {
            Need_Food food = pawn.needs.food;
            if (food == null || (int)food.CurCategory < (int)minCategory || food.CurLevelPercentage > maxLevelPercentage)
            {
                return null;
            }
            bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out var foodSource, out var foodDef, canRefillDispenser: true, canUseInventory: true, allowForbidden: false, false, allowSociallyImproper: false, pawn.IsWildMan(), forceScanWholeMap))
            {
                return null;
            }

            float nutrition = Utils.foodEdibleForgeling[foodSource.def];
            Job job3 = JobMaker.MakeJob(JobDefOf.Ingest, foodSource);
            job3.count = FoodUtility.WillIngestStackCountOf(pawn, foodDef, nutrition);
            return job3;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap")]
    public static class BestFoodSourceOnMap_Patch
    {
        public static bool Prefix(ref Thing __result, Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, 
            bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true,
            bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, 
            bool calculateWantedStackCount = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined, float? minNutrition = null)
        {
            if (eater?.def == FDefOf.Forge_Forgeling_Race)
            {
                __result = BestFoodSourceOnMapOverride(getter, eater, desperate, out foodDef, maxPref, allowPlant, allowDrug, 
                    allowCorpse, allowDispenserFull, allowDispenserEmpty, allowForbidden, allowSociallyImproper, allowHarvest, forceScanWholeMap, ignoreReservations, calculateWantedStackCount, minPrefOverride, minNutrition);
                return false;
            }
            foodDef = null;
            return true;
        }

        public static Thing BestFoodSourceOnMapOverride(Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, bool calculateWantedStackCount = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined, float? minNutrition = null)
        {
            foodDef = null;
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && getter != eater)
            {
                Log.Error(string.Concat(getter, " tried to find food to bring to ", eater, " but ", getter, " is incapable of Manipulation."));
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
                    minPref = (((int)eater.needs.food.CurCategory >= 2) ? FoodPreferability.RawBad : FoodPreferability.MealAwful);
                }
            }
            else
            {
                minPref = minPrefOverride;
            }
            Predicate<Thing> foodValidator = delegate (Thing t)
            {
                Building_NutrientPasteDispenser building_NutrientPasteDispenser = t as Building_NutrientPasteDispenser;
                if (building_NutrientPasteDispenser != null)
                {
                    if (!allowDispenserFull || !getterCanManipulate || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability < (int)minPref 
                    || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability > (int)maxPref || !eater.WillEat(ThingDefOf.MealNutrientPaste, getter) 
                    || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !building_NutrientPasteDispenser.powerComp.PowerOn 
                    || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !t.InteractionCell.Standable(t.Map) 
                    || !FoodUtility.IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) 
                    || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
                    {
                        return false;
                    }
                }
                else
                {
                    int stackCount = 1;
                    float statValue = t.GetStatValue(StatDefOf.Nutrition);
                    if (minNutrition.HasValue)
                    {
                        stackCount = StackCountForNutrition(minNutrition.Value, statValue);
                    }
                    else if (calculateWantedStackCount)
                    {
                        stackCount = WillIngestStackCountOf(eater, t.def, statValue);
                    }
                    if ((int)t.def.ingestible.preferability < (int)minPref || (int)t.def.ingestible.preferability > (int)maxPref || !eater.WillEat(t, getter) || !t.def.IsNutritionGivingIngestible || !t.IngestibleNow || (!allowCorpse && t is Corpse) || (!allowDrug && t.def.IsDrug) || (!allowForbidden && t.IsForbidden(getter)) || (!desperate && t.IsNotFresh()) || t.IsDessicated() || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || (!getter.AnimalAwareOf(t) && !forceScanWholeMap) || (!ignoreReservations && !getter.CanReserve(t, 10, stackCount)))
                    {
                        return false;
                    }
                }
                return true;
            };
            foreach (var def in Utils.foodEdibleForgeling.Keys.ToList().InRandomOrder())
            {
                ThingRequest thingRequest = ThingRequest.ForDef(def);
                Thing bestThing;
                if (getter.RaceProps.Humanlike)
                {

                    bestThing = FoodUtility.SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(thingRequest), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator);
                    if (allowHarvest && getterCanManipulate)
                    {
                        Thing thing = GenClosest.ClosestThingReachable(searchRegionsMax: (!forceScanWholeMap || bestThing != null) ? 30 : (-1), root: getter.Position, map: getter.Map, thingReq: ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), peMode: PathEndMode.Touch, traverseParams: TraverseParms.For(getter), maxDistance: 9999f, validator: delegate (Thing x)
                        {
                            Plant plant = (Plant)x;
                            if (!plant.HarvestableNow)
                            {
                                return false;
                            }
                            ThingDef harvestedThingDef = plant.def.plant.harvestedThingDef;
                            if (!harvestedThingDef.IsNutritionGivingIngestible)
                            {
                                return false;
                            }
                            if (!eater.WillEat(harvestedThingDef, getter))
                            {
                                return false;
                            }
                            if (!getter.CanReserve(plant))
                            {
                                return false;
                            }
                            if (!allowForbidden && plant.IsForbidden(getter))
                            {
                                return false;
                            }
                            return (bestThing == null || (int)GetFinalIngestibleDef(bestThing).ingestible.preferability < (int)harvestedThingDef.ingestible.preferability) ? true : false;
                        });
                        if (thing != null)
                        {
                            bestThing = thing;
                            foodDef = GetFinalIngestibleDef(thing, harvest: true);
                        }
                    }
                    if (foodDef == null && bestThing != null)
                    {
                        foodDef = GetFinalIngestibleDef(bestThing);
                    }
                }
                else
                {
                    int maxRegionsToScan = GetMaxRegionsToScan(getter, forceScanWholeMap);
                    FoodUtility.filtered.Clear();
                    foreach (Thing item in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, useCenter: true))
                    {
                        Pawn pawn = item as Pawn;
                        if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
                        {
                            filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
                        }
                    }
                    bool ignoreEntirelyForbiddenRegions = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, cellTarget: true) && getter.playerSettings != null && getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null;
                    Predicate<Thing> validator = delegate (Thing t)
                    {
                        if (!foodValidator(t))
                        {
                            return false;
                        }
                        if (filtered.Contains(t))
                        {
                            return false;
                        }
                        if (!(t is Building_NutrientPasteDispenser) && (int)t.def.ingestible.preferability <= 2)
                        {
                            return false;
                        }
                        return (!t.IsNotFresh()) ? true : false;
                    };
                    bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, validator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                    filtered.Clear();
                    if (bestThing == null)
                    {
                        desperate = true;
                        bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                    }
                    if (bestThing != null)
                    {
                        foodDef = GetFinalIngestibleDef(bestThing);
                    }
                }
                return bestThing;
            }
            return null;
        }
    }


    [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
    public static class FoodOptimality_Patch
    {
        public static bool Prefix(ref float __result, out Dictionary<ThingDef, OverridenValues> __state, Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
        {
            if (eater?.def == FDefOf.Forge_Forgeling_Race)
            {
                if (!Utils.foodEdibleForgeling.ContainsKey(foodSource.def))
                {
                    __result = -9999999f;
                    __state = null;
                    return false;
                }
                else
                {
                    __state = Utils.AlterStats();
                }
            }
            else
            {
                __state = null;
            }
            if (foodSource.def.ingestible is null)
            {
                __result = -9999999f;
                return false;
            }
            return true;
        }
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class AddHumanlikeOrders_Patch
    {
        public static void Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts, out Dictionary<ThingDef, OverridenValues> __state)
        {
            if (pawn?.def == FDefOf.Forge_Forgeling_Race)
            {
                __state = Utils.AlterStats();
            }
            else
            {
                __state = null;
            }
        }
    
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "WillIngestStackCountOf")]
    public static class WillIngestStackCountOf_Patch
    {
        public static void Prefix(Pawn ingester, ThingDef def, ref float singleFoodNutrition, out Dictionary<ThingDef, OverridenValues> __state)
        {
            if (ingester?.def == FDefOf.Forge_Forgeling_Race)
            {
                __state = Utils.AlterStats();
                singleFoodNutrition = def.GetStatValueAbstract(StatDefOf.Nutrition);
            }
            else
            {
                __state = null;
            }
        }
    
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
        }
    }

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
    
    [HarmonyPatch(typeof(Toils_Ingest), "ChewIngestible")]
    public static class ChewIngestible_Patch
    {
        public static bool Prefix(ref Toil __result, Pawn chewer, float durationMultiplier, TargetIndex ingestibleInd, TargetIndex eatSurfaceInd = TargetIndex.None)
        {
            if (chewer.def == FDefOf.Forge_Forgeling_Race)
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Pawn actor = toil.actor;
                    Thing thing4 = actor.CurJob.GetTarget(ingestibleInd).Thing;
                    toil.actor.pather.StopDead();
                    actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt((float)500 * durationMultiplier);
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
                        Thing thing3 = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
                        if (thing3 != null && thing3.Spawned)
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
                    Thing thing2 = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
                    return (thing2 == null) ? 1f : (1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round(500 * durationMultiplier));
                });
                toil.defaultCompleteMode = ToilCompleteMode.Delay;
                toil.FailOnDestroyedOrNull(ingestibleInd);
                toil.AddFinishAction(delegate
                {
                    if (chewer != null && chewer.CurJob != null)
                    {
                        Thing thing = chewer.CurJob.GetTarget(ingestibleInd).Thing;
                        if (thing != null && chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, thing))
                        {
                            chewer.Map.physicalInteractionReservationManager.Release(chewer, toil.actor.CurJob, thing);
                        }
                    }
                });
                toil.handlingFacing = true;
                Toils_Ingest.AddIngestionEffects(toil, chewer, ingestibleInd, eatSurfaceInd);
                __result = toil;
                return false;
            }
            return true;
        }
    }
    
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

    [HarmonyPatch(typeof(FoodUtility), "TryFindBestFoodSourceFor")]
    public static class TryFindBestFoodSourceFor_Patch
    {
        public static void Prefix(Pawn getter, Pawn eater, out Dictionary<ThingDef, OverridenValues> __state)
        {
            if (eater?.def == FDefOf.Forge_Forgeling_Race)
            {
                __state = Utils.AlterStats();

            }
            else
            {
                __state = null;
            }
        }
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
        }
    }

    [HarmonyPatch(typeof(Thing), "Ingested")]
    public static class Ingested_Patch
    {
        public static void Prefix(Pawn ingester, float nutritionWanted, out Dictionary<ThingDef, OverridenValues> __state)
        {
            if (ingester?.def == FDefOf.Forge_Forgeling_Race)
            {
                __state = Utils.AlterStats();
            }
            else
            {
                __state = null;
            }
        }
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
        }
    }
    
    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    public static class ThoughtsFromIngesting_Patch
    {
        public static void Prefix(Pawn ingester, Thing foodSource, ThingDef foodDef, out Dictionary<ThingDef, OverridenValues> __state)
        {
            if (ingester?.def == FDefOf.Forge_Forgeling_Race)
            {
                __state = Utils.AlterStats();
            }
            else
            {
                __state = null;
            }
        }
        public static void Postfix(Dictionary<ThingDef, OverridenValues> __state)
        {
            if (__state != null)
            {
                Utils.RestoreStats(__state);
            }
        }
    }

    [HarmonyPatch(typeof(Thing), "IngestibleNow", MethodType.Getter)]
    public static class IngestibleNow_Patch
    {
        public static bool disableManually;
        public static void Postfix(Thing __instance, ref bool __result)
        {
            if (!disableManually && Utils.foodEdibleForgeling.ContainsKey(__instance.def))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_Ingest), "ModifyCarriedThingDrawPosWorker")]
    public static class ModifyCarriedThingDrawPosWorker_Patch
    {
        public static bool Prefix(JobDriver_Ingest __instance, ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
        {
            if (pawn.def == FDefOf.Forge_Forgeling_Race) 
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool) })]
    public static class WillEat_Patch1
    {
        private static void Postfix(ref bool __result, Pawn p, Thing food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (food != null) 
            {
                if (Utils.foodEdibleForgeling.ContainsKey(food.def) && p.def != FDefOf.Forge_Forgeling_Race)
                {
                    __result = false;
                }
                else if (p.def == FDefOf.Forge_Forgeling_Race && !Utils.foodEdibleForgeling.ContainsKey(food.def))
                {
                    __result = false;
                }
                else if (p.def == FDefOf.Forge_Forgeling_Race && Utils.foodEdibleForgeling.ContainsKey(food.def))
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool) })]
    public static class WillEat_Patch2
    {
        private static void Postfix(ref bool __result, Pawn p, ThingDef food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (Utils.foodEdibleForgeling.ContainsKey(food) && p.def != FDefOf.Forge_Forgeling_Race)
            {
                __result = false;
            }
            else if (p.def == FDefOf.Forge_Forgeling_Race && !Utils.foodEdibleForgeling.ContainsKey(food))
            {
                __result = false;
            }
            else if (p.def == FDefOf.Forge_Forgeling_Race && Utils.foodEdibleForgeling.ContainsKey(food))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(RaceRestrictionSettings), "CanWear")]
    public static class RaceRestrictionSettings_Patch
    {
        public static void Postfix(ref bool __result, ThingDef apparel, ThingDef race)
        {
            if (race != FDefOf.Forge_Forgeling_Race && apparel == FDefOf.Forge_Apparel_CruciblePlate)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateSkills")]
    public static class GenerateSkills_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                var mining = pawn.skills.GetSkill(SkillDefOf.Mining);
                if (!mining.TotallyDisabled && mining.passion < Passion.Major && Rand.Chance(0.85f))
                {
                    mining.passion = Rand.Bool ? Passion.Minor : Passion.Major;
                }
                var crafting = pawn.skills.GetSkill(SkillDefOf.Crafting);
                if (!crafting.TotallyDisabled && crafting.passion < Passion.Major && Rand.Chance(0.85f))
                {
                    crafting.passion = Rand.Bool ? Passion.Minor : Passion.Major;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodFilth")]
    public static class DropBloodFilth_Patch
    {
        public static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            if (___pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                ((Spark)GenSpawn.Spawn(ThingDefOf.Spark, ___pawn.Position, ___pawn.Map)).Launch(___pawn, ___pawn.Position, ___pawn.Position, ProjectileHitFlags.All);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class ResolveAllGraphicsPrefix_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                var pawn = __instance.pawn;
                ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
                if (thingDef_AlienRace != null)
                {
                    AlienPartGenerator.AlienComp comp = __instance.pawn.GetComp<AlienPartGenerator.AlienComp>();
                    AlienPartGenerator alienPartGenerator = thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator;
                    Graphic nakedGraphic;
                    GraphicPaths currentGraphicPath = thingDef_AlienRace.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage);
                    var bodyPath = currentGraphicPath.body;
                    if (bodyPath.NullOrEmpty())
                    {
                        nakedGraphic = null;
                    }
                    else
                    {
                        AlienPartGenerator alienPartGenerator2 = alienPartGenerator;
                        BodyTypeDef bodyType = pawn.story.bodyType;
                        Shader shader;
                        var path = AlienPartGenerator.GetNakedPath(pawn.story.bodyType, bodyPath, alienPartGenerator.useGenderedBodies ? pawn.gender.ToString() : "");
                        if (!(ContentFinder<Texture2D>.Get(path + "_northm", false) == null))
                        {
                            shader = ShaderDatabase.CutoutComplex;
                        }
                        else
                        {
                            ShaderTypeDef skinShader = currentGraphicPath.skinShader;
                            shader = (((skinShader != null) ? skinShader.Shader : null) ?? ShaderDatabase.Cutout);
                        }
                        nakedGraphic = GetNakedGraphic(pawn, bodyType, shader, __instance.pawn.story.SkinColor, alienPartGenerator.SkinColor(pawn, false), bodyPath, pawn.gender.ToString(), alienPartGenerator2.useGenderedBodies);
                    }
                    __instance.nakedGraphic = nakedGraphic;
                }
                __instance.ResolveApparelGraphics();
                PortraitsCache.SetDirty(pawn);
                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
            }
        }

        public static Graphic GetNakedGraphic(Pawn pawn, BodyTypeDef bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender, bool useGenderedBodies)
        {
            return GraphicDatabase.Get(typeof(Graphic_Multi), AlienPartGenerator.GetNakedPath(bodyType, userpath, useGenderedBodies ? gender : "") 
                + Rand.RangeInclusiveSeeded(1, 5, pawn.thingIDNumber).ToString(), shader, Vector2.one, skinColor, skinColorSecond, null, null, null);
        }
    }
}
