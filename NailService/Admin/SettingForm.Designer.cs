namespace NailService
{
    partial class SettingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Server = new System.Windows.Forms.TextBox();
            this.DB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.NameUser = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ConnectionButton = new System.Windows.Forms.Button();
            this.numericTimeout = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.Eye = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Eye)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F);
            this.label1.Location = new System.Drawing.Point(92, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Настройки";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(177, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Название сервера";
            // 
            // Server
            // 
            this.Server.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.25F);
            this.Server.Location = new System.Drawing.Point(23, 92);
            this.Server.Name = "Server";
            this.Server.Size = new System.Drawing.Size(297, 32);
            this.Server.TabIndex = 2;
            // 
            // DB
            // 
            this.DB.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.25F);
            this.DB.Location = new System.Drawing.Point(23, 158);
            this.DB.Name = "DB";
            this.DB.Size = new System.Drawing.Size(297, 32);
            this.DB.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 24);
            this.label3.TabIndex = 3;
            this.label3.Text = "База данных";
            // 
            // NameUser
            // 
            this.NameUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.25F);
            this.NameUser.Location = new System.Drawing.Point(23, 224);
            this.NameUser.Name = "NameUser";
            this.NameUser.Size = new System.Drawing.Size(297, 32);
            this.NameUser.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 197);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(177, 24);
            this.label4.TabIndex = 5;
            this.label4.Text = "Имя пользователя";
            // 
            // Password
            // 
            this.Password.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.25F);
            this.Password.Location = new System.Drawing.Point(23, 290);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(297, 32);
            this.Password.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 263);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(207, 24);
            this.label5.TabIndex = 7;
            this.label5.Text = "Пароль пользователя";
            // 
            // ConnectionButton
            // 
            this.ConnectionButton.BackColor = System.Drawing.Color.HotPink;
            this.ConnectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ConnectionButton.Location = new System.Drawing.Point(23, 412);
            this.ConnectionButton.Name = "ConnectionButton";
            this.ConnectionButton.Size = new System.Drawing.Size(297, 49);
            this.ConnectionButton.TabIndex = 9;
            this.ConnectionButton.Text = "Установить соединение";
            this.ConnectionButton.UseVisualStyleBackColor = false;
            this.ConnectionButton.Click += new System.EventHandler(this.ConnectionButton_Click);
            // 
            // numericTimeout
            // 
            this.numericTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.25F);
            this.numericTimeout.Location = new System.Drawing.Point(23, 360);
            this.numericTimeout.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numericTimeout.Name = "numericTimeout";
            this.numericTimeout.Size = new System.Drawing.Size(297, 32);
            this.numericTimeout.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 331);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(282, 24);
            this.label6.TabIndex = 12;
            this.label6.Text = "Время бездействия (секунды)";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(23, 522);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(297, 49);
            this.button1.TabIndex = 13;
            this.button1.Text = "Назад";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.HotPink;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Location = new System.Drawing.Point(23, 467);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(297, 49);
            this.button2.TabIndex = 14;
            this.button2.Text = "Проверить соединение";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Eye
            // 
            this.Eye.Location = new System.Drawing.Point(326, 290);
            this.Eye.Name = "Eye";
            this.Eye.Size = new System.Drawing.Size(33, 32);
            this.Eye.TabIndex = 15;
            this.Eye.TabStop = false;
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(368, 586);
            this.Controls.Add(this.Eye);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericTimeout);
            this.Controls.Add(this.ConnectionButton);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.NameUser);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.DB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Server);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Настройки";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numericTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Eye)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Server;
        private System.Windows.Forms.TextBox DB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox NameUser;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button ConnectionButton;
        private System.Windows.Forms.NumericUpDown numericTimeout;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox Eye;
    }
}