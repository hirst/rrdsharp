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
using System.Collections;
using System.Text;

namespace RrdSharp.Core
{
	
	/// <summary>
	/// Class to represent definition of new RRD file.
	/// </summary>
	/// <remarks>
	/// Object of this class is used to create new RRD file from scratch - pass its reference as 
	/// an RrdDb constructor argument (see documentation for RrdDb class). RrdDef  object does not 
	/// actually create new RRD file. It just holds all necessary information which will be used 
	/// during the actual creation process
	///
	/// RRD file definition (RrdDef object) consists of the following elements:
	/// <list type="bullet">
	/// <item><description> path to RRD file that will be created</description></item>
	/// <item><description> starting timestamp</description></item>
	/// <item><description> step</description></item>
	/// <item><description> one or more datasource definitions</description></item>
	/// <item><description> one or more archive definitions </description></item>
	/// </list>
	/// RrdDef provides API to set all these elements.
	/// </remarks>
	public class RrdDef 
	{

		/// <summary>
		/// Default RRD step to be used if not specified in constructor (300 seconds)
		/// </summary>
		public static readonly long DEFAULT_STEP = 300L;

		/// <summary>
		/// If not specified in constructor, starting timestamp will be set to the current timestamp 
		/// plus DEFAULT_INITIAL_SHIFT seconds (-10) 
		/// </summary>
		public static readonly long DEFAULT_INITIAL_SHIFT = -10L;

		private string path;
		private long startTime = Util.Time + DEFAULT_INITIAL_SHIFT;
		private long step = DEFAULT_STEP;
		private ArrayList dsDefs = new ArrayList();
		private ArrayList arcDefs = new ArrayList();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public RrdDef(string path)
		{
			if (path == null || path.Length == 0) 
			{
				throw new RrdException("No filename specified");
			}
			this.path = path;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="step"></param>
		public RrdDef(string path, long step) : this (path)
		{
			if(step <= 0) 
			{
				throw new RrdException("Invalid RRD step specified: " + step);
			}
			this.step = step;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="startTime"></param>
		/// <param name="step"></param>
		public RrdDef(string path, long startTime, long step): this (path, step)
		{
			if(startTime < 0) 
			{
				throw new RrdException("Invalid RRD start time specified: " + startTime);
			}
			this.startTime = startTime;
		}

		/// <summary>
		/// 
		/// </summary>
		public string Path 
		{
			get
			{
				return path;
			}
			set
			{
				this.path = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long StartTime
		{
			get
			{
				return startTime;
			}
			set
			{
				this.startTime = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="date"></param>
		public void SetStartTime(DateTime date)
		{
			this.startTime = Util.TicksToMillis(date.Ticks);
		}
	
		/// <summary>
		/// 
		/// </summary>
		public long Step
		{
			get
			{
				return step;
			}
			set 
			{
				this.step = value;
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsDef"></param>
		public void AddDatasource(DsDef dsDef)
		{
			if(dsDefs.Contains(dsDef)) 
			{
				throw new RrdException("Datasource already defined: " + dsDef.Dump());
			}
			dsDefs.Add(dsDef);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <param name="dsType"></param>
		/// <param name="heartbeat"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public void AddDatasource(string dsName, string dsType, long heartbeat, double minValue, double maxValue)
		{
			AddDatasource(new DsDef(dsName, dsType, heartbeat, minValue, maxValue));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsDefs"></param>
		public void AddDatasource(DsDef[] dsDefs)
		{
			for(int i = 0; i < dsDefs.Length; i++) 
			{
				AddDatasource(dsDefs[i]);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arcDef"></param>
		public void AddArchive(ArcDef arcDef)
		{
			if(arcDefs.Contains(arcDef)) 
			{
				throw new RrdException("Archive already defined: " + arcDef.Dump());
			}
			arcDefs.Add(arcDef);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arcDefs"></param>
		public void AddArchive(ArcDef[] arcDefs)
		{
			for(int i = 0; i < arcDefs.Length; i++)
			{
				AddArchive(arcDefs[i]);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consolFun"></param>
		/// <param name="xff"></param>
		/// <param name="steps"></param>
		/// <param name="rows"></param>
		public void AddArchive(string consolFun, double xff, int steps, int rows)
		{
			AddArchive(new ArcDef(consolFun, xff, steps, rows));
		}

		internal void Validate()
		{
			if(dsDefs.Count == 0) 
			{
				throw new RrdException("No RRD datasource specified. At least one is needed.");
			}
			if(arcDefs.Count == 0) 
			{
				throw new RrdException("No RRD archive specified. At least one is needed.");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public DsDef[] DsDefs
		{
			get
			{
				return (DsDef[]) dsDefs.ToArray(new DsDef("speed", "COUNTER", 600, Double.NaN, Double.NaN).GetType());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public ArcDef[] ArcDefs
		{
			get
			{
				return (ArcDef[]) arcDefs.ToArray(new ArcDef("AVERAGE", 0.5, 1, 24).GetType());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int DsCount
		{
			get
			{
				return dsDefs.Count;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int ArcCount
		{
			get
			{
				return arcDefs.Count;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump() 
		{
			StringBuilder buffer = new StringBuilder(RrdDb.RRDTOOL);
			buffer.Append(" create " + path);
			buffer.Append(" --start " + StartTime);
			buffer.Append(" --step " + Step + " ");
			for(int i = 0; i < dsDefs.Count; i++) 
			{
				DsDef dsDef = (DsDef) dsDefs[i];
				buffer.Append(dsDef.Dump() + " ");
			}
			for(int i = 0; i < arcDefs.Count; i++) 
			{
				ArcDef arcDef = (ArcDef) arcDefs[i];
				buffer.Append(arcDef.Dump() + " ");
			}
			return buffer.ToString().Trim();
		}

		internal string RrdToolCommand
		{
			get
			{
				return Dump();
			}
		}

		internal void RemoveDatasource(string dsName)
		{
			for(int i = 0; i < dsDefs.Count; i++)
			{
				DsDef dsDef = (DsDef) dsDefs[i];
				if(dsDef.DsName.Equals(dsName))
				{
					dsDefs.Remove(i);
					return;
				}
			}
			throw new RrdException("Could not find datasource named '" + dsName + "'");
		}

		internal void RemoveArchive(string consolFun, int steps)
		{
			ArcDef arcDef = FindArchive(consolFun, steps);
			if(!arcDefs.Contains(arcDef)) 
			{
				throw new RrdException("Could not remove archive " +  consolFun + "/" + steps);
			}
			arcDefs.Remove(arcDef);
		}

		internal ArcDef FindArchive(string consolFun, int steps)
		{
			for(int i = 0; i < arcDefs.Count; i++)
			{
				ArcDef arcDef = (ArcDef) arcDefs[i];
				if(arcDef.ConsolFun.Equals(consolFun) && arcDef.Steps == steps) 
				{
					return arcDef;
				}
			}
			throw new RrdException("Could not find archive " + consolFun + "/" + steps);
		}

	}
}