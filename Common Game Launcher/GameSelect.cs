using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KvjChess;
using KvjGomoku;
using KvjConnectFour;

namespace KvjBoardGames
{
    public partial class GameSelect : Form
    {
        public GameSelect()
        {
            InitializeComponent();
        }

        private void btnChess_Click(object sender, EventArgs e)
        {
            this.Hide();
            StartGameMenu submenu = new StartGameMenu(new ChessBoard(), true);
            submenu.ShowDialog(this);
            ExitType exitChoice = submenu.ReturnStatus();
            if (exitChoice != ExitType.GAME_MENU)
            {
                this.Dispose();
            }
            else
            {
                this.Show();
                this.BringToFront();
            }
        }

        private void btnOmok_Click(object sender, EventArgs e)
        {
            this.Hide();
            StartGameMenu submenu = new StartGameMenu(new OmokBoard(), true);
            submenu.ShowDialog(this);
            ExitType exitChoice = submenu.ReturnStatus();
            if (exitChoice != ExitType.GAME_MENU)
            {
                this.Dispose();
            }
            else
            {
                this.Show();
                this.BringToFront();
            }
        }

        private void btnConnect4_Click(object sender, EventArgs e)
        {
            this.Hide();
            StartGameMenu submenu = new StartGameMenu(new Connect4Board(), true);
            submenu.ShowDialog(this);
            ExitType exitChoice = submenu.ReturnStatus();
            if (exitChoice != ExitType.GAME_MENU)
            {
                this.Dispose();
            }
            else
            {
                this.Show();
                this.BringToFront();
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameSelect());
        }
    }
}
