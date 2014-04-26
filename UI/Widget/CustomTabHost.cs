using System;
using System.Linq;
using Xwt;

namespace Baimp
{
	public class CustomTabHost : HBox
	{
		int selectedIndex = -1;
		Distance lean;


		public CustomTabHost() : base()
		{
			Spacing = 0;
			Lean = new Distance(10, 14);
		}

		/// <summary>
		/// Add the specified button.
		/// </summary>
		/// <param name="button">Button.</param>
		public void Add(TabButton button)
		{
			if (selectedIndex == -1) {
				button.Active = true;
				selectedIndex = 0;
			}

			button.Managed = true;
			button.Multiple = false;
			button.Lean = Lean;
			button.Toggled += OnButtonToggled;
			button.Previous = Children.LastOrDefault() as TabButton;

			PackStart(button);
		}

		/// <summary>
		/// Add a new button with the given name.
		/// </summary>
		/// <param name="name">Name.</param>
		public void Add(string name)
		{
			Add (new TabButton(name));
		}

		/// <summary>
		/// Remove all tabs.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the index of the selected button.
		/// </summary>
		/// <value>The index of the selected.</value>
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

		/// <summary>
		/// Gets or sets the selected item.
		/// </summary>
		/// <value>The selected item.</value>
		public TabButton SelectedItem {
			get {
				return Children.ElementAt(SelectedIndex) as TabButton;
			}
			set {
				SelectedIndex = Children.ToList().IndexOf(value);
			}
		}

		public Distance Lean {
			get {
				return lean;
			}
			set {
				lean = value;
				foreach (TabButton child in Children.OfType<TabButton>()) {
					child.Lean = lean;
				}
			}
		}

		#endregion
	}
}

