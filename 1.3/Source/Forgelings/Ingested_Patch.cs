using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace Forgelings
{
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
}
