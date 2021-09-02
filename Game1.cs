using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using DiscordRPC;
using System.Linq;
using System.IO;
using System.Timers;
using System;
using System.Net;
using System.IO.Compression;
using Auth0.OidcClient;
using RestSharp;
using System.Security.Claims;

namespace MonoGame_Test
{
    public class Game1 : Game
    {
        //Read config
        public string[] config = File.ReadAllLines("config");

        //Specify client
        public DiscordRpcClient rpc;

        //Sprites from MGCB, will change some of these to read from skin
        FrameCounter _frameCounter = new FrameCounter();
        Texture2D menu;
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        SpriteFont _defaultfont;

        //Make update tick happen on fixed time not every frame
        public static Timer fixedupdate;

        //Default setting for fpscounter
        public bool fpscounter = false;

        //Skin Items
        private Texture2D cursorsprite;
        private Texture2D hitsprite;

        public Game1()
        {
            FileCheck();

            //Skin extract
            if (!Directory.Exists("userdata/skins/active/" + config[17]))
            {
                ZipFile.ExtractToDirectory("userdata/skins/" + config[17] + ".zip", "userdata/skins/active");
            }
            //Discord RPC
            string RPC_Token = "815220394201841685";
            rpc = new DiscordRpcClient(RPC_Token);


            if (config[15] == "true")
            {
                rpc.Initialize();
            }

            rpc.SetPresence(new RichPresence()
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
            IsMouseVisible = false;
            Window.Title = "Tappu: Made by Juan";
            Window.AllowAltF4 = false;

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

        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            rpc.Dispose();
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
                myWebClient.DownloadFile("https://github.com/juaneth/Tappu-MonoGame/raw/master/skins/default.zip", "userdata/skins/default.zip");
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            //Static textures
            menu = Content.Load<Texture2D>("menu");
            _defaultfont = Content.Load<SpriteFont>("font");

            //Skinned textures

            //Cursor
            FileStream cursorload = new FileStream("userdata/skins/active/default/textures/cursor.png", FileMode.Open);
            cursorsprite = Texture2D.FromStream(GraphicsDevice, cursorload);
            cursorload.Dispose();

            //Hit
            FileStream hitload = new FileStream("userdata/skins/active/default/textures/hit.png", FileMode.Open);
            hitsprite = Texture2D.FromStream(GraphicsDevice, hitload);
            hitload.Dispose();
        }

        protected async override void Initialize()
        {
            Auth0Client client;

            Auth0ClientOptions clientOptions = new Auth0ClientOptions
            {
                Domain = "tappu.eu.auth0.com",
                ClientId = "syGa7Bhq7oQu1VVFOedYzClqm5nQZr0e",
                RedirectUri = "https://tappu.eu.auth0.com/mobile",
                Browser = new Tappu.WebViewBrowserChromium()
            };
            client = new Auth0Client(clientOptions);
            clientOptions.PostLogoutRedirectUri = clientOptions.RedirectUri;

            base.Initialize();

            var loginResult = await client.LoginAsync();
            var result = loginResult.User.Identity.IsAuthenticated + "\n" + loginResult.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            File.WriteAllText("userdata/tmpuser", result);

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
                System.Windows.Forms.MessageBox.Show("PFP Changing");
                changepfp("sss");
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
            
            //Update every frame stuff
            base.Draw(gameTime);
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);

            //Round FPS counter average and set to variable
            int rounded = Convert.ToInt32(_frameCounter.AverageFramesPerSecond);
            var fps = string.Format("FPS: {0}", rounded);

            //Begin Draw
            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied);

            //Draw background
            _spriteBatch.Draw(menu, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            //If fps counter is on, draw it
            if (fpscounter == true)
            {
                _spriteBatch.DrawString(_defaultfont, fps, new Vector2(34, 34), Color.Black);
            }

            //Test hit
            _spriteBatch.Draw(hitsprite, new Vector2(600 + 6, 600 + 6), Color.Black * 0.6f);
            _spriteBatch.Draw(hitsprite, new Vector2(600, 600), Color.White);

            //Get cursor position
            MouseState currentMouseState = Mouse.GetState();
            Vector2 pos = new Vector2(currentMouseState.X - 45, currentMouseState.Y - 45);

            //Draw cursor at cursor position
            _spriteBatch.Draw(cursorsprite, pos, Color.White);

            //End Draw
            _spriteBatch.End();
        }

        private static void changepfp(string userid)
        {
            var client = new RestClient("https://tappu.eu.auth0.com/api/v2/users/" + userid);
            var request = new RestRequest(Method.PATCH);
            request.AddHeader("authorization", "Bearer ABCD");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"user_metadata\": {\"picture\": \"file://C:/Users/euanw/Downloads/smeg.png\"}}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
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

        public const int MAXIMUM_SAMPLES = 10;

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
