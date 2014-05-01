//
//  BaseScan.cs
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
using System.Drawing;
using System.IO;
using XD = Xwt.Drawing;
using System.Collections.Generic;
using System.Xml.Serialization;
using Xwt;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading.Tasks;
using System.Collections;

namespace Baimp
{
	[XmlInclude(typeof(DDPlusScan))]
	[XmlInclude(typeof(VK4Scan))]
	abstract public class BaseScan : IType, IComparer<BaseScan>
	{
		public delegate void ImageLoadedCallback(XD.Image image);
		Object asyncImageLock = new Object();

		/// <summary>
		/// The file path.
		/// </summary>
		string filePath;

		/// <summary>
		/// Name of the scan.
		/// </summary>
		string name = string.Empty;

		/// <summary>
		/// The type of the fiber.
		/// </summary>
		string fiberType = string.Empty;

		/// <summary>
		/// List of unsaved elements
		/// </summary>
		protected HashSet<string> unsaved = new HashSet<string>();

		/// <summary>
		/// The size of the requested bitmap.
		/// </summary>
		protected Xwt.Size requestedBitmapSize = Xwt.Size.Zero;

		/// <summary>
		/// Size of scan.
		/// </summary>
		protected Xwt.Size size;

		/// <summary>
		/// Position in file tree view.
		/// </summary>
		[XmlIgnore]
		public TreePosition position;
		[XmlIgnore]
		public TreePosition positionFiltered;

		/// <summary>
		/// Position of category item in file tree view.
		/// </summary>
		[XmlIgnore]
		public TreePosition parentPosition;

		/// <summary>
		/// True after call to "Initialize".
		/// </summary>
		bool isInitialized;

		/// <summary>
		/// Buffer of rendered images.
		/// </summary>
		Dictionary<string, XD.Image> renderedImage = new Dictionary<string, XD.Image>();
		Dictionary<string, XD.Image> renderedColorImage = new Dictionary<string, XD.Image>();

		/// <summary>
		/// Buffer of thumbnails.
		/// </summary>
		XD.Image[] thumbnails;

		/// <summary>
		/// True if a background worker loads the thumbnail image.
		/// </summary>
		public bool isLoadingThumbnail;

		/// <summary>
		/// The metadata.
		/// </summary>
		protected Dictionary<string, float> metadata = new Dictionary<string, float>();

		/// <summary>
		/// The mask.
		/// </summary>
		Mask mask;

		/// <summary>
		/// Needed for xml serializer.
		/// </summary>
		protected BaseScan()
		{
		}

		/// <summary>
		/// Loading file from filePath and store all information.
		/// </summary>
		/// <param name="filePath">File path.</param>
		/// <param name="newImport">True when data should be read again from original file</param>
		/// <remarks>
		/// Gets recalled on filePath change!
		/// </remarks>
		virtual public void Initialize(string filePath, bool newImport = true)
		{
			this.filePath = filePath;
			this.mask = new Mask(this);

			if (newImport) {
				isInitialized = true;
			}
		}

		/// <summary>
		/// Gets called, when xml deserializer finished
		/// </summary>
		public void OnXmlDeserializeFinish()
		{
			isInitialized = true;

			foreach (Metadata m in internMetadataList) {
				metadata[m.key] = m.value;
			}
		}

		#region abstract methods

		/// <summary>
		/// Regex string with supported file extensions
		/// E.g. "*.abc|*.dfg".
		/// </summary>
		/// <returns>The file extentions.</returns>
		abstract public string SupportedFileExtensions();

		/// <summary>
		/// Array of strings with all available scan types.
		/// E.g. "Intensity", "Intensity".
		/// </summary>
		/// <returns>The fiber types.</returns>
		abstract public string[] AvailableScanTypes();

		/// <summary>
		/// Gets scan as array.
		/// </summary>
		/// <returns>The specified scan as a plain float array.</returns>
		/// <param name="scanType">Type.</param>
		abstract public float[] GetAsArray(string scanType);

		/// <summary>
		/// Get scan as bitmap.
		/// </summary>
		/// <returns>The specified scan as a bitmap.</returns>
		/// <param name="scanType">Scantile</param>
		/// <remarks>
		/// Uncached!
		/// </remarks>
		abstract public unsafe Bitmap GetAsBitmap(string scanType);

