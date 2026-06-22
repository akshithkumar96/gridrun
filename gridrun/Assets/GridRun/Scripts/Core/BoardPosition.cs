using System;

namespace GridRun.Core
{
    public readonly struct BoardPosition : IEquatable<BoardPosition>
    {
        public readonly int X;
        public readonly int Y;

        public BoardPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool IsAdjacentTo(BoardPosition other)
        {
            int distance = Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
            return distance == 1;
        }

        public bool Equals(BoardPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }

        public static bool operator ==(BoardPosition left, BoardPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoardPosition left, BoardPosition right)
        {
            return !left.Equals(right);
        }
    }
}
