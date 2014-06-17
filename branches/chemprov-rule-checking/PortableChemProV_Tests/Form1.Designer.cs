namespace PortableChemProV_Tests
{
    partial class Form1
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tbParentUserName_CM = new System.Windows.Forms.TextBox();
            this.btnGo_CM = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnBrowseOutput_CM = new System.Windows.Forms.Button();
            this.tbOutput_CM = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBrowseChild_CM = new System.Windows.Forms.Button();
            this.tbChildSource_CM = new System.Windows.Forms.TextBox();
            this.gbParentFile = new System.Windows.Forms.GroupBox();
            this.btnBrowseParent_CM = new System.Windows.Forms.Button();
            this.tbParentSource_CM = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tbChildUserName_CM = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gbParentFile.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(320, 343);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.btnGo_CM);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.gbParentFile);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(312, 317);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Comment Merge";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.tbParentUserName_CM);
            this.groupBox3.Location = new System.Drawing.Point(6, 62);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(300, 50);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Parent user name if not in XML";
            // 
            // tbParentUserName_CM
            // 
            this.tbParentUserName_CM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbParentUserName_CM.Location = new System.Drawing.Point(6, 19);
            this.tbParentUserName_CM.Name = "tbParentUserName_CM";
            this.tbParentUserName_CM.Size = new System.Drawing.Size(288, 20);
            this.tbParentUserName_CM.TabIndex = 0;
            // 
            // btnGo_CM
            // 
            this.btnGo_CM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGo_CM.Location = new System.Drawing.Point(6, 286);
            this.btnGo_CM.Name = "btnGo_CM";
            this.btnGo_CM.Size = new System.Drawing.Size(300, 23);
            this.btnGo_CM.TabIndex = 3;
            this.btnGo_CM.Text = "Go";
            this.btnGo_CM.UseVisualStyleBackColor = true;
            this.btnGo_CM.Click += new System.EventHandler(this.btnGo_CM_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnBrowseOutput_CM);
            this.groupBox2.Controls.Add(this.tbOutput_CM);
            this.groupBox2.Location = new System.Drawing.Point(6, 230);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(300, 50);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output File";
            // 
            // btnBrowseOutput_CM
            // 
            this.btnBrowseOutput_CM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutput_CM.Location = new System.Drawing.Point(254, 18);
            this.btnBrowseOutput_CM.Name = "btnBrowseOutput_CM";
            this.btnBrowseOutput_CM.Size = new System.Drawing.Size(40, 20);
            this.btnBrowseOutput_CM.TabIndex = 1;
            this.btnBrowseOutput_CM.Text = "...";
            this.btnBrowseOutput_CM.UseVisualStyleBackColor = true;
            this.btnBrowseOutput_CM.Click += new System.EventHandler(this.btnBrowseOutput_CM_Click);
            // 
            // tbOutput_CM
            // 
            this.tbOutput_CM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOutput_CM.Location = new System.Drawing.Point(6, 19);
            this.tbOutput_CM.Name = "tbOutput_CM";
            this.tbOutput_CM.Size = new System.Drawing.Size(242, 20);
            this.tbOutput_CM.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnBrowseChild_CM);
            this.groupBox1.Controls.Add(this.tbChildSource_CM);
            this.groupBox1.Location = new System.Drawing.Point(6, 118);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(300, 50);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "\"Child\" Input File";
            // 
            // btnBrowseChild_CM
            // 
            this.btnBrowseChild_CM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseChild_CM.Location = new System.Drawing.Point(254, 18);
            this.btnBrowseChild_CM.Name = "btnBrowseChild_CM";
            this.btnBrowseChild_CM.Size = new System.Drawing.Size(40, 20);
            this.btnBrowseChild_CM.TabIndex = 1;
            this.btnBrowseChild_CM.Text = "...";
            this.btnBrowseChild_CM.UseVisualStyleBackColor = true;
            this.btnBrowseChild_CM.Click += new System.EventHandler(this.btnBrowseChild_CM_Click);
            // 
            // tbChildSource_CM
            // 
            this.tbChildSource_CM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbChildSource_CM.Location = new System.Drawing.Point(6, 19);
            this.tbChildSource_CM.Name = "tbChildSource_CM";
            this.tbChildSource_CM.Size = new System.Drawing.Size(242, 20);
            this.tbChildSource_CM.TabIndex = 0;
            // 
            // gbParentFile
            // 
            this.gbParentFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbParentFile.Controls.Add(this.btnBrowseParent_CM);
            this.gbParentFile.Controls.Add(this.tbParentSource_CM);
            this.gbParentFile.Location = new System.Drawing.Point(6, 6);
            this.gbParentFile.Name = "gbParentFile";
            this.gbParentFile.Size = new System.Drawing.Size(300, 50);
            this.gbParentFile.TabIndex = 0;
            this.gbParentFile.TabStop = false;
            this.gbParentFile.Text = "\"Parent\" Input File";
            // 
            // btnBrowseParent_CM
            // 
            this.btnBrowseParent_CM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseParent_CM.Location = new System.Drawing.Point(254, 18);
            this.btnBrowseParent_CM.Name = "btnBrowseParent_CM";
            this.btnBrowseParent_CM.Size = new System.Drawing.Size(40, 20);
            this.btnBrowseParent_CM.TabIndex = 1;
            this.btnBrowseParent_CM.Text = "...";
            this.btnBrowseParent_CM.UseVisualStyleBackColor = true;
            this.btnBrowseParent_CM.Click += new System.EventHandler(this.btnBrowseParent_CM_Click);
            // 
            // tbParentSource_CM
            // 
            this.tbParentSource_CM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbParentSource_CM.Location = new System.Drawing.Point(6, 19);
            this.tbParentSource_CM.Name = "tbParentSource_CM";
            this.tbParentSource_CM.Size = new System.Drawing.Size(242, 20);
            this.tbParentSource_CM.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.tbChildUserName_CM);
            this.groupBox4.Location = new System.Drawing.Point(6, 174);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(300, 50);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Child user name if not in XML";
            // 
            // tbChildUserName_CM
            // 
            this.tbChildUserName_CM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbChildUserName_CM.Location = new System.Drawing.Point(6, 19);
            this.tbChildUserName_CM.Name = "tbChildUserName_CM";
            this.tbChildUserName_CM.Size = new System.Drawing.Size(288, 20);
            this.tbChildUserName_CM.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 367);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "PortableChemProV Library Tests";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbParentFile.ResumeLayout(false);
            this.gbParentFile.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox gbParentFile;
        private System.Windows.Forms.Button btnBrowseParent_CM;
        private System.Windows.Forms.TextBox tbParentSource_CM;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnBrowseChild_CM;
        private System.Windows.Forms.TextBox tbChildSource_CM;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnBrowseOutput_CM;
        private System.Windows.Forms.TextBox tbOutput_CM;
        private System.Windows.Forms.Button btnGo_CM;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox tbParentUserName_CM;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tbChildUserName_CM;
    }
}

