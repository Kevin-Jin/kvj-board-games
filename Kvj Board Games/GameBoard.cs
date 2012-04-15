using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;
using KvjBoardGames.OnlineFunctions;

namespace KvjBoardGames
{
    public abstract class GameBoard : Control
    {
        public abstract string BugsList { get; }
        public abstract string TodoList { get; }

        public abstract Size Dimensions { get; }

        protected ToolStripStatusLabel turnLabel, statusLabel, moveLabel;
        private ToolStripMenuItem forfeitButton, tieButton, startOverButton;
        protected GameSpecificMenuEntries mnu;

        public GameBoard()
        {
            this.MouseDown += new MouseEventHandler(MouseClicked);
        }

        protected abstract void MouseClicked(object sender, MouseEventArgs e);

        internal void SetFrameComponents(ToolStripStatusLabel turnIndicator, ToolStripStatusLabel sessionStatus, ToolStripStatusLabel moveIndicator, ToolStripMenuItem forfeit, ToolStripMenuItem requestTie, ToolStripMenuItem newGame, GameSpecificMenuEntries mnuStrip)
        {
            this.turnLabel = turnIndicator;
            this.statusLabel = sessionStatus;
            this.moveLabel = moveIndicator;
            this.forfeitButton = forfeit;
            this.tieButton = requestTie;
            this.startOverButton = newGame;
            this.mnu = mnuStrip;
            GameWindowAssociated();
        }

        private delegate void RefreshCallback();

        public void Redraw()
        {
            if (this.InvokeRequired)
                this.Invoke(new RefreshCallback(Refresh));
            else
                Refresh();
        }

        protected abstract void GameWindowAssociated();

        protected void NewGameMenuUpdate(bool showNewGame, bool showRequestDraw, bool showForfeit)
        {
            startOverButton.Enabled = showNewGame;
            tieButton.Enabled = showRequestDraw;
            forfeitButton.Enabled = showForfeit;
        }

        public abstract void SetupServer(NetworkInterface comm);

        public abstract void PlayLocal();

        public abstract void PlayFromSave();

        public abstract void PlayServer(NetworkInterface comm);

        public abstract void PlayClient(NetworkInterface comm);

        public abstract void NewGame();

        public abstract void OpenGame();

        public abstract void SendDraw();

        public abstract void SendForfeit();

        public abstract bool InProgress();

        public abstract void CleanUp();

        public abstract byte[] Serialize();

        public abstract void Deserialize(byte[] bytes);
    }
}
