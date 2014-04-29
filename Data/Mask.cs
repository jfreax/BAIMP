//
//  Mask.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
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
	[Flags]
	public enum MaskEntryType
	{
		Point = 0,
		Delete = 1,
		Space = 2
	}

	public class MaskEntry
	{
		public Xwt.Point position;
		public MaskEntryType type;
		public int pointerSize;

		public MaskEntry(Xwt.Point position, MaskEntryType type, int pointerSize)
		{
			this.position = position;
			this.type = type;
			this.pointerSize = pointerSize;
		}
	}

	public class Mask
	{
		#region static member

		public static Xwt.Drawing.Color maskColor = XD.Colors.DarkBlue;

		#endregion

		public delegate void ImageLoadedCallback(XD.Image image);

		readonly BaseScan scan;
		XD.ImageBuilder maskBuilder;
		Bitmap bitmapCache;
		bool wasResetted;

		public readonly List<MaskEntry> MaskPositions = new List<MaskEntry>();

		public Mask(BaseScan scan)
		{
			this.scan = scan;
		}

		/// <summary>
		/// Loads mask data
		/// </summary>
		/// <returns>The mask as an image.</returns>
		XD.Image LoadMask()
		{
			if (!scan.HasMask) {
				return null;
			}

			XD.Image mask = null;
			Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
				if (zipFile != null) {
					ZipEntry maskEntry = zipFile.GetEntry(MaskFilename);
					if (maskEntry != null) {
						Stream maskStream = zipFile.GetInputStream(maskEntry);
						mask = XD.Image.FromStream(maskStream);
						return true;
					}

					return false;
				}

				return false;
			}));

			return mask;
		}

		public Bitmap GetMaskAsBitmap()
		{
			if (bitmapCache == null) {
				bitmapCache = Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
					if (zipFile != null) {
						ZipEntry maskEntry = zipFile.GetEntry(MaskFilename);
						if (maskEntry != null) {
							Stream maskStream = zipFile.GetInputStream(maskEntry);
							return new Bitmap(Image.FromStream(maskStream));
						}

						return null;
					}

					return null;
				})) as Bitmap;
			}

			return bitmapCache;
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
						maskBuilder.Context.DrawImage(mask.WithBoxSize(scan.Size), Xwt.Point.Zero);
					}
			}
				
			return maskBuilder;
		}

		/// <summary>
		/// Gets the mask as image.
		/// </summary>
		/// <returns>The mask as image.</returns>
		public XD.Image GetMaskAsImage()
		{
			var mb = GetMaskBuilder();
			if (mb == null) {
				return null;
			}

			return mb.ToVectorImage().WithBoxSize(scan.RequestedBitmapSize);
		}

		/// <summary>
		/// Gets the mask as image.
		/// </summary>
		/// <returns>The mask as image.</returns>
		public void GetMaskAsImageAsync(ImageLoadedCallback callback)
		{
			Task.Factory.StartNew( () => {
				XD.Image maskImage = GetMaskBuilder().ToVectorImage().WithBoxSize(scan.RequestedBitmapSize);

				Application.Invoke(() => callback(maskImage));
			});
		}
			
		/// <summary>
		/// Saves the mask.
		/// </summary>
		public unsafe void Save()
		{
			if (MaskPositions.Count <= 1) {
				if (wasResetted) {
					Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
						zipFile.BeginUpdate();
						if (zipFile.FindEntry(MaskFilename, false) != -1) {
							zipFile.Delete(zipFile.GetEntry(MaskFilename));
						}

						zipFile.CommitUpdate();

						return null;
					}));
					wasResetted = false;
				}
				return;
			}

			MemoryStream outStream = new MemoryStream();

			using (XD.ImageBuilder mb = GetMaskBuilder()) {
				FlushMaskPositions(mb.Context, 0);

				using (XD.ImageBuilder outIb = new XD.ImageBuilder(mb.Width, mb.Height)) {
					XD.BitmapImage mask = mb.ToBitmap();
					outIb.Context.DrawImage(mask, new Xwt.Point(0, 0));
					outIb.Context.DrawImage(mask, new Xwt.Point(1, 1));
					outIb.Context.DrawImage(mask, new Xwt.Point(-1, -1));

					mask = outIb.ToBitmap();
					mask.Save(outStream, XD.ImageFileType.Png);
				}
			}

			outStream.Position = 0;

			Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
				zipFile.BeginUpdate();

				CustomStaticDataSource source = new CustomStaticDataSource(outStream);

				zipFile.Add(source, MaskFilename);
				zipFile.IsStreamOwner = true;
				zipFile.CommitUpdate();

				return null;
			}));

			maskBuilder.Dispose();
			maskBuilder = null;

			if (bitmapCache != null) {
				bitmapCache.Dispose();
				bitmapCache = null;
			}

			scan.HasMask = true;
			scan.NotifySaved("mask");
		}

		/// <summary>
		/// Resets mask data
		/// </summary>
		public void ResetMask()
		{
			MaskPositions.Clear();
			if (maskBuilder != null) {
				maskBuilder.Dispose();
				maskBuilder = new XD.ImageBuilder(scan.Size.Width, scan.Size.Height);

				wasResetted = true;
				scan.HasMask = false;
				scan.NotifyChange("mask");
			}
		}

		/// <summary>
		/// Renders new mask position.
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="bufferSize">Buffer size (entries that should not be rendered, used by undo function).</param>
		public void FlushMaskPositions(XD.Context ctx, int bufferSize = 30)
		{
			bool first = true;
			if (MaskPositions.Count >= bufferSize) {
				wasResetted = false;

				ctx.SetColor(maskColor);
				for (int i = 0; i < MaskPositions.Count - bufferSize; i++) {
					switch (MaskPositions[i].type) {
					case MaskEntryType.Point:
						ctx.SetLineWidth(MaskPositions[i].pointerSize * 2);
						if (first) {
							first = false;
							ctx.MoveTo(MaskPositions[i].position);
						} else {
							ctx.LineTo(MaskPositions[i].position);
						}
						ctx.Stroke();

						ctx.Arc(
							MaskPositions[i].position.X, MaskPositions[i].position.Y,
							MaskPositions[i].pointerSize, 0, 360);
						ctx.Fill();

						ctx.MoveTo(MaskPositions[i].position);
						break;
					case MaskEntryType.Space:
						ctx.Stroke();
						ctx.ClosePath();
						break;
					case MaskEntryType.Delete:
						ctx.Arc(
							MaskPositions[i].position.X, MaskPositions[i].position.Y,
							MaskPositions[i].pointerSize, 0, 360);
						ctx.Save();
						ctx.Clip();
						int newX = (int) Math.Min(Math.Max(
							MaskPositions[i].position.X - MaskPositions[i].pointerSize, 0), scan.Size.Width);
						int newY = (int) Math.Min(Math.Max(
							MaskPositions[i].position.Y - MaskPositions[i].pointerSize, 0), scan.Size.Height);

						using (XD.ImageBuilder ibnew = 
							new XD.ImageBuilder(MaskPositions[i].pointerSize * 2, MaskPositions[i].pointerSize * 2)) {
							XD.BitmapImage bi = ibnew.ToBitmap();
							scan.GetAsImage(CurrentScanType, false).WithBoxSize(scan.Size).ToBitmap().CopyArea(
								newX, newY, MaskPositions[i].pointerSize * 2, MaskPositions[i].pointerSize * 2,
								bi, 0, 0);
							ctx.DrawImage(bi, new Xwt.Point(newX, newY));
						}
						ctx.Restore();
						ctx.ClosePath();
						break;
					}
				}
				ctx.Stroke();

				MaskPositions.RemoveRange(0, MaskPositions.Count - bufferSize);

				scan.NotifyChange("mask");
			}
		}

		/// <summary>
		/// Undo the last added mask position.
		/// </summary>
		public void Undo()
		{
			for (int i = MaskPositions.Count-1; i >= 0; --i) {
				if (MaskPositions[i].type != MaskEntryType.Space) {
					MaskPositions.RemoveAt(i);
					break;
				}
			}
		}

		#region Properties

		public string CurrentScanType {
			get;
			set;
		}

		/// <summary>
		/// Get the filename in save archive of this mask.
		/// </summary>
		/// <returns>The filename.</returns>
		public string MaskFilename {
			get {
				return String.Format("masks/{0}.png", scan.Name);
			}
		}

		#endregion
	}
}

