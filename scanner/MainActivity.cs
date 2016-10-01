﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Net;
using System.Net;
using System.Net.Http;
using Android.Content;
using System.Collections.Generic;
using System.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System;
using SQLite;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Views;
using Newtonsoft.Json;

namespace scanner
{
	[Activity(Label = "scanner", MainLauncher = true, Icon = "@mipmap/icon", Theme ="@style/MyTheme" )]

	public class MainActivity : ActionBarActivity
	{
		private async void BtnScan_Click(object sender, EventArgs e)
		{
			//var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
			var scanner = new ZXing.Mobile.MobileBarcodeScanner();
           
			//options.PossibleFormats = new List<ZXing.BarcodeFormat>() {
			//	ZXing.BarcodeFormat.CODE_93, ZXing.BarcodeFormat.CODE_39
			//};

			var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
			{
				TryHarder = true,
				PossibleFormats = new List<ZXing.BarcodeFormat>
				{
					ZXing.BarcodeFormat.CODE_39
				}
			};
			var result = await scanner.Scan(options);
			if (result != null)
			{
				string id;
				id = result.Text;
				id = id.Remove(0, 2);
				id = id.Remove(id.Length - 1);
				//Toast.MakeText(this, id, ToastLength.Long).Show();
				string url = "http://theblackcat102.nctu.me:5000/NCTU/api/v1.0/tasks/" + id.ToString();
				JsonValue pay = await FetchStudentAsync(url);
				if (GotPay(pay))
				{
					Toast.MakeText(this, id + ",有繳學聯會費喔", ToastLength.Long).Show();
				}
				else {
					Toast.MakeText(this, id + ",沒有繳學聯會費！！", ToastLength.Long).Show();
				}
			}
		}

		private async void testBtn_Click(object sender, EventArgs e)
		{
			var sqlLiteFilePath = GetFileStreamPath("") + "/db_user.db";
			if (File.Exists(sqlLiteFilePath))
			{
				File.Delete(sqlLiteFilePath);
			}
			string response = createDB(sqlLiteFilePath);
			Toast.MakeText(this, response, ToastLength.Short).Show();
			string TARGETURL = "http://theblackcat102.nctu.me:5000/NCTU/api/v1.0/grade/108";
				//Toast.MakeText(this, TARGETURL, ToastLength.Short).Show();
			var handler = new HttpClientHandler();
				//Toast.MakeText(this, "handler gen pass", ToastLength.Short).Show();
				// ... Use HttpClient.            
			var client = new HttpClient(handler);
				//Toast.MakeText(this, "client gen pass", ToastLength.Short).Show();
			string result = null;
			var byteArray = Encoding.ASCII.GetBytes("theblackcat102:AF51FAF9BE961002");
				//Toast.MakeText(this, "encode gen pass", ToastLength.Short).Show();
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
				//Toast.MakeText(this, "auth gen pass", ToastLength.Short).Show();
				//Console.WriteLine("Successful!\n");
			try
			{
				HttpResponseMessage ServerResponse = await client.GetAsync(TARGETURL);
				HttpContent content = ServerResponse.Content;
				result = await content.ReadAsStringAsync();
				// ... Check Status Code                                
			}
			catch (HttpRequestException except)
			{
				Toast.MakeText(this, "client get failed:" + except.Message, ToastLength.Short).Show();
			}


			// ... Display the result.
			ProgressDialog progress;
			progress = ProgressDialog.Show(this, "Loading", "Please Wait...", true);
			await Task.Factory.StartNew(
			() =>
			{
				if (result != null && result.Length >= 50)
				{
					
					try
					{
						string dbPath = GetFileStreamPath("") + "/db_user.db";
						var db = new SQLiteConnection(dbPath);
						Console.WriteLine("try and catch passed\n");
						var students = Newtonsoft.Json.Linq.JObject.Parse(result);
						foreach (var student in students["students"])
						{
							byte[] utf8bytes = Encoding.UTF8.GetBytes((string)student["name"]);
							string name = Encoding.UTF8.GetString(utf8bytes, 0, utf8bytes.Length);
							var stu = new Student { name = name, stuID = (string)student["stuID"], pay = (string)student["pay"], sex = (string)student["sex"] };
							db.Insert(stu);
							Console.WriteLine("inserted\n");
						}
					}
					catch (SQLiteException ex)
					{
						Console.WriteLine("Sqlite exploded");
						Console.WriteLine(ex.Message);
					}
				}
			}).ContinueWith(
				t =>
				{
					if (progress != null)
					{
						progress.Hide();
					}

				}, TaskScheduler.FromCurrentSynchronizationContext()
			);
		}

