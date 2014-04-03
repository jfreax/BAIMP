using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Xwt;
using System.Xml.Serialization;
using System.Collections.Specialized;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;

namespace Baimp
{
	[XmlRoot("project")]
	public class Project
	{
		public delegate object ZipUsageCallback(ZipFile zipFile);
		private static Object zipFileAccess = new Object();

		public readonly static int MaxLastOpenedProject = 5;

		[XmlArray("scans")]
		[XmlArrayItem("scan")]
		public ScanCollection scanCollection;

		[XmlAttribute]
		public int version = 3;

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

		#region global methos

		/// <summary>
		/// Use this method to access the savegame.
		/// This is the only thread safe method!
		/// </summary>
		/// <param name="callback">Function to run.</param>
		public static object RequestZipAccess(ZipUsageCallback callback)
		{
			object ret = true;
			lock (zipFileAccess) {
				if (!string.IsNullOrEmpty(ProjectFile)) {
					ZipFile zipFile;
					if (File.Exists(ProjectFile)) {
						zipFile = new ZipFile(ProjectFile);
					} else {
						zipFile = ZipFile.Create(ProjectFile);
					}
					using (zipFile) {
						ret = callback(zipFile);
					}

				} else {
					ret = callback(null);
				}
			}

			return ret;
		}

		#endregion

		#region opening

		/// <summary>
		/// Open the specified file.
		/// </summary>
		/// <param name="filePath">File path.</param>
		public bool Open(string filePath)
		{
			Project.ProjectFile = Path.GetFullPath(filePath);

			if (File.Exists(ProjectFile)) {
				bool ret = (bool) RequestZipAccess(new ZipUsageCallback(delegate(ZipFile zipFile) {
					ZipEntry metadata = zipFile.GetEntry("metadata.xml");
					Stream metadataStream = zipFile.GetInputStream(metadata);

					using (XmlTextReader xmlReader = new XmlTextReader(metadataStream)) {
						Project p;

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

						version = p.version;
						LoadedPipelines.Clear();
						LoadedPipelines = p.LoadedPipelines;

						Dictionary<int, MarkerNode> allNodes = new Dictionary<int, MarkerNode>();
						foreach (PipelineNodeWrapper wrapper in LoadedPipelines) {
							foreach (PipelineNode pNode in wrapper.pNodes) {
								pNode.Initialize();

								foreach (Option option in pNode.InternOptions) {
									var localOption = option;
									Option targetOption = pNode.algorithm.Options.Find((Option o) => o.name == localOption.name);
									targetOption.Value = Convert.ChangeType(option.Value, targetOption.Value.GetType()) as IComparable;
								}

								foreach (MarkerNode mNode in pNode.mNodes) {
									allNodes.Add(mNode.ID, mNode);
								}
							}
						}

						foreach (PipelineNodeWrapper wrapper in LoadedPipelines) {
							foreach (PipelineNode pNode in wrapper.pNodes) {
								foreach (MarkerNode mNode in pNode.mNodes) {
									foreach (Edge edge in mNode.Edges) {
										edge.to = allNodes[edge.ToNodeID];
									}
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

						return true;
					}
				}));

				if(!ret) {
					return false;
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

				if (instance == null) {
					// TODO error handling
				} else {
					string fileExtension = instance.SupportedFileExtensions();
					string[] newFiles = Directory.GetFiles(path, fileExtension, SearchOption.AllDirectories);

					scanCollection.AddFiles(new List<string>(newFiles), importer);
					projectChanged(this, new ProjectChangedEventArgs(newFiles));
				}
			}
		}

		#endregion

		#region saving

		/// <summary>
		/// Save project file
		/// </summary>
		public bool Save(PipelineController pipelineController)
		{
			if (ProjectFile == null) {
				if (!NewDialog(false)) {
					return false;
				}
			}

			PipelineCollection pipelines = pipelineController.Pipelines;

			// save scan metadata
			scanCollection.SaveAll();

			// prepare data
			LoadedPipelines.Clear();
			foreach (PipelineView pipeline in pipelines.Values) {
				PipelineNodeWrapper wrapper = new PipelineNodeWrapper(pipeline.PipelineName, pipeline.Nodes);
				LoadedPipelines.Add(wrapper);
				foreach (PipelineNode pNode in pipeline.Nodes) {
					pNode.InternOptions = pNode.algorithm.Options;
				}
			}

			// save metadata
			Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
				zipFile.BeginUpdate();

				zipFile.Add(ProjectMetadata(), "metadata.xml");

				zipFile.IsStreamOwner = true;
				zipFile.CommitUpdate();

				return null;
			}));

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

		public bool NewDialog(bool reset = true)
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

				Project.ProjectFile = filename;

                if (reset) {
					this.LoadedPipelines = new List<PipelineNodeWrapper>();

                    scanCollection.Clear();
                    if (projectChanged != null)
                    {
                        projectChanged(this, new ProjectChangedEventArgs(true));
                    }
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
		static public string ProjectFile {
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

		private List<PipelineNodeWrapper> loadedPipelines = new List<PipelineNodeWrapper>();

		[XmlArray("worksheets")]
		[XmlArrayItem("worksheet")]
		public List<PipelineNodeWrapper> LoadedPipelines {
			get {
				return loadedPipelines;
			}
			set {
				loadedPipelines = value;
			}
		}

		#endregion
	}

	public class PipelineNodeWrapper
	{
		[XmlArray("pipeline")]
		[XmlArrayItem("node")]
		public List<PipelineNode> pNodes;

		[XmlAttribute("name")]
		public string name;

		public PipelineNodeWrapper() {}

		public PipelineNodeWrapper(string pipelineName, List<PipelineNode> nodes)
		{
			name = pipelineName;
			pNodes = nodes;
		}
	}
}

