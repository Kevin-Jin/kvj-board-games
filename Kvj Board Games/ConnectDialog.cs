using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using KvjBoardGames.OnlineFunctions;
using System.Net;

namespace KvjBoardGames
{
    public partial class ConnectDialog : Form
    {
        private const ushort DEFAULT_PORT = 10105;
        private NetworkInterface comm;
        internal NetworkInterface Comm { get { return comm; } }

        public ConnectDialog(bool client)
        {
            InitializeComponent();
            txtPort.Text = DEFAULT_PORT.ToString();
            if (client)
            {
                lblHost.Visible = true;
                txtHost.Visible = true;
                btnSubmit.Text = "Join";
            }
            else
            {
                btnSubmit.Text = "Host";
            }
        }

        private void ShowPortError()
        {
            MessageBox.Show(
                this,
                txtPort.Text + " is not a valid port number.\n"
                + "Valid port numbers are integers in the range of 0 to 65535.\n"
                + "If you do not know what port to use,\n"
                + "leave it at the default value of " + DEFAULT_PORT + (txtHost.Visible ? " or contact the host." : "."),
                this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void StartTryConnect(string host, ushort port)
        {
            btnSubmit.Text = "Connecting...";
            btnSubmit.Enabled = false;
            txtHost.Enabled = false;
            txtPort.Enabled = false;
            Client client = new Client();
            client.Connected += new OpponentConnected(ConnectSuccess);
            client.CaughtSocketException += new SocketExceptionCaught(ConnectFailed);
            client.Connect(host, port);
        }

        private delegate void CloseWindowCallback();

        private void ConnectSuccess(NetworkInterface comm, EndPoint remoteAddress)
        {
            this.comm = comm;
            if (this.InvokeRequired)
                this.Invoke(new CloseWindowCallback(Dispose));
            else
                this.Dispose();
        }

        private void ConnectFailed(SocketException e)
        {
            btnSubmit.Text = "Connect";
            btnSubmit.Enabled = true;
            txtHost.Enabled = true;
            txtPort.Enabled = true;
            MessageBox.Show(this, "Connecting failed: " + e.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void StartTryListen(ushort port)
        {
            btnSubmit.Text = "Binding...";
            btnSubmit.Enabled = false;
            txtPort.Enabled = false;
            ClientListener listener = new ClientListener();
            SocketException e = listener.Listen(port);
            if (e == null)
                ListenSuccess(listener);
            else
                ListenFailed(e);
        }

        private void ListenSuccess(NetworkInterface comm)
        {
            this.comm = comm;
            this.Dispose();
        }

        private void ListenFailed(SocketException e)
        {
            btnSubmit.Text = "Host";
            btnSubmit.Enabled = true;
            txtPort.Enabled = true;
            MessageBox.Show(this, "Hosting failed: " + e.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            ushort port;
            if (txtPort.Text.Length == 0)
            {
                MessageBox.Show(this, "Please enter a port number, or leave it at the default value of " + DEFAULT_PORT + ".", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            try
            {
                port = ushort.Parse(txtPort.Text);
            }
            catch (FormatException)
            {
                ShowPortError();
                return;
            }
            catch (OverflowException)
            {
                ShowPortError();
                return;
            }
            if (txtHost.Visible)
            {
                string host = txtHost.Text;
                if (host.Length == 0)
                {
                    MessageBox.Show(this, "Please enter the host's IP address or host name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    StartTryConnect(host, port);
                }
            }
            else
            {
                StartTryListen(port);
            }
        }
    }
}
