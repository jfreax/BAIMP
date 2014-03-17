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
        public delegate void ImageLoadedCallback(XD.Image image);
        private object lock_image_loading = new object();

		int width;
		int height;
		float zLengthPerDigitF;
		string fiberType;

		Xwt.Size requestedBitmapSize;

		string[] filenames;

		float[][] data;
		float[] max;

		public List<Tuple<string, string>> generalMetadata;

		XD.Image[] renderedImage;
		XD.ImageBuilder[] maskBuilder;

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


        public MemoryStream GetAsMemoryStream(ScanType type)
        {
            MemoryStream memoryStream = new MemoryStream();
            System.Drawing.Bitmap bmp = GetAsBitmap(type);
            if (bmp == null)
            {
                // TODO raise error
                return null;
            }


            bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Tiff);
            memoryStream.Position = 0;

            return memoryStream;

        }

		/// <summary>
		/// Get as image.
		/// </summary>
		/// <returns>The as image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image GetAsImage(ScanType type) {
			//if (renderedImage [(int)type] == null) {
				MemoryStream memoryStream = new MemoryStream ();
				System.Drawing.Bitmap bmp = GetAsBitmap (type);
				if (bmp == null) {
					Console.WriteLine ("bmp == null " + (int)type);
					// TODO raise error
					return null;
				}
				bmp.Save (memoryStream, System.Drawing.Imaging.ImageFormat.Tiff);
				memoryStream.Position = 0;


            return  XD.Image.FromStream (memoryStream);
			//	renderedImage [(int)type] = XD.Image.FromStream (memoryStream);
			//}

			//return renderedImage [(int)type].WithSize (requestedBitmapSize);
		}

        public void GetAsImageAsync(ScanType type, ImageLoadedCallback callback)
        {
            Thread imageLoaderThread = new Thread(delegate() {

                MemoryStream i = null;
                if (renderedImage[(int)type] == null)
                {
                    lock (lock_image_loading)
                    {
                        i = GetAsMemoryStream(type);
                    }
                }

                Xwt.Application.Invoke(delegate()
                {
                    if (renderedImage[(int)type] == null)
                    {
                        renderedImage[(int)type] = XD.Image.FromStream(i).WithSize(requestedBitmapSize);
                    }
                    callback(renderedImage[(int)type]);
                });
                //callback(GetAsImage(type));
            });
            imageLoaderThread.Start();
        }


		/// <summary>
		/// Gets the mask builder to draw on it.
		/// </summary>
		/// <returns>The mask builder.</returns>
		/// <param name="type">Type.</param>
		public XD.ImageBuilder GetMaskBuilder(ScanType type) {
			if(maskBuilder[(int)type] == null) {
				//maskBuilder [(int)type] = new XD.ImageBuilder (requestedBitmapSize.Width, requestedBitmapSize.Height);
				maskBuilder [(int)type] = new XD.ImageBuilder (width, height);
			}
			return maskBuilder [(int)type];
		}


		/// <summary>
		/// Gets the mask as image.
		/// </summary>
		/// <returns>The mask as image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image GetMaskAsImage(ScanType type) {
			return GetMaskBuilder (type).ToVectorImage ().WithSize (requestedBitmapSize);
		}


		/// <summary>
		/// Scales the render size of all images
		/// </summary>
		/// <param name="scaleFactor">Scale factor.</param>
		/// <remarks>
		/// Call GetAsImage again, to get image with correct size
		/// </remarks>
		public void ScaleImage(double scaleFactor) {
			requestedBitmapSize.Height *= scaleFactor;
			requestedBitmapSize.Width *= scaleFactor;
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

		public Xwt.Size RequestedBitmapSize {
			get { return requestedBitmapSize; }
			set { requestedBitmapSize = value;}
		}


		public bool IsScaled() {
			if (requestedBitmapSize.Height != height ||
			    requestedBitmapSize.Width != width) {
				return true;
			} else {
				return false;
			}
		}
	}
}

