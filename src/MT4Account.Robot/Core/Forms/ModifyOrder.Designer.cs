namespace MT4Account.Robot.Core.Forms
{
    partial class ModifyOrder
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
            this.dataGridViewModifyData = new System.Windows.Forms.DataGridView();
            this.btnSave = new System.Windows.Forms.Button();
            this.Login = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OpenTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cmd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Volume = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Symbol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OpenPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CloseTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClosePrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Profit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModifyData)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.dataGridViewModifyData);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnSave);
            this.splitContainer1.Size = new System.Drawing.Size(1045, 528);
            this.splitContainer1.SplitterDistance = 481;
            this.splitContainer1.TabIndex = 0;
            // 
            // dataGridViewModifyData
            // 
            this.dataGridViewModifyData.AllowUserToAddRows = false;
            this.dataGridViewModifyData.AllowUserToDeleteRows = false;
            this.dataGridViewModifyData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewModifyData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewModifyData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Login,
            this.OrderId,
            this.OpenTime,
            this.Cmd,
            this.Volume,
            this.Symbol,
            this.OpenPrice,
            this.CloseTime,
            this.ClosePrice,
            this.Profit});
            this.dataGridViewModifyData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewModifyData.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewModifyData.Name = "dataGridViewModifyData";
            this.dataGridViewModifyData.RowTemplate.Height = 23;
            this.dataGridViewModifyData.Size = new System.Drawing.Size(1045, 481);
            this.dataGridViewModifyData.TabIndex = 0;
            this.dataGridViewModifyData.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewModifyData_CellEndEdit);
            // 
            // btnSave
            // 
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSave.Location = new System.Drawing.Point(970, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 43);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Login
            // 
            this.Login.DataPropertyName = "Login";
            this.Login.HeaderText = "账户";
            this.Login.Name = "Login";
            this.Login.ReadOnly = true;
            // 
            // OrderId
            // 
            this.OrderId.DataPropertyName = "OrderId";
            this.OrderId.HeaderText = "订单号";
            this.OrderId.Name = "OrderId";
            this.OrderId.ReadOnly = true;
            // 
            // OpenTime
            // 
            this.OpenTime.DataPropertyName = "OpenTime";
            this.OpenTime.HeaderText = "开仓时间";
            this.OpenTime.Name = "OpenTime";
            this.OpenTime.ReadOnly = true;
            // 
            // Cmd
            // 
            this.Cmd.DataPropertyName = "Cmd";
            this.Cmd.HeaderText = "类型";
            this.Cmd.Name = "Cmd";
            this.Cmd.ReadOnly = true;
            // 
            // Volume
            // 
            this.Volume.DataPropertyName = "Volume";
            this.Volume.HeaderText = "手数";
            this.Volume.Name = "Volume";
            this.Volume.ReadOnly = true;
            // 
            // Symbol
            // 
            this.Symbol.DataPropertyName = "Symbol";
            this.Symbol.HeaderText = "产品";
            this.Symbol.Name = "Symbol";
            this.Symbol.ReadOnly = true;
            // 
            // OpenPrice
            // 
            this.OpenPrice.DataPropertyName = "OpenPrice";
            this.OpenPrice.HeaderText = "开仓价";
            this.OpenPrice.Name = "OpenPrice";
            this.OpenPrice.ReadOnly = true;
            // 
            // CloseTime
            // 
            this.CloseTime.DataPropertyName = "CloseTime";
            this.CloseTime.HeaderText = "平仓时间";
            this.CloseTime.Name = "CloseTime";
            // 
            // ClosePrice
            // 
            this.ClosePrice.DataPropertyName = "ClosePrice";
            this.ClosePrice.HeaderText = "平仓价";
            this.ClosePrice.Name = "ClosePrice";
            // 
            // Profit
            // 
            this.Profit.DataPropertyName = "Profit";
            this.Profit.HeaderText = "盈利";
            this.Profit.Name = "Profit";
            this.Profit.ReadOnly = true;
            // 
            // ModifyOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 528);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModifyOrder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "修改{0}的交易订单";
            this.Load += new System.EventHandler(this.ModifyOrder_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModifyData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridViewModifyData;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn Login;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderId;
        private System.Windows.Forms.DataGridViewTextBoxColumn OpenTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cmd;
        private System.Windows.Forms.DataGridViewTextBoxColumn Volume;
        private System.Windows.Forms.DataGridViewTextBoxColumn Symbol;
        private System.Windows.Forms.DataGridViewTextBoxColumn OpenPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn CloseTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClosePrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn Profit;
    }
}