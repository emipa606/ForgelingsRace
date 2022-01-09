using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Forgelings
{
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
}
