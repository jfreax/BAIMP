using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Xwt;
using System.Xml.Serialization;

namespace baimp
{
	public class Project
	{
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
				XmlTextReader xmlReader = new XmlTextReader (filePath);

				while (xmlReader.Read ()) {
					switch (xmlReader.NodeType) {
					case XmlNodeType.Element:
						if (xmlReader.Name == "files") {
							while (xmlReader.Read ()) {
								if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "name") {
									files.Add (xmlReader.ReadString ());
								} else if (xmlReader.NodeType == XmlNodeType.EndElement) {
									break;
								}

							}
						} else if (xmlReader.Name == "pipeline") {
							while (xmlReader.Read ()) {
								if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "node") {
									if (xmlReader.AttributeCount >= 3) {
//										Console.WriteLine (xmlReader.GetAttribute (0) + xmlReader.ReadAttributeValue);
//										Console.WriteLine (xmlReader.GetAttribute (1) + xmlReader.ReadAttributeValue);
//										Console.WriteLine (xmlReader.GetAttribute (2) + xmlReader.ReadAttributeValue);
//										//PipelineNode node = new PipelineNode (algoType, new Rectangle (e.Position, PipelineNode.NodeSize));
										//loadedNodes.Add (xmlReader.ReadString ());
									}
								} else if (xmlReader.NodeType == XmlNodeType.EndElement) {
									break;
								}

							}
						}

						break;
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


			using (XmlTextWriter xmlWriter = new XmlTextWriter (FilePath, null)) {
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartDocument ();
				xmlWriter.WriteStartElement ("project");
				xmlWriter.WriteAttributeString ("version", "1.0");

				xmlWriter.WriteStartElement ("files");
				foreach (string file in files) {
					xmlWriter.WriteStartElement ("name");
					xmlWriter.WriteValue (file);
					xmlWriter.WriteEndElement ();
				}

				XmlPipeline pipe = new baimp.XmlPipeline ();
				pipe.nodes = new XmlNode[pipeline.Nodes.Count];

				int i = 0;
				foreach (PipelineNode pNode in pipeline.Nodes) {
					pipe.nodes [i] = (new XmlNode (i, pNode.bound.X, pNode.bound.Y, pNode.algorithm.GetType ().AssemblyQualifiedName));
					//pipe.nodes [i].edge = new XmlEdge[pNode.]
					i++;
				}
				XmlSerializer serializer = new XmlSerializer(typeof(baimp.XmlPipeline));
				serializer.Serialize (xmlWriter, pipe);


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

		string FilePath {
			get;
			set;
		}
			
		public List<string> Files {
			get {
				return files;
			}
		}

		public List<PipelineNode> LoadedNodes {
			get {
				return loadedNodes;
			}
		}
		#endregion
	}
}

