using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Forgelings
{
    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static Dictionary<ThingDef, float> FoodEdibleForgeling => new Dictionary<ThingDef, float>
        {
            {ThingDefOf.WoodLog, 0.1f},
            {ThingDefOf.Chemfuel, 0.25f}
        };

        public static Harmony harmonyInstance;
        static Utils()
        {
            harmonyInstance = new Harmony("Forgelings.Mod");
            harmonyInstance.PatchAll();
            if (FDefOf.Forge_ForgelingSpot.recipes is null)
            {
                FDefOf.Forge_ForgelingSpot.recipes = new List<RecipeDef>();
            }
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                if (recipe.AllRecipeUsers.Contains(FDefOf.FueledSmithy))
                {
                    if (recipe.recipeUsers is null)
                    {
                        recipe.recipeUsers = new List<ThingDef>();
                    }
                    recipe.recipeUsers.Add(FDefOf.Forge_ForgelingSpot);
                }
            }
        }

        public static Dictionary<ThingDef, OverridenValues> AlterStats()
        {
            var dict = new Dictionary<ThingDef, OverridenValues>();
            foreach (var data in FoodEdibleForgeling)
            {
                var def = data.Key;
                dict[def] = new OverridenValues();
                if (def.ingestible != null)
                {
                    dict[def].originalIngestibleProps = def.ingestible;
                }
                if (def.StatBaseDefined(StatDefOf.Nutrition))
                {
                    dict[def].originalNutrients = def.GetStatValueAbstract(StatDefOf.Nutrition);
                }
                def.ingestible = new IngestibleProperties
                {
                    foodType = FoodTypeFlags.Meal,
                    optimalityOffsetHumanlikes = 16,
                    preferability = FoodPreferability.MealFine,
                    tasteThought = ThoughtDefOf.AteLavishMeal,
                    ingestEffect = EffecterDefOf.EatMeat,
                    ingestSound = SoundDefOf.RawMeat_Eat,
                    maxNumToIngestAtOnce = 20,
                    baseIngestTicks = 500,
                    parent = def
                };
                def.SetStatBaseValue(StatDefOf.Nutrition, data.Value);
                //Log.Message("0 Setting: " + def);
            }
            //Log.Message("Altering stats" + new StackTrace().ToString());
            return dict;
        }

        public static void RestoreStats(Dictionary<ThingDef, OverridenValues> dict)
        {
            foreach (var data in dict)
            {
                if (data.Value.originalIngestibleProps != null)
                {
                    //Log.Message("Setting ingestible to " + data.Key);
                    data.Key.ingestible = data.Value.originalIngestibleProps;
                }
                else
                {
                    //Log.Message("Removing ingestible from " + data.Key);
                    data.Key.ingestible = null;
                }
                if (data.Value.originalNutrients.HasValue)
                {
                    //Log.Message("Setting stats to " + data.Key);
                    data.Key.SetStatBaseValue(StatDefOf.Nutrition, data.Value.originalNutrients.Value);
                }
                else
                {
                    //Log.Message("Removing stats from " + data.Key);
                    data.Key.statBases.RemoveAll(x => x.stat == StatDefOf.Nutrition);
                }
            }
            //Log.Message("Restoring stats: " + new StackTrace().ToString());

        }
    }
}
