namespace GridRun.Core
{
    public readonly struct MoveCommand
    {
        public MoveCommand(BoardPosition from, BoardPosition to)
        {
            From = from;
            To = to;
        }

        public BoardPosition From { get; }
        public BoardPosition To { get; }
    }
}
