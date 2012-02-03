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
	/// Class to represent data source values for the given timestamp. 
	/// </summary>
	/// <remarks>
	/// Objects of this class are never created directly (no public constructor is provided). To learn more 
	/// how to update a RRD file, see RRDSharp's Tutorial section.
	/// 
	/// To update a RRD file with RRDSharp use the following procedure:
	/// <list type="number">
	/// <item><description>Obtain empty Sample object by calling method createSample() on respective RrdDb object.</description></item> 
	/// <item><description>Adjust Sample timestamp if necessary (see setTime() method).</description></item>
	/// <item><description>Supply data source values (see setValue()).</description></item>
	/// <item><description>Call Sample's update() method. </description></item>
	/// </list>
	/// Newly created Sample object contains all data source values set to 'unknown'. You should specifify 
	/// only 'known' data source values. However, if you want to specify 'unknown' values too, use Double.NaN.
	/// </remarks>
	public class Sample
	{
		private RrdDb parentDb;
		private long time;
		private string[] dsNames;
		private double[] values;

		internal Sample(RrdDb parentDb, long time)
		{
			this.parentDb = parentDb;
			this.time = time;
			this.dsNames = parentDb.DsNames;
			values = new double[dsNames.Length];
			ClearCurrentValues();
		}

		private void ClearCurrentValues()
		{
			for (int i = 0; i < values.Length; i ++)
			{
				values[i] = Double.NaN;
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <param name="data"></param>
		public void SetValue(string dsName, double data) 
		{
			for(int i = 0; i < dsNames.Length; i++)
			{
				if(dsNames[i].Equals(dsName))
				{
					values[i] = data;
					return;
				}
			}
			throw new RrdException("Datasource " + dsName + " not found");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <param name="data"></param>
		public void SetValue(int i, double data)
		{
			if(i < dsNames.Length)
			{
				values[i] = data;
				return;
			}
			throw new RrdException("Sample index " + i + " out of bounds");
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
			set
			{
				if(value.Length <= this.values.Length)
				{
					for(int i = 0; i < value.Length; i++)
					{
						this.values[i] = value[i];
					}
				}
				else
				{
					throw new RrdException("Invalid number of values specified");
				}
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
			set
			{
				this.time = value;
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
		/// <param name="timeAndValues"></param>
		public virtual void Set(string timeAndValues)
		{
			char[] seps = { ':' };
			string[] tokens = timeAndValues.Split(seps);
			int numTokens = tokens.Length;
			
			long time = Int64.Parse(tokens[0]);
			double[] values = new double[numTokens - 1];
			for (int i = 0; i < numTokens - 1; i++)
			{
				try
				{
					values[i] = Double.Parse(tokens[i + 1]);
				}
				catch (FormatException)
				{
					values[i] = System.Double.NaN;
				}
			}
			Time = time;
			Values = values;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeAndValues"></param>
		public void SetAndUpdate(string timeAndValues)
		{
			Set(timeAndValues);
			Update();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Update()
		{
			lock (parentDb)
			{
				parentDb.Store(this);
			}
			ClearCurrentValues();

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump() 
		{
			StringBuilder buffer = new StringBuilder(RrdDb.RRDTOOL);
			buffer.Append(" update " + parentDb.RrdFile.FilePath + " " + time);
			for(int i = 0; i < values.Length; i++)
			{
				buffer.Append(":");
				buffer.Append(Util.FormatDouble(values[i], true));
			}
			return buffer.ToString();
		}

		internal string RrdToolCommand
		{
			get
			{
				return Dump();
			}
		}
	}
}