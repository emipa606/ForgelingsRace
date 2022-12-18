using HarmonyLib;
using RimWorld;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodFilth")]
public static class DropBloodFilth_Patch
{
    public static bool Prefix(Pawn ___pawn)
    {
        if (___pawn.def != FDefOf.Forge_Forgeling_Race)
        {
            return true;
        }

        ((Spark)GenSpawn.Spawn(ThingDefOf.Spark, ___pawn.PositionHeld, ___pawn.MapHeld)).Launch(___pawn,
            ___pawn.Position, ___pawn.Position, ProjectileHitFlags.All);
        return false;
    }
}