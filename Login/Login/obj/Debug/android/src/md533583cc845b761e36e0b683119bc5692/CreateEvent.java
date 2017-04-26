package md533583cc845b761e36e0b683119bc5692;


public class CreateEvent
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer,
		android.app.DatePickerDialog.OnDateSetListener,
		android.app.TimePickerDialog.OnTimeSetListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onActivityResult:(IILandroid/content/Intent;)V:GetOnActivityResult_IILandroid_content_Intent_Handler\n" +
			"n_onCreateDialog:(I)Landroid/app/Dialog;:GetOnCreateDialog_IHandler\n" +
			"n_onDateSet:(Landroid/widget/DatePicker;III)V:GetOnDateSet_Landroid_widget_DatePicker_IIIHandler:Android.App.DatePickerDialog/IOnDateSetListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_onTimeSet:(Landroid/widget/TimePicker;II)V:GetOnTimeSet_Landroid_widget_TimePicker_IIHandler:Android.App.TimePickerDialog/IOnTimeSetListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("Login.CreateEvent, Login, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", CreateEvent.class, __md_methods);
	}


	public CreateEvent () throws java.lang.Throwable
	{
		super ();
		if (getClass () == CreateEvent.class)
			mono.android.TypeManager.Activate ("Login.CreateEvent, Login, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onActivityResult (int p0, int p1, android.content.Intent p2)
	{
		n_onActivityResult (p0, p1, p2);
	}

	private native void n_onActivityResult (int p0, int p1, android.content.Intent p2);


	public android.app.Dialog onCreateDialog (int p0)
	{
		return n_onCreateDialog (p0);
	}

	private native android.app.Dialog n_onCreateDialog (int p0);


	public void onDateSet (android.widget.DatePicker p0, int p1, int p2, int p3)
	{
		n_onDateSet (p0, p1, p2, p3);
	}

	private native void n_onDateSet (android.widget.DatePicker p0, int p1, int p2, int p3);


	public void onTimeSet (android.widget.TimePicker p0, int p1, int p2)
	{
		n_onTimeSet (p0, p1, p2);
	}

	private native void n_onTimeSet (android.widget.TimePicker p0, int p1, int p2);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
