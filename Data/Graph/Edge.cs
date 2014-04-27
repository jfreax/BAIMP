//
//  Edge.cs
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
using Xwt.Drawing;
using System.Xml.Serialization;

namespace Baimp
{
	public class Edge
	{
		[XmlIgnore]
		public Node to;

		public Edge()
		{

		}

		public Edge(Node to)
		{
			this.to = to;
		}

		public virtual void Draw(Context ctx)
		{
		}

		#region properties

		/// <summary>
		/// ID of target node
		/// </summary>
		/// <value>To node I.</value>
		[XmlAttribute("to")]
		public int ToNodeID {
			get {
				if (to == null) {
					return toid;
				}
				return to.ID;
			}
			set {
				toid = value;
			}
		}

		/// <summary>
		/// Temp variable for xml serializer
		/// </summary>
		private int toid = -1;

		#endregion
	}
}

