using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Xwt;
using System.Xml.Serialization;

namespace baimp
{
	[XmlRoot("project")]
	public class Project
	{
		[XmlAttribute]
		public int version = 1;

		private List<string> files;

		private List<PipelineNode> loadedNodes;

		#region Initialize

		public Project ()
		{
		}

		public Project (string filePath)
		{
			Initialize (filePath);
		}

		private void Initialize (string filePath) {
			this.files = new List<string> ();
			this.FilePath = filePath;

			if (File.Exists (filePath)) {
				using (XmlTextReader xmlReader = new XmlTextReader(filePath)) {

					XmlSerializer deserializer = new XmlSerializer(this.GetType());
					Project p = (Project) deserializer.Deserialize (xmlReader);

					this.Files = p.Files;
					this.version = p.version;
					this.LoadedNodes = p.LoadedNodes;

					Dictionary<int, MarkerNode> allNodes = new Dictionary<int, MarkerNode> ();
					foreach (PipelineNode pNode in p.LoadedNodes) {
						pNode.Initialize ();

						foreach (MarkerNode mNode in pNode.mNodes) {
							allNodes.Add (mNode.id, mNode);
						}
					}

					foreach (PipelineNode pNode in p.LoadedNodes) {
						foreach (MarkerNode mNode in pNode.mNodes) {
							foreach (MarkerEdge edge in mNode.Edges) {
								edge.to = allNodes [edge.ToNodeID];
							}
						}
					}

					if (projectChanged != null) {
						projectChanged (this, new ProjectChangedEventArgs (true));
					}
				}

			} else {
				CreateNewDocument ();
			}
		}

		#endregion

		/// <summary>
		/// Creates a new project file.
		/// </summary>
		void CreateNewDocument ()
		{
			using (XmlTextWriter xmlWriter = new XmlTextWriter (FilePath, null)) {
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartDocument ();
				xmlWriter.WriteStartElement ("files");
				xmlWriter.WriteEndDocument ();
				xmlWriter.Close ();
			}

		}

		/// <summary>
		/// Save project file
		/// </summary>
		public void Save (PipelineView pipeline)
		{
			this.loadedNodes = pipeline.Nodes;

			using (XmlTextWriter xmlWriter = new XmlTextWriter (FilePath, null)) {
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
				serializer.Serialize (xmlWriter, this);

				xmlWriter.WriteEndDocument ();

				xmlWriter.Close ();
			}
		}

		#region Dialogs

		/// <summary>
		/// Show dialog to select folder to import scan
		/// </summary>
		public void ImportDialog ()
		{
			SelectFolderDialog dlg = new SelectFolderDialog ("Import folder");
			if (dlg.Run ()) {
				foreach (string path in dlg.Folders) {
					Import (path);
				}
			}
		}

		public bool OpenDialog ()
		{
			OpenFileDialog openDialog = new OpenFileDialog ("Open Project");
			openDialog.Filters.Add (new FileDialogFilter ("BAIMP Project file", "*.baimp"));
			if (openDialog.Run ()) {
				Initialize (openDialog.FileName);
				if (projectChanged != null) {
					projectChanged (this, new ProjectChangedEventArgs (true));
				}
			}

			return Files.Count > 0;
		}

		public void NewDialog () {
			SaveFileDialog saveDialog = new SaveFileDialog ("New Project");
			saveDialog.Filters.Add (new FileDialogFilter ("BAIMP Project file", "*.baimp"));
			if (saveDialog.Run ()) {
				string filename = saveDialog.FileName;
				if (Path.GetExtension (filename) != "baimp") {
					filename = Path.GetDirectoryName (filename) + "/" + Path.GetFileNameWithoutExtension (filename) + ".baimp";
				}

				Initialize (filename);
			}
		}

		#endregion

		/// <summary>
		/// Import the scans from specified folder.
		/// </summary>
		/// <param name="path">Path.</param>
		public void Import(string path)
		{
			string[] newFiles = Directory.GetFiles(path, "*.dd+", SearchOption.AllDirectories);
			files.AddRange (newFiles);

			projectChanged (this, new ProjectChangedEventArgs (newFiles));
		}

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

		#region Properties

		[XmlIgnore]
		public string FilePath {
			get;
			set;
		}

		[XmlArray("files")]
		[XmlArrayItem("file")]
		public List<string> Files
		{
			get {
				return files;
			}
			set {
				files = value;
			}
		}

		[XmlArray("pipeline")]
		[XmlArrayItem("node")]
		public List<PipelineNode> LoadedNodes
		{
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

