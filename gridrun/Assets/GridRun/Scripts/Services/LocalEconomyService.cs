namespace GridRun.Services
{
    public sealed class LocalEconomyService : IEconomyService
    {
        public bool TrySpendCredits(PlayerProfile profile, int amount)
        {
            if (profile == null || profile.Credits < amount)
            {
                return false;
            }

            profile.Credits -= amount;
            return true;
        }

        public void AddCredits(PlayerProfile profile, int amount)
        {
            if (profile != null)
            {
                profile.Credits += amount;
            }
        }
    }
}
