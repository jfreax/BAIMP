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

namespace baimp
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

		private float zLengthPerDigitF;
		private string fiberType;
		private Xwt.Size size;
		private Xwt.Size requestedBitmapSize;

		private string[] filenames;
		private float[][] data;
		private float[] max;

		public List<Tuple<string, string>> generalMetadata;

		private XD.Image[] renderedImage;
		private XD.ImageBuilder[] maskBuilder;

		/// <summary>
		/// List of unsaved elements
		/// </summary>
		private HashSet<string> unsaved = new HashSet<string>();

		private IniFile ini;

		#region Initialize

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
			fiberType = ini.ReadString ("settings", "FiberType", "Unknown");

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

		#endregion

		#region Loading

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
		/// <remarks>
		/// Set the status of this scan to "unsaved"
		/// </remarks>
		public XD.ImageBuilder GetMaskBuilder (ScanType type)
		{
			if (maskBuilder [(int)type] == null) {
				maskBuilder [(int)type] = new XD.ImageBuilder (Size.Width, Size.Height);

				XD.Image mask = LoadMask (type);
				if (mask != null) {
					maskBuilder [(int)type].Context.DrawImage (mask.WithSize (Size), Xwt.Point.Zero, 0.6);
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

		#region Saving

		/// <summary>
		/// Save all unsaved attributes
		/// </summary>
		public void Save() {
			HashSet<string> unsavedCopy = new HashSet<string> (unsaved);
			foreach (string us in unsavedCopy) {
				switch (us) {
				case "mask_0":
					SaveMask ((ScanType)0);
					break;
				case "mask_1":
					SaveMask ((ScanType)1);
					break;
				case "mask_2":
					SaveMask ((ScanType)2);
					break;
				case "mask_3":
					SaveMask ((ScanType)3);
					break;
				case "FiberType":
					ini.WriteString ("settings", "FiberType", FiberType);
					NotifySaved ("FiberType");
					break;
				}
			}

			ini.UpdateFile ();
		}

		/// <summary>
		/// Saves the mask data to the dd+ file.
		/// </summary>
		/// <param name="type">Scan type.</param>
		public unsafe void SaveMask(ScanType type)
		{
            string base64 = "";
            XD.BitmapImage mask = GetMaskBuilder(type).ToBitmap();
            XD.Color maskColor = ScanView.maskColor.WithAlpha(1.0);

			if (MainClass.toolkitType == Xwt.ToolkitType.Gtk) {
                Parallel.For(0, (int)mask.Height, new Action<int>(y =>
                {
                    for (int x = 0; x < mask.Width; x++) {
                        XD.Color color = mask.GetPixel(x, y);
                        if (color.WithAlpha(1.0) == maskColor) {
                            mask.SetPixel(x, y, color.WithAlpha(0.6));
						} else {
                            mask.SetPixel(x, y, XD.Colors.Transparent);
                        }
                    }
                }));

                using (MemoryStream stream = new MemoryStream()) {
                    mask.Save(stream, XD.ImageFileType.Png);
                    base64 = Convert.ToBase64String(stream.ToArray());
                }
            } else {
                using (MemoryStream ms = new MemoryStream()) {
                    mask.Save(ms, XD.ImageFileType.Png);
                    ms.Seek(0, SeekOrigin.Begin);

                    Bitmap maskBitmap = new Bitmap(ms);
                        
                    BitmapData bmpData = maskBitmap.LockBits(
                        new Rectangle(0, 0, (int)size.Width, (int)size.Height),
                        ImageLockMode.ReadWrite, maskBitmap.PixelFormat);

                    byte* scan0 = (byte*)bmpData.Scan0.ToPointer();
                    int len = (int)size.Width * (int)size.Height;
                    for (int i = 0; i < len; ++i) {
                        byte b = *scan0;
                        scan0++;
                        byte g = *scan0;
                        scan0++;
                        byte r = *scan0;
                        scan0++;
                        XD.Color color = XD.Color.FromBytes(r, g, b);

                        if ((int)(color.Red*10) == (int)(maskColor.Red*10) &&
                            (int)(color.Green*10) == (int)(maskColor.Green*10) &&
                            (int)(color.Blue*10) == (int)(maskColor.Blue*10))
                        {
                            *scan0 = 153; // 60% alpha
                        } else {
                            *(scan0-3) = 0;
                            *(scan0-2) = 0;
                            *(scan0-1) = 0;
                            *(scan0) = 0;
                        }

                        scan0++;
                    }

                    maskBitmap.UnlockBits(bmpData);

                    using (MemoryStream stream = new MemoryStream()) {
                        maskBitmap.Save(stream, ImageFormat.Png);
                        base64 = Convert.ToBase64String(stream.ToArray());
                    }

                    maskBitmap.Dispose();

                }
            }

            switch (type)  {
			case ScanType.Intensity:
				ini.WriteString("masks", "intensity", base64);
                break;
            case ScanType.Topography:
                ini.WriteString("masks", "topography", base64);
                break;
            case ScanType.Color:
                ini.WriteString("masks", "color", base64);
                break;
            }
            ini.UpdateFile();

            maskBuilder[(int)type].Dispose();
            maskBuilder[(int)type] = null;

			NotifySaved ("mask_" + ((int)type));
		}

		#endregion

		#region custom events

		EventHandler<ScanDataEventArgs> scanDataChanged;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<ScanDataEventArgs> ScanDataChanged {
			add {
				scanDataChanged += value;
			}
			remove {
				scanDataChanged -= value;
			}
		}

		#endregion

		/// <summary>
		/// Resets mask data
		/// </summary>
		/// <param name="type">Scan type.</param>
		public void ResetMask(ScanType type)
		{
			maskBuilder[(int)type].Dispose();
			maskBuilder[(int)type] = new XD.ImageBuilder(size.Width, size.Height);

			NotifyChange ("mask_" + ((int)type));
		}

		/// <summary>
		/// Notifies that something for this scan has changed.
		/// </summary>
		/// <param name="changeOf">Type of change</param>
		/// <remarks>
		/// We need to save this data somewhere!
		/// </remarks>
		public void NotifyChange(string changeOf)
		{
			unsaved.Add (changeOf);

			if (scanDataChanged != null) {
				ScanDataEventArgs dataChangedEvent = new ScanDataEventArgs (changeOf, unsaved);
				scanDataChanged (this, dataChangedEvent);
			}
		}

		/// <summary>
		/// Notifies that an attribute was saved.
		/// </summary>
		/// <param name="changeOf">Type of change</param>
		public void NotifySaved(string changeOf)
		{
			unsaved.Remove (changeOf);

			if (scanDataChanged != null) {
				ScanDataEventArgs dataChangedEvent = new ScanDataEventArgs (changeOf, unsaved);
				scanDataChanged (this, dataChangedEvent);
			}
		}

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

		#region Getter and setter

		public string FiberType {
			get { return fiberType; }
			set {
				if(!fiberType.Equals(value)) {
					fiberType = value;
					NotifyChange ("FiberType");
				}
			}
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

		public HashSet<string> Unsaved {
			get {
				return new HashSet<string>(unsaved);
			}
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

		public bool HasUnsaved() {
			return unsaved.Count > 0;
		}
	}
}

