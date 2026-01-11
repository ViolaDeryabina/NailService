namespace NailService
{
    partial class AddUserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddUserForm));
            this.label1 = new System.Windows.Forms.Label();
            this.LastName = new System.Windows.Forms.TextBox();
            this.RoleCb = new System.Windows.Forms.ComboBox();
            this.LoginGenerate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.PasswordGenerate = new System.Windows.Forms.Button();
            this.FirstName = new System.Windows.Forms.TextBox();
            this.MiddleName = new System.Windows.Forms.TextBox();
            this.Login = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 18.25F);
            this.label1.Location = new System.Drawing.Point(182, -1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(321, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "Добавить пользователя";
            // 
            // LastName
            // 
            this.LastName.Font = new System.Drawing.Font("MS Reference Sans Serif", 15.25F);
            this.LastName.Location = new System.Drawing.Point(126, 71);
            this.LastName.Name = "LastName";
            this.LastName.Size = new System.Drawing.Size(354, 32);
            this.LastName.TabIndex = 1;
            this.LastName.TextChanged += new System.EventHandler(this.LastName_TextChanged);
            // 
            // RoleCb
            // 
            this.RoleCb.FormattingEnabled = true;
            this.RoleCb.Location = new System.Drawing.Point(126, 270);
            this.RoleCb.Name = "RoleCb";
            this.RoleCb.Size = new System.Drawing.Size(354, 32);
            this.RoleCb.TabIndex = 2;
            // 
            // LoginGenerate
            // 
            this.LoginGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.LoginGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.LoginGenerate.Location = new System.Drawing.Point(493, 194);
            this.LoginGenerate.Name = "LoginGenerate";
            this.LoginGenerate.Size = new System.Drawing.Size(172, 35);
            this.LoginGenerate.TabIndex = 3;
            this.LoginGenerate.Text = "Сгенирировать";
            this.LoginGenerate.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 24);
            this.label2.TabIndex = 4;
            this.label2.Text = "Фамилия";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 24);
            this.label3.TabIndex = 5;
            this.label3.Text = "Имя";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 24);
            this.label4.TabIndex = 6;
            this.label4.Text = "Отчество";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 199);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 24);
            this.label5.TabIndex = 7;
            this.label5.Text = "Логин";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 238);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 24);
            this.label6.TabIndex = 8;
            this.label6.Text = "Пароль";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 273);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 24);
            this.label7.TabIndex = 9;
            this.label7.Text = "Роль";
            // 
            // PasswordGenerate
            // 
            this.PasswordGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.PasswordGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.PasswordGenerate.Location = new System.Drawing.Point(493, 234);
            this.PasswordGenerate.Name = "PasswordGenerate";
            this.PasswordGenerate.Size = new System.Drawing.Size(172, 35);
            this.PasswordGenerate.TabIndex = 10;
            this.PasswordGenerate.Text = "Сгенирировать";
            this.PasswordGenerate.UseVisualStyleBackColor = false;
            // 
            // FirstName
            // 
            this.FirstName.Font = new System.Drawing.Font("MS Reference Sans Serif", 15.25F);
            this.FirstName.Location = new System.Drawing.Point(126, 114);
            this.FirstName.Name = "FirstName";
            this.FirstName.Size = new System.Drawing.Size(354, 32);
            this.FirstName.TabIndex = 11;
            this.FirstName.TextChanged += new System.EventHandler(this.FirstName_TextChanged);
            // 
            // MiddleName
            // 
            this.MiddleName.Font = new System.Drawing.Font("MS Reference Sans Serif", 15.25F);
            this.MiddleName.Location = new System.Drawing.Point(126, 154);
            this.MiddleName.Name = "MiddleName";
            this.MiddleName.Size = new System.Drawing.Size(354, 32);
            this.MiddleName.TabIndex = 12;
            this.MiddleName.TextChanged += new System.EventHandler(this.MiddleName_TextChanged);
            // 
            // Login
            // 
            this.Login.Font = new System.Drawing.Font("MS Reference Sans Serif", 15.25F);
            this.Login.Location = new System.Drawing.Point(126, 197);
            this.Login.Name = "Login";
            this.Login.Size = new System.Drawing.Size(354, 32);
            this.Login.TabIndex = 13;
            // 
            // Password
            // 
            this.Password.Font = new System.Drawing.Font("MS Reference Sans Serif", 15.25F);
            this.Password.Location = new System.Drawing.Point(126, 234);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(354, 32);
            this.Password.TabIndex = 14;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(12, 356);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(223, 47);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.HotPink;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Location = new System.Drawing.Point(442, 356);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(223, 47);
            this.button4.TabIndex = 16;
            this.button4.Text = "Добавить";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // AddUserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(687, 424);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Login);
            this.Controls.Add(this.MiddleName);
            this.Controls.Add(this.FirstName);
            this.Controls.Add(this.PasswordGenerate);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LoginGenerate);
            this.Controls.Add(this.RoleCb);
            this.Controls.Add(this.LastName);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "AddUserForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Добавление пользователя";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LastName;
        private System.Windows.Forms.ComboBox RoleCb;
        private System.Windows.Forms.Button LoginGenerate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button PasswordGenerate;
        private System.Windows.Forms.TextBox FirstName;
        private System.Windows.Forms.TextBox MiddleName;
        private System.Windows.Forms.TextBox Login;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button button4;
    }
}