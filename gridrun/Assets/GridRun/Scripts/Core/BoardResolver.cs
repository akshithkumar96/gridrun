using System.Collections.Generic;
using System.Linq;

namespace GridRun.Core
{
    public sealed class BoardResolver
    {
        public const int DefaultGlitchTurns = 4;

        private static readonly NodeKind[] FillKinds =
        {
            NodeKind.Attack,
            NodeKind.Defense,
            NodeKind.Cpu,
            NodeKind.Credits,
            NodeKind.Glitch
        };

        private static readonly int[] FillWeights = { 28, 22, 22, 18, 10 };

        public void FillInitialBoard(BoardState board, IRandomSource random)
        {
            foreach (BoardPosition position in board.Positions())
            {
                NodeKind kind = ChooseKindAvoidingInitialMatch(board, position, random);
                board.SetTile(position, CreateFillTile(kind));
            }
        }

        public bool HasMatches(BoardState board)
        {
            return FindMatches(board).Count > 0;
        }

        public ResolutionSummary TryApplyMove(BoardState board, MoveCommand command, IRandomSource random)
        {
            ResolutionSummary summary = new ResolutionSummary();

            if (!board.Contains(command.From) || !board.Contains(command.To))
            {
                summary.Message = "Swap outside grid.";
                return summary;
            }

            if (!command.From.IsAdjacentTo(command.To))
            {
                summary.Message = "Tiles must be adjacent.";
                return summary;
            }

            TileState fromTile = board.GetTile(command.From);
            TileState toTile = board.GetTile(command.To);
            if (fromTile == null || toTile == null)
            {
                summary.Message = "Missing tile.";
                return summary;
            }

            if (fromTile.IsFrozen || toTile.IsFrozen)
            {
                summary.Message = "Frozen tiles are locked by ICE.";
                return summary;
            }

            if (fromTile.PowerUp == PowerUpKind.RootAccessKey || toTile.PowerUp == PowerUpKind.RootAccessKey)
            {
                BoardPosition rootPosition = fromTile.PowerUp == PowerUpKind.RootAccessKey ? command.From : command.To;
                BoardPosition targetPosition = rootPosition == command.From ? command.To : command.From;
                TileState target = board.GetTile(targetPosition);
                if (target == null)
                {
                    return summary;
                }

                summary.Accepted = true;
                summary.Message = "Root Access purge.";
                ClearAllOfKind(board, target.Kind, summary, rootPosition);
                CollapseAndRefill(board, random);
                ResolveBoard(board, random, null, summary);
                return summary;
            }

            board.Swap(command.From, command.To);
            List<MatchGroup> matches = FindMatches(board);
            if (matches.Count == 0)
            {
                board.Swap(command.From, command.To);
                summary.Message = "No network command found.";
                return summary;
            }

            summary.Accepted = true;
            ResolveBoard(board, random, command.To, summary);
            if (summary.TotalCleared >= 5)
            {
                summary.Message = "Combo chain online.";
            }
            else
            {
                summary.Message = "Network command executed.";
            }

            return summary;
        }

        public ResolutionSummary ClearTile(BoardState board, BoardPosition position, IRandomSource random)
        {
            ResolutionSummary summary = new ResolutionSummary { Accepted = true, Message = "Tile destroyed." };
            HashSet<BoardPosition> clearSet = new HashSet<BoardPosition>();
            AddPowerUpClearArea(board, position, clearSet);
            clearSet.Add(position);
            ClearPositions(board, clearSet, summary, null);
            CollapseAndRefill(board, random);
            ResolveBoard(board, random, null, summary);
            return summary;
        }

        public ResolutionSummary ClearAllGlitches(BoardState board, IRandomSource random)
        {
            ResolutionSummary summary = new ResolutionSummary { Accepted = true, Message = "Glitch nodes decrypted." };
            HashSet<BoardPosition> clearSet = new HashSet<BoardPosition>();
            foreach (BoardPosition position in board.Positions())
            {
                TileState tile = board.GetTile(position);
                if (tile != null && tile.Kind == NodeKind.Glitch)
                {
                    clearSet.Add(position);
                }
            }

            ClearPositions(board, clearSet, summary, null);
            CollapseAndRefill(board, random);
            ResolveBoard(board, random, null, summary);
            return summary;
        }

