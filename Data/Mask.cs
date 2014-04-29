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

		public readonly List<MaskEntry> MaskPosition = new List<MaskEntry>();

		public Mask(BaseScan scan)
		{
			this.scan = scan;
		}

		public bool HasMask()
		{
			return (bool) Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
				if (zipFile != null) {
					ZipEntry maskEntry = zipFile.GetEntry(MaskFilename());
					if (maskEntry != null) {
						return true;
					}

					return false;
				}

				return false;
			}));
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
						ZipEntry maskEntry = zipFile.GetEntry(MaskFilename());
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
			if (MaskPosition.Count == 0) {
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

				zipFile.Add(source, MaskFilename());
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

			scan.NotifySaved("mask");
		}

		/// <summary>
		/// Resets mask data
		/// </summary>
		public void ResetMask()
		{
			MaskPosition.Clear();
			if (maskBuilder != null) {
				maskBuilder.Dispose();
				maskBuilder = new XD.ImageBuilder(scan.Size.Width, scan.Size.Height);

				scan.NotifyChange("mask");
			}
		}
			
		/// <summary>
		/// Get the filename in save archive of this mask.
		/// </summary>
		/// <returns>The filename.</returns>
		string MaskFilename()
		{
			return String.Format("masks/{0}.png", scan.Name);
		}

		/// <summary>
		/// Renders new mask position.
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="bufferSize">Buffer size (entries that should not be rendered, used by undo function).</param>
		public void FlushMaskPositions(XD.Context ctx, int bufferSize = 30)
		{
			bool first = true;
			if (MaskPosition.Count >= bufferSize) {
				ctx.SetColor(maskColor);
				for (int i = 0; i < MaskPosition.Count - bufferSize; i++) {
					switch (MaskPosition[i].type) {
					case MaskEntryType.Point:
						ctx.SetLineWidth(MaskPosition[i].pointerSize * 2);
						if (first) {
							first = false;
							ctx.MoveTo(MaskPosition[i].position);
						} else {
							ctx.LineTo(MaskPosition[i].position);
						}
						ctx.Stroke();

						ctx.Arc(
							MaskPosition[i].position.X, MaskPosition[i].position.Y,
							MaskPosition[i].pointerSize, 0, 360);
						ctx.Fill();

						ctx.MoveTo(MaskPosition[i].position);
						break;
					case MaskEntryType.Space:
						ctx.Stroke();
						ctx.ClosePath();
						break;
					case MaskEntryType.Delete:
						ctx.Arc(
							MaskPosition[i].position.X, MaskPosition[i].position.Y,
							MaskPosition[i].pointerSize, 0, 360);
						ctx.Save();
						ctx.Clip();
						int newX = (int) Math.Min(Math.Max(
							MaskPosition[i].position.X - MaskPosition[i].pointerSize, 0), scan.Size.Width);
						int newY = (int) Math.Min(Math.Max(
							MaskPosition[i].position.Y - MaskPosition[i].pointerSize, 0), scan.Size.Height);

						using (XD.ImageBuilder ibnew = 
							new XD.ImageBuilder(MaskPosition[i].pointerSize * 2, MaskPosition[i].pointerSize * 2)) {
							XD.BitmapImage bi = ibnew.ToBitmap();
							scan.GetAsImage(CurrentScanType, false).WithBoxSize(scan.Size).ToBitmap().CopyArea(
								newX, newY, MaskPosition[i].pointerSize * 2, MaskPosition[i].pointerSize * 2,
								bi, 0, 0);
							ctx.DrawImage(bi, new Xwt.Point(newX, newY));
						}
						ctx.Restore();
						ctx.ClosePath();
						break;
					}
				}
				ctx.Stroke();

				MaskPosition.RemoveRange(0, MaskPosition.Count - bufferSize);

				scan.NotifyChange("mask");
			}
		}

		/// <summary>
		/// Undo the last added mask position.
		/// </summary>
		public void Undo()
		{
			for (int i = MaskPosition.Count-1; i >= 0; --i) {
				if (MaskPosition[i].type != MaskEntryType.Space) {
					MaskPosition.RemoveAt(i);
					break;
				}
			}
		}

		#region Properties

		public string CurrentScanType {
			get;
			set;
		}

		#endregion
	}
}

