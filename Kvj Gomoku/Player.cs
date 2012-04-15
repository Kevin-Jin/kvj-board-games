using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KvjBoardGames.OnlineFunctions;
using System.Drawing;
using System.Windows.Forms;

namespace KvjGomoku
{
    internal abstract class Player
    {
        private PieceColor color;

        internal PieceColor Color { get { return color; } }

        internal abstract bool IsLocal();
        internal abstract void MadeMove(int row, int col);
        internal abstract void Dispose();

        protected Player(PieceColor color)
        {
            this.color = color;
        }

        internal void Select(OmokBoard board, int row, int col)
        {
            if (board.GetPiece(row, col) == PieceColor.EMPTY)
            {
                MoveResult allowed = GameLogic.CanMove(board, this, new Point(col, row));
                switch (allowed)
                {
                    case MoveResult.NORMAL:
                        board.DrawPiece(row, col, color);
                        MadeMove(row, col);
                        board.NextTurn(new Point(col, row));
                        break;
                    case MoveResult.OVERLINE:
                        MessageBox.Show(board, "You may not place a piece there\nsince it will cause you to have\nmore than five pieces in a row.", "Illegal Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case MoveResult.DOUBLE_THREES:
                        MessageBox.Show(board, "You may not place a piece there\nsince it will cause you to have\ntwo lines of three or more\nconsecutive pieces.", "Illegal Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
        }
    }

    internal class LocalPlayer : Player
    {
        private NetworkInterface remoteOpponent;

        internal LocalPlayer(PieceColor color, NetworkInterface remoteOpponent)
            : base(color)
        {
            this.remoteOpponent = remoteOpponent;
        }

        internal override bool IsLocal()
        {
            return true;
        }

        internal override void MadeMove(int row, int col)
        {
            if (remoteOpponent != null)
                remoteOpponent.SendMessage(OmokPacketWriter.WritePlacePiece(row, col));
        }

        internal override void Dispose()
        {
            if (remoteOpponent != null)
                remoteOpponent.Disconnect();
        }
    }

    internal class NetworkPlayer : Player
    {
        internal NetworkPlayer(PieceColor color)
            : base(color)
        {
        }

        internal override bool IsLocal()
        {
            return false;
        }

        internal override void MadeMove(int row, int col)
        {
        }

        internal override void Dispose()
        {
        }
    }
}
