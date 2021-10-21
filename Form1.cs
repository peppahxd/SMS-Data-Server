using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WMPLib;

namespace SMS_Data_Server
{
    public partial class Form1 : Form
    {
        /***SETTINGS***/
        public bool poppupMsgEnabled = true;
        /***SETTINGS***/

        Thread server = null;
        TcpListener listener = null;
        TcpClient client = null;
        Persons persons = null;
        public Person selectedPerson = null;
        Form form = null;

        public List<Button> Btns = new List<Button>();
        public List<Button> notifBtns = new List<Button>();

        Random rnd = new Random();

        string IPAddressServer = "0.0.0.0";
        string IPAddressPhone = "";
        int iChatOffset = 60;

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
         (
             int nLeftRect,
             int nTopRect,
             int nRightRect,
             int nBottomRect,
             int nWidthEllipse,
             int nHeightEllipse
         );
        private const int HT_CAPTION = 0x2;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern bool ReleaseCapture();
        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SendMessage(
        IntPtr hwnd,
        int wMsg,
        int wParam,
        int lParam);
        
        class ElipseControl : Component
        {
            [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            public static extern IntPtr CreateRoundRectRgn
                (
                   int nLeftRect,
                   int nTopRect,
                   int nRightRect,
                   int nBottomRect,
                   int nWidthEllipse,
                   int nHeightEllipse
                );
            private Control _cntrl;
            private int _CornerRadius = 30;

            public Control TargetControl
            {
                get { return _cntrl; }
                set
                {
                    _cntrl = value;
                    _cntrl.SizeChanged += (sender, eventArgs) => _cntrl.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _cntrl.Width, _cntrl.Height, _CornerRadius, _CornerRadius));
                }
            }

