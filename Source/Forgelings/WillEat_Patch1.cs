using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(FoodUtility), "WillEat_NewTemp", typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool),
    typeof(bool))]
public static class WillEat_Patch1
{
    private static void Postfix(ref bool __result, Pawn p, Thing food)
    {
        if (p is not { Spawned: true })
        {
            return;
        }

        if (food?.def == null)
        {
            return;
        }

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