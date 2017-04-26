using Android.App;
using Android.Widget;
using Android.OS;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Android.Content;

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

            Button btnLogin = (Button)FindViewById(Resource.Id.btnLogin);
            btnLogin.Click += BtnLogin_Click;

            Button btnRegister = (Button)FindViewById(Resource.Id.btnRegister);
            btnRegister.Click += BtnRegister_Click;
        }

        private void BtnRegister_Click(object sender, System.EventArgs e)
        {
            Intent toRegister = new Intent(this,typeof(Register));
            StartActivity(toRegister);

        }

        private async void BtnLogin_Click(object sender, System.EventArgs e)
        {
            /*validation check needed here*/

            string url = GetString(Resource.String.IP) + "token";

            string data = "grant_type=password&username=" + etEmail.Text + "&password=" + etPassword.Text;

            try
            {
                string response = await MakePostRequest(url, data, false);

                dynamic jsonData = JsonConvert.DeserializeObject(response);

                AccessToken = jsonData.access_token;

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
            //simple request function 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            request.Method = "POST";
            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(serializedDataString);
                writer.Flush();
                writer.Dispose();
            }

            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();

            using (StreamReader sr = new StreamReader(respStream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}

