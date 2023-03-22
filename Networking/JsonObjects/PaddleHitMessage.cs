using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.JsonObjects
{
    public class PaddleHitMessage : MessagePacket
    {
        //ticknumber, the position of the paddle, and position of the ball and direction
        public int tickNumber;
        public Vector2 positionPaddle;
        public Vector2 ballPosition { get; set; }
        public Vector2 ballVelocity { get; set; }

        public PaddleHitMessage()
        {
            //messagetype
            msgType = "PADDLE_HIT";
        }
    }
}