        public ResolutionSummary TickHazardsAndLocks(BoardState board, IRandomSource random)
        {
            ResolutionSummary summary = new ResolutionSummary { Accepted = true };
            HashSet<BoardPosition> explosionSet = new HashSet<BoardPosition>();

            foreach (BoardPosition position in board.Positions())
            {
                TileState tile = board.GetTile(position);
                if (tile == null)
                {
                    continue;
                }

                if (tile.FrozenTurns > 0)
                {
                    tile.FrozenTurns--;
                }

                if (tile.Kind != NodeKind.Glitch)
                {
                    continue;
                }

                tile.GlitchTurns--;
                if (tile.GlitchTurns <= 0)
                {
                    summary.GlitchExplosions++;
                    explosionSet.Add(position);
                    foreach (BoardPosition adjacent in AdjacentAndSelf(board, position))
                    {
                        explosionSet.Add(adjacent);
                    }
                }
            }

            if (explosionSet.Count == 0)
            {
                return summary;
            }

            summary.Message = "Glitch payload detonated.";
            summary.PlayerDamage += summary.GlitchExplosions * 25;
            ClearPositionsWithoutRewards(board, explosionSet, summary);
            CollapseAndRefill(board, random);
            ResolveBoard(board, random, null, summary);
            return summary;
        }

        public List<MatchGroup> FindMatches(BoardState board)
        {
            List<HashSet<BoardPosition>> rawGroups = new List<HashSet<BoardPosition>>();

            for (int y = 0; y < board.Height; y++)
            {
                int x = 0;
                while (x < board.Width)
                {
                    BoardPosition start = new BoardPosition(x, y);
                    TileState startTile = board.GetTile(start);
                    if (startTile == null)
                    {
                        x++;
                        continue;
                    }

                    int end = x + 1;
                    while (end < board.Width && SameMatchKind(startTile, board.GetTile(new BoardPosition(end, y))))
                    {
                        end++;
                    }

                    if (end - x >= 3)
                    {
                        HashSet<BoardPosition> group = new HashSet<BoardPosition>();
                        for (int matchX = x; matchX < end; matchX++)
                        {
                            group.Add(new BoardPosition(matchX, y));
                        }

                        rawGroups.Add(group);
                    }

                    x = end;
                }
            }

            for (int x = 0; x < board.Width; x++)
            {
                int y = 0;
                while (y < board.Height)
                {
                    BoardPosition start = new BoardPosition(x, y);
                    TileState startTile = board.GetTile(start);
                    if (startTile == null)
                    {
                        y++;
                        continue;
                    }

                    int end = y + 1;
                    while (end < board.Height && SameMatchKind(startTile, board.GetTile(new BoardPosition(x, end))))
                    {
                        end++;
                    }

                    if (end - y >= 3)
                    {
                        HashSet<BoardPosition> group = new HashSet<BoardPosition>();
                        for (int matchY = y; matchY < end; matchY++)
                        {
                            group.Add(new BoardPosition(x, matchY));
                        }

                        rawGroups.Add(group);
                    }

                    y = end;
                }
            }

            MergeIntersectingGroups(rawGroups);

            List<MatchGroup> matches = new List<MatchGroup>();
            foreach (HashSet<BoardPosition> group in rawGroups)
            {
                BoardPosition first = group.First();
                TileState tile = board.GetTile(first);
                if (tile != null)
                {
                    matches.Add(new MatchGroup(tile.Kind, group));
                }
            }

            return matches;
        }

        private void ResolveBoard(BoardState board, IRandomSource random, BoardPosition? preferredPowerUpCell, ResolutionSummary summary)
        {
            int cascade = 0;
            while (true)
            {
                List<MatchGroup> matches = FindMatches(board);
                if (matches.Count == 0)
                {
                    break;
                }

                ResolutionStep step = new ResolutionStep(cascade);
                summary.Steps.Add(step);
                ResolveMatchSet(board, matches, preferredPowerUpCell, summary, step);
                CollapseAndRefill(board, random);
                cascade++;
                preferredPowerUpCell = null;

                if (cascade > 32)
                {
                    summary.Message = "Cascade limit reached.";
                    break;
                }
            }

            summary.Cascades += cascade;
        }

        private void ResolveMatchSet(BoardState board, List<MatchGroup> matches, BoardPosition? preferredPowerUpCell, ResolutionSummary summary, ResolutionStep step)
        {
            HashSet<BoardPosition> clearSet = new HashSet<BoardPosition>();
            Dictionary<BoardPosition, CreatedPowerUp> created = new Dictionary<BoardPosition, CreatedPowerUp>();

            foreach (MatchGroup group in matches)
            {
                BoardPosition? creationPosition = ChoosePowerUpCreationPosition(group, preferredPowerUpCell);
                CreatedPowerUp createdPowerUp = CreatePowerUpForGroup(group, creationPosition);
                if (createdPowerUp.PowerUp != PowerUpKind.None)
                {
                    created[createdPowerUp.Position] = createdPowerUp;
                    step.CreatedPowerUps.Add(createdPowerUp);
                }

                foreach (BoardPosition position in group.Cells)
                {
                    TileState tile = board.GetTile(position);
                    if (tile != null && tile.PowerUp != PowerUpKind.None)
                    {
                        AddPowerUpClearArea(board, position, clearSet);
                    }

                    clearSet.Add(position);
                }
            }

            foreach (BoardPosition creationPosition in created.Keys.ToList())
            {
                clearSet.Remove(creationPosition);
            }

            ClearPositions(board, clearSet, summary, step);

            foreach (CreatedPowerUp powerUp in created.Values)
            {
                TileState tile = new TileState(powerUp.Kind, powerUp.PowerUp, powerUp.Axis);
                board.SetTile(powerUp.Position, tile);
            }
        }

