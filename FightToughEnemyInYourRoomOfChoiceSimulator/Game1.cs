using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager gfx;
        private SpriteBatch spriteBatch;

        private List<Rectangle> hitBoxes = new List<Rectangle>();

        private SerialPort Serial;

        Character Kirby;
        List<Frame> kirbyIdleFrames;
        List<Frame> kirbyRunningFrames;
        List<Frame> kirbyJumpingFrames;
        List<Frame> kirbyDoubleJumpingFrames;
        List<Frame> kirbyCrouchingFrames;
        List<Frame> kirbyCrouchMovingFrames;

        Rectangle floor;
        Rectangle roof;
        Rectangle leftWall;
        Rectangle rightWall;

        Rectangle platform;

        public Game1()
        {
            gfx = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();

                foreach (string s in portList)
                {
                    if (s.Contains("Serial"))
                    {
                        Serial = new(, )
                        Serial.Open();
                        break;
                    }
                }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {  
            spriteBatch = new SpriteBatch(GraphicsDevice);

            kirbyIdleFrames = new List<Frame>();
            kirbyRunningFrames = new List<Frame>();
            kirbyJumpingFrames = new List<Frame>();
            kirbyDoubleJumpingFrames = new List<Frame>();
            kirbyCrouchingFrames = new List<Frame>();
            kirbyCrouchMovingFrames = new List<Frame>();         

            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(149, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(191, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(171, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(191, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(211, 126, 16, 16)));

            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(24, 19, 16, 16)));
            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(45, 19, 20, 16)));
            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(68, 19, 16, 16)));
            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(89, 19, 21, 16)));

            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(141, 564, 21, 19)));
            kirbyJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));

            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(141, 564, 21, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));

            kirbyCrouchingFrames.Add(new Frame(Vector2.Zero, new Rectangle(74, 228, 16, 16)));

            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(100, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(100, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(74, 228, 16, 16)));

          
            Kirby = new Character(new Vector2((GraphicsDevice.Viewport.Width - 32)/2, (GraphicsDevice.Viewport.Height - 32)/2), Content.Load<Texture2D>("kirby"), new List<List<Frame>>() { kirbyJumpingFrames, kirbyDoubleJumpingFrames, kirbyCrouchingFrames, kirbyCrouchMovingFrames, kirbyIdleFrames, kirbyRunningFrames, kirbyJumpingFrames }, 4f, 0.2f);

            floor = new Rectangle(0, GraphicsDevice.Viewport.Height - 40, GraphicsDevice.Viewport.Width + 40, 20);
            roof = new Rectangle(0, 20, GraphicsDevice.Viewport.Width + 40, 20);
            leftWall = new Rectangle(20, -20, 20, GraphicsDevice.Viewport.Height + 40);
            rightWall = new Rectangle(GraphicsDevice.Viewport.Width - 40, -20, 20, GraphicsDevice.Viewport.Height + 40);

            hitBoxes.Add(floor);
            hitBoxes.Add(roof);
            hitBoxes.Add(leftWall);
            hitBoxes.Add(rightWall);

            platform = new Rectangle(GraphicsDevice.Viewport.Width / 2 - 150, GraphicsDevice.Viewport.Height - GraphicsDevice.Viewport.Height / 2 + 30, 300, 20);
            hitBoxes.Add(platform);
        }

        int timer = 0;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            HashSet<Keys> keysPressed = new HashSet<Keys>(Keyboard.GetState().GetPressedKeys());

            if (Serial.BytesToRead > 0)
            {
                byte[] bytes = new byte[Serial.BytesToRead];
                Serial.Read(bytes, 0, bytes.Length);
                int joyStick = bytes[^1];

                bool r = (joyStick & 1) == 1;
                bool l = (joyStick & 2) == 2;
                bool d = (joyStick & 4) == 4;
                bool u = (joyStick & 8) == 8;

                if (r) { keysPressed.Add(Keys.Right); }
            }

            Kirby.Update(gameTime, hitBoxes, keysPressed);

            //hitboxes moving
            timer++;
            if (timer == 25)
            {
                timer = 0;
                for (int i = 0; i < hitBoxes.Count; i++)
                {

                    if (hitBoxes[i] == roof)
                    {
                        hitBoxes[i] = new Rectangle(hitBoxes[i].X, hitBoxes[i].Y+1, hitBoxes[i].Width, hitBoxes[i].Height);
                        roof = hitBoxes[i];

                        if (Kirby.GetHitbox().Intersects(hitBoxes[i]))
                        {
                            Kirby.Position.Y++;
                        }
                    }
                    if (hitBoxes[i] == floor)
                    {
                        hitBoxes[i] = new Rectangle(hitBoxes[i].X, hitBoxes[i].Y-1, hitBoxes[i].Width, hitBoxes[i].Height);
                        floor = hitBoxes[i];

                        if (Kirby.GetHitbox().Intersects(hitBoxes[i]))
                        {
                            Kirby.Position.Y--;
                        }
                    }
                    if (hitBoxes[i] == leftWall)
                    {
                        hitBoxes[i] = new Rectangle(hitBoxes[i].X+1, hitBoxes[i].Y, hitBoxes[i].Width, hitBoxes[i].Height);
                        leftWall = hitBoxes[i];

                        if (Kirby.GetHitbox().Intersects(hitBoxes[i]))
                        {
                            Kirby.Position.X++;
                        }
                    }
                    if (hitBoxes[i] == rightWall)
                    {
                        hitBoxes[i] = new Rectangle(hitBoxes[i].X-1, hitBoxes[i].Y, hitBoxes[i].Width, hitBoxes[i].Height);
                        rightWall = hitBoxes[i];

                        if (Kirby.GetHitbox().Intersects(hitBoxes[i]))
                        {
                            Kirby.Position.X--;
                        }
                    }
                }
            }
          
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            Kirby.Draw(spriteBatch);

            foreach (var hb in hitBoxes)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("platform"), new Rectangle(hb.X, hb.Y, hb.Width, hb.Height), Color.White);
            }
            //spriteBatch.Draw(Content.Load<Texture2D>("hitbox"), new Rectangle((int)Kirby.Position.X, (int)Kirby.Position.Y, 32, 32), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}