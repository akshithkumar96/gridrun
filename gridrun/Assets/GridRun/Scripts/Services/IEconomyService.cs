namespace GridRun.Services
{
    public interface IEconomyService
    {
        bool TrySpendCredits(PlayerProfile profile, int amount);
        void AddCredits(PlayerProfile profile, int amount);
    }
}
