using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KvjBoardGames.OnlineFunctions;

namespace KvjGomoku
{
    internal static class OmokOpHeaders
    {
        //MAKE SURE THAT NONE OF THESE HEADERS CONFLICT WITH THOSE IN KvjBoardGames.OnlineFunctions.CommonOpHeaders!
        internal const byte
            PLACE = 0x05
        ;
    }

    internal class OmokPacketHandler : PacketProcessor
    {
        private NetworkPlayer player;
        private OmokBoard board;

        internal OmokPacketHandler(NetworkPlayer p, OmokBoard board)
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
                case OmokOpHeaders.PLACE:
                    player.Select(board, packet[1], packet[2]);
                    break;
            }
        }
    }

    internal static class OmokPacketWriter
    {
        internal static byte[] WritePlacePiece(int row, int col)
        {
            return new byte[] { OmokOpHeaders.PLACE, (byte)row, (byte)col };
        }
    }
}
