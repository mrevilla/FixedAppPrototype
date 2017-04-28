using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Drm;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;

namespace Login
{
    [Activity(Label = "AddFriends")]
    public class AddFriends : Activity
    {
        private TextView tvAddFriendsError;
        private static string AccessToken;
        private SortedList<string, Friend> allUsers = new SortedList<string, Friend>();
        private ListView lvAddFriends;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.AddFriends);

            //setup views
            tvAddFriendsError = (TextView)FindViewById(Resource.Id.tvAddFriendsError);
            lvAddFriends = (ListView)FindViewById(Resource.Id.lvAddFriends);
            lvAddFriends.FastScrollEnabled = true;
            lvAddFriends.ChoiceMode = Android.Widget.ChoiceMode.Multiple;

            AccessToken = Intent.GetStringExtra("token");
            string serializedFriends = Intent.GetStringExtra("friends");

            //build urlS
            string urlAll = GetString(Resource.String.IP) + "api/friends/all";


            //get all the users in the database
            try
            {
                //make a get request
                string response = await MakeGetRequest(urlAll);
                //deserialize response
                dynamic jsonAll = JsonConvert.DeserializeObject(response);
                //get every user
                foreach (var x in jsonAll)
                {
                    Friend person = new Friend();

                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.userName = x.userName;
                    person.newFriendId = x.newFriendId;

                    allUsers.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);
                }

                //deserialize friends
                dynamic friends = JsonConvert.DeserializeObject(serializedFriends);

                foreach (var x in friends)
                {
                    if (allUsers.ContainsKey((x.FirstName + ' ' + x.LastName + " - " + x.userName).ToString()))
                    {
                        allUsers.Remove((x.FirstName + ' ' + x.LastName + " - " + x.userName).ToString());
                    }

                }

                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItemChecked, allUsers.Keys.ToArray());
                lvAddFriends.Adapter = adapter;

            }
            catch
            {
                tvAddFriendsError.Text = "ERROR";
            }

            Button btnAddSelected = (Button)FindViewById(Resource.Id.btnAddSelected);
            btnAddSelected.Click += BtnAddSelected_Click;
        }

        private async void BtnAddSelected_Click(object sender, EventArgs e)
        {
            string url = GetString(Resource.String.IP) + "api/friends";
            var sparseArray = FindViewById<ListView>(Resource.Id.lvAddFriends).CheckedItemPositions;
            for (var i = 0; i < sparseArray.Size(); i++)
            {
                if (sparseArray.ValueAt(i) == true)
                {
                   
                    Friend selectedPerson = allUsers[allUsers.Keys[sparseArray.KeyAt(i)]];

                    string id = JsonConvert.SerializeObject(selectedPerson.newFriendId);
                    string payload = "{" + "newFriendId :" + id + "}";


                    try
                    {
                        string reply = await MakePostRequest(url, payload, true);

                    }
                    catch
                    {
                        tvAddFriendsError.Text = "ERROR ADDING FRIENDS";
                    }
                }
            

                if (tvAddFriendsError.Text != "ERROR ADDING FRIENDS")
                {
                    Toast.MakeText(this, "Friend Requests Sent", ToastLength.Short).Show();
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