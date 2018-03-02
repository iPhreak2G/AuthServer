using System;
using System.Security;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class frmMain : Office2007Form
    {
        public TcpListener listener_handler;
        public Thread listener_thread;
        public bool listener_running = true;
        public static bool dorefresh;
        public void log(string text, bool timestamp = false)
        {

            richTextBoxEx1.Text = string.Concat(richTextBoxEx1.Text, (timestamp ? string.Concat("[", DateTime.Now, "] ") : ""), text);
        }
        public void RefreshUsers()
        {
            try
            {
                ClientHelper.Client[] clients = ClientHelper.GetClients();
                listViewEx1.Items.Clear();
                for (int i = 0; i < (int)clients.Length; i++)
                {
                    ListViewItem listViewItem = new ListViewItem(i.ToString());
                    listViewItem.SubItems.Add(clients[i].Username);
                    listViewItem.SubItems.Add(clients[i].CPUKey);
                    listViewItem.SubItems.Add(clients[i].Expiration);
                    listViewItem.SubItems.Add(clients[i].LastIP);
                    listViewEx1.Items.Add(listViewItem);
                }
                log("Refreshed All clients\n", true);
            }
            catch
            {
                log("Unable to refresh clients\n", true);
            }
        }

        static frmMain()
        {
            frmMain.dorefresh = false;
        }

        public frmMain()
        {
            InitializeComponent();
            comboBoxEx1.SelectedIndex = 0;
            Control.CheckForIllegalCrossThreadCalls = false;
            contextMenuStrip1.Opening += new CancelEventHandler(contextMenuStrip1_Opening);
            log("Launched Server Listener\n", true);
            RefreshUsers();
            listener_handler = new TcpListener(IPAddress.Any, 9825);
            listener_thread = new Thread(new ThreadStart(this.listener));
            listener_thread.Start();
            log("Server Listening On Port 9825\n", true);
        }

        public T b2s<T>(byte[] bytes)
        where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            } finally {
                handle.Free();
            }
        }
        private byte[] s2b<T>(T str)
        where T : struct
        {
            int num = Marshal.SizeOf(str);
            byte[] numArray = new byte[num];
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            Marshal.StructureToPtr(str, intPtr, true);
            Marshal.Copy(intPtr, numArray, 0, num);
            Marshal.FreeHGlobal(intPtr);
            return numArray;
        }

        private void listener()
        {
            frmMain.authentication_response authenticationResponse = new frmMain.authentication_response();
            string[] strArrays;
            byte[] numArray;
            TcpClient tcpClient = new TcpClient();
            this.listener_handler.Start();
            while (true)
            {
                try
                {
                    if (this.listener_running)
                    {
                        if (this.listener_handler.Pending())
                        {
                            tcpClient = this.listener_handler.AcceptTcpClient();
                            tcpClient.ReceiveTimeout = 7000;
                        }
                        if (tcpClient.Connected)
                        {
                            log("client connected!\n", true);
                            string str = tcpClient.Client.RemoteEndPoint.ToString().Split(":".ToCharArray())[0];
                            byte[] numArray1 = new byte[49];
                            if (tcpClient.GetStream().Read(numArray1, 0, 49) > 0)
                            {
                                authentication_request authenticationRequest = b2s<authentication_request>(numArray1);
                                string str1 = "";
                                byte[] cPUKey = authenticationRequest.CPUKey;
                                for (int i = 0; i < (int)cPUKey.Length; i++)
                                {
                                    byte num = cPUKey[i];
                                    str1 = string.Concat(str1, num.ToString("X2"));
                                }
                                ClientHelper.KeyResponse keyResponse = ClientHelper.CheckCPUKey(str1);
                                if (keyResponse == ClientHelper.KeyResponse.Banned) {
                                    strArrays = new string[] { "Banned user (", str1, ", ", str, ") connected, rejecting connection.\n" };
                                    this.log(string.Concat(strArrays), true);
                                    tcpClient.Close();
                                } else if (keyResponse == ClientHelper.KeyResponse.Registered) {
                                    string text = "";
                                    bool flag = false;
                                    int num1 = 0;
                                    ClientHelper.TierType tier = ClientHelper.TierType.Client;
                                    string text1 = "";
                                    int num2 = 0;
                                    while (num2 < listViewEx1.Items.Count) {
                                        if (!(listViewEx1.Items[num2].SubItems[2].Text == str1)) {
                                            num2++;
                                        } else {
                                            text = listViewEx1.Items[num2].SubItems[3].Text;
                                            flag = DateTime.Compare(DateTime.Parse(text), DateTime.Now) == 1;
                                            num1 = int.Parse(listViewEx1.Items[num2].SubItems[0].Text);
                                            tier = ClientHelper.GetTier(num2);
                                            text1 = listViewEx1.Items[num2].SubItems[1].Text;
                                            listViewEx1.Items[num2].SubItems[4].Text = str;
                                            ClientHelper.EditIP(num2, str);
                                            break;
                                        }
                                    }
                                    
                                    if (!(XEXHelper.CheckXEXVersion(authenticationRequest.XEXVersion) ? false : ClientHelper.GetTier(num1) != ClientHelper.TierType.Admin)) {

                                        log(string.Concat(text1, " has connected ", (flag ? "and has time" : "but has no time"), ", sending authentication notice now.\n"), true);
                                        SHA1 sHA1 = SHA1.Create();
                                        numArray = new byte[] { Convert.ToByte(tier) };
                                        sHA1.TransformFinalBlock(numArray, 0, 1);
                                        byte[] hash = sHA1.Hash;
                                        authenticationResponse.Tier = (byte)tier;
                                        byte[] numArray2 = new byte[16];
                                        numArray = numArray2;
                                        authenticationResponse.PacketChecksum = numArray2;
                                        Array.Copy(hash, numArray, 16);
                                        NetworkStream stream = tcpClient.GetStream();
                                        numArray = new byte[] { 15 };
                                        stream.Write(numArray, 0, 1);
                                        if (!flag) {
                                            tcpClient.GetStream().Write(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }, 0, 17);
                                        } else {
                                            tcpClient.GetStream().Write(this.s2b<frmMain.authentication_response>(authenticationResponse), 0, 17);
                                        }
                                        tcpClient.Close();
                                    } else if (!flag) {
                                        log(string.Concat(text1, " has connected with an outdated .xex but doesn't have any time left, sending authentication notice now.\n"), true);
                                        NetworkStream stream1 = tcpClient.GetStream();
                                        numArray = new byte[] { 15 };
                                        stream1.Write(numArray, 0, 1);
                                        tcpClient.GetStream().Write(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }, 0, 17);
                                        tcpClient.Close();
                                    } else {
                                        log(string.Concat(text1, " has connected but has an outdated .xex, sending update now.\n"), true);
                                        NetworkStream networkStream1 = tcpClient.GetStream();
                                        numArray = new byte[] { 63 };
                                        networkStream1.Write(numArray, 0, 1);
                                        byte[] numArray3 = File.ReadAllBytes(XEXHelper.GetXEXName());
                                        byte[] bytes = BitConverter.GetBytes((int)numArray3.Length);
                                        if (BitConverter.IsLittleEndian) {
                                            Array.Reverse(bytes);
                                        }
                                        tcpClient.GetStream().Write(bytes, 0, 4);
                                        tcpClient.GetStream().Write(numArray3, 0, (int)numArray3.Length);
                                        tcpClient.Close();
                                    }
                                    
                                } else if (!XEXHelper.CheckXEXChecksum(authenticationRequest.XEXChecksum)) {
                                    strArrays = new string[] { "Unknown client (", str1, ", ", str, ") has connected with a modified .xex, banning now.\n" };
                                    log(string.Concat(strArrays), true);
                                    ClientHelper.BanCPUKey(str1);
                                    NetworkStream stream2 = tcpClient.GetStream();
                                    numArray = new byte[] { 15 };
                                    stream2.Write(numArray, 0, 1);
                                    tcpClient.GetStream().Write(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }, 0, 17);
                                    tcpClient.Close();
                                } else {
                                    strArrays = new string[] { "Unknown client (", str1, ", ", str, ") has connected", null, null };
                                    strArrays[5] = (XEXHelper.CheckXEXVersion(authenticationRequest.XEXVersion) ? ", " : " with an outdated .xex, ");
                                    strArrays[6] = "sending authentication notice now.\n";
                                    log(string.Concat(strArrays), true);
                                    NetworkStream networkStream2 = tcpClient.GetStream();
                                    numArray = new byte[] { 15 };
                                    networkStream2.Write(numArray, 0, 1);
                                    tcpClient.GetStream().Write(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }, 0, 17);
                                    tcpClient.Close();
                                }
                            }
                        }
                    }
                }
                catch (Exception exmsg)
                {
                    log("Major server crash caught, resuming work.\n", true);
                    log("exception: \n", true);
                    log(exmsg.Message);
                }
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x34, Pack = 1)]
        public unsafe struct authentication_request
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
            [FieldOffset(0)]
            public byte[] CPUKey;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
            [FieldOffset(16)]
            public byte[] XEXChecksum;
            [MarshalAs(UnmanagedType.U1)]
            [FieldOffset(32)]
            public byte XEXVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
            [FieldOffset(36)]
            public byte[] PacketChecksum;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x14, Pack = 1)]
        public unsafe struct authentication_response
        {
            [MarshalAs(UnmanagedType.U1)]
            [FieldOffset(0)]
            public byte Tier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
            [FieldOffset(4)]
            public byte[] PacketChecksum;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBoxEx.Show("Are you sure you want to exit?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
            {
                e.Cancel = true;
            }
            else
            {
                this.listener_thread.Abort();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (this.listViewEx1.SelectedItems.Count != 0)
            {
                this.banUserToolStripMenuItem.Text = (ClientHelper.CheckCPUKey(this.listViewEx1.SelectedItems[0].SubItems[2].Text) == ClientHelper.KeyResponse.Banned ? "Unban Client" : "Ban Client");
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.log("Started listener thread.\n", true);
            this.buttonX2.Enabled = false;
            this.buttonX3.Enabled = true;
            this.listener_running = true;
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            this.log("Stopped listener thread.\n", true);
            this.buttonX2.Enabled = true;
            this.buttonX3.Enabled = false;
            this.listener_running = false;
        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            this.log("Restarted listener thread.\n", true);
            this.listener_running = false;
            this.listener_thread.Abort();
            this.listener_thread = new Thread(new ThreadStart(this.listener));
            this.listener_thread.Start();
            this.listener_running = true;
            this.buttonX2.Enabled = false;
            this.buttonX3.Enabled = true;
        }

        private void fetchClientInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewEx1.SelectedItems.Count != 0)
            {
                ClientHelper.Client clientInformation = ClientHelper.GetClientInformation(this.listViewEx1.SelectedItems[0].Index);
                string str = string.Concat("Username: ", clientInformation.Username, Environment.NewLine);
                str = string.Concat(str, "CPU Key: ", clientInformation.CPUKey, Environment.NewLine);
                str = string.Concat(str, "Expiration: ", clientInformation.Expiration, Environment.NewLine);
                str = string.Concat(str, "Last IP: ", clientInformation.LastIP, Environment.NewLine);
                str = string.Concat(str, "Tier: ", clientInformation.Tier, Environment.NewLine);
                str = string.Concat(str, "Payment Info: ", clientInformation.PaymentInfo);
                if (MessageBoxEx.Show(string.Concat(str, "\n\nDo you want to copy this information to the clipboard?"), "Get Client Information", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Clipboard.SetText(str);
                }
            }
        }

        private void refreshClientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshUsers();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            int i;
            if (!(this.comboBoxEx1.Text == ""))
            {
                int num = 0;
                int selectedIndex = this.comboBoxEx1.SelectedIndex + 1;
                for (i = 0; i < this.listViewEx1.Items.Count; i++)
                {
                    if (this.listViewEx1.Items[i].SubItems[selectedIndex].Text.ToLower().Contains(this.textBoxX1.Text.ToLower()))
                    {
                        num++;
                    }
                }
                if (num != 0)
                {
                    ListViewItem[] item = new ListViewItem[num];
                    int num1 = 0;
                    for (i = 0; i < this.listViewEx1.Items.Count; i++)
                    {
                        if (this.listViewEx1.Items[i].SubItems[selectedIndex].Text.ToLower().Contains(this.textBoxX1.Text.ToLower()))
                        {
                            item[num1] = this.listViewEx1.Items[i];
                            num1++;
                        }
                    }
                    this.listViewEx1.Items.Clear();
                    for (i = 0; i < num; i++)
                    {
                        this.listViewEx1.Items.Add(item[i]);
                    }
                }
                else
                {
                    MessageBoxEx.Show("No results found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else
            {
                MessageBoxEx.Show("You must enter something to search for!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            (new frmUpdate()).ShowDialog();
        }

        private void addUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new frmClient()).ShowDialog();
            if (frmMain.dorefresh)
            {
                RefreshUsers();
                log("Added a new user to the database.\n", true);
            }
        }

        private void editUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewEx1.SelectedItems.Count != 0)
            {
                ClientHelper.Client clientInformation = ClientHelper.GetClientInformation(this.listViewEx1.SelectedItems[0].Index);
                (new frmClient(this.listViewEx1.SelectedItems[0].Index, clientInformation.Username, clientInformation.CPUKey, clientInformation.Expiration, clientInformation.Tier, clientInformation.PaymentInfo)).ShowDialog();
                if (frmMain.dorefresh)
                {
                    this.RefreshUsers();
                }
            }
        }

        
    }
}
