using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

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

