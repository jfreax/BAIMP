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

		Dictionary<string, int> measurementCondition = new Dictionary<string, int>();
		byte[] header;

		private int measurementConditionOffset;
		private int colorImageOffset;
		private int colorLaserImageOffset;
		private int laser0ImageOffset;
		private int laser1ImageOffset;
		private int laser2ImageOffset;
		private int height0ImageOffset;
		private int height1ImageOffset;
		private int height2ImageOffset;

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
			measurementCondition["year"] = fileReader.ReadInt32();
			measurementCondition["month"] = fileReader.ReadInt32();
			measurementCondition["day"] = fileReader.ReadInt32();
			measurementCondition["hour"] = fileReader.ReadInt32();
			measurementCondition["minute"] = fileReader.ReadInt32();
			measurementCondition["second"] = fileReader.ReadInt32();
			measurementCondition["minuteDifftoUTC"] = fileReader.ReadInt32();
			measurementCondition["imageAttributes"] = fileReader.ReadInt32();
			measurementCondition["userInterface"] = fileReader.ReadInt32();
			measurementCondition["colorComposition"] = fileReader.ReadInt32();
			measurementCondition["layers"] = fileReader.ReadInt32();
			measurementCondition["runMode"] = fileReader.ReadInt32();
			measurementCondition["peakmode"] = fileReader.ReadInt32();
			measurementCondition["sharpeningLevel"] = fileReader.ReadInt32();
			measurementCondition["speed"] = fileReader.ReadInt32();
			measurementCondition["distance"] = fileReader.ReadInt32();
			measurementCondition["zPitch"] = fileReader.ReadInt32();
			measurementCondition["opticalZoom"] = fileReader.ReadInt32();
			measurementCondition["numLine"] = fileReader.ReadInt32();
			measurementCondition["line0pos"] = fileReader.ReadInt32();
			measurementCondition["line1pos"] = fileReader.ReadInt32();
			measurementCondition["line2pos"] = fileReader.ReadInt32();
			measurementCondition["reserved0"] = fileReader.ReadInt32();
			measurementCondition["lensMagnification"] = fileReader.ReadInt32();
			measurementCondition["pmtGainMode"] = fileReader.ReadInt32();
			measurementCondition["pmtGain"] = fileReader.ReadInt32();
			measurementCondition["pmtOffset"] = fileReader.ReadInt32();
			measurementCondition["NDFilter"] = fileReader.ReadInt32();
			measurementCondition["reserved1"] = fileReader.ReadInt32();
			measurementCondition["persistCount"] = fileReader.ReadInt32();
			measurementCondition["shutterSpeedMode"] = fileReader.ReadInt32();
			measurementCondition["shutterSpeed"] = fileReader.ReadInt32();
			measurementCondition["whiteBalanceMode"] = fileReader.ReadInt32();
			measurementCondition["whiteBalanceRed"] = fileReader.ReadInt32();
			measurementCondition["whiteBalanceBlue"] = fileReader.ReadInt32();
			measurementCondition["cameraGain"] = fileReader.ReadInt32();
			measurementCondition["planeCompensation"] = fileReader.ReadInt32();
			measurementCondition["XYLengthUnit"] = fileReader.ReadInt32();
			measurementCondition["ZLenghthUnit"] = fileReader.ReadInt32();
			measurementCondition["XYDecimalPlace"] = fileReader.ReadInt32();
			measurementCondition["ZDecimalPlace"] = fileReader.ReadInt32();
			//fileStream.Seek(offset+4*42, SeekOrigin.Begin);
			xscale = fileReader.ReadUInt32();
			yscale = fileReader.ReadUInt32();
			zscale = fileReader.ReadUInt32();
			measurementCondition["GSAmplitude"] = fileReader.ReadInt32();
			measurementCondition["GSOffeset"] = fileReader.ReadInt32();
			measurementCondition["resonantApmplitude"] = fileReader.ReadInt32();
			measurementCondition["resonantOffset"] = fileReader.ReadInt32();
			measurementCondition["resonantPhase"] = fileReader.ReadInt32();
			measurementCondition["lightFilterType"] = fileReader.ReadInt32();
			measurementCondition["reserved2"] = fileReader.ReadInt32();
			measurementCondition["gammaReverse"] = fileReader.ReadInt32();
			measurementCondition["gamma"] = fileReader.ReadInt32();
			measurementCondition["Keyence_offset"] = fileReader.ReadInt32();
			measurementCondition["CCD_BW_Offset"] = fileReader.ReadInt32();
			measurementCondition["NumericalAparture"] = fileReader.ReadInt32();
			measurementCondition["HeadType"] = fileReader.ReadInt32();
			measurementCondition["pmtGain2"] = fileReader.ReadInt32();
			measurementCondition["omitColorImage"] = fileReader.ReadInt32();
			measurementCondition["lensID"] = fileReader.ReadInt32();
			measurementCondition["LightUTMode"] = fileReader.ReadInt32();
			measurementCondition["LightLUTIn0"] = fileReader.ReadInt32();
			measurementCondition["LightLUTOut0"] = fileReader.ReadInt32();
			measurementCondition["LightLUTIn1"] = fileReader.ReadInt32();
			measurementCondition["LightLUTOut1"] = fileReader.ReadInt32();
			measurementCondition["LightLUTIn2"] = fileReader.ReadInt32();
			measurementCondition["LightLUTOut2"] = fileReader.ReadInt32();
			measurementCondition["LightLUTIn3"] = fileReader.ReadInt32();
			measurementCondition["LightLUTOut3"] = fileReader.ReadInt32();
			measurementCondition["LightLUTIn4"] = fileReader.ReadInt32();
			measurementCondition["LightLUTOut4"] = fileReader.ReadInt32();
			measurementCondition["upperPosition"] = fileReader.ReadInt32();
			measurementCondition["lowerPosition"] = fileReader.ReadInt32();
			measurementCondition["lightEffectiveBitDepth"] = fileReader.ReadInt32();
			measurementCondition["heightEffectiveBitDepth"] = fileReader.ReadInt32();
			measurementCondition["resonantPhase2"] = fileReader.ReadInt32();
			measurementCondition["fineMode"] = fileReader.ReadInt32();
			measurementCondition["colorSaturation"] = fileReader.ReadInt32();
			measurementCondition["colorBrightness"] = fileReader.ReadInt32();
			measurementCondition["colorContrast"] = fileReader.ReadInt32();
			measurementCondition["colorFilterSize"] = fileReader.ReadInt32();
			measurementCondition["ViewerVersion1"] = fileReader.ReadInt32();
			measurementCondition["ViewerVersion2"] = fileReader.ReadInt32();
			measurementCondition["ViewerVersion3"] = fileReader.ReadInt32();
			measurementCondition["ViewerVersion4"] = fileReader.ReadInt32();
			measurementCondition["analyzerSavedOption"] = fileReader.ReadInt32();
			measurementCondition["ofdMode"] = fileReader.ReadInt32();
			measurementCondition["ofdAuto"] = fileReader.ReadInt32();
			measurementCondition["lightOfdBrightness"] = fileReader.ReadInt32();
			measurementCondition["lightOfdTexture"] = fileReader.ReadInt32();
			measurementCondition["lightOfdContrast"] = fileReader.ReadInt32();
			measurementCondition["lightOftColor"] = fileReader.ReadInt32();
			measurementCondition["lightOfdBrightness2"] = fileReader.ReadInt32();
			measurementCondition["lightOfdTexture2"] = fileReader.ReadInt32();
			measurementCondition["lightOfdContrast2"] = fileReader.ReadInt32();
			measurementCondition["lightOfdColor2"] = fileReader.ReadInt32();
			measurementCondition["lightOfdBrightness3"] = fileReader.ReadInt32();
			measurementCondition["lightOfdTexture3"] = fileReader.ReadInt32();
			measurementCondition["lightOfdContrast3"] = fileReader.ReadInt32();
			measurementCondition["lightOfdColor3"] = fileReader.ReadInt32();
			measurementCondition["colorLightOfdBrightness"] = fileReader.ReadInt32();
			measurementCondition["colorLightOfdTexture"] = fileReader.ReadInt32();
			measurementCondition["colorLightOfdContrast"] = fileReader.ReadInt32();
			measurementCondition["colorLightOftColor"] = fileReader.ReadInt32();
			measurementCondition["colorOfdBrightness"] = fileReader.ReadInt32();
			measurementCondition["colorOfdTexture"] = fileReader.ReadInt32();
			measurementCondition["colorOfdContrast"] = fileReader.ReadInt32();
			measurementCondition["colorOftColor"] = fileReader.ReadInt32();
			measurementCondition["ldicCompose"] = fileReader.ReadInt32();
			measurementCondition["ldicMixture"] = fileReader.ReadInt32();
			measurementCondition["ldicRange"] = fileReader.ReadInt32();
			measurementCondition["ldicFilterType"] = fileReader.ReadInt32();
			measurementCondition["reserved3"] = fileReader.ReadInt32();
			measurementCondition["refraction"] = fileReader.ReadInt32();
			measurementCondition["filmThickness"] = fileReader.ReadInt32();
			measurementCondition["layerAdjustFilter"] = fileReader.ReadInt32();
			measurementCondition["reserved4"] = fileReader.ReadInt32();
			measurementCondition["reserved5"] = fileReader.ReadInt32();
			measurementCondition["reserved6"] = fileReader.ReadInt32();
			measurementCondition["reserved7"] = fileReader.ReadInt32();
			measurementCondition["filmThicknessAuto"] = fileReader.ReadInt32();
			measurementCondition["reserved8"] = fileReader.ReadInt32();
			measurementCondition["distanceFixed"] = fileReader.ReadInt32();
			measurementCondition["redundantPeakElimination"] = fileReader.ReadInt32();
			measurementCondition["opticalAlignmentForFilm"] = fileReader.ReadInt32();
			measurementCondition["newFilmThicknessReady"] = fileReader.ReadInt32();
			measurementCondition["reserved9"] = fileReader.ReadInt32();
			zscaleF = fileReader.ReadDouble();
			measurementCondition["resonantReverseOffset"] = fileReader.ReadInt32();
			measurementCondition["scanSize"] = fileReader.ReadInt32();
			measurementCondition["NDFilter2"] = fileReader.ReadInt32();
			measurementCondition["LUTApplied"] = (int)fileReader.ReadByte();
			measurementCondition["unitMode"] = fileReader.ReadInt32();
			measurementCondition["unused1"] = fileReader.ReadInt32();
			measurementCondition["WdrApplied"] = (int)fileReader.ReadByte();
			measurementCondition["lensWorkingDistance"] = fileReader.ReadInt32();
			measurementCondition["linerarHeightZero"] = fileReader.ReadInt32();
			measurementCondition["cameraEdgeStrength"] = fileReader.ReadInt32();
			measurementCondition["cameraLinearFilter"] = fileReader.ReadInt32();
			measurementCondition["RpdPitch"] = fileReader.ReadInt32();
			measurementCondition["ZAxisModeOption"] = fileReader.ReadInt32();
			measurementCondition["ZAxisModeApplied"] = fileReader.ReadInt32();
			measurementCondition["HalogenNDFFilter"] = fileReader.ReadInt32();
			measurementCondition["PostProcessingFilter"] = fileReader.ReadInt32();
			measurementCondition["PostProcessingFilterApplied"] = fileReader.ReadInt32();
			measurementCondition["pseudoShapeFilter"] = fileReader.ReadInt32();
			measurementCondition["pseudoShapeFilterApplied"] = fileReader.ReadInt32();
			measurementCondition["pseudoShapeFilterRequired"] = fileReader.ReadInt32();
			measurementCondition["activeNumericalAperture"] = fileReader.ReadInt32();
			measurementCondition["blackLevel"] = fileReader.ReadInt32();

			Metadata.Add(new Metadata("lensMagnification", (measurementCondition["lensMagnification"]/10).ToString()));

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

