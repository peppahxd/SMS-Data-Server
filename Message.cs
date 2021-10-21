using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Data_Server
{
    public enum MESSAGE_TYPE
    {
        SENT,
        RECEIVED
    }

    public class Message
    {
        public DateTime DateTime { get; set; }

        public MESSAGE_TYPE MESSAGE_TYPE { get; set; }
        public string message { get; set; }
        public IntPtr Result { get; internal set; }

        public Person sender { get; set; } 

        public Message(Person sender , DateTime time, MESSAGE_TYPE MESSAGE_TYPE, string message)
        {
            this.sender = sender;
            this.DateTime = time;
            this.MESSAGE_TYPE = MESSAGE_TYPE;
            this.message = message;
        }

    }
}
