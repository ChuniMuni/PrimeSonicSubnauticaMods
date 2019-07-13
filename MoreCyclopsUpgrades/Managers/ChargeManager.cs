﻿namespace MoreCyclopsUpgrades.Managers
{
    using System.Collections.Generic;
    using Common;
    using MoreCyclopsUpgrades.API;
    using MoreCyclopsUpgrades.API.Charging;
    using MoreCyclopsUpgrades.Config;
    using UnityEngine;

    internal class ChargeManager
    {
        private class ChargerCreator
        {
            public readonly CreateCyclopsCharger Creator;
            public readonly string ChargerName;

            public ChargerCreator(CreateCyclopsCharger creator, string chargerName)
            {
                Creator = creator;
                ChargerName = chargerName;
            }
        }

        #region Static

        internal static bool TooLateToRegister { get; private set; }
        internal const float MinimalPowerValue = MCUServices.MinimalPowerValue;

        private static readonly List<ChargerCreator> CyclopsChargerCreators = new List<ChargerCreator>();

        internal static void RegisterChargerCreator(CreateCyclopsCharger createEvent, string name)
        {
            if (CyclopsChargerCreators.Find(c => c.Creator == createEvent || c.ChargerName == name) != null)
            {
                QuickLogger.Warning($"Duplicate CyclopsChargerCreator '{name}' was blocked");
                return;
            }

            QuickLogger.Info($"Received CyclopsChargerCreator '{name}'");
            CyclopsChargerCreators.Add(new ChargerCreator(createEvent, name));
        }

        #endregion

        private readonly IDictionary<string, CyclopsCharger> KnownChargers = new Dictionary<string, CyclopsCharger>();
        private readonly SubRoot Cyclops;

        public bool Initialized { get; private set; } = false;
        private float rechargePenalty = ModConfig.Main.RechargePenalty;
        private readonly IModConfig config = ModConfig.Main;
        private bool requiresVanillaCharging = false;
        private float producedPower = 0f;
        private float powerDeficit = 0f;

        public CyclopsCharger[] Chargers { get; private set; }

        public ChargeManager(SubRoot cyclops)
        {
            QuickLogger.Debug("Creating new ChargeManager");
            Cyclops = cyclops;
        }

        internal T GetCharger<T>(string chargeHandlerName) where T : CyclopsCharger
        {
            if (KnownChargers.TryGetValue(chargeHandlerName, out CyclopsCharger cyclopsCharger))
            {
                return (T)cyclopsCharger;
            }

            return null;
        }

        private void InitializeChargers()
        {
            QuickLogger.Debug("ChargeManager Initialize CyclopsChargers from external mods");

            // First, register chargers from other mods.
            for (int i = 0; i < CyclopsChargerCreators.Count; i++)
            {
                ChargerCreator chargerTemplate = CyclopsChargerCreators[i];
                QuickLogger.Debug($"ChargeManager creating charger '{chargerTemplate.ChargerName}'");
                CyclopsCharger charger = chargerTemplate.Creator.Invoke(Cyclops);

                if (charger == null)
                {
                    QuickLogger.Warning($"CyclopsCharger '{chargerTemplate.ChargerName}' was null");
                }
                else if (!KnownChargers.ContainsKey(chargerTemplate.ChargerName))
                {
                    KnownChargers.Add(chargerTemplate.ChargerName, charger);
                    QuickLogger.Debug($"Created CyclopsCharger '{chargerTemplate.ChargerName}'");
                }
                else
                {
                    QuickLogger.Warning($"Duplicate CyclopsCharger '{chargerTemplate.ChargerName}' was blocked");
                }
            }

            this.Chargers = new CyclopsCharger[KnownChargers.Count];

            int c = 0;
            foreach (CyclopsCharger charger in KnownChargers.Values)
                this.Chargers[c++] = charger;

            // Next, check if an external mod has a different upgrade handler for the original CyclopsThermalReactorModule.
            // If not, then the original thermal charging code will be allowed to run.
            // This is to allow players to choose whether or not they want the newer form of charging.
            requiresVanillaCharging = VanillaUpgrades.Main.IsUsingVanillaUpgrade(TechType.CyclopsThermalReactorModule);

            this.Initialized = true;
            TooLateToRegister = true;
        }

        internal void UpdateRechargePenalty(float penalty)
        {
            rechargePenalty = penalty;
        }

        /// <summary>
        /// Gets the total available reserve power across all equipment upgrade modules.
        /// </summary>
        /// <returns>The <see cref="int"/> value of the total available reserve power.</returns>
        public int GetTotalReservePower()
        {
            if (!this.Initialized)
                InitializeChargers();

            float availableReservePower = 0f;

            for (int i = 0; i < this.Chargers.Length; i++)
                availableReservePower += this.Chargers[i].TotalReserveEnergy;

            return Mathf.FloorToInt(availableReservePower);
        }

        /// <summary>
        /// Recharges the cyclops' power cells using all charging modules across all upgrade consoles.
        /// </summary>
        /// <returns><c>True</c> if the original code for the vanilla Cyclops Thermal Reactor Module is required; Otherwise <c>false</c>.</returns>
        public bool RechargeCyclops()
        {
            if (!this.Initialized)
                InitializeChargers();

            if (Time.timeScale == 0f) // Is the game paused?
                return false;

            if (!TooLateToRegister)
                return false;

            if (this.Chargers == null)
                return false;

            // When in Creative mode or using the NoPower cheat, inform the chargers that there is no power deficit.
            // This is so that each charger can decide what to do individually rather than skip the entire charging cycle all together.
            powerDeficit = GameModeUtils.RequiresPower()
                            ? Cyclops.powerRelay.GetMaxPower() - Cyclops.powerRelay.GetPower()
                            : 0f;

            producedPower = 0f;

            // First, get renewable energy first
            for (int i = 0; i < this.Chargers.Length; i++)
            {
                CyclopsCharger charger = this.Chargers[i];
                producedPower += charger.Generate(powerDeficit);
            }

            // Second, get non-renewable energy if no renewable energy was available
            if (producedPower < MinimalPowerValue && // Did the renewable energy sources not produce any power?
                powerDeficit > config.MinimumEnergyDeficit) // Is the power deficit over the threshhold to start consuming non-renewable energy?
            {
                for (int i = 0; i < this.Chargers.Length; i++)
                {
                    CyclopsCharger charger = this.Chargers[i];
                    producedPower += charger.Drain(powerDeficit);
                }
            }

            if (producedPower > 0f)
            {
                Cyclops.powerRelay.ModifyPower(producedPower * rechargePenalty, out float amountStored);
            }

            // Last, inform the chargers to update their display status
            for (int i = 0; i < this.Chargers.Length; i++)
            {
                CyclopsCharger charger = this.Chargers[i];
                charger.UpdateStatus();
            }

            return requiresVanillaCharging;
        }
    }
}
