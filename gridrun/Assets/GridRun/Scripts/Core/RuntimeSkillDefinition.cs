namespace GridRun.Core
{
    public sealed class RuntimeSkillDefinition
    {
        public RuntimeSkillDefinition(string id, string displayName, int cpuCost, SkillTargetingMode targetingMode, bool unlocked)
        {
            Id = id;
            DisplayName = displayName;
            CpuCost = cpuCost;
            TargetingMode = targetingMode;
            Unlocked = unlocked;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int CpuCost { get; }
        public SkillTargetingMode TargetingMode { get; }
        public bool Unlocked { get; }
    }
}
