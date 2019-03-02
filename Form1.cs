using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SynscanCom2Wifi
{
    public partial class Form1 : Form
    {
        static int MAX_MSG = 128;
        int msg_len = 0;
        char[] msg = new char[MAX_MSG];
        UdpClient udp = null;
        bool end = false;
        bool run = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!end)
            {
                try
                {
                    System.Net.IPEndPoint remote = null;
                    byte[] data = udp.Receive(ref remote);
                    char[] msg = Encoding.ASCII.GetChars(data);
                    String s = new String(msg, 0, msg.Length);
                    serialPort1.Write(data, 0, data.Length);
                    System.Console.WriteLine("<<<" + s);
                }
                catch (Exception /*ex*/)
                {
                    //System.Console.WriteLine(ex.Message);
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!end)
            {
                try
                {
                    char c = (char)serialPort1.ReadChar();
                    msg[msg_len++] = c;
                    if (msg_len == MAX_MSG)
                    {
                        msg_len = 0;
                        continue;
                    }

                    if (c == '\r')
                    {
                        msg[msg_len] = '\0';
                        String s = new String(msg, 0, msg_len);
                        System.Console.WriteLine(">>>" + s);
                        if (udp != null)
                        {
                            byte[] b = Encoding.ASCII.GetBytes(msg);
                            udp.Send(b, b.Length);
                        }
                        msg_len = 0;
                    }
                }
                catch (Exception /*ex*/) {
                    //System.Console.WriteLine(ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!run)
            {
                end = false;
                serialPort1.PortName = com.Text;
                serialPort1.BaudRate = Int32.Parse(bauds.Text);
                serialPort1.Open();
                serialPort1.ReadTimeout = 1000;
                int p = Int32.Parse(port.Text);
                udp = new UdpClient(0);
                udp.Connect(ip.Text, p);
                udp.Client.ReceiveTimeout = 1000;
                button1.Text = "Stop";
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.RunWorkerAsync();
                run = true;
            }
            else
            {
                end = true;
                while (backgroundWorker1.IsBusy || backgroundWorker2.IsBusy)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                }
                serialPort1.Close();
                udp.Close();
                udp = null;
                button1.Text = "Run";
                run = false;
            }
        }

        static void Main(string[] args)
        {
            Form1 f = new Form1();
            f.ShowDialog();
        }
    }
}
