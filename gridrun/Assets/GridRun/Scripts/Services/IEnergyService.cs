namespace GridRun.Services
{
    public interface IEnergyService
    {
        bool TrySpendBattery(PlayerProfile profile, int amount);
        void Refill(PlayerProfile profile);
    }
}
