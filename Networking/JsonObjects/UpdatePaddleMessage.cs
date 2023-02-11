using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.JsonObjects
{
    public class UpdatePaddleMessage : MessagePacket
    {
        public Vector2 position;
        public int tickNumber;
        //public float yVelocity;
        public string direction {  get; set; }
        public UpdatePaddleMessage() 
        {
            msgType = "UPDATE_POS";        
        }
    }
}
