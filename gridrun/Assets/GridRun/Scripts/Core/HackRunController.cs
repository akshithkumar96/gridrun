using System.Collections.Generic;
using System.Linq;

namespace GridRun.Core
{
    public sealed class HackRunController
    {
        private const int BoardWidth = 8;
        private const int BoardHeight = 8;
        private const int AttackDamagePerNode = 34;
        private const int DefenseHealPerNode = 24;
        private const int CpuPerNode = 12;
        private const int CreditsPerNode = 30;
        private const int IceStrikeDamage = 70;
        private const int IceTurnInterval = 3;

        private readonly BoardResolver _resolver;
        private readonly IRandomSource _random;

        public HackRunController(IRandomSource random)
        {
            _random = random;
            _resolver = new BoardResolver();
        }

        public HackRunState State { get; private set; }

        public HackRunState StartDefaultRun()
        {
            BoardState board = new BoardState(BoardWidth, BoardHeight);
            _resolver.FillInitialBoard(board, _random);

            State = new HackRunState(board)
            {
                MovesRemaining = 14,
                PlayerMaxHp = 2000,
                PlayerHp = 1450,
                CpuMax = 100,
                Cpu = 85,
                Credits = 3500,
                EnemyMaxHp = 720,
                EnemyHp = 720,
                TurnsUntilIce = IceTurnInterval,
                Status = "Breach the firewall."
            };

            State.Skills.Add(new RuntimeSkillDefinition("brute_force", "Brute Force.exe", 35, SkillTargetingMode.Tile, true));
            State.Skills.Add(new RuntimeSkillDefinition("firewall", "Firewall.scr", 30, SkillTargetingMode.None, true));
            State.Skills.Add(new RuntimeSkillDefinition("decrypt", "Decrypt.data", 45, SkillTargetingMode.None, true));
            State.Skills.Add(new RuntimeSkillDefinition("locked", "Locked slot", 0, SkillTargetingMode.None, false));

            return State;
        }

        public ResolutionSummary TrySwap(BoardPosition from, BoardPosition to)
        {
            if (!CanAct())
            {
                return Blocked("Run already ended.");
            }

            ResolutionSummary summary = _resolver.TryApplyMove(State.Board, new MoveCommand(from, to), _random);
            if (!summary.Accepted)
            {
                State.Status = summary.Message;
                return summary;
            }

            State.MovesRemaining--;
            ApplyResolution(summary);
            EndPlayerTurn(summary);
            return summary;
        }

        public ResolutionSummary ActivateSkill(string skillId, BoardPosition? target)
        {
            if (!CanAct())
            {
                return Blocked("Run already ended.");
            }

            RuntimeSkillDefinition skill = State.Skills.FirstOrDefault(candidate => candidate.Id == skillId);
            if (skill == null || !skill.Unlocked)
            {
                return Blocked("Software slot locked.");
            }

            if (State.Cpu < skill.CpuCost)
            {
                return Blocked("Not enough CPU cycles.");
            }

            if (skill.TargetingMode == SkillTargetingMode.Tile && !target.HasValue)
            {
                return Blocked("Select a target tile.");
            }

            State.Cpu -= skill.CpuCost;
            ResolutionSummary summary;
            switch (skill.Id)
            {
                case "brute_force":
                    summary = _resolver.ClearTile(State.Board, target.Value, _random);
                    summary.Message = "Brute Force.exe destroyed a node.";
                    ApplyResolution(summary);
                    EndPlayerTurn(summary);
                    return summary;

                case "firewall":
                    summary = new ResolutionSummary { Accepted = true, Message = "Firewall.scr restored shielding." };
                    State.PlayerHp = Clamp(State.PlayerHp + 240, 0, State.PlayerMaxHp);
                    State.Status = summary.Message;
                    return summary;

                case "decrypt":
                    summary = _resolver.ClearAllGlitches(State.Board, _random);
                    ApplyResolution(summary);
                    State.Status = summary.Message;
                    return summary;

                default:
                    return Blocked("Unknown software.");
            }
        }

