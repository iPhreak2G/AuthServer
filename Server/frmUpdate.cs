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
    public partial class frmUpdate : Office2007Form
    {
        public frmUpdate()
        {
            InitializeComponent();
            this.integerInput1.Value = XEXHelper.GetXEXVersion();
            this.textBoxX1.Text = XEXHelper.GetXEXName();
        }

        private void frmUpdate_Load(object sender, EventArgs e)
        {

        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            XEXHelper.UpdateXEX((byte)this.integerInput1.Value, this.textBoxX1.Text);
            base.Close();
        }
    }
}
