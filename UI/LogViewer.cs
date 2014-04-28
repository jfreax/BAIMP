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

namespace Baimp
{
	public class LogViewer : VBox
	{
		LogLevel logLevel;

		public LogViewer(LogLevel logLevel)
		{
			CurrentLogLevel = logLevel;
		}

		void UpdateView()
		{
			Clear();
			List<LogMessage> messages = Log.Get(CurrentLogLevel);
			foreach(LogMessage message in messages) {
				Label messageLabel = new Label();
				try {
					messageLabel.Markup = 
						string.Format("<b>{0}:</b> <span color='{1}'>{2}</span>", 
							message.source, Log.LevelToColorString(message.logLevel), message.message);
				} catch (Exception exception) {
					messageLabel.Text = string.Format("{0}: {1}", message.source, message.message);
				}

				this.PackStart(messageLabel);
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

