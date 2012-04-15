using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace KvjGomoku
{
    internal enum MoveResult
    {
        NORMAL = 0,
        OVERLINE = 1,
        DOUBLE_THREES = 2,
        BOARD_FILLED = 3,
        FIVE_IN_A_ROW = 4
    }

    internal struct Lines
    {
        internal int horizontal, vertical, mainDiagonal, antiDiagonal;

        internal Lines(int horizontal, int vertical, int mainDiagonal, int antiDiagonal)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
            this.mainDiagonal = mainDiagonal;
            this.antiDiagonal = antiDiagonal;
        }
    }

    internal static class GameLogic
    {
        private static Lines GetConsecutiveLines(OmokBoard b, PieceColor color, Point move)
        {
            int horizontal = 1, vertical = 1, mainDiagonal = 1, antiDiagonal = 1;
            int nextX, nextY;

            for (nextX = move.X + 1, nextY = move.Y; nextX < OmokBoard.COLUMNS && b.GetPiece(nextY, nextX) == color; nextX++)
                horizontal++;
            for (nextX = move.X - 1; nextX >= 0 && b.GetPiece(nextY, nextX) == color; nextX--)
                horizontal++;

            for (nextX = move.X, nextY = move.Y + 1; nextY < OmokBoard.ROWS && b.GetPiece(nextY, nextX) == color; nextY++)
                vertical++;
            for (nextX = move.X, nextY = move.Y - 1; nextY >= 0 && b.GetPiece(nextY, nextX) == color; nextY--)
                vertical++;

            for (nextX = move.X + 1, nextY = move.Y + 1; nextX < OmokBoard.COLUMNS && nextY < OmokBoard.ROWS && b.GetPiece(nextY, nextX) == color; nextX++, nextY++)
                mainDiagonal++;
            for (nextX = move.X - 1, nextY = move.Y - 1; nextX >= 0 && nextY >= 0 && b.GetPiece(nextY, nextX) == color; nextX--, nextY--)
                mainDiagonal++;

            for (nextX = move.X + 1, nextY = move.Y - 1; nextX < OmokBoard.COLUMNS && nextY >= 0 && b.GetPiece(nextY, nextX) == color; nextX++, nextY--)
                antiDiagonal++;
            for (nextX = move.X - 1, nextY = move.Y + 1; nextX >= 0 && nextY < OmokBoard.ROWS && b.GetPiece(nextY, nextX) == color; nextX--, nextY++)
                antiDiagonal++;

            return new Lines(horizontal, vertical, mainDiagonal, antiDiagonal);
        }

        internal static MoveResult CurrentStatus(OmokBoard b, Player p, Point move)
        {
            Lines sucessive = GetConsecutiveLines(b, p.Color, move);
            if (sucessive.horizontal == 5 || sucessive.vertical == 5 || sucessive.mainDiagonal == 5 || sucessive.antiDiagonal == 5)
                return MoveResult.FIVE_IN_A_ROW;
            for (int i = 0; i < OmokBoard.ROWS; i++)
                for (int j = 0; j < OmokBoard.COLUMNS; j++)
                    if (b.GetPiece(i, j) == PieceColor.EMPTY)
                        return MoveResult.NORMAL;
            return MoveResult.BOARD_FILLED;
        }

        internal static MoveResult CanMove(OmokBoard b, Player p, Point move)
        {
            Lines sucessive = GetConsecutiveLines(b, p.Color, move);
            if (sucessive.horizontal > 5 || sucessive.vertical > 5 || sucessive.mainDiagonal > 5 || sucessive.antiDiagonal > 5)
                return MoveResult.OVERLINE;
            if (sucessive.horizontal == 3 && (sucessive.vertical == 3 || sucessive.mainDiagonal == 3 || sucessive.antiDiagonal == 3)
                    || sucessive.vertical == 3 && (sucessive.mainDiagonal == 3 || sucessive.antiDiagonal == 3)
                    || sucessive.mainDiagonal == 3 && sucessive.antiDiagonal == 3)
                return MoveResult.DOUBLE_THREES;
            return MoveResult.NORMAL;
        }
    }
}
