using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(FoodUtility), "WillEat_NewTemp", typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool),
    typeof(bool))]
public static class WillEat_Patch2
{
    private static void Postfix(ref bool __result, Pawn p, ThingDef food)
    {
        if (Utils.FoodEdibleForgeling.ContainsKey(food) && p.def != FDefOf.Forge_Forgeling_Race)
        {
            __result = false;
        }
        else if (p.def == FDefOf.Forge_Forgeling_Race && !Utils.FoodEdibleForgeling.ContainsKey(food))
        {
            __result = false;
        }
        else if (p.def == FDefOf.Forge_Forgeling_Race && Utils.FoodEdibleForgeling.ContainsKey(food))
        {
            __result = true;
        }
    }
}