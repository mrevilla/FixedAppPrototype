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
    [Activity(Label = "PendingFriends")]
    public class PendingFriends : Activity
    {
        private TextView tvPendingError;
        SortedList<string, Friend> pendingFriends = new SortedList<string, Friend>();
        private ListView lvPending;
        private static string AccessToken;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PendingFriends);

            //set views
            tvPendingError = (TextView)FindViewById(Resource.Id.tvPendingError);
            lvPending = (ListView)FindViewById(Resource.Id.lvPending);
            lvPending.FastScrollEnabled = true;

            AccessToken = Intent.GetStringExtra("token");
            string serializedResponse = Intent.GetStringExtra("response");

            dynamic jsonData = JsonConvert.DeserializeObject(serializedResponse);

            foreach (var x in jsonData)
            {
                if (x.friendshipEstablished == false.ToString())
                {
                    Friend person = new Friend();
                    person.newFriendId = x.newFriendId;
                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.PhoneNumber = x.PhoneNumber;
                    person.userName = x.userName;
                    person.friendshipEstablished = x.friendshipEstablished;

                    pendingFriends.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);
                }

            }

            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, pendingFriends.Keys.ToArray());
            // Bind the adapter to the ListView.
            lvPending.Adapter = adapter;
            lvPending.ItemClick += LvPending_ItemClick;
        }

        private void LvPending_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent toFriendProfile = new Intent(this, typeof(PendingProfile));
            var selectedFriend = pendingFriends[pendingFriends.Keys[e.Position]];
            string serializedFriend = JsonConvert.SerializeObject(selectedFriend);
            toFriendProfile.PutExtra("friend", serializedFriend);
            toFriendProfile.PutExtra("token", AccessToken);
            StartActivity(toFriendProfile);

        }

        protected async override void OnResume()
        {
            base.OnResume();
            string serializedResponse;
            dynamic jsonData;
            string url = GetString(Resource.String.IP) + "api/friends";
            pendingFriends.Clear();
            try
            {

                serializedResponse = await MakeGetRequest(url);
                jsonData = JsonConvert.DeserializeObject(serializedResponse);

                if (jsonData == null)
                {
                    tvPendingError.Text = "ERROR";
                }

                foreach (var x in jsonData)
                {
                    if (x.friendshipEstablished == false.ToString())
                    {
                        Friend person = new Friend();
                        person.newFriendId = x.newFriendId;
                        person.FirstName = x.FirstName;
                        person.LastName = x.LastName;
                        person.PhoneNumber = x.PhoneNumber;
                        person.userName = x.userName;
                        person.friendshipEstablished = x.friendshipEstablished;

                        pendingFriends.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);
                    }

                }
            }
            catch
            {
                tvPendingError.Text = "ERROR";
            }

           

            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, pendingFriends.Keys.ToArray());
            // Bind the adapter to the ListView.
            lvPending.Adapter = adapter;
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