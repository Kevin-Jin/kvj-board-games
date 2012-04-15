using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using KvjBoardGames.OnlineFunctions;

namespace KvjChess
{
    internal enum PieceType
    {
        UNDEFINED = 0,
        PAWN = 1,
        KNIGHT = 2,
        BISHOP = 3,
        ROOK = 5,
        QUEEN = 9,
        KING = 127
    }

    internal class ChessPiece
    {
        private readonly bool white;
        internal bool White { get { return white; } }
        private PieceType type;
        internal PieceType Type { get { return type; } set { type = value; } }
        private SquareCoordinates lastSquare;
        internal SquareCoordinates LastSquare { get { return lastSquare; } set { lastSquare = value; } }
        private ushort lastMove;
        internal ushort LastMove { get { return lastMove; } set { lastMove = value; } }

        internal ChessPiece(bool white, PieceType type)
        {
            this.white = white;
            this.type = type;
            this.lastMove = 0;
            this.lastSquare = new SquareCoordinates(-1, -1);
        }

        private ChessPiece(bool white, PieceType type, ushort lastMove, SquareCoordinates lastSquare)
        {
            this.white = white;
            this.type = type;
            this.lastMove = lastMove;
            this.lastSquare = lastSquare;
        }

        internal bool IsA(PieceType type)
        {
            return this.type == type;
        }

        internal bool IsFriendly(bool currentlyWhite)
        {
            return this.white == currentlyWhite;
        }

        internal bool HasNotMoved()
        {
            return lastSquare.File == -1 && lastSquare.Rank == -1;
        }

        internal byte[] Serialize()
        {
            byte[] b = new byte[4];
            b[0] = (byte)type;
            if (b[0] != 0 && white)
                b[0] |= 0x80;
            b[1] = (byte)(!HasNotMoved() ? (lastSquare.File * 8 + lastSquare.Rank) : 0xFF);
            ByteTools.WriteUint16(b, 2, lastMove);
            return b;
        }

        internal static ChessPiece Deserialize(byte[] b)
        {
            if (b[0] == 0)
                return null;
            bool white = ((b[0] & 0x80) == 0x80);
            if (white)
                b[0] ^= 0x80;
            SquareCoordinates lastSquare = b[1] != 0xFF ? new SquareCoordinates(b[1] / 8, b[1] % 8) : new SquareCoordinates(-1, -1);
            ushort lastMove = ByteTools.ReadUint16(b, 2);
            return new ChessPiece(white, (PieceType)b[0], lastMove, lastSquare);
        }
    }

    internal static class ChessPieceImageFactory
    {
        private static readonly Dictionary<PieceType, Image> whiteImages;
        private static readonly Dictionary<PieceType, Image> blackImages;

        static ChessPieceImageFactory()
        {
            whiteImages = new Dictionary<PieceType, Image>();
            blackImages = new Dictionary<PieceType, Image>();

            Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();
            whiteImages[PieceType.PAWN] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.white_pawn.png"));
            whiteImages[PieceType.KNIGHT] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.white_knight.png"));
            whiteImages[PieceType.BISHOP] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.white_bishop.png"));
            whiteImages[PieceType.ROOK] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.white_rook.png"));
            whiteImages[PieceType.QUEEN] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.white_queen.png"));
            whiteImages[PieceType.KING] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.white_king.png"));

            blackImages[PieceType.PAWN] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.black_pawn.png"));
            blackImages[PieceType.KNIGHT] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.black_knight.png"));
            blackImages[PieceType.BISHOP] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.black_bishop.png"));
            blackImages[PieceType.ROOK] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.black_rook.png"));
            blackImages[PieceType.QUEEN] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.black_queen.png"));
            blackImages[PieceType.KING] = Image.FromStream(thisExe.GetManifestResourceStream("KvjChess.pieces.black_king.png"));
        }

        internal static Image GetWhitePiece(PieceType type)
        {
            return whiteImages[type];
        }

        internal static Image GetBlackPiece(PieceType type)
        {
            return blackImages[type];
        }
    }

    internal static class MoveRecorder
    {
        private static string PieceTypeInitial(PieceType type)
        {
            switch (type)
            {
                case PieceType.PAWN:
                    //algebraic notation doesn't actually include the initial when the move involves a pawn, but this is more consistent so just do it
                    //return moved.White ? "♙" : "♟"; //Figurine Algebraic Notation... small font size makes it too hard to read
                    return "P";
                case PieceType.KNIGHT:
                    //return moved.White ? "♘" : "♞"; //Figurine Algebraic Notation... small font size makes it too hard to read
                    return "N";
                case PieceType.BISHOP:
                    //return moved.White ? "♗" : "♝"; //Figurine Algebraic Notation... small font size makes it too hard to read
                    return "B";
                case PieceType.ROOK:
                    //return moved.White ? "♖" : "♜"; //Figurine Algebraic Notation... small font size makes it too hard to read
                    return "R";
                case PieceType.QUEEN:
                    //return moved.White ? "♕" : "♛"; //Figurine Algebraic Notation... small font size makes it too hard to read
                    return "Q";
                case PieceType.KING:
                    //return moved.White ? "♔" : "♚"; //Figurine Algebraic Notation... small font size makes it too hard to read
                    return "K";
                default:
                    return null;
            }
        }

        internal static string GetMoveNotation(SquareCoordinates start, Move m, ChessPiece moved, ChessPiece taken)
        {
            string notation = PieceTypeInitial(moved.Type);
            notation += start.ToString().ToLower();
            //long algebraic notation doesn't actually specify the piece initial of the taken piece, but this is more consistent so just do it
            notation += !m.Overtaken ? "-" : ("x" + PieceTypeInitial(taken.Type));
            notation += m.Coordinates.ToString().ToLower();
            return notation;
        }

        internal static string GetMoveNotationForCastle(bool kingSide)
        {
            return (kingSide ? "0-0" : "0-0-0");
        }
    }
}
