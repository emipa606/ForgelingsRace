using AlienRace;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Forgelings
{
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class ResolveAllGraphicsPrefix_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.def == FDefOf.Forge_Forgeling_Race)
            {
                var pawn = __instance.pawn;
                ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
                if (thingDef_AlienRace != null)
                {
                    AlienPartGenerator.AlienComp comp = __instance.pawn.GetComp<AlienPartGenerator.AlienComp>();
                    AlienPartGenerator alienPartGenerator = thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator;
                    Graphic nakedGraphic;
                    GraphicPaths currentGraphicPath = thingDef_AlienRace.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage);
                    var bodyPath = currentGraphicPath.body;
                    if (bodyPath.NullOrEmpty())
                    {
                        nakedGraphic = null;
                    }
                    else
                    {
                        AlienPartGenerator alienPartGenerator2 = alienPartGenerator;
                        BodyTypeDef bodyType = pawn.story.bodyType;
                        Shader shader;
                        var path = AlienPartGenerator.GetNakedPath(pawn.story.bodyType, bodyPath, alienPartGenerator.useGenderedBodies ? pawn.gender.ToString() : "");
                        if (!(ContentFinder<Texture2D>.Get(path + "_northm", false) == null))
                        {
                            shader = ShaderDatabase.CutoutComplex;
                        }
                        else
                        {
                            ShaderTypeDef skinShader = currentGraphicPath.skinShader;
                            shader = (((skinShader != null) ? skinShader.Shader : null) ?? ShaderDatabase.Cutout);
                        }
                        nakedGraphic = GetNakedGraphic(pawn, bodyType, shader, __instance.pawn.story.SkinColor, alienPartGenerator.SkinColor(pawn, false), bodyPath, pawn.gender.ToString(), alienPartGenerator2.useGenderedBodies);
                    }
                    __instance.nakedGraphic = nakedGraphic;
                }
                __instance.ResolveApparelGraphics();
                PortraitsCache.SetDirty(pawn);
                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
            }
        }

        public static Graphic GetNakedGraphic(Pawn pawn, BodyTypeDef bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath, string gender, bool useGenderedBodies)
        {
            return GraphicDatabase.Get(typeof(Graphic_Multi), AlienPartGenerator.GetNakedPath(bodyType, userpath, useGenderedBodies ? gender : "")
                + Rand.RangeInclusiveSeeded(1, 5, pawn.thingIDNumber).ToString(), shader, Vector2.one, skinColor, skinColorSecond, null, null, null);
        }
    }
}
