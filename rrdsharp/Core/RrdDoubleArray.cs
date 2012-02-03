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
	internal class RrdDoubleArray : RrdPrimitive 
	{
		private int length;

		internal RrdDoubleArray(IRrdUpdatable updater, int length) : base(updater, length * RrdDouble.SIZE)
		{
			this.length = length;
		}
	
		internal RrdDoubleArray(IRrdUpdatable updater, int length, double initVal) : base(updater, length * RrdDouble.SIZE) 
		{
			this.length = length;
			Set(0, initVal, length);
		}

		internal void Set(int index, double val) 
		{
			Set(index, val, 1);
		}

		internal void Set(int index, double val, int count)
		{
			RestorePosition(index, RrdDouble.SIZE);
			rrdFile.WriteDouble(val, count);
		}

		internal double Get(int index)
		{
			RestorePosition(index, RrdDouble.SIZE);
			return rrdFile.ReadDouble();
		}

		internal double[] Get(int index, int count)
		{
			RestorePosition(index, RrdDouble.SIZE);
			return rrdFile.ReadDouble(count);
		}
	}
}