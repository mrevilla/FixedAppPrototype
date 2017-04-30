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
    [Activity(Label = "EventList")]
    public class EventList : Activity
    {
        private static string AccessToken;
        private static string userName;
        private TextView tvELError;
        SortedList<string, string> relatedEvents = new SortedList<string, string>();
        private ListView lvAllEvents;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EventList);

            //set views
            tvELError = (TextView) FindViewById(Resource.Id.tvELError);
            lvAllEvents = (ListView) FindViewById(Resource.Id.lvAllEvents);
            lvAllEvents.FastScrollEnabled = true;
            

            //get intent data
            AccessToken = Intent.GetStringExtra("token");
            userName = Intent.GetStringExtra("userName");

            //get all the events associated with the person
            string urlEvents = GetString(Resource.String.IP) + "api/events";
            string urlAttending = GetString(Resource.String.IP) + "api/attendances";

            /////////
            dynamic eventData;
            dynamic attendingData;
            /// ////
            relatedEvents.Clear();
            //try get request
            try
            {
                string eventsResponse = await MakeGetRequest(urlEvents);
                string attendingResponse = await MakeGetRequest(urlAttending);
                eventData = JsonConvert.DeserializeObject(eventsResponse);
                attendingData = JsonConvert.DeserializeObject(attendingResponse);

                //get all the events that the user is hosting
                foreach (var x in eventData)
                {

                    //filter out the events not owned by the user
                    if (x.Username == userName)
                    {

                        relatedEvents.Add(x.Name.ToString() + " - " + userName, x.EventId.ToString());
                    }
                }

                //get all the events the user is invited to
                foreach (var x in attendingData)
                {
                    relatedEvents.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(),
                        x.EventId.ToString());
                }

                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1,
                    relatedEvents.Keys.ToArray());
                lvAllEvents.Adapter = adapter;

            }
            catch
            {
                tvELError.Text = "ERROR GETTING EVENTS";
            }

            lvAllEvents.ItemClick += LvAllEvents_ItemClick;

            //button for my events
            Button btnMyEvents = (Button) FindViewById(Resource.Id.btnMyEvents);
            btnMyEvents.Click += BtnMyEvents_Click;
        }

        private void LvAllEvents_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //get the selected event and pass information to the next activity
            Intent toEventProfile = new Intent(this, typeof(EventProfile));
            toEventProfile.PutExtra("token", AccessToken);
            toEventProfile.PutExtra("eventId", relatedEvents[relatedEvents.Keys[e.Position]]);
            toEventProfile.PutExtra("userName", userName);
            StartActivity(toEventProfile);
        }

        private void BtnMyEvents_Click(object sender, EventArgs e)
        {
            
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