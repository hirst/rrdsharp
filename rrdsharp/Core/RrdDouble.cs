// ============================================================
//  RRDSharp: Managed implementation of RRDTool for .NET/Mono
// ============================================================
//
// Project Info:  http://sourceforge.net/projects/rrdsharp/
// Project Lead:  Julio David Quintana (david@quintana.org)
//
// Distributed under terms of the LGPL:
//
// This library is free software; you can redistribute it and/or modify it under the terms
// of the GNU Lesser General Public License as published by the Free Software Foundation;
// either version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this
// library; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330,
// Boston, MA 02111-1307, USA.


using System;

namespace RrdSharp.Core
{

	internal class RrdDouble : RrdPrimitive
	{
		internal static readonly int SIZE = 8;
		private double cache;

		internal RrdDouble(IRrdUpdatable updater) : base(updater, SIZE)
		{
			LoadCache();
		}

		internal void LoadCache()
		{
			if (rrdFile.RrdMode == RrdFile.MODE_RESTORE)
			{
				RestorePosition();
				cache = rrdFile.ReadDouble();
				cached = true;
			}
		}

		internal RrdDouble(double initValue, IRrdUpdatable updater) : base (updater, SIZE)
		{
			Set(initValue);
		}

		internal void Set(double data)
		{
			if (!cached || cache != data)
			{
				RestorePosition();
				rrdFile.WriteDouble(data);
				cache = data;
				cached = true;
			}
		}

		internal double Get() 
		{
			return cache;
		}
	}
}