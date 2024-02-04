using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;

using Weighted_Directed_Graph;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager gfx;
        private SpriteBatch spriteBatch;

        private List<Rectangle> hitBoxes = new List<Rectangle>();

        private SerialPort Serial;       
        private ManagementObjectSearcher searcher;
        private int portsTaken;
        private bool joystickInUse;

        Character Kirby;
        List<Frame> kirbyIdleFrames;
        List<Frame> kirbyRunningFrames;
        List<Frame> kirbyJumpingFrames;
        List<Frame> kirbyDoubleJumpingFrames;
        List<Frame> kirbyCrouchingFrames;
        List<Frame> kirbyCrouchMovingFrames;

        Graph<Point> graph;
        Heap<Vertex<Point>> priorityQueue;

        Rectangle floor;
        Rectangle roof;
        Rectangle leftWall;
        Rectangle rightWall;
        Rectangle platform;

        int timer;


        static int TwoDToOneD(int x, int y, int width) // width = columns
           => x + y * width;
        public void GenerateGraph(int rows, int columns)
        {
            Point[] offsets = new Point[] { };
            offsets = new Point[] { new Point(1, 0), new Point(-1, 0), new Point(0, -1), new Point(0, 1), new Point(1, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1) };

            Dictionary<int, Vertex<Point>> graphValues = new Dictionary<int, Vertex<Point>>();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int num = TwoDToOneD(x, y, columns) + 1;
                    if (graphValues.ContainsKey(num) == false)
                    {
                        var vertex = new Vertex<Point>(new Point(x, y));
                        graph.AddVertex(vertex);
                        graphValues.Add(num, vertex);
                    }

                    for (int i = 0; i < offsets.Length; i++)
                    {
                        int newX = x + offsets[i].X;
                        int newY = y + offsets[i].Y;
                        Point newPoint = new Point(newX, newY);
                        if (newPoint.X < 0 || newPoint.X >= columns || newPoint.Y < 0 || newPoint.Y >= rows)
                        {
                            continue;
                        }

                        int neighborValue = TwoDToOneD(newX, newY, columns) + 1;
                        if (!graphValues.ContainsKey(neighborValue))
                        {
                            var vertex = new Vertex<Point>(new Point(newX, newY));
                            graph.AddVertex(vertex);
                            graphValues.Add(neighborValue, vertex);
                        }

                        float distance = (float)Math.Sqrt(Math.Pow(newX - x, 2) + Math.Pow(newY - y, 2));
                        graph.AddEdge(graphValues[num], graphValues[neighborValue], distance);
                    }
                }
            }
        }


        private void JoystickInput (HashSet<Keys> keys)
        {
            if (Serial != null && Serial.BytesToRead > 0)
            {
                byte[] bytes = new byte[Serial.BytesToRead];
                Serial.Read(bytes, 0, bytes.Length);
                int joyStick = bytes[^1];

                bool r = (joyStick & 1) == 1;
                bool l = (joyStick & 2) == 2;
                bool d = (joyStick & 4) == 4;
                bool u = (joyStick & 8) == 8;

                if (r) { keys.Add(Keys.Right); }
                if (l) { keys.Add(Keys.Left); }
                if (d) { keys.Add(Keys.Down); }
                if (u) { keys.Add(Keys.Up); }
            }
        }

        private bool setJoystick()
        {
            var portnames = SerialPort.GetPortNames();            
            var ports = searcher.Get().Cast<ManagementBaseObject>().Select(p => p["Caption"].ToString()).ToList();

            var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
            portsTaken = portList.Count;

            foreach (string s in portList)
            {
                if (s.Contains("CH340"))
                {
                    string comPort = s.Substring(s.IndexOf('(') + 1, s.IndexOf(')') - s.IndexOf('(') - 1);
                    Serial = new(comPort, 150000);
                    Serial.Open();

                    Console.WriteLine("Controller connected.");
                    return true;
                }
            }
            return false;
        }

        public Game1()
        {
            gfx = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            timer = 0;

            Console.WriteLine("Searching for Controller...");

            searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_PnPEntity WHERE Caption like '%(COM%'");

            Stopwatch sw = new();
            sw.Start();

            if (setJoystick())
            {
                sw.Stop();

                //optimizes speed for when disconnected and searching for reconnection
                searcher.Options = new System.Management.EnumerationOptions()
                {
                    ReturnImmediately = true,
                    EnumerateDeep = false,
                    BlockSize = 1024,
                    DirectRead = true,
                    EnsureLocatable = false,
                    Rewindable = false,
                    UseAmendedQualifiers = false,
                };   

                Console.WriteLine("Controller found. (" + sw.Elapsed + " s)");
                Console.WriteLine("If unplugged, can be plugged back in but cannot use keyboard when unplugged (very slow).");

                joystickInUse = true;

            }
            else
            {
                sw.Stop();

                Console.WriteLine("Controller not found. (" + sw.Elapsed + "s)");
                Console.WriteLine("Cannot be used if plugged in later. (will be fixed later hopefully !)");

                joystickInUse = false;
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

            kirbyJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));

            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(141, 564, 21, 19)));
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

            graph = new Graph<Point>();

            GenerateGraph(GraphicsDevice.Viewport.Height - 32, GraphicsDevice.Viewport.Width - 32);
            priorityQueue = null;

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

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            HashSet<Keys> keysPressed = new HashSet<Keys>(Keyboard.GetState().GetPressedKeys());

            //joystick added on movement if used
            if (joystickInUse)
            {
                try
                {
                    if (Serial != null && Serial.BytesToRead > 0)
                    {
                        JoystickInput(keysPressed);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Serial.Dispose();
                    Serial.Close();
                    Serial = null;
                    Console.WriteLine("Controller disconnected.");
                }

                if (Serial == null && searcher.Get().Count != portsTaken)
                {
                    setJoystick();
                }
            }

            priorityQueue = null;

            Point kirbyPoint = new Point((int)Kirby.Position.X, (int)Kirby.Position.Y);
            Vertex<Point> kirbyVertex = graph.Search(kirbyPoint);

            Point metaKnightPoint = new Point(250, GraphicsDevice.Viewport.Height - 32);
            Vertex<Point> metaKnightVertex = graph.Search(metaKnightPoint);

            List<Vertex<Point>> path = graph.AStar(kirbyVertex, metaKnightVertex, Heuristics.Euclidean, out priorityQueue);

            ;

            //base movement
            Kirby.Update(gameTime, hitBoxes, keysPressed);

            //hitboxes moving
            timer++;
            if (timer == 50)
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

            //var startingPoint = graph.Search(new Point((start.position.X + start.size / 2) / start.size, (start.position.Y + start.size / 2) / start.size));
            //var endingPoint = graph.Search(new Point((end.position.X + end.size / 2) / end.size, (end.position.Y + end.size / 2) / end.size));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            //draw background
            spriteBatch.Draw(Content.Load<Texture2D>("background"), new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            //draw characters
            Kirby.Draw(spriteBatch);

            //draw hiboxes
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