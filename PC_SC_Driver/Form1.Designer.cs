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
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // connect
            // 
            this.connect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connect.Location = new System.Drawing.Point(957, 15);
            this.connect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(112, 35);
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
            this.textBox1.Location = new System.Drawing.Point(18, 126);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(1049, 404);
            this.textBox1.TabIndex = 1;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(18, 18);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(927, 28);
            this.comboBox1.TabIndex = 2;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // btn_onlyRead
            // 
            this.btn_onlyRead.Location = new System.Drawing.Point(18, 60);
            this.btn_onlyRead.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_onlyRead.Name = "btn_onlyRead";
            this.btn_onlyRead.Size = new System.Drawing.Size(95, 57);
            this.btn_onlyRead.TabIndex = 3;
            this.btn_onlyRead.Text = "2 sec - only read";
            this.btn_onlyRead.UseVisualStyleBackColor = true;
            this.btn_onlyRead.Click += new System.EventHandler(this.btn_onlyRead_Click);
            // 
            // btn_andWrite
            // 
            this.btn_andWrite.Location = new System.Drawing.Point(121, 60);
            this.btn_andWrite.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_andWrite.Name = "btn_andWrite";
            this.btn_andWrite.Size = new System.Drawing.Size(65, 57);
            this.btn_andWrite.TabIndex = 3;
            this.btn_andWrite.Text = "2 sec - write";
            this.btn_andWrite.UseVisualStyleBackColor = true;
            this.btn_andWrite.Click += new System.EventHandler(this.btn_andWrite_Click);
            // 
            // btn_writeTo
            // 
            this.btn_writeTo.Location = new System.Drawing.Point(194, 60);
            this.btn_writeTo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_writeTo.Name = "btn_writeTo";
            this.btn_writeTo.Size = new System.Drawing.Size(116, 57);
            this.btn_writeTo.TabIndex = 3;
            this.btn_writeTo.Text = "write to 2 sec - 012345";
            this.btn_writeTo.UseVisualStyleBackColor = true;
            this.btn_writeTo.Click += new System.EventHandler(this.btn_writeTo_Click);
            // 
            // btn_erase
            // 
            this.btn_erase.Location = new System.Drawing.Point(450, 60);
            this.btn_erase.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_erase.Name = "btn_erase";
            this.btn_erase.Size = new System.Drawing.Size(110, 29);
            this.btn_erase.TabIndex = 3;
            this.btn_erase.Text = "erase";
            this.btn_erase.UseVisualStyleBackColor = true;
            this.btn_erase.Click += new System.EventHandler(this.btn_erase_Click);
            // 
            // btn_reRead
            // 
            this.btn_reRead.Location = new System.Drawing.Point(572, 60);
            this.btn_reRead.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_reRead.Name = "btn_reRead";
            this.btn_reRead.Size = new System.Drawing.Size(112, 57);
            this.btn_reRead.TabIndex = 4;
            this.btn_reRead.Text = "re read";
            this.btn_reRead.UseVisualStyleBackColor = true;
            this.btn_reRead.Click += new System.EventHandler(this.btn_reRead_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(322, 60);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 57);
            this.button1.TabIndex = 5;
            this.button1.Text = "use API";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.useAPI_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(450, 88);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(110, 29);
            this.button2.TabIndex = 6;
            this.button2.Text = "write test";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button_writeTest_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(386, 60);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(56, 57);
            this.button3.TabIndex = 7;
            this.button3.Text = "read API";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.readAPI_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1078, 525);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
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
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1100, 581);
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
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

