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
    }
}