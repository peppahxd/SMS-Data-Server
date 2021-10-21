using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMS_Data_Server
{
    public class Person
    {
        public int ID { get; set; }

        public string PhoneNumber { get; set; }

        public string Name { get; set; }

        public List<Message> Messages { get; set; }

        public Color Color { get; set; }

        public Person(int ID, string PhoneNumber, string Name, List<Message> Messages)
        {
            this.ID = ID;
            this.PhoneNumber = PhoneNumber;
            this.Name = Name;
            this.Messages = Messages;
        }


        public void ChangeName(string name)
        {
            var txt = File.ReadAllText("Contacts/" + this.PhoneNumber + ".txt");
            txt = txt.Replace(this.Name, name);

            File.WriteAllText("Contacts/" + this.PhoneNumber + ".txt", txt);
            this.Name = name;
        }
    }
}
