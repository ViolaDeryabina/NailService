namespace NailService
{
    partial class AddServiceForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddServiceForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.NameService = new System.Windows.Forms.TextBox();
            this.Price = new System.Windows.Forms.TextBox();
            this.Description = new System.Windows.Forms.TextBox();
            this.Category = new System.Windows.Forms.ComboBox();
            this.AddService = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.lblCharCount = new System.Windows.Forms.Label();
            this.pictureBoxService = new System.Windows.Forms.PictureBox();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxService)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F);
            this.label1.Location = new System.Drawing.Point(255, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Добавить услугу";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Название";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "Категория";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "Цена";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 204);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 24);
            this.label5.TabIndex = 4;
            this.label5.Text = "Описание";
            // 
            // NameService
            // 
            this.NameService.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.NameService.Location = new System.Drawing.Point(139, 75);
            this.NameService.Name = "NameService";
            this.NameService.Size = new System.Drawing.Size(354, 31);
            this.NameService.TabIndex = 5;
            this.NameService.TextChanged += new System.EventHandler(this.NameService_TextChanged);
            this.NameService.Leave += new System.EventHandler(this.NameService_Leave);
            this.NameService.Validating += new System.ComponentModel.CancelEventHandler(this.NameService_Validating);
            // 
            // Price
            // 
            this.Price.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.Price.Location = new System.Drawing.Point(139, 161);
            this.Price.Name = "Price";
            this.Price.Size = new System.Drawing.Size(354, 31);
            this.Price.TabIndex = 6;
            this.Price.TextChanged += new System.EventHandler(this.Price_TextChanged);
            // 
            // Description
            // 
            this.Description.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.Description.Location = new System.Drawing.Point(139, 204);
            this.Description.Multiline = true;
            this.Description.Name = "Description";
            this.Description.Size = new System.Drawing.Size(354, 128);
            this.Description.TabIndex = 7;
            this.Description.TextChanged += new System.EventHandler(this.Description_TextChanged);
            // 
            // Category
            // 
            this.Category.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Category.FormattingEnabled = true;
            this.Category.Location = new System.Drawing.Point(139, 118);
            this.Category.Name = "Category";
            this.Category.Size = new System.Drawing.Size(354, 32);
            this.Category.TabIndex = 8;
            // 
            // AddService
            // 
            this.AddService.BackColor = System.Drawing.Color.HotPink;
            this.AddService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddService.Location = new System.Drawing.Point(270, 372);
            this.AddService.Name = "AddService";
            this.AddService.Size = new System.Drawing.Size(223, 47);
            this.AddService.TabIndex = 9;
            this.AddService.Text = "Добавить";
            this.AddService.UseVisualStyleBackColor = false;
            this.AddService.Click += new System.EventHandler(this.AddService_Click);
            // 
            // Back
            // 
            this.Back.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.Back.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Back.Location = new System.Drawing.Point(16, 373);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(223, 47);
            this.Back.TabIndex = 10;
            this.Back.Text = "Отмена";
            this.Back.UseVisualStyleBackColor = false;
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // lblCharCount
            // 
            this.lblCharCount.AutoSize = true;
            this.lblCharCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.lblCharCount.Location = new System.Drawing.Point(425, 335);
            this.lblCharCount.Name = "lblCharCount";
            this.lblCharCount.Size = new System.Drawing.Size(50, 20);
            this.lblCharCount.TabIndex = 11;
            this.lblCharCount.Text = "0/500";
            // 
            // pictureBoxService
            // 
            this.pictureBoxService.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxService.Location = new System.Drawing.Point(20, 28);
            this.pictureBoxService.Name = "pictureBoxService";
            this.pictureBoxService.Size = new System.Drawing.Size(230, 230);
            this.pictureBoxService.TabIndex = 12;
            this.pictureBoxService.TabStop = false;
            this.pictureBoxService.Click += new System.EventHandler(this.pictureBoxService_Click);
            this.pictureBoxService.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBoxService_DragDrop);
            this.pictureBoxService.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBoxService_DragEnter);
            this.pictureBoxService.MouseHover += new System.EventHandler(this.pictureBoxService_MouseHover);
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Location = new System.Drawing.Point(20, 267);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(230, 32);
            this.btnLoadImage.TabIndex = 13;
            this.btnLoadImage.Text = "Выбрать";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::NailService.Properties.Resources.back_1;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(10, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 72);
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(203)))), ((int)(((byte)(219)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(20, 305);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(230, 32);
            this.button1.TabIndex = 15;
            this.button1.Text = "Очистить";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.pictureBoxService);
            this.groupBox1.Controls.Add(this.btnLoadImage);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.groupBox1.Location = new System.Drawing.Point(510, 66);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(265, 356);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Фото";
            // 
            // AddServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(787, 431);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblCharCount);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.AddService);
            this.Controls.Add(this.Category);
            this.Controls.Add(this.Description);
            this.Controls.Add(this.Price);
            this.Controls.Add(this.NameService);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddServiceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Добавить услугу";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxService)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox NameService;
        private System.Windows.Forms.TextBox Price;
        private System.Windows.Forms.TextBox Description;
        private System.Windows.Forms.ComboBox Category;
        private System.Windows.Forms.Button AddService;
        private System.Windows.Forms.Button Back;
        private System.Windows.Forms.Label lblCharCount;
        private System.Windows.Forms.PictureBox pictureBoxService;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}