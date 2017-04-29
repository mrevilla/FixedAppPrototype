using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace Login
{
    [Activity(Label = "PendingProfile")]
    public class PendingProfile : Activity
    {
        private static dynamic friend;
        private TextView tvPPName;
        private string AccessToken;
        private TextView tvPPError;
        private TextView tvPPEmail;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PendingProfile);

            //set views
            tvPPError = (TextView)FindViewById(Resource.Id.tvPPError);
            tvPPName = (TextView)FindViewById(Resource.Id.tvPPName);
            tvPPEmail = (TextView)FindViewById(Resource.Id.tvPPEmail);

            //get the access token and person
            AccessToken = Intent.GetStringExtra("token");
            string serializedFriend = Intent.GetStringExtra("friend");
            friend = JsonConvert.DeserializeObject(serializedFriend);

            tvPPName.Text = friend.FirstName + ' ' + friend.LastName;
            tvPPEmail.Text = friend.userName;

            Button btnConfirm = (Button)FindViewById(Resource.Id.btnConfirm);
            btnConfirm.Click += BtnConfirm_Click;

            Button btnDeny = (Button)FindViewById(Resource.Id.btnDeny);
            btnDeny.Click += BtnDeny_Click;
        }

        private async void BtnDeny_Click(object sender, EventArgs e)
        {
            string url = GetString(Resource.String.IP) + "api/friends/" + friend.newFriendId;
            string response = await MakeDeleteRequest(url, true);
            if (response != null)
            {
                Toast.MakeText(this, "Friendship Denied/Cancelled", ToastLength.Short).Show();
                Finish();
            }
        }

        private async void BtnConfirm_Click(object sender, EventArgs e)
        {
            string url = GetString(Resource.String.IP) + "api/friends";
            string payload = "{" + "newFriendId :" + JsonConvert.SerializeObject(friend.newFriendId) + "}";

            try
            {
                string response = await MakePostRequest(url, payload, true);
                dynamic reply = JsonConvert.DeserializeObject(response);
                if (reply == null)
                {
                    Toast.MakeText(this, "Friendship Confirmed", ToastLength.Short).Show();
                    Finish();

                }
                else
                {
                    Toast.MakeText(this, "Waiting for friend to confirm", ToastLength.Long).Show();

                }
            }
            catch
            {
                tvPPError.Text = "ERROR SENDING CONFIRMATION";
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

        public async Task<string> MakeDeleteRequest(string url, bool isJson)
        {
            //simple request function 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            request.Method = "DELETE";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
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