        private BoardPosition? ChoosePowerUpCreationPosition(MatchGroup group, BoardPosition? preferredPowerUpCell)
        {
            if (group.Cells.Count < 4)
            {
                return null;
            }

            if (preferredPowerUpCell.HasValue && group.Cells.Contains(preferredPowerUpCell.Value))
            {
                return preferredPowerUpCell.Value;
            }

            return group.Cells[group.Cells.Count / 2];
        }

        private CreatedPowerUp CreatePowerUpForGroup(MatchGroup group, BoardPosition? creationPosition)
        {
            if (!creationPosition.HasValue)
            {
                return new CreatedPowerUp(new BoardPosition(-1, -1), group.Kind, PowerUpKind.None, PowerUpAxis.None);
            }

            if (group.Cells.Count >= 5 && group.IsStraight)
            {
                return new CreatedPowerUp(creationPosition.Value, group.Kind, PowerUpKind.RootAccessKey, PowerUpAxis.None);
            }

            if (group.Cells.Count >= 5)
            {
                return new CreatedPowerUp(creationPosition.Value, group.Kind, PowerUpKind.EmpBomb, PowerUpAxis.None);
            }

            if (group.Cells.Count >= 4)
            {
                return new CreatedPowerUp(creationPosition.Value, group.Kind, PowerUpKind.LaserDrill, group.Axis);
            }

            return new CreatedPowerUp(creationPosition.Value, group.Kind, PowerUpKind.None, PowerUpAxis.None);
        }

        private void ClearAllOfKind(BoardState board, NodeKind kind, ResolutionSummary summary, BoardPosition rootPosition)
        {
            HashSet<BoardPosition> clearSet = new HashSet<BoardPosition>();
            foreach (BoardPosition position in board.Positions())
            {
                TileState tile = board.GetTile(position);
                if (tile != null && tile.Kind == kind)
                {
                    clearSet.Add(position);
                }
            }

            clearSet.Add(rootPosition);
            ClearPositions(board, clearSet, summary, null);
        }

        private void ClearPositions(BoardState board, HashSet<BoardPosition> clearSet, ResolutionSummary summary, ResolutionStep step)
        {
            ResolutionStep targetStep = step;
            if (targetStep == null)
            {
                targetStep = new ResolutionStep(summary.Steps.Count);
                summary.Steps.Add(targetStep);
            }

            foreach (BoardPosition position in clearSet)
            {
                TileState tile = board.GetTile(position);
                if (tile == null)
                {
                    continue;
                }

                summary.AddCleared(tile, position, targetStep);
                board.SetTile(position, null);
            }
        }

        private void ClearPositionsWithoutRewards(BoardState board, HashSet<BoardPosition> clearSet, ResolutionSummary summary)
        {
            ResolutionStep targetStep = new ResolutionStep(summary.Steps.Count);
            summary.Steps.Add(targetStep);

            foreach (BoardPosition position in clearSet)
            {
                if (board.GetTile(position) == null)
                {
                    continue;
                }

                targetStep.ClearedPositions.Add(position);
                board.SetTile(position, null);
            }
        }

        private void AddPowerUpClearArea(BoardState board, BoardPosition position, HashSet<BoardPosition> clearSet)
        {
            TileState tile = board.GetTile(position);
            if (tile == null)
            {
                return;
            }

            switch (tile.PowerUp)
            {
                case PowerUpKind.LaserDrill:
                    if (tile.Axis == PowerUpAxis.Column)
                    {
                        for (int y = 0; y < board.Height; y++)
                        {
                            clearSet.Add(new BoardPosition(position.X, y));
                        }
                    }
                    else
                    {
                        for (int x = 0; x < board.Width; x++)
                        {
                            clearSet.Add(new BoardPosition(x, position.Y));
                        }
                    }

                    break;

                case PowerUpKind.EmpBomb:
                    for (int y = position.Y - 1; y <= position.Y + 1; y++)
                    {
                        for (int x = position.X - 1; x <= position.X + 1; x++)
                        {
                            BoardPosition nearby = new BoardPosition(x, y);
                            if (board.Contains(nearby))
                            {
                                clearSet.Add(nearby);
                            }
                        }
                    }

                    break;

                case PowerUpKind.RootAccessKey:
                    foreach (BoardPosition candidate in board.Positions())
                    {
                        TileState candidateTile = board.GetTile(candidate);
                        if (candidateTile != null && candidateTile.Kind == tile.Kind)
                        {
                            clearSet.Add(candidate);
                        }
                    }

                    break;
            }
        }

