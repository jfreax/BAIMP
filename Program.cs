using System;
using System.Linq;
using System.IO;
using Mono.Options;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xwt;

namespace bachelorarbeit_implementierung
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args) {

			bool show_help = false;
			string filename = null;
			string path = null;
			string featureExtraction = null;

			// commandline parsing
			var p = new OptionSet () { { "h|?|help", "show help screen",
					v => show_help = v != null
				}, { "f|file=", "vk4, dd+ or image file to open",
					v => filename = v
				}, { "p|path=", "path to folder with scans",
					v => path = v
				}, { "e|extraction=", "select and run feature extraction",
					v => featureExtraction = v
				},
			};

			// print help if not arguments are given
			if (args.Length == 0) {
				printHelp (p);
			}

			try
			{
				p.Parse(args);
			}
			catch (OptionException e)
			{
				Console.Out.WriteLine (e.Message);
				printHelp (p);
				return;
			}

			// print help
			if (show_help) {
				printHelp (p);
				return;
			}

			if (!string.IsNullOrEmpty(path)) {
				Console.Out.WriteLine ("Path: " + path);

				// start application
				Application.Initialize (ToolkitType.Wpf);

				MainWindow w = new MainWindow (path);
				w.Show ();
				Application.Run ();

				w.Dispose ();
				Application.Dispose ();

			}
		}

		/// <summary>
		/// Prints the help.
		/// </summary>
		/// <param name="p">Commandline options</param>
		static void printHelp(OptionSet p)
		{
			Console.Out.WriteLine ("Usage: ");
			p.WriteOptionDescriptions(Console.Out);
		}



	}
}
