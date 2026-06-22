namespace GridRun.Services
{
    public interface IPurchaseService
    {
        bool IsPurchaseAvailable { get; }
        bool HasOwnedPack(PlayerProfile profile, string packId);
        void GrantPack(PlayerProfile profile, string packId);
    }
}
