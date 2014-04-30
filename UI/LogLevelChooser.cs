//
//  LogLevelChooser.cs
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
using System;
using System.Linq;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class LogLevelChooser : Canvas
	{
		LogLevel selectedLogLevel;
		Image[] renderedImage;

		VBox buttons = new VBox();
		Window popupWindow = new Window();

		double windowHeight;

		public LogLevelChooser(LogLevel selectedLogLevel)
		{
			SelectedLogLevel = selectedLogLevel;

			// prerender
			string[] logNames = Enum.GetNames(typeof(LogLevel));
			int length = logNames.Length;
			renderedImage = new Image[length];

			using (TextLayout text = new TextLayout()) {
				for (int i = 0; i < length; i++) {
					text.Text = logNames[i];
					Size size = text.GetSize();
					using (ImageBuilder ib = new ImageBuilder(size.Width + size.Height*2 + 3, size.Height)) {
						Color color = Color.FromName(Log.LevelToColorString((LogLevel) i));

						Draw(ib.Context, (LogLevel) i, color);
						renderedImage[i] = ib.ToBitmap();

						Button button = new Button { Image = renderedImage[i], ImagePosition = ContentPosition.Left };
						button.HorizontalPlacement = WidgetPlacement.Start;
						button.Margin = 0;
						button.ExpandHorizontal = true;
						button.Style = ButtonStyle.Flat;
						buttons.PackStart(button, true, true);

						button.CanGetFocus = false;
						button.Tag = i;
						button.Clicked += OnLogChange;

						windowHeight += size.Height * 2;
					}
				}
			}

			// hide window on lost fokus
			buttons.CanGetFocus = true;
			buttons.LostFocus += delegate {
				if (menuHide != null) {
					menuHide(this, EventArgs.Empty);
				}

				popupWindow.Hide();
			};
			buttons.ButtonPressed += delegate {
				// do nothing
				// workaround to propagate event to each button
			};

			buttons.Spacing = 0;

			popupWindow.Padding = 0;
			popupWindow.ShowInTaskbar = false;
			popupWindow.Decorated = false;
			popupWindow.Content = buttons;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			ctx.DrawImage(renderedImage[(int) SelectedLogLevel], Point.Zero);
		}

		static void Draw(Context ctx, LogLevel level, Color color)
		{
			using (TextLayout text = new TextLayout()) {
				text.Text = level.ToString();

				double height = text.GetSize().Height;

				ctx.SetColor(color);
				ctx.RoundRectangle(0, 1, height - 2, height - 2, 3);
				ctx.Fill();

				// inner shadow
				ctx.RoundRectangle(0, 1, height - 2, height - 2, 3);
				LinearGradient g = new LinearGradient(1, 2, height - 2, height - 2);
				g.AddColorStop(0, Colors.Black.BlendWith(color, 0.7));
				g.AddColorStop(1, color);
				ctx.Pattern = g;
				ctx.Fill();

				ctx.SetColor(Colors.Black);
				ctx.DrawTextLayout(text, new Point(height + 3, 0));
			}
		}

		void OnLogChange(object sender, EventArgs e)
		{
			Button b = sender as Button;
			if (b != null) {
				SelectedLogLevel = (LogLevel) b.Tag;

				if (logLevelChanged != null) {
					logLevelChanged(sender, EventArgs.Empty);
				}
				popupWindow.Hide();
			}
		}

		protected override Size OnGetPreferredSize(SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return renderedImage[(int) SelectedLogLevel].Size;
		}

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			if (menuShow != null) {
				menuShow(this, EventArgs.Empty);
			}
			popupWindow.Location = ScreenBounds.Location.Offset(0, -windowHeight);
			popupWindow.Show();
			buttons.SetFocus();
		}

		#region Properties

		public LogLevel SelectedLogLevel {
			get {
				return selectedLogLevel;
			}
			set {
				selectedLogLevel = value;

				QueueForReallocate();
				QueueDraw();
			}
		}

		#endregion

		#region Events

		EventHandler<EventArgs> logLevelChanged;

		/// <summary>
		/// Occurs when the log level was changed.
		/// </summary>
		public event EventHandler<EventArgs> LogLevelChanged {
			add {
				logLevelChanged += value;
			}
			remove {
				logLevelChanged -= value;
			}
		}

		EventHandler<EventArgs> menuShow;

		/// <summary>
		/// Occurs when the menu gets shown.
		/// </summary>
		public event EventHandler<EventArgs> MenuShow {
			add {
				menuShow += value;
			}
			remove {
				menuShow -= value;
			}
		}

		EventHandler<EventArgs> menuHide;

		/// <summary>
		/// Occurs when the menu gets hidden.
		/// </summary>
		public event EventHandler<EventArgs> MenuHide {
			add {
				menuHide += value;
			}
			remove {
				menuHide -= value;
			}
		}

		#endregion
	}
}

