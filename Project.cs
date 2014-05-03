//
//  Project.cs
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

		static Object zipFileAccess = new Object();
		public readonly static int MaxLastOpenedProject = 5;
		[XmlArray("scans")]
		[XmlArrayItem("scan")]
		public ScanCollection scanCollection;
		[XmlAttribute]
		public int version = 3;
		[XmlIgnore]
		bool pipelineRunning;

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
		/// Use this method to access the save file.
		/// This is the only thread safe method!
		/// </summary>
		/// <param name="callback">Function to run.</param>
		public static object RequestZipAccess(ZipUsageCallback callback)
		{
			object ret = null;
			lock (zipFileAccess) {
				if (!string.IsNullOrEmpty(ProjectFile)) {
					try {
						ZipFile zipFile;
						if (File.Exists(ProjectFile)) {
							zipFile = new ZipFile(ProjectFile);
						} else {
							zipFile = ZipFile.Create(ProjectFile);
							zipFile.BeginUpdate();
							zipFile.CommitUpdate();
							zipFile.Close();
							zipFile = new ZipFile(ProjectFile);
						}
						using (zipFile) {
							ret = callback(zipFile);
						}
					} catch (Exception e) {
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
						if (e.InnerException != null) {
							Console.WriteLine(e.InnerException.Message);
						}
						Log.Add(LogLevel.Error, "Project", "Failed to open save file.\n\t" + e.Message);
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
					if (metadata == null) {
						return false;
					}

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
							ErrorMessage = e.Message + "\n" + e.InnerException.Message;
							Log.Add(LogLevel.Error, this.GetType().Name, "Failed to deserialize save file.\n" + ErrorMessage);
							return false;
						}

						version = p.version;
						LoadedPipelines.Clear();
						LoadedPipelines = p.LoadedPipelines;

						Dictionary<int, MarkerNode> allNodes = new Dictionary<int, MarkerNode>();
						int noNodes = 0;
						foreach (PipelineNodeWrapper wrapper in LoadedPipelines) {
							foreach (PipelineNode pNode in wrapper.pNodes) {
								pNode.InitializeNodes(this);

								foreach (BaseOption option in pNode.InternOptions) {
									var localOption = option;
									BaseOption targetOption = 
										pNode.algorithm.Options.Find((BaseOption o) => o.Name == localOption.Name);
									if (targetOption != null) {
										targetOption.Value = 
											Convert.ChangeType(option.Value, targetOption.Value.GetType());
									}
								}
									
								foreach (MarkerNode mNode in pNode.mNodes) {
									allNodes.Add(mNode.ID, mNode);
								}

								noNodes++;
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
							scanCollection.AddRange(p.scanCollection);
							foreach (BaseScan scan in this.scanCollection) {
								if (zipFile.FindEntry(scan.Mask.MaskFilename, false) != -1) {
									scan.HasMask = true;
								}
								scan.OnXmlDeserializeFinish();
							}
						} else {
							this.scanCollection = new ScanCollection();
						}

						if (projectChanged != null) {
							projectChanged(this, new ProjectChangedEventArgs(true));
						}

						Log.Add(LogLevel.Info, this.GetType().Name,
							string.Format("\"{0}\" was loaded successfully. #Scans: {1}; #Worksheets: {2}; #Nodes: {3}",
								Path.GetFileName(filePath), scanCollection.Count, LoadedPipelines.Count, noNodes));

						return true;
					}
				}));

				if (!ret) {
					Log.Add(LogLevel.Error, this.GetType().Name, 
						string.Format(
							"Opening file \"{0}\" failed!" + (string.IsNullOrEmpty(ErrorMessage) ? "": "\n{1}")
							, ProjectFile, ErrorMessage));
					return false;
				}
			} else {
				ErrorMessage = "File not found";
				Log.Add(LogLevel.Error, this.GetType().Name, 
								string.Format("Opening failed. File \"{0}\" not found.", ProjectFile));
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

			List<string> allNewFiles = new List<string>();
			foreach (Type importer in importers) {
				BaseScan instance = Activator.CreateInstance(importer) as BaseScan;

				if (instance == null) {
					// TODO error handling
				} else {
					string fileExtension = instance.SupportedFileExtensions();
					string[] newFiles = Directory.GetFiles(path, fileExtension, SearchOption.AllDirectories);
					allNewFiles.AddRange(newFiles);

					scanCollection.AddFiles(new List<string>(newFiles), importer);
				}
			}
			projectChanged(this, new ProjectChangedEventArgs(allNewFiles.ToArray()));
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
				PipelineNodeWrapper wrapper = new PipelineNodeWrapper(pipeline);
				LoadedPipelines.Add(wrapper);
				foreach (PipelineNode pNode in pipeline.Nodes) {
					pNode.InternOptions = pNode.algorithm.Options;
				}

				if (pipeline == pipelineController.CurrentPipeline) {
					wrapper.active = true;
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

			Log.Add(LogLevel.Info, this.GetType().Name, 
				string.Format("Project \"{0}\" saved.", Path.GetFileName(ProjectFile)));

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
					ErrorMessage = "Invalid file path.";
					Log.Add(LogLevel.Error, this.GetType().Name, ErrorMessage);
					return false;
				}

				if (Path.GetExtension(filename) != "baimp") {
					filename = Path.GetDirectoryName(filename) + "/" + Path.GetFileNameWithoutExtension(filename) + ".baimp";
				}

				Project.ProjectFile = filename;

				using (ZipFile zipFile = ZipFile.Create(ProjectFile)) {
					zipFile.BeginUpdate();
					zipFile.CommitUpdate();
					zipFile.Close();
				}

				if (reset) {
					LoadedPipelines = new List<PipelineNodeWrapper>();
					LoadedPipelines.Add(new PipelineNodeWrapper("Master"));

					scanCollection.Clear();
					if (projectChanged != null) {
						projectChanged(this, new ProjectChangedEventArgs(true));
					}
				}

				return true;
			}

			return false;
		}

		#endregion

		#region internal save/open methods

		CustomStaticDataSource ProjectMetadata()
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
				typeof(BaseScan),
				typeof(BaseOption)
			};

			XmlSerializer serializer = new XmlSerializer(this.GetType(), extraTypes);
			serializer.Serialize(xmlWriter, this);

			xmlWriter.WriteEndDocument();
			ms.Position = 0;
			
			return new CustomStaticDataSource(ms);
		}

		#endregion

		public void NotifyPipelineStart(PipelineView pipeline)
		{
			Log.Add(LogLevel.Info, this.GetType().Name,
				"Start executing pipeline! Worksheet \"" + pipeline.PipelineName + "\".");
			if (pipelineExecuted != null) {
				pipelineExecuted(pipeline, null);
			}
		}

		public void NotifyPipelineStop(PipelineView pipeline)
		{
			Log.Add(LogLevel.Info, this.GetType().Name,
				"Stop executing pipeline! Worksheet \"" + pipeline.PipelineName + "\".");
			if (pipelineFinished != null) {
				pipelineFinished(pipeline, null);
			}
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

		EventHandler<EventArgs> pipelineExecuted;

		/// <summary>
		/// Occurs when we start executing the a pipeline
		/// </summary>
		public event EventHandler<EventArgs> PipelineExecuted {
			add {
				pipelineExecuted += value;
			}
			remove {
				pipelineExecuted -= value;
			}
		}

		EventHandler<EventArgs> pipelineFinished;

		/// <summary>
		/// Occurs when we pipeline execution finished
		/// </summary>
		public event EventHandler<EventArgs> PipelineFinished {
			add {
				pipelineFinished += value;
			}
			remove {
				pipelineFinished -= value;
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

		List<PipelineNodeWrapper> loadedPipelines = new List<PipelineNodeWrapper>();

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

		public bool IsPipelineRunning {
			get {
				return pipelineRunning;
			}
			set {
				pipelineRunning = value;
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
		[XmlAttribute("scrollX")]
		public double scrollX;
		[XmlAttribute("scrollY")]
		public double scrollY;
		[XmlAttribute("active")]
		public bool active;

		public PipelineNodeWrapper()
		{
		}

		public PipelineNodeWrapper(string name)
		{
			this.name = name;
		}

		public PipelineNodeWrapper(PipelineView pipeline)
		{
			name = pipeline.PipelineName;
			pNodes = pipeline.Nodes;

			scrollX = pipeline.Scrollview.HorizontalScrollControl.Value;
			scrollY = pipeline.Scrollview.VerticalScrollControl.Value;
		}
	}
}

