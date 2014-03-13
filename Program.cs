using System;
using Mono.Options;
using System.Collections.Generic;

namespace bachelorarbeit_implementierung
{
	class MainClass
	{
		public static void Main (string[] args) {

			bool show_help = false;
			string fileName;
			string featureExtraction;

			var p = new OptionSet () { { "h|?|help", "show help screen",
					v => show_help = v != null
				}, { "f|file=", "vk4, dd+ or image file to open",
					v => fileName = v
				}, { "e|extraction=", "select and run feature extraction",
					v => featureExtraction = v
				},
			};

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

			if (show_help) {
				printHelp (p);
				return;
			}
		}

		static void printHelp(OptionSet p)
		{
			Console.Out.WriteLine ("Usage: ");
			p.WriteOptionDescriptions(Console.Out);
		}



	}
}
