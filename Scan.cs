using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace bachelorarbeit_implementierung
{
	public enum ScanType
	{
		Intensity = 0,
		Topography = 1,
		Color = 2,
		None = 3
	};

	public class Scan
	{
		int width;
		int height;
		float zLengthPerDigitF;

		string[] filenames;

		float[][] data;

		public Scan (string filename)
		{
			filenames = new string[(int)ScanType.None+1];
			filenames [(int)ScanType.None] = filename;

			// parse dd+ file informations
			var ini = new IniFile(filename);

			height = ini.ReadInteger("general", "Height", 0);
			width = ini.ReadInteger("general", "Width", 0);
			zLengthPerDigitF = (float) ini.ReadDoubleInvariant("general", "ZLengthPerDigitF", 0.0);

			// set file pathes
			string path = Path.GetDirectoryName (filename);
			filenames [(int)ScanType.Intensity] = String.Format ("{0}/{1}", path, ini.ReadString ("buffers", "intensity"));
			filenames [(int)ScanType.Topography] = String.Format ("{0}/{1}", path, ini.ReadString ("buffers", "topography"));
			filenames [(int)ScanType.Color] = String.Format ("{0}/{1}", path, ini.ReadString ("buffers", "color"));

			// initialize empty data containers
			data = new float[(int)ScanType.None+1][];
		}

		public byte[] GetByteBuffer(ScanType type) {
			Stream s = File.OpenRead (filenames [(int)type]);
			BinaryReader input = new BinaryReader (s);

			int length = input.ReadInt32() * input.ReadInt32();
			return input.ReadBytes(length * 4);
		}

		public float[] GetAsArray(ScanType type) {
			if (data [(int)type] != null) {
				return data[(int)type];
			}

			Stream s = File.OpenRead (filenames [(int)type]);
			BinaryReader input = new BinaryReader (s);

			Int32 width = input.ReadInt32();
			Int32 height = input.ReadInt32();

			int length = width * height;
			data[(int)type] = new float[length];

			byte[] buffer = input.ReadBytes(length * 4);
			int offset = 0;
			for (int i = 0; i < length; i++) {
				data[(int)type][i] = BitConverter.ToSingle(buffer, offset);
				offset += 4;

				if (type == ScanType.Topography) {
					data [(int)type] [i] *= zLengthPerDigitF / 10000000.0f;
				}
			}

			return data[(int)type];
		}

		public Bitmap GetAsBitmap(ScanType type) {
			Bitmap bitmap = new Bitmap (width, height, PixelFormat.Format32bppRgb);
			float[] array = GetAsArray (type);

			if (type == ScanType.Color) {
				//Create a BitmapData and Lock all pixels to be written 
				BitmapData bmpData = bitmap.LockBits (
					                     new Rectangle (0, 0, width, height),   
					                     ImageLockMode.WriteOnly, bitmap.PixelFormat);

				//Copy the data from the byte array into BitmapData.Scan0
				byte[] buffer = GetByteBuffer (type);
				Marshal.Copy (buffer, 0, bmpData.Scan0, buffer.Length);

				//Unlock the pixels
				bitmap.UnlockBits (bmpData);

			} else {
				float max = array.Max ();
				for (int i = 0; i < width * height; i++) {
					array [i] = (array [i] * 255) / max;
				}

				for (int x = 0; x < width; x++) {
					for (int y = 0; y < height; y++) {
						int color = (int)array [y * width + x];
						bitmap.SetPixel (x, y, Color.FromArgb (255, color, color, color));
					}
				}
			}

			return bitmap;
		}
	}
}

