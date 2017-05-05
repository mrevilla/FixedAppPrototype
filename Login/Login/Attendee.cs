using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

/***
 * Class that describes which users are going to an event and what specific event it is.
*/

namespace Login
{
    class Attendee
    {
        // Declare Event ID
        public string EventId;
        
        // Declare Attendee ID
        public string AttendeeId;
    }
}
