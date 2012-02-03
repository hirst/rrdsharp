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
	/// Class to represent single data source definition within the RRD file.
	/// </summary>
	/// <remarks>
	/// Datasource definition consists of the following five elements:
	/// <list type="bullet">
	/// <item><description>data source name</description></item>
	/// <item><description>data source type</description></item>
	/// <item><description>heartbeat</description></item>
	/// <item><description>minimal value</description></item>
	/// <item><description>maximal value</description></item>
	/// </list>
	/// </remarks>
	public class DsDef 
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly string [] DS_TYPES = { "GAUGE", "COUNTER", "DERIVE", "ABSOLUTE" };

		private string dsName, dsType;
		private long heartbeat;
		private double minValue, maxValue;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <param name="dsType"></param>
		/// <param name="heartbeat"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public DsDef(string dsName, string dsType, long heartbeat, double minValue, double maxValue)
		{
			this.dsName = dsName;
			this.dsType = dsType;
			this.heartbeat = heartbeat;
			this.minValue = minValue;
			this.maxValue = maxValue;
			Validate();
		}

		/// <summary>
		/// 
		/// </summary>
		public string DsName
		{
			get
			{
				return dsName;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string DsType
		{
			get
			{
				return dsType;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long Heartbeat
		{
			get
			{
				return heartbeat;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double MinValue
		{
			get
			{
				return minValue;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double MaxValue 
		{
			get
			{
				return maxValue;
			}
		}

		private void Validate()
		{
			if(dsName == null || dsName.Length == 0)
			{
				throw new RrdException("Invalid datasource name specified");
			}
			if(!IsValidDsType(dsType))
			{
				throw new RrdException("Invalid datasource type specified: " + dsType);
			}
			if(heartbeat <= 0) 
			{
				throw new RrdException("Invalid heartbeat, must be positive: " + heartbeat);
			}
			if(!Double.IsNaN(minValue) && !Double.IsNaN(maxValue) && minValue >= maxValue) 
			{
				throw new RrdException("Invalid min/max values specified: " +
					minValue + "/" + maxValue);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsType"></param>
		/// <returns></returns>
		public static bool IsValidDsType(string dsType)
		{
			for(int i = 0; i < DS_TYPES.Length; i++) 
			{
				if(DS_TYPES[i].Equals(dsType)) 
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump()
		{
			return "DS:" + dsName + ":" + dsType + ":" + heartbeat +
				":" + Util.FormatDouble(minValue, "U", false) +
				":" + Util.FormatDouble(maxValue, "U", false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public new bool Equals(Object obj) 
		{
			if(obj is DsDef) 
			{
				DsDef dsObj = (DsDef) obj;
				return dsName.Equals(dsObj.dsName);
			}
			return false;
		}

	}
}