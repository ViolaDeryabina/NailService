namespace NailService
{
    partial class EditMasterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditMasterForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Phone = new System.Windows.Forms.TextBox();
            this.Description = new System.Windows.Forms.TextBox();
            this.EditMaster = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.FIO = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 20.25F);
            this.label1.Location = new System.Drawing.Point(184, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(340, 34);
            this.label1.TabIndex = 0;
            this.label1.Text = "Редактировать мастера";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "ФИО";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "Телефон";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "Описание";
            // 
            // Phone
            // 
            this.Phone.Location = new System.Drawing.Point(133, 120);
            this.Phone.Name = "Phone";
            this.Phone.Size = new System.Drawing.Size(441, 31);
            this.Phone.TabIndex = 5;
            this.Phone.TextChanged += new System.EventHandler(this.Phone_TextChanged);
            // 
            // Description
            // 
            this.Description.Location = new System.Drawing.Point(133, 161);
            this.Description.Multiline = true;
            this.Description.Name = "Description";
            this.Description.Size = new System.Drawing.Size(441, 148);
            this.Description.TabIndex = 6;
            // 
            // EditMaster
            // 
            this.EditMaster.BackColor = System.Drawing.Color.HotPink;
            this.EditMaster.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.EditMaster.Location = new System.Drawing.Point(351, 355);
            this.EditMaster.Name = "EditMaster";
            this.EditMaster.Size = new System.Drawing.Size(223, 47);
            this.EditMaster.TabIndex = 7;
            this.EditMaster.Text = "Редактировать";
            this.EditMaster.UseVisualStyleBackColor = false;
            this.EditMaster.Click += new System.EventHandler(this.EditMaster_Click);
            // 
            // Back
            // 
            this.Back.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.Back.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Back.Location = new System.Drawing.Point(16, 355);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(223, 47);
            this.Back.TabIndex = 8;
            this.Back.Text = "Отмена";
            this.Back.UseVisualStyleBackColor = false;
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::NailService.Properties.Resources.back_1;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(10, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 72);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // FIO
            // 
            this.FIO.Location = new System.Drawing.Point(133, 75);
            this.FIO.Name = "FIO";
            this.FIO.Size = new System.Drawing.Size(441, 31);
            this.FIO.TabIndex = 10;
            // 
            // EditMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(602, 424);
            this.Controls.Add(this.FIO);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.EditMaster);
            this.Controls.Add(this.Description);
            this.Controls.Add(this.Phone);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditMasterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Редактирование мастера";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Phone;
        private System.Windows.Forms.TextBox Description;
        private System.Windows.Forms.Button EditMaster;
        private System.Windows.Forms.Button Back;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox FIO;
    }
}