        public bool CanUseSkill(RuntimeSkillDefinition skill)
        {
            return skill != null && skill.Unlocked && State != null && State.Cpu >= skill.CpuCost && CanAct();
        }

        private bool CanAct()
        {
            return State != null && !State.Won && !State.Lost;
        }

        private void EndPlayerTurn(ResolutionSummary summary)
        {
            ResolutionSummary hazardSummary = _resolver.TickHazardsAndLocks(State.Board, _random);
            ApplyResolution(hazardSummary);
            State.PlayerHp = Clamp(State.PlayerHp - hazardSummary.PlayerDamage, 0, State.PlayerMaxHp);

            State.TurnsUntilIce--;
            if (State.TurnsUntilIce <= 0 && !State.Won && !State.Lost)
            {
                RunIceCountermeasure();
                State.TurnsUntilIce = IceTurnInterval;
            }

            CheckEndState();
            if (!State.Won && !State.Lost && !string.IsNullOrEmpty(summary.Message))
            {
                State.Status = summary.Message;
            }
        }

        private void ApplyResolution(ResolutionSummary summary)
        {
            if (summary == null)
            {
                return;
            }

            int attack = summary.Count(NodeKind.Attack);
            int defense = summary.Count(NodeKind.Defense);
            int cpu = summary.Count(NodeKind.Cpu);
            int credits = summary.Count(NodeKind.Credits);

            if (attack > 0)
            {
                State.EnemyHp = Clamp(State.EnemyHp - attack * AttackDamagePerNode, 0, State.EnemyMaxHp);
            }

            if (defense > 0)
            {
                State.PlayerHp = Clamp(State.PlayerHp + defense * DefenseHealPerNode, 0, State.PlayerMaxHp);
            }

            if (cpu > 0)
            {
                State.Cpu = Clamp(State.Cpu + cpu * CpuPerNode, 0, State.CpuMax);
            }

            if (credits > 0)
            {
                State.Credits += credits * CreditsPerNode;
            }
        }

        private void RunIceCountermeasure()
        {
            int roll = _random.Range(0, 2);
            if (roll == 0)
            {
                FreezeRandomRow();
            }
            else
            {
                SpawnGlitches(3);
            }

            State.PlayerHp = Clamp(State.PlayerHp - IceStrikeDamage, 0, State.PlayerMaxHp);
        }

        private void FreezeRandomRow()
        {
            int row = _random.Range(0, State.Board.Height);
            for (int x = 0; x < State.Board.Width; x++)
            {
                TileState tile = State.Board.GetTile(new BoardPosition(x, row));
                if (tile != null)
                {
                    tile.FrozenTurns = 2;
                }
            }

            State.Status = "Black ICE froze row " + (row + 1) + ".";
        }

        private void SpawnGlitches(int count)
        {
            List<BoardPosition> candidates = State.Board.Positions()
                .Where(position =>
                {
                    TileState tile = State.Board.GetTile(position);
                    return tile != null && tile.Kind != NodeKind.Glitch && tile.PowerUp == PowerUpKind.None;
                })
                .ToList();

            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                int index = _random.Range(0, candidates.Count);
                BoardPosition position = candidates[index];
                candidates.RemoveAt(index);
                State.Board.SetTile(position, new TileState(NodeKind.Glitch));
            }

            State.Status = "Black ICE injected glitch nodes.";
        }

        private void CheckEndState()
        {
            if (State.EnemyHp <= 0)
            {
                State.Won = true;
                State.Status = "Firewall breached. Data extracted.";
                return;
            }

            if (State.PlayerHp <= 0)
            {
                State.Lost = true;
                State.Status = "Rig shielding collapsed.";
                return;
            }

            if (State.MovesRemaining <= 0)
            {
                State.Lost = true;
                State.Status = "Trace complete. Server lockdown.";
            }
        }

        private ResolutionSummary Blocked(string message)
        {
            if (State != null)
            {
                State.Status = message;
            }

            return new ResolutionSummary { Message = message };
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
