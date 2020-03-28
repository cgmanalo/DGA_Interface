using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using GenLogic;
using System.Windows.Controls;

namespace Transformer_Dissolved_Gas_Monitoring
{
    public partial class frmMain : Form
    {
        string serverIP = Dns.GetHostEntry("raspberrypi").AddressList[0].ToString();
        //string serverIP = Dns.GetHostEntry("www.google.com").AddressList[0].ToString();
        int port = 1234;//8080;
        TcpClient client;
        bool connectStatus = false;

        public frmMain()
        {
            InitializeComponent();
            TimerMain.Enabled = false;

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient(serverIP, port);
                //client.ConnectAsync(serverIP, port).Wait(TimeSpan.FromSeconds(2));
                //int byteCount = Encoding.ASCII.GetByteCount(txtRequest.Text);
                //MessageBox.Show("success");
                byte[] sendData;
                sendData = Encoding.ASCII.GetBytes(txtRequest.Text);
                NetworkStream stream = client.GetStream();
                stream.Write(sendData, 0, sendData.Length);
                StreamReader sr = new StreamReader(stream);
                sr.BaseStream.ReadTimeout = 3000;
                try
                {
                    string response = sr.ReadLine();
                    string transName = ParseString(response, 1);
                    string gasName = ParseString(response, 2);
                    string gasValue = ParseString(response, 3);
                    if (transName == "T1")
                    {
                        txtReceiveA.Text = gasName + " = " + gasValue;
                    }
                    else if (transName == "T2")
                    {
                        txtReceiveB.Text = gasName + " = " + gasValue;
                    }
                    else
                    {
                        MessageBox.Show("Invalid transformer name.");
                    }
                }
                catch (IOException)
                {
                    string message = "A connection attempt failed because the connected party did not properly respond after a period of time.";
                    MessageBox.Show(message, "Read Timeout Error");
                }
                stream.Close();
                client.Close();
            }
            catch (SocketException)
            {
                MessageBox.Show("No connection could be made because the target machine actively refused it.", "Connection Error");
            }

        }

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    //ProgressBar progBar = new ProgressBar();
        //    //progBar.Orientation = Orientation.Vertical;
        //}

        public string ParseString(string rawString, int fieldNum)
        {
            int startParse;
            int delimeterCount;
            string extractString;

            char[] rawStringArray = rawString.ToArray();
            extractString = "";

            startParse = 0;
            delimeterCount = 1;
            for (int i = startParse; i < rawString.Length; i++)
            {
                if (rawStringArray[i] == '|')
                {
                    delimeterCount += 1;
                    startParse = i + 1;
                }
                if (delimeterCount == fieldNum)
                {
                    for (int j = startParse; j < rawString.Length; j++)
                    {
                        if (rawStringArray[j] == '|' || rawStringArray[j] == '\r' || rawStringArray[j] == '\n')
                        {
                            return extractString;
                        }
                        extractString += rawStringArray[j];
                    }
                    return extractString;
                }
            }
            return "";
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            if (client.Connected)
            {
                byte[] sendData;
                sendData = Encoding.ASCII.GetBytes("T1|H2");
                NetworkStream stream = client.GetStream();
                stream.Write(sendData, 0, sendData.Length);
                StreamReader sr = new StreamReader(stream);
                sr.BaseStream.ReadTimeout = 3000;
                try
                {
                    string response = sr.ReadLine();
                    string transName = ParseString(response, 1);
                    string gasName = ParseString(response, 2);
                    string gasValue = ParseString(response, 3);
                    if (transName == "T1")
                    {
                        txtReceiveA.Text = gasName + " = " + gasValue;
                    }
                    else if (transName == "T2")
                    {
                        txtReceiveB.Text = gasName + " = " + gasValue;
                    }
                    else
                    {
                        MessageBox.Show("Invalid transformer name.");
                    }
                }
                catch (IOException)
                {
                    string message = "A connection attempt failed because the connected party did not properly respond after a period of time.";
                    MessageBox.Show(message, "Read Timeout Error");
                }
            }
            else
            {
                MessageBox.Show("Client not connected.");
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!connectStatus)
                {
                    client = new TcpClient();
                    client.Connect(cmbIPAddress.Text, Convert.ToInt32(cmbPort.Text));
                    //client.BeginConnect(cmbIPAddress.Text, Convert.ToInt32(cmbPort.Text), new AsyncCallback(AcceptCallbak), null);
                    btnConnect.Text = "Disconnect";
                    //btnDisconnect.Enabled = true;
                    TimerMain.Enabled = true;
                    connectStatus = true;
                }
                else
                {
                    client.Close();
                    //btnDisconnect.Enabled = false;
                    btnConnect.Text = "Connect";
                    TimerMain.Enabled = false;
                    connectStatus = false;
                }

            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void AcceptCallbak(IAsyncResult ar)
        {
            client.EndConnect(ar);
            //btnConnect.Text = "Disconnect";
        }

    }
}
