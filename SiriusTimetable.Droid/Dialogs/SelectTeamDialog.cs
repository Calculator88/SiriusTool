using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using SiriusTimetable.Common.ViewModels;
using SiriusTimetable.Core.Services;
using SiriusTimetable.Core.Timetable;
using Android.Support.V7.App;
using Android.Views.Animations;

namespace SiriusTimetable.Droid.Dialogs
{
	public class SelectTeamDialog : AppCompatDialogFragment, View.IOnClickListener , ViewSwitcher.IViewFactory
	{
		#region Private fields

		private const string TeamTag = "TEAM";
		private const string DirectionTag = "DIRECTION";

		private string _selectedDirection;
		private string _selectedGroup;
		private TimetableInfo _info;
		private ISelectTeamDialogResultListener _listener;

		private List<TextView> _numbers;
		private readonly ImageView[] _images = new ImageView[5];
		private TextSwitcher _groupName;
		private LinearLayout _groups;
		private Button _selectButton;
		private TextSwitcher _directionName;

		#endregion

		#region Fragment lifecycle

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			_listener = Activity as ISelectTeamDialogResultListener;
			_info = ServiceLocator.GetService<TimetableViewModel>().TimetableInfo;

			var dataExists = _info.ShortLongTeamNameDictionary != null && 
				(_info.DirectionPossibleNumbers != null || _info.UnknownPossibleTeams != null);
			if(!dataExists) Dismiss();
		}
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Dialog.SetCanceledOnTouchOutside(true);

			var v = inflater.Inflate(Resource.Layout.SelectTeamDialog, null, false);
			
			_groups = v.FindViewById<LinearLayout>(Resource.Id.lay_groups);

			_images[0] = v.FindViewById<ImageView>(Resource.Id.img_science);
			if(!_info.DirectionPossibleNumbers.ContainsKey(TimetableDirection.Science) || _info.DirectionPossibleNumbers[TimetableDirection.Science].Count == 0)
				_images[0].Visibility = ViewStates.Gone;

			_images[1] = v.FindViewById<ImageView>(Resource.Id.img_sport);
			if(!_info.DirectionPossibleNumbers.ContainsKey(TimetableDirection.Sport) || _info.DirectionPossibleNumbers[TimetableDirection.Sport].Count == 0)
				_images[1].Visibility = ViewStates.Gone;

			_images[2] = v.FindViewById<ImageView>(Resource.Id.img_art);
			if(!_info.DirectionPossibleNumbers.ContainsKey(TimetableDirection.Art) || _info.DirectionPossibleNumbers[TimetableDirection.Art].Count == 0)
				_images[2].Visibility = ViewStates.Gone;

			_images[3] = v.FindViewById<ImageView>(Resource.Id.img_literature);
			if(!_info.DirectionPossibleNumbers.ContainsKey(TimetableDirection.Literature) || _info.DirectionPossibleNumbers[TimetableDirection.Literature].Count == 0)
				_images[3].Visibility = ViewStates.Gone;

			_images[4] = v.FindViewById<ImageView>(Resource.Id.img_unknown);
			if(_info.UnknownPossibleTeams == null || _info.UnknownPossibleTeams.Count == 0)
				_images[4].Visibility = ViewStates.Gone;

			_groupName = v.FindViewById<TextSwitcher>(Resource.Id.group_name);
			_groupName.SetFactory(this);

			_directionName = v.FindViewById<TextSwitcher>(Resource.Id.dir_name);
			_directionName.SetFactory(this);

			_selectButton = v.FindViewById<Button>(Resource.Id.btn_select);
			_selectButton.SetOnClickListener(this);

			v.FindViewById<Button>(Resource.Id.btn_close).SetOnClickListener(this);

			var inSlideAnimation = AnimationUtils.LoadAnimation(Context, Android.Resource.Animation.SlideInLeft);
			var outSlideAnimation = AnimationUtils.LoadAnimation(Context, Android.Resource.Animation.SlideOutRight);
			var inFadeAnimation = AnimationUtils.LoadAnimation(Context, Android.Resource.Animation.FadeIn);
			var outFadeAnimation = AnimationUtils.LoadAnimation(Context, Android.Resource.Animation.FadeOut);

			inSlideAnimation.Duration = 80;
			outSlideAnimation.Duration = 80;
			inFadeAnimation.Duration = 150;
			outFadeAnimation.Duration = 150;

			_groupName.InAnimation = inSlideAnimation;
			_groupName.OutAnimation = outSlideAnimation;
			_directionName.InAnimation = inFadeAnimation;
			_directionName.OutAnimation = outFadeAnimation;

			_images[0].SetOnClickListener(this);   
			_images[1].SetOnClickListener(this);
			_images[2].SetOnClickListener(this);
			_images[3].SetOnClickListener(this);
			_images[4].SetOnClickListener(this);
			
			if(savedInstanceState == null) return v;

			var dir = savedInstanceState.GetInt(DirectionTag);
			OnClick(new View(Activity) { Id = dir });

			var team = savedInstanceState.GetInt(TeamTag);
			OnClick(new View(Activity) { Id = team });

