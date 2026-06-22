namespace GridRun.Services
{
    public interface IProgressStore
    {
        PlayerProfile Load();
        void Save(PlayerProfile profile);
    }
}
