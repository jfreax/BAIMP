using System;
using System.Drawing;
using System.IO;
using XD = Xwt.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Xwt;
using ICSharpCode.SharpZipLib.Zip;

namespace Baimp
{
	[XmlInclude(typeof(DDPlusScan))]
	[XmlInclude(typeof(VK4Scan))]
	abstract public class BaseScan
	{
		public delegate void ImageLoadedCallback(XD.Image image);
		private Object asyncImageLock = new Object();

		/// <summary>
		/// The file path.
		/// </summary>
		private string filePath;

		/// <summary>
		/// Name of the scan
		/// </summary>
		private string name = string.Empty;

		/// <summary>
		/// The type of the fiber.
		/// </summary>
		private string fiberType = string.Empty;

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
		/// Position in file tree view
		/// </summary>
		[XmlIgnore]
		public TreePosition position;

		/// <summary>
		/// Position of category item in file tree view
		/// </summary>
		[XmlIgnore]
		public TreePosition parentPosition;

		/// <summary>
		/// True after call to "Initialize".
		/// </summary>
		private bool isInitialized = false;

		/// <summary>
		/// Buffer of rendered images
		/// </summary>
		private Dictionary<string, XD.Image> renderedImage = new Dictionary<string, XD.Image>();

		/// <summary>
		/// The metadata.
		/// </summary>
		private List<Metadata> metadata = new List<Metadata>();

		/// <summary>
		/// The masks.
		/// </summary>
		private Mask masks;

		/// <summary>
		/// Needed for xml serializer
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
			this.masks = new Mask(this);

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
		/// <returns>The specified scan as a plan float array.</returns>
		/// <param name="scanType">Type.</param>
		abstract public UInt32[] GetAsArray(string scanType);

		/// <summary>
		/// Get scan as bitmap.
		/// </summary>
		/// <returns>The specified scan as a bitmap.</returns>
		/// <param name="scanType">Scantile</param>
		abstract public unsafe Bitmap GetAsBitmap(string scanType);

		/// <summary>
		/// Get as image.
		/// </summary>
		/// <returns>The as image.</returns>
		/// <param name="scanType">Type.</param>
		/// <param name="saveImage">Save the result?</param>
		public Xwt.Drawing.Image GetAsImage(string scanType, bool saveImage = true)
		{
			lock (asyncImageLock) {
				if (!renderedImage.ContainsKey(scanType) || renderedImage[scanType] == null) {
					MemoryStream mStream = GetAsMemoryStream(scanType);
					XD.Image img = XD.Image.FromStream(mStream).WithBoxSize(requestedBitmapSize);
					mStream.Dispose();

					if(!saveImage) {
						return img;
					}

					renderedImage[scanType] = img;
				}
			}
			return renderedImage[scanType].WithBoxSize(requestedBitmapSize);
		}

		/// <summary>
		/// Gets image as memory stream in tiff format
		/// </summary>
		/// <returns>The memory stream.</returns>
		/// <param name="scanType">Scan type.</param>
		public MemoryStream GetAsMemoryStream(string scanType)
		{
			MemoryStream memoryStream = new MemoryStream();
			Bitmap bmp = GetAsBitmap(scanType);
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
		/// Gets or sets the metadata.
		/// </summary>
		/// <value>The metadata.</value>
		[XmlArray("metadata")]
		[XmlArrayItem("datum")]
		virtual public List<Metadata> Metadata {
			get {
				return metadata;
			}
			set {
				metadata = value;
			}
		}

		#endregion

		/// <summary>
		/// Gets as image (sync).
		/// </summary>
		/// <param name="scanType">Scan type.</param>
		/// <param name="callback">Function to call on finish.</param>
		public virtual void GetAsImageAsync(string scanType, ImageLoadedCallback callback)
		{
			ManagedThreadPool.QueueUserWorkItem(o => {
				XD.Image image = GetAsImage(scanType);

				Application.Invoke(() => callback(image.WithBoxSize(requestedBitmapSize)));
			});
		}

		/// <summary>
		/// Generates thumbnails of every scan type from this scan.
		/// Saves them to project file (if there is any).
		/// </summary>
		/// <returns>The thumbnails.</returns>
		public virtual XD.Image[] GenerateThumbnails()
		{
			XD.Image[] thumbnails = Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
				XD.Image[] ret = new XD.Image[AvailableScanTypes().Length];

				int i = -1;
				foreach (string scanType in AvailableScanTypes()) {
					i++;

					if(zipFile != null) {
						ZipEntry maskEntry = zipFile.GetEntry(String.Format("thumbnails/{0}_{1}.png", Name, scanType));
						if(maskEntry != null) {
							Stream previewStream = zipFile.GetInputStream(maskEntry);
							ret[i] = XD.Image.FromStream(previewStream);
							break;
						}
					}

					XD.Image newImage = GetAsImage(scanType, false);

					Xwt.Drawing.BitmapImage newRenderedImage = newImage.WithBoxSize(96).ToBitmap();
					newImage.Dispose();

					MemoryStream mStream = new MemoryStream();
					newRenderedImage.Save(mStream, Xwt.Drawing.ImageFileType.Png);
					mStream.Position = 0;
					CustomStaticDataSource source = new CustomStaticDataSource(mStream);

					if(zipFile != null) {
						zipFile.BeginUpdate();
						zipFile.Add(source, String.Format("thumbnails/{0}_{1}.png", Name, scanType));
						zipFile.IsStreamOwner = true;
						zipFile.CommitUpdate();
					}

					ret[i] = newRenderedImage;
				}
				return ret;

			})) as XD.Image[];

			return thumbnails;
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
				if (toSave.StartsWith("mask_", StringComparison.Ordinal)) {
					string[] splitted = toSave.Split('_');
					if (splitted.Length >= 2) {
						masks.Save(splitted[1]);
					}
				}
			}
			if (unsaved.Count > 0) {
				unsaved.Clear();
				NotifyChange("");
			}
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
				name = value;
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
		public Mask Masks {
			get {
				return masks;
			}
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
	}
}

