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
    [Activity(Label = "MyEvents")]
    public class MyEvents : Activity
    {
        private static string AccessToken;
        private static string userName;
        private TextView tvMyEventsError;
        SortedList<string, string> myEvents = new SortedList<string, string>();
        private ListView lvMyEvents;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MyEvents);

            //setviews
            tvMyEventsError = (TextView) FindViewById(Resource.Id.tvMyEventsError);
            lvMyEvents = (ListView) FindViewById(Resource.Id.lvMyEvents);
            lvMyEvents.FastScrollEnabled = true;

            //get intents
            AccessToken = Intent.GetStringExtra("token");
            userName = Intent.GetStringExtra("userName");

            //get all the events associated with the person
            string urlEvents = GetString(Resource.String.IP) + "api/events";

            dynamic eventData;

            //try get request
            try
            {
                string eventsResponse = await MakeGetRequest(urlEvents);
                eventData = JsonConvert.DeserializeObject(eventsResponse);

                myEvents.Clear();
                //get all the events that the user is hosting
                foreach (var x in eventData)
                {

                    //filter out the events not owned by the user
                    if (x.Username == userName)
                    {

                        myEvents.Add(x.Name.ToString() + " - " + userName, x.EventId.ToString());
                    }
                }

                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1,
                    myEvents.Keys.ToArray());
                lvMyEvents.Adapter = adapter;

            }
            catch
            {
                tvMyEventsError.Text = "ERROR GETTING EVENTS";
            }

            lvMyEvents.ItemClick += LvMyEvents_ItemClick;
        }

        protected async override void OnResume()
        {
            base.OnResume();

            string urlEvents = GetString(Resource.String.IP) + "api/events";

            dynamic eventData;

            //try get request
            try
            {
           
                string eventsResponse = await MakeGetRequest(urlEvents);
                eventData = JsonConvert.DeserializeObject(eventsResponse);

                myEvents.Clear();

                //get all the events that the user is hosting
                foreach (var x in eventData)
                {

                    //filter out the events not owned by the user
                    if (x.Username == userName)
                    {

                        myEvents.Add(x.Name.ToString() + " - " + userName, x.EventId.ToString());
                    }
                }

                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1,
                    myEvents.Keys.ToArray());
                lvMyEvents.Adapter = adapter;
            }
            catch
            {
                tvMyEventsError.Text = "ERROR ON RESUME";
            }
        }
    

    private void LvMyEvents_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent toEventProfile = new Intent(this, typeof(EventProfile));
            toEventProfile.PutExtra("token", AccessToken);
            toEventProfile.PutExtra("eventId", myEvents[myEvents.Keys[e.Position]]);
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