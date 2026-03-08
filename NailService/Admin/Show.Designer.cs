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
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.AddUsers = new System.Windows.Forms.Button();
            this.InMenu = new System.Windows.Forms.Button();
            this.Users = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.InMenuMaster = new System.Windows.Forms.Button();
            this.AddMaster = new System.Windows.Forms.Button();
            this.dataGridViewMasters = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.AddService = new System.Windows.Forms.Button();
            this.InMenuService = new System.Windows.Forms.Button();
            this.dataGridViewServices = new System.Windows.Forms.DataGridView();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.AddClient = new System.Windows.Forms.Button();
            this.InMenuClient = new System.Windows.Forms.Button();
            this.dataGridViewClients = new System.Windows.Forms.DataGridView();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.InMenuStatus = new System.Windows.Forms.Button();
            this.EditStatusButton = new System.Windows.Forms.Button();
            this.AddStatusButton = new System.Windows.Forms.Button();
            this.StatusTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataGridViewStatuses = new System.Windows.Forms.DataGridView();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Users)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMasters)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewServices)).BeginInit();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClients)).BeginInit();
            this.tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatuses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Font = new System.Drawing.Font("MS Reference Sans Serif", 12.25F);
            this.tabControl1.Location = new System.Drawing.Point(3, 74);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(978, 556);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.Controls.Add(this.AddUsers);
            this.tabPage1.Controls.Add(this.InMenu);
            this.tabPage1.Controls.Add(this.Users);
            this.tabPage1.Font = new System.Drawing.Font("MS Reference Sans Serif", 12.25F);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(970, 523);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Пользователи";
            // 
            // AddUsers
            // 
            this.AddUsers.BackColor = System.Drawing.Color.HotPink;
            this.AddUsers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddUsers.Location = new System.Drawing.Point(683, 468);
            this.AddUsers.Name = "AddUsers";
            this.AddUsers.Size = new System.Drawing.Size(281, 48);
            this.AddUsers.TabIndex = 2;
            this.AddUsers.Text = "Добавить пользователя";
            this.AddUsers.UseVisualStyleBackColor = false;
            this.AddUsers.Click += new System.EventHandler(this.AddUsers_Click);
            // 
            // InMenu
            // 
            this.InMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.InMenu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenu.Location = new System.Drawing.Point(6, 468);
            this.InMenu.Name = "InMenu";
            this.InMenu.Size = new System.Drawing.Size(281, 48);
            this.InMenu.TabIndex = 1;
            this.InMenu.Text = "В меню";
            this.InMenu.UseVisualStyleBackColor = false;
            this.InMenu.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // Users
            // 
            this.Users.AllowUserToAddRows = false;
            this.Users.AllowUserToDeleteRows = false;
            this.Users.AllowUserToResizeColumns = false;
            this.Users.AllowUserToResizeRows = false;
            this.Users.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.Users.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Users.Location = new System.Drawing.Point(6, 7);
            this.Users.Name = "Users";
            this.Users.Size = new System.Drawing.Size(958, 455);
            this.Users.TabIndex = 0;
            this.Users.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Users_MouseClick);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.InMenuMaster);
            this.tabPage2.Controls.Add(this.AddMaster);
            this.tabPage2.Controls.Add(this.dataGridViewMasters);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(970, 523);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Мастера";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // InMenuMaster
            // 
            this.InMenuMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.InMenuMaster.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuMaster.Location = new System.Drawing.Point(6, 468);
            this.InMenuMaster.Name = "InMenuMaster";
            this.InMenuMaster.Size = new System.Drawing.Size(281, 48);
            this.InMenuMaster.TabIndex = 4;
            this.InMenuMaster.Text = "В меню";
            this.InMenuMaster.UseVisualStyleBackColor = false;
            this.InMenuMaster.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // AddMaster
            // 
            this.AddMaster.BackColor = System.Drawing.Color.HotPink;
            this.AddMaster.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddMaster.Location = new System.Drawing.Point(683, 468);
            this.AddMaster.Name = "AddMaster";
            this.AddMaster.Size = new System.Drawing.Size(281, 48);
            this.AddMaster.TabIndex = 3;
            this.AddMaster.Text = "Добавить мастера";
            this.AddMaster.UseVisualStyleBackColor = false;
            this.AddMaster.Click += new System.EventHandler(this.AddMaster_Click);
            // 
            // dataGridViewMasters
            // 
            this.dataGridViewMasters.AllowUserToAddRows = false;
            this.dataGridViewMasters.AllowUserToDeleteRows = false;
            this.dataGridViewMasters.AllowUserToResizeColumns = false;
            this.dataGridViewMasters.AllowUserToResizeRows = false;
            this.dataGridViewMasters.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewMasters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMasters.Location = new System.Drawing.Point(6, 7);
            this.dataGridViewMasters.Name = "dataGridViewMasters";
            this.dataGridViewMasters.Size = new System.Drawing.Size(958, 455);
            this.dataGridViewMasters.TabIndex = 0;
            this.dataGridViewMasters.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridViewMasters_MouseClick);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.AddService);
            this.tabPage4.Controls.Add(this.InMenuService);
            this.tabPage4.Controls.Add(this.dataGridViewServices);
            this.tabPage4.Location = new System.Drawing.Point(4, 29);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(970, 523);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Услуги";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // AddService
            // 
            this.AddService.BackColor = System.Drawing.Color.HotPink;
            this.AddService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddService.Location = new System.Drawing.Point(683, 468);
            this.AddService.Name = "AddService";
            this.AddService.Size = new System.Drawing.Size(281, 48);
            this.AddService.TabIndex = 7;
            this.AddService.Text = "Добавить уcлугу";
            this.AddService.UseVisualStyleBackColor = false;
            this.AddService.Click += new System.EventHandler(this.AddService_Click);
            // 
            // InMenuService
            // 
            this.InMenuService.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.InMenuService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuService.Location = new System.Drawing.Point(6, 468);
            this.InMenuService.Name = "InMenuService";
            this.InMenuService.Size = new System.Drawing.Size(281, 48);
            this.InMenuService.TabIndex = 5;
            this.InMenuService.Text = "В меню";
            this.InMenuService.UseVisualStyleBackColor = false;
            this.InMenuService.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // dataGridViewServices
            // 
            this.dataGridViewServices.AllowUserToAddRows = false;
            this.dataGridViewServices.AllowUserToDeleteRows = false;
            this.dataGridViewServices.AllowUserToResizeColumns = false;
            this.dataGridViewServices.AllowUserToResizeRows = false;
            this.dataGridViewServices.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewServices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewServices.Location = new System.Drawing.Point(6, 7);
            this.dataGridViewServices.Name = "dataGridViewServices";
            this.dataGridViewServices.Size = new System.Drawing.Size(958, 455);
            this.dataGridViewServices.TabIndex = 0;
            this.dataGridViewServices.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridViewServices_MouseClick);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.AddClient);
            this.tabPage5.Controls.Add(this.InMenuClient);
            this.tabPage5.Controls.Add(this.dataGridViewClients);
            this.tabPage5.Location = new System.Drawing.Point(4, 29);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(970, 523);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Клиенты";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // AddClient
            // 
            this.AddClient.BackColor = System.Drawing.Color.HotPink;
            this.AddClient.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddClient.Location = new System.Drawing.Point(683, 468);
            this.AddClient.Name = "AddClient";
            this.AddClient.Size = new System.Drawing.Size(281, 48);
            this.AddClient.TabIndex = 7;
            this.AddClient.Text = "Добавить клиента";
            this.AddClient.UseVisualStyleBackColor = false;
            this.AddClient.Click += new System.EventHandler(this.AddClient_Click);
            // 
            // InMenuClient
            // 
            this.InMenuClient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.InMenuClient.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuClient.Location = new System.Drawing.Point(6, 468);
            this.InMenuClient.Name = "InMenuClient";
            this.InMenuClient.Size = new System.Drawing.Size(281, 48);
            this.InMenuClient.TabIndex = 5;
            this.InMenuClient.Text = "В меню";
            this.InMenuClient.UseVisualStyleBackColor = false;
            this.InMenuClient.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // dataGridViewClients
            // 
            this.dataGridViewClients.AllowUserToAddRows = false;
            this.dataGridViewClients.AllowUserToDeleteRows = false;
            this.dataGridViewClients.AllowUserToResizeColumns = false;
            this.dataGridViewClients.AllowUserToResizeRows = false;
            this.dataGridViewClients.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewClients.Location = new System.Drawing.Point(6, 7);
            this.dataGridViewClients.Name = "dataGridViewClients";
            this.dataGridViewClients.Size = new System.Drawing.Size(958, 455);
            this.dataGridViewClients.TabIndex = 0;
            this.dataGridViewClients.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridViewClients_MouseClick);
            // 
            // tabPage6
            // 
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
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // InMenuStatus
            // 
            this.InMenuStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.InMenuStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuStatus.Location = new System.Drawing.Point(683, 468);
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
            this.EditStatusButton.Location = new System.Drawing.Point(293, 469);
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
            this.Load += new System.EventHandler(this.Show_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Users)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMasters)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewServices)).EndInit();
            this.tabPage5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClients)).EndInit();
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatuses)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.DataGridView Users;
        private System.Windows.Forms.DataGridView dataGridViewMasters;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.DataGridView dataGridViewServices;
        private System.Windows.Forms.DataGridView dataGridViewClients;
        private System.Windows.Forms.Button InMenu;
        private System.Windows.Forms.Button AddUsers;
        private System.Windows.Forms.Button AddMaster;
        private System.Windows.Forms.Button InMenuMaster;
        private System.Windows.Forms.Button AddService;
        private System.Windows.Forms.Button InMenuService;
        private System.Windows.Forms.Button AddClient;
        private System.Windows.Forms.Button InMenuClient;
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
    }
}