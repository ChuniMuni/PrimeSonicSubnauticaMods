﻿namespace UpgradedVehicles
{
    using System.Collections.Generic;
    using Common;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using SMLHelper.V2.Handlers;
    using UnityEngine;

    internal class SeaMothMk3
    {
        public static TechType TechTypeID { get; private set; } = TechType.None; // Default for when not set but still used in comparisons
        public const string NameID = "SeaMothMk3";
        public const string FriendlyName = "Seamoth Mk3";
        public const string Description = "The highest end SeaMoth. Ready for adventures in the most hostile environments.";

        public static void Patch(TechType seamothDepthMk5)
        {
            TechTypeID = TechTypeHandler.AddTechType(NameID, FriendlyName, Description);

            SpriteHandler.RegisterSprite(TechTypeID, @"./QMods/UpgradedVehicles/Assets/SeamothMk3.png");

            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Constructor, TechTypeID, "Vehicles");
            CraftDataHandler.SetCraftingTime(TechTypeID, 20f);
            CraftDataHandler.AddTechData(TechTypeID, GetRecipe(seamothDepthMk5));

            PrefabHandler.RegisterPrefab(new SeaMothMk3Prefab(TechTypeID, NameID));
            KnownTechHandler.EditAnalysisTechEntry(seamothDepthMk5, new List<TechType>(1) { TechTypeID }, $"{FriendlyName} blueprint discovered!");
        }

        private static TechData GetRecipe(TechType seamothDepthMk5)
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[5]
                             {
                                 new Ingredient(TechType.PlasteelIngot, 1), // Stronger than titanium ingot                                 
                                 new Ingredient(TechType.EnameledGlass, 2), // Stronger than glass
                                 new Ingredient(TechType.Lead, 1),

                                 new Ingredient(seamothDepthMk5, 1), // Minimum crush depth of 1700 without upgrades
                                 new Ingredient(VehiclePowerCore.TechTypeID, 1), // armor and speed without engine efficiency penalty
                             })
            };
        }

        internal class SeaMothMk3Prefab : ModPrefab
        {
            internal SeaMothMk3Prefab(TechType techtype, string nameID) : base(nameID, $"{nameID}Prefab", techtype)
            {
            }

            public override GameObject GetGameObject()
            {
                GameObject seamothPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth");
                GameObject obj = GameObject.Instantiate(seamothPrefab);

                var seamoth = obj.GetComponent<SeaMoth>();

                var life = seamoth.GetComponent<LiveMixin>();

                LiveMixinData lifeData = (LiveMixinData)ScriptableObject.CreateInstance(typeof(LiveMixinData));

                life.data.CloneFieldsInto(lifeData);
                lifeData.maxHealth = life.maxHealth * 2.25f; // 125% more HP

                life.data = lifeData;
                life.health = life.data.maxHealth;

                // Always on upgrades handled in OnUpgradeModuleChange patch

                return obj;
            }
        }
    }
}
