
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
            this.ConsoleMessage.Size = new System.Drawing.Size(776, 426);
            this.ConsoleMessage.TabIndex = 0;
            // 
            // serverGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ConsoleMessage);
            this.Name = "serverGUI";
            this.Text = "server";
            this.Load += new System.EventHandler(this.server_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox ConsoleMessage;
    }
}

