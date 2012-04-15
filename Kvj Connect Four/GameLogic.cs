using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KvjConnectFour
{
    internal enum MoveResult
    {
        NORMAL = 0,
        BOARD_FILLED = 1,
        FOUR_IN_A_ROW = 2
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
        private static Lines GetConsecutiveLines(Connect4Board b, PieceColor color, int col, Connect4Column move)
        {
            int horizontal = 1, vertical = 1, mainDiagonal = 1, antiDiagonal = 1;
            int nextX, nextY;

            for (nextX = col + 1, nextY = move.LastDropRow; nextX < Connect4Board.COLUMNS && b.GetPiece(nextY, nextX) == color; nextX++)
                horizontal++;
            for (nextX = col - 1; nextX >= 0 && b.GetPiece(nextY, nextX) == color; nextX--)
                horizontal++;

            for (nextX = col, nextY = move.LastDropRow + 1; nextY < Connect4Board.ROWS && b.GetPiece(nextY, nextX) == color; nextY++)
                vertical++;
            for (nextX = col, nextY = move.LastDropRow - 1; nextY >= 0 && b.GetPiece(nextY, nextX) == color; nextY--)
                vertical++;

            for (nextX = col + 1, nextY = move.LastDropRow + 1; nextX < Connect4Board.COLUMNS && nextY < Connect4Board.ROWS && b.GetPiece(nextY, nextX) == color; nextX++, nextY++)
                mainDiagonal++;
            for (nextX = col - 1, nextY = move.LastDropRow - 1; nextX >= 0 && nextY >= 0 && b.GetPiece(nextY, nextX) == color; nextX--, nextY--)
                mainDiagonal++;

            for (nextX = col + 1, nextY = move.LastDropRow - 1; nextX < Connect4Board.COLUMNS && nextY >= 0 && b.GetPiece(nextY, nextX) == color; nextX++, nextY--)
                antiDiagonal++;
            for (nextX = col - 1, nextY = move.LastDropRow + 1; nextX >= 0 && nextY < Connect4Board.ROWS && b.GetPiece(nextY, nextX) == color; nextX--, nextY++)
                antiDiagonal++;

            return new Lines(horizontal, vertical, mainDiagonal, antiDiagonal);
        }

        internal static MoveResult CurrentStatus(Connect4Board b, Player p, int col, Connect4Column move)
        {
            Lines sucessive = GetConsecutiveLines(b, p.Color, col, move);
            if (sucessive.horizontal == 4 || sucessive.vertical == 4 || sucessive.mainDiagonal == 4 || sucessive.antiDiagonal == 4)
                return MoveResult.FOUR_IN_A_ROW;
            for (int i = 0; i < Connect4Board.ROWS; i++)
                for (int j = 0; j < Connect4Board.COLUMNS; j++)
                    if (b.GetPiece(i, j) == PieceColor.EMPTY)
                        return MoveResult.NORMAL;
            return MoveResult.BOARD_FILLED;
        }
    }
}
