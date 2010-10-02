namespace CassiniDev.ServerLog
{
    partial class BodyView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TextViewTab = new System.Windows.Forms.TabPage();
            this.TextViewTextBox = new System.Windows.Forms.RichTextBox();
            this.HexViewTab = new System.Windows.Forms.TabPage();
            this.HexViewTextBox = new System.Windows.Forms.RichTextBox();
            this.ImageViewTab = new System.Windows.Forms.TabPage();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.TextViewTab.SuspendLayout();
            this.HexViewTab.SuspendLayout();
            this.ImageViewTab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(271, 123);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.TextViewTab);
            this.tabControl1.Controls.Add(this.HexViewTab);
            this.tabControl1.Controls.Add(this.ImageViewTab);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(287, 157);
            this.tabControl1.TabIndex = 2;
            // 
            // TextViewTab
            // 
            this.TextViewTab.Controls.Add(this.TextViewTextBox);
            this.TextViewTab.Location = new System.Drawing.Point(4, 22);
            this.TextViewTab.Name = "TextViewTab";
            this.TextViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.TextViewTab.Size = new System.Drawing.Size(279, 131);
            this.TextViewTab.TabIndex = 1;
            this.TextViewTab.Text = "TextView";
            this.TextViewTab.UseVisualStyleBackColor = true;
            // 
            // TextViewTextBox
            // 
            this.TextViewTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextViewTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextViewTextBox.Location = new System.Drawing.Point(3, 3);
            this.TextViewTextBox.Name = "TextViewTextBox";
            this.TextViewTextBox.ReadOnly = true;
            this.TextViewTextBox.Size = new System.Drawing.Size(273, 125);
            this.TextViewTextBox.TabIndex = 0;
            this.TextViewTextBox.Text = "";
            this.toolTip1.SetToolTip(this.TextViewTextBox, "CTRL+ Mouse Wheel to Zoom");
            this.TextViewTextBox.WordWrap = false;
            // 
            // HexViewTab
            // 
            this.HexViewTab.Controls.Add(this.HexViewTextBox);
            this.HexViewTab.Location = new System.Drawing.Point(4, 22);
            this.HexViewTab.Name = "HexViewTab";
            this.HexViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.HexViewTab.Size = new System.Drawing.Size(279, 131);
            this.HexViewTab.TabIndex = 0;
            this.HexViewTab.Text = "HexView";
            this.HexViewTab.UseVisualStyleBackColor = true;
            // 
            // HexViewTextBox
            // 
            this.HexViewTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HexViewTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HexViewTextBox.Location = new System.Drawing.Point(3, 3);
            this.HexViewTextBox.Name = "HexViewTextBox";
            this.HexViewTextBox.ReadOnly = true;
            this.HexViewTextBox.Size = new System.Drawing.Size(273, 125);
            this.HexViewTextBox.TabIndex = 1;
            this.HexViewTextBox.Text = "";
            this.toolTip1.SetToolTip(this.HexViewTextBox, "CTRL+ Mouse Wheel to Zoom");
            this.HexViewTextBox.WordWrap = false;
            // 
            // ImageViewTab
            // 
            this.ImageViewTab.AutoScroll = true;
            this.ImageViewTab.Controls.Add(this.pictureBox1);
            this.ImageViewTab.Location = new System.Drawing.Point(4, 22);
            this.ImageViewTab.Name = "ImageViewTab";
            this.ImageViewTab.Padding = new System.Windows.Forms.Padding(4);
            this.ImageViewTab.Size = new System.Drawing.Size(279, 131);
            this.ImageViewTab.TabIndex = 2;
            this.ImageViewTab.Text = "ImageView";
            this.ImageViewTab.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richTextBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(279, 131);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "TODO";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(279, 131);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "Expose extension API, probably via MEF, to enable incremental implemention of add" +
                "itional views, e.g. a PDF view, a XAML view";
            this.toolTip1.SetToolTip(this.richTextBox1, "CTRL+ Mouse Wheel to Zoom");
            // 
            // BodyView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.Controls.Add(this.tabControl1);
            this.Name = "BodyView";
            this.Size = new System.Drawing.Size(287, 157);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.TextViewTab.ResumeLayout(false);
            this.HexViewTab.ResumeLayout(false);
            this.ImageViewTab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage HexViewTab;
        private System.Windows.Forms.RichTextBox HexViewTextBox;
        private System.Windows.Forms.TabPage TextViewTab;
        private System.Windows.Forms.RichTextBox TextViewTextBox;
        private System.Windows.Forms.TabPage ImageViewTab;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}
