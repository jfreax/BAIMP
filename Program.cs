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
				toolkitType = ToolkitType.Wpf;
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
