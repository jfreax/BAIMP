//
//  VK4Scan.cs
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
using System.Xml.Serialization;
using System.Drawing;
using System.Drawing.Imaging;

namespace Baimp
{
	public class VK4Scan : BaseScan
	{
		static string[] scanTypes = {
			"Intensity",
			"Topography",
			"Color"
		};

		byte[] header;

		int measurementConditionOffset;
		int colorImageOffset;
		int colorLaserImageOffset;
		int laser0ImageOffset;
		int laser1ImageOffset;
		int laser2ImageOffset;
		int height0ImageOffset;
		int height1ImageOffset;
		int height2ImageOffset;

		[XmlIgnore]
		public uint xscale;
		[XmlIgnore]
		public uint yscale;
		[XmlIgnore]
		public uint zscale;
		[XmlIgnore]
		public double zscaleF;

		/// <summary>
		/// Needed for xml serializer
		/// </summary>
		public VK4Scan()
		{
		}

		public override void Initialize(string filePath, bool newImport = true)
		{
			base.Initialize(filePath, newImport);

			using (FileStream fileStream = new FileStream(filePath, FileMode.Open)) {
				using (BinaryReader fileReader = new BinaryReader(fileStream)) {
					header = new byte[12];
					fileReader.Read(header, 0, 12);

					//reading offsets
					measurementConditionOffset = fileReader.ReadInt32();
					colorImageOffset = fileReader.ReadInt32();
					colorLaserImageOffset = fileReader.ReadInt32();
					laser0ImageOffset = fileReader.ReadInt32();
					laser1ImageOffset = fileReader.ReadInt32();
					laser2ImageOffset = fileReader.ReadInt32();
					height0ImageOffset = fileReader.ReadInt32();
					height1ImageOffset = fileReader.ReadInt32();
					height2ImageOffset = fileReader.ReadInt32();

					if (newImport) {
						ReadMeasurementCondition(fileReader);
					}
				}
			}
		}

