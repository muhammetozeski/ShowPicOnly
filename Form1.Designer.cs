namespace ShowPicOnly
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources._976_700x500;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(689, 493);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "Form1";
            ShowIcon = false;
            StartPosition = FormStartPosition.Manual;
            Text = "İmage";
            TopMost = true;
            Load += Form1_Load;
            MouseDown += Form1_MouseDown;
            ResumeLayout(false);
        }

        #endregion
    }
}