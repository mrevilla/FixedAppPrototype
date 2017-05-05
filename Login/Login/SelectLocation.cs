using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Thread = Java.Lang.Thread;


/***
  *  Activity for selecting a location of where a user is hosting an event. The location
  * can be chosen at a specific location using Google Maps or manually typed in.
  *
  * OnCreate 
  * The function that is called after the Activity is created. 
  *
  * BtnSaveLocation_Click
  * Event handler used when the user attempts to save the location
  * 
  * OnMapLongClick
  * Event handler when putting in the pins
  *
  * BtnAddress_Click
  * Event handler when the user sets the address of the event
  *
  * SetUpMap
  * Set up map fragment
  * 
  * OnMapReady
  * Set map position on Reno
  *
  * MakeGetRequest    
  * Sends GET Request to API
  * 
  */

namespace Login
{
    [Activity(Label = "SelectLocation")]
    public class SelectLocation : Activity, IOnMapReadyCallback, GoogleMap.IOnMapLongClickListener
    {
        private GoogleMap slMap;
        private EditText etAddress1;
        private EditText etAddress2;
        private EditText etCity;
        private EditText etState;
        private EditText etPostal;
        private static string response;
        private static dynamic jsondata;
        private static double lat;
        private static double lng;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SelectLocation);
            SetUpMap();

            etAddress1 = (EditText)FindViewById(Resource.Id.etAddress1);
            etAddress2 = (EditText)FindViewById(Resource.Id.etAddress2);
            etCity = (EditText)FindViewById(Resource.Id.etCity);
            etState = (EditText)FindViewById(Resource.Id.etState);
            etPostal = (EditText)FindViewById(Resource.Id.etPostal);

            Button btnAddress = (Button)FindViewById(Resource.Id.btnAddress);
            btnAddress.Click += BtnAddress_Click;

            Button btnSaveLocation = (Button) FindViewById(Resource.Id.btnSaveLocation);
            btnSaveLocation.Click += BtnSaveLocation_Click;

        }

        private void BtnSaveLocation_Click(object sender, EventArgs e)
        {
            if (etAddress1.Text != null && etCity.Text != null && etState.Text != null && lat != 0.0 && lng != 0.0) 
            {
                Intent returnIntent = new Intent(this, typeof(CreateEvent));
                returnIntent.PutExtra("address1", etAddress1.Text);
                returnIntent.PutExtra("address2", etAddress2.Text);
                returnIntent.PutExtra("city", etCity.Text);
                returnIntent.PutExtra("state", etState.Text);
                returnIntent.PutExtra("postal", etPostal.Text);
                returnIntent.PutExtra("lat", lat);
                returnIntent.PutExtra("lng", lng); 
                
                SetResult(Result.Ok, returnIntent);
                Finish();
            }
        }

        private async void BtnAddress_Click(object sender, EventArgs e)
        {
            //set url to location
            string url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + etAddress1.Text + ' ' +
                etAddress2.Text + etCity.Text + etState.Text;
            try
            {
                response = await MakeGetRequest(url);
            }
            catch (Exception exception)
            {
                etPostal.Text = "BAD REQUEST";
            }

            if (response != null)
            {
                jsondata = JsonConvert.DeserializeObject(response);
                System.Double.TryParse(jsondata.results[0].geometry.location.lat.ToString(), out lat);
                System.Double.TryParse(jsondata.results[0].geometry.location.lng.ToString(), out lng);
                
                //set camera to location and add event
                LatLng latlng = new LatLng(lat, lng);
                CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 17);
                slMap.MoveCamera(camera);
                MarkerOptions options = new MarkerOptions().SetPosition(latlng);
                slMap.AddMarker(options);

            }
        }

        private void SetUpMap()
        {   
            if (slMap == null)
            {   //set up map fragment
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.slMap).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {   //Position it at Reno
            slMap = googleMap;
            LatLng latlng = new LatLng(39.519962, -119.797516);
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 11);
            slMap.MoveCamera(camera);
            slMap.SetOnMapLongClickListener(this);
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            //sets http request to be a get.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";

            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            //read data
            using (StreamReader sr = new StreamReader(respStream))
            {        
                string strContent = sr.ReadToEnd();
                respStream = null;
                return strContent;
            }
        }

        public async void OnMapLongClick(LatLng point)
        {
            //Move camera to location
            lat = point.Latitude;
            lng = point.Longitude;
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(point, 17);
            slMap.MoveCamera(camera);
            MarkerOptions options = new MarkerOptions().SetPosition(point);
            slMap.AddMarker(options);

            //set up url for geolocation.
            string url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + point.Latitude + ',' + point.Longitude;
            try
            {
                response = await MakeGetRequest(url);
            }
            catch (Exception exception)
            {
                etPostal.Text = "BAD REQUEST";
            }

            if (response != null)
            {
                jsondata = JsonConvert.DeserializeObject(response);

                //parses data
                string formattedAddress = jsondata.results[0].formatted_address;

                //displays data
                List<string> address = formattedAddress.Split(',').ToList<string>();
                etAddress1.Text = address[0];
                etCity.Text = address[1];
                etState.Text = address[2].Substring(0,3);
                etPostal.Text = address[2].Substring(3, 6);
            }

        }
    }
}
