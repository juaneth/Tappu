using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace MonoGame_Test
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string[] Prev = File.ReadAllLines("config");
            Thread.Sleep(50);
            if (checkBox1.Checked)
            {
                File.WriteAllText("config", "fpscounter ->\ntrue" + Prev[2] + "\n" + Prev[3]);
            }
            else
            {
                if (!checkBox1.Checked)
                {
                    File.WriteAllText("config", Prev + "fpscounter ->\nfalse");
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string[] Prev = File.ReadAllLines("config");
            if (Prev[0] == "")
            {

            }
        }
    }
}
