using Android.App;
using Android.Widget;
using Android.OS;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Android.Content;

/***
 * Activity that brings up the starting page of the app. It allows the user to 
 * login or go to another page to register as a PartyUp user.
 * 
 * OnCreate The function that is called after the Activity is created.
 * 
 * BtnRegister_Click
 * Event handler that is called when the user presses the register button.
 *
 * BtnLogin_Click
 * Event handler that is called when the user wants to login
 * 
 * MakePostRequest
 * Sends POST Request to API
 */


namespace Login
{
    [Activity(Label = "Login", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        EditText etEmail;
        EditText etPassword;
        TextView tvTest;

        private static string AccessToken;
        protected override void OnCreate(Bundle bundle)
        {
            this.Title = "PartyUp Login";
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            etEmail = (EditText)FindViewById(Resource.Id.etEmail);
            etPassword = (EditText)FindViewById(Resource.Id.etPassword);
            tvTest = (TextView)FindViewById(Resource.Id.tvTest);

            //EVENT HANDLERS
            Button btnLogin = (Button)FindViewById(Resource.Id.btnLogin);
            btnLogin.Click += BtnLogin_Click;
            Button btnRegister = (Button)FindViewById(Resource.Id.btnRegister);
            btnRegister.Click += BtnRegister_Click;
        }

        private void BtnRegister_Click(object sender, System.EventArgs e)
        {
            //Starts Register Activity
            Intent toRegister = new Intent(this,typeof(Register));
            StartActivity(toRegister);

        }

        private async void BtnLogin_Click(object sender, System.EventArgs e)
        {
            /*validation check needed here*/

            //Obtain the database's ip for a token.
            string url = GetString(Resource.String.IP) + "token";
            
            //format user's login input.
            string data = "grant_type=password&username=" + etEmail.Text + "&password=" + etPassword.Text;
            
            try
            {
                //get response from the API using user's credentials
                string response = await MakePostRequest(url, data, false);

                dynamic jsonData = JsonConvert.DeserializeObject(response);

                AccessToken = jsonData.access_token;
 
                //start UserInfo activity
                Intent activityUserInfo = new Intent(this, typeof(UserInfo));
                activityUserInfo.PutExtra("token", AccessToken);
                StartActivity(activityUserInfo);
            }
            catch (System.Exception)
            {
                tvTest.Text = "CONNECTION ERROR";   
            }

        }

        public async Task<string> MakePostRequest(string url, string serializedDataString, bool isJson)
        {
            //sets http request to be a post. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            //Add the token to the url
            request.Method = "POST";
            var stream = await request.GetRequestStreamAsync();
            
            //write data
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(serializedDataString);
                writer.Flush();
                writer.Dispose();
            }

            //get data from API reply
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();

            using (StreamReader sr = new StreamReader(respStream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}

