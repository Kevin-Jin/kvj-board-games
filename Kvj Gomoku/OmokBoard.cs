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

namespace KvjGomoku
{
    static class Launch
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartGameMenu(new OmokBoard(), false));
        }
    }

    internal enum Turn
    {
        NOT_IN_SESSION = 0,
        LOCAL_BLACK = 1,
        LOCAL_WHITE = 2,
        NETWORK_OPPONENT_BLACK = 3,
        NETWORK_OPPONENT_WHITE = 4,
    }

    internal enum PieceColor
    {
        EMPTY = 0,
        BLACK = 1,
        WHITE = 2
    }

    public class OmokBoard : GameBoard
    {
        internal const int
            ROWS = 15,
            COLUMNS = 15,
            TOP_PAD = 5,
            LEFT_PAD = 5,
            BOTTOM_PAD = 5,
            RIGHT_PAD = 5,
            PIECE_WIDTH = 16,
            PIECE_OUTLINE = 2,
            LINE_THICKNESS = 3,
            SQUARE_LENGTH = 32
        ;

        public override string BugsList
        {
            get { return "1.) Rule of three and three does not check\nif both rows are open."; }
        }

        public override string TodoList
        {
            get { return "1.) Better notifications of network opponents leaving\n2.) Local games: don't show save progress dialog if no moves have been made\n3.) Glow around square that mouse is hovering over, around square of opponent's last move, and around line of winning move at end of game."; }
        }

        private readonly Size size = new Size(LEFT_PAD + COLUMNS * SQUARE_LENGTH + RIGHT_PAD, TOP_PAD + ROWS * SQUARE_LENGTH + BOTTOM_PAD);
        public override Size Dimensions { get { return size; } }
        private readonly PieceColor[,] pieces;
        private bool twoPlayers;
        private Player blackPlayer, whitePlayer;
        private NetworkInterface localInterface;
        private Turn turn;

        public OmokBoard()
        {
            pieces = new PieceColor[ROWS, COLUMNS];
            turn = Turn.NOT_IN_SESSION;
        }

        private void ClearAllSquares()
        {
            for (int i = 0; i < ROWS; i++)
                for (int j = 0; j < COLUMNS; j++)
                    pieces[i, j] = PieceColor.EMPTY;
        }

        internal PieceColor GetPiece(int row, int col)
        {
            return pieces[row, col];
        }

        protected override void GameWindowAssociated()
        {
        }

        private void ClientConnected(NetworkInterface comm, EndPoint remoteAddress)
        {
            Console.WriteLine("Cliented connected from " + remoteAddress);
            PlayClient(comm);
        }

        public override void SetupServer(NetworkInterface comm)
        {
            comm.Connected += new OpponentConnected(ClientConnected);
            localInterface = comm;
            turn = Turn.NOT_IN_SESSION;
            moveLabel.Text = "";
            turnLabel.Text = "Waiting for opponent to connect.";
            NewGameMenuUpdate(false, false, false);
        }

        public override void PlayLocal()
        {
            localInterface = null;
            ClearAllSquares();
            twoPlayers = true;
            blackPlayer = new LocalPlayer(PieceColor.BLACK, null);
            whitePlayer = new LocalPlayer(PieceColor.WHITE, null);
            turn = Turn.LOCAL_BLACK;
            moveLabel.Text = "";
            turnLabel.Text = "Black's turn.";
            NewGameMenuUpdate(true, false, false);
        }

        public override void PlayFromSave()
        {
            localInterface = null;
            twoPlayers = true;
            blackPlayer = new LocalPlayer(PieceColor.BLACK, null);
            whitePlayer = new LocalPlayer(PieceColor.WHITE, null);
            moveLabel.Text = "";
            turnLabel.Text = (turn == Turn.LOCAL_BLACK ? "Black" : "White") + "'s turn.";
            NewGameMenuUpdate(true, false, false);
        }

        private void comm_Disconnected()
        {
            turn = Turn.NOT_IN_SESSION;
            turnLabel.Text = "Opponent disconnected.";
        }

        public override void PlayServer(NetworkInterface comm)
        {
            comm.Disconnected += new OpponentDisconnected(comm_Disconnected);
            localInterface = comm;
            ClearAllSquares();
            twoPlayers = false;
            NetworkPlayer p = new NetworkPlayer(PieceColor.BLACK);
            blackPlayer = p;
            whitePlayer = new LocalPlayer(PieceColor.WHITE, comm);
            comm.Handler = new OmokPacketHandler(p, this);
            turn = Turn.NETWORK_OPPONENT_BLACK;
            moveLabel.Text = "";
            turnLabel.Text = "Black's turn.";
            NewGameMenuUpdate(false, true, true);
        }

        public override void PlayClient(NetworkInterface comm)
        {
            comm.Disconnected += new OpponentDisconnected(comm_Disconnected);
            ClearAllSquares();
            twoPlayers = false;
            blackPlayer = new LocalPlayer(PieceColor.BLACK, comm);
            NetworkPlayer p = new NetworkPlayer(PieceColor.WHITE);
            whitePlayer = p;
            comm.Handler = new OmokPacketHandler(p, this);
            turn = Turn.LOCAL_BLACK;
            moveLabel.Text = "";
            turnLabel.Text = "Black's turn.";
            NewGameMenuUpdate(false, true, true);

            Redraw();
        }

        internal void ResetBoard()
        {
            ClearAllSquares();
            turn = blackPlayer.IsLocal() ? Turn.LOCAL_BLACK : Turn.NETWORK_OPPONENT_BLACK;
            moveLabel.Text = "";
            turnLabel.Text = "Black's turn.";
            if (!twoPlayers)
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
            bool blackForfeited = whitePlayer.IsLocal();
            turnLabel.Text = (blackForfeited ? "White" : "Black") + " won.";
            MessageBox.Show(this, (blackForfeited ? "Black" : " White") + " forfeited.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NewGameMenuUpdate(true, false, false);
        }

        public override void SendForfeit()
        {
            turn = Turn.NOT_IN_SESSION;
            bool blackForfeited = blackPlayer.IsLocal();
            turnLabel.Text = (blackForfeited ? "White" : "Black") + " won.";
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
            byte[] serialized = new byte[ROWS * COLUMNS + 1];
            for (int i = 0; i < ROWS; i++)
                for (int j = 0; j < COLUMNS; j++)
                    serialized[i * COLUMNS + j] = (byte) pieces[i, j];
            serialized[ROWS * COLUMNS] = (byte)turn;
            return serialized;
        }

        public override void Deserialize(byte[] bytes)
        {
            if (bytes.Length != ROWS * COLUMNS + 1)
                return; //unrecognized encoding
            for (int i = 0; i < ROWS * COLUMNS; i++)
                pieces[i / COLUMNS, i % COLUMNS] = (PieceColor)bytes[i];
            turn = (Turn)bytes[ROWS * COLUMNS];
        }

        private void DrawSquare(Graphics g, int row, int col)
        {
            using (Brush b = new SolidBrush(Color.Ivory))
            {
                g.FillRectangle(b, LEFT_PAD + col * SQUARE_LENGTH, TOP_PAD + row * SQUARE_LENGTH, SQUARE_LENGTH, SQUARE_LENGTH);
            }
            using (Pen p = new Pen(Color.Black, LINE_THICKNESS))
            {
                g.DrawLine(p, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2, TOP_PAD + row * SQUARE_LENGTH, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH);
                g.DrawLine(p, LEFT_PAD + col * SQUARE_LENGTH, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH / 2, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH / 2);
            }
        }

        private void DrawBoard(Graphics g)
        {
            for (int i = 0; i < ROWS; i++)
                for (int j = 0; j < COLUMNS; j++)
                    DrawSquare(g, i, j);
        }

        private void DrawPiece(Graphics g, int row, int col, PieceColor color)
        {
            Color c = Color.Empty;
            switch (color)
            {
                case PieceColor.BLACK:
                    c = Color.Black;
                    break;
                case PieceColor.WHITE:
                    c = Color.White;
                    break;
            }
            using (Brush b = new SolidBrush(c))
            {
                using (Pen outline = new Pen(Color.Black, PIECE_OUTLINE))
                {
                    g.DrawEllipse(outline, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, PIECE_WIDTH, PIECE_WIDTH);
                }
                g.FillEllipse(b, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, PIECE_WIDTH, PIECE_WIDTH);
            }
        }

        private void DrawPieces(Graphics g)
        {
            for (int i = 0; i < ROWS; i++)
                for (int j = 0; j < COLUMNS; j++)
                    if (pieces[i, j] != PieceColor.EMPTY)
                        DrawPiece(g, i, j, pieces[i, j]);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Graphics g = e.Graphics)
            {
                DrawBoard(g);
                DrawPieces(g);
            }
        }

        protected override void MouseClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X, y = e.Y;
                int leftBound = LEFT_PAD, rightBound = LEFT_PAD + COLUMNS * SQUARE_LENGTH;
                int topBound = TOP_PAD, bottomBound = TOP_PAD + ROWS * SQUARE_LENGTH;
                if (x >= leftBound && x <= rightBound &&
                    y >= topBound && y <= bottomBound)
                {
                    x -= leftBound;
                    y -= topBound;
                    int row = y / SQUARE_LENGTH;
                    int col = x / SQUARE_LENGTH;
                    if (turn == Turn.LOCAL_BLACK)
                        blackPlayer.Select(this, row, col);
                    else if (turn == Turn.LOCAL_WHITE)
                        whitePlayer.Select(this, row, col);
                }
            }
        }

        internal void DrawPiece(int row, int col, PieceColor color)
        {
            pieces[row, col] = color;
            using (Graphics g = this.CreateGraphics())
            {
                DrawPiece(g, row, col, color);
            }
        }

        internal void NextTurn(Point move)
        {
            switch (turn)
            {
                case Turn.LOCAL_BLACK:
                    turnLabel.Text = "White's turn.";
                    if (twoPlayers)
                    {
                        switch (GameLogic.CurrentStatus(this, blackPlayer, move))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FIVE_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Black won.";
                                MessageBox.Show(this, "Black got five in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.LOCAL_WHITE;
                                break;
                        }
                    }
                    else
                    {
                        switch (GameLogic.CurrentStatus(this, blackPlayer, move))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FIVE_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Black won.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "You got five in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.NETWORK_OPPONENT_WHITE;
                                break;
                        }
                    }
                    break;
                case Turn.LOCAL_WHITE:
                    turnLabel.Text = "Black's turn.";
                    if (twoPlayers)
                    {
                        switch (GameLogic.CurrentStatus(this, whitePlayer, move))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FIVE_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "White won.";
                                MessageBox.Show(this, "White got five in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.LOCAL_BLACK;
                                break;
                        }
                    }
                    else
                    {
                        switch (GameLogic.CurrentStatus(this, whitePlayer, move))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FIVE_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "White won.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "You got five in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.NETWORK_OPPONENT_BLACK;
                                break;
                        }
                    }
                    break;
                case Turn.NETWORK_OPPONENT_BLACK:
                    switch (GameLogic.CurrentStatus(this, blackPlayer, move))
                    {
                        case MoveResult.BOARD_FILLED:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "Draw.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case MoveResult.FIVE_IN_A_ROW:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "Black won.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Opponent got five in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            turn = Turn.LOCAL_WHITE;
                            turnLabel.Text = "White's turn.";
                            break;
                    }
                    break;
                case Turn.NETWORK_OPPONENT_WHITE:
                    switch (GameLogic.CurrentStatus(this, whitePlayer, move))
                    {
                        case MoveResult.BOARD_FILLED:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "Draw.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case MoveResult.FIVE_IN_A_ROW:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "White won.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Opponent got five in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            turn = Turn.LOCAL_BLACK;
                            turnLabel.Text = "Black's turn.";
                            break;
                    }
                    break;
            }
        }
    }
}
