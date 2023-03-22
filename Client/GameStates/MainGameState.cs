using Client.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Networking.JsonObjects;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;

namespace GameStates
{
    public class MainGameState : GameObjectList
    {
        private int myId;
        int tickCounter = 0;
        int enemyTickCounter;
        private Paddle leftPaddle, rightPaddle, myPaddle, theirPaddle;
        private Arrow marker;
        private Ball ball;
        Keys lastKeyPressed;

        string enemyDirection;

        TextGameObject tickCounterText;
        NoChangeMessage noChangeMessage;
        PaddleHitMessage paddleHit;
        UpdatePaddleMessage updateMessage;
        BaseProject.Game1 main;


        //physics:
        Vector2 yIncr = new Vector2(0, 10);

        //variables used for checking dropped frames..
        Stopwatch stopWatch;  
        readonly double accurateMs = 1000/60.0;
        readonly int MAX_ATTEMPTS = 3;
        //--------------------------------------------
        

        public MainGameState(BaseProject.Game1 mn)
        {
            main = mn;

            leftPaddle = new Paddle(new Vector2(100 - 12, 100));
            rightPaddle = new Paddle(new Vector2(700 - 12, 100));
            marker = new Arrow(new Vector2());
            ball = new Ball(400, 400, 2, -2);

            Add(leftPaddle);
            Add(rightPaddle);
            Add(marker);
            Add(ball);

            tickCounterText = new TextGameObject("Spritefont");
            tickCounterText.Text = "sds";
            tickCounterText.Position = new Vector2(350, 30);
            Add(tickCounterText);

            noChangeMessage = new NoChangeMessage();
            updateMessage = new UpdatePaddleMessage();
            paddleHit = new PaddleHitMessage();

        }

