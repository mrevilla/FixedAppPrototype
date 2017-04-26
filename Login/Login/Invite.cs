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
    [Activity(Label = "Invite")]
    public class Invite : Activity
    {
        ListView listViewInviteFriends;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Invite);

            listViewInviteFriends = (ListView) FindViewById(Resource.Id.listViewInviteFriends);
            listViewInviteFriends.FastScrollEnabled = true;

            //get users invited to the event
            //if null request all friends
            //if not null remove the already invited friends

        }
    }
}