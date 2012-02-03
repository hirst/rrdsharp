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
	internal abstract class RrdPrimitive 
	{
		private long pointer;
		private int byteCount;

		internal RrdFile rrdFile;
		internal bool cached = false;

		internal RrdPrimitive(IRrdUpdatable parent, int byteCount)
		{
			rrdFile = parent.RrdFile;
			// this will set pointer and byteCount
			rrdFile.Allocate(this, byteCount);
		}
	
		internal long Pointer 
		{
			get
			{
				return pointer;
			}
			set
			{
				this.pointer = value;
			}
		}
	
		internal int ByteCount 
		{
			get
			{
				return byteCount;
			}
			set
			{
				this.byteCount = value;
			}
		}
	
		internal byte[] ReadBytes()
		{
			byte[] b = new byte[byteCount];
			RestorePosition();
			int bytesRead = rrdFile.Read(ref b);
			return b;
		}

		internal void WriteBytes(byte[] b)
		{
			RestorePosition();
			rrdFile.Write(b);
		}

		internal void RestorePosition()
		{
			rrdFile.Seek(pointer);
		}

		internal void RestorePosition(int unitIndex, int unitSize)
		{
			rrdFile.Seek(pointer + unitIndex * unitSize);
		}
	}
}