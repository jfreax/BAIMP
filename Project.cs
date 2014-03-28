using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Xwt;
using System.Xml.Serialization;
using baimp.Properties;
using System.Collections.Specialized;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;

namespace baimp
{
	[XmlRoot("project")]
	public class Project
	{
		public readonly static int MaxLastOpenedProject = 5;

		[XmlArray("scans")]
		[XmlArrayItem("scan")]
		public ScanCollection scanCollection;

		[XmlAttribute]
		public int version = 2;

		private List<PipelineNode> loadedNodes;

		#region initialize

		public Project()
		{
		}

		public Project(string filePath)
		{
			scanCollection = new ScanCollection();

			if (!string.IsNullOrEmpty(filePath)) {
				Open(filePath);
			}
		}

		#endregion

		#region opening

		/// <summary>
		/// Open the specified file.
		/// </summary>
		/// <param name="filePath">File path.</param>
		public bool Open(string filePath)
		{
			this.ProjectFile = Path.GetFullPath(filePath);

			if (File.Exists(ProjectFile)) {
				using (ZipFile zipFile = new ZipFile(ProjectFile)) {
					ZipEntry metadata = zipFile.GetEntry("metadata.xml");
					Stream metadataStream = zipFile.GetInputStream(metadata);

					using (XmlTextReader xmlReader = new XmlTextReader(metadataStream)) {
						Project p = null;

						try {
							XmlSerializer deserializer = new XmlSerializer(this.GetType());
							p = (Project) deserializer.Deserialize(xmlReader);
						} catch (Exception e) {
							Console.WriteLine(e);
							Console.WriteLine(e.Message);
							Console.WriteLine(e.InnerException.Message);
							this.ErrorMessage = e.Message + "\n" + e.InnerException.Message;
							return false;
						}

						this.version = p.version;
						this.LoadedNodes = p.LoadedNodes;

						Dictionary<int, MarkerNode> allNodes = new Dictionary<int, MarkerNode>();
						foreach (PipelineNode pNode in p.LoadedNodes) {
							pNode.Initialize();

							foreach (Option option in pNode._intern_Options) {
								Option targetOption = pNode.algorithm.Options.Find((Option o) => o.name == option.name);
								targetOption.Value = Convert.ChangeType(option.Value, targetOption.Value.GetType()) as IComparable;
							}

							foreach (MarkerNode mNode in pNode.mNodes) {
								allNodes.Add(mNode.ID, mNode);
							}
						}

						foreach (PipelineNode pNode in p.LoadedNodes) {
							foreach (MarkerNode mNode in pNode.mNodes) {
								foreach (Edge edge in mNode.Edges) {
									edge.to = allNodes[edge.ToNodeID];
								}
							}
						}

						if (p.scanCollection != null) {
							this.scanCollection = p.scanCollection;
							foreach (BaseScan scan in this.scanCollection) {
								scan.OnXmlDeserializeFinish();
							}
						} else {
							this.scanCollection = new ScanCollection();
						}
							
						if (projectChanged != null) {
							projectChanged(this, new ProjectChangedEventArgs(true));
						}
					}
				}
			} else {
				ErrorMessage = "File not found";
				return false;
			}

			StringCollection last = Settings.Default.LastOpenedProjects;
			if (last == null) { 
				last = new StringCollection();
				Settings.Default.LastOpenedProjects = last;
			} else {
				if (last.Contains(ProjectFile)) {
					last.Remove(ProjectFile);
				} else if (last.Count >= MaxLastOpenedProject) {
					last.RemoveAt(0);
				}
			}

			last.Add(ProjectFile);
			Settings.Default.LastOpenedProjects = last;
			Settings.Default.Save();

			return true;
		}

		/// <summary>
		/// Import the scans from specified folder.
		/// </summary>
		/// <param name="path">Path.</param>
		public void Import(string path)
		{
			Type baseType = typeof(BaseScan);
			IEnumerable<Type> importers = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(t => t.BaseType == baseType);

			foreach (Type importer in importers) {
				BaseScan instance = Activator.CreateInstance(importer) as BaseScan;

				string fileExtension = instance.SupportedFileExtensions();
				string[] newFiles = Directory.GetFiles(path, fileExtension, SearchOption.AllDirectories);

				scanCollection.AddFiles(new List<string>(newFiles), importer, true);
				projectChanged(this, new ProjectChangedEventArgs(newFiles));
			}
		}

		#endregion

		#region saving

