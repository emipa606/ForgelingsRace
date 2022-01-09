using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool) })]
    public static class WillEat_Patch1
    {
        private static void Postfix(ref bool __result, Pawn p, Thing food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (food?.def != null && p != null)
            {
                if (Utils.FoodEdibleForgeling.ContainsKey(food.def) && p.def != FDefOf.Forge_Forgeling_Race)
                {
                    __result = false;
                }
                else if (p.def == FDefOf.Forge_Forgeling_Race && !Utils.FoodEdibleForgeling.ContainsKey(food.def))
                {
                    __result = false;
                }
                else if (p.def == FDefOf.Forge_Forgeling_Race && Utils.FoodEdibleForgeling.ContainsKey(food.def))
                {
                    __result = true;
                }
            }
        }
    }
}
