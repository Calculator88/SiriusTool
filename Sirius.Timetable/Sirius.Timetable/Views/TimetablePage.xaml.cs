﻿using System;
using Sirius.Timetable.Models;
using Sirius.Timetable.ViewModels;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;

namespace Sirius.Timetable.Views
{
	public partial class TimetablePage
	{
		public TimetablePage()
		{
			InitializeComponent();
			BindingContext = _viewModel = new TimetableViewModel(null, null);
		}
		
		private void ListViewOnActivityTapped(object sender, ItemTappedEventArgs e)
		{
			var item = (TimetableItem)e.Item;
			item.IsSelected = !item.IsSelected;
		}

		private readonly TimetableViewModel _viewModel;

		private void ListViewOnActivitySelected(object sender, SelectedItemChangedEventArgs e)
		{
			((ListView) sender).SelectedItem = null;
		}
	}
}