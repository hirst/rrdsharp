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

	/// <summary>
	/// Class used to represent data fetched from the RRD file.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Object of this class is created when the method fetchData() is called on a FetchRequest object.
	/// </para>
	/// <para>
	/// Data returned from the RRD file is, simply, just one big table filled with timestamps and corresponding 
	/// datasource values. Use getRowCount() method to count the number of returned timestamps (table rows).
	/// </para>
	/// <para>
	/// The first table column is filled with timestamps. Time intervals between consecutive timestamps are 
	/// guaranteed to be equal. Use getTimestamps() method to get an array of timestamps returned.
	/// </para>
	/// <para>
	/// Remaining columns are filled with datasource values for the whole timestamp range, on a 
	/// column-per-datasource basis. Use getColumnCount() to find the number of datasources and getValues(i) 
	/// method to obtain all values for the i-th datasource. Returned datasource values correspond to the 
	/// values returned with getTimestamps() method.
	/// </para>
	/// </remarks>
	public class FetchData 
	{
		private FetchRequest request;
		private Archive matchingArchive;
		private string[] dsNames;
		private long[] timestamps;
		private double[][] values;

		internal FetchData(Archive matchingArchive, FetchRequest request)
		{
			this.matchingArchive = matchingArchive;
			this.dsNames = request.Filter;
			if(this.dsNames == null) 
			{
				this.dsNames = matchingArchive.ParentDb.DsNames;
			}
			this.request = request;
		}

		/// <summary>
		/// 
		/// </summary>
		public long[] Timestamps
		{
			get
			{
				return timestamps;
			}
			set
			{ 
				this.timestamps = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double[][] Values
		{
			get
			{
				return values;
			}
			set
			{
				this.values = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int RowCount 
		{
			get
			{
				return timestamps.Length;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int ColumnCount
		{
			get
			{
				return dsNames.Length;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <returns></returns>
		public FetchPoint GetRow(int rowIndex)
		{
			int numCols = ColumnCount;
			FetchPoint point = new FetchPoint(timestamps[rowIndex], ColumnCount);
			for(int dsIndex = 0; dsIndex < numCols; dsIndex++) 
			{
				point.SetValue(dsIndex, values[dsIndex][rowIndex]);
			}
			return point;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsIndex"></param>
		/// <returns></returns>
		public double[] GetValues(int dsIndex)
		{
			return values[dsIndex];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <returns></returns>
		public double[] GetValues(string dsName)
		{
			for(int dsIndex = 0; dsIndex < ColumnCount; dsIndex++)
			{
				if(dsName.Equals(dsNames[dsIndex]))
				{
					return GetValues(dsIndex);
				}
			}
			throw new RrdException("Datasource [" + dsName + "] not found");
		}

		internal FetchRequest Request
		{
			get
			{
				return request;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long FirstTimestamp
		{
			get
			{
				return timestamps[0];
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long LastTimestamp
		{
			get
			{
				return timestamps[timestamps.Length - 1];
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Archive MatchingArchive
		{
			get
			{
				return matchingArchive;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string[] DsNames
		{
			get
			{
				return dsNames;
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <returns></returns>
		public int GetDsIndex( string dsName )
		{
			// Let's assume the table of dsNames is always small, so it is not necessary to use a hashmap for lookups
			for (int i = 0; i < dsNames.Length; i++)
				if ( dsNames[i].Equals(dsName) )
					return i;
		
			return -1;		// Datasource not found !
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dump()
		{
			for(int i = 0; i < RowCount; i++)
			{
				Console.WriteLine(GetRow(i).Dump());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <param name="consolFun"></param>
		/// <returns></returns>
		public double GetAggregate(string dsName, string consolFun)
		{
			return GetAggregate(dsName, consolFun, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <param name="consolFun"></param>
		/// <param name="rpnExpression"></param>
		/// <returns></returns>
		public double GetAggregate(string dsName, string consolFun, string rpnExpression)
		{
			if(consolFun.Equals("MAX"))
			{
				return GetMax(dsName, rpnExpression);
			}
			else if(consolFun.Equals("MIN"))
			{
				return GetMin(dsName, rpnExpression);
			}
			else if(consolFun.Equals("LAST"))
			{
				return GetLast(dsName, rpnExpression);
			}
			else if(consolFun.Equals("AVERAGE"))
			{
				return GetAverage(dsName, rpnExpression);
			}
			else
			{
				throw new RrdException("Unsupported consolidation function [" + consolFun + "]");
			}
		}

		private double GetMax(string dsName, string rpnExpression)
		{
			RpnCalculator rpnCalculator = null;
			if(rpnExpression != null)
			{
				rpnCalculator = new RpnCalculator(rpnExpression);
			}
			double[] vals = GetValues(dsName);
			double max = Double.NaN;
			for(int i = 0; i < vals.Length - 1; i++)
			{
				double val = vals[i + 1];
				if(rpnCalculator != null) 
				{
					rpnCalculator.Value = val;
					val = rpnCalculator.Calculate();
				}
				max = Util.Max(max, val);
			}
			return max;
		}

		private double GetMin(string dsName, string rpnExpression)
		{
			RpnCalculator rpnCalculator = null;
			if(rpnExpression != null)
			{
				rpnCalculator = new RpnCalculator(rpnExpression);
			}
			double [] vals = GetValues(dsName);
			double min = Double.NaN;
			for(int i = 0; i < vals.Length - 1; i++) 
			{
				double val = vals[i + 1];
				if(rpnCalculator != null)
				{
					rpnCalculator.Value = val;
					val = rpnCalculator.Calculate();
				}
				min = Util.Min(min, val);
			}
			return min;
		}

		private double GetLast(string dsName, string rpnExpression)
		{
			RpnCalculator rpnCalculator = null;
			if(rpnExpression != null)
			{
				rpnCalculator = new RpnCalculator(rpnExpression);
			}
			double [] vals = GetValues(dsName);
			double val = vals[vals.Length - 1];
			if(rpnCalculator != null)
			{
				rpnCalculator.Value = val;
				val = rpnCalculator.Calculate();
			}
			return val;
		}

		private double GetAverage(string dsName, string rpnExpression)
		{
			RpnCalculator rpnCalculator = null;
			if(rpnExpression != null)
			{
				rpnCalculator = new RpnCalculator(rpnExpression);
			}
			double [] vals = GetValues(dsName);
			double totalVal = 0;
			long totalSecs = 0;
			for(int i = 0; i < vals.Length - 1; i++)
			{
				long t1 = Math.Max(request.FetchStart, timestamps[i]);
				long t2 = Math.Min(request.FetchEnd, timestamps[i + 1]);
				double val = vals[i + 1];
				if(rpnCalculator != null)
				{
					rpnCalculator.Value = val;
					val = rpnCalculator.Calculate();
				}
				if(!Double.IsNaN(val))
				{
					totalSecs += (t2 - t1);
					totalVal += (t2 - t1) * val;
				}
			}
			return totalSecs > 0? totalVal / totalSecs: Double.NaN;
		}

	}

}