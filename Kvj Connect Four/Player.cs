using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KvjBoardGames.OnlineFunctions;

namespace KvjConnectFour
{
    internal abstract class Player
    {
        private PieceColor color;

        internal PieceColor Color { get { return color; } }

        internal abstract bool IsLocal();
        internal abstract void MadeMove(int col);
        internal abstract void Dispose();

        protected Player(PieceColor color)
        {
            this.color = color;
        }

        internal void Select(Connect4Board board, int col)
        {
            Connect4Column column = board.GetColumn(col);
            if (!column.Full)
            {
                board.DrawPiece(col, color);
                MadeMove(col);
                board.NextTurn(col);
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

        internal override void MadeMove(int col)
        {
            if (remoteOpponent != null)
                remoteOpponent.SendMessage(Connect4PacketWriter.WritePlacePiece(col));
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

        internal override void MadeMove(int col)
        {
        }

        internal override void Dispose()
        {
        }
    }
}
