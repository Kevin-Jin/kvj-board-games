namespace KvjChess
{
    partial class PromotionSelect
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
            this.btnQueen = new System.Windows.Forms.Button();
            this.btnKnight = new System.Windows.Forms.Button();
            this.btnRook = new System.Windows.Forms.Button();
            this.btnBishop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            this.lblPrompt.Location = new System.Drawing.Point(12, 9);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(260, 33);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "Your pawn has reached its eighth rank. What kind of piece would you like to promo" +
                "te your pawn to?";
            // 
            // btnQueen
            // 
            this.btnQueen.Location = new System.Drawing.Point(12, 45);
            this.btnQueen.Name = "btnQueen";
            this.btnQueen.Size = new System.Drawing.Size(260, 45);
            this.btnQueen.TabIndex = 1;
            this.btnQueen.Text = "Queen";
            this.btnQueen.UseVisualStyleBackColor = true;
            this.btnQueen.Click += new System.EventHandler(this.btnQueen_Click);
            // 
            // btnKnight
            // 
            this.btnKnight.Location = new System.Drawing.Point(12, 96);
            this.btnKnight.Name = "btnKnight";
            this.btnKnight.Size = new System.Drawing.Size(260, 45);
            this.btnKnight.TabIndex = 2;
            this.btnKnight.Text = "Knight";
            this.btnKnight.UseVisualStyleBackColor = true;
            this.btnKnight.Click += new System.EventHandler(this.btnKnight_Click);
            // 
            // btnRook
            // 
            this.btnRook.Location = new System.Drawing.Point(12, 147);
            this.btnRook.Name = "btnRook";
            this.btnRook.Size = new System.Drawing.Size(260, 45);
            this.btnRook.TabIndex = 3;
            this.btnRook.Text = "Rook";
            this.btnRook.UseVisualStyleBackColor = true;
            this.btnRook.Click += new System.EventHandler(this.btnRook_Click);
            // 
            // btnBishop
            // 
            this.btnBishop.Location = new System.Drawing.Point(12, 198);
            this.btnBishop.Name = "btnBishop";
            this.btnBishop.Size = new System.Drawing.Size(260, 45);
            this.btnBishop.TabIndex = 4;
            this.btnBishop.Text = "Bishop";
            this.btnBishop.UseVisualStyleBackColor = true;
            this.btnBishop.Click += new System.EventHandler(this.btnBishop_Click);
            // 
            // PromotionSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 264);
            this.Controls.Add(this.btnBishop);
            this.Controls.Add(this.btnRook);
            this.Controls.Add(this.btnKnight);
            this.Controls.Add(this.btnQueen);
            this.Controls.Add(this.lblPrompt);
            this.Name = "PromotionSelect";
            this.Text = "Pawn Promotion";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.Button btnQueen;
        private System.Windows.Forms.Button btnKnight;
        private System.Windows.Forms.Button btnRook;
        private System.Windows.Forms.Button btnBishop;
    }
}