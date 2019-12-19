﻿namespace UpgradedVehicles
{
    using System;
    using System.Reflection;
    using Common;
    using Harmony;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Handlers;
    using UpgradedVehicles.SaveData;

    public class QPatch
    {
        private static UpgradeOptions configOptions;
        private static Craftable speedModule;

        public static void Patch()
        {
            try
            {
                QuickLogger.Message("Started patching - " + QuickLogger.GetAssemblyVersion());

                //Handle CrossMod Updates
                if (TechTypeHandler.TryGetModdedTechType("SeamothHullModule4", out TechType vehicleHullModule4) &&
                    TechTypeHandler.TryGetModdedTechType("SeamothHullModule5", out TechType vehicleHullModule5))
                {
                    VehicleUpgrader.SetModdedDepthModules(vehicleHullModule4, vehicleHullModule5);
                }
                //Handle SpeedBooster
                speedModule = SpeedBooster.GetSpeedBoosterCraftable();

                VehicleUpgrader.SetSpeedBooster(speedModule);

                //Handle Config Options
                configOptions = new UpgradeOptions();
                configOptions.Initialize();

                VehicleUpgrader.SetBonusSpeedMultipliers(configOptions.SeamothBonusSpeedMultiplier, configOptions.ExosuitBonusSpeedMultiplier);

                var harmony = HarmonyInstance.Create("com.upgradedvehicles.psmod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Message("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error("EXCEPTION on Patch: " + ex.ToString());
            }
        }
    }
}
