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
		private readonly BaseScan scan;
		private XD.ImageBuilder maskBuilder;

		public Mask(BaseScan scan)
		{
			this.scan = scan;
		}

		/// <summary>
		/// Loads mask data
		/// </summary>
		/// <returns>The mask as an image.</returns>
		private XD.Image LoadMask()
		{
			XD.Image mask = null;
			Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
				if (zipFile != null) {
					ZipEntry maskEntry = zipFile.GetEntry(MaskFilename());
					if (maskEntry != null) {
						Stream maskStream = zipFile.GetInputStream(maskEntry);
						mask = XD.Image.FromStream(maskStream);
					}

					return true;
				} else {
					return false;
				}
			}));

			return mask;
		}

		/// <summary>
		/// Gets the mask builder to draw on it.
		/// </summary>
		/// <returns>The mask builder.</returns>
		/// <remarks>
		/// Set the status of this scan to "unsaved"
		/// </remarks>
		public XD.ImageBuilder GetMaskBuilder()
		{
			if (maskBuilder == null) {
				maskBuilder = new XD.ImageBuilder(scan.Size.Width, scan.Size.Height);

				XD.Image mask = LoadMask();
				if (mask != null) {
					maskBuilder.Context.DrawImage(mask.WithBoxSize(scan.Size), Xwt.Point.Zero, 0.6);
				}
			}
				
			return maskBuilder;
		}

		/// <summary>
		/// Gets the mask as image.
		/// </summary>
		/// <returns>The mask as image.</returns>
		/// <param name="scanType">Type.</param>
		public XD.Image GetMaskAsImage()
		{
			return GetMaskBuilder().ToVectorImage().WithBoxSize(scan.RequestedBitmapSize);
		}
			

		/// <summary>
		/// Saves the mask.
		/// </summary>
		public unsafe void Save()
		{
			MemoryStream outStream = new MemoryStream();

			XD.BitmapImage mask = GetMaskBuilder().ToBitmap();
			XD.Color maskColor = ScanView.maskColor.WithAlpha(1.0);

			if (MainClass.toolkitType == ToolkitType.Gtk) {
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

			Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
				zipFile.BeginUpdate();

				CustomStaticDataSource source = new CustomStaticDataSource(outStream);

				zipFile.Add(source, MaskFilename());
				zipFile.IsStreamOwner = true;
				zipFile.CommitUpdate();

				return null;
			}));

			maskBuilder.Dispose();
			maskBuilder = null;

			scan.NotifySaved("mask");
		}


		/// <summary>
		/// Resets mask data
		/// </summary>
		public void ResetMask()
		{
			if (maskBuilder != null) {
				maskBuilder.Dispose();
				maskBuilder = new XD.ImageBuilder(scan.Size.Width, scan.Size.Height);

				scan.NotifyChange("mask");
			}
		}


		private string MaskFilename()
		{
			return String.Format("masks/{0}.png", scan.Name);
		}
	}
}

