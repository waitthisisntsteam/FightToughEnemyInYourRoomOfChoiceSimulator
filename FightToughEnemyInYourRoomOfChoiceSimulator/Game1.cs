using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

        //joystick added on movement if used (Arduino Joystick + Button)
        private SerialPort Serial;       
        private ManagementObjectSearcher searcher;
        private int portsTaken;
        private bool joystickInUse;

        //kirby setup
        Character Kirby;
        Point kirbyPoint;
        Vertex<Point> kirbyVertex;
        List<Frame> kirbyIdleFrames;
        List<Frame> kirbyRunningFrames;
        List<Frame> kirbyJumpingFrames;
        List<Frame> kirbyDoubleJumpingFrames;
        List<Frame> kirbyCrouchingFrames;
        List<Frame> kirbyCrouchMovingFrames;

        //metaKnight setup
        Character MetaKnight;
        Point metaKnightPoint;
        Vertex<Point> metaKnightVertex;
        Point kirbyPointPrev;
        List<Frame> metaKnightIdleFrames;
        List<Frame> metaKnightRunningFrames;
        List<Frame> metaKnightJumpingFrames;
        List<Frame> metaKnightDoubleJumpingFrames;
        List<Frame> metaKnightCrouchingFrames;
        List<Frame> metaKnightCrouchMovingFrames;

        //graph setup
        Graph<Point> graph;
        Heap<Vertex<Point>> priorityQueue;
        List<Vertex<Point>> path;
        int pathIndex;

        //hitboxes setup
        Rectangle floor;
        Rectangle roof;
        Rectangle leftWall;
        Rectangle rightWall;
        Rectangle platform;

        //timer lol
        int timer;

        //pathfinding grid setup
        static int TwoDToOneD(int x, int y, int width) => x + y * width;    
        public void GenerateGraph(int rows, int columns, List<Point> exclusionPoints)
        {
            Point[] offsets = new Point[] { };
            offsets = new Point[] { new Point(1, 0), new Point(-1, 0), new Point(0, -1), new Point(0, 1), new Point(1, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1) };

            Dictionary<int, Vertex<Point>> graphValues = new Dictionary<int, Vertex<Point>>();
            for (int y = 0; y < rows * 1; y+=1)
            {
                for (int x = 0; x < columns * 1; x+=1)
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
                        int newX = x + offsets[i].X * 1;
                        int newY = y + offsets[i].Y * 1;
                        Point newPoint = new Point(newX, newY);
                        if (newPoint.X < 0 || newPoint.X >= columns * 1 || newPoint.Y < 0 || newPoint.Y >= rows * 1)
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
                        if (exclusionPoints.Contains(new Point(x, y)))
                        {
                            distance += 100;
                        }
                        graph.AddEdge(graphValues[num], graphValues[neighborValue], distance);
                    }                  
                }
            }
        }

        //public List<Vertex<Point>> PathOptimizer(List<Vertex<Point>> originalPath)
        //{
        //    List<Vertex<Point>> newPath = new List<Vertex<Point>>();
        //    List<Point> points = new List<Point>();

        //    foreach(Vertex<Point> v in originalPath)
        //    {
        //        Vertex<Point> newV = new Vertex<Point>(new Point(v.Value.X / 4, v.Value.Y));
        //        Point newP = newV.Value;

        //        if (!points.Contains(newP))
        //        {
        //            points.Add(newP);
        //            newPath.Add(newV);
        //        }
        //    }
        //
        //    return newPath;
        //}

        //joystick added on movement if used
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

            //joystick added on movement if used
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

            kirbyPoint = default;
            kirbyVertex = null;
            kirbyPointPrev = kirbyPoint;

            Kirby = new Character(new Vector2((GraphicsDevice.Viewport.Width - 32)/2 + 200, (GraphicsDevice.Viewport.Height - 32)/2), Content.Load<Texture2D>("kirby"), new List<List<Frame>>() { kirbyJumpingFrames, kirbyDoubleJumpingFrames, kirbyCrouchingFrames, kirbyCrouchMovingFrames, kirbyIdleFrames, kirbyRunningFrames, kirbyJumpingFrames }, 4f, 0.2f, Keys.Up, Keys.Down, Keys.Left, Keys.Right);



            metaKnightIdleFrames = new List<Frame>();
            metaKnightRunningFrames = new List<Frame>();
            metaKnightJumpingFrames = new List<Frame>();
            metaKnightDoubleJumpingFrames = new List<Frame>();
            metaKnightCrouchingFrames = new List<Frame>();
            metaKnightCrouchMovingFrames = new List<Frame>();

            metaKnightIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 435, 41, 25)));
            metaKnightIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(169, 435, 41, 25)));
           
            metaKnightRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(1, 436, 41, 25)));
            metaKnightRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(43, 436, 41, 25)));
          
            metaKnightJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 436, 41, 25)));
          
            metaKnightDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(85, 436, 41, 25)));
            metaKnightDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(85, 436, 41, 25)));
            metaKnightDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 436, 41, 25)));
            metaKnightDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 436, 41, 25)));
            metaKnightDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 436, 41, 25)));
          
            metaKnightCrouchingFrames.Add(new Frame(Vector2.Zero, new Rectangle(211, 436, 41, 25)));
       
            metaKnightCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(211, 436, 41, 25)));

            metaKnightPoint = default;
            metaKnightVertex = null;

            MetaKnight = new Character(new Vector2((GraphicsDevice.Viewport.Width - 32) / 2 - 200, (GraphicsDevice.Viewport.Height - 32) / 2), Content.Load<Texture2D>("kirby"), new List<List<Frame>>() { metaKnightJumpingFrames, metaKnightDoubleJumpingFrames, metaKnightCrouchingFrames, metaKnightCrouchMovingFrames, metaKnightIdleFrames, metaKnightRunningFrames, metaKnightJumpingFrames }, 4.0f, 0.2f, Keys.W, Keys.S, Keys.A, Keys.D);



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

            List<Point> excluded = new List<Point>();
            foreach (var hb in hitBoxes)
            {
                if (hb == platform)
                {
                    for (int x = hb.X - 50; x < hb.X + hb.Width + 50; x++)
                    {
                        for (int y = hb.Y; y < hb.Y + hb.Height + 100; y++)
                        {
                            excluded.Add(new Point(x, y));
                        }
                    }
                }
            }

            


            graph = new Graph<Point>();
            GenerateGraph((GraphicsDevice.Viewport.Height - 32) / 1, (GraphicsDevice.Viewport.Width - 32) / 1, excluded);
            priorityQueue = null;
            path = null;
            pathIndex = 0;
        }

        public bool kirbyIdle = false;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            HashSet<Keys> keysPressed = new HashSet<Keys>(Keyboard.GetState().GetPressedKeys());
            Kirby.currentFrame++;
            Kirby.idle = true;

            MetaKnight.currentFrame++;
            MetaKnight.idle = true;

            //joystick added on movement if used
            /*if (joystickInUse)
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
            }*/

            //kirby movement

            if (path != null && pathIndex < path.Count - 1)
            {
                MetaKnight.idle = false;

                if (path[pathIndex].Value.X - 32 < path[pathIndex + 1].Value.X - 32)
                {
                    MetaKnight.Position.X += 4;

                    MetaKnight.Direction = SpriteEffects.None;
                    MetaKnight.characterState = CharacterState.Running;
                }
                else if (path[pathIndex].Value.X > path[pathIndex + 1].Value.X)
                {
                    MetaKnight.Position.X -= 4;

                    MetaKnight.Direction = SpriteEffects.FlipHorizontally;
                    MetaKnight.characterState = CharacterState.Running;
                }
                else if (path[pathIndex].Value.Y < path[pathIndex + 1].Value.Y && MetaKnight.Position.Y < Kirby.Position.Y)
                {
                    //keysPressed.Add(Keys.S);

                    MetaKnight.Position.Y++;

                    MetaKnight.currentFrame = 0;
                    MetaKnight.characterState = CharacterState.Crouching;
                }
                else if (path[pathIndex].Value.Y > path[pathIndex + 1].Value.Y - 32)
                {
                    keysPressed.Add(Keys.W);
                    MetaKnight.currentFrame = 0;
                    MetaKnight.characterState = CharacterState.Jumping;
                }

                pathIndex++;
            }
            

            Kirby.Update(gameTime, hitBoxes, keysPressed);
            MetaKnight.Update(gameTime, hitBoxes, keysPressed);

            //tick update
            timer++;
            if (timer >= 40)
            {
                timer = 0;

                //pathfinding update
                priorityQueue = null;

                kirbyPoint = new Point((int)Kirby.Position.X / 4, (int)Kirby.Position.Y);
                kirbyVertex = graph.Search(kirbyPoint);

                if (kirbyIdle)
                {
                    metaKnightPoint = new Point((int)MetaKnight.Position.X / 4, (int)MetaKnight.Position.Y);
                    metaKnightVertex = graph.Search(metaKnightPoint);

                    if (path != null)
                    {
                        path.Clear();
                    }

                    path = graph.AStar(metaKnightVertex, kirbyVertex, Heuristics.Euclidean, out priorityQueue);
                    //path = PathOptimizer(path);
                    pathIndex = 0;
                }

                kirbyIdle = false;
                //platforms closing in
                /*
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
                }*/
            }
            if (kirbyPointPrev != kirbyPoint)
            {
                kirbyIdle = true;
            }


            kirbyPointPrev = kirbyPoint;
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
            MetaKnight.Draw(spriteBatch);

            //draw hiboxes
            foreach (var hb in hitBoxes)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("platform"), new Rectangle(hb.X, hb.Y, hb.Width, hb.Height), Color.White);
            }
            //spriteBatch.Draw(Content.Load<Texture2D>("hitbox"), new Rectangle((int)Kirby.Position.X, (int)Kirby.Position.Y, 32, 32), Color.White);
            //spriteBatch.Draw(Content.Load<Texture2D>("hitbox"), new Rectangle(kirbyPoint.X, kirbyPoint.Y, 5, 5), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}