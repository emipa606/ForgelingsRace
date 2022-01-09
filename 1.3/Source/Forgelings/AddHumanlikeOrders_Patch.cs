using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Forgelings
{
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
}