		private void ReadMeasurementCondition(BinaryReader fileReader)
		{
			fileReader.ReadBytes(4 * 10);
			metadata["year"] = fileReader.ReadInt32();
			metadata["month"] = fileReader.ReadInt32();
			metadata["day"] = fileReader.ReadInt32();
			metadata["hour"] = fileReader.ReadInt32();
			metadata["minute"] = fileReader.ReadInt32();
			metadata["second"] = fileReader.ReadInt32();
			metadata["minuteDifftoUTC"] = fileReader.ReadInt32();
			metadata["imageAttributes"] = fileReader.ReadInt32();
			metadata["userInterface"] = fileReader.ReadInt32();
			metadata["colorComposition"] = fileReader.ReadInt32();
			metadata["layers"] = fileReader.ReadInt32();
			metadata["runMode"] = fileReader.ReadInt32();
			metadata["peakmode"] = fileReader.ReadInt32();
			metadata["sharpeningLevel"] = fileReader.ReadInt32();
			metadata["speed"] = fileReader.ReadInt32();
			metadata["distance"] = fileReader.ReadInt32();
			metadata["zPitch"] = fileReader.ReadInt32();
			metadata["opticalZoom"] = fileReader.ReadInt32();
			metadata["numLine"] = fileReader.ReadInt32();
			metadata["line0pos"] = fileReader.ReadInt32();
			metadata["line1pos"] = fileReader.ReadInt32();
			metadata["line2pos"] = fileReader.ReadInt32();
			metadata["reserved0"] = fileReader.ReadInt32();
			metadata["LensMagnification"] = fileReader.ReadInt32() / 10;
			metadata["pmtGainMode"] = fileReader.ReadInt32();
			metadata["pmtGain"] = fileReader.ReadInt32();
			metadata["pmtOffset"] = fileReader.ReadInt32();
			metadata["NDFilter"] = fileReader.ReadInt32();
			metadata["reserved1"] = fileReader.ReadInt32();
			metadata["persistCount"] = fileReader.ReadInt32();
			metadata["shutterSpeedMode"] = fileReader.ReadInt32();
			metadata["shutterSpeed"] = fileReader.ReadInt32();
			metadata["whiteBalanceMode"] = fileReader.ReadInt32();
			metadata["whiteBalanceRed"] = fileReader.ReadInt32();
			metadata["whiteBalanceBlue"] = fileReader.ReadInt32();
			metadata["cameraGain"] = fileReader.ReadInt32();
			metadata["planeCompensation"] = fileReader.ReadInt32();
			metadata["XYLengthUnit"] = fileReader.ReadInt32();
			metadata["ZLenghthUnit"] = fileReader.ReadInt32();
			metadata["XYDecimalPlace"] = fileReader.ReadInt32();
			metadata["ZDecimalPlace"] = fileReader.ReadInt32();
			//fileStream.Seek(offset+4*42, SeekOrigin.Begin);
			xscale = fileReader.ReadUInt32();
			yscale = fileReader.ReadUInt32();
			zscale = fileReader.ReadUInt32();
			metadata["GSAmplitude"] = fileReader.ReadInt32();
			metadata["GSOffeset"] = fileReader.ReadInt32();
			metadata["resonantApmplitude"] = fileReader.ReadInt32();
			metadata["resonantOffset"] = fileReader.ReadInt32();
			metadata["resonantPhase"] = fileReader.ReadInt32();
			metadata["lightFilterType"] = fileReader.ReadInt32();
			metadata["reserved2"] = fileReader.ReadInt32();
			metadata["gammaReverse"] = fileReader.ReadInt32();
			metadata["gamma"] = fileReader.ReadInt32();
			metadata["Keyence_offset"] = fileReader.ReadInt32();
			metadata["CCD_BW_Offset"] = fileReader.ReadInt32();
			metadata["NumericalAparture"] = fileReader.ReadInt32();
			metadata["HeadType"] = fileReader.ReadInt32();
			metadata["pmtGain2"] = fileReader.ReadInt32();
			metadata["omitColorImage"] = fileReader.ReadInt32();
			metadata["lensID"] = fileReader.ReadInt32();
			metadata["LightUTMode"] = fileReader.ReadInt32();
			metadata["LightLUTIn0"] = fileReader.ReadInt32();
			metadata["LightLUTOut0"] = fileReader.ReadInt32();
			metadata["LightLUTIn1"] = fileReader.ReadInt32();
			metadata["LightLUTOut1"] = fileReader.ReadInt32();
			metadata["LightLUTIn2"] = fileReader.ReadInt32();
			metadata["LightLUTOut2"] = fileReader.ReadInt32();
			metadata["LightLUTIn3"] = fileReader.ReadInt32();
			metadata["LightLUTOut3"] = fileReader.ReadInt32();
			metadata["LightLUTIn4"] = fileReader.ReadInt32();
			metadata["LightLUTOut4"] = fileReader.ReadInt32();
			metadata["upperPosition"] = fileReader.ReadInt32();
			metadata["lowerPosition"] = fileReader.ReadInt32();
			metadata["lightEffectiveBitDepth"] = fileReader.ReadInt32();
			metadata["heightEffectiveBitDepth"] = fileReader.ReadInt32();
			metadata["resonantPhase2"] = fileReader.ReadInt32();
			metadata["fineMode"] = fileReader.ReadInt32();
			metadata["colorSaturation"] = fileReader.ReadInt32();
			metadata["colorBrightness"] = fileReader.ReadInt32();
			metadata["colorContrast"] = fileReader.ReadInt32();
			metadata["colorFilterSize"] = fileReader.ReadInt32();
			metadata["ViewerVersion1"] = fileReader.ReadInt32();
			metadata["ViewerVersion2"] = fileReader.ReadInt32();
			metadata["ViewerVersion3"] = fileReader.ReadInt32();
			metadata["ViewerVersion4"] = fileReader.ReadInt32();
			metadata["analyzerSavedOption"] = fileReader.ReadInt32();
			metadata["ofdMode"] = fileReader.ReadInt32();
			metadata["ofdAuto"] = fileReader.ReadInt32();
			metadata["lightOfdBrightness"] = fileReader.ReadInt32();
			metadata["lightOfdTexture"] = fileReader.ReadInt32();
			metadata["lightOfdContrast"] = fileReader.ReadInt32();
			metadata["lightOftColor"] = fileReader.ReadInt32();
			metadata["lightOfdBrightness2"] = fileReader.ReadInt32();
			metadata["lightOfdTexture2"] = fileReader.ReadInt32();
			metadata["lightOfdContrast2"] = fileReader.ReadInt32();
			metadata["lightOfdColor2"] = fileReader.ReadInt32();
			metadata["lightOfdBrightness3"] = fileReader.ReadInt32();
			metadata["lightOfdTexture3"] = fileReader.ReadInt32();
			metadata["lightOfdContrast3"] = fileReader.ReadInt32();
			metadata["lightOfdColor3"] = fileReader.ReadInt32();
			metadata["colorLightOfdBrightness"] = fileReader.ReadInt32();
			metadata["colorLightOfdTexture"] = fileReader.ReadInt32();
			metadata["colorLightOfdContrast"] = fileReader.ReadInt32();
			metadata["colorLightOftColor"] = fileReader.ReadInt32();
			metadata["colorOfdBrightness"] = fileReader.ReadInt32();
			metadata["colorOfdTexture"] = fileReader.ReadInt32();
			metadata["colorOfdContrast"] = fileReader.ReadInt32();
			metadata["colorOftColor"] = fileReader.ReadInt32();
			metadata["ldicCompose"] = fileReader.ReadInt32();
			metadata["ldicMixture"] = fileReader.ReadInt32();
			metadata["ldicRange"] = fileReader.ReadInt32();
			metadata["ldicFilterType"] = fileReader.ReadInt32();
			metadata["reserved3"] = fileReader.ReadInt32();
			metadata["refraction"] = fileReader.ReadInt32();
			metadata["filmThickness"] = fileReader.ReadInt32();
			metadata["layerAdjustFilter"] = fileReader.ReadInt32();
			metadata["reserved4"] = fileReader.ReadInt32();
			metadata["reserved5"] = fileReader.ReadInt32();
			metadata["reserved6"] = fileReader.ReadInt32();
			metadata["reserved7"] = fileReader.ReadInt32();
			metadata["filmThicknessAuto"] = fileReader.ReadInt32();
			metadata["reserved8"] = fileReader.ReadInt32();
			metadata["distanceFixed"] = fileReader.ReadInt32();
			metadata["redundantPeakElimination"] = fileReader.ReadInt32();
			metadata["opticalAlignmentForFilm"] = fileReader.ReadInt32();
			metadata["newFilmThicknessReady"] = fileReader.ReadInt32();
			metadata["reserved9"] = fileReader.ReadInt32();
			zscaleF = fileReader.ReadDouble();
			metadata["resonantReverseOffset"] = fileReader.ReadInt32();
			metadata["scanSize"] = fileReader.ReadInt32();
			metadata["NDFilter2"] = fileReader.ReadInt32();
			metadata["LUTApplied"] = (int)fileReader.ReadByte();
			metadata["unitMode"] = fileReader.ReadInt32();
			metadata["unused1"] = fileReader.ReadInt32();
			metadata["WdrApplied"] = (int)fileReader.ReadByte();
			metadata["lensWorkingDistance"] = fileReader.ReadInt32();
			metadata["linerarHeightZero"] = fileReader.ReadInt32();
			metadata["cameraEdgeStrength"] = fileReader.ReadInt32();
			metadata["cameraLinearFilter"] = fileReader.ReadInt32();
			metadata["RpdPitch"] = fileReader.ReadInt32();
			metadata["ZAxisModeOption"] = fileReader.ReadInt32();
			metadata["ZAxisModeApplied"] = fileReader.ReadInt32();
			metadata["HalogenNDFFilter"] = fileReader.ReadInt32();
			metadata["PostProcessingFilter"] = fileReader.ReadInt32();
			metadata["PostProcessingFilterApplied"] = fileReader.ReadInt32();
			metadata["pseudoShapeFilter"] = fileReader.ReadInt32();
			metadata["pseudoShapeFilterApplied"] = fileReader.ReadInt32();
			metadata["pseudoShapeFilterRequired"] = fileReader.ReadInt32();
			metadata["activeNumericalAperture"] = fileReader.ReadInt32();
			metadata["blackLevel"] = fileReader.ReadInt32();
		}

