//
//  ScanDataEventArgs.cs
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

namespace Baimp
{
	public class ScanDataEventArgs : EventArgs
	{
		HashSet<string> unsaved;
		readonly string changed;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.ScanDataEventArgs"/> class.
		/// </summary>
		/// <param name="unsaved">List of unsaved elements</param>
		/// <param name="changed"></param>
		public ScanDataEventArgs(string changed, HashSet<string> unsaved = null)
		{
			this.changed = changed;
			this.unsaved = unsaved;
		}

		public HashSet<string> Unsaved { 
			get {
				return unsaved;
			}
		}

		public string Changed { 
			get {
				return changed;
			}
		}
	}
}

