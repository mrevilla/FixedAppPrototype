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
using System.Net.Http;
using System.Net.Http.Headers;
using Android.Provider;

namespace Login
{
    [Activity(Label = "FriendProfile")]
    public class FriendProfile : Activity
    {
        private static dynamic friend;
        private TextView tvFPName;
        private string AccessToken;
        private TextView tvFPError; 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.FriendProfile);
            // Link the views
            tvFPName = (TextView)FindViewById(Resource.Id.tvFPName);
            TextView tvFPEmail = (TextView)FindViewById(Resource.Id.tvFPEmail);
            TextView tvFPPhone = (TextView)FindViewById(Resource.Id.tvFPPhone);
            Button btnFPEmail = (Button)FindViewById(Resource.Id.btnFPEmail);
            Button btnFPRemove = (Button)FindViewById(Resource.Id.btnFPRemove);
            Button btnSMS = (Button)FindViewById(Resource.Id.btnSMS);
            tvFPError = (TextView) FindViewById(Resource.Id.tvFPError);

            // Get the serialization
            string serializedFriend = Intent.GetStringExtra("friend");
            friend = JsonConvert.DeserializeObject(serializedFriend);

            AccessToken = Intent.GetStringExtra("token");


            tvFPName.Text = friend.FirstName + ' ' + friend.LastName;
            tvFPEmail.Text = friend.userName;
            tvFPPhone.Text = friend.PhoneNumber;

            if (friend.PhoneNumber != null)
            {
                btnSMS.Visibility = ViewStates.Visible;
            }

            btnFPEmail.Click += BtnFPEmail_Click;
            btnFPRemove.Click += BtnFPRemove_Click;
            btnSMS.Click += BtnSMS_Click;
        }

        private void BtnSMS_Click(object sender, EventArgs e)
        {
            //action should now go to sending sms
            var smsUri = Android.Net.Uri.Parse("smsto:" + friend.PhoneNumber);
            var smsIntent = new Intent(Intent.ActionSendto, smsUri);
            smsIntent.PutExtra("sms_body", "");
            StartActivity(smsIntent);
        }

        private async void BtnFPRemove_Click(object sender, EventArgs e)
        {
            /*Working on getting the delete request to work*/

            try
            {
                string url = GetString(Resource.String.IP) + "api/friends/" + friend.newFriendId;
                string response = await MakePostRequest(url, true);
                Finish();
            }
            catch (Exception exception)
            {
                tvFPError.Text = "ERROR"; 
            }
        }

        private void BtnFPEmail_Click(object sender, EventArgs e)
        {

            var email = new Intent(Android.Content.Intent.ActionSend);

            email.PutExtra(Android.Content.Intent.ExtraEmail,
                new string[] { friend.userName });

            email.PutExtra(Android.Content.Intent.ExtraSubject, "PartyUp - ");

            email.SetType("message/rfc822");

            StartActivity(email);

           
        }

        public async Task<string> MakePostRequest(string url, bool isJson)
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