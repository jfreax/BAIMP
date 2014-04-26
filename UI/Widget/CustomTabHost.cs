using System;
using System.Linq;
using Xwt;

namespace Baimp
{
	public class CustomTabHost : HBox
	{
		public CustomTabHost() : base()
		{
			Spacing = 0;
		}

		public void Add(TabButton button)
		{
			if (selectedIndex == -1) {
				button.Active = true;
				selectedIndex = 0;
			}

			button.Managed = true;
			button.Multiple = false;
			button.Toggled += OnButtonToggled;
			button.Previous = Children.LastOrDefault() as TabButton;

			PackStart(button);
		}

		public void Add(string name)
		{
			Add (new TabButton(name));
		}

		public new void Clear()
		{
			base.Clear();
			selectedIndex = -1;
		}

		void OnButtonToggled(object sender, EventArgs e)
		{
			TabButton button = sender as TabButton;

			if (button != null) {
				SelectedItem = button;

				if (selectionChangedEvent != null) {
					selectionChangedEvent(sender, e);
				}
			}
		}

		#region Custom events

		EventHandler<EventArgs> selectionChangedEvent;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<EventArgs> SelectionChanged {
			add {
				selectionChangedEvent += value;
			}
			remove {
				selectionChangedEvent -= value;
			}
		}

		#endregion

		#region Properties

		int selectedIndex = -1;
		public int SelectedIndex {
			get {
				return selectedIndex;
			}
			set {
				if ( selectedIndex == value) {
					return;
				}

				TabButton oldButton = Children.ElementAt(selectedIndex) as TabButton;
				if (oldButton != null) {
					oldButton.Active = false;
				}
				selectedIndex = value;

				TabButton newButton = Children.ElementAt(selectedIndex) as TabButton;
				if (newButton != null) {
					newButton.Active = true;
				}
			}
		}

		public TabButton SelectedItem {
			get {
				return Children.ElementAt(SelectedIndex) as TabButton;
			}
			set {
				SelectedIndex = Children.ToList().IndexOf(value);
			}
		}

		#endregion
	}
}

