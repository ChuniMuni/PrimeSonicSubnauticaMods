﻿using Common;

namespace IonCubeGenerator.Craftables
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using UnityEngine;

    internal partial class AlienEletronicsCase : Craftable
    {
        internal static TechType TechTypeID { get; private set; }

        public AlienEletronicsCase() : base("AlienCase", "Energy Condensor", "Used in highly advanced and high powered electrical applications.")
        {
            OnFinishedPatching = () => { TechTypeID = this.TechType; };
        }

        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Fabricator;
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Electronics;
        public override string AssetsFolder { get; } = "IonCubeGenerator/Assets";
        public override string[] StepsToFabricatorTab { get; } = new[] { "Resources", "Electronics" };
        public override TechType RequiredForUnlock { get; } = TechType.PrecursorPrisonIonGenerator;

        public override GameObject GetGameObject()
        {
            var prefab = GameObject.Instantiate(_alienElectronicsCasePrefab);

            GameObject consoleModel = prefab.FindChild("model");

            // Update sky applier
            SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
            skyApplier.renderers = consoleModel.GetComponentsInChildren<MeshRenderer>();
            skyApplier.anchorSky = Skies.Auto;

            // Make the object drop slowly in water
            var wf = prefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 20f;
            QuickLogger.Debug($"Set {ClassID} WaterForces");

            // We can pick this item
            var pickupable = prefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Add fabricating animation
            var fabricatingA = prefab.AddComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 1.0f;

            //Add the prefabIdentifier
            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = this.ClassID;

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.PrecursorIonCrystal, 2),
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.Magnetite, 1),
                    new Ingredient(TechType.Aerogel, 1),
                    new Ingredient(TechType.Diamond, 1),
                }
            };
        }
    }
}
