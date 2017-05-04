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
 *
 * BtnAddress_Click
 *
 * SetUpMap
 * 
 * OnMapReady
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
            
            //EVENT HANDLERS
            Button btnAddress = (Button)FindViewById(Resource.Id.btnAddress);
            btnAddress.Click += BtnAddress_Click;

            Button btnSaveLocation = (Button) FindViewById(Resource.Id.btnSaveLocation);
            btnSaveLocation.Click += BtnSaveLocation_Click;

        }

        private void BtnSaveLocation_Click(object sender, EventArgs e)
        {
            //if input was valid, save the daata and return the extras
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
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.slMap).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            slMap = googleMap;
            LatLng latlng = new LatLng(39.519962, -119.797516);
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 11);
            slMap.MoveCamera(camera);
            slMap.SetOnMapLongClickListener(this);
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";

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

        public async void OnMapLongClick(LatLng point)
        {
            lat = point.Latitude;
            lng = point.Longitude;
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(point, 17);
            slMap.MoveCamera(camera);
            MarkerOptions options = new MarkerOptions().SetPosition(point);
            slMap.AddMarker(options);

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

                string formattedAddress = jsondata.results[0].formatted_address;

                List<string> address = formattedAddress.Split(',').ToList<string>();
                etAddress1.Text = address[0];
                etCity.Text = address[1];
                etState.Text = address[2].Substring(0,3);
                etPostal.Text = address[2].Substring(3, 6);
            }

        }
    }
}
