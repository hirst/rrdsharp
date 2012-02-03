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

	internal class RrdString : RrdPrimitive
	{
		private static readonly int SIZE = 20;

		private string cache;

		internal RrdString(IRrdUpdatable updater) : base(updater, SIZE * 2)
		{
			LoadCache();
		}

		internal void LoadCache()
		{
			if(rrdFile.RrdMode == RrdFile.MODE_RESTORE)
			{
				RestorePosition();
				char [] c = new char[SIZE];
				for (int i = 0; i < SIZE; i++)
				{
					c[i] = rrdFile.ReadChar();
				}
				cache = new String(c).Trim();
				cached = true;
			}
		}

		internal RrdString(string initValue, IRrdUpdatable updater) : base(updater, SIZE * 2)
		{
			Set(initValue);
		}

		
		internal void Set(string data) 
		{
			data = data.Trim();
			if(!cached || !cache.Equals(data))
			{
				RestorePosition();
				for (int i = 0; i < SIZE; i++)
				{
					if (i < data.Length)
					{
						rrdFile.WriteChar(data[i]);
					}
					else
					{
						rrdFile.WriteChar(' ');
					}
				}
				cache = data;
				cached = true;
			}
		}

		internal string Get()
		{
			return cache;
		}
	}
}