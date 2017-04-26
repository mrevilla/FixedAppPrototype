using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Login.Resources.layout;
using Newtonsoft.Json;

namespace Login
{
    [Activity(Label = "UserInfo")]
    public class UserInfo : Activity
    {
        TextView tvUserInfo;
        private static string AccessToken;
        private static string firstName;
        private static string lastName;
        private static string userName;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            this.Title = "UserInfo";

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.UserInfo);
            tvUserInfo = (TextView)FindViewById(Resource.Id.tvUserInfo);
            AccessToken = Intent.GetStringExtra("token");
            if(AccessToken == null)
            {
                Finish();
            }
            else
            {
                string url = GetString(Resource.String.IP)+ "api/account/userinfo";

                try
                {
                    string response = await MakeGetRequest(url);

                    dynamic jsonData = JsonConvert.DeserializeObject(response);
                    firstName = jsonData.firstName;
                    lastName = jsonData.lastName;
                    userName = jsonData.Email;
                

                    tvUserInfo.Text = "Welcome," + '\n' + firstName + ' ' + lastName;
                }
                catch (Exception e)
                {
                    tvUserInfo.Text = "ERROR"; 
                    Finish();
                    throw;
                }
            }

            Button btnEditProfile = (Button) FindViewById(Resource.Id.btnEditProfile);
            btnEditProfile.Click += BtnEditProfile_Click;

            Button btnLogout = (Button)FindViewById(Resource.Id.btnLogout);
            btnLogout.Click += BtnLogout_Click;

            Button btnFriends = (Button)FindViewById(Resource.Id.btnFriends);
            btnFriends.Click += BtnFriends_Click;

            Button btnMap = (Button) FindViewById(Resource.Id.btnMap);
            btnMap.Click += BtnMap_Click;

            Button btnCreateEvent = (Button) FindViewById(Resource.Id.btnCreateEvent);
            btnCreateEvent.Click += BtnCreateEvent_Click;

            Button btnEventInvite = (Button) FindViewById(Resource.Id.btnEventInvite);
            btnEventInvite.Click += BtnEventInvite_Click;

        }

        private void BtnEventInvite_Click(object sender, EventArgs e)
        {
            Intent toInvite = new Intent(this, typeof(EventInvite));
            toInvite.PutExtra("token", AccessToken);
            toInvite.PutExtra("userName", userName);
            StartActivity(toInvite);
        }

        private void BtnCreateEvent_Click(object sender, EventArgs e)
        {
            Intent toCreateEvent = new Intent(this, typeof(CreateEvent));
            toCreateEvent.PutExtra("token", AccessToken);
            StartActivity(toCreateEvent);
        }

        private void BtnMap_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected async override void OnResume()
        {
            base.OnResume();

            string url = GetString(Resource.String.IP) + "api/account/userinfo";

            try
            {
                string response = await MakeGetRequest(url);

                dynamic jsonData = JsonConvert.DeserializeObject(response);
                firstName = jsonData.firstName;
                lastName = jsonData.lastName;


                tvUserInfo.Text = "Welcome," + '\n' + firstName + ' ' + lastName;
            }
            catch (Exception e)
            {
                tvUserInfo.Text = "ERROR";
                Finish();
                throw;
            }

        }

        private void BtnEditProfile_Click(object sender, EventArgs e)
        {
            Intent toEditProfile = new Intent(this,typeof(EditProfile));
            toEditProfile.PutExtra("AccessToken", AccessToken);
            StartActivity(toEditProfile);
        }

        private void BtnAddFriends_Click(object sender, EventArgs e)
        {
            //Intent activityUserInfo = new Intent(this, typeof(AddFriends));
            //activityUserInfo.PutExtra("token", AccessToken);
            //StartActivity(activityUserInfo);
        }

        private void BtnFriends_Click(object sender, EventArgs e)
        {
            Intent activityUserInfo = new Intent(this, typeof(Friends));
            activityUserInfo.PutExtra("token", AccessToken);
            StartActivity(activityUserInfo);
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            Finish();
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            using (StreamReader sr = new StreamReader(respStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                respStream = null;
                return strContent;
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
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
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