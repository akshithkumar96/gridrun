using System.Collections.Generic;

namespace GridRun.Core
{
    public sealed class HackRunState
    {
        public HackRunState(BoardState board)
        {
            Board = board;
            Skills = new List<RuntimeSkillDefinition>();
            Status = "Infiltration started.";
        }

        public BoardState Board { get; }
        public int MovesRemaining { get; set; }
        public int PlayerHp { get; set; }
        public int PlayerMaxHp { get; set; }
        public int Cpu { get; set; }
        public int CpuMax { get; set; }
        public int Credits { get; set; }
        public int EnemyHp { get; set; }
        public int EnemyMaxHp { get; set; }
        public int TurnsUntilIce { get; set; }
        public bool Won { get; set; }
        public bool Lost { get; set; }
        public string Status { get; set; }
        public List<RuntimeSkillDefinition> Skills { get; }
    }
}
