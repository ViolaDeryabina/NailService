namespace NailService
{
    partial class Show
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Show));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.InMenuStatus = new System.Windows.Forms.Button();
            this.EditStatusButton = new System.Windows.Forms.Button();
            this.AddStatusButton = new System.Windows.Forms.Button();
            this.StatusTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataGridViewStatuses = new System.Windows.Forms.DataGridView();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.CategoryTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.EditCategoryButton = new System.Windows.Forms.Button();
            this.AddCategoryButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridViewCategories = new System.Windows.Forms.DataGridView();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabControl1.SuspendLayout();
            this.tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatuses)).BeginInit();
            this.tabPage7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCategories)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Font = new System.Drawing.Font("MS Reference Sans Serif", 12.25F);
            this.tabControl1.Location = new System.Drawing.Point(3, 74);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(978, 556);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage6
            // 
            this.tabPage6.BackColor = System.Drawing.Color.White;
            this.tabPage6.Controls.Add(this.InMenuStatus);
            this.tabPage6.Controls.Add(this.EditStatusButton);
            this.tabPage6.Controls.Add(this.AddStatusButton);
            this.tabPage6.Controls.Add(this.StatusTextBox);
            this.tabPage6.Controls.Add(this.label2);
            this.tabPage6.Controls.Add(this.dataGridViewStatuses);
            this.tabPage6.Location = new System.Drawing.Point(4, 29);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(970, 523);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Статусы";
            // 
            // InMenuStatus
            // 
            this.InMenuStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.InMenuStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuStatus.Location = new System.Drawing.Point(683, 470);
            this.InMenuStatus.Name = "InMenuStatus";
            this.InMenuStatus.Size = new System.Drawing.Size(281, 48);
            this.InMenuStatus.TabIndex = 5;
            this.InMenuStatus.Text = "В меню";
            this.InMenuStatus.UseVisualStyleBackColor = false;
            this.InMenuStatus.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // EditStatusButton
            // 
            this.EditStatusButton.BackColor = System.Drawing.Color.HotPink;
            this.EditStatusButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.EditStatusButton.Location = new System.Drawing.Point(293, 470);
            this.EditStatusButton.Name = "EditStatusButton";
            this.EditStatusButton.Size = new System.Drawing.Size(281, 48);
            this.EditStatusButton.TabIndex = 4;
            this.EditStatusButton.Text = "Редактировать";
            this.EditStatusButton.UseVisualStyleBackColor = false;
            this.EditStatusButton.Click += new System.EventHandler(this.EditStatus_Click);
            // 
            // AddStatusButton
            // 
            this.AddStatusButton.BackColor = System.Drawing.Color.HotPink;
            this.AddStatusButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddStatusButton.Location = new System.Drawing.Point(6, 470);
            this.AddStatusButton.Name = "AddStatusButton";
            this.AddStatusButton.Size = new System.Drawing.Size(281, 48);
            this.AddStatusButton.TabIndex = 3;
            this.AddStatusButton.Text = "Добавить";
            this.AddStatusButton.UseVisualStyleBackColor = false;
            this.AddStatusButton.Click += new System.EventHandler(this.AddStatus_Click);
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.Location = new System.Drawing.Point(7, 424);
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.Size = new System.Drawing.Size(280, 27);
            this.StatusTextBox.TabIndex = 2;
            this.StatusTextBox.TextChanged += new System.EventHandler(this.StatusTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 399);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 22);
            this.label2.TabIndex = 1;
            this.label2.Text = "Название статуса";
            // 
            // dataGridViewStatuses
            // 
            this.dataGridViewStatuses.AllowUserToAddRows = false;
            this.dataGridViewStatuses.AllowUserToDeleteRows = false;
            this.dataGridViewStatuses.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewStatuses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewStatuses.Location = new System.Drawing.Point(6, 7);
            this.dataGridViewStatuses.Name = "dataGridViewStatuses";
            this.dataGridViewStatuses.ReadOnly = true;
            this.dataGridViewStatuses.Size = new System.Drawing.Size(958, 363);
            this.dataGridViewStatuses.TabIndex = 0;
            this.dataGridViewStatuses.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridViewStatuses_MouseClick);
            // 
            // tabPage7
            // 
            this.tabPage7.BackColor = System.Drawing.Color.White;
            this.tabPage7.Controls.Add(this.CategoryTextBox);
            this.tabPage7.Controls.Add(this.label4);
            this.tabPage7.Controls.Add(this.EditCategoryButton);
            this.tabPage7.Controls.Add(this.AddCategoryButton);
            this.tabPage7.Controls.Add(this.button1);
            this.tabPage7.Controls.Add(this.dataGridViewCategories);
            this.tabPage7.Location = new System.Drawing.Point(4, 29);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Size = new System.Drawing.Size(970, 523);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "Категории";
            // 
            // CategoryTextBox
            // 
            this.CategoryTextBox.Location = new System.Drawing.Point(7, 424);
            this.CategoryTextBox.Name = "CategoryTextBox";
            this.CategoryTextBox.Size = new System.Drawing.Size(284, 27);
            this.CategoryTextBox.TabIndex = 5;
            this.CategoryTextBox.TextChanged += new System.EventHandler(this.CategoryTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 399);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(188, 22);
            this.label4.TabIndex = 4;
            this.label4.Text = "Название категории";
            // 
            // EditCategoryButton
            // 
            this.EditCategoryButton.BackColor = System.Drawing.Color.HotPink;
            this.EditCategoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.EditCategoryButton.Location = new System.Drawing.Point(293, 470);
            this.EditCategoryButton.Name = "EditCategoryButton";
            this.EditCategoryButton.Size = new System.Drawing.Size(281, 48);
            this.EditCategoryButton.TabIndex = 3;
            this.EditCategoryButton.Text = "Редактировать";
            this.EditCategoryButton.UseVisualStyleBackColor = false;
            this.EditCategoryButton.Click += new System.EventHandler(this.EditCategory_Click);
            // 
            // AddCategoryButton
            // 
            this.AddCategoryButton.BackColor = System.Drawing.Color.HotPink;
            this.AddCategoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddCategoryButton.Location = new System.Drawing.Point(6, 470);
            this.AddCategoryButton.Name = "AddCategoryButton";
            this.AddCategoryButton.Size = new System.Drawing.Size(281, 48);
            this.AddCategoryButton.TabIndex = 2;
            this.AddCategoryButton.Text = "Добавить";
            this.AddCategoryButton.UseVisualStyleBackColor = false;
            this.AddCategoryButton.Click += new System.EventHandler(this.AddCategory_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(683, 470);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(281, 48);
            this.button1.TabIndex = 1;
            this.button1.Text = "В меню";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // dataGridViewCategories
            // 
            this.dataGridViewCategories.AllowUserToAddRows = false;
            this.dataGridViewCategories.AllowUserToDeleteRows = false;
            this.dataGridViewCategories.AllowUserToResizeColumns = false;
            this.dataGridViewCategories.AllowUserToResizeRows = false;
            this.dataGridViewCategories.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCategories.Location = new System.Drawing.Point(6, 7);
            this.dataGridViewCategories.Name = "dataGridViewCategories";
            this.dataGridViewCategories.ReadOnly = true;
            this.dataGridViewCategories.Size = new System.Drawing.Size(958, 363);
            this.dataGridViewCategories.TabIndex = 0;
            this.dataGridViewCategories.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridViewCategories_MouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 22.25F);
            this.label1.Location = new System.Drawing.Point(390, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 38);
            this.label1.TabIndex = 2;
            this.label1.Text = "Справочники";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS Reference Sans Serif", 12.25F);
            this.label3.Location = new System.Drawing.Point(656, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 22);
            this.label3.TabIndex = 3;
            this.label3.Text = "Роль: ФИО";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::NailService.Properties.Resources.back_1;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 72);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // Show
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(984, 637);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Show";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Справочники";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Show_FormClosing);
            this.Load += new System.EventHandler(this.Show_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatuses)).EndInit();
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCategories)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.Button InMenuStatus;
        private System.Windows.Forms.Button EditStatusButton;
        private System.Windows.Forms.Button AddStatusButton;
        private System.Windows.Forms.TextBox StatusTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dataGridViewStatuses;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.Button AddCategoryButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridViewCategories;
        private System.Windows.Forms.Button EditCategoryButton;
        private System.Windows.Forms.TextBox CategoryTextBox;
        private System.Windows.Forms.Label label4;
    }
}