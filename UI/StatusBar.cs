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
using System.Collections.Generic;

namespace Baimp
{
	public class StatusBar : HBox
	{
		readonly Timer timer;
		readonly Label logEntry = new Label();
		Label threadLabel = new Label();

		FrameBox logFrame = new FrameBox();
		ScrollView logScroller = new ScrollView();
		HBox logBox;

		LogLevel currentLogLevel = LogLevel.Debug;

		int maxThreads;

		public StatusBar()
		{
			CanGetFocus = true;
			InitializeUI();

			timer = new Timer(o => UpdateThreadLabel(), null, 1000, 1000);
			Log.LogAdded += ShowLogEntry;

			// Show log window on double click
			logScroller.Content = new LogViewer(LogLevel.Debug);;
			logEntry.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				if (e.MultiplePress >= 2) {
					if (Log.Count(LogLevel.Debug) > 0) {
						this.SetFocus();
						logFrame.Content = logScroller;

						MinHeight = LogViewHeight();
					}
				}
			};

			logScroller.Content.BoundsChanged += delegate {
				this.MinHeight = LogViewHeight();
			};

			// add last missing log (if any)
			LogMessage last = Log.Get(LogLevel.Debug, 1).LastOrDefault();
			if (!string.IsNullOrEmpty(last.Message)) {
				ShowLogEntry(null, new LogEventArgs(last));
			}
		}

		void InitializeUI()
		{
			logBox = new HBox();
			LogLevelChooser logLevelChooser = new LogLevelChooser(currentLogLevel);
			logLevelChooser.MarginRight = 10;
			logLevelChooser.LogLevelChanged += delegate {
				CurrentLogLevel = logLevelChooser.SelectedLogLevel;
			};

			logBox.PackStart(logEntry, true);
			logBox.PackEnd(logLevelChooser);
			logFrame.Content = logBox;
			PackStart(logFrame, true, true);
			PackEnd(threadLabel, vpos: WidgetPlacement.End);

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
			if (e.LogMessage.LogLevel >= CurrentLogLevel) {
				try {
					logEntry.Markup = 
					string.Format("<b>{0}:</b> <span color='{1}'>{2}</span>", 
						e.LogMessage.Source, Log.LevelToColorString(e.LogMessage.LogLevel), e.LogMessage.Message);
				} catch (Exception exception) {
					logEntry.Text = string.Format("{0}: {1}", e.LogMessage.Source, e.LogMessage.Message);
				}
			}
		}

		double LogViewHeight()
		{
			return Math.Min(logScroller.Content.Size.Height, ParentWindow.Height * 0.3);
		}

		protected override void OnLostFocus(EventArgs args)
		{
			base.OnLostFocus(args);
			ScrollView scroller = logFrame.Content as ScrollView;
			if (scroller != null) {
				logFrame.Content = logBox;
				MinHeight = -1;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			timer.Dispose();
		}

		public LogLevel CurrentLogLevel {
			get {
				return currentLogLevel;
			}
			set {
				currentLogLevel = value;

				List<LogMessage> logs = Log.Get(currentLogLevel, 1);
				if (logs != null && logs.Count > 0) {
					ShowLogEntry(this, new LogEventArgs(logs.Last()));
				} else {
					logEntry.Text = logEntry.Markup = "";
				}
			}
		}
	}
}

