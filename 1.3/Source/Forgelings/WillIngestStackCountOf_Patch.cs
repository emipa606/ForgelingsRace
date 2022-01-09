using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Forgelings
{
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
}
