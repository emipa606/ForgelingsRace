using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(PawnGenerator), "GenerateSkills")]
public static class GenerateSkills_Patch
{
    public static void Postfix(Pawn pawn)
    {
        if (pawn.def != FDefOf.Forge_Forgeling_Race)
        {
            return;
        }

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