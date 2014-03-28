using System;
using System.Drawing;
using System.IO;
using XD = Xwt.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Xwt;

namespace baimp
{
	[XmlInclude(typeof(DDPlusScan))]
	abstract public class BaseScan
	{
		public delegate void ImageLoadedCallback(XD.Image image);

		/// <summary>
		/// The file path.
		/// </summary>
		private string filePath;

		/// <summary>
		/// The type of the fiber.
		/// </summary>
		private string fiberType;

		/// <summary>
		/// List of unsaved elements
		/// </summary>
		protected HashSet<string> unsaved = new HashSet<string>();

		/// <summary>
		/// The size of the requested bitmap.
		/// </summary>
		protected Xwt.Size requestedBitmapSize;

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
		/// The masks.
		/// </summary>
		private Mask masks;

		public BaseScan()
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
		/// Get specified scan as byte buffer.
		/// </summary>
		/// <returns>The byte buffer.</returns>
		/// <param name="type">Type.</param>
		abstract public byte[] GetByteBuffer(string scanType);

		/// <summary>
		/// Gets scan as array.
		/// </summary>
		/// <returns>The specified scan as a plan float array.</returns>
		/// <param name="type">Type.</param>
		abstract public float[] GetAsArray(string scanType);

		/// <summary>
		/// Get scan as bitmap.
		/// </summary>
		/// <returns>The specified scan as a bitmap.</returns>
		/// <param name="type">Scantile</param>
		abstract public unsafe Bitmap GetAsBitmap(string scanType);

		/// <summary>
		/// Gets image as memory stream in tiff format
		/// </summary>
		/// <returns>The memory stream.</returns>
		/// <param name="type">Scan type.</param>
		abstract public MemoryStream GetAsMemoryStream(string scanType);

		/// <summary>
		/// Get as image.
		/// </summary>
		/// <returns>The as image.</returns>
		/// <param name="type">Type.</param>
		abstract public XD.Image GetAsImage(string scanType);

		/// <summary>
		/// Gets or sets the metadata.
		/// </summary>
		/// <value>The metadata.</value>
		abstract public List<Metadata> Metadata {
			get;
			set;
		}

		#endregion

		/// <summary>
		/// Gets as image (sync).
		/// </summary>
		/// <param name="type">Scan type.</param>
		/// <param name="callback">Function to call on finish.</param>
		public virtual void GetAsImageAsync(string scanType, ImageLoadedCallback callback)
		{
			Thread imageLoaderThread = new Thread(delegate() {
				XD.Image image = GetAsImage(scanType);

				Xwt.Application.Invoke(delegate() {
					callback(image.WithSize(requestedBitmapSize));
				});
			});
			imageLoaderThread.Start();
		}

		/// <summary>
		/// Notifies that something for this scan has changed.
		/// </summary>
		/// <param name="changeOf">Type of change</param>
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
			foreach (string toSave in unsaved) {
				if (toSave.StartsWith("mask_")) {
					string[] splitted = toSave.Split('_');
					if (splitted.Length >= 2) {
						masks.Save(splitted[1]);
					}
				}
			}

//			foreach (string scanType in AvailableScanTypes()) {
//				masks.Save(scanType);
//			}

			unsaved.Clear();
			NotifyChange("");
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

		[XmlIgnore]
		public string Name {
			get {
				return Path.GetFileNameWithoutExtension(filePath);
			}
		}

		[XmlElement("filepath")]
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
				return fiberType;
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
			if (requestedBitmapSize.Height != size.Height ||
				requestedBitmapSize.Width != size.Width) {
				return true;
			} else {
				return false;
			}
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

