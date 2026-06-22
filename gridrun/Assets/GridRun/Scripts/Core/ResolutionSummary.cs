using System.Collections.Generic;

namespace GridRun.Core
{
    public sealed class ResolutionSummary
    {
        public ResolutionSummary()
        {
            ClearedCounts = new Dictionary<NodeKind, int>();
            Steps = new List<ResolutionStep>();
            Message = string.Empty;
        }

        public bool Accepted { get; set; }
        public string Message { get; set; }
        public int Cascades { get; set; }
        public Dictionary<NodeKind, int> ClearedCounts { get; }
        public List<ResolutionStep> Steps { get; }
        public int GlitchExplosions { get; set; }
        public int PlayerDamage { get; set; }

        public int TotalCleared
        {
            get
            {
                int total = 0;
                foreach (KeyValuePair<NodeKind, int> pair in ClearedCounts)
                {
                    total += pair.Value;
                }

                return total;
            }
        }

        public void AddCleared(TileState tile, BoardPosition position, ResolutionStep step)
        {
            if (tile == null)
            {
                return;
            }

            int count;
            ClearedCounts.TryGetValue(tile.Kind, out count);
            ClearedCounts[tile.Kind] = count + 1;
            step.ClearedPositions.Add(position);
        }

        public int Count(NodeKind kind)
        {
            int count;
            return ClearedCounts.TryGetValue(kind, out count) ? count : 0;
        }
    }
}
