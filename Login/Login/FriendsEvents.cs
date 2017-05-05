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
* FriendsEvents.cs Activity: . 
*
* 
* OnCreate: Initializes and sets activity view to be displayed.
* 
* OnResume: Overrided function that displays the events associated with user. 
*
* LvFriendEvents_ItemClick: Event handler that opens up the EventProfile Activity.
*
* MakeGetRequest: Receives data from web api through http get request and returns the content received. 
*  
* 
***/
namespace Login
{
    [Activity(Label = "FriendsEvents")]
    public class FriendsEvents : Activity
    {
        // initialize variables 
        private static string AccessToken;
        private static string userName;
        private TextView tvFriendsEventsError;
        SortedList<string, string> friendsEvents = new SortedList<string, string>();
        private ListView lvFriendEvents;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.FriendsEvents);

            //setup views
            tvFriendsEventsError = (TextView) FindViewById(Resource.Id.tvFriendsEventsError);
            lvFriendEvents = (ListView) FindViewById(Resource.Id.lvFriendEvents);
            lvFriendEvents.FastScrollEnabled = true;

            //get intents
            AccessToken = Intent.GetStringExtra("token");
            userName = Intent.GetStringExtra("userName");

            //get all the events associated with the person
            string urlAttendance = GetString(Resource.String.IP) + "api/attendances";
            dynamic eventData;

            try
            {
                string response = await MakeGetRequest(urlAttendance);

                if (response != null)
                {
                    
                    eventData = JsonConvert.DeserializeObject(response);

                    friendsEvents.Clear();

                    // loop through dynamic to find all events 
                    foreach (var x in eventData)
                    {

                       friendsEvents.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventId.ToString());
                    }

                    // display list 
                    ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1,
                        friendsEvents.Keys.ToArray());
                    lvFriendEvents.Adapter = adapter;
                }

                else
                {
                    tvFriendsEventsError.Text = "No events found";
                }

            }
            catch
            {
                tvFriendsEventsError.Text = "ERROR GETTING EVENTS";
            }

            lvFriendEvents.ItemClick += LvFriendEvents_ItemClick;
        }

        protected async override void OnResume()
        {
            base.OnResume();

            //get all the events associated with the person
            string urlAttendance = GetString(Resource.String.IP) + "api/attendances";
            dynamic eventData;

            try
            {
                string response = await MakeGetRequest(urlAttendance);

                if (response != null)
                {

                    eventData = JsonConvert.DeserializeObject(response);
                    friendsEvents.Clear();

                    // loop through dynamic and save all events to list 
                    foreach (var x in eventData)
                    {

                        friendsEvents.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventId.ToString());
                    }

                    // display events in a list 
                    ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1,
                    friendsEvents.Keys.ToArray());
                    lvFriendEvents.Adapter = adapter;
                }

                else
                {
                    tvFriendsEventsError.Text = "No events found";
                }

            }
            catch
            {
                tvFriendsEventsError.Text = "ERROR GETTING EVENTS";
            }

        }

        private void LvFriendEvents_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // initialize intent for new activity 
            Intent toEventProfile = new Intent(this, typeof(EventProfile));

            // pass data to new activities through extras 
            toEventProfile.PutExtra("token", AccessToken);
            toEventProfile.PutExtra("eventId", friendsEvents[friendsEvents.Keys[e.Position]]);
            toEventProfile.PutExtra("userName", userName);
            StartActivity(toEventProfile);
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            //initialize http request and set method to a get request 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            //initialize variables to stream data from web api
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            using (StreamReader sr = new StreamReader(respStream))
            {
                // read in data 
                string strContent = sr.ReadToEnd();
                respStream = null;
                return strContent;
            }
        }

    }
}
