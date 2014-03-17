using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace bachelorarbeit_implementierung
{
	public enum ScanType
	{
		Intensity = 0,
		Topography = 1,
		Color = 2,
		Metadata = 3 // must be the last one!
	};

	public class Scan
	{
		int width;
		int height;
		float zLengthPerDigitF;
		string fiberType;

		string[] filenames;

		float[][] data;
		float[] max;

		public List<Tuple<string, string>> generalMetadata;

		Xwt.Drawing.Image[] renderedImage;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.Scan"/> class.
		/// </summary>
		/// <param name="filePath">Path to dd+ file.</param>
		public Scan (string filePath)
		{
			filenames = new string[(int)ScanType.Metadata+1];
			filenames [(int)ScanType.Metadata] = filePath;

			// parse dd+ file informations
			var ini = new IniFile(filePath);

			height = ini.ReadInteger("general", "Height", 0);
			width = ini.ReadInteger("general", "Width", 0);
			zLengthPerDigitF = (float) ini.ReadDoubleInvariant("general", "ZLengthPerDigitF", 0.0);
			fiberType = ini.ReadString ("fiber", "FiberType", "Unbekannt");

			generalMetadata = ini.ReadAllStrings ("general");

			// set file pathes
			string path = Path.GetDirectoryName (filePath);
			filenames [(int)ScanType.Intensity] = String.Format ("{0}/{1}", path, ini.ReadString ("buffers", "intensity"));
			filenames [(int)ScanType.Topography] = String.Format ("{0}/{1}", path, ini.ReadString ("buffers", "topography"));
			filenames [(int)ScanType.Color] = String.Format ("{0}/{1}", path, ini.ReadString ("buffers", "color"));

			// initialize empty data containers
			data = new float[(int)ScanType.Metadata][];
			max = new float[(int)ScanType.Metadata];
			renderedImage = new Xwt.Drawing.Image[(int)ScanType.Metadata];
		}


		/// <summary>
		/// Get specified scan as byte buffer.
		/// </summary>
		/// <returns>The byte buffer.</returns>
		/// <param name="type">Type.</param>
		public byte[] GetByteBuffer(ScanType type) {
			Stream s = File.OpenRead (filenames [(int)type]);
			BinaryReader input = new BinaryReader (s);

			int length = input.ReadInt32() * input.ReadInt32();
			return input.ReadBytes(length * 4);
		}


		/// <summary>
		/// Gets scan as array.
		/// </summary>
		/// <returns>The specified scan as a plan float array.</returns>
		/// <param name="type">Type.</param>
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

				if (data [(int)type] [i] > max [(int)type]) {
					max [(int)type] = data [(int)type] [i];
				}
			}
//			Parallel.For(0, length, new Action<int>(i =>
//			{
//				
//
//			}));

			return data[(int)type];
		}


		/// <summary>
		/// Get scan as bitmap.
		/// </summary>
		/// <returns>The specified scan as a bitmap.</returns>
		/// <param name="type">Scantile</param>
		public unsafe Bitmap GetAsBitmap(ScanType type) {
			Bitmap bitmap = null;
			if (type == ScanType.Color) {
				bitmap = new Bitmap (width, height, PixelFormat.Format32bppRgb);
			} else {
				//bitmap = new Bitmap (width, height, PixelFormat.Format16bppRgb555);
                bitmap = new Bitmap (width, height, PixelFormat.Format32bppRgb);
			}

			//Create a BitmapData and Lock all pixels to be written 
			BitmapData bmpData = bitmap.LockBits (
				new Rectangle (0, 0, width, height),   
				ImageLockMode.WriteOnly, bitmap.PixelFormat);

			Stream s = File.OpenRead (filenames [(int)type]);
			BinaryReader input = new BinaryReader (s);
			input.ReadInt64 ();

			if (type == ScanType.Color) {
				byte* scan0 = (byte*)bmpData.Scan0.ToPointer();

				byte[] buffer = input.ReadBytes(height * width * 4);
				for (int i = 0; i < height * width * 4; ++i) {
					*scan0 = buffer[i];
					scan0++;
				}
			} else {
				float[] array = GetAsArray (type);
				float max = this.max [(int)type];
                
				int* scan0 = (int*)bmpData.Scan0.ToPointer();
				for (int i = 0; i < height * width; ++i) {
					int color = (int)((array [i] * 255) / max);
					color |= (color << 24) | (color << 16) | (color << 8);
					*scan0 = color;
					scan0++;
				}
			}

			//Unlock the pixels
			bitmap.UnlockBits (bmpData);

			return bitmap;
		}

		public Xwt.Drawing.Image GetAsImage(ScanType type) {
			if (renderedImage [(int)type] == null) {
				MemoryStream memoryStream = new MemoryStream ();
				System.Drawing.Bitmap bmp = GetAsBitmap (type);
				if (bmp == null) {
					Console.WriteLine ("bmp == null " + (int)type);
					// TODO raise error
					return null;
				}
				bmp.Save (memoryStream, System.Drawing.Imaging.ImageFormat.Tiff);
				memoryStream.Position = 0;

				renderedImage [(int)type] = Xwt.Drawing.Image.FromStream (memoryStream);
			} else {
				double w = renderedImage [(int)type].Width;
				Console.WriteLine (w);
			}

			return renderedImage[(int)type];
		}

		public override string ToString() {
			return Name;
		}

		/////////////////////
		// Getter & Setter //
		/////////////////////

		public string FiberType {
			get { return fiberType; }
		}

		public string Name {
			get {
				return Path.GetFileNameWithoutExtension (filenames [(int)ScanType.Metadata]);
			}
		}
	}
}

