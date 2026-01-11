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
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.AddRole = new System.Windows.Forms.Button();
            this.InMenuRole = new System.Windows.Forms.Button();
            this.dataGridViewRoles = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.AddService = new System.Windows.Forms.Button();
            this.InMenuService = new System.Windows.Forms.Button();
            this.dataGridViewServices = new System.Windows.Forms.DataGridView();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.AddClient = new System.Windows.Forms.Button();
            this.InMenuClient = new System.Windows.Forms.Button();
            this.dataGridViewClients = new System.Windows.Forms.DataGridView();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.RoleTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.EditRole = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Users)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMasters)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRoles)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewServices)).BeginInit();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClients)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new System.Drawing.Font("MS Reference Sans Serif", 12.25F);
            this.tabControl1.Location = new System.Drawing.Point(3, 2);
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
            this.AddUsers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.AddUsers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddUsers.Location = new System.Drawing.Point(6, 470);
            this.AddUsers.Name = "AddUsers";
            this.AddUsers.Size = new System.Drawing.Size(281, 48);
            this.AddUsers.TabIndex = 2;
            this.AddUsers.Text = "Добавить пользователя";
            this.AddUsers.UseVisualStyleBackColor = false;
            this.AddUsers.Click += new System.EventHandler(this.AddUsers_Click);
            // 
            // InMenu
            // 
            this.InMenu.BackColor = System.Drawing.Color.HotPink;
            this.InMenu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenu.Location = new System.Drawing.Point(683, 469);
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
            this.InMenuMaster.BackColor = System.Drawing.Color.HotPink;
            this.InMenuMaster.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuMaster.Location = new System.Drawing.Point(683, 469);
            this.InMenuMaster.Name = "InMenuMaster";
            this.InMenuMaster.Size = new System.Drawing.Size(281, 48);
            this.InMenuMaster.TabIndex = 4;
            this.InMenuMaster.Text = "В меню";
            this.InMenuMaster.UseVisualStyleBackColor = false;
            this.InMenuMaster.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // AddMaster
            // 
            this.AddMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.AddMaster.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddMaster.Location = new System.Drawing.Point(6, 470);
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
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.EditRole);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.RoleTextBox);
            this.tabPage3.Controls.Add(this.AddRole);
            this.tabPage3.Controls.Add(this.InMenuRole);
            this.tabPage3.Controls.Add(this.dataGridViewRoles);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(970, 523);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Роли";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // AddRole
            // 
            this.AddRole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.AddRole.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddRole.Location = new System.Drawing.Point(6, 470);
            this.AddRole.Name = "AddRole";
            this.AddRole.Size = new System.Drawing.Size(281, 48);
            this.AddRole.TabIndex = 6;
            this.AddRole.Text = "Добавить роль";
            this.AddRole.UseVisualStyleBackColor = false;
            // 
            // InMenuRole
            // 
            this.InMenuRole.BackColor = System.Drawing.Color.HotPink;
            this.InMenuRole.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuRole.Location = new System.Drawing.Point(683, 468);
            this.InMenuRole.Name = "InMenuRole";
            this.InMenuRole.Size = new System.Drawing.Size(281, 48);
            this.InMenuRole.TabIndex = 5;
            this.InMenuRole.Text = "В меню";
            this.InMenuRole.UseVisualStyleBackColor = false;
            this.InMenuRole.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // dataGridViewRoles
            // 
            this.dataGridViewRoles.AllowUserToAddRows = false;
            this.dataGridViewRoles.AllowUserToDeleteRows = false;
            this.dataGridViewRoles.AllowUserToResizeColumns = false;
            this.dataGridViewRoles.AllowUserToResizeRows = false;
            this.dataGridViewRoles.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewRoles.Location = new System.Drawing.Point(6, 7);
            this.dataGridViewRoles.Name = "dataGridViewRoles";
            this.dataGridViewRoles.Size = new System.Drawing.Size(958, 363);
            this.dataGridViewRoles.TabIndex = 0;
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
            this.AddService.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.AddService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddService.Location = new System.Drawing.Point(6, 469);
            this.AddService.Name = "AddService";
            this.AddService.Size = new System.Drawing.Size(281, 48);
            this.AddService.TabIndex = 7;
            this.AddService.Text = "Добавить уcлугу";
            this.AddService.UseVisualStyleBackColor = false;
            this.AddService.Click += new System.EventHandler(this.AddService_Click);
            // 
            // InMenuService
            // 
            this.InMenuService.BackColor = System.Drawing.Color.HotPink;
            this.InMenuService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuService.Location = new System.Drawing.Point(683, 469);
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
            this.AddClient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.AddClient.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddClient.Location = new System.Drawing.Point(6, 469);
            this.AddClient.Name = "AddClient";
            this.AddClient.Size = new System.Drawing.Size(281, 48);
            this.AddClient.TabIndex = 7;
            this.AddClient.Text = "Добавить клиента";
            this.AddClient.UseVisualStyleBackColor = false;
            this.AddClient.Click += new System.EventHandler(this.AddClient_Click);
            // 
            // InMenuClient
            // 
            this.InMenuClient.BackColor = System.Drawing.Color.HotPink;
            this.InMenuClient.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuClient.Location = new System.Drawing.Point(683, 469);
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
            this.dataGridViewClients.Size = new System.Drawing.Size(958, 445);
            this.dataGridViewClients.TabIndex = 0;
            // 
            // RoleTextBox
            // 
            this.RoleTextBox.Location = new System.Drawing.Point(7, 424);
            this.RoleTextBox.Name = "RoleTextBox";
            this.RoleTextBox.Size = new System.Drawing.Size(280, 27);
            this.RoleTextBox.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 399);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 22);
            this.label1.TabIndex = 8;
            this.label1.Text = "Название роли";
            // 
            // EditRole
            // 
            this.EditRole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.EditRole.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.EditRole.Location = new System.Drawing.Point(293, 469);
            this.EditRole.Name = "EditRole";
            this.EditRole.Size = new System.Drawing.Size(281, 48);
            this.EditRole.TabIndex = 9;
            this.EditRole.Text = "Редактировать";
            this.EditRole.UseVisualStyleBackColor = false;
            // 
            // Show
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Show";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Просмотр пользователей";
            this.Load += new System.EventHandler(this.Show_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Users)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMasters)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRoles)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewServices)).EndInit();
            this.tabPage5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClients)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.DataGridView Users;
        private System.Windows.Forms.DataGridView dataGridViewMasters;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.DataGridView dataGridViewRoles;
        private System.Windows.Forms.DataGridView dataGridViewServices;
        private System.Windows.Forms.DataGridView dataGridViewClients;
        private System.Windows.Forms.Button InMenu;
        private System.Windows.Forms.Button AddUsers;
        private System.Windows.Forms.Button AddMaster;
        private System.Windows.Forms.Button InMenuMaster;
        private System.Windows.Forms.Button AddRole;
        private System.Windows.Forms.Button InMenuRole;
        private System.Windows.Forms.Button AddService;
        private System.Windows.Forms.Button InMenuService;
        private System.Windows.Forms.Button AddClient;
        private System.Windows.Forms.Button InMenuClient;
        private System.Windows.Forms.Button EditRole;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox RoleTextBox;
    }
}