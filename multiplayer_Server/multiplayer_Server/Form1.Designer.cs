
namespace multiplayer_Server {
    partial class serverGUI {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.ConsoleMessage = new System.Windows.Forms.TextBox();
            this.PLAYER1IP = new System.Windows.Forms.Label();
            this.GameStatusLabel = new System.Windows.Forms.Label();
            this.PLAYER2IP = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ConsoleMessage
            // 
            this.ConsoleMessage.BackColor = System.Drawing.Color.Gainsboro;
            this.ConsoleMessage.Location = new System.Drawing.Point(12, 12);
            this.ConsoleMessage.Multiline = true;
            this.ConsoleMessage.Name = "ConsoleMessage";
            this.ConsoleMessage.ReadOnly = true;
            this.ConsoleMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ConsoleMessage.Size = new System.Drawing.Size(495, 426);
            this.ConsoleMessage.TabIndex = 0;
            // 
            // PLAYER1IP
            // 
            this.PLAYER1IP.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PLAYER1IP.Location = new System.Drawing.Point(522, 76);
            this.PLAYER1IP.Name = "PLAYER1IP";
            this.PLAYER1IP.Size = new System.Drawing.Size(275, 37);
            this.PLAYER1IP.TabIndex = 1;
            this.PLAYER1IP.Text = "PLAYER1 : 255.255.255.255";
            this.PLAYER1IP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GameStatusLabel
            // 
            this.GameStatusLabel.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.GameStatusLabel.Location = new System.Drawing.Point(522, 27);
            this.GameStatusLabel.Name = "GameStatusLabel";
            this.GameStatusLabel.Size = new System.Drawing.Size(275, 37);
            this.GameStatusLabel.TabIndex = 2;
            this.GameStatusLabel.Text = "Status : Waiting For Player";
            this.GameStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PLAYER2IP
            // 
            this.PLAYER2IP.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PLAYER2IP.Location = new System.Drawing.Point(522, 113);
            this.PLAYER2IP.Name = "PLAYER2IP";
            this.PLAYER2IP.Size = new System.Drawing.Size(275, 36);
            this.PLAYER2IP.TabIndex = 3;
            this.PLAYER2IP.Text = "PLAYER2 : 255.255.255.255";
            this.PLAYER2IP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // serverGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.PLAYER2IP);
            this.Controls.Add(this.GameStatusLabel);
            this.Controls.Add(this.PLAYER1IP);
            this.Controls.Add(this.ConsoleMessage);
            this.Name = "serverGUI";
            this.Text = "server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox ConsoleMessage;
        private System.Windows.Forms.Label PLAYER1IP;
        private System.Windows.Forms.Label GameStatusLabel;
        private System.Windows.Forms.Label PLAYER2IP;
    }
}

