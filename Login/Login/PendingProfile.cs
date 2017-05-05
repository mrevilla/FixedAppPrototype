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

/***
 * This Activity shows the information of a user requesting friendship. The friendship 
 * request can be accepted or rejected at this activity.
 *
 * 
 * OnCreate The function that is called after the Activity is created.
 * 
 * BtnDeny_Click
 * Event handler used when the user wants to deny the friend request.
 *
 * BtnConfirm_Click
 * Event handler used when the wants to approve the friend request.
 * 
 * MakePostRequest
 * Sends POST Request to API
 *
 * MakeDeleteRequest
 * Sends DELETE Request to API
 * 
 */

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
                        
            //Event handlers
            Button btnConfirm = (Button)FindViewById(Resource.Id.btnConfirm);
            btnConfirm.Click += BtnConfirm_Click;

            Button btnDeny = (Button)FindViewById(Resource.Id.btnDeny);
            btnDeny.Click += BtnDeny_Click;
        }

        private async void BtnDeny_Click(object sender, EventArgs e)
        {
   
            //Obtain the database's ip for the friends controller.
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
            //Obtain the database's ip for the friends controller.
            string url = GetString(Resource.String.IP) + "api/friends";
            string payload = "{" + "newFriendId :" + JsonConvert.SerializeObject(friend.newFriendId) + "}";

            try
            {
                //Get data from the url.
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
            //sets http request to be a post.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            //Add the token to the url
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
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

        public async Task<string> MakeDeleteRequest(string url, bool isJson)
        {
            //sets http request to be a delete.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            //Add the token to the url
            request.Method = "DELETE";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            //write data
            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
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