        //private member needed for navigated drawer
        private DrawerLayout drawerLayout;
        private ListView leftDrawer;
        private ActionBarDrawerToggle DrawerToggle;
        private ArrayAdapter<String> drawerAdapter;
        private string[] index = { "掃描", "添加內容", "搜尋", "下載資料" };

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

            //Set Toolbar
            SupportToolbar Toolbar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(Toolbar);
            Toolbar.SetLogo(Resource.Mipmap.Icon);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            //Set DrawerToggle
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            leftDrawer = FindViewById<ListView>(Resource.Id.left_drawer);
            DrawerToggle = new ActionBarDrawerToggle(
                this,                         //Host Activity
                drawerLayout,               //DrawerLayout
                Resource.String.openDrawer,
                Resource.String.closeDrawer
                );
            DrawerToggle.SyncState();
            drawerLayout.SetDrawerListener(DrawerToggle);

            //Create Index to Add in the drawer            
            drawerAdapter = new ArrayAdapter<String>(this,Android.Resource.Layout.SimpleListItem1,index);
            leftDrawer.SetAdapter(drawerAdapter);
            leftDrawer.ItemClick += leftDrawer_ItemClick;
		}

        /********This Area is for the ckick event for toolbar and its button*******/
        //Any icon on the toolbar was "clicked"
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            DrawerToggle.OnOptionsItemSelected(item);
            return base.OnOptionsItemSelected(item);
        }

        private void leftDrawer_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (index[e.Position] == "掃描")
            {
                BtnScan_Click(sender, e);
            }
            else if (index[e.Position] == "添加內容")
            {
                var intent = new Intent(this, typeof(InputNewUser));
                StartActivity(intent);
            }

            else if (index[e.Position] == "搜尋")
            {
                var searchLayout = new Intent(this, typeof(Search));
                StartActivity(searchLayout);
            }

            else if (index[e.Position] == "下載資料")
            {
                testBtn_Click(sender, e);
            }
        }

        /********This Area is for the ckick event for toolbar and its button*******/

        public string insertDB( string name, string id, string pay,string sex)
		{
			try
			{
				string dbPath = GetFileStreamPath("") + "/db_user.db";
				var db = new SQLiteConnection(dbPath);
				var stu = new Student { name = name, stuID = id, pay = pay ,sex = sex};
				db.Insert(stu);
				return "success";
			}
			catch (SQLiteException ex)
			{
				return ex.Message;
			}
		}
		private bool GotPay(JsonValue json)
		{
			JsonValue Pay = json["task"];
			bool payment = Pay["pay"];
			return payment;
		}
		public string createDB(string dbPath)
		{
			try
			{
				var db = new SQLiteConnection(dbPath);
				db.CreateTable<Student>();
				db.Close();
				return "db create success";
			}
			catch (SQLiteException ex)
			{
				return ex.Message;
			}
		}

		private async Task<JsonValue> FetchStudentAsync(string url)
		{
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new System.Uri(url));
			request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("theblackcat102:AF51FAF9BE961002"));
			request.ContentType = "application/json";
			request.Method = "GET";
			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync())
			{
				//Get a stream representation of the HTTP web response:
				using (Stream stream = response.GetResponseStream())
				{
					// Use this stream to build a JSON document object:
					JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
					//string reponse = "Response: " + jsonDoc.ToString();
					//debug purpose
					//Toast.MakeText(this, reponse, ToastLength.Long).Show();
					// Return the JSON document:
					return jsonDoc;
				}
			}
			//private CredentialCache GetCredential(string url)
			//{
			//	ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
			//	CredentialCache credentialCache = new CredentialCache();
			//	credentialCache.Add(new System.Uri(url), "Basic", new NetworkCredential("python", "python123"));
			//	return credentialCache;
			//}
		}
	}
}


