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
 * The activity lists friends that are invited to a specific event. 
 * They can be chosen to be uninvited.
 * 
 * OnCreate The function that is called after the Activity is created.
 * 
 * BtnRevoke_Click
 * Event handler used for uninviting a user from an event.
 * 
 * MakeGetRequest
 * Sends GET request to API
 * 
 * MakePostRequest
 * Sends POST request to API
 * 
 */

namespace Login
{
    [Activity(Label = "UninvitePage")]
    public class UninvitePage : Activity
    {
        private TextView tvUninviteError;
        private static string eventId;
        private static string AccessToken;
        private ListView lvUninvite;
        SortedList<string, Friend> attendingList = new SortedList<string, Friend>();
        List<string>peopleInvited = new List<string>();

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.UninvitePage);

            //get data from intent
            eventId = Intent.GetStringExtra("eventId");
            AccessToken = Intent.GetStringExtra("token");

            //get the views
            tvUninviteError = (TextView)FindViewById(Resource.Id.tvUninviteError);
            lvUninvite = (ListView)FindViewById(Resource.Id.lvUninvite);
            lvUninvite.FastScrollEnabled = true;
            lvUninvite.ChoiceMode = Android.Widget.ChoiceMode.Multiple;

            //set the url
            string urlAttendees = GetString(Resource.String.IP) + "api/attendances/" + eventId;

            //get the list of friends
            try
            {
                //send request
                string response = await MakeGetRequest(urlAttendees);

                //deserialize response
                dynamic jsonAttendees = JsonConvert.DeserializeObject(response);

                //put each person into a referenced list
                foreach (var x in jsonAttendees)
                {
                    //capture details about the person
                    Friend person = new Friend();
                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.userName = x.UserName;
                    person.newFriendId = x.UserId;

                    //add the person to a referencable list
                    attendingList.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);

                    //create a list the users will see
                    peopleInvited.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName);
                }

                //sort the list
                peopleInvited.Sort();


                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItemChecked, peopleInvited);
                lvUninvite.Adapter = adapter;

                Button btnRevoke = (Button) FindViewById(Resource.Id.btnRevoke);
                btnRevoke.Click += BtnRevoke_Click;
            }
            catch
            {
                tvUninviteError.Text = "ERROR";
            }

        }

        private async void BtnRevoke_Click(object sender, EventArgs e)
        {
            var sparseArray = FindViewById<ListView>(Resource.Id.lvUninvite).CheckedItemPositions;
            for (var i = 0; i < sparseArray.Size(); i++)
            {
                //check to see if the person has been selected.
                if (sparseArray.ValueAt(i) == true)
                {
                    //get that persons information
                    Friend receiver = attendingList[peopleInvited[sparseArray.KeyAt(i)]];
                    //create the invitation
                    Attendee uninvite = new Attendee();
                    uninvite.AttendeeId = receiver.newFriendId;
                    uninvite.EventId = eventId;

                    //serialize the inviation
                    string serializedUnvite = JsonConvert.SerializeObject(uninvite);

                    //generate correct url
                    string url = GetString(Resource.String.IP) + "api/Attendances/" + eventId + "/" + receiver.newFriendId;

                    //send invite to the person
                    try
                    {
                        string response = await MakePostRequest(url, serializedUnvite, true);
                    }
                    catch
                    {
                        tvUninviteError.Text = "ERROR";
                    }

                }

                if (tvUninviteError.Text != "ERROR")
                {
                    Toast.MakeText(this, "Selected Invitations Revoked", ToastLength.Short).Show();
                    Finish();
                }
            }
        }
        

        public static async Task<string> MakeGetRequest(string url)
        {
            //set http request to be a get
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
            //set http request to be a POST
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
