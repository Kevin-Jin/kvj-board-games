using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KvjBoardGames
{
    public partial class StartGameMenu : Form
    {
        private GameBoard board;
        private ExitType exitChoice;
        private bool canExitToGameSelect;

        public StartGameMenu(GameBoard board, bool fromCommonLauncher)
        {
            InitializeComponent();
            if (!fromCommonLauncher)
                btnBack.Enabled = false;
            this.board = board;
            this.canExitToGameSelect = fromCommonLauncher;
        }

        private void ShowGameWindow(GameWindow frame)
        {
            this.Hide();
            frame.ShowDialog(this);
            //frame has exited
            exitChoice = frame.ReturnStatus();
            if (exitChoice != ExitType.PLAY_MENU)
            {
                this.Dispose();
            }
            else
            {
                this.Show();
                this.BringToFront();
            }
        }

        private void btnStartLocal_Click(object sender, EventArgs e)
        {
            GameWindow frame = new GameWindow(board, null, canExitToGameSelect);
            board.PlayLocal();
            ShowGameWindow(frame);
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            if (CommonFunctions.OpenSaveFile(board))
            {
                GameWindow frame = new GameWindow(board, null, canExitToGameSelect);
                board.PlayFromSave();
                ShowGameWindow(frame);
            }
        }

        private void btnHost_Click(object sender, EventArgs e)
        {
            ConnectDialog networkInfo = new ConnectDialog(false);
            networkInfo.ShowDialog(this);
            if (networkInfo.Comm != null)
            {
                GameWindow frame = new GameWindow(board, networkInfo.Comm, canExitToGameSelect);
                board.SetupServer(networkInfo.Comm);
                ShowGameWindow(frame);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectDialog networkInfo = new ConnectDialog(true);
            networkInfo.ShowDialog(this);
            if (networkInfo.Comm != null)
            {
                GameWindow frame = new GameWindow(board, networkInfo.Comm, canExitToGameSelect);
                board.PlayServer(networkInfo.Comm);
                ShowGameWindow(frame);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            exitChoice = ExitType.GAME_MENU;
            this.Dispose();
        }

        public ExitType ReturnStatus()
        {
            return exitChoice;
        }
    }
}
