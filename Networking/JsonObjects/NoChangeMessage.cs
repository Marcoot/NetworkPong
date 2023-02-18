using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.JsonObjects
{
    public class NoChangeMessage : MessagePacket
    {
        public int tickNumber;
        public string direction {  get; set; }

        public NoChangeMessage()
        {

        }
    }
}
