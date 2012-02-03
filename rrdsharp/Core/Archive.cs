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
	/// Class to represent single RRD archive in a RRD file with its internal state. 
	/// </summary>
	/// <remarks>
	/// Normally, you don't need methods to manipulate archive objects directly because the 
	/// RRDSharp framework does it automatically for you.
	///
	/// Each archive object consists of three parts: an archive definition, archive state objects 
	/// (one state object for each datasource,) and round robin archives (one round robin for each 
	/// datasource). The API (read-only) is provided to access each of these parts.
	/// </remarks>
	public class Archive : IRrdUpdatable 
	{
		private RrdDb parentDb;
	
		private RrdString consolFun;
		private RrdDouble xff;
		private RrdInt steps, rows;
		private Robin [] robins;
		private ArcState [] states;

	
		/// <summary>
		/// 
		/// </summary>
		public long ArcStep
		{
			get
			{
				long step = parentDb.Header.Step;
				return step * steps.Get();
			}
		}

		internal RrdDb ParentDb
		{
			get
			{
				return parentDb;
			}
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
		public string ConsolFun
		{
			get
			{
				return consolFun.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double Xff
		{
			get
			{
				return xff.Get();
			}
			set
			{
				if (value < 0D || value >= 1D)
				{
					throw new RrdException("Invalid xff supplied (" + value + "), must be >= 0 and < 1");
				}
				this.xff.Set(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int Steps
		{
			get
			{
				return steps.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int Rows
		{
			get
			{
				return rows.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long StartTime
		{
			get
			{
				long endTime = EndTime;
				long arcStep = ArcStep;
				long numRows = rows.Get();
				return endTime - (numRows - 1) * arcStep;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long EndTime
		{
			get
			{
				long arcStep = ArcStep;
				long lastUpdateTime = parentDb.Header.LastUpdateTime;
				return Util.Normalize(lastUpdateTime, arcStep);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentDb"></param>
		/// <param name="arcDef"></param>
		public Archive(RrdDb parentDb, ArcDef arcDef)
		{
			this.parentDb = parentDb;
			consolFun = new RrdString(arcDef.ConsolFun, this);
			xff = new RrdDouble(arcDef.Xff,this);
			steps = new RrdInt(arcDef.Steps,this);
			rows = new RrdInt(arcDef.Rows,this);
			int n = parentDb.Header.DsCount;
			robins = new Robin[n];
			states = new ArcState[n];
			for(int i = 0; i < n; i++)
			{
				states[i] = new ArcState(this);
				robins[i] = new Robin(this, rows.Get());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentDb"></param>
		public Archive(RrdDb parentDb) 
		{
			this.parentDb = parentDb;
			consolFun = new RrdString(this);
			xff = new RrdDouble(this);
			steps = new RrdInt(this);
			rows = new RrdInt(this);
			int n = parentDb.Header.DsCount;
			states = new ArcState[n];
			robins = new Robin[n];
			for(int i = 0; i < n; i++) 
			{
				states[i] = new ArcState(this);
				robins[i] = new Robin(this, rows.Get());
			}
		}
		
		internal void archive(int dsIndex, double data, long numUpdates)
		{
			Robin robin = robins[dsIndex];
			ArcState state = states[dsIndex];
			long step = parentDb.Header.Step;
			long lastUpdateTime = parentDb.Header.LastUpdateTime;
			long updateTime = Util.Normalize(lastUpdateTime, step) + step;
			long arcStep = ArcStep;
		
			while(numUpdates > 0) 
			{
				Accumulate(state, data);
				numUpdates--;
				if(updateTime % arcStep == 0)
				{
					FinalizeStep(state, robin);
					break;
				}
				else
				{
					updateTime += step;
				}
			}
		
		
			int bulkUpdateCount = (int) Math.Min(numUpdates / steps.Get(), (long) rows.Get());
			robin.BulkStore(data, bulkUpdateCount);
			
            long remainingUpdates = numUpdates % steps.Get();
			for (long i = 0; i < remainingUpdates; i++)
			{
				Accumulate(state, data);
			}

		}

		private void Accumulate(ArcState state, double data)
		{
			if(Double.IsNaN(data)) 
			{
				state.NanSteps = (state.NanSteps + 1);
			}
			else 
			{
				if(consolFun.Get().Equals("MIN"))
				{
					state.AccumValue = Util.Min(state.AccumValue, data);
				}
				else if(consolFun.Get().Equals("MAX")) 
				{
					state.AccumValue = Util.Max(state.AccumValue, data);
				}
				else if(consolFun.Get().Equals("LAST"))
				{
					state.AccumValue = data;
				}
				else if(consolFun.Get().Equals("AVERAGE"))
				{
					state.AccumValue = Util.Sum(state.AccumValue, data);
				}
			}
		}

		private void FinalizeStep(ArcState state, Robin robin)
		{
			// should store
			long arcSteps = steps.Get();
			double arcXff = xff.Get();
			long nanSteps = state.NanSteps;
			//double nanPct = (double) nanSteps / (double) arcSteps;
			double accumValue = state.AccumValue;
			if (nanSteps <= arcXff * arcSteps && !Double.IsNaN(accumValue))
			{
				if(consolFun.Get().Equals("AVERAGE"))
				{
					accumValue /= (arcSteps - nanSteps);
				}
				robin.Store(accumValue);
			}
			else
			{
				robin.Store(Double.NaN);
			}
			state.AccumValue = Double.NaN;
			state.NanSteps = 0;
		}

		internal FetchPoint[] Fetch(FetchRequest request)
		{
			if(request.Filter != null) 
			{
				throw new RrdException("Fetch() method does not support filtered datasources." +
					" Use fetchData() to get filtered fetch data.");
			}
			
			long arcStep = ArcStep;
			long fetchStart = Util.Normalize(request.FetchStart, arcStep);
			long fetchEnd = Util.Normalize(request.FetchEnd, arcStep);
			if(fetchEnd < request.FetchEnd)
			{
				fetchEnd += arcStep;
			}
			long startTime = StartTime;
			long endTime = EndTime;
			int dsCount = robins.Length;
			int ptsCount = (int) ((fetchEnd - fetchStart) / arcStep + 1);
			FetchPoint[] points = new FetchPoint[ptsCount];
			for(int i = 0; i < ptsCount; i++)
			{
				long time = fetchStart + i * arcStep;
				FetchPoint point = new FetchPoint(time, dsCount);
				if(time >= startTime && time <= endTime)
				{
					int robinIndex = (int)((time - startTime) / arcStep);
					for(int j = 0; j < dsCount; j++)
					{
						point.SetValue(j, robins[j].GetValue(robinIndex));
					}
				}
				points[i] = point;
			}
			return points;
		}

		internal FetchData FetchData(FetchRequest request) 
		{
			long arcStep = ArcStep;
			long fetchStart = Util.Normalize(request.FetchStart, arcStep);
			long fetchEnd = Util.Normalize(request.FetchEnd, arcStep);
			if(fetchEnd < request.FetchEnd)
			{
				fetchEnd += arcStep;
			}
			long startTime = StartTime;
			long endTime = EndTime;
			string[] dsToFetch = request.Filter;
			if(dsToFetch == null)
			{
				dsToFetch = parentDb.DsNames;
			}
			int dsCount = dsToFetch.Length;
			int ptsCount = (int) ((fetchEnd - fetchStart) / arcStep + 1);
			long[] timestamps = new long[ptsCount];
			double[][] values = new double[dsCount][];
			for (int i=0; i < dsCount; i++)
			{
				values[i] = new double[ptsCount];
			}
			long matchStartTime = Math.Max(fetchStart, startTime);
			long matchEndTime = Math.Min(fetchEnd, endTime);
			double [][] robinValues = null;

			if(matchStartTime <= matchEndTime) 
			{
				// preload robin values
				int matchCount = (int)((matchEndTime - matchStartTime) / arcStep + 1);
				int matchStartIndex = (int)((matchStartTime - startTime) / arcStep);
				robinValues = new double[dsCount][];
				for(int i = 0; i < dsCount; i++) 
				{
					int dsIndex = parentDb.GetDsIndex(dsToFetch[i]);
					robinValues[i] = robins[dsIndex].GetValues(matchStartIndex, matchCount);
				}
			}
			for(int ptIndex = 0; ptIndex < ptsCount; ptIndex++)
			{
				long time = fetchStart + ptIndex * arcStep;
				timestamps[ptIndex] = time;
				
				for(int i = 0; i < dsCount; i++) 
				{
					double val = Double.NaN;
					if(time >= matchStartTime && time <= matchEndTime) 
					{
						// inbound time
						int robinValueIndex = (int)((time - matchStartTime) / arcStep);
						val = robinValues[i][robinValueIndex];
					}
					values[i][ptIndex] = val;
				}
			}
			FetchData fetchData = new FetchData(this, request);
			fetchData.Timestamps = timestamps;
			fetchData.Values = values;
			return fetchData;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsIndex"></param>
		/// <returns></returns>
		public ArcState GetArcState(int dsIndex) 
		{
			return states[dsIndex];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsIndex"></param>
		/// <returns></returns>
		public Robin GetRobin(int dsIndex) 
		{
			return robins[dsIndex];
		}
	
		internal string Dump() 
		{
			StringBuilder buffer = new StringBuilder("== ARCHIVE ==\n");
			buffer.Append("RRA:" + consolFun.Get() + ":" + xff.Get() + ":" +
				steps.Get() + ":" + rows.Get() + "\n");
			buffer.Append("interval [" + StartTime + ", " + EndTime + "]" + "\n");
			for(int i = 0; i < robins.Length; i++)
			{
				buffer.Append(states[i].Dump());
				buffer.Append(robins[i].Dump());
			}
			return buffer.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		public void CopyStateTo(IRrdUpdatable other)
		{
			if(!(other is Archive))
			{
				throw new RrdException("Cannot copy Archive object to " + other.ToString());
			}
			Archive arc = (Archive) other;
			if(!arc.consolFun.Get().Equals(consolFun.Get()))
			{
				throw new RrdException("Incompatible consolidation functions");
			}
			if(arc.steps.Get() != steps.Get())
			{
				throw new RrdException("Incompatible number of steps");
			}
			int count = parentDb.Header.DsCount;
			for(int i = 0; i < count; i++)
			{
				int j = Util.GetMatchingDatasourceIndex(parentDb, i, arc.parentDb);
				if(j >= 0) 
				{
					states[i].CopyStateTo(arc.states[j]);
					robins[i].CopyStateTo(arc.robins[j]);
				}
			}
		}

	}
}