        public void StartGame(int id)
        {
            Debug.WriteLine("My ID is " + id);
            myId = id;
            if (myId == 0)
            {
                myPaddle = leftPaddle;
                theirPaddle = rightPaddle;
            }
            else
            {
                myPaddle = rightPaddle;
                theirPaddle = leftPaddle;
            }
            //Set Marker at the right spot!
            marker.Position = new Vector2(70 + myId * 600, 10);

            //Start a Stopwatch (necesary to check for frame drops)
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        /// <summary>
        /// Tick() is the main method for handling game events, physics, etc.
        /// </summary>
        private void Tick()
        {
            //update tick
            tickCounter++;
            tickCounterText.Text = "frame tick: " + tickCounter;            

            //Ball collisions with Walls       
            if (ball.Position.X <= 0 || ball.Position.X >= 800 - ball.size)
            {
                ball.BounceHorizontal();
            }
            if (ball.Position.Y <= 0 || ball.Position.Y >= 600 - ball.size)
            {
                ball.BounceVertical();
            }

            // a (crude and simple) collisionCheck for Paddles
            if (ball.CollidesWith(leftPaddle) || ball.CollidesWith(rightPaddle))
            {
                ball.BounceHorizontal();

            }
            //sends if ball hits my own paddle
            if (ball.CollidesWith(myPaddle))
            {
                paddleHit.tickNumber = tickCounter;
                paddleHit.positionPaddle = myPaddle.Position;

                paddleHit.ballPosition = ball.Position;
                paddleHit.ballVelocity = ball.Velocity;

                main.SendObject(paddleHit);
            }

            //MovingOppenentPaddle();
            //Update ball (nb: DON'T replace this with MonoGame's Update; messes up the determinism of frames)
            ball.Tick();

        }

        /// <summary>
        /// Use HandleInput for all the code when 'pressing keyboard buttons'
        /// </summary>
        public override void HandleInput(InputHelper inputHelper)
        {
            base.HandleInput(inputHelper);

            //key W
            if (inputHelper.IsKeyDown(Keys.W))
            {
                myPaddle.Position -= yIncr;
                //if still holding key W
                if (lastKeyPressed != Keys.W)
                {
                    updateMessage.position = myPaddle.Position;

                    updateMessage.direction = "Up";
                    //message.yVelocity = yIncr.Y;

                    updateMessage.tickNumber = tickCounter;
                    main.SendObject(updateMessage);

                    noChangeMessage.direction = "Up";
                    lastKeyPressed = Keys.W;
                }
                else
                {
                    noChangeMessage.tickNumber = tickCounter;
                    main.SendObject(noChangeMessage);
                }
            }
            //key S
            else if (inputHelper.IsKeyDown(Keys.S))
            {
                myPaddle.Position += yIncr;

                if (lastKeyPressed != Keys.S)
                {
                    myPaddle.Position += yIncr;
                    //now, send your message:
                    updateMessage.position = myPaddle.Position;

                    updateMessage.direction = "Down";
                    //message.yVelocity = yIncr.Y;
                    updateMessage.tickNumber = tickCounter;
                    main.SendObject(updateMessage);
                }
                else
                {
                    noChangeMessage.tickNumber = tickCounter;
                    main.SendObject(noChangeMessage);
                }
            }
            //No keys
            else
            {
                if (lastKeyPressed != Keys.None)
                {
                    {
                        myPaddle.Velocity = Vector2.Zero;
                        updateMessage.position = myPaddle.Position;

                        updateMessage.direction = "Nothing";

                        updateMessage.tickNumber = tickCounter;
                        main.SendObject(updateMessage);
                        
                    }
                }
                else
                {
                    noChangeMessage.tickNumber = tickCounter;
                    main.SendObject(noChangeMessage);
                }
            }
        }

        /// <summary>
        /// HandleMessage is called when a network-message is received from the server
        /// </summary>
        public void HandleMessage(byte[] receiveBytes)
        {
            string returnData = Encoding.UTF8.GetString(receiveBytes);
            //When Other paddle is moved
            if (returnData.Contains("UPDATE_POS"))
            {
                UpdatePaddleMessage msg = JsonConvert.DeserializeObject<UpdatePaddleMessage>(returnData);
                theirPaddle.Position = msg.position;

                enemyTickCounter = msg.tickNumber;
                enemyDirection = msg.direction;

                //if (msg.tickNumber != tickCounter)
                if (enemyTickCounter < tickCounter) tickDifferenceHandler();
            }
            if (returnData.Contains("NO_CHANGE"))
            {
                NoChangeMessage msg = JsonConvert.DeserializeObject<NoChangeMessage>(returnData);
                enemyTickCounter= msg.tickNumber;
                enemyDirection = msg.direction;

                if (enemyTickCounter < tickCounter) tickDifferenceHandler();
            }
            if (returnData.Contains("PADDLE_HIT"))
            {
                PaddleHitMessage msg = JsonConvert.DeserializeObject<PaddleHitMessage>(returnData);

                ball.x = (int)msg.ballPosition.X;
                ball.y = (int)msg.ballPosition.Y;

                ball.vx = (int)msg.ballVelocity.X;
                ball.vy = (int)msg.ballVelocity.Y;
               if (enemyTickCounter < tickCounter) tickDifferenceHandler();
            }
        }
        //--------------------------------------------------------

        /// <summary>
        /// WARNING! Update() IS NOT used anymore for game physics etc; put all game logic in Tick() instead
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            /* NB:
             * voor het vak Infrastructure is de method "Update" NIET  handig om te gebruiken voor game-updates; 
             * GEBRUIK SVP method "Tick()" ... (Anders is de Game Loop niet deterministisch!)
             *
             * (verklaring: "Update()" in Monogame houdt blijkbaar zich niet netjes aan 60fps. 
             * Bijvoorbeeld: als je het window versleept, krijg je een flinke 'frame drop'.
             * Onderstaande code compenseert dit a.d.h.v. SystemTime, om daarmee verloren ticks in te halen)
             *             
            */

            int desiredTickCount, attempts = 0;
            desiredTickCount = (int)(stopWatch.ElapsedMilliseconds / accurateMs);
            
            // If there is a difference in ticks (desired vs actual): Do a tick MULTIPLE times until it's equal again..                        
            while(desiredTickCount > tickCounter)
            {                
                Tick();

                //note: capped at maximum of [MAX_ATTEMPTS] ticks per update, to be safe.. Any remaining ticks will transfer to next frame.
                attempts++;
                if (attempts >= MAX_ATTEMPTS) break;
            }
            


            base.Update(gameTime);
        }

        public void tickDifferenceHandler()
        {
            int tickDifference = tickCounter - enemyTickCounter;

            //if no movement stop, enemydirection up and down changes yIncrease * tickdifference of their position
            if (enemyDirection == "Stop") return;
            else if (enemyDirection == "Up") theirPaddle.Position -= yIncr * tickDifference;
            else if (enemyDirection == "Down") theirPaddle.Position += yIncr * tickDifference;
        }

        public override void Reset()
        {           
            base.Reset();
        }

        public void OnExiting()
        {
            
        }

    }
}
