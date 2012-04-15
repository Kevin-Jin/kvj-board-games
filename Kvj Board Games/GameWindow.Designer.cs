namespace KvjBoardGames
{
    partial class GameWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent(GameBoard board)
        {
            InitializeComponent();
            this.board = board;
            this.SuspendLayout();
            this.board.Location = new System.Drawing.Point(0, mnuStrip.Height);
            this.board.Size = board.Dimensions;
            this.Controls.Add(board);
            this.ClientSize = new System.Drawing.Size(board.Size.Width, board.Location.Y + board.Size.Height + statusStrip.Height);
            this.ResumeLayout();
            this.PerformLayout();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mnuStrip = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReset = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRestart = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAskTie = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuForfeit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExitToChooseGame = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExitToChoosePlayMethod = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExitToDesktop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBugs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTodo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.turnIndicator = new System.Windows.Forms.ToolStripStatusLabel();
            this.sessionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.moveIndicator = new System.Windows.Forms.ToolStripStatusLabel();
            this.mnuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuStrip
            // 
            this.mnuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuHelp});
            this.mnuStrip.Location = new System.Drawing.Point(0, 0);
            this.mnuStrip.Name = "mnuStrip";
            this.mnuStrip.Size = new System.Drawing.Size(0, 24);
            this.mnuStrip.TabIndex = 0;
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuReset,
            this.mnuOpen,
            this.mnuSave,
            this.mnuExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "File";
            // 
            // mnuReset
            // 
            this.mnuReset.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRestart,
            this.mnuAskTie,
            this.mnuForfeit});
            this.mnuReset.Name = "mnuReset";
            this.mnuReset.Size = new System.Drawing.Size(184, 22);
            this.mnuReset.Text = "New Game";
            // 
            // mnuRestart
            // 
            this.mnuRestart.Name = "mnuRestart";
            this.mnuRestart.Size = new System.Drawing.Size(146, 22);
            this.mnuRestart.Text = "New Game";
            this.mnuRestart.Click += new System.EventHandler(this.mnuRestart_Click);
            // 
            // mnuAskTie
            // 
            this.mnuAskTie.Name = "mnuAskTie";
            this.mnuAskTie.Size = new System.Drawing.Size(146, 22);
            this.mnuAskTie.Text = "Request Draw";
            this.mnuAskTie.Click += new System.EventHandler(this.mnuAskTie_Click);
            // 
            // mnuForfeit
            // 
            this.mnuForfeit.Name = "mnuForfeit";
            this.mnuForfeit.Size = new System.Drawing.Size(146, 22);
            this.mnuForfeit.Text = "Forfeit";
            this.mnuForfeit.Click += new System.EventHandler(this.mnuForfeit_Click);
            // 
            // mnuOpen
            // 
            this.mnuOpen.Name = "mnuOpen";
            this.mnuOpen.Size = new System.Drawing.Size(184, 22);
            this.mnuOpen.Text = "Resume Saved Game";
            this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
            // 
            // mnuSave
            // 
            this.mnuSave.Name = "mnuSave";
            this.mnuSave.Size = new System.Drawing.Size(184, 22);
            this.mnuSave.Text = "Save Progress";
            this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
            // 
            // mnuExit
            // 
            this.mnuExit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExitToChooseGame,
            this.mnuExitToChoosePlayMethod,
            this.mnuExitToDesktop});
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(184, 22);
            this.mnuExit.Text = "Exit";
            // 
            // mnuExitToChooseGame
            // 
            this.mnuExitToChooseGame.Name = "mnuExitToChooseGame";
            this.mnuExitToChooseGame.Size = new System.Drawing.Size(199, 22);
            this.mnuExitToChooseGame.Text = "To Choose Game Menu";
            this.mnuExitToChooseGame.Click += new System.EventHandler(this.mnuExitToChooseGame_Click);
            // 
            // mnuExitToChoosePlayMethod
            // 
            this.mnuExitToChoosePlayMethod.Name = "mnuExitToChoosePlayMethod";
            this.mnuExitToChoosePlayMethod.Size = new System.Drawing.Size(199, 22);
            this.mnuExitToChoosePlayMethod.Text = "To Choose Play Menu";
            this.mnuExitToChoosePlayMethod.Click += new System.EventHandler(this.mnuExitToChoosePlayMethod_Click);
            // 
            // mnuExitToDesktop
            // 
            this.mnuExitToDesktop.Name = "mnuExitToDesktop";
            this.mnuExitToDesktop.Size = new System.Drawing.Size(199, 22);
            this.mnuExitToDesktop.Text = "Application";
            this.mnuExitToDesktop.Click += new System.EventHandler(this.mnuExitToDesktop_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuBugs,
            this.mnuTodo,
            this.mnuAbout});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(44, 20);
            this.mnuHelp.Text = "Help";
            // 
            // mnuBugs
            // 
            this.mnuBugs.Name = "mnuBugs";
            this.mnuBugs.Size = new System.Drawing.Size(123, 22);
            this.mnuBugs.Text = "Bugs";
            this.mnuBugs.Click += new System.EventHandler(this.mnuBugs_Click);
            // 
            // mnuTodo
            // 
            this.mnuTodo.Name = "mnuTodo";
            this.mnuTodo.Size = new System.Drawing.Size(123, 22);
            this.mnuTodo.Text = "Todo List";
            this.mnuTodo.Click += new System.EventHandler(this.mnuTodo_Click);
            // 
            // mnuAbout
            // 
            this.mnuAbout.Name = "mnuAbout";
            this.mnuAbout.Size = new System.Drawing.Size(123, 22);
            this.mnuAbout.Text = "About";
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.turnIndicator,
            this.sessionStatus,
            this.moveIndicator});
            this.statusStrip.Location = new System.Drawing.Point(0, -22);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(0, 22);
            this.statusStrip.TabIndex = 1;
            // 
            // turnIndicator
            // 
            this.turnIndicator.Name = "turnIndicator";
            this.turnIndicator.Size = new System.Drawing.Size(0, 17);
            this.moveIndicator.Spring = true;
            this.turnIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sessionStatus
            // 
            this.sessionStatus.Name = "midIndicator";
            this.sessionStatus.Size = new System.Drawing.Size(0, 17);
            this.sessionStatus.Spring = true;
            this.sessionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // moveIndicator
            // 
            this.moveIndicator.Name = "moveIndicator";
            this.moveIndicator.Size = new System.Drawing.Size(0, 17);
            this.moveIndicator.Spring = true;
            this.moveIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // GameWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mnuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.mnuStrip;
            this.MaximizeBox = false;
            this.Name = "GameWindow";
            this.Text = "Kvj Board Games";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameWindow_FormClosing);
            this.mnuStrip.ResumeLayout(false);
            this.mnuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GameBoard board;
        private System.Windows.Forms.MenuStrip mnuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuReset;
        private System.Windows.Forms.ToolStripMenuItem mnuOpen;
        private System.Windows.Forms.ToolStripMenuItem mnuSave;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuBugs;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.ToolStripMenuItem mnuExitToChooseGame;
        private System.Windows.Forms.ToolStripMenuItem mnuExitToChoosePlayMethod;
        private System.Windows.Forms.ToolStripMenuItem mnuExitToDesktop;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuAskTie;
        private System.Windows.Forms.ToolStripMenuItem mnuForfeit;
        private System.Windows.Forms.ToolStripStatusLabel turnIndicator;
        private System.Windows.Forms.ToolStripStatusLabel sessionStatus;
        private System.Windows.Forms.ToolStripStatusLabel moveIndicator;
        private System.Windows.Forms.ToolStripMenuItem mnuRestart;
        private System.Windows.Forms.ToolStripMenuItem mnuTodo;
    }
}