		/// <summary>
		/// Get colorized scan as 32bit argb bitmap.
		/// 
		/// Heightmap, intensity, etc should be green to red encoded.
		/// </summary>
		/// <returns>The specified scan as a bitmap.</returns>
		/// <param name="scanType">Scantile</param>
		/// <remarks>
		/// Uncached!
		/// </remarks>
		abstract public unsafe Bitmap GetAsColorizedBitmap(string scanType);

		/// <summary>
		/// Get as image.
		/// </summary>
		/// <returns>The as image.</returns>
		/// <param name="scanType">Type.</param>
		/// <param name="saveImage">Save the result?</param>
		/// <param name="colorized">Colorize (green to red) heightmap/intensity/... ?</param>
		public Xwt.Drawing.Image GetAsImage(string scanType, bool colorized, bool saveImage = true)
		{
			lock (asyncImageLock) {
				var currentRenderDict = colorized ? renderedColorImage : renderedImage;

				if (!currentRenderDict.ContainsKey(scanType) || currentRenderDict[scanType] == null || !saveImage) {
					MemoryStream mStream = GetAsMemoryStream(scanType, colorized);
					XD.Image img = XD.Image.FromStream(mStream).WithBoxSize(requestedBitmapSize);
					mStream.Dispose();

					if(!saveImage) {
						return img;
					}

					currentRenderDict[scanType] = img;
				}
			}
			return colorized ? 
				renderedColorImage[scanType].WithBoxSize(requestedBitmapSize) : 
				renderedImage[scanType].WithBoxSize(requestedBitmapSize);
		}

		/// <summary>
		/// Gets image as memory stream.
		/// </summary>
		/// <returns>The memory stream.</returns>
		/// <param name="scanType">Scan type.</param>
		/// <param name="colorized">Colorize (green to red) heightmap/intensity/... ?</param>>
		public MemoryStream GetAsMemoryStream(string scanType, bool colorized)
		{
			MemoryStream memoryStream = new MemoryStream();
			Bitmap bmp;
			if (colorized) {
				bmp = GetAsColorizedBitmap(scanType);
			} else {
				bmp = GetAsBitmap(scanType);
			}
			if (bmp == null) {
				// TODO raise error
				return null;
			}
				
			bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			bmp.Dispose();
			memoryStream.Position = 0;

			return memoryStream;
		}

		/// <summary>
		/// For xml (de-)serializer only!
		/// Gets the metadata.
		/// </summary>
		/// <value>The metadata.</value>
		[XmlIgnore]
		public Dictionary<string, float> Metadata {
			get {
				return metadata;
			}
		}

		private List<Metadata> internMetadataList = new List<Metadata>();
		/// <summary>
		/// For xml (de-)serializer only!
		/// Gets or sets the metadata.
		/// </summary>
		/// <value>The metadata.</value>
		[XmlArray("metadata")]
		[XmlArrayItem("datum")]
		public List<Metadata> InternMetadata {
			get {
				internMetadataList.Clear();
				foreach (var m in metadata) {
					internMetadataList.Add(new Metadata(m.Key, m.Value));
				}

				return internMetadataList;
			}
		}

		#endregion

		/// <summary>
		/// Gets as image (sync).
		/// </summary>
		/// <param name="scanType">Scan type.</param>
		/// <param name="callback">Function to call on finish.</param>
		/// <param name="colorized">Colorize (green to red) heightmap/intensity/... ?</param>
		public void GetAsImageAsync(string scanType, bool colorized, ImageLoadedCallback callback)
		{
			Task.Factory.StartNew( () => {
				try {
					XD.Image image = GetAsImage(scanType, colorized);
					Application.Invoke(() => callback(image.WithBoxSize(requestedBitmapSize)));
				} catch (Exception e) {
					Console.WriteLine(e.StackTrace);
					Console.WriteLine(e.Message);
				}
			});
		}

