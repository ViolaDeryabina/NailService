namespace NailService
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Login = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Autorization = new System.Windows.Forms.Button();
            this.Exit = new System.Windows.Forms.Button();
            this.Eye = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Eye)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(71, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 34);
            this.label1.TabIndex = 0;
            this.label1.Text = "Авторизация";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(16, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Логин";
            // 
            // Login
            // 
            this.Login.Location = new System.Drawing.Point(19, 112);
            this.Login.Name = "Login";
            this.Login.Size = new System.Drawing.Size(313, 31);
            this.Login.TabIndex = 2;
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(19, 181);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(313, 31);
            this.Password.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(16, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 24);
            this.label3.TabIndex = 3;
            this.label3.Text = "Пароль";
            // 
            // Autorization
            // 
            this.Autorization.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.Autorization.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Autorization.Location = new System.Drawing.Point(19, 258);
            this.Autorization.Name = "Autorization";
            this.Autorization.Size = new System.Drawing.Size(313, 43);
            this.Autorization.TabIndex = 5;
            this.Autorization.Text = "Войти";
            this.Autorization.UseVisualStyleBackColor = false;
            this.Autorization.Click += new System.EventHandler(this.Autorization_Click);
            // 
            // Exit
            // 
            this.Exit.BackColor = System.Drawing.Color.HotPink;
            this.Exit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Exit.Location = new System.Drawing.Point(19, 318);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(313, 43);
            this.Exit.TabIndex = 6;
            this.Exit.Text = "Выйти";
            this.Exit.UseVisualStyleBackColor = false;
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // Eye
            // 
            this.Eye.Location = new System.Drawing.Point(339, 181);
            this.Eye.Name = "Eye";
            this.Eye.Size = new System.Drawing.Size(35, 35);
            this.Eye.TabIndex = 7;
            this.Eye.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(383, 394);
            this.Controls.Add(this.Eye);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.Autorization);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Login);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Авторизация";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.Eye)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Login;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Autorization;
        private System.Windows.Forms.Button Exit;
        private System.Windows.Forms.PictureBox Eye;
    }
}

