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
    [Activity(Label = "FriendsEvents")]
    public class FriendsEvents : Activity
    {
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
                    foreach (var x in eventData)
                    {

                       friendsEvents.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventId.ToString());
                    }

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
                    foreach (var x in eventData)
                    {

                        friendsEvents.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventId.ToString());
                    }

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
            Intent toEventProfile = new Intent(this, typeof(EventProfile));
            toEventProfile.PutExtra("token", AccessToken);
            toEventProfile.PutExtra("eventId", friendsEvents[friendsEvents.Keys[e.Position]]);
            toEventProfile.PutExtra("userName", userName);
            StartActivity(toEventProfile);
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

    }
}