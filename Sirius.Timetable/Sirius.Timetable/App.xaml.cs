﻿using Sirius.Timetable.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Sirius.Timetable
{
	public partial class App
	{
        public App()
		{
			InitializeComponent();

			SetMainPage();
		}

		public static void SetMainPage()
		{
            Current.MainPage = new Master();
                //Children =
                //{
                //    new NavigationPage(new ItemsPage())
                //    {
                //        Title = "Browse",
                //        Icon = Device.OnPlatform<string>("tab_feed.png",null,null)
                //    },
                //    new NavigationPage(new AboutPage())
                //    {
                //        Title = "About",
                //        Icon = Device.OnPlatform<string>("tab_about.png",null,null)
                //    },
                //}
        }
	}
}
