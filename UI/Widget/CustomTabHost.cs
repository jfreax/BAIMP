//
//  CustomTabHost.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
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
			button.Closed += OnTabClose;

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

			if (button != null && SelectedItem != button) {
				SelectedItem = button;

				if (selectionChangedEvent != null) {
					selectionChangedEvent(sender, e);
				}
			}
		}

		/// <summary>
		/// Raises when one tab should be closed.
		/// </summary>
		/// <param name="sender">Tab.</param>
		/// <param name="e">Event arguments.</param>
		void OnTabClose(object sender, CloseEventArgs e)
		{
			TabButton button = sender as TabButton;

			if (button != null && (CanCloseAll || Children.Count() > 1)) {
				if (SelectedIndex == Children.Count() - 1) {
					SelectedIndex -= 1;
				}
					
				if (tabClosedEvent != null) {
					tabClosedEvent(sender, e);
				}

				if (e.Close) {
					Remove(button);
					button.Dispose();
				}
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

		EventHandler<CloseEventArgs> tabClosedEvent;

		/// <summary>
		/// Occurs when a tab was closed.
		/// </summary>
		public event EventHandler<CloseEventArgs> TabClose {
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

		/// <summary>
		/// Number of tabs.
		/// </summary>
		/// <value>The count.</value>
		public int Count {
			get {
				return Children.Count();
			}
		}

		#endregion
	}
}

