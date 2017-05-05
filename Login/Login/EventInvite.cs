using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Javax.Crypto.Interfaces;
using Newtonsoft.Json;

/***
 * Populates a list of the user’s events and allows the user to invite
 * other users to an event.
 *
 * OnCreate
 * The function that is called after the activity is created.
 *
 * ListInviteEvents_ItemClick
 * Event handler used when the user selects an event and wants to invite
 * other users to the event.
 *
 * MakeGetRequest
 * Sends GET Request to API
*/

namespace Login
{
    [Activity(Label = "EventInvite")]
    public class EventInvite : Activity
    {
        private static string AccessToken;
        private static string userName;
        private TextView tvEventInviteError;
        private ListView listInviteEvents;
        SortedList<string, string>ids = new SortedList<string, string>();
        List<string> names = new List<string>();
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EventInvite);

            //initialize views
            tvEventInviteError = (TextView)FindViewById(Resource.Id.tvEventInviteError);
            listInviteEvents = (ListView)FindViewById(Resource.Id.lvInviteEvents);
            //intialize variables
            string url = GetString(Resource.String.IP) + "api/events";



            //get the access token and username from previous activity
            AccessToken = Intent.GetStringExtra("token");
            userName = Intent.GetStringExtra("userName");

            //get the events associated with the user
            try
            {

                string serializedResponse = await MakeGetRequest(url);
                dynamic jsonData = JsonConvert.DeserializeObject(serializedResponse);

                if (jsonData == null)
                {
                    tvEventInviteError.Text = "No Events Found";
                }


                else
                {
                    foreach (var x in jsonData)
                    {

                        //filter out the events not owned by the user
                        if (x.Username == userName)
                        {
                            //get ids and names of events that match
                            names.Add(x.Name.ToString());
                            ids.Add(x.Name.ToString(), x.EventId.ToString());
                        }
                    }

                    //alphabetize names
                    names.Sort();

                    //route to activity when clicked on
                    ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, names);
                    listInviteEvents.Adapter = adapter;

                    //if event is clicked
                    listInviteEvents.ItemClick += ListInviteEvents_ItemClick;
                }
            }
            catch
            {
                tvEventInviteError.Text = "ERROR";
            }
        }

        private void ListInviteEvents_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //move the selected event to the next activity
            Intent toInvitedFriends = new Intent(this, typeof(Invite_Uninvite));
            //serialize event
            toInvitedFriends.PutExtra("eventId", ids[names[e.Position]]);
            toInvitedFriends.PutExtra("token", AccessToken);
            StartActivity(toInvitedFriends);
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            // Create URL header
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            // Get response from the API
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            // Read data
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
