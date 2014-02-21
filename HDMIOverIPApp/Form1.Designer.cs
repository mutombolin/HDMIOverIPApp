namespace HDMIOverIPApp
{
    partial class Form1
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label_IpAddress = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label_Port = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnStatus = new System.Windows.Forms.Button();
            this.btnSTB = new System.Windows.Forms.Button();
            this.btnMCU = new System.Windows.Forms.Button();
            this.btnIRProtocol = new System.Windows.Forms.Button();
            this.btnIRPattern = new System.Windows.Forms.Button();
            this.btnTV = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Arial monospaced for SAP", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(118, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(292, 29);
            this.textBox1.TabIndex = 0;
            // 
            // label_IpAddress
            // 
            this.label_IpAddress.AutoSize = true;
            this.label_IpAddress.Font = new System.Drawing.Font("Arial", 14F);
            this.label_IpAddress.Location = new System.Drawing.Point(9, 6);
            this.label_IpAddress.Name = "label_IpAddress";
            this.label_IpAddress.Size = new System.Drawing.Size(103, 22);
            this.label_IpAddress.TabIndex = 1;
            this.label_IpAddress.Text = "Ip Address";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBox2);
            this.panel1.Controls.Add(this.label_Port);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.label_IpAddress);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(646, 94);
            this.panel1.TabIndex = 2;
            // 
            // label_Port
            // 
            this.label_Port.AutoSize = true;
            this.label_Port.Font = new System.Drawing.Font("Arial", 14F);
            this.label_Port.Location = new System.Drawing.Point(67, 46);
            this.label_Port.Name = "label_Port";
            this.label_Port.Size = new System.Drawing.Size(45, 22);
            this.label_Port.TabIndex = 2;
            this.label_Port.Text = "Port";
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Arial monospaced for SAP", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(118, 46);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(292, 29);
            this.textBox2.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnTV);
            this.panel2.Controls.Add(this.btnIRPattern);
            this.panel2.Controls.Add(this.btnIRProtocol);
            this.panel2.Controls.Add(this.btnMCU);
            this.panel2.Controls.Add(this.btnSTB);
            this.panel2.Controls.Add(this.btnStatus);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 94);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(646, 58);
            this.panel2.TabIndex = 3;
            // 
            // btnStatus
            // 
            this.btnStatus.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnStatus.Location = new System.Drawing.Point(13, 6);
            this.btnStatus.Name = "btnStatus";
            this.btnStatus.Size = new System.Drawing.Size(99, 42);
            this.btnStatus.TabIndex = 0;
            this.btnStatus.Text = "Status";
            this.btnStatus.UseVisualStyleBackColor = true;
            this.btnStatus.Click += new System.EventHandler(this.btnStatus_Click);
            // 
            // btnSTB
            // 
            this.btnSTB.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnSTB.Location = new System.Drawing.Point(118, 6);
            this.btnSTB.Name = "btnSTB";
            this.btnSTB.Size = new System.Drawing.Size(99, 42);
            this.btnSTB.TabIndex = 1;
            this.btnSTB.Text = "STB";
            this.btnSTB.UseVisualStyleBackColor = true;
            // 
            // btnMCU
            // 
            this.btnMCU.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnMCU.Location = new System.Drawing.Point(223, 6);
            this.btnMCU.Name = "btnMCU";
            this.btnMCU.Size = new System.Drawing.Size(99, 42);
            this.btnMCU.TabIndex = 2;
            this.btnMCU.Text = "MCU";
            this.btnMCU.UseVisualStyleBackColor = true;
            // 
            // btnIRProtocol
            // 
            this.btnIRProtocol.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnIRProtocol.Location = new System.Drawing.Point(328, 6);
            this.btnIRProtocol.Name = "btnIRProtocol";
            this.btnIRProtocol.Size = new System.Drawing.Size(99, 42);
            this.btnIRProtocol.TabIndex = 3;
            this.btnIRProtocol.Text = "IR";
            this.btnIRProtocol.UseVisualStyleBackColor = true;
            // 
            // btnIRPattern
            // 
            this.btnIRPattern.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnIRPattern.Location = new System.Drawing.Point(433, 6);
            this.btnIRPattern.Name = "btnIRPattern";
            this.btnIRPattern.Size = new System.Drawing.Size(99, 42);
            this.btnIRPattern.TabIndex = 4;
            this.btnIRPattern.Text = "IR Raw";
            this.btnIRPattern.UseVisualStyleBackColor = true;
            // 
            // btnTV
            // 
            this.btnTV.AccessibleDescription = "Set TV Command";
            this.btnTV.AccessibleName = "name";
            this.btnTV.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.btnTV.Location = new System.Drawing.Point(538, 6);
            this.btnTV.Name = "btnTV";
            this.btnTV.Size = new System.Drawing.Size(99, 42);
            this.btnTV.TabIndex = 5;
            this.btnTV.Text = "TV";
            this.btnTV.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.listBox1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 296);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(646, 199);
            this.panel3.TabIndex = 4;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 18;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(646, 199);
            this.listBox1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 495);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label_IpAddress;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label_Port;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnMCU;
        private System.Windows.Forms.Button btnSTB;
        private System.Windows.Forms.Button btnStatus;
        private System.Windows.Forms.Button btnTV;
        private System.Windows.Forms.Button btnIRPattern;
        private System.Windows.Forms.Button btnIRProtocol;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ListBox listBox1;
    }
}

