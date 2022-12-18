using HarmonyLib;
using Verse;

namespace Forgelings;

[HarmonyPatch(typeof(Thing), "IngestibleNow", MethodType.Getter)]
public static class IngestibleNow_Patch
{
    public static bool disableManually;

    public static void Postfix(Thing __instance, ref bool __result)
    {
        if (!disableManually && Utils.FoodEdibleForgeling.ContainsKey(__instance.def))
        {
            __result = true;
        }
    }
}