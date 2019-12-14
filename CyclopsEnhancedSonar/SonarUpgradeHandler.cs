﻿namespace CyclopsEnhancedSonar
{
    using MoreCyclopsUpgrades.API.Upgrades;

    internal class SonarUpgradeHandler : UpgradeHandler
    {
        public const int MaxUpgrades = 2;

        private CySonarComponent cySonar;
        private CySonarComponent CySonar => cySonar ?? (cySonar = base.Cyclops.GetComponentInChildren<SubControl>()?.gameObject.GetComponent<CySonarComponent>());

        private CyclopsSonarButton cyButton;
        private CyclopsSonarButton CyButton => cyButton ?? (cyButton = base.Cyclops.GetComponentInChildren<CyclopsSonarButton>());

        public SonarUpgradeHandler(SubRoot cyclops)
            : base(TechType.CyclopsSonarModule, cyclops)
        {
            this.MaxCount = MaxUpgrades;
            OnFinishedUpgrades = () =>
            {
                bool hasUpgrade = base.HasUpgrade;

                // Vanilla behavior that toggles the sonar button
                base.Cyclops.sonarUpgrade = hasUpgrade;

                // New behavior that toggles the near field sonar map
                this.CySonar?.SetMapState(hasUpgrade);

                if (hasUpgrade)
                {
                    float pinginterval = 6.1f - 1.1f * this.Count;
                    this.CyButton.pingIterationDuration = pinginterval;
                }
            };
        }
    }
}