		#region implemented abstract members of BaseScan

		public override string SupportedFileExtensions()
		{
			return "*.vk4";
		}

		public override string[] AvailableScanTypes()
		{
			return scanTypes;
		}

		public override float[] GetAsArray(string scanType)
		{
			using (FileStream fileStream = new FileStream(FilePath, FileMode.Open)) {
				using (BinaryReader fileReader = new BinaryReader(fileStream)) {
					int offset = 0;
					switch (scanType) {
					case "Intensity":
						offset = laser0ImageOffset;
						break;
					case "Topography":
						offset = height0ImageOffset;
						break;
					case "Color":
						offset = colorImageOffset;
						break;
					}

					fileStream.Seek(offset, SeekOrigin.Begin);
					size.Width = fileReader.ReadInt32();
					size.Height = fileReader.ReadInt32();

					if (requestedBitmapSize == Xwt.Size.Zero) {
						requestedBitmapSize = size;
					}

					int bitdepth = fileReader.ReadInt32();

					fileStream.Seek(20, SeekOrigin.Current); // skip more useless data

					int width = (int) size.Width;
					int height = (int) size.Height;

					if (bitdepth == 16) {
						fileStream.Seek(191 * 4, SeekOrigin.Current); // ?
						UInt16[] array = new UInt16[width * height];

						int len = width * height * 2;
						Buffer.BlockCopy(fileReader.ReadBytes(len), 0, array, 0, len);
						return Array.ConvertAll(array, Convert.ToSingle);

					} 
					if (bitdepth == 24) {
						byte[] array = new byte[width * height * 3];
						int len = width * height * 3;

						Buffer.BlockCopy(fileReader.ReadBytes(len), 0, array, 0, len);
						return Array.ConvertAll(array, Convert.ToSingle);
						//return array;
					} 
					if (bitdepth == 32) {
						fileStream.Seek(191 * 4, SeekOrigin.Current);
						UInt32[] array = new UInt32[width * height];
						int len = width * height * 4;

						Buffer.BlockCopy(fileReader.ReadBytes(len), 0, array, 0, len);
						return Array.ConvertAll(array, Convert.ToSingle);
						//return array;
					}

					return null;
				}
			}
		}

