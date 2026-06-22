namespace GridRun.Core
{
    public enum NodeKind
    {
        Attack = 0,
        Defense = 1,
        Cpu = 2,
        Credits = 3,
        Glitch = 4
    }

    public enum PowerUpKind
    {
        None = 0,
        LaserDrill = 1,
        EmpBomb = 2,
        RootAccessKey = 3
    }

    public enum PowerUpAxis
    {
        None = 0,
        Row = 1,
        Column = 2
    }

    public enum ObjectiveKind
    {
        DefeatIce = 0,
        CollectCredits = 1,
        SurviveMoves = 2
    }

    public enum CountermeasureKind
    {
        FreezeRow = 0,
        SpawnGlitches = 1
    }

    public enum SkillTargetingMode
    {
        None = 0,
        Tile = 1
    }

    public enum UpgradeKind
    {
        MemoryBoard = 0,
        Processor = 1,
        SoftwareDeck = 2
    }
}
