namespace KvjBoardGames
{
    partial class StartGameMenu
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
            this.btnStartLocal = new System.Windows.Forms.Button();
            this.btnHost = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnResume = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStartLocal
            // 
            this.btnStartLocal.Location = new System.Drawing.Point(12, 12);
            this.btnStartLocal.Name = "btnStartLocal";
            this.btnStartLocal.Size = new System.Drawing.Size(198, 23);
            this.btnStartLocal.TabIndex = 0;
            this.btnStartLocal.Text = "New Local Game";
            this.btnStartLocal.UseVisualStyleBackColor = true;
            this.btnStartLocal.Click += new System.EventHandler(this.btnStartLocal_Click);
            // 
            // btnHost
            // 
            this.btnHost.Location = new System.Drawing.Point(12, 70);
            this.btnHost.Name = "btnHost";
            this.btnHost.Size = new System.Drawing.Size(198, 23);
            this.btnHost.TabIndex = 2;
            this.btnHost.Text = "Host Network Game";
            this.btnHost.UseVisualStyleBackColor = true;
            this.btnHost.Click += new System.EventHandler(this.btnHost_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(12, 99);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(198, 23);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Join Network Game";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnResume
            // 
            this.btnResume.Location = new System.Drawing.Point(12, 41);
            this.btnResume.Name = "btnResume";
            this.btnResume.Size = new System.Drawing.Size(198, 23);
            this.btnResume.TabIndex = 1;
            this.btnResume.Text = "Finish Local Game";
            this.btnResume.UseVisualStyleBackColor = true;
            this.btnResume.Click += new System.EventHandler(this.btnResume_Click);
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(12, 128);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(198, 23);
            this.btnBack.TabIndex = 4;
            this.btnBack.Text = "Return to Game Select";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // StartGameMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(222, 161);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnResume);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnHost);
            this.Controls.Add(this.btnStartLocal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "StartGameMenu";
            this.Text = "Kvj Board Games - Select Play Method";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStartLocal;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.Button btnBack;
    }
}