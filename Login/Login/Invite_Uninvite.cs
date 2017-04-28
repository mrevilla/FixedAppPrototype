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