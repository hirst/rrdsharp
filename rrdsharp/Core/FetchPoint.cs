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
using System.Text;

namespace RrdSharp.Core
{

	/// <summary>
	/// Class to represent data source values for the specific timestamp.
	/// </summary>
	/// <remarks>
	/// Objects of this class are created during the fetching process. See fetch()  
	/// method of the FetchRequest class.
	/// </remarks>
	public class FetchPoint 
	{

		private long time;
		private double[] values;

		internal FetchPoint(long time, int size)
		{
			this.time = time;
			values = new double[size];
			for(int i = 0; i < size; i++)
			{
				values[i] = Double.NaN;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long Time
		{
			get
			{
				return time;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double[] Values
		{
			get
			{
				return values;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int Size
		{
			get
			{
				return values.Length;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public double GetValue(int i)
		{
			return values[i];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		public void SetValue(int index, double data)
		{
			values[index] = data;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump() 
		{
			StringBuilder buffer = new StringBuilder(time + ": ");
			for(int i = 0; i < values.Length; i++) 
			{
				buffer.Append(Util.FormatDouble(values[i],true));
				buffer.Append(" ");
			}
			return buffer.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString() 
		{
			return Dump();
		}
	}
}