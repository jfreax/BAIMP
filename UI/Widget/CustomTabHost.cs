using System;
using System.Linq;
using Xwt;

namespace Baimp
{
	public class CustomTabHost : HBox
	{
		int selectedIndex = -1;
		Distance lean;
		bool closeable;

		public CustomTabHost()
		{
			Spacing = 0;
			Lean = new Distance(10, 14);

			CanCloseAll = false;
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
			button.Closeable = Closeable;

			button.Previous = Children.LastOrDefault() as TabButton;

			button.Toggled += OnButtonToggled;
			button.Closed += OnTabClosed;

			PackStart(button);
		}

		/// <summary>
		/// Add a new button with the given name.
		/// </summary>
		/// <param name="name">Name.</param>
		public void Add(string name)
		{
			Add(new TabButton(name));
		}

		/// <summary>
		/// Remove all tabs.
		/// </summary>
		public new void Clear()
		{
			base.Clear();
			selectedIndex = -1;
		}

		/// <summary>
		/// Raised when one tabs active status toggled.
		/// </summary>
		/// <param name="sender">Tab bar.</param>
		/// <param name="e">Event arguments.</param>
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

		/// <summary>
		/// Raises when one tab was closed.
		/// </summary>
		/// <param name="sender">Tab.</param>
		/// <param name="e">Event arguments.</param>
		void OnTabClosed(object sender, EventArgs e)
		{
			TabButton button = sender as TabButton;

			if (button != null && (CanCloseAll || Children.Count() > 1)) {
				if (SelectedIndex == Children.Count() - 1) {
					SelectedIndex -= 1;
				}

				Remove(button);

				if (tabClosedEvent != null) {
					tabClosedEvent(sender, e);
				}

				button.Dispose();
			}
		}

		#region Custom events

		EventHandler<EventArgs> selectionChangedEvent;

		/// <summary>
		/// Occurs when scan data changed.
		/// </summary>
		public event EventHandler<EventArgs> SelectionChanged {
			add {
				selectionChangedEvent += value;
			}
			remove {
				selectionChangedEvent -= value;
			}
		}

		EventHandler<EventArgs> tabClosedEvent;

		/// <summary>
		/// Occurs when a tab was closed.
		/// </summary>
		public event EventHandler<EventArgs> TabClosed {
			add {
				tabClosedEvent += value;
			}
			remove {
				tabClosedEvent -= value;
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
				if (selectedIndex == value) {
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

		/// <summary>
		/// Gets or sets a value indicating whether the tabs are closeable.
		/// </summary>
		/// <value><c>true</c> if closeable; otherwise, <c>false</c>.</value>
		public bool Closeable {
			get {
				return closeable;
			}
			set {
				closeable = value;
				foreach (TabButton child in Children.OfType<TabButton>()) {
					child.Closeable = closeable;
				}
			}
		}

		/// <summary>
		/// Can all tabs be closed?
		/// </summary>
		/// <value><c>true</c> if all tabs can be closed; otherwise, <c>false</c>.</value>
		public bool CanCloseAll {
			get;
			set;
		}

		#endregion
	}
}

