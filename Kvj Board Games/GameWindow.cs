using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using KvjBoardGames.OnlineFunctions;
using System.IO;

namespace KvjBoardGames
{
    public partial class GameWindow : Form
    {
        private readonly bool network;
        private ExitType exitChoice;

        public GameWindow(GameBoard board, NetworkInterface comm, bool canExitToGameSelect)
        {
            InitializeComponent(board);
            network = (comm != null);
            exitChoice = ExitType.DESKTOP;
            board.SetFrameComponents(turnIndicator, sessionStatus, moveIndicator, mnuForfeit, mnuAskTie, mnuRestart, new GameSpecificMenuEntries(mnuStrip));
            if (network)
            {
                this.mnuOpen.Enabled = false;
                this.mnuSave.Enabled = false;
            }
            if (!canExitToGameSelect)
                this.mnuExitToChooseGame.Enabled = false;
        }

        private bool Save()
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "Kvj Save File (*.kvj)|*.kvj";
            if (d.ShowDialog(this) != DialogResult.Cancel)
            {
                try
                {
                    using (FileStream fs = (System.IO.FileStream)d.OpenFile())
                    {
                        byte[] write = board.Serialize();
                        if (write != null && write.Length > 0)
                            fs.Write(write, 0, write.Length);
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            if (!board.InProgress() || ConfirmClose())
                if (CommonFunctions.OpenSaveFile(board))
                    board.OpenGame();
        }

        private void mnuRestart_Click(object sender, EventArgs e)
        {
            if (!board.InProgress() || ConfirmClose())
                board.NewGame();
        }

        private void mnuAskTie_Click(object sender, EventArgs e)
        {
            board.SendDraw();
        }

        private void mnuForfeit_Click(object sender, EventArgs e)
        {
            board.SendForfeit();
        }

        private void mnuBugs_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, board.BugsList, "Bugs List", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void mnuTodo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, board.TodoList, "Todo List", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(this);
        }

        private bool ConfirmClose()
        {
            bool exit;
            if (network)
            {
                DialogResult result = MessageBox.Show("A game is currently in progress.\nDo you wish to forfeit and leave this room?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    board.SendForfeit();
                    exit = true;
                }
                else //if (result == DialogResult.No)
                {
                    exit = false;
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("A game is currently in progress.\nDo you wish to save the current progress before leaving this room?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button3);
                if (result == DialogResult.Yes)
                {
                    exit = Save();
                }
                else if (result == DialogResult.No)
                {
                    exit = true;
                }
                else //if (result == DialogResult.Cancel)
                {
                    exit = false;
                }
            }
            return exit;
        }

        private void GameWindow_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (board.InProgress() && !ConfirmClose())
                e.Cancel = true;
            else
                board.CleanUp();
        }

        private void mnuExitToChooseGame_Click(object sender, EventArgs e)
        {
            exitChoice = ExitType.GAME_MENU;
            this.Close();
        }

        private void mnuExitToChoosePlayMethod_Click(object sender, EventArgs e)
        {
            exitChoice = ExitType.PLAY_MENU;
            this.Close();
        }

        private void mnuExitToDesktop_Click(object sender, EventArgs e)
        {
            exitChoice = ExitType.DESKTOP;
            this.Close();
        }

        public ExitType ReturnStatus()
        {
            return exitChoice;
        }
    }

    public enum ExitType
    {
        DESKTOP,
        PLAY_MENU,
        GAME_MENU
    }

    public class GameSpecificMenuEntries
    {
        private MenuStrip main;
        private Dictionary<string, ToolStripMenuItem> menuEntries;
        private Dictionary<string, Dictionary<string, ToolStripMenuItem>> subMenuEntries;

        internal GameSpecificMenuEntries(MenuStrip main)
        {
            this.main = main;
            this.menuEntries = new Dictionary<string, ToolStripMenuItem>();
            this.subMenuEntries = new Dictionary<string, Dictionary<string, ToolStripMenuItem>>();
        }

        public void AddMenuEntry(string name)
        {
            ToolStripMenuItem newEntry = new ToolStripMenuItem(name);
            main.Items.Add(newEntry);
            menuEntries[name] = newEntry;
            subMenuEntries[name] = new Dictionary<string, ToolStripMenuItem>();
        }

        public void AddSubmenuEntry(string menu, string name, EventHandler actions, bool checkMarked)
        {
            ToolStripMenuItem newSubEntry = new ToolStripMenuItem(name);
            newSubEntry.Checked = checkMarked;
            newSubEntry.Click += actions;
            menuEntries[menu].DropDownItems.Add(newSubEntry);
            subMenuEntries[menu][name] = newSubEntry;
        }

        public void DisableEntry(string menu, string name)
        {
            if (name != null)
                subMenuEntries[menu][name].Enabled = false;
            else
                menuEntries[menu].Enabled = false;
        }

        public void EnableEntry(string menu, string name)
        {
            if (name != null)
                subMenuEntries[menu][name].Enabled = true;
            else
                menuEntries[menu].Enabled = true;
        }

        public void Commit()
        {
            main.Refresh();
        }
    }
}