		/// <summary>
		/// Generates thumbnails of every scan type from this scan.
		/// Saves them to project file (if there is any).
		/// </summary>
		/// <returns>The thumbnails.</returns>
		public virtual XD.Image[] GetThumbnails()
		{
			if (thumbnails != null) {
				return thumbnails;
			}

			if (isLoadingThumbnail) {
				return null;
			}

			isLoadingThumbnail = true;
			XD.Image[] lThumbnails = 
				Project.RequestZipAccess(
					new Project.ZipUsageCallback(GetThumbnails)
				) as XD.Image[];

			if (lThumbnails != null) {
				thumbnails = lThumbnails;
			}

			isLoadingThumbnail = false;
			return thumbnails;
		}

		/// <summary>
		/// Generates thumbnails of every scan type from this scan.
		/// Saves them to project file (if there is any).
		/// </summary>
		/// <returns>The thumbnails.</returns>
		/// <param name="zipFile">Zipfile handler.</param>
		public virtual XD.Image[] GetThumbnails(ZipFile zipFile)
		{
			XD.Image[] ret = new XD.Image[AvailableScanTypes().Length];

			int i = -1;
			foreach (string scanType in AvailableScanTypes()) {
				i++;

				if(zipFile != null) {
					ZipEntry maskEntry = zipFile.GetEntry(String.Format("thumbnails/{0}_{1}.png", Name, scanType));
					if(maskEntry != null) {
						Stream previewStream = zipFile.GetInputStream(maskEntry);
						ret[i] = XD.Image.FromStream(previewStream);
						continue;
					}
				}

				XD.Image newImage = GetAsImage(scanType, false);

				Xwt.Drawing.BitmapImage newRenderedImage = newImage.WithBoxSize(96).ToBitmap();
				newImage.Dispose();

				if(zipFile != null) {
					MemoryStream mStream = new MemoryStream();
					newRenderedImage.Save(mStream, Xwt.Drawing.ImageFileType.Png);
					mStream.Position = 0;
					CustomStaticDataSource source = new CustomStaticDataSource(mStream);

					zipFile.BeginUpdate();
					zipFile.Add(source, String.Format("thumbnails/{0}_{1}.png", Name, scanType));
					zipFile.IsStreamOwner = true;
					zipFile.CommitUpdate();
				}

				ret[i] = newRenderedImage;
			}

			if (ret != null) {
				thumbnails = ret;
			}

			return ret;
		}

		/// <summary>
		/// Get thumbnail of a specified fiber type.
		/// </summary>
		/// <returns>The thumbnail.</returns>
		/// <param name="scantype">Scan type.</param>
		public virtual XD.Image GetThumbnail(string scantype)
		{
			XD.Image[] thumbList = GetThumbnails();
			if (thumbList != null) {
				int i = 0;
				foreach (string st in AvailableScanTypes()) {
					if (st == scantype) {
						break;
					}
					i++;
				}
				return thumbList[i];
			}

			return null;
		}

		/// <summary>
		/// Notifies that something for this scan has changed.
		/// </summary>
		/// <param name="changeOf">Type of change</param>
		/// <param name="addToUnsaved">Add change to list of unsaved elements</param>
		/// <remarks>
		/// We need to save this data somewhere!
		/// </remarks>
		public virtual void NotifyChange(string changeOf, bool addToUnsaved = true)
		{
			if (isInitialized) {
				if (!string.IsNullOrEmpty(changeOf) && addToUnsaved) {
					unsaved.Add(changeOf);
				}

				if (scanDataChanged != null) {
					ScanDataEventArgs dataChangedEvent = new ScanDataEventArgs(changeOf, unsaved);
					scanDataChanged(this, dataChangedEvent);
				}
			}
		}

		/// <summary>
		/// Notifies that an attribute was saved.
		/// </summary>
		/// <param name="changeOf">Type of change</param>
		public void NotifySaved(string changeOf)
		{
			unsaved.Remove(changeOf);

			if (scanDataChanged != null) {
				ScanDataEventArgs dataChangedEvent = new ScanDataEventArgs(changeOf, unsaved);
				scanDataChanged(this, dataChangedEvent);
			}
		}

