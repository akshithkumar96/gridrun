namespace GridRun.Core
{
    public sealed class TileState
    {
        public TileState(NodeKind kind)
            : this(kind, PowerUpKind.None, PowerUpAxis.None)
        {
        }

        public TileState(NodeKind kind, PowerUpKind powerUp, PowerUpAxis axis)
        {
            Kind = kind;
            PowerUp = powerUp;
            Axis = axis;
            GlitchTurns = kind == NodeKind.Glitch ? BoardResolver.DefaultGlitchTurns : 0;
        }

        public NodeKind Kind { get; set; }
        public PowerUpKind PowerUp { get; set; }
        public PowerUpAxis Axis { get; set; }
        public int GlitchTurns { get; set; }
        public int FrozenTurns { get; set; }

        public bool IsFrozen
        {
            get { return FrozenTurns > 0; }
        }

        public TileState Clone()
        {
            return new TileState(Kind, PowerUp, Axis)
            {
                GlitchTurns = GlitchTurns,
                FrozenTurns = FrozenTurns
            };
        }
    }
}
