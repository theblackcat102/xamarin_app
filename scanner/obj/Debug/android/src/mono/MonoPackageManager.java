package mono;

import java.io.*;
import java.lang.String;
import java.util.Locale;
import java.util.HashSet;
import java.util.zip.*;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.res.AssetManager;
import android.util.Log;
import mono.android.Runtime;

public class MonoPackageManager {

	static Object lock = new Object ();
	static boolean initialized;

	public static android.content.Context Context;

	public static void LoadApplication (Context context, ApplicationInfo runtimePackage, String[] apks)
	{
		synchronized (lock) {
			if (context instanceof android.app.Application) {
				Context = context;
			}
			if (!initialized) {
				android.content.IntentFilter timezoneChangedFilter  = new android.content.IntentFilter (
						android.content.Intent.ACTION_TIMEZONE_CHANGED
				);
				context.registerReceiver (new mono.android.app.NotifyTimeZoneChanges (), timezoneChangedFilter);
				
				System.loadLibrary("monodroid");
				Locale locale       = Locale.getDefault ();
				String language     = locale.getLanguage () + "-" + locale.getCountry ();
				String filesDir     = context.getFilesDir ().getAbsolutePath ();
				String cacheDir     = context.getCacheDir ().getAbsolutePath ();
				String dataDir      = context.getApplicationInfo ().dataDir + "/lib";
				ClassLoader loader  = context.getClassLoader ();

				Runtime.init (
						language,
						apks,
						runtimePackage.dataDir + "/lib",
						new String[]{
							filesDir,
							cacheDir,
							dataDir,
						},
						loader,
						new java.io.File (
							android.os.Environment.getExternalStorageDirectory (),
							"Android/data/" + context.getPackageName () + "/files/.__override__").getAbsolutePath (),
						MonoPackageManager_Resources.Assemblies,
						context.getPackageName ());
				
				mono.android.app.ApplicationRegistration.registerApplications ();
				
				initialized = true;
			}
		}
	}

	public static void setContext (Context context)
	{
		// Ignore; vestigial
	}

	public static String[] getAssemblies ()
	{
		return MonoPackageManager_Resources.Assemblies;
	}

	public static String[] getDependencies ()
	{
		return MonoPackageManager_Resources.Dependencies;
	}

	public static String getApiPackageName ()
	{
		return MonoPackageManager_Resources.ApiPackageName;
	}
}

class MonoPackageManager_Resources {
	public static final String[] Assemblies = new String[]{
		/* We need to ensure that "scanner.dll" comes first in this list. */
		"scanner.dll",
		"Connectivity.Plugin.Abstractions.dll",
		"Connectivity.Plugin.dll",
		"Newtonsoft.Json.dll",
		"RestSharp.dll",
		"SQLite-net.dll",
		"SQLitePCL.raw.dll",
		"Xamarin.Android.Support.Animated.Vector.Drawable.dll",
		"Xamarin.Android.Support.v4.dll",
		"Xamarin.Android.Support.v7.AppCompat.dll",
		"Xamarin.Android.Support.Vector.Drawable.dll",
		"ZXing.Net.Mobile.Core.dll",
		"zxing.portable.dll",
		"ZXingNetMobile.dll",
		"Java.Interop.dll",
		"System.Runtime.dll",
		"System.Threading.Tasks.dll",
		"System.Resources.ResourceManager.dll",
		"System.Threading.dll",
		"System.ServiceModel.Internals.dll",
		"System.Collections.dll",
		"System.Runtime.Extensions.dll",
		"System.Reflection.dll",
		"System.Linq.Expressions.dll",
		"System.Diagnostics.Debug.dll",
		"System.Linq.dll",
		"System.Reflection.Extensions.dll",
		"System.Text.Encoding.dll",
		"System.IO.dll",
		"System.Text.RegularExpressions.dll",
		"System.Globalization.dll",
		"System.Collections.Concurrent.dll",
		"System.Runtime.InteropServices.dll",
	};
	public static final String[] Dependencies = new String[]{
	};
	public static final String ApiPackageName = null;
}
