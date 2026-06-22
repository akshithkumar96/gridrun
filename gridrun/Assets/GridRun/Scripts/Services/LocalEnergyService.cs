namespace GridRun.Services
{
    public sealed class LocalEnergyService : IEnergyService
    {
        public bool TrySpendBattery(PlayerProfile profile, int amount)
        {
            if (profile == null || profile.Battery < amount)
            {
                return false;
            }

            profile.Battery -= amount;
            return true;
        }

        public void Refill(PlayerProfile profile)
        {
            if (profile != null)
            {
                profile.Battery = 5;
            }
        }
    }
}