		public override unsafe Bitmap GetAsBitmap(string scanType)
		{
			float[] array = GetAsArray(scanType);
			int width = (int) size.Width;
			int height = (int) size.Height;

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
				byte* scan0 = (byte*) bmpData.Scan0.ToPointer();
				int len = width * height; // color image are 24 bit
				for (int i = 0; i < len * 3; i += 3) {
				
					*scan0 = (byte)array[i];
					scan0++;
					*scan0 = (byte)array[i+1];
					scan0++;
					*scan0 = (byte)array[i+2];
					scan0++;
					*scan0 = 255;
					scan0++;
				}
			} else {
				float max = array.Max();
				float min = array.Min();

				byte* scan0 = (byte*) bmpData.Scan0.ToPointer();
				int len = width * height;
				for (int i = 0; i < len; ++i) {
					byte color = (byte) ((array[i] - min) / (max - min) * 255);
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
			if (scanType == "Color") {
				return GetAsBitmap(scanType);
			}
				
			float[] array = GetAsArray(scanType);
			int width = (int) size.Width;
			int height = (int) size.Height;

			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

			//Create a BitmapData and Lock all pixels to be written 
			BitmapData bmpData = bitmap.LockBits(
				new Rectangle(0, 0, width, height),   
				ImageLockMode.WriteOnly, bitmap.PixelFormat);
				
			int offset = (bmpData.Stride / 3) - width;

			byte* scan0 = (byte*) bmpData.Scan0.ToPointer();
			float max = array.Max();
			float min = array.Min();

			int i = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++, i++, scan0++) {
					double intensitiy = 1 - (array[i] - min) / (max - min);
					Color color = ImageTools.ColorFromHSV(intensitiy * 130, 0.8-intensitiy*0.4, 0.9);
						
					*scan0 = color.B;
					scan0++;
					*scan0 = color.G;
					scan0++;
					*scan0 = color.R;
				}

				scan0 += offset;
			}

			//Unlock the pixels
			bitmap.UnlockBits(bmpData);

			return bitmap;
		}

		#endregion
	}
}