		/// <summary>
		/// Save data and marka this scan as saved.
		/// </summary>
		public void Save()
		{
			HashSet<string> unsavedCopy = new HashSet<string>(unsaved);
			foreach (string toSave in unsavedCopy) {
				if (toSave == "mask") {
					mask.Save();
				}
			}

			if (mask.MaskPositions.Count > 1) {
				mask.Save();
			}

			if (unsaved.Count > 0) {
				unsaved.Clear();
				NotifyChange("");
			}
		}

		/// <summary>
		/// Determines whether this scan has all needed user input set.
		/// </summary>
		/// <returns><c>true</c> if this scan is finish; otherwise, <c>false</c>.</returns>
		public bool IsFinish()
		{
			return HasMask && metadata.ContainsKey("LensMagnification") && FiberType != "Unknown";
		}

		/// <summary>
		/// Scales the render size of all images
		/// </summary>
		/// <param name="scaleFactor">Scale factor.</param>
		/// <remarks>
		/// Call GetAsImage again, to get image with correct size
		/// </remarks>
		public void ScaleImage(double scaleFactor)
		{
			requestedBitmapSize.Height *= scaleFactor;
			requestedBitmapSize.Width *= scaleFactor;
		}

		public override string ToString()
		{
			return Name;
		}

		[XmlAttribute("name")]
		public string Name {
			get {
				if (string.IsNullOrEmpty(name)) {
					return Path.GetFileNameWithoutExtension(filePath);
				}
				return name;
			}
			set {
				if (name != value) {
					name = value;
					NotifyChange("Name");
				}
			}
		}

		[XmlAttribute("filepath")]
		public string FilePath {
			get {
				return filePath;
			}
			set {
				Initialize(value, false);
			}
		}

		/// <summary>
		/// Gets or sets the type of the fiber.
		/// E.g. "acryl"
		/// </summary>
		/// <value>The type of the fiber.</value>
		[XmlElement("fiberType")]
		public string FiberType {
			get {
				return string.IsNullOrEmpty(fiberType) ? "Unknown" : fiberType;
			}
			set {
				if (fiberType != value) {
					fiberType = value;
					NotifyChange("FiberType");
				}
			}
		}

		[XmlElement("size")]
		public Xwt.Size Size {
			get { 
				return size;
			}
			set {
				size = value;
			}
		}

		public Xwt.Point GetScaleFactor()
		{
			return new Xwt.Point(
				Size.Width / RequestedBitmapSize.Width,
				Size.Height / RequestedBitmapSize.Height
			);
		}

		[XmlIgnore]
		public HashSet<string> Unsaved {
			get {
				return new HashSet<string>(unsaved);
			}
		}
			
		[XmlElement("requestedSize")]
		public Xwt.Size RequestedBitmapSize {
			get { 
				return requestedBitmapSize;
			}
			set {
				requestedBitmapSize = value;
			}
		}
			
		[XmlIgnore]
		public Mask Mask {
			get {
				return mask;
			}
		}

		[XmlIgnore]
		public bool HasMask {
			get;
			set;
		}

		/// <summary>
		/// Determines whether this scan is scaled.
		/// </summary>
		/// <returns><c>true</c> if this instance is scaled; otherwise, <c>false</c>.</returns>
		public bool IsScaled()
		{
			const double EPSILON = 0.1;
			if (Math.Abs(requestedBitmapSize.Height - size.Height) > EPSILON ||
			    Math.Abs(requestedBitmapSize.Width - size.Width) > EPSILON) {
				return true;
			}
			return false;
		}

		public bool HasUnsaved()
		{
			return unsaved.Count > 0;
		}

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

		public Widget ToWidget()
		{
			throw new NotImplementedException();
		}

		public object RawData()
		{
			throw new NotImplementedException();
		}
			
		public void Dispose()
		{
			if (renderedImage != null) {
				foreach (XD.Image image in renderedImage.Values) {
					image.Dispose();
				}
				renderedImage.Clear();
			}

			if (thumbnails != null) {
				foreach (XD.Image thumb in thumbnails) {
					thumb.Dispose();
				}
				thumbnails = null;
			}
		}

		#region IComparer implementation

		public int Compare(BaseScan x, BaseScan y)
		{
			if (x != null && y != null) {
				return string.Compare(x.FiberType, y.FiberType, StringComparison.Ordinal);
			}

			return 0;
		}

		#endregion
	}
}

