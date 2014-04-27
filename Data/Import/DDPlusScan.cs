//
//  DDPlusScan.cs
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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using XD = Xwt.Drawing;
using System.Drawing;

namespace Baimp
{
	public class DDPlusScan : BaseScan
	{
		static string[] scanTypes = {
			"Intensity",
			"Topography",
			"Color"
		};
			
		Dictionary<string, string> filenames = new Dictionary<string, string>();
		Dictionary<string, float[]> arrayData = new Dictionary<string, float[]>();

		Dictionary<string, float> max = new Dictionary<string, float>();


		/// <summary>
		/// Needed for xml serializer
		/// </summary>
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
					Metadata.Add(new Metadata(datum.Item1, datum.Item2));
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
			return scanTypes;
		}

		/// <summary>
		/// Get specified scan as byte buffer.
		/// </summary>
		/// <returns>The byte buffer.</returns>
		/// <param name="scanType">Scan type.</param>
		private byte[] GetByteBuffer(string scanType)
		{
			Stream s = File.OpenRead(filenames[scanType]);
			BinaryReader input = new BinaryReader(s);

			int length = input.ReadInt32() * input.ReadInt32();

			return input.ReadBytes(length * 4);
		}

		/// <summary>
		/// Gets scan as array.
		/// </summary>
		/// <returns>The specified scan as a plain float array.</returns>
		/// <param name="scanType">Type.</param>
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
				Metadata zLength = Metadata.Find( m => m.key == "zLengthPerDigitF" );
				if (zLength != null) {
					float zLengthValue = Convert.ToSingle(zLength.value) / 10000000.0f;
					arrayData[scanType] = arrayData[scanType].Select( x => x * zLengthValue).ToArray();
				}
			}

			return arrayData[scanType];
		}

		/// <summary>
		/// Get scan as bitmap.
		/// </summary>
		/// <returns>The specified scan as a bitmap.</returns>
		/// <param name="scanType">Scan type.</param>
		public override unsafe Bitmap GetAsBitmap(string scanType)
		{
			int width = (int) Size.Width;
			int height = (int) Size.Height;

			Bitmap bitmap;
			if (scanType == "Color") {
				bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			} else {
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
				

			if (scanType == "Color") {
				int len = width * height * 4;
				//byte[] buffer = GetByteBuffer(scanType);
				//Marshal.Copy(buffer, 0, bmpData.Scan0, len);

				UnmanagedMemoryStream ums = 
					new UnmanagedMemoryStream((byte*)bmpData.Scan0.ToPointer(), 0, len, FileAccess.Write);

				Stream s = File.OpenRead(filenames[scanType]);
				s.Position = 8; // skip first two ints
				s.CopyTo(ums, len);

			} else {
				float[] array = GetAsArray(scanType);
				float maxForType = this.max[scanType];
				float min = array.Min();

				byte* scan0 = (byte*) bmpData.Scan0.ToPointer();
				int len = width * height;
				for (int i = 0; i < len; ++i) {
					byte color = (byte) ((array[i] - min) / (maxForType - min) * 255);
					*scan0 = color;
					scan0++;
				}
			}

			//Unlock the pixels
			bitmap.UnlockBits(bmpData);

			return bitmap;
		}

		public override unsafe Bitmap GetAsColorizedBitmap(string scanType)
		{
			return GetAsBitmap(scanType); // TODO
		}

		#endregion
	}
}

