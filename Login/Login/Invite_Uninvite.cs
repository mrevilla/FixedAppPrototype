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

/***
 *  The activity prompts the user which friends they can invite or uninvite to an event.
 * 
 * OnCreate The function that is called after the Activity is created.
 *
 * DELAGATES:
 *
 * btnInvite    
 * Event handler used when the user wants to invite their friend to an event. 
 * 
 * btnUninvite    
 * Event handler used when the user wants to uninvite their friend to an event.
 */

namespace Login
{
    [Activity(Label = "Invite_Uninvite")]
    public class Invite_Uninvite : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Invite_Uninvite);

            Button btnInvite = (Button)FindViewById(Resource.Id.btnInvite);
            TextView tvIUError = (TextView) FindViewById(Resource.Id.tvIUError);

            btnInvite.Click += delegate
            {
                //Start InvitationPage activity with token and eventId extras.
                Intent toInvitationPage = new Intent(this,typeof(InvitationPage));
                string accessToken = Intent.GetStringExtra("token");
                string eventId = Intent.GetStringExtra("eventId");
                toInvitationPage.PutExtra("token", accessToken);
                toInvitationPage.PutExtra("eventId", eventId);
                StartActivity(toInvitationPage);

            };

            Button btnUninvite = (Button)FindViewById(Resource.Id.btnUninvite);
            btnUninvite.Click += delegate
            {
            
                //Start UninvitePage activity with token and eventId extras.
                Intent toUninvitePage = new Intent(this, typeof(UninvitePage));
                string accessToken = Intent.GetStringExtra("token");
                string eventId = Intent.GetStringExtra("eventId");
                toUninvitePage.PutExtra("token", accessToken);
                toUninvitePage.PutExtra("eventId", eventId);
                StartActivity(toUninvitePage);
            };
        }
    }
}
