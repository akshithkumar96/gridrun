namespace GridRun.Services
{
    public sealed class LocalPurchaseService : IPurchaseService
    {
        public bool IsPurchaseAvailable
        {
            get { return false; }
        }

        public bool HasOwnedPack(PlayerProfile profile, string packId)
        {
            return profile != null && profile.OwnedPremiumPacks.Contains(packId);
        }

        public void GrantPack(PlayerProfile profile, string packId)
        {
            if (profile != null && !profile.OwnedPremiumPacks.Contains(packId))
            {
                profile.OwnedPremiumPacks.Add(packId);
            }
        }
    }
}
