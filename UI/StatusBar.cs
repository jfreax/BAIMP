//
//  StatusBar.cs
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
using System.Threading;

namespace Baimp
{
	public class StatusBar : HBox
	{
		readonly Timer timer;
		Label threadLabel = new Label();
		Label logEntry = new Label();
		int maxThreads;

		public StatusBar()
		{
			InitializeUI();

			timer = new Timer(o => UpdateThreadLabel(), null, 1000, 1000);
			Log.LogAdded += ShowLogEntry;

			// Show log window on double click
			logEntry.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				if (e.MultiplePress >= 2) {
					if (Log.Count(LogLevel.Debug) > 0) {

						Dialog d = new Dialog();
						d.Content = new LogViewer(LogLevel.Debug);

						d.Run();
						d.Dispose();
					}
				}
			};

			// add lasz missing log (if any)
			LogMessage last = Log.Get(LogLevel.Debug).LastOrDefault();
			if (!string.IsNullOrEmpty(last.Message)) {
				ShowLogEntry(null, new LogEventArgs(last));
			}
		}

		void InitializeUI()
		{
			PackStart(logEntry, true);
			PackEnd(threadLabel);

			int completionPortThreads;
			ThreadPool.GetMaxThreads(out maxThreads, out completionPortThreads);
		}

		/// <summary>
		/// Updates the label showing number of threads.
		/// </summary>
		void UpdateThreadLabel()
		{
			int workerThreads;
			int completionPortThreads;
			ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);

			Application.Invoke(() => threadLabel.Text = "#Threads: " + (maxThreads - workerThreads));
		}

		/// <summary>
		/// Show given log entry in status bar
		/// </summary>
		/// <param name="sender">Sender, can be null.</param>
		/// <param name="e">Arguments with log message.</param>
		void ShowLogEntry(object sender, LogEventArgs e)
		{
			try {
				logEntry.Markup = 
					string.Format("<b>{0}:</b> <span color='{1}'>{2}</span>", 
					e.LogMessage.Source, Log.LevelToColorString(e.LogMessage.LogLevel), e.LogMessage.Message);
			} catch (Exception exception) {
				logEntry.Text = string.Format("{0}: {1}", e.LogMessage.Source, e.LogMessage.Message);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			timer.Dispose();
		}
	}
}

