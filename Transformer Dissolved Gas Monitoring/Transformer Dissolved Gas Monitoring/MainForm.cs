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

        }

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
                sendData = Encoding.ASCII.GetBytes("T2A");
                NetworkStream stream = client.GetStream();
                stream.Write(sendData, 0, sendData.Length);
                StreamReader sr = new StreamReader(stream);
                sr.BaseStream.ReadTimeout = 3000;
                try
                {
                    string response = sr.ReadLine();
                    string transName = ParseString(response, 1);
                    if (transName == "T1")
                    {
                        string gasName = ParseString(response, 2);
                        string gasValue = ParseString(response, 3);
                        if (gasName == "H2")
                        {
                            trkH2A.Value = Convert.ToInt32((100/1000.0)*Convert.ToDouble(gasValue));
                            lblH2A.Text = gasValue;
                        }
                        else if(gasName == "CH4")
                        {
                            trkCH4A.Value = Convert.ToInt32((100 / 80.0) * Convert.ToDouble(gasValue));
                            lblCH4A.Text = gasValue;
                        }
                        else if (gasName == "C2H4")
                        {
                            trkC2H4A.Value = Convert.ToInt32((100 / 80.0) * Convert.ToDouble(gasValue));
                            lblC2H4A.Text = gasValue;
                        }
                        else if (gasName == "C2H6")
                        {
                            trkC2H6A.Value = Convert.ToInt32((100 / 35.0) * Convert.ToDouble(gasValue));
                            lblC2H6A.Text = gasValue;
                        }
                    }
                    else if (transName == "T2")
                    {
                        string gasName = ParseString(response, 2);
                        string gasValue = ParseString(response, 3);
                        if (gasName == "H2")
                        {
                            trkH2B.Value = Convert.ToInt32((100 / 1000.0) * Convert.ToDouble(gasValue));

                        }
                        else if (gasName == "CH4")
                        {
                            trkCH4B.Value = Convert.ToInt32((100 / 80.0) * Convert.ToDouble(gasValue));
                        }
                    }
                    else if (transName == "T1A")
                    {
                        trkH2A.Value = Convert.ToInt32((100 / 1000.0) * Convert.ToDouble(ParseString(response, 2)));
                        lblH2A.Text = ParseString(response, 2);
                        trkCH4A.Value = Convert.ToInt32((100 / 80.0) * Convert.ToDouble(ParseString(response, 3)));
                        lblCH4A.Text = ParseString(response, 3);
                        trkC2H4A.Value = Convert.ToInt32((100 / 80.0) * Convert.ToDouble(ParseString(response, 4)));
                        lblC2H4A.Text = ParseString(response, 4);
                        trkC2H6A.Value = Convert.ToInt32((100 / 35.0) * Convert.ToDouble(ParseString(response, 5)));
                        lblC2H6A.Text = ParseString(response, 5);
                        trkCOA.Value = Convert.ToInt32((100 / 1000.0) * Convert.ToDouble(ParseString(response, 6)));
                        lblCOA.Text = ParseString(response, 6);
                        trkCO2A.Value = Convert.ToInt32((100 / 15000.0) * Convert.ToDouble(ParseString(response, 7)));
                        lblCO2A.Text = ParseString(response, 7);
                        trkC2H2A.Value = Convert.ToInt32((100 / 70.0) * Convert.ToDouble(ParseString(response, 8)));
                        lblC2H2A.Text = ParseString(response, 8);
                    }
                    else if (transName == "T2A")
                    {
                        trkH2B.Value = Convert.ToInt32((100 / 1000.0) * Convert.ToDouble(ParseString(response, 2)));
                        lblH2B.Text = ParseString(response, 2);
                        trkCH4B.Value = Convert.ToInt32((100 / 80.0) * Convert.ToDouble(ParseString(response, 3)));
                        lblCH4B.Text = ParseString(response, 3);
                        trkC2H4B.Value = Convert.ToInt32((100 / 80.0) * Convert.ToDouble(ParseString(response, 4)));
                        lblC2H4B.Text = ParseString(response, 4);
                        trkC2H6B.Value = Convert.ToInt32((100 / 35.0) * Convert.ToDouble(ParseString(response, 5)));
                        lblC2H6B.Text = ParseString(response, 5);
                        trkCOB.Value = Convert.ToInt32((100 / 1000.0) * Convert.ToDouble(ParseString(response, 6)));
                        lblCOB.Text = ParseString(response, 6);
                        trkCO2B.Value = Convert.ToInt32((100 / 15000.0) * Convert.ToDouble(ParseString(response, 7)));
                        lblCO2B.Text = ParseString(response, 7);
                        trkC2H2B.Value = Convert.ToInt32((100 / 70.0) * Convert.ToDouble(ParseString(response, 8)));
                        lblC2H2B.Text = ParseString(response, 8);
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
