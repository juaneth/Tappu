using System;
using System.Windows.Forms;
using Auth0.OidcClient;

namespace Tappu
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void Login_Load(object sender, EventArgs e)
        {
            this.Visible = false;

            Auth0Client client;

            Auth0ClientOptions clientOptions = new Auth0ClientOptions
            {
                Domain = "tappu.eu.auth0.com",
                ClientId = "syGa7Bhq7oQu1VVFOedYzClqm5nQZr0e",
                RedirectUri = "https://tappu.eu.auth0.com/mobile",
                Browser = new WebViewBrowserChromium()
            };
            client = new Auth0Client(clientOptions);
            clientOptions.PostLogoutRedirectUri = clientOptions.RedirectUri;

            var loginResult = await client.LoginAsync();

            if (loginResult.IsError)
            {
                MessageBox.Show("There has been an error, please try again later \nsmart people code: " + loginResult.ErrorDescription);
            }

            var sss = loginResult.AuthenticationTime;

            bool signed = loginResult.User.Identity.IsAuthenticated;
            if (signed == true)
            {
                MessageBox.Show("Logged in succesfully in " + sss + "!");
            }

            this.Close();
        }
    }
}
