//
//  BaseAlgorithm.cs
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
using System.Collections.Generic;
using Xwt;
using System.Threading;

namespace Baimp
{
	public enum AlgorithmType
	{
		Input,
		Output,
		Filter,
		Descriptor,
		Extraction,
		Misc
	}

	public enum RequestType
	{
		ScanCollection
	}

	abstract public class BaseAlgorithm
	{
		public readonly PipelineNode Parent;

		/// <summary>
		/// Input data types, their properties and contraints.
		/// </summary>
		protected List<Compatible> input;

		/// <summary>
		/// Output data types, their properties and contraints.
		/// </summary>
		protected List<Compatible> output;

		/// <summary>
		/// List of data we want from the program
		/// </summary>
		protected HashSet<RequestType> request;

		/// <summary>
		/// List of all options
		/// </summary>
		public List<BaseOption> options;

		/// <summary>
		/// The cancellation token.
		/// </summary>
		public CancellationToken cancellationToken;


		internal BaseAlgorithm(PipelineNode parent, ScanCollection scanCollection)
		{
			this.Parent = parent;

			input = new List<Compatible>();
			output = new List<Compatible>();
			request = new HashSet<RequestType>();
			options = new List<BaseOption>();
		}

		/// <summary>
		/// Executes the algorithm.
		/// </summary>
		/// <param name="requestedData">Requested data.</param>
		/// <param name="options">User setted options</param>
		/// <param name="inputArgs">Input arguments.</param>
		/// <remarks>
		/// Return null, when no more data is available (important for sequential data output).
		/// Use Yield() function to return data when you want to output more then one result per input.
		/// </remarks>
		abstract public IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs);

		abstract public AlgorithmType AlgorithmType {
			get;
		}

		/// <summary>
		/// Gets the help text.
		/// </summary>
		/// <value>The help text.</value>
		abstract public string HelpText{
			get;
		}

		/// <summary>
		/// Text shown on top of node in pipeline graph.
		/// </summary>
		abstract public string Headline();

		/// <summary>
		/// String used to represent this node in exported data.
		/// Should be short and without any spaces, new lines and special characters.
		/// </summary>
		/// <returns>The name.</returns>
		abstract public string ShortName();

		/// <summary>
		/// Text shown in algorithm tree viewer.
		/// </summary>
		public override string ToString()
		{
			return this.GetType().Name;
		}

		#region do not override

		/// <summary>
		/// Yield the specified data array.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="inputRef">Reference to input data that was used to compute the result.</param>
		protected void Yield(IType[] data, params IType[] inputRef)
		{
			int threadID = Thread.CurrentThread.ManagedThreadId;
			if (yielded[threadID] != null) {
				var lYield = yielded[threadID];
					try {
						lYield(this, new AlgorithmEventArgs(data, inputRef));
					} catch (Exception e) {
						Console.WriteLine(e.StackTrace);
					}
			}
		}

		/// <summary>
		/// Sets the progress
		/// </summary>
		/// <param name="percent">Progress 0-100.</param>
		public void SetProgress(int percent)
		{
			int threadID = Thread.CurrentThread.ManagedThreadId;
			Application.Invoke( () => Parent.SetProgress(threadID, percent));
		}

		public bool IsCanceled {
			get {
				if (cancellationToken.IsCancellationRequested) {
					return true;
				}

				return false;
			}
		}

		#endregion

		#region events

		Dictionary<int, EventHandler<AlgorithmEventArgs>> yielded = 
			new Dictionary<int, EventHandler<AlgorithmEventArgs>>();

		private object yield_lock = new object();

		/// <summary>
		/// Yield sequential data
		/// </summary>
		public event EventHandler<AlgorithmEventArgs> Yielded {
			add {
				lock(yield_lock) {
					int threadID = Thread.CurrentThread.ManagedThreadId;
					if (!yielded.ContainsKey(threadID)) {
						yielded[threadID] = null;
					} 
					yielded[threadID] += value;
				}
			}
			remove {
				lock (yield_lock) {
					yielded[Thread.CurrentThread.ManagedThreadId] -= value;
				}
			}
		}
			
		#endregion

		#region properties

		public List<Compatible> Input {
			get {
				return input;
			}
		}

		public List<Compatible> Output {
			get {
				return output;
			}
		}

		public HashSet<RequestType> Request {
			get {
				return request;
			}
		}

		public List<BaseOption> Options {
			get {
				return options;
			}
		}

		#endregion
	}
}