            public int CornerRadius
            {
                get { return _CornerRadius; }
                set
                {
                    _CornerRadius = value;
                    if (_cntrl != null)
                        _cntrl.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _cntrl.Width, _cntrl.Height, _CornerRadius, _CornerRadius));
                }
            }
        }
        public Form1()
        {
            form = this;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            ElipseControl nn = new ElipseControl();
            nn.TargetControl = panel1;
            nn.CornerRadius = 10;

            if (IPAddressServer == "")
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Current IP: ", "Server IP", "Default", 0, 0);

                IPAddressServer = input;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            server = new Thread(new ThreadStart(Server));
            server.Start();

            if (!Directory.Exists("Contacts"))
                Directory.CreateDirectory("Contacts");

            persons = new Persons(this);
            persons.LoadPersons();
        }


        public void AddChat(string PhoneNumber, string Name)
        {
            Person person = new Person(1, PhoneNumber, Name, null);

            char c = person.Name.FirstOrDefault();

            Button avatarButton = new Button();
            avatarButton.Name = "texbox" + person.PhoneNumber;
            avatarButton.Text = c.ToString();
            avatarButton.Top = iChatOffset;
            avatarButton.Left = 10;
            avatarButton.Width = 40;
            avatarButton.Height = 40;
            avatarButton.ForeColor = Color.White;
            avatarButton.Font = new Font("Unispace", 10, FontStyle.Bold);
            ElipseControl nn = new ElipseControl();
            nn.TargetControl = avatarButton;
            avatarButton.FlatStyle = FlatStyle.Flat;
            avatarButton.BackColor = GenerateRandomColor();
            avatarButton.FlatAppearance.BorderSize = 0;
            nn.CornerRadius = 10;

            AddControl(avatarButton);

            Button nameButton = new Button();
            nameButton.Name = "button" + person.PhoneNumber;
            nameButton.Text = person.Name;
            nameButton.Top = iChatOffset;
            nameButton.Left = 55;
            nameButton.Width = 140;
            nameButton.Height = 40;
            nameButton.ForeColor = Color.White;
            nameButton.BackColor = Color.FromArgb(0, 111, 114, 155);
            nameButton.FlatStyle = FlatStyle.Flat;
            nameButton.TextAlign = ContentAlignment.MiddleLeft;
            nameButton.FlatAppearance.BorderSize = 0;
            nameButton.Font = new Font("Unispace", 10, FontStyle.Regular);
            Btns.Add(nameButton);

            Button notifBtn = new Button();
            notifBtn.Name = "notifBtn" + person.PhoneNumber;
            notifBtn.Top = 15;
            notifBtn.Left = 125;
            notifBtn.Width = 10;
            notifBtn.Height = 10;
            notifBtn.BackColor = Color.Red;
            notifBtn.FlatStyle = FlatStyle.Flat;
            notifBtn.FlatAppearance.BorderSize = 0;
            notifBtn.BackColor = Color.Red;
            nn.TargetControl = notifBtn;
            nn.CornerRadius = 10;
            notifBtn.Visible = false;

            nameButton.Controls.Add(notifBtn);
            notifBtns.Add(notifBtn);

            nameButton.Click += (s, e) =>
            {
                string phone = nameButton.Name.Replace("button", "");
                this.richTextBox1.Text = "";

                selectedPerson = persons.findPersonByNumber(phone);
                persons.LoadMessages(nameButton.Name.Replace("button", ""));

                avatarbox.FlatAppearance.BorderSize = 0;
                nn.TargetControl = avatarbox;
                avatarbox.BackColor = person.Color;
                avatarbox.Text = person.Name;
                nn.CornerRadius = 50;

                label2.Text = "Name: " + selectedPerson.Name;
                label4.Text = "Phone Number: " + selectedPerson.PhoneNumber;

                this.FindButtonByName(phone).Visible = false;


                this.richTextBox1.Invoke((Action)delegate
                {
                    this.richTextBox1.ScrollToCaret();
                });

            };

            nameButton.MouseUp += (s, e) =>
            {
                MouseEventArgs me = (MouseEventArgs)e;
                if (e.Button == MouseButtons.Right)
                {
                    selectedPerson = persons.findPersonByNumber(nameButton.Name.Replace("button", ""));
                    
                    string input = Microsoft.VisualBasic.Interaction.InputBox("Current Name: " + selectedPerson.Name, "Change Name", "Default", 0, 0);
                    selectedPerson.ChangeName(input);

                    nameButton.Text = input;
                }
            };

            AddControl(nameButton);

            iChatOffset += 50;
        }


        public void CreateChatMessage(Message message)
        {
            Font font = new Font("Unispace", 10, FontStyle.Bold);
            this.richTextBox1.Invoke((Action)delegate
            {
                this.richTextBox1.SelectionFont = font;
            });

            if (message.MESSAGE_TYPE == MESSAGE_TYPE.RECEIVED)
            {
                this.richTextBox1.Invoke((Action)delegate
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                    richTextBox1.SelectionColor = Color.FromArgb(74, 77, 112);
                });


                if (!ApplicationIsActivated())
                {
                    WindowsMediaPlayer myplayer = new WindowsMediaPlayer();
                    myplayer.URL = "http://webdev-cremers.be/swiftly.mp3";
                    myplayer.controls.play();
                }
            }

            if (message.MESSAGE_TYPE == MESSAGE_TYPE.SENT)
            {
                this.richTextBox1.Invoke((Action)delegate
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Right;
                    richTextBox1.SelectionColor = Color.FromArgb(142, 147, 212);
                });
            }

            //Console.WriteLine("Current selected: " + selectedPerson.PhoneNumber);
            //Console.WriteLine("message from: " + message.sender.PhoneNumber);

            //dit wordt pas gecalled wanneer ge op de persoon klikt die iets stuurt.

            richTextBox1.AppendText(message.message + Environment.NewLine + Environment.NewLine);

            this.richTextBox1.Invoke((Action)delegate
            {
                this.richTextBox1.ScrollToCaret();
            });
        }

        public void Server()
        {
            if (IPAddressServer != "")
            {
                listener = new TcpListener(IPAddress.Parse(IPAddressServer), 8500);
                listener.Start();

                Console.WriteLine(IPAddressServer);

                while (true)
                {
                    this.client = listener.AcceptTcpClient();
                    Console.WriteLine("CONNECTION ACCEPTED");

                    NetworkStream ns = this.client.GetStream();

                    byte[] buffer = new byte[1024];
                    ns.Read(buffer, 0, buffer.Length);

                    
                    
                    if (buffer.Length > 0)
                    {
                        string messageFromClient = Encoding.ASCII.GetString(buffer);
                        string[] message = messageFromClient.Split('$');
                        if (message.Length >= 2)
                        {
                            if (!persons.DoesPersonExist(message[0]))
                                persons.CreatePerson(message[0], message[1]);

                            else
                                persons.AddMessage(message[0], message[1]);
                        }

                        else if (messageFromClient.Contains("IP="))
                        {
                            string[] _message = messageFromClient.Split('&');
                            string _ip = _message[1].Replace("IP=", "");
                            this.IPAddressPhone = _ip;
                        }
                    }

                    Thread.Sleep(100);
                }
            }
        }

        public void SendMessageToPhone(string message)
        {
            if (this.IPAddressPhone == "")
            {
                MessageBox.Show("You need to connect your phone first to your pc in order to send messages");
                return;
            }

            TcpClient client = new TcpClient();
            client.Connect(this.IPAddressPhone.Trim(), 8600);

            byte[] buff = Encoding.ASCII.GetBytes(message);
            NetworkStream ns = client.GetStream();

            ns.Write(buff, 0, buff.Length);

            client.Close();
        }

        public Button FindButtonByName(string phonenr)
        {
            for (int i = 0; i < notifBtns.Count; i++)
            {
                if (notifBtns[i].Name == "notifBtn" + phonenr)
                    return notifBtns[i];
            }

            return null;
        }

        public void ShowNotification(Button button)
        {
            button.Invoke((Action)delegate
            {
                button.Visible = true;
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        delegate void AddControlCallback(Button button);
        private void AddControl(Button button)
        {
            Control mainPanel = this.Controls["panelChats"];
            if (mainPanel.InvokeRequired)
            {
                AddControlCallback d = new AddControlCallback(AddControl);
                this.Invoke(d, new object[] { button });
            }
            else
            {
                mainPanel.Controls.Add(button);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Person person = new Person(1, "", "", null);
            Message message = new Message(person, DateTime.Now, MESSAGE_TYPE.SENT, this.textBox1.Text);
            CreateChatMessage(message);

            File.AppendAllText("Contacts/" + selectedPerson.PhoneNumber + ".txt", "\n" + DateTime.Now + "$sent$" + message.message);
            textBox1.Clear();

            selectedPerson.Messages.Add(message);

            richTextBox1.ScrollToCaret();

            SendMessageToPhone("MESSAGE$" + selectedPerson.PhoneNumber + "$" + message.message + "$");
        }

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                Rectangle rct = DisplayRectangle;
                if (rct.Contains(e.Location))
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            form.Show();
            Application.DoEvents();
            Thread.Sleep(100);

            Form form2 = new Form2();
            form2.StartPosition = FormStartPosition.Manual;
            form2.Location = form.Location;
            form2.Size = form.Size;
            form2.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            form.Show();
            Application.DoEvents();
            Thread.Sleep(100);

            Form form3 = new Form3();
            form3.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private Color GenerateRandomColor()
        {
            Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Purple, Color.Orange, Color.Magenta, Color.Beige, Color.Yellow, Color.Brown };

            
            return colors[rnd.Next(0, colors.Length)];
        }

        public void Alert(string msg)
        {
            Console.WriteLine("bruhhhh");
            Form4 frm = new Form4();
            frm.showAlert(msg);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.button2.PerformClick();
            }
        }
    }
}
