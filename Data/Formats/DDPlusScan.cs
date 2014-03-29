using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using XD = Xwt.Drawing;
using System.Drawing;
using System.Xml.Serialization;
using System.Diagnostics;

namespace baimp
{
	public class DDPlusScan : BaseScan
	{
		List<Metadata> metadata = new List<Metadata>();

		private Dictionary<string, string> filenames = new Dictionary<string, string>();
		private Dictionary<string, float[]> arrayData = new Dictionary<string, float[]>();
		private Dictionary<string, XD.Image> renderedImage = new Dictionary<string, XD.Image>();

		private Dictionary<string, float> max = new Dictionary<string, float>();

		public DDPlusScan()
		{
		}

		/// <summary>
		/// Loading file from filePath and store all information.
		/// </summary>
		/// <param name="filePath">File path.</param>
		/// <param name="newImport">True when data should be read again from original file</param>
		/// <remarks>Gets recalled on filePath change!</remarks>
		public override void Initialize(string filePath, bool newImport = true)
		{
			base.Initialize(filePath, newImport);

			IniFile ini = new IniFile(filePath);

			// set file pathes
			string path = Path.GetDirectoryName(filePath);
			filenames["Intensity"] = String.Format("{0}/{1}", path, ini.ReadString("buffers", "intensity"));
			filenames["Topography"] = String.Format("{0}/{1}", path, ini.ReadString("buffers", "topography"));
			filenames["Color"] = String.Format("{0}/{1}", path, ini.ReadString("buffers", "color"));

			if (newImport) {
				size = new Xwt.Size(ini.ReadInteger("general", "Width", 0), ini.ReadInteger("general", "Height", 0));
				requestedBitmapSize = new Xwt.Size(size.Width, size.Height);

				foreach (Tuple<string, string > datum in ini.ReadAllStrings("general")) {
					metadata.Add(new Metadata(datum.Item1, datum.Item2));
				}
			}
		}

		#region implemented abstract members of BaseScan

		public override string SupportedFileExtensions()
		{
			return "*.dd+";
		}

		public override string[] AvailableScanTypes()
		{
			return new string[] {
				"Intensity",
				"Topography",
				"Color"
			};
		}

		public override byte[] GetByteBuffer(string scanType)
		{
			Stream s = File.OpenRead(filenames[scanType]);
			BinaryReader input = new BinaryReader(s);

			int length = input.ReadInt32() * input.ReadInt32();

			return input.ReadBytes(length * 4);
		}

		public override float[] GetAsArray(string scanType)
		{
			if (arrayData.ContainsKey(scanType) && arrayData[scanType] != null) {
				return arrayData[scanType];
			}

			if (!max.ContainsKey(scanType)) {
				max[scanType] = 0.0f;
			}

			Stream s = File.OpenRead(filenames[scanType]);
			BinaryReader input = new BinaryReader(s);

			Int32 width = input.ReadInt32();
			Int32 height = input.ReadInt32();

			int length = width * height;
			arrayData[scanType] = new float[length];
			byte[] buffer = input.ReadBytes(length * 4);

			Buffer.BlockCopy(buffer, 0, arrayData[scanType], 0, length * 4);
			max[scanType] = arrayData[scanType].Max();

			if (scanType == "Topography") {
				Metadata zLength = metadata.Find( m => m.key == "zLengthPerDigitF" );
				if (zLength != null) {
					float zLengthValue = Convert.ToSingle(zLength.value) / 10000000.0f;
					arrayData[scanType].Select( x => x * zLengthValue);
				}
			}

			return arrayData[scanType];
		}

		public override unsafe System.Drawing.Bitmap GetAsBitmap(string scanType)
		{
			int width = (int) Size.Width;
			int height = (int) Size.Height;

			Bitmap bitmap = null;
			if (scanType == "Color") {
				bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			} else {
				//bitmap = new Bitmap (width, height, PixelFormat.Format16bppRgb555);
				//bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
				bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

				ColorPalette grayscalePalette = bitmap.Palette;
				for(int i = 0; i < 256; i++) {
					grayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
				}
				bitmap.Palette = grayscalePalette;
			}

			//Create a BitmapData and Lock all pixels to be written 
			BitmapData bmpData = bitmap.LockBits(
				new Rectangle(0, 0, width, height),   
				ImageLockMode.WriteOnly, bitmap.PixelFormat);

			Stream s = File.OpenRead(filenames[scanType]);
			BinaryReader input = new BinaryReader(s);
			input.ReadInt64();

			if (scanType == "Color") {
				byte* scan0 = (byte*) bmpData.Scan0.ToPointer();

				int len = width * height * 4;
				byte[] buffer = input.ReadBytes(len);
				for (int i = 0; i < len; ++i) {
					*scan0 = buffer[i];
					scan0++;
				}
			} else {
				float[] array = GetAsArray(scanType);
				//float max = (float) Math.Max(this.max[scanType], 1.0);
				float max = this.max[scanType];

				int len = width * height;
				byte* scan0 = (byte*) bmpData.Scan0.ToPointer();
				for (int i = 0; i < len; ++i) {
					byte color = (byte) ((array[i] * 255.0) / max);
					*scan0 = color;
					scan0++;
				}
			}

			//Unlock the pixels
			bitmap.UnlockBits(bmpData);

			return bitmap;
		}

		public override System.IO.MemoryStream GetAsMemoryStream(string scanType)
		{
			MemoryStream memoryStream = new MemoryStream();
			System.Drawing.Bitmap bmp = GetAsBitmap(scanType);
			if (bmp == null) {
				// TODO raise error
				return null;
			}

			bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			memoryStream.Position = 0;

			return memoryStream;
		}

		public override Xwt.Drawing.Image GetAsImage(string scanType)
		{
			if (!renderedImage.ContainsKey(scanType) || renderedImage[scanType] == null) {
				MemoryStream mStream = GetAsMemoryStream(scanType);
				renderedImage[scanType] = XD.Image.FromStream(mStream).WithSize(requestedBitmapSize);
				mStream.Dispose();
			}

			return renderedImage[scanType].WithSize(requestedBitmapSize);
		}
			
		[XmlArray("metadata")]
		[XmlArrayItem("datum")]
		public override List<Metadata> Metadata {
			get {
				return metadata;
			}
			set {
				metadata = value;
			}
		}

		#endregion
	}
}

