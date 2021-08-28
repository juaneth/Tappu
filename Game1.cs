using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using DiscordRPC;
using System.Linq;
using System.IO;
using System.Timers;
using System;
using DiscordRPC.Logging;
using System.Net;

namespace MonoGame_Test
{
    public class Game1 : Game
    {
        //Specify client
        public DiscordRpcClient client;

        //Sprites from MGCB, will change some of these to read from skin
        public FrameCounter _frameCounter = new FrameCounter();
        public Texture2D menu;
        public Texture2D loading;
        public Texture2D hitred;
        public Texture2D hitblue;
        public Texture2D cursor;
        public GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;
        public SpriteFont _spriteFont;

        //Make update tick happen on fixed time not every frame
        public static Timer fixedupdate;

        //Default setting for fpscounter
        public bool fpscounter = false;

        //Read config
        public string[] config = File.ReadAllLines("config");

        public Game1()
        {
            FileCheck();

            //Discord RPC
            string RPC_Token = "815220394201841685";
            client = new DiscordRpcClient(RPC_Token);

            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };

            if (config[15] == "true")
            {
                client.Initialize();
            }

            client.SetPresence(new RichPresence()
            {
                Details = "Made by Juan!",
                State = "In Menu",
                Assets = new Assets()
                {
                    LargeImageKey = "tappuicon",
                    LargeImageText = "Tappu: Made by Juan",
                }
            });

            //Window Settings stuff
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "Tappu: Made by Juan";
            Window.AllowAltF4 = false;

            //Mouse Cursor

            //FPS Cap, idk how this works and it doesnt work well a lot but oh well
            if (config[11] == "true")
            {
                _graphics.SynchronizeWithVerticalRetrace = false;
                IsFixedTimeStep = false;
            }
            else
            {
                if (Int32.Parse(config[13]) > 74)
                {
                    System.Windows.Forms.MessageBox.Show("When the FPS Cap is higher or equal to 75 it may become unstable and/or unuseable (max amount is about 270, for some " +
                        "reason the fps when capped doesnt go higher than that, could be because i have a 240HZ monitor but oh well), turn on the FPS Counter in the " +
                        "config to see how many real fps you get or turn on unlimited FPS if you dont mind high GPU usage");
                }

                _graphics.SynchronizeWithVerticalRetrace = true;
                IsFixedTimeStep = true;
                TargetElapsedTime = TimeSpan.FromSeconds(1d / Int32.Parse(config[13]));
            } 

            //Fullscreen Initialization
            //If user wants like 3x fps, they can set windowed and make it hide borders, cba to make this so maybe someone can mod it in (helps when running on low end machines)
            if (config[3] == "true")
            {
                //Get screen resolution
                var resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

                _graphics.IsFullScreen = true;
                //Set screensize
                _graphics.PreferredBackBufferWidth = resolution.Width;
                _graphics.PreferredBackBufferHeight = resolution.Height;
            }
            else
            {
                var width = Convert.ToInt32(config[5]);
                var height = Convert.ToInt32(config[6]);

                _graphics.PreferredBackBufferWidth = width;
                _graphics.PreferredBackBufferHeight = height;
            }
            
            //Apply all changes specified
            _graphics.ApplyChanges();
        }

        private void FileCheck()
        {
            Console.WriteLine("Checking file structure and config");

            if (!File.Exists("config"))
            {
                File.Create("config");
            }

            if (!Directory.Exists("userdata"))
            {
                Directory.CreateDirectory("userdata");
            }

            if (!Directory.Exists("userdata/skins"))
            {
                Directory.CreateDirectory("userdata/skins");
            }

            if (!Directory.Exists("userdata/maps"))
            {
                Directory.CreateDirectory("userdata/maps");
            }

            if (!File.Exists("userdata/skins/default.zip"))
            {
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile("", "userdata/skins/default.zip");
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            menu = Content.Load<Texture2D>("menu");
            loading = Content.Load<Texture2D>("loading");
            hitred = Content.Load<Texture2D>("hit-red");
            hitblue = Content.Load<Texture2D>("hit-blue");
            _spriteFont = Content.Load<SpriteFont>("Arial");

            FileStream fileStream = new FileStream("userdata/skins/" + config[17], FileMode.Open);
            Texture2D spriteAtlas = Texture2D.FromStream(GraphicsDevice, fileStream);
            fileStream.Dispose();
        }

        protected override void Initialize()
        {
            //Add UI Components, may port to winforms for beta
            this.Components.Add(new UI(this));

            base.Initialize();

            //Set update fixed timer
            fixedupdate = new Timer(); fixedupdate.Interval = 4.16666666667 /* 240HZ in ms */; fixedupdate.Elapsed += UpdateFixed; fixedupdate.Enabled = true;

            if (config[1] == "true")
            {
                fpscounter = true;
            }
            else
            {
                fpscounter = false;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        //Update but fixed to 240hz, not frame rate so when FPS cap isnt on animations dont break
        private static void UpdateFixed(object source, ElapsedEventArgs e)
        {
            //Keyboard Checks
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                System.Windows.Forms.Application.Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                
            }

            if (Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    System.Windows.Forms.MessageBox.Show("Restart Needed for Fullscreen Toggle, go into config and restart to change this");
                }
            }

            //Sample for animations I wanna make later

            /*foreach (var i in Enumerable.Range(0, 14))
            {
                
            }*/
        }

        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here

            base.Draw(gameTime);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _frameCounter.Update(deltaTime);

            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);

            _spriteBatch.Begin();

            _spriteBatch.Draw(menu, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            if (fpscounter == true)
            {
                _spriteBatch.DrawString(_spriteFont, fps, new Vector2(1, 1), Color.White);
            }

            _spriteBatch.End();
        }

        protected void OpenSongSelector()
        {
            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            _spriteBatch.Draw(menu, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            _spriteBatch.End();
        }
    }

    public class FrameCounter
    {
        public FrameCounter()
        {
        }

        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }

        public const int MAXIMUM_SAMPLES = 100;

        private Queue<float> _sampleBuffer = new Queue<float>();

        public bool Update(float deltaTime)
        {
            CurrentFramesPerSecond = 1.0f / deltaTime;

            _sampleBuffer.Enqueue(CurrentFramesPerSecond);

            if (_sampleBuffer.Count > MAXIMUM_SAMPLES)
            {
                _sampleBuffer.Dequeue();
                AverageFramesPerSecond = _sampleBuffer.Average(i => i);
            }
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }

            TotalFrames++;
            TotalSeconds += deltaTime;
            return true;
        }
    }
}
