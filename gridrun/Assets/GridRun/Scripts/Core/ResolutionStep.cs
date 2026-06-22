using System.Collections.Generic;

namespace GridRun.Core
{
    public sealed class ResolutionStep
    {
        public ResolutionStep(int cascadeIndex)
        {
            CascadeIndex = cascadeIndex;
            ClearedPositions = new List<BoardPosition>();
            CreatedPowerUps = new List<CreatedPowerUp>();
        }

        public int CascadeIndex { get; }
        public List<BoardPosition> ClearedPositions { get; }
        public List<CreatedPowerUp> CreatedPowerUps { get; }
    }

    public readonly struct CreatedPowerUp
    {
        public CreatedPowerUp(BoardPosition position, NodeKind kind, PowerUpKind powerUp, PowerUpAxis axis)
        {
            Position = position;
            Kind = kind;
            PowerUp = powerUp;
            Axis = axis;
        }

        public BoardPosition Position { get; }
        public NodeKind Kind { get; }
        public PowerUpKind PowerUp { get; }
        public PowerUpAxis Axis { get; }
    }
}
