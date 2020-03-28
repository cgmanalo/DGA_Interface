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

namespace Transformer_Dissolved_Gas_Monitoring
{
    public partial class frmMain : Form
    {
        string serverIP = Dns.GetHostEntry("raspberrypi").AddressList[0].ToString();
        int port = 1234;
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient(serverIP, port);
                //int byteCount = Encoding.ASCII.GetByteCount(txtRequest.Text);
                byte[] sendData;
                sendData = Encoding.ASCII.GetBytes(txtRequest.Text);
                NetworkStream stream = client.GetStream();
                stream.Write(sendData, 0, sendData.Length);
                StreamReader sr = new StreamReader(stream);
                sr.BaseStream.ReadTimeout = 3000;
                try
                {
                    string response = sr.ReadLine();
                    txtReceive.Text = response;
                    //prgH2.Value = (int) Convert.ToDouble(ParseString(response, 2));
                    //VProgressBar.Value = (int)Convert.ToDouble(ParseString(response, 2));
                }
                catch (IOException)
                {
                    string message = "A connection attempt failed because the connected party did not properly respond after a period of time.";
                    MessageBox.Show(message,"Read Timeout Error");
                }

                stream.Close();
                client.Close();
            }
            catch(SocketException)
            {
                MessageBox.Show("No connection could be made because the target machine actively refused it.","Connection Error");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
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
    }

    public class VerticalProgressBar : ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }

}
