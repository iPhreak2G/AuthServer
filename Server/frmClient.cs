using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using Helpers;
namespace Server
{
    public partial class frmClient : Office2007Form
    {
        public int index = 0;

        public frmClient()
        {
            InitializeComponent();
            this.Text = "Add Client";
            comboBoxEx1.SelectedIndex = 0;
        }

        public frmClient(int _index, string _username, string _cpukey, string _expiration, string _tier, string _paymentinfo)
        {
            this.InitializeComponent();
            this.Text = string.Concat("Edit \"", _username, "\"'s information");
            this.index = _index;
            this.textBoxX1.Text = _username;
            this.textBoxX2.Text = _cpukey;
            this.dateTimeInput1.Value = DateTime.Parse(_expiration);
            this.comboBoxEx1.Text = _tier;
            this.textBoxX3.Text = _paymentinfo;
        }

        private void frmClient_Load(object sender, EventArgs e)
        {

        }

        

        private void buttonX1_Click(object sender, EventArgs e)
        {
            DateTime value;
            if (!(this.Text == "Add Client"))
            {
                ClientHelper.EditUsername(this.index, this.textBoxX1.Text);
                ClientHelper.EditCPUKey(this.index, this.textBoxX2.Text);
                int num = this.index;
                value = this.dateTimeInput1.Value;
                ClientHelper.EditExpiration(num, value.ToString());
                ClientHelper.EditTier(this.index, this.comboBoxEx1.Text);
                ClientHelper.EditPaymentInfo(this.index, this.textBoxX3.Text);
            }
            else
            {
                string text = this.textBoxX1.Text;
                string str = this.textBoxX2.Text;
                value = this.dateTimeInput1.Value;
                ClientHelper.AddClient(text, str, value.ToString(), this.comboBoxEx1.Text, this.textBoxX3.Text);
            }
            frmMain.dorefresh = true;
            base.Close();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            base.Close();
        }
    }
}
