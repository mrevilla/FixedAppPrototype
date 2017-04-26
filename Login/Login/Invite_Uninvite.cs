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

            Button btnInvite = (Button) FindViewById(Resource.Id.btnInvite);

            btnInvite.Click += delegate
            {
                
            };

            Button btnUninvite = (Button) FindViewById(Resource.Id.btnUninvite);
            btnUninvite.Click += delegate
            {

            };
        }
    }
}