//
//  ProjectChangedEventArgs.cs
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

namespace Baimp
{
	public class ProjectChangedEventArgs : EventArgs
	{
		public string[] addedFiles;
		public bool refresh = false;

		public ProjectChangedEventArgs(string[] addedFiles)
		{
			this.addedFiles = addedFiles;
		}

		public ProjectChangedEventArgs(bool refresh)
		{
			this.refresh = refresh;
		}
	}
}

