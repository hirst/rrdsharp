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
	/// Class to represent archive values for a single datasource.
	/// </summary>
	/// <remarks>
	/// Robin class is the heart of the so-called "round robin database" concept. Basically, 
	/// each Robin object is a fixed length array of double values. Each double value reperesents 
	/// consolidated archive value for the specific timestamp. When the underlying array of double 
	/// values gets completely filled, new values will replace the oldest entries.
	///
	/// A Robin object does not hold values in memory - such object could be quite large. Instead of it,
	///  Robin stores all values on the disk and reads them only when necessary. 
	/// 
	/// </remarks>
	public class Robin : IRrdUpdatable
	{

		private Archive parentArc;
		private RrdInt pointer;
		private RrdDoubleArray values;
		private int rows;

		internal Robin(Archive parentArc, int rows)
		{
			this.parentArc = parentArc;
			this.rows = rows;
			if (RrdFile.RrdMode == RrdFile.MODE_CREATE)
			{
				pointer = new RrdInt(0,this);
				values = new RrdDoubleArray(this, rows, Double.NaN);
			}
			else
			{
				pointer = new RrdInt(this);
				values = new RrdDoubleArray(this, rows);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double[] Values 
		{
			get
			{
				return GetValues(0,rows);;
			}
		}

		internal void Store(double newValue) 
		{
			int position = pointer.Get();
			values.Set(position, newValue);
			pointer.Set((position + 1) % rows);
		}

		internal void BulkStore(double newValue, int bulkCount) 
		{
		
			int position = pointer.Get();
			// update tail
			int tailUpdateCount = Math.Min(rows - position, bulkCount);
			values.Set(position, newValue, tailUpdateCount);
			pointer.Set((position + tailUpdateCount) % rows);
			// do we need to update from the start?
			int headUpdateCount = bulkCount - tailUpdateCount;
			if(headUpdateCount > 0) 
			{
				values.Set(0, newValue, headUpdateCount);
				pointer.Set(headUpdateCount);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public RrdFile RrdFile
		{
			get
			{
				return parentArc.RrdFile;
			}
		}

		internal string Dump() 
		{
			StringBuilder buffer = new StringBuilder("Robin " + pointer.Get() + "/" + rows + ": ");
			int startPos = pointer.Get();
			for(int i = startPos; i < startPos + rows; i++) 
			{
				buffer.Append(Util.FormatDouble(values.Get(i % rows),true) + " ");
			}
			buffer.Append("\n");
			return buffer.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public double GetValue(int index)
		{
			int arrayIndex = (pointer.Get() + index) % rows;
			return values.Get(arrayIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		internal double[] GetValues(int index, int count)
		{
			int startIndex = (pointer.Get() + index) % rows;
			int tailReadCount = Math.Min(rows - startIndex, count);
			double[] tailValues = values.Get(startIndex, tailReadCount);
			if(tailReadCount < count) 
			{
				int headReadCount = count - tailReadCount;
				double[] headValues = values.Get(0, headReadCount);
				double[] myValues = new double[count];
				int k = 0;
				for(int i = 0; i < tailValues.Length; i++)
				{
					myValues[k++] = tailValues[i];
				}
				for(int i = 0; i < headValues.Length; i++)
				{
					myValues[k++] = headValues[i];
				}
				return myValues;
			}
			else 
			{
				return tailValues;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Archive Parent
		{
			get
			{
				return parentArc;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int Size
		{
			get
			{
				return rows;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		public void CopyStateTo(IRrdUpdatable other)
		{
			if(!(other is Robin)) 
			{
				throw new RrdException("Cannot copy Robin object to " + other.ToString());
			}
			Robin robin = (Robin) other;
			int rowsDiff = rows - robin.rows;
			if(rowsDiff == 0) 
			{
				// Identical dimensions. Do copy in BULK to speed things up
				robin.pointer.Set(pointer.Get());
				robin.values.WriteBytes(values.ReadBytes());
			}
			else 
			{
				// different sizes
				for(int i = 0; i < robin.rows; i++) 
				{
					int j = i + rowsDiff;
					robin.Store(j >= 0? GetValue(j): Double.NaN);
				}
			}
		}

		internal void FilterValues(double minValue, double maxValue)
		{
			for(int i = 0; i < rows; i++) 
			{
				double val = values.Get(i);
				if(!Double.IsNaN(minValue) && !Double.IsNaN(val) && minValue > val) 
				{
					values.Set(i, Double.NaN);
				}
				if(!Double.IsNaN(maxValue) && !Double.IsNaN(val) && maxValue < val) 
				{
					values.Set(i, Double.NaN);
				}
			}
		}


	}
}