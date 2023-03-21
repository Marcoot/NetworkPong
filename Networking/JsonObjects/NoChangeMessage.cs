using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.JsonObjects
{
    public class NoChangeMessage : MessagePacket
    {
        //int of ticknumber en the direction it is going
        public int tickNumber;
        public string direction {  get; set; }

        public NoChangeMessage()
        {
            msgType = "NO_CHANGE";
        }
    }
}
