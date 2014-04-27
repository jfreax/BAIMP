using System;
using System.Linq;
using System.Collections.Generic;

namespace Baimp
{
	public enum LogLevel {
		Debug = 1,
		Error = 2,
		Verbose = 3,
		Warning = 4,
		Info = 5
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
		public static void Append(LogLevel logLevel, string source, string message)
		{
			LogMessages.Add(new LogMessage(logLevel, source, message));
		}

		/// <summary>
		/// Get all logs with a given log level or higher.
		/// </summary>
		/// <param name="logLevel">Log level.</param>
		public static List<LogMessage> Get(LogLevel logLevel)
		{
			List<LogMessage> output = new List<LogMessage>();
			output.AddRange(LogMessages.Where(m => (int) m.logLevel >= (int) logLevel));

			return output;
		}
	}

	public struct LogMessage
	{
		public LogLevel logLevel;
		public string source;
		public string message;

		public LogMessage(LogLevel logLevel, string source, string message)
		{
			this.logLevel = logLevel;
			this.source = source;
			this.message = message;
		}
	}
}

