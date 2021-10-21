using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMS_Data_Server
{
    public class Persons : Form
    {
        List<Person> People { get; set; }
        Form1 form1 = null;

        int currentIndex = 0;

        public Persons(Form1 form1)
        {
            this.form1 = form1;
            this.People = new List<Person>();
        }


        public Person findPersonByNumber(string phonenr)
        {
            for (int i = 0; i < this.People.Count; i++)
            {
                if (this.People[i].PhoneNumber == phonenr)
                    return this.People[i];
            }

            return null;
        }

        public string findPathByNumber(string phonenr)
        {
            var files = Directory.GetFiles("Contacts");
            for (int i = 0; i < files.Length; i++)
            {
                var lines = File.ReadAllLines(files[i]);
                if (lines[1].Contains(phonenr))
                    return files[i];
            }

            return null;
        }


        public void LoadPersons()
        {
            var files = Directory.GetFiles("Contacts");
            
            for (int i = 0; i < files.Length; i++)
            {
                var lines = File.ReadAllLines(files[i]);
                Person person = null;

                List<Message> messages = new List<Message>();
                for (int j = 0; j < lines.Length; j++)
                {
                    if (j == 0 || j == 1 || j == 2) continue;

                    var actualLines = lines[j].Split('$');

                    Message message = null;
                    person = new Person(currentIndex, lines[1], lines[0], messages);

                    if (actualLines[1] == "received")
                        message = new Message(person, DateTime.Parse(actualLines[0]), MESSAGE_TYPE.RECEIVED, actualLines[2]);
                    else
                        message = new Message(person, DateTime.Parse(actualLines[0]), MESSAGE_TYPE.SENT, actualLines[2]);

                    messages.Add(message);
                }

                this.People.Add(person);
                this.form1.AddChat(lines[1], lines[0]);

                currentIndex++;
            }
        }

        public void LoadMessages(string phonenr)
        {
            var person = findPersonByNumber(phonenr);

            for (int i = 0; i < person.Messages.Count; i++)
            {
                this.form1.CreateChatMessage(person.Messages[i]);
            }
        }

        public void CreatePerson(string phonenr, string message)
        {
            File.WriteAllText("Contacts/" + phonenr + ".txt", "GEEN NAAM\n" + phonenr + "\n\n" + DateTime.Now + "$received$" + message);

            List<Message> messages = new List<Message>();
            Person person = new Person(currentIndex, phonenr, "GEEN NAAM", messages);
            messages.Add(new Message(person, DateTime.Now, MESSAGE_TYPE.RECEIVED, message));
            person.Messages = messages;

            this.People.Add(person);
            this.form1.AddChat(phonenr, "GEEN NAAM");

            currentIndex++;
        }

        public void AddMessage(string phonenr, string message)
        {
            var person = this.findPersonByNumber(phonenr);

            Message actualMessage = new Message(person, DateTime.Now, MESSAGE_TYPE.RECEIVED, message);

            person.Messages.Add(actualMessage);
            File.AppendAllText("Contacts/" + phonenr + ".txt", "\n" + DateTime.Now + "$received$" + message);



            this.form1.ShowNotification(this.form1.FindButtonByName(phonenr));




            if (this.form1.poppupMsgEnabled)
            {
                //Form4 frm = new Form4();
                //frm.BringToFront();
                //frm.showAlert("New message from " + person.Name);
            }


            this.form1.CreateChatMessage(actualMessage);

            if (this.form1.selectedPerson.PhoneNumber == phonenr)
            {
                for (int i = 0; i < this.form1.Btns.Count; i++)
                {
                    if (this.form1.Btns[i].Name.Contains(phonenr))
                    {
                        this.form1.Btns[i].PerformClick();
                        break;
                    }
                }
            }
        }
        public bool DoesPersonExist(string phonenr)
        {
            var files = Directory.GetFiles("Contacts");
            for (int i = 0; i < files.Length; i++)
            {
                var lines = File.ReadAllLines(files[i]);
                if (lines[1].Contains(phonenr))
                    return true;

            }

            return false;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Persons
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Persons";
            this.Load += new System.EventHandler(this.Persons_Load);
            this.ResumeLayout(false);

        }

        private void Persons_Load(object sender, EventArgs e)
        {

        }
    }
}
