namespace NailService
{
    partial class MenuMaster
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuMaster));
            this.Exit = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FIOlabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dataGridViewSchedule = new System.Windows.Forms.DataGridView();
            this.lblWeek = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSchedule)).BeginInit();
            this.SuspendLayout();
            // 
            // Exit
            // 
            this.Exit.BackColor = System.Drawing.Color.HotPink;
            this.Exit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Exit.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.Exit.Location = new System.Drawing.Point(12, 735);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(223, 55);
            this.Exit.TabIndex = 2;
            this.Exit.Text = "Выйти из аккаунта";
            this.Exit.UseVisualStyleBackColor = false;
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.FIOlabel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Font = new System.Drawing.Font("MS Reference Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(13, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1141, 96);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // FIOlabel
            // 
            this.FIOlabel.AutoSize = true;
            this.FIOlabel.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.FIOlabel.Location = new System.Drawing.Point(846, 17);
            this.FIOlabel.Name = "FIOlabel";
            this.FIOlabel.Size = new System.Drawing.Size(140, 24);
            this.FIOlabel.TabIndex = 1;
            this.FIOlabel.Text = "Мастер: ФИО";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 26.25F);
            this.label1.Location = new System.Drawing.Point(472, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(320, 44);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ногтевой сервис";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::NailService.Properties.Resources.back_1;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(6, 17);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 72);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // dataGridViewSchedule
            // 
            this.dataGridViewSchedule.AllowUserToAddRows = false;
            this.dataGridViewSchedule.AllowUserToDeleteRows = false;
            this.dataGridViewSchedule.AllowUserToResizeColumns = false;
            this.dataGridViewSchedule.AllowUserToResizeRows = false;
            this.dataGridViewSchedule.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewSchedule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSchedule.Location = new System.Drawing.Point(13, 131);
            this.dataGridViewSchedule.Name = "dataGridViewSchedule";
            this.dataGridViewSchedule.ReadOnly = true;
            this.dataGridViewSchedule.Size = new System.Drawing.Size(1141, 592);
            this.dataGridViewSchedule.TabIndex = 5;
            this.dataGridViewSchedule.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewSchedule_CellDoubleClick);
            // 
            // lblWeek
            // 
            this.lblWeek.AutoSize = true;
            this.lblWeek.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.lblWeek.Location = new System.Drawing.Point(835, 106);
            this.lblWeek.Name = "lblWeek";
            this.lblWeek.Size = new System.Drawing.Size(55, 24);
            this.lblWeek.TabIndex = 2;
            this.lblWeek.Text = "Дата";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(832, 739);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 42);
            this.button1.TabIndex = 6;
            this.button1.Text = "<";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnPrevWeek_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(894, 739);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(56, 42);
            this.button2.TabIndex = 7;
            this.button2.Text = ">";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnNextWeek_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(965, 735);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(189, 46);
            this.button3.TabIndex = 8;
            this.button3.Text = "Текущая неделя";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnCurrentWeek_Click);
            // 
            // MenuMaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1170, 803);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblWeek);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.dataGridViewSchedule);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MenuMaster";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Меню мастера";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuMaster_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSchedule)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button Exit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label FIOlabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridViewSchedule;
        private System.Windows.Forms.Label lblWeek;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}