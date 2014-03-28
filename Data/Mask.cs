using System;
using System.Collections.Generic;
using XD = Xwt.Drawing;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace baimp
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
			using (ZipFile zipFile = new ZipFile(Project.ProjectFile)) {
				ZipEntry maskEntry = zipFile.GetEntry(String.Format("masks/%1_%2.png", scan.Name, scanType));
				if (maskEntry != null) {
					Stream maskStream = zipFile.GetInputStream(maskEntry);

					mask = XD.Image.FromStream(maskStream);
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
		public unsafe void SaveMask(string scanType)
		{

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
	}
}

