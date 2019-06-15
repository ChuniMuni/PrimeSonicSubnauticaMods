﻿namespace MoreCyclopsUpgrades.CyclopsUpgrades
{
    using MoreCyclopsUpgrades.API;

    internal class ChargingUpgradeHandler : UpgradeHandler
    {
        internal ChargingUpgradeHandler SiblingUpgrade = null;

        public ChargingUpgradeHandler(TechType techType, SubRoot cyclops) : base(techType, cyclops)
        {
            IsAllowedToAdd = (Pickupable item, bool verbose) =>
            {
                if (SiblingUpgrade == null)
                    return this.Count < this.MaxCount;

                return (SiblingUpgrade.Count + this.Count) < this.MaxCount;
            };
        }
    }
}