			return v;
		}
		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			outState.PutInt(DirectionTag, DirectionToId(_selectedDirection));
			outState.PutInt(TeamTag, TeamToId(_selectedGroup));
		}

		#endregion

		#region Private methods

		private void SetGroupsOpacity(int id)
		{
			if(_numbers == null) return;
			foreach(var number in _numbers)
				number.Alpha = number.Id == id ? 1 : 0.25f;
		}
		private string GetDirectionById(int id)
		{
			var el = _images.FirstOrDefault(view => view.Id == id);
			if(el != null) return (string)el.Tag;
			return null;
		}
		private string GetGroupById(int id)
		{
			return _numbers == null
				? null
				: (from number in _numbers where number.Id == id select (string)number.Tag).FirstOrDefault();
		}
		private void OnChooseGroup(int id)
		{
			var group = GetGroupById(id);
			if(_selectedGroup == group)
				return;

			SetGroupsOpacity(id);
			_selectedGroup = group;

			if(_selectedDirection == Resources.GetString(Resource.String.TxtUnknown))
			{
				_groupName.SetText(_info.ShortLongTeamNameDictionary[group]);
			}
			else
			{
				_groupName.SetText(_info.ShortLongTeamNameDictionary[TimetableInfo.GetDirection(_selectedDirection[0].ToString()) + _selectedGroup]);
			}

			_groupName.Visibility = ViewStates.Visible;
			_selectButton.Enabled = true;
		}
		private void OnChooseDirection(int imageId)
		{
			_selectButton.Enabled = false;
			_groupName.Visibility = ViewStates.Gone;

			if(_selectedDirection == GetDirectionById(imageId))
			{
				UpdateImageOpacity(-1);
				_directionName.Visibility = ViewStates.Gone;
				_groups.Visibility = ViewStates.Gone;
				_selectButton.Enabled = false;
				_selectedDirection = null;
				_selectedGroup = null;
				return;
			}

			UpdateImageOpacity(imageId);
			_selectedDirection = GetDirectionById(imageId);
			_directionName.SetText(_selectedDirection);
			_selectedGroup = null;

			if(_selectedDirection == Resources.GetString(Resource.String.TxtUnknown))
				_numbers = _info.UnknownPossibleTeams.Select(GetGroupSelector).ToList();
			else
				_numbers = _info.DirectionPossibleNumbers[TimetableInfo.GetDirection(_selectedDirection[0].ToString())]
					.Select(item => item.ToString("00"))
					.Select(GetGroupSelector).ToList();

			_groups.RemoveAllViews();

			for(var it = 0; it < _numbers.Count; )
			{
				var lay = new LinearLayout(Context);
				lay.SetGravity(GravityFlags.CenterHorizontal);				
				for(var summSumb = 0; summSumb < 19 && it < _numbers.Count; ++it)
				{
					lay.AddView(_numbers[it], new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
					summSumb += _numbers[it].Text.Length;
				}
				_groups.AddView(lay, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
			}

			_groups.Visibility = ViewStates.Visible;
			_directionName.Visibility = ViewStates.Visible;
		}
		private TextView GetGroupSelector(string text)
		{
			var selecter = new TextView(Context)
			{
				Text = text,
				Tag = text,
				Id = text.GetHashCode(),
				TextSize = 18,
				Alpha = 0.25f
			};
			selecter.SetPadding(4, 0, 4, 0);
			selecter.SetTextColor(Color.Black);
			selecter.SetOnClickListener(this);

			var attrs = new[]{ Android.Resource.Attribute.SelectableItemBackground};
			var ta = Activity.ObtainStyledAttributes(attrs);
			var selectedItemDrawable = ta.GetDrawable(0);
			ta.Recycle();
			selecter.SetBackgroundDrawable(selectedItemDrawable);
			return selecter;
		}
		private void UpdateImageOpacity(int id)
		{
			foreach(var image in _images)
				image.Alpha = image.Id == id ? 1f : 0.25f;
		}
		private void OnChoose()
		{
			if(_selectedDirection == Resources.GetString(Resource.String.TxtUnknown))
				_listener.SelectTeamOnChoose(_selectedGroup);
			else
				_listener.SelectTeamOnChoose(TimetableInfo.GetDirection(_selectedDirection[0].ToString()) + _selectedGroup);
			Dismiss();
		}
		private void OnClose()
		{
			Dismiss();
		}
		private int DirectionToId(string dir)
		{
			foreach(var image in _images)
				if((string)image.Tag == dir) return image.Id;
			return -1;
		}
		private int TeamToId(string team)
		{
			if(_numbers == null) return -1;
			foreach(var number in _numbers)
				if((string)number.Tag == team) return number.Id;
			return -1;
		}

		#endregion

		#region Public methods

		public void OnClick(View v)
		{
			var id = v.Id;
			if(id == -1) return;
			switch(id)
			{
				case Resource.Id.btn_select:
					OnChoose();
					break;
				case Resource.Id.btn_close:
					OnClose();
					break;
				case Resource.Id.img_sport:
				case Resource.Id.img_art:
				case Resource.Id.img_literature:
				case Resource.Id.img_science:
				case Resource.Id.img_unknown:
					OnChooseDirection(id);
					break;
				default:
					OnChooseGroup(id);
					break;
			}
		}

		public View MakeView()
		{
			var textView = new TextView(Context);
			textView.Gravity = GravityFlags.CenterHorizontal | GravityFlags.Center;
			textView.SetLines(2);
			return textView;
		}

		#endregion

		#region Interaction interfaces

		public interface ISelectTeamDialogResultListener
		{
			void SelectTeamOnChoose(string result);
		}

		#endregion
	}
}