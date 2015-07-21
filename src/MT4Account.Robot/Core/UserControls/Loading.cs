using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MT4Account.Robot.Core.UserControls
{
    public partial class Loading : UserControl
    {
        private Panel panel;

        public event Action Showed;
        public event Action Hided;
        public Loading()
        {
            InitializeComponent();
        }

        private void InitPanel()
        {
            panel = new Panel();
            panel.Height = 30;
            panel.Width = 200;
            panel.Visible = true;
            panel.BorderStyle = BorderStyle.FixedSingle;


            Label label = new Label();
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Dock = DockStyle.Fill;
            label.Text = "请稍后......";
            panel.Controls.Add(label);
            this.Controls.Add(panel);
            this.BringToFront(); //设置panel空间顶端显示
        }

        private void Loading_Load(object sender, EventArgs e)
        {
            InitPanel();
            this.Visible = false;
        }

        public void ShowLoading()
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                this.Location = new Point((this.Parent.Width / 2) - 100, (this.Parent.Height / 2) - 30);
                this.Visible = true;
                if (Showed != null)
                {
                    Showed();
                }
            }));

        }
        public void HideLoading()
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                this.Visible = false;
                if (Hided != null)
                {
                    Hided();
                }
            }));
        }
    }
}
