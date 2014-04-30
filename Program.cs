//
//  Program.cs
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
using Mono.Options;
using Xwt;
using System.Threading;

namespace Baimp
{
	public enum OSType {
		Unix,
		Windows,
		MaxOSX}
;

	class MainClass
	{
		public static ToolkitType toolkitType = ToolkitType.Gtk;

		[STAThread]
		public static void Main(string[] args)
		{
			bool show_help = false;
			string filename = null;
			string path = null;
			string featureExtraction = null;

			// commandline parsing
			var p = new OptionSet() { { "h|?|help", "show help screen",
					v => show_help = v != null
				}, { "f|file=", "project file to open",
					v => filename = v
				}, { "p|path=", "path to folder with scans",
					v => path = v
				}, { "e|extraction=", "select and run feature extraction",
					v => featureExtraction = v
				},
			};

			try {
				p.Parse(args);
			} catch (OptionException e) {
				Console.Out.WriteLine(e.Message);
				printHelp(p);
				return;
			}

			// print help
			if (show_help) {
				printHelp(p);
				return;
			}

			ThreadPool.SetMaxThreads(8, 16);

			// start application
			if (GetOS() == OSType.Unix) {
				toolkitType = ToolkitType.Gtk;
			} else if (GetOS() == OSType.MaxOSX) {
				toolkitType = ToolkitType.Cocoa;
			} else {
				toolkitType = ToolkitType.Gtk;
			}

			Application.Initialize(toolkitType);

			Project project = new Project(filename);
			Window w = new MainWindow(project);

			w.Show();
			Application.Run();

			w.Dispose();
			Application.Dispose();
		}

		/// <summary>
		/// Prints the help.
		/// </summary>
		/// <param name="p">Commandline options</param>
		static void printHelp(OptionSet p)
		{
			Console.Out.WriteLine("Usage: ");
			p.WriteOptionDescriptions(Console.Out);
		}

		static OSType GetOS()
		{
			int p = (int) Environment.OSVersion.Platform;
			if (p == 4 || p == 128) {
				return OSType.Unix;
			} else if (p == 6) {
				return OSType.MaxOSX;
			} else {
				return OSType.Windows;
			}
		}
	}
}
