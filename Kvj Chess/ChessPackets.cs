using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KvjBoardGames.OnlineFunctions;

namespace KvjChess
{
    internal static class ChessOpHeaders
    {
        //MAKE SURE THAT NONE OF THESE HEADERS CONFLICT WITH THOSE IN KvjBoardGames.OnlineFunctions.CommonOpHeaders!
        internal const byte
            SELECT_SQUARE = 0x05,
            CASTLE = 0x06,
            PROMOTION = 0x07
        ;
    }

    internal class ChessPacketHandler : PacketProcessor
    {
        private NetworkPlayer player;
        private ChessBoard board;

        internal ChessPacketHandler(NetworkPlayer p, ChessBoard board)
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
                case ChessOpHeaders.SELECT_SQUARE:
                    player.Select(board, new SquareCoordinates(packet[1], packet[2]));
                    break;
                case ChessOpHeaders.CASTLE:
                    player.Castle(board, packet[1] == 1);
                    break;
                case ChessOpHeaders.PROMOTION:
                    player.PromotePiece(board, new SquareCoordinates(packet[1], packet[2]), (PieceType)packet[3]);
                    break;
            }
        }
    }

    internal static class ChessPacketWriter
    {
        internal static byte[] WriteSelectSquare(SquareCoordinates coord)
        {
            return new byte[] { ChessOpHeaders.SELECT_SQUARE, (byte)coord.File, (byte)coord.Rank };
        }

        internal static byte[] WriteCastle(bool kingSide)
        {
            return new byte[] { ChessOpHeaders.CASTLE, (byte)(kingSide ? 1 : 0) };
        }

        internal static byte[] WritePromotion(SquareCoordinates coord, PieceType newType)
        {
            return new byte[] { ChessOpHeaders.PROMOTION, (byte)coord.File, (byte)coord.Rank, (byte)newType };
        }
    }
}
