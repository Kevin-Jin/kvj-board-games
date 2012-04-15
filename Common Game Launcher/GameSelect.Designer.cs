namespace KvjBoardGames
{
    partial class GameSelect
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPrompt = new System.Windows.Forms.Label();
            this.btnCheckers = new System.Windows.Forms.Button();
            this.btnOmok = new System.Windows.Forms.Button();
            this.btnConnect4 = new System.Windows.Forms.Button();
            this.btnChess = new System.Windows.Forms.Button();
            this.btnPoker = new System.Windows.Forms.Button();
            this.btnPalace = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(13, 13);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(165, 13);
            this.lblPrompt.TabIndex = 1;
            this.lblPrompt.Text = "Select the game you wish to play:";
            // 
            // btnCheckers
            // 
            this.btnCheckers.Location = new System.Drawing.Point(144, 45);
            this.btnCheckers.Name = "btnCheckers";
            this.btnCheckers.Size = new System.Drawing.Size(126, 47);
            this.btnCheckers.TabIndex = 2;
            this.btnCheckers.Text = "Checkers";
            this.btnCheckers.UseVisualStyleBackColor = true;
            // 
            // btnOmok
            // 
            this.btnOmok.Location = new System.Drawing.Point(12, 98);
            this.btnOmok.Name = "btnOmok";
            this.btnOmok.Size = new System.Drawing.Size(126, 47);
            this.btnOmok.TabIndex = 3;
            this.btnOmok.Text = "Gomoku (Five in a Row)";
            this.btnOmok.UseVisualStyleBackColor = true;
            this.btnOmok.Click += new System.EventHandler(this.btnOmok_Click);
            // 
            // btnConnect4
            // 
            this.btnConnect4.Location = new System.Drawing.Point(144, 98);
            this.btnConnect4.Name = "btnConnect4";
            this.btnConnect4.Size = new System.Drawing.Size(126, 47);
            this.btnConnect4.TabIndex = 4;
            this.btnConnect4.Text = "Connect 4 (Four in a Row)";
            this.btnConnect4.UseVisualStyleBackColor = true;
            this.btnConnect4.Click += new System.EventHandler(this.btnConnect4_Click);
            // 
            // btnChess
            // 
            this.btnChess.Location = new System.Drawing.Point(12, 45);
            this.btnChess.Name = "btnChess";
            this.btnChess.Size = new System.Drawing.Size(126, 47);
            this.btnChess.TabIndex = 1;
            this.btnChess.Text = "Chess";
            this.btnChess.UseVisualStyleBackColor = true;
            this.btnChess.Click += new System.EventHandler(this.btnChess_Click);
            // 
            // btnPoker
            // 
            this.btnPoker.Location = new System.Drawing.Point(144, 151);
            this.btnPoker.Name = "btnPoker";
            this.btnPoker.Size = new System.Drawing.Size(126, 47);
            this.btnPoker.TabIndex = 6;
            this.btnPoker.Text = "Poker";
            this.btnPoker.UseVisualStyleBackColor = true;
            // 
            // btnPalace
            // 
            this.btnPalace.Location = new System.Drawing.Point(12, 151);
            this.btnPalace.Name = "btnPalace";
            this.btnPalace.Size = new System.Drawing.Size(126, 47);
            this.btnPalace.TabIndex = 5;
            this.btnPalace.Text = "Palace";
            this.btnPalace.UseVisualStyleBackColor = true;
            // 
            // GameSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 211);
            this.Controls.Add(this.btnPoker);
            this.Controls.Add(this.btnPalace);
            this.Controls.Add(this.btnChess);
            this.Controls.Add(this.btnConnect4);
            this.Controls.Add(this.btnOmok);
            this.Controls.Add(this.btnCheckers);
            this.Controls.Add(this.lblPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "GameSelect";
            this.Text = "Kvj Board Games - Select a Game";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.Button btnCheckers;
        private System.Windows.Forms.Button btnOmok;
        private System.Windows.Forms.Button btnConnect4;
        private System.Windows.Forms.Button btnChess;
        private System.Windows.Forms.Button btnPoker;
        private System.Windows.Forms.Button btnPalace;
    }
}