		/// <summary>
		/// Save project file
		/// </summary>
		public bool Save(PipelineView pipeline)
		{
			if (ProjectFile == null) {
				if (!NewDialog()) {
					return false;
				}
			}

			// save scan metadata
			scanCollection.SaveAll();

			// prepare data
			this.loadedNodes = pipeline.Nodes;
			foreach (PipelineNode pNode in loadedNodes) {
				pNode._intern_Options = pNode.algorithm.Options;
			}

			ZipFile zipFile;
//			try {
				if (File.Exists(ProjectFile)) {
					zipFile = new ZipFile(ProjectFile);
				} else {
					zipFile = ZipFile.Create(ProjectFile);
				}

				using (zipFile) {
					zipFile.BeginUpdate();

					zipFile.Add(ProjectMetadata(), "metadata.xml");

					zipFile.IsStreamOwner = true;
					zipFile.CommitUpdate();

				} // closes also memorystream
//			} catch (Exception e) {
//				Console.WriteLine(e.Message);
//			}

			return true;
		}

		#endregion

		#region dialogs

		/// <summary>
		/// Show dialog to select folder to import scan
		/// </summary>
		public void ImportDialog()
		{
			SelectFolderDialog dlg = new SelectFolderDialog("Import folder");
			if (dlg.Run()) {
				foreach (string path in dlg.Folders) {
					Import(path);
				}
			}
		}

		public bool OpenDialog()
		{
			OpenFileDialog openDialog = new OpenFileDialog("Open Project");
			openDialog.Filters.Add(new FileDialogFilter("BAIMP Project file", "*.baimp"));
			if (openDialog.Run()) {
				if (string.IsNullOrEmpty(openDialog.FileName)) {
					return false;
				}

				if (!Open(openDialog.FileName)) {
					return false;
				}

				return scanCollection != null && scanCollection.Count > 0;
			}

			return false;
		}

		public bool NewDialog()
		{
			SaveFileDialog saveDialog = new SaveFileDialog("New Project");
			saveDialog.Filters.Add(new FileDialogFilter("BAIMP Project file", "*.baimp"));
			if (saveDialog.Run()) {
				string filename = saveDialog.FileName;
				if (string.IsNullOrEmpty(filename)) {
					ErrorMessage = "File not found";
					return false;
				}

				if (Path.GetExtension(filename) != "baimp") {
					filename = Path.GetDirectoryName(filename) + "/" + Path.GetFileNameWithoutExtension(filename) + ".baimp";
				}

				this.ProjectFile = filename;
				//this.Files = new List<string>();
				this.LoadedNodes = new List<PipelineNode>();

				scanCollection.Clear();
				if (projectChanged != null) {
					projectChanged(this, new ProjectChangedEventArgs(true));
				}

				return true;
			}

			return false;
		}

		#endregion

		#region internal save/open methods

		private CustomStaticDataSource ProjectMetadata()
		{
			MemoryStream ms = new MemoryStream();
			XmlTextWriter xmlWriter = new XmlTextWriter(ms, null);
			xmlWriter.Formatting = Formatting.Indented;

			var extraTypes = new[] {
				typeof(PipelineNode),
				typeof(MarkerNode),
				typeof(List<MarkerNode>),
				typeof(MarkerEdge),
				typeof(Edge),
				typeof(Node),
				typeof(ScanCollection),
				typeof(List<BaseScan>),
				typeof(BaseScan)
			};

			XmlSerializer serializer = new XmlSerializer(this.GetType(), extraTypes);
			serializer.Serialize(xmlWriter, this);

			xmlWriter.WriteEndDocument();
			ms.Position = 0;
			
			return new CustomStaticDataSource(ms);
		}

		#endregion

		#region custom events

		EventHandler<ProjectChangedEventArgs> projectChanged;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<ProjectChangedEventArgs> ProjectChanged {
			add {
				projectChanged += value;
			}
			remove {
				projectChanged -= value;
			}
		}

		#endregion

		#region properties

		[XmlIgnore]
		public string ProjectFile {
			get;
			set;
		}

		[XmlIgnore]
		public string ErrorMessage {
			get;
			set;
		}

//		[XmlArray("files")]
//		[XmlArrayItem("file")]
//		public List<string> Files {
//			get {
//				return files;
//			}
//			set {
//				files = value;
//			}
//		}

		[XmlArray("pipeline")]
		[XmlArrayItem("node")]
		public List<PipelineNode> LoadedNodes {
			get {
				return loadedNodes;
			}
			set {
				loadedNodes = value;
			}
		}

		#endregion
	}
}

