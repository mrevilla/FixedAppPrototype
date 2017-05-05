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
using Newtonsoft.Json;

/***
 * Activity that handles the creation of an event. User enters details of the
 * event, such as name and location. Sends a POST request to post the details
 * of the event in the database.
 *
 * OnCreate
 * The function that is called after the activity is created.
 *
 * BtnCETime_Click
 * Event handler used when the user clicks on the set time button.
 *
 * BtnCEDate_Click
 * Event handler used when the user clicks on the set date button.
 *
 * BtnCECreateEvent_Click
 * Event handler used when the user clicks on the create event button.
 *
 * BtnSelectLocation_Click
 * Event handler used when the user clicks on the set location button.
 *
 * OnActivityResult
 * Sets the event location that was given by the user.
 *
 * OnCreateDialog
 * Allows the date picker and time picker dialogs to open.
 *
 * OnDateSet
 * Sets the date that was given by the user.
 *
 * OnTimeSet
 * Sets the time that was given by the user.
 *
 * MakePostRequest
 * Sends HTTP Request to API.
*/

namespace Login
{
    [Activity(Label = "CreateEvent")]
    public class CreateEvent : Activity, DatePickerDialog.IOnDateSetListener, TimePickerDialog.IOnTimeSetListener
    {
        private static string AccessToken;
        private EditText etCEEventName;
        private EditText etCEDetails;
        private static Event evt;
        private TextView tvCEError;
        private TextView tvCETime;
        private TextView tvCEDate;
        private const int DATE_DIALOG = 1;
        private const int TIME_DIALOG = 2;
        private int year = DateTime.Now.Year;
        private int month = DateTime.Now.Month;
        private int day = DateTime.Now.Day;
        private int hours;
        private int minutes;
        private TextView tvCEAddress;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.CreateEvent);

            evt = new Event();

            AccessToken = Intent.GetStringExtra("token");

            etCEEventName = (EditText)FindViewById(Resource.Id.etCEEventName);
            etCEDetails = (EditText)FindViewById(Resource.Id.etCEDetails);
            tvCEError = (TextView)FindViewById(Resource.Id.tvCEError);
            tvCETime = (TextView)FindViewById(Resource.Id.tvCETime);
            tvCEDate = (TextView)FindViewById(Resource.Id.tvCEDate);
            tvCEAddress = (TextView)FindViewById(Resource.Id.tvCEAddress);

            // EVENT HANDLER
            Button btnCEDate = (Button)FindViewById(Resource.Id.btnCEDate);
            btnCEDate.Click += BtnCEDate_Click;

            Button btnCETime = (Button)FindViewById(Resource.Id.btnCETime);
            btnCETime.Click += BtnCETime_Click;

            Button btnSelectLocation = (Button)FindViewById(Resource.Id.btnSelectLocation);
            btnSelectLocation.Click += BtnSelectLocation_Click;

            Button btnCECreateEvent = (Button)FindViewById(Resource.Id.btnCECreateEvent);
            btnCECreateEvent.Click += BtnCECreateEvent_Click;
        }

        private void BtnCETime_Click(object sender, EventArgs e)
        {
            // Show clock to set time
            ShowDialog(TIME_DIALOG);
        }

        private void BtnCEDate_Click(object sender, EventArgs e)
        {
            // Show calendar to set date
            ShowDialog(DATE_DIALOG);
        }

        private async void BtnCECreateEvent_Click(object sender, EventArgs e)
        {
            // Creates the event upon tapping the create event button
            evt.Name = etCEEventName.Text;
            evt.Details = etCEDetails.Text;

            string formattedString = tvCEDate.Text + tvCETime.Text;
            evt.EventDateTime = formattedString;

            string payload = JsonConvert.SerializeObject(evt);
            
            // Get database url
            string url = GetString(Resource.String.IP) + "api/events";

            try
            {
                string response = await MakePostRequest(url, payload, true);


                if (response != null)
                {
                    Toast.MakeText(this, "Event Created", ToastLength.Short).Show();
                }
                Finish();

            }
            catch (Exception exception)
            {
                tvCEError.Text = "ERROR";
            }


        }

        private void BtnSelectLocation_Click(object sender, EventArgs e)
        {
            // Switches to the set location activity
            Intent activityIntent = new Intent(this, typeof(SelectLocation));
            StartActivityForResult(activityIntent, 0);
        }

        protected override void OnActivityResult(int requestCode,
            [GeneratedEnum] Result resultCode, Intent data)
        {
            // Sets the event location that was given by the user
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {

                evt.Address1 = data.GetStringExtra("address1");
                evt.Address2 = data.GetStringExtra("address2");
                evt.City = data.GetStringExtra("city");
                evt.State = data.GetStringExtra("state");
                evt.PostalCode = data.GetStringExtra("postal");
                evt.Latitude = data.GetDoubleExtra("lat", -1);
                evt.Longitude = data.GetDoubleExtra("lng", -1);

                tvCEAddress.Text = "Address:" + '\n'
                    + evt.Address1 + '\n'
                    + evt.Address2 + '\n'
                    + evt.City + ", " + evt.State + evt.PostalCode + '\n'
                    + evt.Latitude.ToString() + ", " + evt.Longitude.ToString();
            }
        }

        protected override Dialog OnCreateDialog(int id)
        {
            // Opens either the date picker or time picker dialog
            switch (id)
            {
                case DATE_DIALOG:
                    {
                        return new DatePickerDialog(this, this, year, month - 1, day);
                    }
                    break;

                case TIME_DIALOG:
                    {
                        return new TimePickerDialog(this, this, hours, minutes, false);
                    }
                    break;

                default:
                    break;

            }
            return null;
        }

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            // Sets the date that was given by the user
            this.year = year;
            this.month = month;
            this.day = dayOfMonth;

            tvCEDate.Text = year.ToString() + '-' + (month + 1).ToString("00") + '-' + dayOfMonth.ToString("00");
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            // Sets the time that was given by the user
            this.hours = hourOfDay;
            this.minutes = minute;

            tvCETime.Text = "T" + hourOfDay.ToString("00") + ":" + minute.ToString("00") + ":" + "00";
        }

        public async Task<string> MakePostRequest(string url, string serializedDataString, bool isJson)
        {
            // Sets http request to be a post
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            // Add the token to the url
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            var stream = await request.GetRequestStreamAsync();
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
