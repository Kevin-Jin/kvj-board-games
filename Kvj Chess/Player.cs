using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using KvjBoardGames.OnlineFunctions;
using System.Windows.Forms;
using System.Collections;

namespace KvjChess
{
    internal struct Material
    {
        private List<SquareCoordinates> pieces;
        internal List<SquareCoordinates> Pieces { get { return pieces; } }
        private SquareCoordinates kingSquare;
        internal SquareCoordinates KingSquare { get { return kingSquare; } }

        internal Material(List<SquareCoordinates> pieces, SquareCoordinates king)
        {
            this.pieces = pieces;
            this.kingSquare = king;
        }

        internal void MovePiece(PieceType type, SquareCoordinates from, SquareCoordinates to)
        {
            if (type != PieceType.KING)
            {
                pieces.Remove(from);
                pieces.Add(to);
            }
            else
            {
                kingSquare = to;
            }
        }
    }

    internal abstract class Player
    {
        private readonly bool white;
        internal bool White { get { return white; } }
        private SquareCoordinates firstSelection;
        private MoveSet moves;
        private Material material;
        public Material Material { get { return material; } }

        protected Player(bool white, Material material)
        {
            this.white = white;
            this.material = material;
            ResetSelected();
        }

        internal void Reset(Material material)
        {
            this.material = material;
            ResetSelected();
        }

        private void ResetSelected()
        {
            moves = null;
        }

        protected abstract void FirstSquareSelected(ChessBoard board, SquareCoordinates coord, MoveSet moves);
        protected abstract void MadeMove(ChessBoard board, ChessPiece movedPiece, SquareCoordinates from, SquareCoordinates to, MoveSet moves);
        protected abstract void PerformedCastle(ChessBoard board, bool kingSide, MoveSet moves);
        protected abstract void Unselected(ChessBoard board, SquareCoordinates coord, MoveSet moves);

        internal abstract bool IsLocal();
        internal abstract void Dispose();

