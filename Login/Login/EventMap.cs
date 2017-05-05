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
/***
* EventMap.cs Activity: This activity uses the Google Maps API to list out the created and invited events of user. 
*
* 
* OnCreate: Initializes and sets activity view to display map and corresponding events. 
*
* BtnMVDelete_Click: Event handler which makes a delete request to delete event.
*
* MakeGetRequest: Receives data from web api through http get request and returns the content received. 
*
* SetUpMap: Sets up the map view to display in activity.
* 
* OnMapReady: Handles all of the pin posting and markers sent to the map along with handlers for marker options
* 
* MvMap_MarkerClick: Displays the event information when an even pin is clicked on and focuses camera on location
* 
* MakeDeleteRequest: Sets the http delete request sent to the web api
*
* OnMapLongClick: 
*
***/
namespace Login
{
    [Activity(Label = "EventMap")]
    public class EventMap : Activity, IOnMapReadyCallback, GoogleMap.IOnMapLongClickListener
    {
        // initialize variables
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

            // wire buttons
            btnMVDelete= (Button)FindViewById(Resource.Id.btnMVDelete);
            btnMVDelete.Click += BtnMVDelete_Click;

            // get extra from previous activity 
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
                    
                    // loop through events 
                    foreach (var x in allEvents)
                    {
                        // checks for all events invited or created by user and adds to events list 
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
            // set url access to web api
            string deleteUrl = GetString(Resource.String.IP) + "api/Events/" + eventId;

            try
            {
                // make delete request to web api to delete event 
                string response = await MakeDeleteRequest(deleteUrl, true);
                Toast.MakeText(this, "Event Deleted", ToastLength.Short).Show();
                
                // resets the textviews in activity to blank 
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

                // removes event from list 
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
            //initialize http request and set method to a get request 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            //initialize variables to stream data from web api
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            // read content that was returned from the get request
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
            // if there is no map set fragmet to display map specified in OnMapReady
            if (mvMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mvMap).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            // initialize map and set camera to lattitude location 
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
                
                // set the title and information of event 
                SetTitle(x.Value.Name + " - " + x.Value.Username).SetSnippet(x.Value.Details);

                // changes color of pin to indicate if event is created by user 
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
            // shows information of marker and sets camera position
            e.Marker.ShowInfoWindow();
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(e.Marker.Position, 11);
            mvMap.MoveCamera(camera);

            // set textview of activity to reflect information of event selected
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

            // allows event to be deleted if user is owner of event
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
            // initialize http request and set url to passed string 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            // set http method to delete 
            request.Method = "DELETE";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {

                writer.Flush();
                writer.Dispose();
            }

            // initialize stream reader to previous request 
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();

            // read data from stream
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
