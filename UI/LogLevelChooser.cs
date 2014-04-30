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
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class LogLevelChooser : Canvas
	{
		readonly TextLayout text = new TextLayout();
		LogLevel selectedLogLevel;
		Color color;

		public LogLevelChooser(LogLevel selectedLogLevel)
		{
			SelectedLogLevel = selectedLogLevel;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			double height = text.GetSize().Height;
			ctx.RoundRectangle(0, 1, height - 2, height - 2, 3);

			ctx.SetColor(color);
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

		protected override Size OnGetPreferredSize(SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			Size size = text.GetSize();
			size.Width += size.Height + 3;
			return size;
		}

		public LogLevel SelectedLogLevel {
			get {
				return selectedLogLevel;
			}
			set {
				selectedLogLevel = value;
				text.Text = selectedLogLevel.ToString();
				color = Color.FromName(Log.LevelToColorString(SelectedLogLevel));

				QueueForReallocate();
				QueueDraw();
			}
		}
	}
}