        internal void Select(ChessBoard board, SquareCoordinates coord)
        {
            ChessPiece selectedPiece = board.GetPiece(coord);
            if (moves == null)
            {
                if (selectedPiece != null && selectedPiece.White == white)
                {
                    firstSelection = coord;
                    moves = GameLogic.MoveSet(board, coord, this);
                    board.HighlightSquare(coord, Color.Yellow);
                    FirstSquareSelected(board, coord, moves);
                }
            }
            else
            {
                if (coord == firstSelection)
                {
                    MoveSet prevMoves = moves;
                    ResetSelected();
                    board.UnhighlightSquare(coord);
                    Unselected(board, coord, prevMoves);
                }
                else
                {
                    Move m = GetMove(moves, coord);
                    if (m != null)
                    {
                        ChessPiece movedPiece, takenPiece = null;
                        MoveSet prevMoves = moves;
                        ResetSelected();
                        board.UnhighlightSquare(firstSelection);
                        if (m.Overtaken)
                            takenPiece = board.ClearSquare(m.TakenPiece);

                        movedPiece = board.ClearSquare(firstSelection);
                        movedPiece.LastSquare = firstSelection;
                        movedPiece.LastMove = board.CurrentMove;
                        board.DrawPiece(coord, movedPiece);
                        material.MovePiece(movedPiece.Type, firstSelection, coord);

                        MadeMove(board, movedPiece, firstSelection, coord, prevMoves);
                        board.UpdateLastMove(firstSelection, m, movedPiece, takenPiece);
                        board.NextTurn();
                    }
                    else if (moves.CheckableMoves.Contains(coord) && !board.ShowCheckedMoves)
                    {
                        MessageBox.Show(board, "You may not place the selected piece there\nsince it will either place your king in check\nor will leave your king in check.", "Illegal Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        internal void Castle(ChessBoard board, bool kingSide)
        {
            MoveSet prevMoves = moves;
            if (moves != null)
            {
                ResetSelected();
                board.UnhighlightSquare(firstSelection);
            }

            SquareCoordinates kingStart, kingEnd, rookStart, rookEnd;
            ChessPiece piece;

            int rank = white ? 0 : 7;
            kingStart = new SquareCoordinates(4, rank);
            if (kingSide)
            {
                kingEnd = new SquareCoordinates(6, rank);
                rookStart = new SquareCoordinates(7, rank);
                rookEnd = new SquareCoordinates(5, rank);
            }
            else
            {
                kingEnd = new SquareCoordinates(2, rank);
                rookStart = new SquareCoordinates(0, rank);
                rookEnd = new SquareCoordinates(3, rank);
            }
            //rookEnd = new SquareCoordinates((kingStart.File + kingEnd.File) / 2, rank);

            piece = board.ClearSquare(kingStart);
            piece.LastSquare = kingStart;
            piece.LastMove = board.CurrentMove;
            board.DrawPiece(kingEnd, piece);
            material.MovePiece(PieceType.KING, kingStart, kingEnd);

            piece = board.ClearSquare(rookStart);
            piece.LastSquare = rookStart;
            piece.LastMove = board.CurrentMove;
            board.DrawPiece(rookEnd, piece);
            material.MovePiece(PieceType.ROOK, rookStart, rookEnd);

            PerformedCastle(board, kingSide, prevMoves);
            board.UpdateLastMoveFromCastle(kingSide);
            board.NextTurn();
        }

        internal void PromotePiece(ChessBoard board, SquareCoordinates coord, PieceType promotedType)
        {
            PromotePiece(board, coord, promotedType, board.GetPiece(coord));
        }

        protected void PromotePiece(ChessBoard board, SquareCoordinates coord, PieceType promotedType, ChessPiece piece)
        {
            piece.Type = promotedType;
            board.ClearSquare(coord);
            board.DrawPiece(coord, piece);
        }

        private Move GetMove(MoveSet moves, SquareCoordinates coord)
        {
            foreach (Move legalMove in moves.LegalMoves)
                if (legalMove.Coordinates == coord)
                    return legalMove;
            return null;
        }
    }

    internal class LocalPlayer : Player
    {
        private NetworkInterface remoteOpponent;

        internal LocalPlayer(ChessBoard board, bool white, NetworkInterface remoteOpponent, Material material)
            : base(white, material)
        {
            this.remoteOpponent = remoteOpponent;
        }

        protected override void FirstSquareSelected(ChessBoard board, SquareCoordinates coord, MoveSet moves)
        {
            if (board.ShowLegalMoves)
                foreach (Move legalMove in moves.LegalMoves)
                    board.HighlightSquare(legalMove.Coordinates, Color.Green);
            if (board.ShowCheckedMoves)
                foreach (SquareCoordinates checkedMoves in moves.CheckableMoves)
                    board.HighlightSquare(checkedMoves, Color.Indigo);
            if (board.ShowBlockedMoves)
                foreach (SquareCoordinates checkedMoves in moves.BlockedSquareMoves)
                    board.HighlightSquare(checkedMoves, Color.Red);
            if (remoteOpponent != null)
                remoteOpponent.SendMessage(ChessPacketWriter.WriteSelectSquare(coord));
        }

        protected override void MadeMove(ChessBoard board, ChessPiece movedPiece, SquareCoordinates from, SquareCoordinates to, MoveSet moves)
        {
            foreach (Move legalMove in moves.LegalMoves)
                board.UnhighlightSquare(legalMove.Coordinates);
            foreach (SquareCoordinates checkedMoves in moves.CheckableMoves)
                board.UnhighlightSquare(checkedMoves);
            foreach (SquareCoordinates checkedMoves in moves.BlockedSquareMoves)
                board.UnhighlightSquare(checkedMoves);
            PieceType promoted = PieceType.UNDEFINED;
            if (movedPiece.IsA(PieceType.PAWN) && (White && to.Rank == 7 || !White && to.Rank == 0))
                PromotePiece(board, to, promoted = new PromotionSelect().Prompt(board));
            if (remoteOpponent != null)
            {
                remoteOpponent.SendMessage(ChessPacketWriter.WriteSelectSquare(to));
                if (promoted != PieceType.UNDEFINED)
                    remoteOpponent.SendMessage(ChessPacketWriter.WritePromotion(to, promoted));
            }
        }

        protected override void PerformedCastle(ChessBoard board, bool kingSide, MoveSet moves)
        {
            if (moves != null)
            {
                foreach (Move legalMove in moves.LegalMoves)
                    board.UnhighlightSquare(legalMove.Coordinates);
                foreach (SquareCoordinates checkedMoves in moves.CheckableMoves)
                    board.UnhighlightSquare(checkedMoves);
                foreach (SquareCoordinates checkedMoves in moves.BlockedSquareMoves)
                    board.UnhighlightSquare(checkedMoves);
            }
            if (remoteOpponent != null)
                remoteOpponent.SendMessage(ChessPacketWriter.WriteCastle(kingSide));
        }

        protected override void Unselected(ChessBoard board, SquareCoordinates coord, MoveSet moves)
        {
            foreach (Move legalMove in moves.LegalMoves)
                board.UnhighlightSquare(legalMove.Coordinates);
            foreach (SquareCoordinates checkedMoves in moves.CheckableMoves)
                board.UnhighlightSquare(checkedMoves);
            foreach (SquareCoordinates checkedMoves in moves.BlockedSquareMoves)
                board.UnhighlightSquare(checkedMoves);
            if (remoteOpponent != null)
                remoteOpponent.SendMessage(ChessPacketWriter.WriteSelectSquare(coord));
        }

        internal override bool IsLocal()
        {
            return true;
        }

        internal override void Dispose()
        {
            if (remoteOpponent != null)
                remoteOpponent.Disconnect();
        }
    }

    internal class NetworkPlayer : Player
    {
        internal NetworkPlayer(ChessBoard board, bool white, Material material)
            : base(white, material)
        {
        }

        protected override void FirstSquareSelected(ChessBoard board, SquareCoordinates coord, MoveSet moves)
        {
        }

        protected override void MadeMove(ChessBoard board, ChessPiece movedPiece, SquareCoordinates from, SquareCoordinates to, MoveSet moves)
        {
        }

        protected override void PerformedCastle(ChessBoard board, bool kingSide, MoveSet moves)
        {
        }

        protected override void Unselected(ChessBoard board, SquareCoordinates coord, MoveSet moves)
        {
        }

        internal override bool IsLocal()
        {
            return false;
        }

        internal override void Dispose()
        {
        }
    }
}
