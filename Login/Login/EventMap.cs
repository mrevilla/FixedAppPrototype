using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace Login
{
    [Activity(Label = "EventMap")]
    public class EventMap : Activity, IOnMapReadyCallback, GoogleMap.IOnMapLongClickListener
    {
        private TextView tvMVError;
        private static string eventId;
        private static string AccessToken;
        private static string userName;
        Event requestedEvent;
        private GoogleMap mvMap;
        private EditText etMVAddress1;
        private EditText etMVAddress2;
        private EditText etMVCity;
        private EditText etMVState;
        private EditText etMVPostal;
        private EditText etMVEventName;
        private EditText etMVDetails;
        private EditText etMVDateTime;
        private EditText etMVLongitude;
        private EditText etMVLatitude;
        private EditText etMVEventHost;

        private Button btnMVDelete;
        //SortedList<string, string> eventDetails = new SortedList<string, string>();
        //SortedList<string, string> eventIds = new SortedList<string, string>();
        List<string> invited = new List<string>();
        SortedList<string, Event> mapEvents = new SortedList<string, Event>();
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EventMap);

            //set views
            tvMVError = (TextView)FindViewById(Resource.Id.tvMVError);
            etMVAddress1 = (EditText)FindViewById(Resource.Id.etMVAddress1);
            etMVAddress2 = (EditText)FindViewById(Resource.Id.etMVAddress2);
            etMVCity = (EditText)FindViewById(Resource.Id.etMVCity);
            etMVState = (EditText)FindViewById(Resource.Id.etMVState);
            etMVPostal = (EditText)FindViewById(Resource.Id.etMVPostal);
            etMVEventName = (EditText)FindViewById(Resource.Id.etMVEventName);
            etMVDetails = (EditText)FindViewById(Resource.Id.etMVDetails);
            etMVDateTime = (EditText)FindViewById(Resource.Id.etMVDateTime);
            etMVLongitude = (EditText)FindViewById(Resource.Id.etMVLongitude);
            etMVLatitude = (EditText)FindViewById(Resource.Id.etMVLatitude);
            etMVEventHost = (EditText)FindViewById(Resource.Id.etMVEventHost);

             btnMVDelete= (Button)FindViewById(Resource.Id.btnMVDelete);
            btnMVDelete.Click += BtnMVDelete_Click;

            AccessToken = Intent.GetStringExtra("token");
            userName = Intent.GetStringExtra("userName");

            //build urls
            string EventsUrl = GetString(Resource.String.IP) + "api/events";
            string friendEventsUrl = GetString(Resource.String.IP) + "api/attendances";

            try
            {

                //get all the events
                //if the events match the username or are in the attending event, put them in map events

                //get the attending events
                string attendanceResponse = await MakeGetRequest(friendEventsUrl);

                if (attendanceResponse != null)
                {
                    dynamic attendanceEvents = JsonConvert.DeserializeObject(attendanceResponse);

                    foreach (var x in attendanceEvents)
                    {
                        //eventDetails.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventDetails.ToString());                        //eventDetails.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventDetails.ToString());                        //eventDetails.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventDetails.ToString());
                        //eventIds.Add(x.EventName.ToString() + " - " + x.EventOwner.ToString(), x.EventId.ToString());
                        invited.Add(x.EventId.ToString());
                    }

                    invited.Sort();
                }

                string allSerialized = await MakeGetRequest(EventsUrl);

                if (allSerialized != null)
                {
                    dynamic allEvents = JsonConvert.DeserializeObject(allSerialized);

                    foreach (var x in allEvents)
                    {
                        if (invited.Contains(x.EventId.ToString()) || x.Username.ToString() == userName)
                        {
                            Event captureEvent = new Event();
                            captureEvent.Address1 = x.Address1.ToString();
                            captureEvent.Address2 = x.Address2.ToString();
                            captureEvent.City = x.City.ToString();
                            captureEvent.State = x.State.ToString();
                            captureEvent.Country = x.Country.ToString();
                            captureEvent.PostalCode = x.PostalCode.ToString();
                            captureEvent.Longitude = x.Longitude;
                            captureEvent.Latitude = x.Latitude;
                            captureEvent.Name = x.Name.ToString();
                            captureEvent.Details = x.Details.ToString();
                            captureEvent.EventDateTime = x.EventDateTime.ToString();
                            captureEvent.Username = x.Username;

                            mapEvents.Add(x.EventId.ToString(), captureEvent);
                        }
                    }

                }

            }
            catch
            {
                tvMVError.Text = "ERROR GETTING EVENTS";
            }

            SetUpMap();
        }

        private async void BtnMVDelete_Click(object sender, EventArgs e)
        {
            string deleteUrl = GetString(Resource.String.IP) + "api/Events/" + eventId;

            try
            {
                string response = await MakeDeleteRequest(deleteUrl, true);
                Toast.MakeText(this, "Event Deleted", ToastLength.Short).Show();
                etMVAddress1.Text = "";
                etMVAddress2.Text = "";
                etMVCity.Text = "";
                etMVState.Text = "";
                etMVPostal.Text = "";
                etMVEventName.Text = "";
                etMVDetails.Text = "";
                etMVDateTime.Text = "";
                etMVLongitude.Text = "";
                etMVLatitude.Text = "";
                etMVEventHost.Text = "";
                btnMVDelete.Visibility = ViewStates.Gone;

                mapEvents.Remove(eventId);
                mvMap.Clear();
                SetUpMap();
                OnMapReady(mvMap);

            }
            catch
            {
                tvMVError.Text = "ERROR IN DELETING";
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

        private void SetUpMap()
        {
            if (mvMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mvMap).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mvMap = googleMap;



            LatLng latlng = new LatLng(39.519962, -119.797516);
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 11);
            mvMap.MoveCamera(camera);

            //place a pin where the event is located
            //loop through each of the events and place a pin.

            foreach (KeyValuePair<string, Event> x in mapEvents)
            {
                LatLng newPin = new LatLng(x.Value.Latitude, x.Value.Longitude);
                MarkerOptions options = new MarkerOptions().SetPosition(newPin).
                    SetTitle(x.Value.Name + " - " + x.Value.Username).SetSnippet(x.Value.Details);


                if (x.Value.Username == userName)
                {
                    options.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueCyan));
                }
                Marker newMarker = mvMap.AddMarker(options);
                newMarker.Tag = x.Key;


            }



            mvMap.MarkerClick += MvMap_MarkerClick;



            //mvMap.AddMarker(options);
            //epMap.SetOnMapLongClickListener(this);
        }

        private void MvMap_MarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            e.Marker.ShowInfoWindow();
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(e.Marker.Position, 11);
            mvMap.MoveCamera(camera);

            Event selectedEvent = mapEvents[e.Marker.Tag.ToString()];
            etMVAddress1.Text = selectedEvent.Address1;
            etMVAddress2.Text = selectedEvent.Address2;
            etMVCity.Text = selectedEvent.City;
            etMVState.Text = selectedEvent.State;
            etMVPostal.Text = selectedEvent.PostalCode;
            etMVEventName.Text = selectedEvent.Name;
            etMVDetails.Text = selectedEvent.Details;
            etMVDateTime.Text = selectedEvent.EventDateTime;
            etMVLongitude.Text = selectedEvent.Longitude.ToString();
            etMVLatitude.Text = selectedEvent.Latitude.ToString();
            etMVEventHost.Text = selectedEvent.Username;

            if (userName == selectedEvent.Username)
            {
                btnMVDelete.Visibility = ViewStates.Visible;
                eventId = e.Marker.Tag.ToString();
            }
            else
            {
                btnMVDelete.Visibility = ViewStates.Gone;
            }


        }

        public async Task<string> MakeDeleteRequest(string url, bool isJson)
        {
            //simple request function 
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

        public void OnMapLongClick(LatLng point)
        {

        }
    }
}