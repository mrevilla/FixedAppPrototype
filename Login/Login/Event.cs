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

namespace Login
{
    class Event
    {
        public string Name;
        public string EventDateTime;
        public string Details;
        public string Address1;
        public string Address2;
        public string City;
        public string State;
        public string Country;
        public string PostalCode;
        public double Latitude;
        public double Longitude;
        public string Username;
    }
}