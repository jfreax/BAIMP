//
//  LogViewer.cs
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
using System.Collections.Generic;
using Xwt.Drawing;

namespace Baimp
{
	public class LogViewer : Table
	{
		LogLevel logLevel;

		public LogViewer(LogLevel logLevel)
		{
			CurrentLogLevel = logLevel;

			DefaultColumnSpacing = 0;
			DefaultRowSpacing = 0;

			Log.LogAdded += (object sender, LogEventArgs e) => UpdateView();
		}

		void UpdateView()
		{
			Clear();
			List<LogMessage> messages = Log.Get(CurrentLogLevel);
			messages.Reverse();

			int i = 0;
			foreach(LogMessage message in messages) {
				Label source = new Label(" " + message.Source);
				try {
					source.Markup = string.Format("<b>{0}</b>", source.Text);
					Add(source, 0, i);
				} catch (Exception e) {} // workaround

				Label messageLabel = new Label(" " + message.Message + " ");
				try {
					messageLabel.Markup = 
						string.Format(
							"<span color='{0}'>{1}</span>", Log.LevelToColorString(message.LogLevel), messageLabel.Text
						);
				} catch (Exception e) {} // workaround
				Add(messageLabel, 1, i, hexpand: true);

				Label timestamp = new Label(
					string.Format(
						"{0} {1} ", message.Timestamp.ToShortDateString(), message.Timestamp.ToLongTimeString())
				);
				try {
					timestamp.Markup = string.Format("<i>{0}</i>", timestamp.Text);
				} catch (Exception e) {} // workaround
				Add(timestamp, 2, i, hpos: WidgetPlacement.End);

				// color
				if (i % 2 == 0) {
					source.BackgroundColor = Colors.AliceBlue;
					messageLabel.BackgroundColor = Colors.AliceBlue;
					timestamp.BackgroundColor = Colors.AliceBlue;
				}

				Add(new Label() { MarginTop = 8 }, 3, i);

				i++;
			}
		}

		public LogLevel CurrentLogLevel {
			get {
				return logLevel;
			}
			set {
				logLevel = value;
				UpdateView();
			}
		}
	}
}

