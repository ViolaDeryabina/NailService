namespace NailService
{
    partial class MenuDirector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuDirector));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FIOlabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Exit = new System.Windows.Forms.Button();
            this.Seervices = new System.Windows.Forms.Button();
            this.Reports = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.FIOlabel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("MS Reference Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(13, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(959, 81);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // FIOlabel
            // 
            this.FIOlabel.AutoSize = true;
            this.FIOlabel.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.FIOlabel.Location = new System.Drawing.Point(687, 54);
            this.FIOlabel.Name = "FIOlabel";
            this.FIOlabel.Size = new System.Drawing.Size(164, 24);
            this.FIOlabel.TabIndex = 1;
            this.FIOlabel.Text = "Директор: ФИО";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 26.25F);
            this.label1.Location = new System.Drawing.Point(350, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(320, 44);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ногтевой сервис";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Exit);
            this.groupBox2.Controls.Add(this.Seervices);
            this.groupBox2.Controls.Add(this.Reports);
            this.groupBox2.Font = new System.Drawing.Font("MS Reference Sans Serif", 8.25F);
            this.groupBox2.Location = new System.Drawing.Point(13, 83);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(235, 466);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            // 
            // Exit
            // 
            this.Exit.BackColor = System.Drawing.Color.HotPink;
            this.Exit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Exit.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.Exit.Location = new System.Drawing.Point(6, 405);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(223, 55);
            this.Exit.TabIndex = 2;
            this.Exit.Text = "Выйти из аккаунта";
            this.Exit.UseVisualStyleBackColor = false;
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // Seervices
            // 
            this.Seervices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.Seervices.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Seervices.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.Seervices.Location = new System.Drawing.Point(6, 81);
            this.Seervices.Name = "Seervices";
            this.Seervices.Size = new System.Drawing.Size(223, 55);
            this.Seervices.TabIndex = 1;
            this.Seervices.Text = "Услуги";
            this.Seervices.UseVisualStyleBackColor = false;
            this.Seervices.Click += new System.EventHandler(this.Seervices_Click);
            // 
            // Reports
            // 
            this.Reports.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.Reports.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Reports.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.Reports.Location = new System.Drawing.Point(6, 20);
            this.Reports.Name = "Reports";
            this.Reports.Size = new System.Drawing.Size(223, 55);
            this.Reports.TabIndex = 0;
            this.Reports.Text = "Отчёты";
            this.Reports.UseVisualStyleBackColor = false;
            this.Reports.Click += new System.EventHandler(this.Reports_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::NailService.Properties.Resources.back;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Location = new System.Drawing.Point(255, 91);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(717, 458);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // MenuDirector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MenuDirector";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Меню директора";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label FIOlabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Reports;
        private System.Windows.Forms.Button Exit;
        private System.Windows.Forms.Button Seervices;
    }
}