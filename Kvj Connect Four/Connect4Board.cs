using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KvjBoardGames;
using System.Drawing;
using KvjBoardGames.OnlineFunctions;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace KvjConnectFour
{
    static class Launch
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartGameMenu(new Connect4Board(), false));
        }
    }

    internal enum PieceColor
    {
        EMPTY = 0,
        YELLOW = 1,
        RED = 2
    }

    internal class Connect4Column
    {
        private readonly PieceColor[] pieces;
        private int top;
        internal bool Full { get { return top >= Connect4Board.ROWS; } }
        internal int LastDropRow { get { return top - 1; } }

        internal Connect4Column()
        {
            pieces = new PieceColor[Connect4Board.ROWS];
            top = 0;
        }

        internal PieceColor this[int i]
        {
            get { return pieces[i]; }
        }

        internal void Drop(PieceColor piece)
        {
            pieces[top++] = piece;
        }

        internal void Clear()
        {
            for (int i = 0; i < pieces.Length; i++)
                pieces[i] = 0;
            top = 0;
        }
    }

    internal enum Turn
    {
        NOT_IN_SESSION = 0,
        LOCAL_YELLOW = 1,
        LOCAL_RED = 2,
        NETWORK_OPPONENT_YELLOW = 3,
        NETWORK_OPPONENT_RED = 4,
    }

    public class Connect4Board : GameBoard
    {
        internal const int
            ROWS = 6,
            COLUMNS = 7,
            TOP_PAD = 5,
            LEFT_PAD = 5,
            BOTTOM_PAD = 5,
            RIGHT_PAD = 5,
            PIECE_WIDTH = 54,
            PIECE_OUTLINE = 2,
            LINE_THICKNESS = 4,
            SQUARE_LENGTH = 64
        ;

        public override string BugsList
        {
            get { return ""; }
        }

        public override string TodoList
        {
            get { return "1.) Better notifications of network opponents leaving\n2.) Local games: don't show save progress dialog if no moves have been made\n3.) Glow around column that mouse is hovering over, around column of opponent's last move, and around line of winning move at end of game."; }
        }

        private readonly Size size = new Size(LEFT_PAD + COLUMNS * SQUARE_LENGTH + RIGHT_PAD, TOP_PAD + ROWS * SQUARE_LENGTH + BOTTOM_PAD);
        public override Size Dimensions { get { return size; } }
        private readonly Connect4Column[] columns;
        private bool twoPlayers;
        private Player yellowPlayer, redPlayer;
        private NetworkInterface localInterface;
        private Turn turn;

        public Connect4Board()
        {
            columns = new Connect4Column[COLUMNS];
            for (int i = 0; i < columns.Length; i++)
                columns[i] = new Connect4Column();
            turn = Turn.NOT_IN_SESSION;
        }

        private void ClearAllSquares()
        {
            for (int i = 0; i < COLUMNS; i++)
                columns[i].Clear();
        }

        internal Connect4Column GetColumn(int col)
        {
            return columns[col];
        }

        internal PieceColor GetPiece(int row, int col)
        {
            return columns[col][row];
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
            yellowPlayer = new LocalPlayer(PieceColor.YELLOW, null);
            redPlayer = new LocalPlayer(PieceColor.RED, null);
            turn = Turn.LOCAL_YELLOW;
            moveLabel.Text = "";
            turnLabel.Text = "Yellow's turn.";
            NewGameMenuUpdate(true, false, false);
        }

        public override void PlayFromSave()
        {
            localInterface = null;
            twoPlayers = true;
            yellowPlayer = new LocalPlayer(PieceColor.YELLOW, null);
            redPlayer = new LocalPlayer(PieceColor.RED, null);
            moveLabel.Text = "";
            turnLabel.Text = (turn == Turn.LOCAL_YELLOW ? "Yellow" : "Red") + "'s turn.";
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
            NetworkPlayer p = new NetworkPlayer(PieceColor.YELLOW);
            yellowPlayer = p;
            redPlayer = new LocalPlayer(PieceColor.RED, comm);
            comm.Handler = new Connect4PacketHandler(p, this);
            turn = Turn.NETWORK_OPPONENT_YELLOW;
            moveLabel.Text = "";
            turnLabel.Text = "Yellow's turn.";
            NewGameMenuUpdate(false, true, true);
        }

        public override void PlayClient(NetworkInterface comm)
        {
            comm.Disconnected += new OpponentDisconnected(comm_Disconnected);
            ClearAllSquares();
            twoPlayers = false;
            yellowPlayer = new LocalPlayer(PieceColor.YELLOW, comm);
            NetworkPlayer p = new NetworkPlayer(PieceColor.RED);
            redPlayer = p;
            comm.Handler = new Connect4PacketHandler(p, this);
            turn = Turn.LOCAL_YELLOW;
            moveLabel.Text = "";
            turnLabel.Text = "Yellow's turn.";
            NewGameMenuUpdate(false, true, true);

            Redraw();
        }

        internal void ResetBoard()
        {
            ClearAllSquares();
            turn = yellowPlayer.IsLocal() ? Turn.LOCAL_YELLOW : Turn.NETWORK_OPPONENT_YELLOW;
            moveLabel.Text = "";
            turnLabel.Text = "Yellow's turn.";
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
            bool yellowForfeited = redPlayer.IsLocal();
            turnLabel.Text = (yellowForfeited ? "Red" : "Yellow") + " won.";
            MessageBox.Show(this, (yellowForfeited ? "Yellow" : " Red") + " forfeited.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NewGameMenuUpdate(true, false, false);
        }

        public override void SendForfeit()
        {
            turn = Turn.NOT_IN_SESSION;
            bool yellowForfeited = yellowPlayer.IsLocal();
            turnLabel.Text = (yellowForfeited ? "Red" : "Yellow") + " won.";
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
            if (redPlayer != null)
                redPlayer.Dispose();
            if (yellowPlayer != null)
                yellowPlayer.Dispose();
        }

        public override byte[] Serialize()
        {
            using (MemoryStream serialized = new MemoryStream())
            {
                for (int i = 0; i < COLUMNS; i++)
                {
                    int height = columns[i].LastDropRow + 1;
                    serialized.WriteByte((byte)height);
                    for (int j = 0; j < height; j++)
                        serialized.WriteByte((byte)columns[i][j]);
                }
                serialized.WriteByte((byte)turn);
                return serialized.ToArray();
            }
        }

        public override void Deserialize(byte[] bytes)
        {
            using (MemoryStream reader = new MemoryStream(bytes))
            {
                for (int i = 0; i < COLUMNS; i++)
                {
                    int height = reader.ReadByte();
                    for (int j = 0; j < height; j++)
                        columns[i].Drop((PieceColor)reader.ReadByte());
                }
                turn = (Turn)reader.ReadByte();
            }
        }

        private void DrawSquare(Graphics g, int row, int col)
        {
            using (Brush b = new SolidBrush(Color.Ivory))
            {
                g.FillRectangle(b, LEFT_PAD + col * SQUARE_LENGTH + LINE_THICKNESS / 2, TOP_PAD + row * SQUARE_LENGTH + LINE_THICKNESS / 2, SQUARE_LENGTH - LINE_THICKNESS, SQUARE_LENGTH - LINE_THICKNESS);
            }
            /*using (Pen p = new Pen(Color.Black, LINE_THICKNESS))
            {
                g.DrawLine(p, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2, TOP_PAD + row * SQUARE_LENGTH, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH);
                g.DrawLine(p, LEFT_PAD + col * SQUARE_LENGTH, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH / 2, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH, TOP_PAD + row * SQUARE_LENGTH + SQUARE_LENGTH / 2);
            }*/
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
                case PieceColor.YELLOW:
                    c = Color.Yellow;
                    break;
                case PieceColor.RED:
                    c = Color.Red;
                    break;
            }
            using (Brush b = new SolidBrush(c))
            {
                using (Pen outline = new Pen(Color.Black, PIECE_OUTLINE))
                {
                    g.DrawEllipse(outline, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, TOP_PAD + (ROWS - 1 - row) * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, PIECE_WIDTH, PIECE_WIDTH);
                }
                g.FillEllipse(b, LEFT_PAD + col * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, TOP_PAD + (ROWS - 1 - row) * SQUARE_LENGTH + SQUARE_LENGTH / 2 - PIECE_WIDTH / 2, PIECE_WIDTH, PIECE_WIDTH);
            }
        }

        private void DrawPieces(Graphics g)
        {
            for (int i = 0; i < ROWS; i++)
                for (int j = 0; j < COLUMNS; j++)
                    if (columns[j][i] != PieceColor.EMPTY)
                        DrawPiece(g, i, j, columns[j][i]);
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
                    int col = x / SQUARE_LENGTH;
                    if (turn == Turn.LOCAL_YELLOW)
                        yellowPlayer.Select(this, col);
                    else if (turn == Turn.LOCAL_RED)
                        redPlayer.Select(this, col);
                }
            }
        }

        internal void DrawPiece(int col, PieceColor color)
        {
            columns[col].Drop(color);
            using (Graphics g = this.CreateGraphics())
            {
                DrawPiece(g, columns[col].LastDropRow, col, color);
            }
        }

        internal void NextTurn(int col)
        {
            switch (turn)
            {
                case Turn.LOCAL_YELLOW:
                    turnLabel.Text = "Red's turn.";
                    if (twoPlayers)
                    {
                        switch (GameLogic.CurrentStatus(this, yellowPlayer, col, columns[col]))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FOUR_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Yellow won.";
                                MessageBox.Show(this, "Yellow got four in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.LOCAL_RED;
                                break;
                        }
                    }
                    else
                    {
                        switch (GameLogic.CurrentStatus(this, yellowPlayer, col, columns[col]))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FOUR_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Yellow won.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "You got four in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.NETWORK_OPPONENT_RED;
                                break;
                        }
                    }
                    break;
                case Turn.LOCAL_RED:
                    turnLabel.Text = "Yellow's turn.";
                    if (twoPlayers)
                    {
                        switch (GameLogic.CurrentStatus(this, redPlayer, col, columns[col]))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FOUR_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Red won.";
                                MessageBox.Show(this, "Red got four in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.LOCAL_YELLOW;
                                break;
                        }
                    }
                    else
                    {
                        switch (GameLogic.CurrentStatus(this, redPlayer, col, columns[col]))
                        {
                            case MoveResult.BOARD_FILLED:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Draw.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case MoveResult.FOUR_IN_A_ROW:
                                turn = Turn.NOT_IN_SESSION;
                                turnLabel.Text = "Red won.";
                                NewGameMenuUpdate(true, false, false);
                                MessageBox.Show(this, "You got four in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                turn = Turn.NETWORK_OPPONENT_YELLOW;
                                break;
                        }
                    }
                    break;
                case Turn.NETWORK_OPPONENT_YELLOW:
                    switch (GameLogic.CurrentStatus(this, yellowPlayer, col, columns[col]))
                    {
                        case MoveResult.BOARD_FILLED:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "Draw.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case MoveResult.FOUR_IN_A_ROW:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "Yellow won.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Opponent got four in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            turn = Turn.LOCAL_RED;
                            turnLabel.Text = "Red's turn.";
                            break;
                    }
                    break;
                case Turn.NETWORK_OPPONENT_RED:
                    switch (GameLogic.CurrentStatus(this, redPlayer, col, columns[col]))
                    {
                        case MoveResult.BOARD_FILLED:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "Draw.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Board is filled.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case MoveResult.FOUR_IN_A_ROW:
                            turn = Turn.NOT_IN_SESSION;
                            turnLabel.Text = "Red won.";
                            NewGameMenuUpdate(true, false, false);
                            MessageBox.Show(this, "Opponent got four in a row.", "Game Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            turn = Turn.LOCAL_YELLOW;
                            turnLabel.Text = "Yellow's turn.";
                            break;
                    }
                    break;
            }
        }
    }
}
