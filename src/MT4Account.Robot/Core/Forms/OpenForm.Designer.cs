namespace MT4Account.Robot
{
    partial class OpenForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.comCmd = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtVlome = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridViewOpenData = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbSymbol = new System.Windows.Forms.ComboBox();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LoginId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StartAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Ratio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.操作 = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOpenData)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(649, 454);
            this.splitContainer1.SplitterDistance = 100;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbSymbol);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnOpen);
            this.groupBox1.Controls.Add(this.comCmd);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtVlome);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(649, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "开仓参数设置";
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(546, 37);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 4;
            this.btnOpen.Text = "开仓";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // comCmd
            // 
            this.comCmd.FormattingEnabled = true;
            this.comCmd.Items.AddRange(new object[] {
            "buy",
            "sell"});
            this.comCmd.Location = new System.Drawing.Point(221, 40);
            this.comCmd.Name = "comCmd";
            this.comCmd.Size = new System.Drawing.Size(121, 20);
            this.comCmd.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "方向：";
            // 
            // txtVlome
            // 
            this.txtVlome.Location = new System.Drawing.Point(68, 39);
            this.txtVlome.Name = "txtVlome";
            this.txtVlome.Size = new System.Drawing.Size(100, 21);
            this.txtVlome.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "手数：";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dataGridViewOpenData);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(649, 350);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "开仓账户";
            // 
            // dataGridViewOpenData
            // 
            this.dataGridViewOpenData.AllowUserToAddRows = false;
            this.dataGridViewOpenData.AllowUserToDeleteRows = false;
            this.dataGridViewOpenData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewOpenData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOpenData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.LoginId,
            this.StartAmount,
            this.Ratio,
            this.操作});
            this.dataGridViewOpenData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOpenData.Location = new System.Drawing.Point(3, 17);
            this.dataGridViewOpenData.Name = "dataGridViewOpenData";
            this.dataGridViewOpenData.ReadOnly = true;
            this.dataGridViewOpenData.RowTemplate.Height = 23;
            this.dataGridViewOpenData.Size = new System.Drawing.Size(643, 330);
            this.dataGridViewOpenData.TabIndex = 0;
            this.dataGridViewOpenData.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewOpenData_CellContentClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(348, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "产品：";
            // 
            // cmbSymbol
            // 
            this.cmbSymbol.FormattingEnabled = true;
            this.cmbSymbol.Location = new System.Drawing.Point(395, 39);
            this.cmbSymbol.Name = "cmbSymbol";
            this.cmbSymbol.Size = new System.Drawing.Size(121, 20);
            this.cmbSymbol.TabIndex = 6;
            // 
            // colName
            // 
            this.colName.HeaderText = "姓名";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // LoginId
            // 
            this.LoginId.HeaderText = "MT4账户";
            this.LoginId.Name = "LoginId";
            this.LoginId.ReadOnly = true;
            // 
            // StartAmount
            // 
            this.StartAmount.HeaderText = "起始金额";
            this.StartAmount.Name = "StartAmount";
            this.StartAmount.ReadOnly = true;
            // 
            // Ratio
            // 
            this.Ratio.HeaderText = "盈利系数";
            this.Ratio.Name = "Ratio";
            this.Ratio.ReadOnly = true;
            // 
            // 操作
            // 
            this.操作.HeaderText = "操作";
            this.操作.Name = "操作";
            this.操作.ReadOnly = true;
            this.操作.Text = "移除";
            this.操作.ToolTipText = "移除";
            this.操作.UseColumnTextForButtonValue = true;
            // 
            // OpenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 454);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OpenForm";
            this.Load += new System.EventHandler(this.OpenForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOpenData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dataGridViewOpenData;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtVlome;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comCmd;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbSymbol;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn LoginId;
        private System.Windows.Forms.DataGridViewTextBoxColumn StartAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn Ratio;
        private System.Windows.Forms.DataGridViewButtonColumn 操作;
    }
}