using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Xwt;
using System.Xml.Serialization;
using baimp.Properties;
using System.Collections.Specialized;

namespace baimp
{
	[XmlRoot("project")]
	public class Project
	{
		public readonly static int MaxLastOpenedProject = 5;

		[XmlAttribute]
		public int version = 1;
		private List<string> files = new List<string>();
		private List<PipelineNode> loadedNodes;

		#region initialize

		public Project()
		{
		}

		public Project(string filePath)
		{
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
		public void Open(string filePath)
		{
			this.FilePath = Path.GetFullPath(filePath);


			StringCollection last = Settings.Default.LastOpenedProjects;
			if (last == null) { 
				last = new StringCollection();
				Settings.Default.LastOpenedProjects = last;
			} else {
				if (last.Contains(FilePath)) {
					last.Remove(FilePath);
				} else if (last.Count >= MaxLastOpenedProject) {
					last.RemoveAt(0);
				}
			}

			last.Add(FilePath);
			Settings.Default.LastOpenedProjects = last;
			Settings.Default.Save();

			if (File.Exists(FilePath)) {
				using (XmlTextReader xmlReader = new XmlTextReader(FilePath)) {

					XmlSerializer deserializer = new XmlSerializer(this.GetType());
					Project p = (Project) deserializer.Deserialize(xmlReader);

					this.Files = p.Files;
					this.version = p.version;
					this.LoadedNodes = p.LoadedNodes;

					Dictionary<int, MarkerNode> allNodes = new Dictionary<int, MarkerNode>();
					foreach (PipelineNode pNode in p.LoadedNodes) {
						pNode.Initialize();

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

					if (projectChanged != null) {
						projectChanged(this, new ProjectChangedEventArgs(true));
					}
				}

			}
		}

		/// <summary>
		/// Import the scans from specified folder.
		/// </summary>
		/// <param name="path">Path.</param>
		public void Import(string path)
		{
			string[] newFiles = Directory.GetFiles(path, "*.dd+", SearchOption.AllDirectories);
			files.AddRange(newFiles);

			projectChanged(this, new ProjectChangedEventArgs(newFiles));
		}

		#endregion

		#region saving

		/// <summary>
		/// Save project file
		/// </summary>
		public bool Save(PipelineView pipeline)
		{
			if (FilePath == null) {
				if (!NewDialog()) {
					return false;
				}
			}

			this.loadedNodes = pipeline.Nodes;
			using (XmlTextWriter xmlWriter = new XmlTextWriter(FilePath, null)) {
				xmlWriter.Formatting = Formatting.Indented;

				var extraTypes = new[] {
					typeof(PipelineNode),
					typeof(MarkerNode),
					typeof(List<MarkerNode>),
					typeof(MarkerEdge),
					typeof(Edge),
					typeof(Node)
				};

				XmlSerializer serializer = new XmlSerializer(this.GetType(), extraTypes);
				serializer.Serialize(xmlWriter, this);

				xmlWriter.WriteEndDocument();
				xmlWriter.Close();
			}

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

				Open(openDialog.FileName);
				if (projectChanged != null) {
					projectChanged(this, new ProjectChangedEventArgs(true));
				}

				return Files.Count > 0;
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
					return false;
				}

				if (Path.GetExtension(filename) != "baimp") {
					filename = Path.GetDirectoryName(filename) + "/" + Path.GetFileNameWithoutExtension(filename) + ".baimp";
				}

				this.FilePath = filename;
				return true;
			}

			return false;
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
		public string FilePath {
			get;
			set;
		}

		[XmlArray("files")]
		[XmlArrayItem("file")]
		public List<string> Files {
			get {
				return files;
			}
			set {
				files = value;
			}
		}

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

