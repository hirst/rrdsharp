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
	/// Class to represent single datasource within RRD file.
	/// </summary>
	/// <remarks>
	/// Each datasource object holds the following information: datasource definition (once set, never changed)
	/// and datasource state variables (changed whenever RRD file gets updated).
	///
	/// Normally, you don't need to manipluate Datasource objects directly, it's up to the RRDSharp framework 
	/// to do it for you. 
	///	</remarks>
	public class Datasource : IRrdUpdatable 
	{
		private RrdDb parentDb;
		// definition
		private RrdString dsName, dsType;
		private RrdLong heartbeat;
		private RrdDouble minValue, maxValue;
		// state variables
		private RrdDouble lastValue;
		private RrdLong nanSeconds;
		private RrdDouble accumValue;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentDb"></param>
		/// <param name="dsDef"></param>
		public Datasource(RrdDb parentDb, DsDef dsDef)
		{
			this.parentDb = parentDb;
			dsName = new RrdString(dsDef.DsName, this);
			dsType = new RrdString(dsDef.DsType, this);
			heartbeat = new RrdLong(dsDef.Heartbeat, this);
			minValue = new RrdDouble(dsDef.MinValue, this);
			maxValue = new RrdDouble(dsDef.MaxValue, this);
			lastValue = new RrdDouble(Double.NaN, this);
			accumValue = new RrdDouble(0.0, this);
			Header header = parentDb.Header;
			nanSeconds = new RrdLong(header.LastUpdateTime % header.Step, this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentDb"></param>
		public Datasource(RrdDb parentDb)
		{
			this.parentDb = parentDb;
			dsName = new RrdString(this);
			dsType = new RrdString(this);
			heartbeat = new RrdLong(this);
			minValue = new RrdDouble(this);
			maxValue = new RrdDouble(this);
			lastValue = new RrdDouble(this);
			accumValue = new RrdDouble(this);
			nanSeconds = new RrdLong(this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump()
		{
			return "== DATASOURCE ==\n" +
				"DS:" + dsName.Get() + ":" + dsType.Get() + ":" +
				heartbeat.Get() + ":" + minValue.Get() + ":" +
				maxValue.Get() + "\nlastValue:" + lastValue.Get() +
				" nanSeconds:" + nanSeconds.Get() +
				" accumValue:" + accumValue.Get() + "\n";
		}

		/// <summary>
		/// 
		/// </summary>
		public RrdFile RrdFile
		{
			get
			{
				return parentDb.RrdFile;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string DsName
		{
			get
			{
				return dsName.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string DsType
		{
			get
			{
				return dsType.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long Heartbeat
		{
			get
			{
				return heartbeat.Get();
			}
			set
			{
				if (value < 1L) 
				{
					throw new RrdException("Invalid heartbeat specified: " + value);
				}
				this.heartbeat.Set(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double MinValue
		{
			get
			{
				return minValue.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double MaxValue
		{
			get
			{
				return maxValue.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double LastValue
		{
			get
			{
				return lastValue.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double AccumValue
		{
			get
			{
				return accumValue.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long NanSeconds
		{
			get
			{
				return nanSeconds.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int DsIndex
		{
			get
			{
				try
				{
					return parentDb.GetDsIndex(dsName.Get());
				}
				catch(RrdException)
				{
					return -1;
				}
			}
		}

		internal void Process(long newTime, double newValue)
		{
			Header header = parentDb.Header;
			long step = header.Step;
			long oldTime = header.LastUpdateTime;
			long startTime = Util.Normalize(oldTime, step);
			long endTime = startTime + step;
			double oldValue = lastValue.Get();
			double updateValue = CalculateUpdateValue(oldTime, oldValue, newTime, newValue);
			if(newTime < endTime) 
			{
				Accumulate(oldTime, newTime, updateValue);
			}
			else 
			{
				// should store something
				long boundaryTime = Util.Normalize(newTime, step);
				Accumulate(oldTime, boundaryTime, updateValue);
				double value = CalculateTotal(startTime, boundaryTime);
				// how many updates?
				long numSteps= (boundaryTime - endTime) / step + 1L;
				// ACTION!
				parentDb.Archive(this, value, numSteps);
				// cleanup
				nanSeconds.Set(0);
				accumValue.Set(0.0);
				Accumulate(boundaryTime, newTime, updateValue);
			}
		}

		private double CalculateUpdateValue(long oldTime, double oldValue, long newTime, double newValue)
		{
			double updateValue = Double.NaN;
			if(newTime - oldTime <= heartbeat.Get()) 
			{
				string type = dsType.Get();
				if(type.Equals("GAUGE")) 
				{
					updateValue = newValue;
				}
				else if(type.Equals("ABSOLUTE")) 
				{
					if(!Double.IsNaN(newValue)) 
					{
						updateValue = newValue / (newTime - oldTime);
					}
				}
				else if(type.Equals("DERIVE")) 
				{
					if(!Double.IsNaN(newValue) && !Double.IsNaN(oldValue)) 
					{
						updateValue = (newValue - oldValue) / (newTime - oldTime);
					}
				}
				else if(type.Equals("COUNTER")) 
				{
					if(!Double.IsNaN(newValue) && !Double.IsNaN(oldValue)) 
					{
						double diff = newValue - oldValue;
						double max32bit = Math.Pow(2, 32);
						double max64bit = Math.Pow(2, 64);
						if(diff < 0) 
						{
							diff += max32bit;
						}
						if(diff < 0) 
						{
							diff += max64bit - max32bit;
						}
						if(diff >= 0) 
						{
							updateValue = diff / (newTime - oldTime);
						}
					}
				}
				if(!Double.IsNaN(updateValue)) 
				{
					double minVal = minValue.Get();
					double maxVal = maxValue.Get();
					if(!Double.IsNaN(minVal) && updateValue < minVal) 
					{
						updateValue = Double.NaN;
					}
					if(!Double.IsNaN(maxVal) && updateValue > maxVal) 
					{
						updateValue = Double.NaN;
					}
				}
			}
			lastValue.Set(newValue);
			return updateValue;
		}

		private void Accumulate(long oldTime, long newTime, double updateValue)
		{
			if(Double.IsNaN(updateValue)) 
			{
				nanSeconds.Set(nanSeconds.Get() + (newTime - oldTime));
			}
			else 
			{
				accumValue.Set(accumValue.Get() + updateValue * (newTime - oldTime));
			}
		}

		private double CalculateTotal(long startTime, long boundaryTime)
		{
			double totalValue = Double.NaN;
			long validSeconds = boundaryTime - startTime - nanSeconds.Get();
			if(nanSeconds.Get() <= heartbeat.Get() && validSeconds > 0) 
			{
				totalValue = accumValue.Get() / validSeconds;
			}
			return totalValue;
		}
    
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		public void CopyStateTo(IRrdUpdatable other)
		{
			if(!(other is Datasource)) 
			{
				throw new RrdException("Cannot copy Datasource object to " + other.ToString());
			}
			Datasource datasource = (Datasource) other;
			if(!datasource.dsName.Get().Equals(dsName.Get())) 
			{
				throw new RrdException("Incomaptible datasource names");
			}
			if(!datasource.dsType.Get().Equals(dsType.Get())) 
			{
				throw new RrdException("Incomaptible datasource types");
			}
			datasource.lastValue.Set(lastValue.Get());
			datasource.nanSeconds.Set(nanSeconds.Get());
			datasource.accumValue.Set(accumValue.Get());
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="filterArchivedValues"></param>
		public void SetMinValue(double minValue, bool filterArchivedValues)
		{
			double maxValue = this.maxValue.Get();
			if(!Double.IsNaN(minValue) && !Double.IsNaN(maxValue) && minValue >= maxValue)
			{
				throw new RrdException("Invalid min/max values: " + minValue + "/" + maxValue);
			}
    		this.minValue.Set(minValue);
			if(!Double.IsNaN(minValue) && filterArchivedValues) 
			{
				int dsIndex = DsIndex;
				Archive[] archives = parentDb.Archives;
				for(int i = 0; i < archives.Length; i++)
				{
					archives[i].GetRobin(dsIndex).FilterValues(minValue, Double.NaN);
				}
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="maxValue"></param>
		/// <param name="filterArchivedValues"></param>
		public void SetMaxValue(double maxValue, bool filterArchivedValues)
		{
			double minValue = this.minValue.Get();
			if(!Double.IsNaN(minValue) && !Double.IsNaN(maxValue) && minValue >= maxValue) 
			{
				throw new RrdException("Invalid min/max values: " + minValue + "/" + maxValue);
			}
    		this.maxValue.Set(maxValue);
			if(!Double.IsNaN(maxValue) && filterArchivedValues)
			{
				int dsIndex = DsIndex;
				Archive[] archives = parentDb.Archives;
				for(int i = 0; i < archives.Length; i++) 
				{
					archives[i].GetRobin(dsIndex).FilterValues(Double.NaN, maxValue);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <param name="filterArchivedValues"></param>
		public void SetMinMaxValue(double minValue, double maxValue, bool filterArchivedValues)
		{
			if(!Double.IsNaN(minValue) && !Double.IsNaN(maxValue) && minValue >= maxValue)
			{
				throw new RrdException("Invalid min/max values: " + minValue + "/" + maxValue);
			}
			this.minValue.Set(minValue);
    		this.maxValue.Set(maxValue);
			if(!(Double.IsNaN(minValue) && Double.IsNaN(maxValue)) && filterArchivedValues)
			{
				int dsIndex = DsIndex;
				Archive[] archives = parentDb.Archives;
				for(int i = 0; i < archives.Length; i++)
				{
					archives[i].GetRobin(dsIndex).FilterValues(minValue, maxValue);
				}
			}
		}
		
	}
}