        private void CollapseAndRefill(BoardState board, IRandomSource random)
        {
            for (int x = 0; x < board.Width; x++)
            {
                List<TileState> column = new List<TileState>();
                for (int y = board.Height - 1; y >= 0; y--)
                {
                    TileState tile = board.GetTile(new BoardPosition(x, y));
                    if (tile != null)
                    {
                        column.Add(tile);
                    }
                }

                int writeIndex = 0;
                for (int y = board.Height - 1; y >= 0; y--)
                {
                    TileState next = writeIndex < column.Count ? column[writeIndex] : CreateFillTile(ChooseWeightedKind(random));
                    board.SetTile(new BoardPosition(x, y), next);
                    writeIndex++;
                }
            }
        }

        private IEnumerable<BoardPosition> AdjacentAndSelf(BoardState board, BoardPosition position)
        {
            int[] offsets = { -1, 0, 1 };
            for (int y = 0; y < offsets.Length; y++)
            {
                for (int x = 0; x < offsets.Length; x++)
                {
                    BoardPosition candidate = new BoardPosition(position.X + offsets[x], position.Y + offsets[y]);
                    if (board.Contains(candidate))
                    {
                        yield return candidate;
                    }
                }
            }
        }

        private static void MergeIntersectingGroups(List<HashSet<BoardPosition>> groups)
        {
            bool changed;
            do
            {
                changed = false;
                for (int a = 0; a < groups.Count; a++)
                {
                    for (int b = a + 1; b < groups.Count; b++)
                    {
                        if (!groups[a].Overlaps(groups[b]))
                        {
                            continue;
                        }

                        groups[a].UnionWith(groups[b]);
                        groups.RemoveAt(b);
                        changed = true;
                        break;
                    }

                    if (changed)
                    {
                        break;
                    }
                }
            }
            while (changed);
        }

        private static bool SameMatchKind(TileState a, TileState b)
        {
            return a != null && b != null && a.Kind == b.Kind;
        }

        private NodeKind ChooseKindAvoidingInitialMatch(BoardState board, BoardPosition position, IRandomSource random)
        {
            for (int attempt = 0; attempt < 32; attempt++)
            {
                NodeKind kind = ChooseWeightedKind(random);
                if (!WouldCreateInitialMatch(board, position, kind))
                {
                    return kind;
                }
            }

            foreach (NodeKind kind in FillKinds)
            {
                if (!WouldCreateInitialMatch(board, position, kind))
                {
                    return kind;
                }
            }

            return NodeKind.Attack;
        }

        private static bool WouldCreateInitialMatch(BoardState board, BoardPosition position, NodeKind kind)
        {
            if (position.X >= 2)
            {
                TileState left1 = board.GetTile(new BoardPosition(position.X - 1, position.Y));
                TileState left2 = board.GetTile(new BoardPosition(position.X - 2, position.Y));
                if (left1 != null && left2 != null && left1.Kind == kind && left2.Kind == kind)
                {
                    return true;
                }
            }

            if (position.Y >= 2)
            {
                TileState up1 = board.GetTile(new BoardPosition(position.X, position.Y - 1));
                TileState up2 = board.GetTile(new BoardPosition(position.X, position.Y - 2));
                if (up1 != null && up2 != null && up1.Kind == kind && up2.Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        private static NodeKind ChooseWeightedKind(IRandomSource random)
        {
            int total = 0;
            for (int i = 0; i < FillWeights.Length; i++)
            {
                total += FillWeights[i];
            }

            int roll = random.Range(0, total);
            int running = 0;
            for (int i = 0; i < FillWeights.Length; i++)
            {
                running += FillWeights[i];
                if (roll < running)
                {
                    return FillKinds[i];
                }
            }

            return NodeKind.Attack;
        }

        private static TileState CreateFillTile(NodeKind kind)
        {
            TileState tile = new TileState(kind);
            if (kind == NodeKind.Glitch)
            {
                tile.GlitchTurns = DefaultGlitchTurns;
            }

            return tile;
        }
    }
}
