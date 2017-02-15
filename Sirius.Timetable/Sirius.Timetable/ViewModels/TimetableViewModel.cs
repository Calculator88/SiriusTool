﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sirius.Timetable.Helpers;
using Sirius.Timetable.Models;
using Sirius.Timetable.Services;
using Xamarin.Forms;

namespace Sirius.Timetable.ViewModels
{
	public class TimetableViewModel : ObservableObject
	{
		public TimetableViewModel(DateTime? date, string team)
		{

			SelectTeamCommand = new Command(async () => await SelectTeamExecute());
			RefreshCommand = new Command(async () => await RefreshTimetable(true));

			if (!date.HasValue || String.IsNullOrEmpty(team))
			{
				RefreshOnlyTimetable();
				Timetable = new ObservableRangeCollection<TimetableItem>();
			}
			else
			{
				_date = date.Value;
				_team = team;
				RefreshCommand.Execute(null);
			}
		}

		private async Task SelectTeamExecute()
		{
			var liters = TimetableService.TeamsLiterPossibleNumbers.Select(pair => pair.Key);
			var liter =
				await Application.Current.MainPage.DisplayActionSheet("Выберите команду", "Отмена", null, liters.ToArray());
			Debug.WriteLine(liter);
			if (String.IsNullOrEmpty(liter) || liter == "Отмена")
			{
				IsBusy = false;
				return;
			}
			var numbers = TimetableService.TeamsLiterPossibleNumbers[liter];
			var number = await Application.Current.MainPage.DisplayActionSheet("Выберите команду", "Отмена", null, numbers.ToArray());
			Debug.WriteLine(number);
			if (String.IsNullOrEmpty(number) || number == "Отмена")
			{
				IsBusy = false;
				return;
			}

			_team = liter + number;
			_date = DateTime.ParseExact("06.02.2017", "dd.MM.yyyy", null);
			await RefreshTimetable(false);
			OnPropertyChanged(nameof(Team));
		}

		private static async void RefreshOnlyTimetable()
		{
			await TimetableService.RefreshTimetables();
		}
		private async Task RefreshTimetable(bool hard)
		{
			if (String.IsNullOrEmpty(_team))
			{
				SelectTeamCommand.Execute(null);
				IsBusy = false;
				return;
			}
			if (hard)
				await TimetableService.RefreshTimetables();

			var dateKey = _date.ToString("dd.MM.yyyy").Replace(".", "");
			var timetable = TimetableService.Timetables[dateKey];
			var currentTimetable = timetable.Teams[TimetableService.KeywordDictionary[_team]];
			var collection = currentTimetable.Select(activity => new TimetableItem(activity));
			Timetable = new ObservableCollection<TimetableItem>(collection);

			IsBusy = false;
		}
		public Command RefreshCommand { get; set; }
		public Command SelectTeamCommand { get; set; }
		public string TeamName
		{
			get { return String.IsNullOrEmpty(_teamName) ? "Выбрать команду" : _teamName; }
			set { SetProperty(ref _teamName, value); }
		}
		public bool IsBusy
		{
			get { return _isBusy; }
			set { SetProperty(ref _isBusy, value); }
		}
		public ObservableCollection<TimetableItem> Timetable
		{
			get { return _timetable; }
			set { SetProperty(ref _timetable, value); }
		}
		public string Team => TimetableService.KeywordDictionary[_team];
		private ObservableCollection<TimetableItem> _timetable;
		private string _teamName;
		private bool _isBusy;
		private DateTime _date;
		private string _team;
	}
}