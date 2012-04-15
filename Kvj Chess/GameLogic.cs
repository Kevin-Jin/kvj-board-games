using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace KvjChess
{
    internal class MoveSet
    {
        private readonly Move[] validMoves;
        public Move[] LegalMoves { get { return validMoves; } }
        private readonly SquareCoordinates[] threateningMoves;
        public SquareCoordinates[] CheckableMoves { get { return threateningMoves; } }
        private readonly SquareCoordinates[] blockedMoves;
        public SquareCoordinates[] BlockedSquareMoves { get { return blockedMoves; } }

        internal MoveSet(List<Move> allowedMoves, List<SquareCoordinates> concernableMoves, List<SquareCoordinates> occupiedMoves)
        {
            validMoves = allowedMoves.ToArray();
            threateningMoves = concernableMoves.ToArray();
            blockedMoves = occupiedMoves.ToArray();
        }

        /// <summary>
        /// Concat two MoveSets to form one MoveSet.
        /// </summary>
        /// <param name="moves1"></param>
        /// <param name="moves2"></param>
        internal MoveSet(MoveSet moves1, MoveSet moves2)
        {
            validMoves = moves1.validMoves.Concat(moves2.validMoves).ToArray();
            threateningMoves = moves1.threateningMoves.Concat(moves2.threateningMoves).ToArray();
            blockedMoves = moves1.blockedMoves.Concat(moves2.blockedMoves).ToArray();
        }

        /// <summary>
        /// Construct an empty MoveSet (no valid moves, no checkable moves, and no blocked moves).
        /// </summary>
        internal MoveSet()
        {
            validMoves = new Move[0];
            threateningMoves = new SquareCoordinates[0];
            blockedMoves = new SquareCoordinates[0];
        }
    }

    internal class Move
    {
        private readonly SquareCoordinates coord;
        internal SquareCoordinates Coordinates { get { return coord; } }
        private readonly bool overtaken;
        public bool Overtaken { get { return overtaken; } }
        private readonly SquareCoordinates takePiece;
        internal SquareCoordinates TakenPiece { get { return takePiece; } }

        internal Move(SquareCoordinates moveTo)
        {
            coord = moveTo;
            overtaken = false;
        }

        internal Move(SquareCoordinates moveTo, SquareCoordinates overtake)
        {
            coord = moveTo;
            this.overtaken = true;
            takePiece = overtake;
        }
    }

    internal enum KingStatus
    {
        NO_IMMEDIATE_CONCERN = 0,
        CHECK = 1,
        STALEMATE = 2,
        CHECKMATE = 3
    }

    //TODO: en passant - use ChessPiece.LastMove and ChessBoard.CurrentMove to determine if it was only one move ago.
    // use ChessPiece.LastSquare and the current coordinates of the piece to see if it moved up 2 spaces in one move from its initial location.
    //TODO: add fifty-move rule, draw if no pieces captured or no pawns moved in the previous 50 moves.
    //Draw if Threefold repetition
    //Draw if perpetual check?
    //Draw if "insufficient material"
    //Draw by time limit
    //Draw by fortress
    //Draw if king vs. king; king vs. king + bishop; king vs. king + knight; king + bishop vs. king + bishop (when bishops are on squares of same color)
    internal static class GameLogic
    {
        private static SquareCoordinates EnPassant(ChessBoard board, SquareCoordinates moveTo, bool white)
        {
            if (white)
            {
                if (moveTo.Rank != 5)
                    return new SquareCoordinates(-1, -1);
                ChessPiece pawn = board.GetPiece(new SquareCoordinates(moveTo.File, 4));
                if (pawn == null || !pawn.IsA(PieceType.PAWN) || pawn.IsFriendly(white))
                    return new SquareCoordinates(-1, -1);
                if (pawn.LastMove != board.CurrentMove - 1 || pawn.LastSquare != new SquareCoordinates(moveTo.File, 6))
                    return new SquareCoordinates(-1, -1);
                return new SquareCoordinates(moveTo.File, moveTo.Rank - 1);
            }
            else
            {
                if (moveTo.Rank != 2)
                    return new SquareCoordinates(-1, -1);
                ChessPiece pawn = board.GetPiece(new SquareCoordinates(moveTo.File, 3));
                if (pawn == null || !pawn.IsA(PieceType.PAWN) || pawn.IsFriendly(white))
                    return new SquareCoordinates(-1, -1);
                if (pawn.LastMove != board.CurrentMove - 1 || pawn.LastSquare != new SquareCoordinates(moveTo.File, 1))
                    return new SquareCoordinates(-1, -1);
                return new SquareCoordinates(moveTo.File, moveTo.Rank + 1);
            }
        }

        private delegate void ProcessSquare(SquareCoordinates to);

        //TODO: promotion (pawn reaching opponent's end of board -> any piece)
        private static MoveSet PawnMoves(ChessBoard board, SquareCoordinates coord, Player p)
        {
            List<Move> allowedMoves = new List<Move>();
            List<SquareCoordinates> movesWithCheck = new List<SquareCoordinates>();
            List<SquareCoordinates> blockedMoves = new List<SquareCoordinates>();
            bool boardOrientation = board.WhiteOnBottom;
            bool white = p.White;
            SquareCoordinates nextSquare;
            ChessPiece existingPiece;
            Move m;
            int nextRank, nextFile;
            //check if piece would go out of bounds
            if (white && (nextRank = coord.Rank + 1) < 8 || !white && (nextRank = coord.Rank - 1) >= 0)
            {
                nextSquare = new SquareCoordinates(coord.File, nextRank);
                existingPiece = board.GetPiece(nextSquare);
                if (existingPiece == null)
                    //move up 1 if no piece is in that space
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(nextSquare)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(nextSquare);
                else
                    blockedMoves.Add(nextSquare);

                //diagonal take
                SquareCoordinates opponentEnPassantPawn;
                ProcessSquare diagonalProc = delegate(SquareCoordinates to)
                {
                    if ((existingPiece = board.GetPiece(to)) != null)
                    {
                        if (!existingPiece.IsFriendly(white))
                            if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, to)))
                                allowedMoves.Add(m);
                            else
                                movesWithCheck.Add(to);
                    }
                    else if ((opponentEnPassantPawn = EnPassant(board, to, p.White)).File != -1 && opponentEnPassantPawn.Rank != -1)
                    {
                        if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, opponentEnPassantPawn)))
                            allowedMoves.Add(m);
                        else
                            movesWithCheck.Add(nextSquare);
                    }
                };
                nextFile = coord.File + 1;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextFile < 8)
                    diagonalProc(new SquareCoordinates(nextFile, nextRank)); //move diagonally up right if an opponent piece is in that space
                nextFile = coord.File - 1;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextFile >= 0)
                    diagonalProc(new SquareCoordinates(nextFile, nextRank)); //move diagonally up left if an opponent piece is in that space
            }
            //check if we are still in the home position
            if (white && (nextRank = coord.Rank + 2) == 3 || !white && (nextRank = coord.Rank - 2) == 4)
            {
                nextSquare = new SquareCoordinates(coord.File, nextRank);
                SquareCoordinates betweenSquare = new SquareCoordinates(coord.File, (nextRank + coord.Rank) / 2);
                existingPiece = board.GetPiece(nextSquare);
                ChessPiece betweenPiece = board.GetPiece(betweenSquare);
                if (existingPiece == null && betweenPiece == null)
                    //move up 2 spaces if no piece is at that space and no piece is in between
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(nextSquare)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(nextSquare);
                else
                    blockedMoves.Add(nextSquare);
            }
            return new MoveSet(allowedMoves, movesWithCheck, blockedMoves);
        }

        private static MoveSet KnightMoves(ChessBoard board, SquareCoordinates coord, Player p)
        {
            List<Move> allowedMoves = new List<Move>();
            List<SquareCoordinates> movesWithCheck = new List<SquareCoordinates>();
            List<SquareCoordinates> blockedMoves = new List<SquareCoordinates>();
            bool boardOrientation = board.WhiteOnBottom;
            bool white = p.White;
            //SquareCoordinates opponentEnPassantPawn;
            ChessPiece existingPiece;
            Move m;
            int nextFile, nextRank;

            ProcessSquare proc = delegate(SquareCoordinates to)
            {
                if ((existingPiece = board.GetPiece(to)) == null)
                    /*if ((opponentEnPassantPawn = EnPassant(board, to, p.White)).File != -1 && opponentEnPassantPawn.Rank != -1)
                        if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, opponentEnPassantPawn)))
                            allowedMoves.Add(m);
                        else
                            movesWithCheck.Add(to);
                    else*/
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else if (!existingPiece.IsFriendly(p.White))
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, to)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else
                    blockedMoves.Add(to);
            };

            nextFile = coord.File + 1;
            if (nextFile < 8)
            {
                nextRank = coord.Rank + 2;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank < 8)
                    proc(new SquareCoordinates(nextFile, nextRank)); //right 1 up 2
                nextRank = coord.Rank - 2;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank >= 0)
                    proc(new SquareCoordinates(nextFile, nextRank)); //right 1 down 2
            }
            nextFile = coord.File + 2;
            if (nextFile < 8)
            {
                nextRank = coord.Rank + 1;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank < 8)
                    proc(new SquareCoordinates(nextFile, nextRank)); //right 2 up 1
                nextRank = coord.Rank - 1;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank >= 0)
                    proc(new SquareCoordinates(nextFile, nextRank)); //right 2 down 1
            }
            nextFile = coord.File - 1;
            if (nextFile >= 0)
            {
                nextRank = coord.Rank + 2;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank < 8)
                    proc(new SquareCoordinates(nextFile, nextRank)); //left 1 up 2
                nextRank = coord.Rank - 2;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank >= 0)
                    proc(new SquareCoordinates(nextFile, nextRank)); //left 1 down 2
            }
            nextFile = coord.File - 2;
            if (nextFile >= 0)
            {
                nextRank = coord.Rank + 1;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank < 8)
                    proc(new SquareCoordinates(nextFile, nextRank)); //left 2 up 1
                nextRank = coord.Rank - 1;
                //check if we are in bounds and that either no piece exists there, or the piece is not a friendly
                if (nextRank >= 0)
                    proc(new SquareCoordinates(nextFile, nextRank)); //left 2 down 1
            }
            return new MoveSet(allowedMoves, movesWithCheck, blockedMoves);
        }

        private static MoveSet BishopMoves(ChessBoard board, SquareCoordinates coord, Player p)
        {
            List<Move> allowedMoves = new List<Move>();
            List<SquareCoordinates> movesWithCheck = new List<SquareCoordinates>();
            List<SquareCoordinates> blockedMoves = new List<SquareCoordinates>();
            bool boardOrientation = board.WhiteOnBottom;
            bool white = p.White;
            SquareCoordinates nextSquare/*, opponentEnPassantPawn*/;
            Move m;
            int nextFile, nextRank;

            ProcessSquare diagonalProc = delegate(SquareCoordinates to)
            {
                /*if ((opponentEnPassantPawn = EnPassant(board, to, p.White)).File != -1 && opponentEnPassantPawn.Rank != -1)
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, opponentEnPassantPawn)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else*/
                if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to)))
                    allowedMoves.Add(m);
                else
                    movesWithCheck.Add(to);
            };

            ProcessSquare blockingPieceProc = delegate(SquareCoordinates to)
            {
                if (!board.GetPiece(to).IsFriendly(white))
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, to)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else
                    blockedMoves.Add(to);
            };

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (++nextFile < 8 && ++nextRank < 8 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                diagonalProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextFile < 8 && nextRank < 8)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (++nextFile < 8 && --nextRank >= 0 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                diagonalProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextFile < 8 && nextRank >= 0)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (--nextFile >= 0 && ++nextRank < 8 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                diagonalProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextFile >= 0 && nextRank < 8)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (--nextFile >= 0 && --nextRank >= 0 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                diagonalProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextFile >= 0 && nextRank >= 0)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            return new MoveSet(allowedMoves, movesWithCheck, blockedMoves);
        }

        private static MoveSet RookMoves(ChessBoard board, SquareCoordinates coord, Player p)
        {
            List<Move> allowedMoves = new List<Move>();
            List<SquareCoordinates> movesWithCheck = new List<SquareCoordinates>();
            List<SquareCoordinates> blockedMoves = new List<SquareCoordinates>();
            bool boardOrientation = board.WhiteOnBottom;
            bool white = p.White;
            SquareCoordinates nextSquare/*, opponentEnPassantPawn*/;
            Move m;
            int nextFile, nextRank;

            ProcessSquare linearProc = delegate(SquareCoordinates to)
            {
                /*if ((opponentEnPassantPawn = EnPassant(board, to, p.White)).File != -1 && opponentEnPassantPawn.Rank != -1)
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, opponentEnPassantPawn)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else*/
                if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to)))
                    allowedMoves.Add(m);
                else
                    movesWithCheck.Add(to);
            };

            ProcessSquare blockingPieceProc = delegate(SquareCoordinates to)
            {
                if (!board.GetPiece(to).IsFriendly(white))
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, to)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else
                    blockedMoves.Add(to);
            };

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (++nextFile < 8 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                linearProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextFile < 8)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (++nextRank < 8 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                linearProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextRank < 8)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (--nextFile >= 0 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                linearProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextFile >= 0)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            nextFile = coord.File;
            nextRank = coord.Rank;
            while (--nextRank >= 0 && board.GetPiece(nextSquare = new SquareCoordinates(nextFile, nextRank)) == null)
                linearProc(nextSquare);
            //we stop the loop if we encounter any piece, or if we start going out of bounds off the board.
            //if we did not stop at the edge of the board, check to see if the piece we stopped at is an opponent piece. if it is, we can move there as well
            if (nextRank >= 0)
                blockingPieceProc(new SquareCoordinates(nextFile, nextRank));

            return new MoveSet(allowedMoves, movesWithCheck, blockedMoves);
        }

        private static MoveSet QueenMoves(ChessBoard board, SquareCoordinates coord, Player p)
        {
            //Queen's allowed moves are basically a combination of those of rook (diagonals) and bishop (vertical/horizontal)
            //so just call the methods to get rook and bishop moves and return a union of them
            return new MoveSet(RookMoves(board, coord, p), BishopMoves(board, coord, p));
        }

        private static MoveSet KingMoves(ChessBoard board, SquareCoordinates coord, Player p)
        {
            List<Move> allowedMoves = new List<Move>();
            List<SquareCoordinates> movesWithCheck = new List<SquareCoordinates>();
            List<SquareCoordinates> blockedMoves = new List<SquareCoordinates>();
            bool boardOrientation = board.WhiteOnBottom;
            bool white = p.White;
            //SquareCoordinates opponentEnPassantPawn;
            ChessPiece existingPiece;
            Move m;
            int nextFile, nextRank;

            ProcessSquare proc = delegate(SquareCoordinates to)
            {
                //check if either no piece exists there, or the piece is not a friendly
                if ((existingPiece = (board.GetPiece(to))) == null)
                    /*if ((opponentEnPassantPawn = EnPassant(board, to, p.White)).File != -1 && opponentEnPassantPawn.Rank != -1)
                        if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, opponentEnPassantPawn)))
                            allowedMoves.Add(m);
                        else
                            movesWithCheck.Add(to);
                    else*/
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else if (!existingPiece.IsFriendly(white))
                    if (!board.WillPlaceKingInCheck(p, coord, m = new Move(to, to)))
                        allowedMoves.Add(m);
                    else
                        movesWithCheck.Add(to);
                else
                    blockedMoves.Add(to);
            };

            nextFile = coord.File - 1;
            if (nextFile >= 0)
            {
                nextRank = coord.Rank - 1;
                if (nextRank >= 0) //check if we are in bounds
                    proc(new SquareCoordinates(nextFile, nextRank)); //left down
                nextRank = coord.Rank;
                proc(new SquareCoordinates(nextFile, nextRank)); //left
                nextRank = coord.Rank + 1;
                if (nextRank < 8) //check if we are in bounds
                    proc(new SquareCoordinates(nextFile, nextRank)); //left up
            }

            nextFile = coord.File;
            nextRank = coord.Rank - 1;
            if (nextRank >= 0) //check if we are in bounds
                proc(new SquareCoordinates(nextFile, nextRank)); //down
            nextRank = coord.Rank + 1;
            if (nextRank < 8) //check if we are in bounds
                proc(new SquareCoordinates(nextFile, nextRank)); //up

            nextFile = coord.File + 1;
            if (nextFile < 8)
            {
                nextRank = coord.Rank - 1;
                if (nextRank >= 0) //check if we are in bounds
                    proc(new SquareCoordinates(nextFile, nextRank)); //right down
                nextRank = coord.Rank;
                proc(new SquareCoordinates(nextFile, nextRank)); //right
                nextRank = coord.Rank + 1;
                if (nextRank < 8) //check if we are in bounds
                    proc(new SquareCoordinates(nextFile, nextRank)); //right up
            }

            return new MoveSet(allowedMoves, movesWithCheck, blockedMoves);
        }

        internal static MoveSet MoveSet(ChessBoard board, SquareCoordinates coord, Player p)
        {
            switch (board.GetPiece(coord).Type)
            {
                case PieceType.PAWN:
                    return PawnMoves(board, coord, p);
                case PieceType.KNIGHT:
                    return KnightMoves(board, coord, p);
                case PieceType.BISHOP:
                    return BishopMoves(board, coord, p);
                case PieceType.ROOK:
                    return RookMoves(board, coord, p);
                case PieceType.QUEEN:
                    return QueenMoves(board, coord, p);
                case PieceType.KING:
                    return KingMoves(board, coord, p);
                default:
                    return new MoveSet();
            }
        }

        private static SquareCoordinates[] Attackers(ChessBoard board, SquareCoordinates pieceLoc, bool pieceWhite, bool allowPawnDiagonal)
        {
            List<SquareCoordinates> attackingPieces = new List<SquareCoordinates>();
            int file, rank;
            ChessPiece existingPiece;
            SquareCoordinates coord;
            //go vertically/horizontally and see if there's an unblocked opponent queen or rook.
            existingPiece = null;
            coord = new SquareCoordinates(-1, -1);
            for (file = pieceLoc.File + 1, rank = pieceLoc.Rank; file < 8 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; file++) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.ROOK) || existingPiece.IsA(PieceType.QUEEN)))
                attackingPieces.Add(coord);
            existingPiece = null;
            coord = new SquareCoordinates(-1, -1);
            for (file = pieceLoc.File - 1, rank = pieceLoc.Rank; file >= 0 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; file--) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.ROOK) || existingPiece.IsA(PieceType.QUEEN)))
                attackingPieces.Add(coord);
            existingPiece = null;
            coord = new SquareCoordinates(-1, -1);
            for (file = pieceLoc.File, rank = pieceLoc.Rank + 1; rank < 8 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; rank++) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.ROOK) || existingPiece.IsA(PieceType.QUEEN) || !allowPawnDiagonal && pieceWhite && rank - pieceLoc.Rank == 1 && existingPiece.IsA(PieceType.PAWN)))
                attackingPieces.Add(coord);
            existingPiece = null;
            coord = new SquareCoordinates(-1, -1);
            for (file = pieceLoc.File, rank = pieceLoc.Rank - 1; rank >= 0 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; rank--) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.ROOK) || existingPiece.IsA(PieceType.QUEEN) || !allowPawnDiagonal && !pieceWhite && pieceLoc.Rank - rank == 1 && existingPiece.IsA(PieceType.PAWN)))
                attackingPieces.Add(coord);

            //go diagonally and see if there's an unblocked opponent queen or bishop (or pawn if only one diagonal away).
            existingPiece = null;
            for (file = pieceLoc.File + 1, rank = pieceLoc.Rank + 1; file < 8 && rank < 8 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; file++, rank++) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.BISHOP) || existingPiece.IsA(PieceType.QUEEN) || allowPawnDiagonal && pieceWhite && file - pieceLoc.File == 1 && rank - pieceLoc.Rank == 1 && existingPiece.IsA(PieceType.PAWN)))
                attackingPieces.Add(coord);
            existingPiece = null;
            for (file = pieceLoc.File + 1, rank = pieceLoc.Rank - 1; file < 8 && rank >= 0 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; file++, rank--) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.BISHOP) || existingPiece.IsA(PieceType.QUEEN) || allowPawnDiagonal && !pieceWhite && file - pieceLoc.File == 1 && pieceLoc.Rank - rank == 1 && existingPiece.IsA(PieceType.PAWN)))
                attackingPieces.Add(coord);
            existingPiece = null;
            for (file = pieceLoc.File - 1, rank = pieceLoc.Rank + 1; file >= 0 && rank < 8 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; file--, rank++) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.BISHOP) || existingPiece.IsA(PieceType.QUEEN) || allowPawnDiagonal && pieceWhite && pieceLoc.File - file == 1 && rank - pieceLoc.Rank == 1 && existingPiece.IsA(PieceType.PAWN)))
                attackingPieces.Add(coord);
            existingPiece = null;
            for (file = pieceLoc.File - 1, rank = pieceLoc.Rank - 1; file >= 0 && rank >= 0 && (existingPiece = board.GetPiece((coord = new SquareCoordinates(file, rank)))) == null; file--, rank--) ;
            if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && (existingPiece.IsA(PieceType.BISHOP) || existingPiece.IsA(PieceType.QUEEN) || allowPawnDiagonal && !pieceWhite && pieceLoc.File - file == 1 && pieceLoc.Rank - rank == 1 && existingPiece.IsA(PieceType.PAWN)))
                attackingPieces.Add(coord);

            //go through all 8 L-shapes and see if there's a knight in each (doesn't matter if another piece is in the way).
            file = pieceLoc.File + 2;
            rank = pieceLoc.Rank + 1;
            if (file < 8 && rank < 8)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            file = pieceLoc.File + 2;
            rank = pieceLoc.Rank - 1;
            if (file < 8 && rank >= 0)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            file = pieceLoc.File + 1;
            rank = pieceLoc.Rank + 2;
            if (file < 8 && rank < 8)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            file = pieceLoc.File + 1;
            rank = pieceLoc.Rank - 2;
            if (file < 8 && rank >= 0)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            file = pieceLoc.File - 2;
            rank = pieceLoc.Rank + 1;
            if (file >= 0 && rank < 8)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            file = pieceLoc.File - 2;
            rank = pieceLoc.Rank - 1;
            if (file >= 0 && rank >= 0)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            file = pieceLoc.File - 1;
            rank = pieceLoc.Rank + 2;
            if (file >= 0 && rank < 8)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            file = pieceLoc.File - 1;
            rank = pieceLoc.Rank - 2;
            if (file >= 0 && rank >= 0)
            {
                coord = new SquareCoordinates(file, rank);
                existingPiece = board.GetPiece(coord);
                if (existingPiece != null && !existingPiece.IsFriendly(pieceWhite) && existingPiece.IsA(PieceType.KNIGHT))
                    attackingPieces.Add(coord);
            }
            return attackingPieces.ToArray();
        }

        private static bool CanInterpose(ChessBoard board, SquareCoordinates from, SquareCoordinates to, Player p)
        {
            switch (board.GetPiece(from).Type)
            {
                case PieceType.PAWN:
                    return false; //if pawns cannot be captured, they cannot be blocked by friendly pieces!
                case PieceType.KNIGHT:
                    return false; //if knights cannot be captured, they cannot be blocked by friendly pieces!
                case PieceType.BISHOP:
                    if (from.File - to.File == from.Rank - to.Rank)
                    {
                        for (int file = Math.Min(from.File, to.File) + 1, rank = Math.Min(from.Rank, to.Rank) + 1; file < Math.Max(from.File, to.File); file++, rank++)
                            if (Attackers(board, new SquareCoordinates(file, rank), !p.White, false).Length > 0)
                                return true;
                    }
                    else if (from.File - to.File == -(from.Rank - to.Rank))
                    {
                        for (int file = Math.Min(from.File, to.File) + 1, rank = Math.Max(from.Rank, to.Rank) + 1; file < Math.Max(from.File, to.File) && rank < 8; file++, rank--)
                            if (Attackers(board, new SquareCoordinates(file, rank), !p.White, false).Length > 0)
                                return true;
                    }
                    return false;
                case PieceType.ROOK:
                    if (from.File == to.File)
                    {
                        for (int file = from.File, rank = Math.Min(from.Rank, to.Rank) + 1; rank < Math.Max(from.Rank, to.Rank); rank++)
                            if (Attackers(board, new SquareCoordinates(file, rank), !p.White, false).Length > 0)
                                return true;
                    }
                    else if (from.Rank == to.Rank)
                    {
                        for (int file = Math.Min(from.File, to.File) + 1, rank = from.Rank; file < Math.Max(from.File, to.File); file++)
                            if (Attackers(board, new SquareCoordinates(file, rank), !p.White, false).Length > 0)
                                return true;
                    }
                    return false;
                case PieceType.QUEEN:
                    if (from.File == to.File || from.Rank == to.Rank)
                        goto case PieceType.ROOK;
                    else if (Math.Abs(from.File - to.File) == Math.Abs(from.Rank - to.Rank))
                        goto case PieceType.BISHOP;
                    return false;
                default:
                    return false;
            }
        }

        internal static bool KingInCheck(ChessBoard board, SquareCoordinates kingSquare, bool white)
        {
            return Attackers(board, kingSquare, white, true).Length != 0;
        }

        internal static bool KingInCheck(ChessBoard board, Player p)
        {
            return KingInCheck(board, p.Material.KingSquare, p.White);
        }

        internal static KingStatus KingConcern(ChessBoard board, Player p)
        {
            MoveSet moves = KingMoves(board, p.Material.KingSquare, p);

            SquareCoordinates[] attackingPieces = Attackers(board, p.Material.KingSquare, p.White, true);
            if (attackingPieces.Length != 0)
            {
                if (moves.LegalMoves.Length != 0)
                    return KingStatus.CHECK;

                foreach (SquareCoordinates coord in attackingPieces)
                    if (Attackers(board, coord, !p.White, true).Length == 0 && !CanInterpose(board, coord, p.Material.KingSquare, p))
                        return KingStatus.CHECKMATE; //there is one opponent piece that can reach our king and is unable to be captured or blocked

                return KingStatus.CHECK;
            }
            else
            {
                if (moves.LegalMoves.Length != 0)
                    return KingStatus.NO_IMMEDIATE_CONCERN;

                foreach (SquareCoordinates coord in p.Material.Pieces)
                    if (MoveSet(board, coord, p).LegalMoves.Length != 0)
                        return KingStatus.NO_IMMEDIATE_CONCERN; //there is one friendly piece we can move

                return KingStatus.STALEMATE;
            }
        }

        internal static bool CanCastle(ChessBoard board, SquareCoordinates coord, Player p)
        {
            ChessPiece piece = board.GetPiece(coord);
            if (piece != null && piece.IsA(PieceType.ROOK) && piece.IsFriendly(p.White) && piece.HasNotMoved())
            {
                SquareCoordinates kingSq = p.White ? new SquareCoordinates("E1") : new SquareCoordinates("E8");
                piece = board.GetPiece(kingSq);
                if (piece != null && piece.IsA(PieceType.KING) && piece.IsFriendly(p.White) && piece.HasNotMoved() && !KingInCheck(board, p))
                {
                    if (coord.File == 0)
                    {
                        if (!board.WillPlaceKingInCheck(p, new SquareCoordinates[] { new SquareCoordinates(4, coord.Rank), new SquareCoordinates(0, coord.Rank) }, new SquareCoordinates[] { new SquareCoordinates(2, coord.Rank), new SquareCoordinates(3, coord.Rank) }))
                        {
                            for (int file = 1, rank = coord.Rank; file < 4; file++)
                                if (board.GetPiece(coord = new SquareCoordinates(file, rank)) != null || Attackers(board, coord, p.White, true).Length > 0)
                                    return false;
                            return true;
                        }
                    }
                    else if (coord.File == 7)
                    {
                        if (!board.WillPlaceKingInCheck(p, new SquareCoordinates[] { new SquareCoordinates(4, coord.Rank), new SquareCoordinates(7, coord.Rank) }, new SquareCoordinates[] { new SquareCoordinates(6, coord.Rank), new SquareCoordinates(5, coord.Rank) }))
                        {
                            for (int file = 5, rank = coord.Rank; file < 7; file++)
                                if (board.GetPiece(coord = new SquareCoordinates(file, rank)) != null || Attackers(board, coord, p.White, true).Length > 0)
                                    return false;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
