using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KvjBoardGames.OnlineFunctions;

namespace KvjConnectFour
{
    internal static class Connect4OpHeaders
    {
        //MAKE SURE THAT NONE OF THESE HEADERS CONFLICT WITH THOSE IN KvjBoardGames.OnlineFunctions.CommonOpHeaders!
        internal const byte
            PLACE = 0x05
        ;
    }

    internal class Connect4PacketHandler : PacketProcessor
    {
        private NetworkPlayer player;
        private Connect4Board board;

        internal Connect4PacketHandler(NetworkPlayer p, Connect4Board board)
        {
            this.player = p;
            this.board = board;
        }

        public void Handle(byte[] packet)
        {
            switch (packet[0])
            {
                case CommonOpHeaders.END_SESSION:
                    break;
                case CommonOpHeaders.FORFEIT:
                    board.ReceivedForfeit();
                    break;
                case CommonOpHeaders.RESET_GAME:
                    board.ResetBoard();
                    break;
                case CommonOpHeaders.DRAW_REQUEST:
                    board.ReceivedDraw();
                    break;
                case CommonOpHeaders.DRAW_RESPONSE:
                    if (packet[1] == 1)
                        board.DoDraw();
                    else
                        board.DrawDenied();
                    break;
                case Connect4OpHeaders.PLACE:
                    player.Select(board, packet[1]);
                    break;
            }
        }
    }

    internal static class Connect4PacketWriter
    {
        internal static byte[] WritePlacePiece(int col)
        {
            return new byte[] { Connect4OpHeaders.PLACE, (byte)col };
        }
    }
}
