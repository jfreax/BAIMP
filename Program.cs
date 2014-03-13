using System;
using System.IO;
using Mono.Options;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace bachelorarbeit_implementierung
{
	class MainClass
	{
		public static void Main (string[] args) {

			bool show_help = false;
			string filename = null;
			string featureExtraction;

			// commandline parsing
			var p = new OptionSet () { { "h|?|help", "show help screen",
					v => show_help = v != null
				}, { "f|file=", "vk4, dd+ or image file to open",
					v => filename = v
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

			if (show_help) {
				printHelp (p);
				return;
			}

			if (!string.IsNullOrEmpty(filename)) {
				var MyIni = new IniFile(filename);

				int height = Convert.ToInt32(MyIni.ReadString("general", "Height"));
				int width = Convert.ToInt32 (MyIni.ReadString("general", "Width"));
				string intensity = MyIni.ReadString("buffers", "intensity");

				Stream s = File.OpenRead(
					String.Format ("{0}/{1}", Path.GetDirectoryName (filename), intensity)
				);

				BinaryReader input = new BinaryReader (s);

				Int32 sizeX = input.ReadInt32();
				Int32 sizeY = input.ReadInt32();

				Console.Out.WriteLine("Size: " + sizeX + "x" + sizeY);

				int length = width * height;
				float[] array = new float[length];

				byte[] buffer = input.ReadBytes(length * 4);
				int offset = 0;
				float max = 0f;
				for (int i = 0; i < length; i++) {
					array[i] = BitConverter.ToSingle(buffer, offset);
					offset += 4;

					if (array [i] > max) {
						max = array [i];
					}
				}

				for (int i = 0; i < length; i++) {
					array [i] = (array [i]*255) / max;
				}

				Bitmap bitmap = new Bitmap (width, height);

				for (int x = 0; x < width; x++) {
					for (int y = 0; y < height; y++) {
						bitmap.SetPixel(x, y, Color.FromArgb(255, (int)array[y*width+x], (int)array[y*width+x], (int)array[y*width+x]));
					}
				}

				//Create a BitmapData and Lock all pixels to be written 
				//BitmapData bmpData = bitmap.LockBits(
				//	new Rectangle(0, 0, width, height),   
				//	ImageLockMode.WriteOnly, bitmap.PixelFormat);

				//Copy the data from the byte array into BitmapData.Scan0
				//Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);

				//Unlock the pixels
				//bitmap.UnlockBits(bmpData);

				bitmap.Save ("bla.png");

				Console.Out.WriteLine ("Width2: " + buffer.Length);
			}
		}

		static void printHelp(OptionSet p)
		{
			Console.Out.WriteLine ("Usage: ");
			p.WriteOptionDescriptions(Console.Out);
		}



	}
}
