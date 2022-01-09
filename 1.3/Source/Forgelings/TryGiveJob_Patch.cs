using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Forgelings
{
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

            float nutrition = Utils.FoodEdibleForgeling[foodSource.def];
            Job job3 = JobMaker.MakeJob(JobDefOf.Ingest, foodSource);
            job3.count = FoodUtility.WillIngestStackCountOf(pawn, foodDef, nutrition);
            return job3;
        }
    }
}
