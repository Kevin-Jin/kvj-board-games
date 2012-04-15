using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KvjBoardGames;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;
using KvjBoardGames.OnlineFunctions;
using System.Net;

namespace KvjChess
{
    static class Launch
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartGameMenu(new ChessBoard(), false));
        }
    }

    internal struct BoardIndices
    {
        private int row, col;
        internal int Row { get { return row; } }
        internal int Column { get { return col; } }

        internal BoardIndices(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        internal BoardIndices(SquareCoordinates coord, bool whiteOnBottom)
        {
            this.row = whiteOnBottom ? (7 - coord.Rank) : coord.Rank;
            this.col = whiteOnBottom ? coord.File : (7 - coord.File);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;
            if (obj == null || !(obj is BoardIndices))
                return false;
            BoardIndices other = (BoardIndices)obj;
            return (this.row == other.row && this.col == other.col) ;
        }

        public override int GetHashCode()
        {
            return row * 8 + col;
        }

        public static bool operator ==(BoardIndices a, BoardIndices b)
        {
            return a.row == b.row && a.col == b.col;
        }

        public static bool operator !=(BoardIndices a, BoardIndices b)
        {
            return a.row != b.row || a.col != b.col;
        }
    }

    internal struct SquareCoordinates
    {
        private int file, rank;
        internal int File { get { return file; } set { file = value; } }
        internal int Rank { get { return rank; } set { rank = value; } }

        internal SquareCoordinates(int file, int rank)
        {
            this.file = file;
            this.rank = rank;
        }

        internal SquareCoordinates(bool whiteOnBottom, BoardIndices loc)
        {
            file = whiteOnBottom ? loc.Column : (7 - loc.Column);
            rank = whiteOnBottom ? (7 - loc.Row) : loc.Row;
        }

        internal SquareCoordinates(string notation)
        {
            file = char.ToUpper(notation[0]) - 'A';
            rank = notation[1] - '1';
        }

        internal byte Serialize()
        {
            return (byte) (file * 8 + rank);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;
            if (obj == null || !(obj is SquareCoordinates))
                return false;
            SquareCoordinates other = (SquareCoordinates)obj;
            return (this.file == other.file && this.rank == other.rank);
        }

        public override int GetHashCode()
        {
            return file * 8 + rank;
        }

        public static bool operator ==(SquareCoordinates a, SquareCoordinates b)
        {
            return a.file == b.file && a.rank == b.rank;
        }

        public static bool operator !=(SquareCoordinates a, SquareCoordinates b)
        {
            return a.file != b.file || a.rank != b.rank;
        }

        public override string ToString()
        {
            return ((char)(file + 'A')).ToString() + ((char)(rank + '1')).ToString();
        }

        internal static SquareCoordinates Deserialize(byte enc)
        {
            int file = enc / 8;
            int rank = enc % 8;
            return new SquareCoordinates(file, rank);
        }
    }

    internal enum Turn
    {
        NOT_IN_SESSION = 0,
        LOCAL_WHITE = 1,
        LOCAL_BLACK = 2,
        NETWORK_OPPONENT_WHITE = 3,
        NETWORK_OPPONENT_BLACK = 4,
    }

    public class ChessBoard : GameBoard
    {
        private const int
            TOP_PAD = 5,
            LEFT_PAD = 5,
            BOTTOM_PAD = 5,
            RIGHT_PAD = 5,
            BORDER_WIDTH = 3,
            SQUARE_LENGTH = 45
        ;

        public override string BugsList
        {
            get { return "1.) Forfeiting or drawing an active network game when your opponent forcefully disconnected, or closing or restarting a finished network game when your opponent has already closed, will give you socket exceptions.\n2.) No checks made when a king approaches opponent's king, so king can capture opponent's king (and still not win the game). Kings shouldn't be able to be captured at all, period.\n3.) On networked matches, a move made while your opponent has a message box open (e.g. a draw request) will not be recognized on the opponent's board."; }
        }

        public override string TodoList
        {
            get { return "1.) Fifty-move rule -> Draw\n2.) Threefold repition -> Draw\n3.) Perpetual check -> Draw\n4.) \"Insufficient material\" -> Draw\n5.) Fortress -> Draw\n6.) Support for time limit -> Draw\n7.) King vs. King; King vs. King + Bishop; King vs. King + Knight;\nKing + Bishop vs. King + Bishop (bishops on same color squares) -> Draw\n8.) Better notifications of network opponents leaving\n9.) Local games: don't show save progress dialog if no moves have been made\n10.) Extremely long term: make an AI\n11.) Glow around square that mouse is hovering over and around square of opponent's last move.\n12.) Improve Host Network Game button so that connection is done on another thread and Host button isn't frozen when clicked. Host button should toggle to Cancel Bind when clicked. Join Network Game should end the connection attempt when the dialog box is closed, and should toggle Connect button to Cancel Attempt when it is clicked.\n13.) Add a chat box in multiplayer (also should log when opponent disconnects).\n14.) Add an undo button.\n15.) Add a button that shows the current scores.\n16.) Allow host to be given a choice to be white or black. Maybe give white to loser after each game"; }
        }

        private readonly Size size = new Size(LEFT_PAD + BORDER_WIDTH + 8 * (SQUARE_LENGTH + BORDER_WIDTH) + RIGHT_PAD, TOP_PAD + BORDER_WIDTH + 8 * (SQUARE_LENGTH + BORDER_WIDTH) + BOTTOM_PAD);
        public override Size Dimensions { get { return size; } }
        private readonly ChessPiece[,] pieces;
        private readonly Dictionary<SquareCoordinates, Color> highlightedSquares;
        private bool whiteOnBottom;
        internal bool WhiteOnBottom { get { return whiteOnBottom; } }
        private bool twoPlayers;
        private Player whitePlayer, blackPlayer;
        private NetworkInterface localInterface;
        private Turn turn;
        private ushort moveNum;
        internal ushort CurrentMove { get { return moveNum; } }

        private bool showLegal, showChecked, showBlocked, flipBoard;
        internal bool ShowLegalMoves { get { return showLegal; } }
        internal bool ShowCheckedMoves { get { return showChecked; } }
        internal bool ShowBlockedMoves { get { return showBlocked; } }
        private MenuItem castleContextMenu;
        private bool castleKingside;

        public ChessBoard()
        {
            pieces = new ChessPiece[8, 8];
            highlightedSquares = new Dictionary<SquareCoordinates, Color>();
            turn = Turn.NOT_IN_SESSION;
        }

        private void SetPiece(SquareCoordinates coord, ChessPiece piece)
        {
            pieces[coord.File, coord.Rank] = piece;
        }

        private void ClearAllSquares()
        {
            for (int file = 0; file < 8; file++)
                for (int rank = 0; rank < 8; rank++)
                    pieces[file, rank] = null;
        }

        private Material[] SetDefaultPieces()
        {
            //reset all pieces in case we played on this board before
            ClearAllSquares();

            List<SquareCoordinates> wPieces = new List<SquareCoordinates>(15), bPieces = new List<SquareCoordinates>(15);
            SquareCoordinates wKing, bKing;
            SquareCoordinates coord;
            SetPiece(coord = new SquareCoordinates("A1"), new ChessPiece(true, PieceType.ROOK));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("B1"), new ChessPiece(true, PieceType.KNIGHT));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("C1"), new ChessPiece(true, PieceType.BISHOP));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("D1"), new ChessPiece(true, PieceType.QUEEN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("E1"), new ChessPiece(true, PieceType.KING));
            wKing = coord;
            SetPiece(coord = new SquareCoordinates("F1"), new ChessPiece(true, PieceType.BISHOP));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("G1"), new ChessPiece(true, PieceType.KNIGHT));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("H1"), new ChessPiece(true, PieceType.ROOK));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("A2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("B2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("C2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("D2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("E2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("F2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("G2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("H2"), new ChessPiece(true, PieceType.PAWN));
            wPieces.Add(coord);

            SetPiece(coord = new SquareCoordinates("A8"), new ChessPiece(false, PieceType.ROOK));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("B8"), new ChessPiece(false, PieceType.KNIGHT));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("C8"), new ChessPiece(false, PieceType.BISHOP));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("D8"), new ChessPiece(false, PieceType.QUEEN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("E8"), new ChessPiece(false, PieceType.KING));
            bKing = coord;
            SetPiece(coord = new SquareCoordinates("F8"), new ChessPiece(false, PieceType.BISHOP));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("G8"), new ChessPiece(false, PieceType.KNIGHT));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("H8"), new ChessPiece(false, PieceType.ROOK));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("A7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("B7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("C7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("D7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("E7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("F7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("G7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);
            SetPiece(coord = new SquareCoordinates("H7"), new ChessPiece(false, PieceType.PAWN));
            bPieces.Add(coord);

            return new Material[] { new Material(wPieces, wKing), new Material(bPieces, bKing) };
        }

        private Material[] GetMaterial()
        {
            List<SquareCoordinates> wPieces = new List<SquareCoordinates>(), bPieces = new List<SquareCoordinates>();
            SquareCoordinates wKing = new SquareCoordinates(-1, -1), bKing = new SquareCoordinates(-1, -1);
            ChessPiece piece;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if ((piece = pieces[i, j]) != null)
                        if (piece.White)
                            if (piece.IsA(PieceType.KING))
                                wKing = new SquareCoordinates(i, j);
                            else
                                wPieces.Add(new SquareCoordinates(i, j));
                        else
                            if (piece.IsA(PieceType.KING))
                                bKing = new SquareCoordinates(i, j);
                            else
                                bPieces.Add(new SquareCoordinates(i, j));

            if (wKing.File == -1 && wKing.Rank == -1)
                throw new ApplicationException("White king not found in save file.");
            if (bKing.File == -1 && bKing.Rank == -1)
                throw new ApplicationException("Black king not found in save file.");

            return new Material[] { new Material(wPieces, wKing), new Material(bPieces, bKing) };
        }

        private bool IsSquareWhite(BoardIndices loc)
        {
            //simplified from (row % 2 == 0 && col % 2 == 0 || row % 2 == 1 && col % 2 == 1);
            return (loc.Row % 2 == 0 ^ loc.Column % 2 == 1);
        }

        private void DrawSquare(Graphics g, BoardIndices loc)
        {
            using (Brush b = new SolidBrush(IsSquareWhite(loc) ? Color.LightGray : Color.Gray))
            {
                g.FillRectangle(b, new Rectangle(GetLeftmostX(loc.Column), GetTopmostY(loc.Row), SQUARE_LENGTH, SQUARE_LENGTH));
            }
        }

        private void DrawBoard(Graphics g)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    DrawSquare(g, new BoardIndices(i, j));
        }

        private void DrawPiece(Graphics g, BoardIndices loc, bool white, PieceType type)
        {
            Image img = white ? ChessPieceImageFactory.GetWhitePiece(type) : ChessPieceImageFactory.GetBlackPiece(type);
            int iconWidth = img.Width, iconHeight = img.Height;
            g.DrawImage(img,
                    GetLeftmostX(loc.Column) + SQUARE_LENGTH / 2 - iconWidth / 2,
                    GetTopmostY(loc.Row) + SQUARE_LENGTH / 2 - iconHeight / 2, iconWidth, iconHeight);
        }

        private void DrawPieces(Graphics g)
        {
            ChessPiece p;
            for (int file = 0; file < 8; file++)
                for (int rank = 0; rank < 8; rank++)
                    if ((p = pieces[file, rank]) != null)
                        DrawPiece(g, new BoardIndices(new SquareCoordinates(file, rank), whiteOnBottom), p.White, p.Type);
        }

        private void DrawSquareBorders(Graphics g)
        {
            foreach (KeyValuePair<SquareCoordinates, Color> entry in highlightedSquares)
                ChangeSquareBorderColor(g, entry.Value, new BoardIndices(entry.Key, whiteOnBottom));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Graphics g = e.Graphics)
            {
                DrawBoard(g);
                DrawPieces(g);
                DrawSquareBorders(g);
            }
        }

        protected override void MouseClicked(object sender, MouseEventArgs e)
        {
            int x = e.X, y = e.Y;
            int leftBound = LEFT_PAD + BORDER_WIDTH, rightBound = LEFT_PAD + 8 * (SQUARE_LENGTH + BORDER_WIDTH);
            int topBound = TOP_PAD + BORDER_WIDTH, bottomBound = TOP_PAD + 8 * (SQUARE_LENGTH + BORDER_WIDTH);
            if (x >= leftBound && x <= rightBound &&
                y >= topBound && y <= bottomBound)
            {
                x -= leftBound;
                y -= topBound;
                int row = y / (SQUARE_LENGTH + BORDER_WIDTH);
                int col = x / (SQUARE_LENGTH + BORDER_WIDTH);
                int subX = x - col * (SQUARE_LENGTH + BORDER_WIDTH), subY = y - row * (SQUARE_LENGTH + BORDER_WIDTH);

                //if we're not in a border (space b/w two squares), then select the piece
                if (subX < ChessBoard.SQUARE_LENGTH && subY < ChessBoard.SQUARE_LENGTH)
                {
                    SquareCoordinates coord = new SquareCoordinates(whiteOnBottom, new BoardIndices(row, col));
                    if (e.Button == MouseButtons.Left)
                    {
                        if (turn == Turn.LOCAL_WHITE)
                            whitePlayer.Select(this, coord);
                        else if (turn == Turn.LOCAL_BLACK)
                            blackPlayer.Select(this, coord);
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        if (turn == Turn.LOCAL_WHITE)
                        {
                            if (GameLogic.CanCastle(this, coord, whitePlayer))
                            {
                                castleContextMenu.Visible = true;
                                castleKingside = (coord.File == 7);
                            }
                        }
                        else if (turn == Turn.LOCAL_BLACK)
                        {
                            if (GameLogic.CanCastle(this, coord, blackPlayer))
                            {
                                castleContextMenu.Visible = true;
                                castleKingside = (coord.File == 7);
                            }
                        }
                    }
                }
            }
        }

        internal void UpdateLastMove(SquareCoordinates from, Move m, ChessPiece moved, ChessPiece taken)
        {
            moveLabel.Text = MoveRecorder.GetMoveNotation(from, m, moved, taken);
            if (m.Overtaken)
                WaitingPlayer().Material.Pieces.Remove(m.TakenPiece);
        }

        internal void UpdateLastMoveFromCastle(bool kingSide)
        {
            moveLabel.Text = MoveRecorder.GetMoveNotationForCastle(kingSide);
        }

        internal void NextTurn()
        {
            switch (turn)
            {
                case Turn.LOCAL_WHITE:
                    turnLabel.Text = "Black's turn.";
                    if (twoPlayers)
                    {
                        turn = Turn.LOCAL_BLACK;
                        moveNum++;
                        if (flipBoard)
                        {
                            whiteOnBottom = false;
                            Redraw();
                        }
                        switch (GameLogic.KingConcern(this, blackPlayer))
                        {
                            case KingStatus.CHECK:
                                MessageBox.Show(this, "Black king in check.", "King Threat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.STALEMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "½-½";
                                turnLabel.Text = "Draw.";
                                MessageBox.Show(this, "Black stalemated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.CHECKMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "1-0";
                                turnLabel.Text = "White won.";
                                MessageBox.Show(this, "Black king checkedmated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                        }
                    }
                    else
                    {
                        turn = Turn.NETWORK_OPPONENT_BLACK;
                        moveNum++;
                        switch (GameLogic.KingConcern(this, blackPlayer))
                        {
                            case KingStatus.CHECK:
                                MessageBox.Show(this, "You have checked the opponent king.", "King Threat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.STALEMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "½-½";
                                turnLabel.Text = "Draw.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Opponent stalemated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.CHECKMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "1-0";
                                turnLabel.Text = "White won.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Opponent checkmated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                        }
                    }
                    break;
                case Turn.LOCAL_BLACK:
                    turnLabel.Text = "White's turn.";
                    if (twoPlayers)
                    {
                        turn = Turn.LOCAL_WHITE;
                        moveNum++;
                        if (flipBoard)
                        {
                            whiteOnBottom = true;
                            Redraw();
                        }
                        switch (GameLogic.KingConcern(this, whitePlayer))
                        {
                            case KingStatus.CHECK:
                                MessageBox.Show(this, "White king in check.", "King Threat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.STALEMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "½-½";
                                turnLabel.Text = "Draw.";
                                MessageBox.Show(this, "White stalemated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.CHECKMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "0-1";
                                turnLabel.Text = "Black won.";
                                MessageBox.Show(this, "White king checkmated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                        }
                    }
                    else
                    {
                        turn = Turn.NETWORK_OPPONENT_WHITE;
                        moveNum++;
                        switch (GameLogic.KingConcern(this, whitePlayer))
                        {
                            case KingStatus.CHECK:
                                MessageBox.Show(this, "You have checked the opponent king.", "King Threat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.STALEMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "½-½";
                                turnLabel.Text = "Draw.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Opponent stalemated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case KingStatus.CHECKMATE:
                                turn = Turn.NOT_IN_SESSION;
                                moveLabel.Text = "0-1";
                                turnLabel.Text = "Black won.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Opponent checkmated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                        }
                    }
                    break;
                case Turn.NETWORK_OPPONENT_WHITE:
                    turnLabel.Text = "Black's turn.";
                    turn = Turn.LOCAL_BLACK;
                    moveNum++;
                    switch (GameLogic.KingConcern(this, blackPlayer))
                    {
                        case KingStatus.CHECK:
                            MessageBox.Show(this, "Your king is in check.", "King Threat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case KingStatus.STALEMATE:
                            turn = Turn.NOT_IN_SESSION;
                            moveLabel.Text = "½-½";
                            turnLabel.Text = "Draw.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "You are stalemated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case KingStatus.CHECKMATE:
                            turn = Turn.NOT_IN_SESSION;
                            moveLabel.Text = "1-0";
                            turnLabel.Text = "White won.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "You were checkmated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                    }
                    break;
                case Turn.NETWORK_OPPONENT_BLACK:
                    turnLabel.Text = "White's turn.";
                    turn = Turn.LOCAL_WHITE;
                    moveNum++;
                    switch (GameLogic.KingConcern(this, whitePlayer))
                    {
                        case KingStatus.CHECK:
                            MessageBox.Show(this, "Your king is in check.", "King Threat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case KingStatus.STALEMATE:
                            turn = Turn.NOT_IN_SESSION;
                            moveLabel.Text = "½-½";
                            turnLabel.Text = "Draw.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "You are stalemated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case KingStatus.CHECKMATE:
                            turn = Turn.NOT_IN_SESSION;
                            moveLabel.Text = "0-1";
                            turnLabel.Text = "Black won.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "You were checkmated.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                    }
                    break;
            }
        }

        internal bool WillPlaceKingInCheck(Player p, SquareCoordinates from, Move move)
        {
            //temporarily moves a piece from the square 'from' to the square 'to'
            //without showing it on the board, and checks GameLogic to see if we are in check.
            //no matter the result, the board will be restored right afterwards.

            //perhaps just create a copy of the board so we don't have to restore the old pieces?
            SquareCoordinates to = move.Coordinates;
            SquareCoordinates taken = move.TakenPiece;
            ChessPiece fromPiece = pieces[from.File, from.Rank];
            ChessPiece takenPiece = pieces[taken.File, taken.Rank];
            pieces[taken.File, taken.Rank] = null;
            pieces[to.File, to.Rank] = fromPiece;
            pieces[from.File, from.Rank] = null;
            bool movedKing = fromPiece.IsA(PieceType.KING) && fromPiece.White == p.White;
            bool inCheck = GameLogic.KingInCheck(this, movedKing ? to : p.Material.KingSquare, p.White);
            pieces[from.File, from.Rank] = fromPiece;
            pieces[to.File, to.Rank] = null;
            pieces[taken.File, taken.Rank] = takenPiece;
            return inCheck;
        }

        internal bool WillPlaceKingInCheck(Player p, SquareCoordinates[] fromSquares, SquareCoordinates[] toSquares)
        {
            SquareCoordinates king = p.Material.KingSquare;
            ChessPiece[] overwrittenPieces = new ChessPiece[fromSquares.Length];
            for (int i = 0; i < fromSquares.Length; i++)
            {
                SquareCoordinates from = fromSquares[i];
                SquareCoordinates to = toSquares[i];
                ChessPiece fromPiece = pieces[from.File, from.Rank];
                overwrittenPieces[i] = pieces[to.File, to.Rank];
                pieces[to.File, to.Rank] = fromPiece;
                pieces[from.File, from.Rank] = null;
                if (fromPiece.IsA(PieceType.KING) && fromPiece.IsFriendly(p.White))
                    king = to;
            }
            bool inCheck = GameLogic.KingInCheck(this, king, p.White);
            for (int i = 0; i < fromSquares.Length; i++)
            {
                SquareCoordinates from = fromSquares[i];
                SquareCoordinates to = toSquares[i];
                ChessPiece fromPiece = pieces[to.File, to.Rank];
                pieces[from.File, from.Rank] = fromPiece;
                pieces[to.File, to.Rank] = overwrittenPieces[i];
            }
            return inCheck;
        }

        internal Player ActivePlayer()
        {
            switch (turn)
            {
                case Turn.LOCAL_WHITE:
                case Turn.NETWORK_OPPONENT_WHITE:
                    return whitePlayer;
                case Turn.LOCAL_BLACK:
                case Turn.NETWORK_OPPONENT_BLACK:
                    return blackPlayer;
                default:
                    return null;
            }
        }

        internal Player WaitingPlayer()
        {
            switch (turn)
            {
                case Turn.LOCAL_WHITE:
                case Turn.NETWORK_OPPONENT_WHITE:
                    return blackPlayer;
                case Turn.LOCAL_BLACK:
                case Turn.NETWORK_OPPONENT_BLACK:
                    return whitePlayer;
                default:
                    return null;
            }
        }

        internal ChessPiece ClearSquare(SquareCoordinates coord)
        {
            ChessPiece removed = GetPiece(coord);
            SetPiece(coord, null);
            using (Graphics g = this.CreateGraphics())
            {
                DrawSquare(g, new BoardIndices(coord, whiteOnBottom));
            }
            return removed;
        }

        internal void DrawPiece(SquareCoordinates coord, ChessPiece piece)
        {
            SetPiece(coord, piece);
            using (Graphics g = this.CreateGraphics())
            {
                DrawPiece(g, new BoardIndices(coord, whiteOnBottom), piece.White, piece.Type);
            }
        }

        internal ChessPiece GetPiece(SquareCoordinates coord)
        {
            return pieces[coord.File, coord.Rank];
        }

        private void ChangeSquareBorderColor(Graphics g, Color color, BoardIndices loc)
        {
            int width = Math.Max(BORDER_WIDTH / 2, 1);
            using (Pen p = new Pen(color))
            {
                g.DrawRectangle(p, new Rectangle(GetLeftmostX(loc.Column) - width, GetTopmostY(loc.Row) - width, SQUARE_LENGTH + width, SQUARE_LENGTH + width));
            }
        }

        internal void HighlightSquare(SquareCoordinates coord, Color color)
        {
            highlightedSquares[coord] = color;
            using (Graphics g = this.CreateGraphics())
            {
                ChangeSquareBorderColor(g, color, new BoardIndices(coord, whiteOnBottom));
            }
        }

        internal void UnhighlightSquare(SquareCoordinates coord)
        {
            highlightedSquares.Remove(coord);
            using (Graphics g = this.CreateGraphics())
            {
                ChangeSquareBorderColor(g, BackColor, new BoardIndices(coord, whiteOnBottom));
            }
        }

        private void Castle(object o, EventArgs a)
        {
            if (turn == Turn.LOCAL_WHITE)
                whitePlayer.Castle(this, castleKingside);
            else if (turn == Turn.LOCAL_BLACK)
                blackPlayer.Castle(this, castleKingside);
        }

        protected override void GameWindowAssociated()
        {
            showLegal = true;
            showChecked = true;
            showBlocked = true;
            flipBoard = false;

            mnu.AddMenuEntry("Options");
            mnu.AddSubmenuEntry("Options", "Legal Move Highlighting", new EventHandler(delegate(object o, EventArgs a) { showLegal = !showLegal; ((ToolStripMenuItem)o).Checked = showLegal; }), showLegal);
            mnu.AddSubmenuEntry("Options", "Checked Move Highlighting", new EventHandler(delegate(object o, EventArgs a) { showChecked = !showChecked; ((ToolStripMenuItem)o).Checked = showChecked; }), showChecked);
            mnu.AddSubmenuEntry("Options", "Blocked Move Highlighting", new EventHandler(delegate(object o, EventArgs a) { showBlocked = !showBlocked; ((ToolStripMenuItem)o).Checked = showBlocked; }), showBlocked);
            mnu.AddSubmenuEntry("Options", "Flip Board Each Turn", new EventHandler(delegate(object o, EventArgs a) { flipBoard = !flipBoard; ((ToolStripMenuItem)o).Checked = flipBoard; }), flipBoard);
            mnu.Commit();

            castleContextMenu = new MenuItem("Castle", new EventHandler(Castle));
            castleContextMenu.Visible = false;
            this.ContextMenu = new ContextMenu(new MenuItem[] { castleContextMenu });
            this.ContextMenu.Collapse += new EventHandler(delegate(object o, EventArgs a) { castleContextMenu.Visible = false; });
        }

        private void comm_Disconnected()
        {
            turn = Turn.NOT_IN_SESSION;
            moveLabel.Text = "";
            turnLabel.Text = "Opponent disconnected.";
        }

        public override void SetupServer(NetworkInterface comm)
        {
            comm.Connected += new OpponentConnected(ClientConnected);
            localInterface = comm;
            turn = Turn.NOT_IN_SESSION;
            moveLabel.Text = "";
            turnLabel.Text = "Waiting for opponent to connect.";
            mnu.DisableEntry("Options", "Flip Board Each Turn");
            NewGameMenuUpdate(false, false, false);
        }

        public override void PlayLocal()
        {
            localInterface = null;
            Material[] pieces = SetDefaultPieces();
            highlightedSquares.Clear();
            twoPlayers = true;
            whitePlayer = new LocalPlayer(this, true, null, pieces[0]);
            blackPlayer = new LocalPlayer(this, false, null, pieces[1]);
            turn = Turn.LOCAL_WHITE;
            whiteOnBottom = true;
            moveLabel.Text = "";
            turnLabel.Text = "White's turn.";
            mnu.EnableEntry("Options", "Flip Board Each Turn");
            NewGameMenuUpdate(true, false, false);
        }

        public override void PlayFromSave()
        {
            localInterface = null;
            Material[] pieces = GetMaterial();
            highlightedSquares.Clear();
            twoPlayers = true;
            whitePlayer = new LocalPlayer(this, true, null, pieces[0]);
            blackPlayer = new LocalPlayer(this, false, null, pieces[1]);
            moveLabel.Text = "";
            turnLabel.Text = (turn == Turn.LOCAL_WHITE ? "White's" : "Black's") + " turn.";
            whiteOnBottom = !flipBoard || turn == Turn.LOCAL_WHITE;
            mnu.EnableEntry("Options", "Flip Board Each Turn");
            NewGameMenuUpdate(true, false, false);
        }

        public override void PlayServer(NetworkInterface comm)
        {
            comm.Disconnected += new OpponentDisconnected(comm_Disconnected);
            localInterface = comm;
            Material[] pieces = SetDefaultPieces();
            highlightedSquares.Clear();
            twoPlayers = false;
            NetworkPlayer p = new NetworkPlayer(this, true, pieces[0]);
            whitePlayer = p;
            blackPlayer = new LocalPlayer(this, false, comm, pieces[1]);
            comm.Handler = new ChessPacketHandler(p, this);
            turn = Turn.NETWORK_OPPONENT_WHITE;
            moveLabel.Text = "";
            turnLabel.Text = "White's turn.";
            whiteOnBottom = false;
            mnu.DisableEntry("Options", "Flip Board Each Turn");
            NewGameMenuUpdate(false, true, true);
        }

        public override void PlayClient(NetworkInterface comm)
        {
            comm.Disconnected += new OpponentDisconnected(comm_Disconnected);
            Material[] pieces = SetDefaultPieces();
            highlightedSquares.Clear();
            twoPlayers = false;
            whitePlayer = new LocalPlayer(this, true, comm, pieces[0]);
            NetworkPlayer p = new NetworkPlayer(this, false, pieces[1]);
            blackPlayer = p;
            comm.Handler = new ChessPacketHandler(p, this);
            turn = Turn.LOCAL_WHITE;
            moveLabel.Text = "";
            turnLabel.Text = "White's turn.";
            whiteOnBottom = true;
            mnu.DisableEntry("Options", "Flip Board Each Turn");
            NewGameMenuUpdate(false, true, true);

            Redraw();
        }

        private void ClientConnected(NetworkInterface comm, EndPoint remoteAddress)
        {
            Console.WriteLine("Cliented connected from " + remoteAddress);
            PlayClient(comm);
        }

        internal void ResetBoard()
        {
            Material[] pieces = SetDefaultPieces();
            highlightedSquares.Clear();
            whitePlayer.Reset(pieces[0]);
            blackPlayer.Reset(pieces[1]);
            turn = whitePlayer.IsLocal() ? Turn.LOCAL_WHITE : Turn.NETWORK_OPPONENT_WHITE;
            moveLabel.Text = "";
            turnLabel.Text = "White's turn.";
            if (twoPlayers)
                whiteOnBottom = true;
            else
                NewGameMenuUpdate(false, true, true);
            Redraw();
        }

        public override void NewGame()
        {
            ResetBoard();
            if (!twoPlayers)
                localInterface.SendMessage(CommonPacketWriter.WriteResetGame());
        }

        public override void OpenGame()
        {
            PlayFromSave();
            Redraw();
        }

        public override void SendDraw()
        {
            localInterface.SendMessage(CommonPacketWriter.WriteDrawRequest());
        }

        internal void ReceivedDraw()
        {
            bool agree = MessageBox.Show(this, "Opponent requested a draw. Agree?", "Draw Request", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
            localInterface.SendMessage(CommonPacketWriter.WriteDrawResponse(agree)); 
            if (agree)
                DoDraw();
        }

        internal void DoDraw()
        {
            turn = Turn.NOT_IN_SESSION;
            moveLabel.Text = "½-½";
            turnLabel.Text = "Draw.";
            MessageBox.Show(this, "Draw mutually agreed.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NewGameMenuUpdate(true, false, false);
        }

        internal void DrawDenied()
        {
            MessageBox.Show(this, "Opponent denied draw request.", "Draw Rejected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal void ReceivedForfeit()
        {
            turn = Turn.NOT_IN_SESSION;
            bool whiteForfeited = blackPlayer.IsLocal();
            moveLabel.Text = whiteForfeited ? "0-1" : "1-0";
            turnLabel.Text = (whiteForfeited ? "Black" : "White") + " won.";
            MessageBox.Show(this, (whiteForfeited ? "White" : " Black") + " resigned.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NewGameMenuUpdate(true, false, false);
        }

        public override void SendForfeit()
        {
            turn = Turn.NOT_IN_SESSION;
            bool whiteForfeited = whitePlayer.IsLocal();
            moveLabel.Text = whiteForfeited ? "0-1" : "1-0";
            turnLabel.Text = (whiteForfeited ? "Black" : "White") + " won.";
            localInterface.SendMessage(CommonPacketWriter.WriteForfeit());
            NewGameMenuUpdate(true, false, false);
        }

        public override bool InProgress()
        {
            return turn != Turn.NOT_IN_SESSION;
        }

        public override void CleanUp()
        {
            if (localInterface != null)
                localInterface.Disconnect();
            if (whitePlayer != null)
                whitePlayer.Dispose();
            if (blackPlayer != null)
                blackPlayer.Dispose();
        }

        public override byte[] Serialize()
        {
            ChessPiece piece;
            byte[] serialized = new byte[64 * 4 + 1 + 2];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if ((piece = pieces[i, j]) != null)
                        ByteTools.WriteBytes(serialized, (i * 8 + j) * 4, piece.Serialize());
            serialized[64 * 4] = (byte)turn;
            ByteTools.WriteUint16(serialized, 64 * 4 + 1, moveNum);
            return serialized;
        }

        public override void Deserialize(byte[] serialized)
        {
            if (serialized.Length != (64 * 4 + 1 + 2))
                return; //unrecognized encoding
            for (int i = 0; i < 64; i++)
                pieces[i / 8, i % 8] = ChessPiece.Deserialize(ByteTools.ReadBytes(serialized, i * 4, 4));
            turn = (Turn)serialized[64 * 4];
            moveNum = ByteTools.ReadUint16(serialized, 64 * 4 + 1);
        }

        private static int GetLeftmostX(int col)
        {
            return LEFT_PAD + BORDER_WIDTH + col * (SQUARE_LENGTH + BORDER_WIDTH);
        }

        private static int GetTopmostY(int row)
        {
            return TOP_PAD + BORDER_WIDTH + row * (SQUARE_LENGTH + BORDER_WIDTH);
        }
    }
}
