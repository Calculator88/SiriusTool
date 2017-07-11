using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using SiriusTimetable.Common.ViewModels;
using SiriusTimetable.Core.Services;
using SiriusTimetable.Core.Services.Abstractions;
using SiriusTimetable.Droid.Services;

namespace SiriusTimetable.Droid
{
	public class SplashActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			RegisterServices();

			var intent = new Intent(Application.Context, typeof(MainActivity));
			var team = ServiceLocator.GetService<ICacher>().Get(ServiceLocator.GetService<ICacher>().CacheDirectory + MainActivity.CACHEDTEAMNAME);
			var shortTeam = ServiceLocator.GetService<ICacher>().Get(ServiceLocator.GetService<ICacher>().CacheDirectory + MainActivity.CACHEDSHORTTEAM);
			if(!String.IsNullOrEmpty(team))
			{
				intent.PutExtra(MainActivity.ISTEAMCACHED, true);
				intent.PutExtra(MainActivity.CACHEDTEAMNAME, team);
				intent.PutExtra(MainActivity.CACHEDSHORTTEAM, shortTeam);
			}

			StartActivity(intent);
			Finish();
		}
		private void RegisterServices()
		{
			ServiceLocator.RegisterService<IDateTimeService>(new DateTimeService());
			ServiceLocator.RegisterService<IResourceService>(new ResourceService(Resources));
			//ServiceLocator.RegisterService<ISelectedTeamCacher>(new SelectedTeamCacher(CacheDir.Path));
			ServiceLocator.RegisterService<ITimerService>(new TimerService());
			ServiceLocator.RegisterService<ICacher>(new Cacher(CacheDir.Path));
			ServiceLocator.RegisterService<ITimetableDownloader>(new TimetableDownloaderTesting());
			ServiceLocator.RegisterService<IJsonParser>(new JsonParser());
			ServiceLocator.RegisterService<ITimetableProvider>(new TimetableProvider());
			ServiceLocator.RegisterService<ILocalNotificationService>(new LocalNotificationService());
			ServiceLocator.RegisterService(new TimetableViewModel(ServiceLocator.GetService<IDateTimeService>().GetCurrentTime()));
		}
	}
}