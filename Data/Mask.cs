using System;
using System.Collections.Generic;
using Xwt;
using XD = Xwt.Drawing;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Baimp
{
	public class Mask
	{
		private BaseScan scan;
		private Dictionary<string, XD.ImageBuilder> maskBuilder = new Dictionary<string, XD.ImageBuilder>();

		public Mask(BaseScan scan)
		{
			this.scan = scan;
		}

		/// <summary>
		/// Loads mask data
		/// </summary>
		/// <returns>The mask as an image.</returns>
		/// <param name="type">Type.</param>
		private XD.Image LoadMask(string scanType)
		{
			XD.Image mask = null;
			if (File.Exists(Project.ProjectFile)) {
				using (ZipFile zipFile = new ZipFile(Project.ProjectFile)) {
					ZipEntry maskEntry = zipFile.GetEntry(MaskFilename(scanType));
					if (maskEntry != null) {
						Stream maskStream = zipFile.GetInputStream(maskEntry);

						mask = XD.Image.FromStream(maskStream);
					}
				}
			}
			return mask;
		}

		/// <summary>
		/// Gets the mask builder to draw on it.
		/// </summary>
		/// <returns>The mask builder.</returns>
		/// <param name="type">Type.</param>
		/// <remarks>
		/// Set the status of this scan to "unsaved"
		/// </remarks>
		public XD.ImageBuilder GetMaskBuilder(string scanType)
		{
			if (!maskBuilder.ContainsKey(scanType) || maskBuilder[scanType] == null) {
				maskBuilder[scanType] = new XD.ImageBuilder(scan.Size.Width, scan.Size.Height);

				XD.Image mask = LoadMask(scanType);
				if (mask != null) {
					maskBuilder[scanType].Context.DrawImage(mask.WithSize(scan.Size), Xwt.Point.Zero, 0.6);
				}
			}

			return maskBuilder[scanType];
		}

		/// <summary>
		/// Gets the mask as image.
		/// </summary>
		/// <returns>The mask as image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image GetMaskAsImage(string scanType)
		{
			return GetMaskBuilder(scanType).ToVectorImage().WithSize(scan.RequestedBitmapSize);
		}
			

		/// <summary>
		/// Saves the mask.
		/// </summary>
		/// <param name="type">Type.</param>
		public unsafe void Save(string scanType)
		{
			MemoryStream outStream = new MemoryStream();

			XD.BitmapImage mask = GetMaskBuilder(scanType).ToBitmap();
			XD.Color maskColor = ScanView.maskColor.WithAlpha(1.0);

			if (MainClass.toolkitType == Xwt.ToolkitType.Gtk) {
				Parallel.For(0, (int) mask.Height, new Action<int>(y => {
					for (int x = 0; x < mask.Width; x++) {
						XD.Color color = mask.GetPixel(x, y);
						if (color.WithAlpha(1.0) == maskColor) {
							mask.SetPixel(x, y, color.WithAlpha(0.6));
						} else {
							mask.SetPixel(x, y, XD.Colors.Transparent);
						}
					}
				}));

				mask.Save(outStream, XD.ImageFileType.Png);
			} else {
				using (MemoryStream ms = new MemoryStream()) {
					mask.Save(ms, XD.ImageFileType.Png);
					ms.Seek(0, SeekOrigin.Begin);

					Bitmap maskBitmap = new Bitmap(ms);

					BitmapData bmpData = maskBitmap.LockBits(
						new System.Drawing.Rectangle(0, 0, (int) scan.Size.Width, (int) scan.Size.Height),
						ImageLockMode.ReadWrite, maskBitmap.PixelFormat);

					byte* scan0 = (byte*) bmpData.Scan0.ToPointer();
					int len = (int) scan.Size.Width * (int) scan.Size.Height;
					for (int i = 0; i < len; ++i) {
						byte b = *scan0;
						scan0++;
						byte g = *scan0;
						scan0++;
						byte r = *scan0;
						scan0++;
						XD.Color color = XD.Color.FromBytes(r, g, b);

						if ((int) (color.Red * 10) == (int) (maskColor.Red * 10) &&
							(int) (color.Green * 10) == (int) (maskColor.Green * 10) &&
							(int) (color.Blue * 10) == (int) (maskColor.Blue * 10)) {
							*scan0 = 153; // 60% alpha
						} else {
							*(scan0 - 3) = 0;
							*(scan0 - 2) = 0;
							*(scan0 - 1) = 0;
							*(scan0) = 0;
						}

						scan0++;
					}

					maskBitmap.UnlockBits(bmpData);
					maskBitmap.Save(outStream, ImageFormat.Png);
					maskBitmap.Dispose();
				}
			}

			outStream.Position = 0;

			using (ZipFile zipFile = new ZipFile(Project.ProjectFile)) {
				zipFile.BeginUpdate();

				CustomStaticDataSource source = new CustomStaticDataSource(outStream);

				zipFile.Add(source, MaskFilename(scanType));
				zipFile.IsStreamOwner = true;
				zipFile.CommitUpdate();
			}

			maskBuilder[scanType].Dispose();
			maskBuilder[scanType] = null;

			scan.NotifySaved("mask_" + scanType);
		}


		/// <summary>
		/// Resets mask data
		/// </summary>
		/// <param name="type">Scan type.</param>
		public void ResetMask(string scanType)
		{
			maskBuilder[scanType].Dispose();
			maskBuilder[scanType] = new XD.ImageBuilder(scan.Size.Width, scan.Size.Height);

			scan.NotifyChange("mask_" + scanType);
		}


		private string MaskFilename(string scanType)
		{
			return String.Format("masks/{0}_{1}.png", scan.Name, scanType);
		}
	}
}

