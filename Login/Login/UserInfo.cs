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

/***
 * This activity is used after the user has logged in. Several buttons are shown
 * that will take the user to different features/activities of the application.
 * 
 * OnCreate 
 * The function that is called after the Activity is created.
 * 
 * BtnEvents_Click  
 * Event handler used when the user wants to go the events list page.
 * 
 * BtnEventInvite_Click 
 * Event handler used when the user clicks on the event invite button.
 * 
 * BtnCreateEvent_Click 
 * Event handler used when the user click on create events button
 * 
 * BtnMap_Click 
 * Event handler used when the user clicks on the map button. The EventMap activity is
 * started.
 * 
 * BtnLogout_Click
 * Event handler that closes the activity when the user presses the log out button.
 * 
 * OnResume
 * The function that is called after the activity visible to the user again.    
 * 
 * BtnEditProfile_Click 
 * Event handler when the user clicks button to edit profile. Closes activity
 * and goes EditProfile
 * 
 * BtnFriends_Click - 
 * Event handler when the user clicks button to go to friends. Closes the activity
 * and goes to Friends activity.
 * 
 * BtnLogout_Click 
 * Event handler when the user wants to log out. Closes the activity.
 * 
 * MakeGetRequest    
 * Sends GET Request to API
 * 
 * MakePostRequest    
 * Sends HTTP Request to API
 */

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
            //Get user's token
            AccessToken = Intent.GetStringExtra("token");
            //Close activity if the token receieved is null.
            if(AccessToken == null)
            {
                Finish();
            }
            else
            {
                //Obtain the database's ip for the userinfo controller.
                string url = GetString(Resource.String.IP)+ "api/account/userinfo";

                try
                {
                    //Get data from the url. 
                    string response = await MakeGetRequest(url);

                    //Deserialize incoming data.
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
            
            //EVENT HANDLER
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

            Button btnEvents = (Button) FindViewById(Resource.Id.btnEvents);
            btnEvents.Click += BtnEvents_Click;

        }

        private void BtnEvents_Click(object sender, EventArgs e)
        {
            //Start EventList Activity with token and username extras
            Intent toEventList = new Intent(this,typeof(EventList));
            toEventList.PutExtra("token", AccessToken);
            toEventList.PutExtra("userName", userName);
            StartActivity(toEventList);
        }

        private void BtnEventInvite_Click(object sender, EventArgs e)
        {
           //Start EventInvite Activity with token and username extras
            Intent toInvite = new Intent(this, typeof(EventInvite));
            toInvite.PutExtra("token", AccessToken);
            toInvite.PutExtra("userName", userName);
            StartActivity(toInvite);
        }

        private void BtnCreateEvent_Click(object sender, EventArgs e)
        {
             //Start CreateEvent Activity with token extras
            Intent toCreateEvent = new Intent(this, typeof(CreateEvent));
            toCreateEvent.PutExtra("token", AccessToken);
            StartActivity(toCreateEvent);
        }

        private void BtnMap_Click(object sender, EventArgs e)
        {
            //Start EventMap Activity with token and username extras
            Intent toEventMap = new Intent(this, typeof(EventMap));
            toEventMap.PutExtra("token", AccessToken);
            toEventMap.PutExtra("userName", userName);
            StartActivity(toEventMap);
        }

        protected async override void OnResume()
        {
            base.OnResume();

            //Get database url
            string url = GetString(Resource.String.IP) + "api/account/userinfo";

            try
            {
            
                //get response from database using url
                string response = await MakeGetRequest(url);

                //deserialize data obtained.
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
              //Start Friends Activity with token and username extras
            Intent toEditProfile = new Intent(this,typeof(EditProfile));
            toEditProfile.PutExtra("AccessToken", AccessToken);
            StartActivity(toEditProfile);
        }



        private void BtnFriends_Click(object sender, EventArgs e)
        {
         //Start Friends Activity with token and username extras
            Intent activityUserInfo = new Intent(this, typeof(Friends));
            activityUserInfo.PutExtra("token", AccessToken);
            activityUserInfo.PutExtra("userName", userName);
            StartActivity(activityUserInfo);
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            Finish();
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            //create URL header.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

           //get response from the API
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            //read data
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
    }
}
