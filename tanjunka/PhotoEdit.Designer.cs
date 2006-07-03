namespace Tanjunka
{
    partial class PhotoEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.photoPanel = new Tanjunka.PhotoPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
// 
// tabControl1
// 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(292, 266);
            this.tabControl1.TabIndex = 0;
// 
// tabPage1
// 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(284, 240);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
// 
// splitContainer1
// 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
// 
// Panel1
// 
            this.splitContainer1.Panel1.Controls.Add(this.photoPanel);
// 
// Panel2
// 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.button2);
            this.splitContainer1.Size = new System.Drawing.Size(278, 234);
            this.splitContainer1.SplitterDistance = 155;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.Text = "splitContainer1";
// 
// photoPanel
// 
            this.photoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.photoPanel.Location = new System.Drawing.Point(0, 0);
            this.photoPanel.Name = "photoPanel";
            this.photoPanel.Size = new System.Drawing.Size(155, 234);
            this.photoPanel.TabIndex = 0;
            this.photoPanel.Text = "PhotoEdit";
// 
// pictureBox1
// 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBox1.Location = new System.Drawing.Point(26, 190);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(47, 27);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
// 
// panel1
// 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Location = new System.Drawing.Point(15, 150);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(48, 33);
            this.panel1.TabIndex = 2;
// 
// label1
// 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 14);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
// 
// button2
// 
            this.button2.Location = new System.Drawing.Point(26, 63);
            this.button2.Name = "button2";
            this.button2.TabIndex = 0;
            this.button2.Text = "button2";
// 
// tabPage2
// 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(284, 240);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
// 
// PhotoEdit
// 
            this.AutoScaleDimensions = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.tabControl1);
            this.Name = "PhotoEdit";
            this.Text = "PhotoEdit";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button button2;
        private PhotoPanel photoPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;

    }
}