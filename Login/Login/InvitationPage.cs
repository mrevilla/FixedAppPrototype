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
    [Activity(Label = "InvitationPage")]
    public class InvitationPage : Activity
    {

        private static string AccessToken;
        private static string eventId;
        private TextView tvInvitationPageError;
        SortedList<string, Friend> attendingList = new SortedList<string, Friend>();
        SortedList<string, Friend> friendsList = new SortedList<string, Friend>();
        List<string> validInvite = new List<string>();
        private ListView lvInvitationPage;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.InvitationPage);
            tvInvitationPageError = (TextView)FindViewById(Resource.Id.tvInvitationPageError);
            lvInvitationPage = (ListView) FindViewById(Resource.Id.lvInvitationPage);
            lvInvitationPage.FastScrollEnabled = true;
            lvInvitationPage.ChoiceMode = Android.Widget.ChoiceMode.Multiple;

            //get intent data
            AccessToken = Intent.GetStringExtra("token");
            eventId = Intent.GetStringExtra("eventId");

            //setup urls for friends and attendance requests
            string urlAttendees = GetString(Resource.String.IP) + "api/attendances/" + eventId;
            string urlFriends = GetString(Resource.String.IP) + "api/friends";

            try
            {
                string response = await MakeGetRequest(urlAttendees);
                dynamic jsonAttendees = JsonConvert.DeserializeObject(response);

                foreach (var x in jsonAttendees)
                {
      
                    Friend person = new Friend();
                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.userName = x.UserName;
                    person.newFriendId = x.UserId;

                    attendingList.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);

                }

                string friendResponse = await MakeGetRequest(urlFriends);
                dynamic jsonFriends = JsonConvert.DeserializeObject(friendResponse);

                foreach (var x in jsonFriends)
                {
                    Friend person = new Friend();
                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.PhoneNumber = x.PhoneNumber;
                    person.userName = x.userName;
                    person.friendshipEstablished = x.friendshipEstablished;
                    person.newFriendId = x.newFriendId;

                    friendsList.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);
                }

                foreach (KeyValuePair<string, Friend> kvp in friendsList)
                {
                    if (kvp.Value.friendshipEstablished == true.ToString() && !attendingList.ContainsKey(kvp.Key))
                    {
                        validInvite.Add(kvp.Value.FirstName + ' ' + kvp.Value.LastName + " - " + kvp.Value.userName);
                    }
                }

                //create a list view and populate it with the valid invites. Make it a checkbox and a button at the top for invites
                validInvite.Sort();
                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItemChecked, validInvite);
                lvInvitationPage.Adapter = adapter;

                Button btnSendInvites = (Button) FindViewById(Resource.Id.btnSendInvites);
                btnSendInvites.Click += BtnSendInvites_Click;


            }
            catch
            {
                tvInvitationPageError.Text = "ERROR";
            }


        }

        private async void BtnSendInvites_Click(object sender, EventArgs e)
        {
            var sparseArray = FindViewById<ListView>(Resource.Id.lvInvitationPage).CheckedItemPositions;
            for (var i = 0; i < sparseArray.Size(); i++)
            {
                //tvInvitationPageError.Text += (sparseArray.KeyAt(i) + "=" + sparseArray.ValueAt(i) + ",") + '\n';
                //check to see if the person has been selected.
                if (sparseArray.ValueAt(i) == true)
                {
                    //get that persons information
                    Friend receiver = friendsList[validInvite[sparseArray.KeyAt(i)]];
                    //create the invitation
                    Attendee invitation = new Attendee();
                    invitation.AttendeeId = receiver.newFriendId;
                    invitation.EventId = eventId;

                    //serialize the inviation
                    string serializedInvitation = JsonConvert.SerializeObject(invitation);
                    
                    //generate correct url
                    string url = GetString(Resource.String.IP) + "api/Attendances";
                    //send invite to the person
                    try
                    {
                        string response = await MakePostRequest(url, serializedInvitation, true);
                    }
                    catch
                    {
                        tvInvitationPageError.Text = "ERROR";
                    }
                   
                }

                if (tvInvitationPageError.Text != "ERROR")
                {
                    Toast.MakeText(this, "Invitations Sent", ToastLength.Short).Show();
                    Finish();
                }
            }

            
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

    }
}