using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Xwt;

namespace baimp
{
	public class Project
	{
		private List<string> files;

		public Project (string filePath)
		{
			this.files = new List<string> ();
			this.FilePath = filePath;

			if (File.Exists (filePath)) {
				XmlTextReader xmlReader = new XmlTextReader (filePath);

				while (xmlReader.Read ()) {
					switch (xmlReader.NodeType) {
					case XmlNodeType.Element:
						if (xmlReader.Name == "Files") {
							while (xmlReader.Read ()) {
								if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Name") {
									files.Add (xmlReader.ReadString());
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

		/// <summary>
		/// Creates a new project file.
		/// </summary>
		void CreateNewDocument ()
		{
			using (XmlTextWriter xmlWriter = new XmlTextWriter (FilePath, null)) {
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartDocument ();
				xmlWriter.WriteStartElement ("Files");
				xmlWriter.WriteEndDocument ();
				xmlWriter.Close ();
			}

		}

		public void ImportDialog ()
		{
			SelectFolderDialog dlg = new SelectFolderDialog ("Import folder");
			if (dlg.Run ()) {
				foreach (string path in dlg.Folders) {
					Import (path);
				}
			}
		}

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
		#endregion
	}
}

