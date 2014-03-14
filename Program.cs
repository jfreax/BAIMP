using System;
using System.Linq;
using System.IO;
using Mono.Options;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace bachelorarbeit_implementierung
{
	class MainClass
	{
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

			// start application
			MainWindow main = new MainWindow ();
			Application.Run (main);


			if (!string.IsNullOrEmpty(path)) {
				ScanCollection scans = new ScanCollection (path);
				//Scan scan = new Scan (filename);

				//Bitmap bitmap = scan.GetAsBitmap (ScanType.Intensity);
				//bitmap.Save ("intensity.png");

				//Application.Run (new MainWindow (bitmap));


				//scan.GetAsBitmap (ScanType.Topography).Save("topography.png");
				//scan.GetAsBitmap (ScanType.Color).Save("color.png");
			}
		}

		static void printHelp(OptionSet p)
		{
			Console.Out.WriteLine ("Usage: ");
			p.WriteOptionDescriptions(Console.Out);
		}



	}
}
