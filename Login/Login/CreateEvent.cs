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
            ShowDialog(TIME_DIALOG);
        }

        private void BtnCEDate_Click(object sender, EventArgs e)
        {
            ShowDialog(DATE_DIALOG);
        }

        private async void BtnCECreateEvent_Click(object sender, EventArgs e)
        {
            evt.Name = etCEEventName.Text;
            evt.Details = etCEDetails.Text;

            string formattedString = tvCEDate.Text + tvCETime.Text;
            evt.EventDateTime = formattedString;

            string payload = JsonConvert.SerializeObject(evt);
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
            Intent activityIntent = new Intent(this, typeof(SelectLocation));
            StartActivityForResult(activityIntent, 0);
        }

        protected override void OnActivityResult(int requestCode,
            [GeneratedEnum] Result resultCode, Intent data)
        {
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
            this.year = year;
            this.month = year;
            this.day = dayOfMonth;

            tvCEDate.Text = year.ToString() + '-' + month.ToString("00") + '-' + dayOfMonth.ToString("00");
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            this.hours = hourOfDay;
            this.minutes = minute;

            tvCETime.Text = "T" + hourOfDay.ToString("00") + ":" + minute.ToString("00") + ":" + "00";
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
