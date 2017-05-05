using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Newtonsoft.Json;

/***
 * Activity that allows the user to modify their profile information,
 * such as first name and last name.
 *
 * OnCreate
 * The function that is called after the Activity is created.
 *
 * BtnEditSaveChanges_Click
 * Event handler used when the user clicks on the save changes button.
 *
 * MakeGetRequest
 * Sends GET Request to API.
 *
 * MakePostRequest
 * Sends HTTP Request to API.
*/

namespace Login.Resources.layout
{
    [Activity(Label = "EditProfile")]
    public class EditProfile : Activity
    {
        EditText etEditFirstName;
        EditText etEditLastName;
        TextView etEditEmail;
        EditText etEditPhoneNumber;
        TextView tvEditError;
        private static string AccessToken;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EditProfile);

            AccessToken = Intent.GetStringExtra("AccessToken"); 

            etEditFirstName = (EditText) FindViewById(Resource.Id.etEditFirstName);
            etEditLastName = (EditText) FindViewById(Resource.Id.etEditLastName);
            etEditEmail = (TextView) FindViewById(Resource.Id.etEditEmail);
            etEditPhoneNumber = (EditText) FindViewById(Resource.Id.etEditPhoneNumber);
            tvEditError = (TextView) FindViewById(Resource.Id.tvEditError);
          

            try
            {
                string url = GetString(Resource.String.IP) + "api/account/userinfo";

                string response = await MakeGetRequest(url);

                dynamic jsonData = JsonConvert.DeserializeObject(response);
                etEditFirstName.Text = jsonData.firstName;
                etEditLastName.Text = jsonData.lastName;
                etEditEmail.Text = jsonData.Email;
                etEditPhoneNumber.Text = jsonData.PhoneNumber; 


            }
            catch 
            {
                tvEditError.Text = "ERROR"; 
            }

            // EVENT HANDLER
            Button btnEditSaveChanges = (Button) FindViewById(Resource.Id.btnEditSaveChanges);
            btnEditSaveChanges.Click += BtnEditSaveChanges_Click;
        }

        private async void BtnEditSaveChanges_Click(object sender, EventArgs e)
        {
            /*Input validation*/
            try
            {
                string url = GetString(Resource.String.IP) + "api/account/userinfo";
                string data = "&PhoneNumber=" + etEditPhoneNumber.Text +
                              "&firstName=" + etEditFirstName.Text + "&lastName=" + etEditLastName.Text;
                string response = await MakePostRequest(url, data,false);

                Finish();
            }
            catch 
            {
                tvEditError.Text = "ERROR";
            }
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

        public async Task<string> MakePostRequest(string url, string serializedDataString, bool isJson)
        {
            // Sets HTTP request to be a POST
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            // Add the token to the URL
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            var stream = await request.GetRequestStreamAsync();
            
            // Write data
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(serializedDataString);
                writer.Flush();
                writer.Dispose();
            }

            // Get data from API reply
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();

            using (StreamReader sr = new StreamReader(respStream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
