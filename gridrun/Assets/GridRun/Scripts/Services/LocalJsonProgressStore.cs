using System.IO;
using UnityEngine;

namespace GridRun.Services
{
    public sealed class LocalJsonProgressStore : IProgressStore
    {
        private const string FileName = "gridrun_profile.json";

        public PlayerProfile Load()
        {
            string path = GetPath();
            if (!File.Exists(path))
            {
                return new PlayerProfile();
            }

            string json = File.ReadAllText(path);
            PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(json);
            return profile ?? new PlayerProfile();
        }

        public void Save(PlayerProfile profile)
        {
            string json = JsonUtility.ToJson(profile, true);
            File.WriteAllText(GetPath(), json);
        }

        private static string GetPath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }
    }
}
