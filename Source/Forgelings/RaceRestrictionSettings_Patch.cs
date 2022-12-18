using AlienRace;
using HarmonyLib;
using Verse;

namespace Forgelings;

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