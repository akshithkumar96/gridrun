using System.Linq;
using GridRun.Core;
using NUnit.Framework;

namespace GridRun.Tests.EditMode
{
    public sealed class BoardResolverTests
    {
        [Test]
        public void FillInitialBoardCreatesNoImmediateMatches()
        {
            BoardState board = new BoardState(8, 8);
            BoardResolver resolver = new BoardResolver();

            resolver.FillInitialBoard(board, new DeterministicRandomSource(1234));

            Assert.That(resolver.FindMatches(board), Is.Empty);
        }

        [Test]
        public void MatchFourCreatesLaserDrill()
        {
            BoardState board = CreatePatternBoard();
            BoardResolver resolver = new BoardResolver();
            board.SetTile(new BoardPosition(0, 0), new TileState(NodeKind.Attack));
            board.SetTile(new BoardPosition(1, 0), new TileState(NodeKind.Attack));
            board.SetTile(new BoardPosition(2, 0), new TileState(NodeKind.Attack));
            board.SetTile(new BoardPosition(3, 0), new TileState(NodeKind.Defense));
            board.SetTile(new BoardPosition(3, 1), new TileState(NodeKind.Attack));

            ResolutionSummary summary = resolver.TryApplyMove(
                board,
                new MoveCommand(new BoardPosition(3, 0), new BoardPosition(3, 1)),
                new DeterministicRandomSource(5));

            bool createdLaser = summary.Steps
                .SelectMany(step => step.CreatedPowerUps)
                .Any(powerUp => powerUp.PowerUp == PowerUpKind.LaserDrill);
            Assert.That(summary.Accepted, Is.True);
            Assert.That(createdLaser, Is.True);
        }

        [Test]
        public void RootAccessKeyPurgesTargetNodeKind()
        {
            BoardState board = CreatePatternBoard();
            BoardResolver resolver = new BoardResolver();
            board.SetTile(new BoardPosition(0, 0), new TileState(NodeKind.Cpu, PowerUpKind.RootAccessKey, PowerUpAxis.None));
            board.SetTile(new BoardPosition(1, 0), new TileState(NodeKind.Attack));
            board.SetTile(new BoardPosition(4, 4), new TileState(NodeKind.Attack));
            board.SetTile(new BoardPosition(6, 6), new TileState(NodeKind.Attack));

            ResolutionSummary summary = resolver.TryApplyMove(
                board,
                new MoveCommand(new BoardPosition(0, 0), new BoardPosition(1, 0)),
                new DeterministicRandomSource(9));

            Assert.That(summary.Accepted, Is.True);
            Assert.That(summary.Count(NodeKind.Attack), Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void HackRunControllerStartsPlayablePrototypeState()
        {
            HackRunController controller = new HackRunController(new DeterministicRandomSource(42));
            HackRunState state = controller.StartDefaultRun();

            Assert.That(state.Board.Width, Is.EqualTo(8));
            Assert.That(state.Board.Height, Is.EqualTo(8));
            Assert.That(state.MovesRemaining, Is.EqualTo(14));
            Assert.That(state.PlayerMaxHp, Is.EqualTo(2000));
            Assert.That(state.Skills.Count(skill => skill.Unlocked), Is.EqualTo(3));
            Assert.That(new BoardResolver().FindMatches(state.Board), Is.Empty);
        }

        private static BoardState CreatePatternBoard()
        {
            BoardState board = new BoardState(8, 8);
            NodeKind[] cycle =
            {
                NodeKind.Attack,
                NodeKind.Defense,
                NodeKind.Cpu,
                NodeKind.Credits
            };

            foreach (BoardPosition position in board.Positions())
            {
                board.SetTile(position, new TileState(cycle[(position.X + position.Y * 2) % cycle.Length]));
            }

            return board;
        }
    }
}
