using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Sprite Guy;
        private SpriteEffects direction = SpriteEffects.FlipHorizontally;
        private SpriteEffects e_d = SpriteEffects.FlipHorizontally;
        private bool goUp = false;
        private bool jump = false;
        private int jumpHeight = 0;

        public void Jump()
        {
            if (goUp == true)
            {
                jump = true;
                Guy.Position.Y -= jumpHeight;
                if (jumpHeight < 15)
                {
                    jumpHeight += 1;
                }
                else if (jumpHeight == 15)
                {
                    jumpHeight = 0;
                    goUp = false;
                }
            }
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            Jump();

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                if (goUp == false)
                {
                    goUp = true;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {

            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {

            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {

            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            



            base.Draw(gameTime);
        }
    }
}