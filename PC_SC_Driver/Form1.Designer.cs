namespace PC_SC_Driver
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
            this.connect = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btn_onlyRead = new System.Windows.Forms.Button();
            this.btn_andWrite = new System.Windows.Forms.Button();
            this.btn_writeTo = new System.Windows.Forms.Button();
            this.btn_erase = new System.Windows.Forms.Button();
            this.btn_reRead = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // connect
            // 
            this.connect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connect.Location = new System.Drawing.Point(381, 10);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(75, 23);
            this.connect.TabIndex = 0;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 82);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(444, 264);
            this.textBox1.TabIndex = 1;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(363, 21);
            this.comboBox1.TabIndex = 2;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // btn_onlyRead
            // 
            this.btn_onlyRead.Location = new System.Drawing.Point(12, 39);
            this.btn_onlyRead.Name = "btn_onlyRead";
            this.btn_onlyRead.Size = new System.Drawing.Size(75, 37);
            this.btn_onlyRead.TabIndex = 3;
            this.btn_onlyRead.Text = "2 sec - only read";
            this.btn_onlyRead.UseVisualStyleBackColor = true;
            this.btn_onlyRead.Click += new System.EventHandler(this.btn_onlyRead_Click);
            // 
            // btn_andWrite
            // 
            this.btn_andWrite.Location = new System.Drawing.Point(93, 39);
            this.btn_andWrite.Name = "btn_andWrite";
            this.btn_andWrite.Size = new System.Drawing.Size(75, 37);
            this.btn_andWrite.TabIndex = 3;
            this.btn_andWrite.Text = "2 sec - write";
            this.btn_andWrite.UseVisualStyleBackColor = true;
            this.btn_andWrite.Click += new System.EventHandler(this.btn_andWrite_Click);
            // 
            // btn_writeTo
            // 
            this.btn_writeTo.Location = new System.Drawing.Point(174, 39);
            this.btn_writeTo.Name = "btn_writeTo";
            this.btn_writeTo.Size = new System.Drawing.Size(77, 37);
            this.btn_writeTo.TabIndex = 3;
            this.btn_writeTo.Text = "write to 2 sec - 012345";
            this.btn_writeTo.UseVisualStyleBackColor = true;
            this.btn_writeTo.Click += new System.EventHandler(this.btn_writeTo_Click);
            // 
            // btn_erase
            // 
            this.btn_erase.Location = new System.Drawing.Point(300, 39);
            this.btn_erase.Name = "btn_erase";
            this.btn_erase.Size = new System.Drawing.Size(75, 37);
            this.btn_erase.TabIndex = 3;
            this.btn_erase.Text = "erase 2 sec";
            this.btn_erase.UseVisualStyleBackColor = true;
            this.btn_erase.Click += new System.EventHandler(this.btn_erase_Click);
            // 
            // btn_reRead
            // 
            this.btn_reRead.Location = new System.Drawing.Point(381, 39);
            this.btn_reRead.Name = "btn_reRead";
            this.btn_reRead.Size = new System.Drawing.Size(75, 37);
            this.btn_reRead.TabIndex = 4;
            this.btn_reRead.Text = "re read";
            this.btn_reRead.UseVisualStyleBackColor = true;
            this.btn_reRead.Click += new System.EventHandler(this.btn_reRead_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(257, 39);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(37, 37);
            this.button1.TabIndex = 5;
            this.button1.Text = "write to 2 sec - 012345";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 358);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_reRead);
            this.Controls.Add(this.btn_erase);
            this.Controls.Add(this.btn_writeTo);
            this.Controls.Add(this.btn_andWrite);
            this.Controls.Add(this.btn_onlyRead);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.connect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(484, 397);
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Тест считывателя Mifare";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btn_onlyRead;
        private System.Windows.Forms.Button btn_andWrite;
        private System.Windows.Forms.Button btn_writeTo;
        private System.Windows.Forms.Button btn_erase;
        private System.Windows.Forms.Button btn_reRead;
        private System.Windows.Forms.Button button1;
    }
}

