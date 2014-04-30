//
//  Log.cs
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
using System.Collections.Generic;

namespace Baimp
{
	public enum LogLevel {
		Debug = 0,
		Error = 1,
		Verbose = 2,
		Warning = 3,
		Info = 4
	}

	public static class Log
	{
		static readonly List<LogMessage> LogMessages = new List<LogMessage>();

		/// <summary>
		/// Add a new log entry.
		/// </summary>
		/// <param name="logLevel">Log level.</param>
		/// <param name="source">Source.</param>
		/// <param name="message">Message.</param>
		public static void Add(LogLevel logLevel, string source, string message)
		{
			LogMessage logMessage = new LogMessage(logLevel, source, message);
			LogMessages.Add(logMessage);

			if (logAdded != null) {
				logAdded(null, new LogEventArgs(logMessage));
			}
		}

		/// <summary>
		/// Get last logs with a given log level or higher.
		/// </summary>
		/// <param name="logLevel">Log level.</param>
		/// <param name="count">Number of logs to retrieve; -1 mean all.</param>
		public static List<LogMessage> Get(LogLevel logLevel, int count = -1)
		{
			List<LogMessage> output = new List<LogMessage>();
			if (count > 0) {
				output.AddRange(
					LogMessages.Where(m => (int) m.LogLevel >= (int) logLevel).Take(count)
				);
			} else {
				output.AddRange(
					LogMessages.Where(m => (int) m.LogLevel >= (int) logLevel)
				);
			}

			return output;
		}

		public static int Count(LogLevel logLevel)
		{
			return LogMessages.Count(m => (int) m.LogLevel >= (int) logLevel);
		}

		#region Events

		static EventHandler<LogEventArgs> logAdded;

		/// <summary>
		/// Occurs when a log entry was added.
		/// </summary>
		public static event EventHandler<LogEventArgs> LogAdded {
			add {
				logAdded += value;
			}
			remove {
				logAdded -= value;
			}
		}

		#endregion

		public static string LevelToColorString(LogLevel level)
		{
			switch (level) {
			case LogLevel.Debug:
				return "#01f6df";
			case LogLevel.Error:
				return "#ff0000";
			case LogLevel.Verbose:
				return "#655fe0";
			case LogLevel.Warning:
				return "#fbfa00";
			case LogLevel.Info:
				return "#000000";
			}

			return "#000000";
		}
	}

	public struct LogMessage
	{
		public readonly LogLevel LogLevel;
		public readonly string Source;
		public readonly string Message;
		public readonly DateTime Timestamp;

		public LogMessage(LogLevel logLevel, string source, string message)
		{
			this.LogLevel = logLevel;
			this.Source = source;
			this.Message = message;

			Timestamp = DateTime.Now;
		}
	}
}

