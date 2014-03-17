using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using XD = Xwt.Drawing;
using System.Threading;

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
		public delegate void ImageLoadedCallback (XD.Image image);
		private object lock_image_loading = new object ();

		float zLengthPerDigitF;
		string fiberType;
		Xwt.Size size;
		Xwt.Size requestedBitmapSize;

		string[] filenames;
		float[][] data;
		float[] max;
		public List<Tuple<string, string>> generalMetadata;

		XD.Image[] renderedImage;
		XD.ImageBuilder[] maskBuilder;

		IniFile ini;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.Scan"/> class.
		/// </summary>
		/// <param name="filePath">Path to dd+ file.</param>
		public Scan (string filePath)
		{
			filenames = new string[(int)ScanType.Metadata + 1];
			filenames [(int)ScanType.Metadata] = filePath;

			// parse dd+ file informations
			ini = new IniFile (filePath);

			int height = ini.ReadInteger ("general", "Height", 0);
			int width = ini.ReadInteger ("general", "Width", 0);
			zLengthPerDigitF = (float)ini.ReadDoubleInvariant ("general", "ZLengthPerDigitF", 0.0);
			fiberType = ini.ReadString ("fiber", "FiberType", "Unbekannt");

			size = new Xwt.Size (width, height);
			requestedBitmapSize = new Xwt.Size (width, height);

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
			maskBuilder = new XD.ImageBuilder [(int)ScanType.Metadata];
		}

		#region loading

		/// <summary>
		/// Get specified scan as byte buffer.
		/// </summary>
		/// <returns>The byte buffer.</returns>
		/// <param name="type">Type.</param>
		public byte[] GetByteBuffer (ScanType type)
		{
			Stream s = File.OpenRead (filenames [(int)type]);
			BinaryReader input = new BinaryReader (s);

			int length = input.ReadInt32 () * input.ReadInt32 ();
			return input.ReadBytes (length * 4);
		}
			

		/// <summary>
		/// Gets scan as array.
		/// </summary>
		/// <returns>The specified scan as a plan float array.</returns>
		/// <param name="type">Type.</param>
		public float[] GetAsArray (ScanType type)
		{
			if (data [(int)type] != null) {
				return data [(int)type];
			}

			Stream s = File.OpenRead (filenames [(int)type]);
			BinaryReader input = new BinaryReader (s);

			Int32 width = input.ReadInt32 ();
			Int32 height = input.ReadInt32 ();

			int length = width * height;
			data [(int)type] = new float[length];

			byte[] buffer = input.ReadBytes (length * 4);
			int offset = 0;
			for (int i = 0; i < length; i++) {
				data [(int)type] [i] = BitConverter.ToSingle (buffer, offset);
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

			return data [(int)type];
		}

		/// <summary>
		/// Get scan as bitmap.
		/// </summary>
		/// <returns>The specified scan as a bitmap.</returns>
		/// <param name="type">Scantile</param>
		public unsafe Bitmap GetAsBitmap (ScanType type)
		{
			int width = (int)Size.Width;
			int height = (int)Size.Height;

			Bitmap bitmap = null;
			if (type == ScanType.Color) {
				bitmap = new Bitmap (width, height, PixelFormat.Format32bppArgb);
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
				byte* scan0 = (byte*)bmpData.Scan0.ToPointer ();

				int len = width * height * 4;
				byte[] buffer = input.ReadBytes (len);
				for (int i = 0; i < len; ++i) {
					*scan0 = buffer [i];
					scan0++;
				}
			} else {
				float[] array = GetAsArray (type);
				float max = this.max [(int)type];
                
				int len = width * height;
				int* scan0 = (int*)bmpData.Scan0.ToPointer ();
				for (int i = 0; i < len; ++i) {
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

		/// <summary>
		/// Gets image as memory stream in tiff format
		/// </summary>
		/// <returns>The memory stream.</returns>
		/// <param name="type">Scan type.</param>
		public MemoryStream GetAsMemoryStream (ScanType type)
		{
			MemoryStream memoryStream = new MemoryStream ();
			System.Drawing.Bitmap bmp = GetAsBitmap (type);
			if (bmp == null) {
				// TODO raise error
				return null;
			}

			bmp.Save (memoryStream, System.Drawing.Imaging.ImageFormat.Tiff);
			memoryStream.Position = 0;

			return memoryStream;
		}

		/// <summary>
		/// Get as image.
		/// </summary>
		/// <returns>The as image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image GetAsImage (ScanType type)
		{
			if (renderedImage [(int)type] == null) {
				MemoryStream memoryStream = new MemoryStream ();
				System.Drawing.Bitmap bmp = GetAsBitmap (type);
				if (bmp == null) {
					// TODO raise error
					return null;
				}
				bmp.Save (memoryStream, System.Drawing.Imaging.ImageFormat.Tiff);
				memoryStream.Position = 0;

				renderedImage [(int)type] = XD.Image.FromStream (memoryStream).WithSize (requestedBitmapSize);
			}
				
			return renderedImage [(int)type].WithSize (requestedBitmapSize);
		}

		/// <summary>
		/// Gets as image (sync).
		/// </summary>
		/// <param name="type">Scan type.</param>
		/// <param name="callback">Function to call on finish.</param>
		public void GetAsImageAsync (ScanType type, ImageLoadedCallback callback)
		{
			Thread imageLoaderThread = new Thread (delegate() {

				MemoryStream i = null;
				if (renderedImage [(int)type] == null) {
					lock (lock_image_loading) {
						i = GetAsMemoryStream (type);
					}
				}

				Xwt.Application.Invoke (delegate() {
					if (renderedImage [(int)type] == null) {
						renderedImage [(int)type] = XD.Image.FromStream (i).WithSize (requestedBitmapSize);
					}
					callback (renderedImage [(int)type].WithSize (requestedBitmapSize));
				});
			});
			imageLoaderThread.Start ();
		}

		/// <summary>
		/// Gets the mask builder to draw on it.
		/// </summary>
		/// <returns>The mask builder.</returns>
		/// <param name="type">Type.</param>
		public XD.ImageBuilder GetMaskBuilder (ScanType type)
		{
			if (maskBuilder [(int)type] == null) {
				maskBuilder [(int)type] = new XD.ImageBuilder (Size.Width, Size.Height);

				XD.Image mask = LoadMask (type);
				if (mask != null) {
					maskBuilder [(int)type].Context.DrawImage (mask.WithSize (Size), Xwt.Point.Zero);
				}
			}
			return maskBuilder [(int)type];
		}

		/// <summary>
		/// Gets the mask as image.
		/// </summary>
		/// <returns>The mask as image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image GetMaskAsImage (ScanType type)
		{
			return GetMaskBuilder (type).ToVectorImage ().WithSize (requestedBitmapSize);
		}


		/// <summary>
		/// Loads mask data
		/// </summary>
		/// <returns>The mask as an image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image LoadMask(ScanType type) {
			string base64mask = null;

			switch(type) {
			case ScanType.Intensity:
				base64mask = ini.ReadString ("masks", "intensity");
				break;
			case ScanType.Topography:
				base64mask = ini.ReadString ("masks", "topography");
				break;
			case ScanType.Color:
				base64mask = ini.ReadString ("masks", "color");
				break;
			}

			if(String.IsNullOrEmpty(base64mask)) {
				return null;
			} else {
				using (MemoryStream stream = new MemoryStream(
					Convert.FromBase64String(base64mask))) 
				{
					return XD.Image.FromStream (stream);
				}
			}
		}

		#endregion

		#region saving

		/// <summary>
		/// Saves the mask data to the dd+ file.
		/// </summary>
		/// <param name="type">Scan type.</param>
		public void SaveMask(ScanType type)
		{
			XD.BitmapImage mask = GetMaskBuilder (type).ToBitmap ();

			Parallel.For(0, (int)mask.Height, new Action<int>(y => {
				//for (int y = 0; y < mask.Height; y++) {
				for (int x = 0; x < mask.Width; x++) {
					XD.Color color = mask.GetPixel (x, y);
					if (color == ScanView.maskColor) {
						//mask.SetPixel (x, y, XD.Colors.Black);
					} else {
						mask.SetPixel (x, y, XD.Colors.Transparent);
					}
				}
			}));
				//}

			using (MemoryStream stream = new MemoryStream())
			{
				mask.Save(stream, XD.ImageFileType.Png);

				string base64 = Convert.ToBase64String (stream.ToArray ());
				switch(type) {
				case ScanType.Intensity:
					ini.WriteString ("masks", "intensity", base64);
					break;
				case ScanType.Topography:
					ini.WriteString ("masks", "topography", base64);
					break;
				case ScanType.Color:
					ini.WriteString ("masks", "color", base64);
					break;
				}
				ini.UpdateFile ();
			}
		}

		#endregion

		/// <summary>
		/// Scales the render size of all images
		/// </summary>
		/// <param name="scaleFactor">Scale factor.</param>
		/// <remarks>
		/// Call GetAsImage again, to get image with correct size
		/// </remarks>
		public void ScaleImage (double scaleFactor)
		{
			requestedBitmapSize.Height *= scaleFactor;
			requestedBitmapSize.Width *= scaleFactor;
		}


		public override string ToString ()
		{
			return Name;
		}

		#region getter/setter

		public string FiberType {
			get { return fiberType; }
		}

		public string Name {
			get {
				return Path.GetFileNameWithoutExtension (filenames [(int)ScanType.Metadata]);
			}
		}

		public Xwt.Size Size {
			get { return size; }
		}

		public Xwt.Point GetScaleFactor ()
		{
			return new Xwt.Point (
				Size.Width / RequestedBitmapSize.Width,
				Size.Height / RequestedBitmapSize.Height
			);
		}

		#endregion

		public Xwt.Size RequestedBitmapSize {
			get { return requestedBitmapSize; }
			set { requestedBitmapSize = value; }
		}

		/// <summary>
		/// Determines whether this scan is scaled.
		/// </summary>
		/// <returns><c>true</c> if this instance is scaled; otherwise, <c>false</c>.</returns>
		public bool IsScaled ()
		{
			if (requestedBitmapSize.Height != size.Height ||
			    requestedBitmapSize.Width != size.Width) {
				return true;
			} else {
				return false;
			}
		}
	}
}

