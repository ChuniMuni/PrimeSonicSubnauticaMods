﻿namespace MoreCyclopsUpgrades.Items.ThermalModule
{
    using CommonCyclopsUpgrades;

    internal class ThermalUpgradeHandler : AmbientEnergyUpgradeHandler
    {
        public ThermalUpgradeHandler(TechType tier1Id, TechType tier2Id, SubRoot cyclops)
            : base(tier1Id, tier2Id, CyclopsThermalChargerMk2.MaxThermalReached(), cyclops)
        {
        }
    }
}
