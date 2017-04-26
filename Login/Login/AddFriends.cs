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
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Login
{
    [Activity(Label = "AddFriends")]
    public class AddFriends : Activity
    {
        TextView tvFellowPartiers;
        private static string AccessToken;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.AddFriends);

            tvFellowPartiers = (TextView)FindViewById(Resource.Id.tvFellowPartiers);

            AccessToken = Intent.GetStringExtra("token");

            string url = GetString(Resource.String.IP) + "/api/friends";

            string response = await MakeGetRequest(url);

            dynamic jsonData = JsonConvert.DeserializeObject(response);

            foreach(var x in jsonData)
            {
                tvFellowPartiers.Text = tvFellowPartiers.Text + '\n' + x.UserName;
            }
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
       //     request.Headers.Add("Authorization", "Bearer " + AccessToken);

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