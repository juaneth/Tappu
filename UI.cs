using System;
using Microsoft.Xna.Framework;
using MonoGame.UI.Forms;

namespace MonoGame_Test
{
    class UI : ControlManager
    {
        public UI(Game game) : base(game)
        {

        }

        public override void InitializeComponent()
        {

            string[] config = System.IO.File.ReadAllLines("config");
            var ss = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;

            if (config[3] == "true")
            {
                var start = new Button()
                {
                    Location = (new Vector2(ss.Width / 2, ss.Height / 2)),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "Start Game!",
                    Size = new Vector2(200, 100),
                    BackgroundColor = Color.Gray,
                    Enabled = true,
                    IsVisible = true,
                    FontName = "Arial"
                };

                start.Clicked += Start_Clicked;
                Controls.Add(start);
            }

            if (config[3] == "false")
            {
                var widthread = Int32.Parse(config[5]);
                var heightread = Int32.Parse(config[6]);

                var start = new Button()
                {
                    Location = (new Vector2(widthread / 2, heightread / 2)),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "Start Game!",
                    Size = new Vector2(200, 100),
                    BackgroundColor = Color.Gray,
                    Enabled = true,
                    IsVisible = true,
                    FontName = "Arial",
                    
                };

                start.Clicked += Start_Clicked;
                Controls.Add(start);
            }

        }

        public void Start_Clicked(object sender, EventArgs e)
        {
            
        }
    }
}

