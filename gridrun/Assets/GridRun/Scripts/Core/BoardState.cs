using System.Collections.Generic;

namespace GridRun.Core
{
    public sealed class BoardState
    {
        private readonly TileState[,] _tiles;

        public BoardState(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new TileState[width, height];
        }

        public int Width { get; }
        public int Height { get; }

        public bool Contains(BoardPosition position)
        {
            return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
        }

        public TileState GetTile(BoardPosition position)
        {
            return Contains(position) ? _tiles[position.X, position.Y] : null;
        }

        public void SetTile(BoardPosition position, TileState tile)
        {
            if (!Contains(position))
            {
                return;
            }

            _tiles[position.X, position.Y] = tile;
        }

        public void Swap(BoardPosition a, BoardPosition b)
        {
            TileState tileA = GetTile(a);
            SetTile(a, GetTile(b));
            SetTile(b, tileA);
        }

        public IEnumerable<BoardPosition> Positions()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return new BoardPosition(x, y);
                }
            }
        }

        public BoardState Clone()
        {
            BoardState clone = new BoardState(Width, Height);
            foreach (BoardPosition position in Positions())
            {
                TileState tile = GetTile(position);
                clone.SetTile(position, tile == null ? null : tile.Clone());
            }

            return clone;
        }
    }
}
