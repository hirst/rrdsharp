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
	/// Class to represent fetch request.
	/// </summary>
	/// <remarks>
	/// You cannot create FetchRequest directly (no public constructor is provided). 
	/// Use createFetchRequest() method of your RrdDb object.
	/// </remarks>
	public class FetchRequest
	{
		private RrdDb parentDb;
		private string consolFun;
		private long fetchStart;
		private long fetchEnd;
		private long resolution;
		private string[] filter;

		internal FetchRequest(RrdDb parentDb, string consolFun, long fetchStart, long fetchEnd, long resolution)
		{
			this.parentDb = parentDb;
			this.consolFun = consolFun;
			this.fetchStart = fetchStart;
			this.fetchEnd = fetchEnd;
			this.resolution = resolution;
			Validate();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		public void SetFilter(string [] filter)
		{
			this.filter = filter;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		public void SetFilter(string filter)
		{
			this.filter = (filter == null)? null: (new string [] { filter });
		}

		/// <summary>
		/// 
		/// </summary>
		public string[] Filter
		{
			get
			{
				return filter;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string ConsolFun
		{
			get
			{
				return consolFun;
			}
		}

		/// <summary>
		/// 
		/// </summary>
	
		public long FetchStart
		{
			get
			{
				return fetchStart;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long FetchEnd
		{
			get
			{
				return fetchEnd;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long Resolution
		{
			get
			{
				return resolution;
			}
		}

		private void Validate()
		{
			if(!ArcDef.IsValidConsolFun(consolFun))
			{
				throw new RrdException("Invalid consolidation function in fetch request: " + consolFun);
			}
			if(fetchStart < 0) 
			{
				throw new RrdException("Invalid start time in fetch request: " + fetchStart);
			}
			if(fetchEnd < 0) 
			{
				throw new RrdException("Invalid end time in fetch request: " + fetchEnd);
			}
			if(fetchStart >= fetchEnd) 
			{
				throw new RrdException("Invalid start/end time in fetch request: " + fetchStart +
					"/" + fetchEnd);
			}
			if(resolution <= 0) 
			{
				throw new RrdException("Invalid resolution in fetch request: " + resolution);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump()
		{
			return RrdDb.RRDTOOL + " fetch " + parentDb.RrdFile.FilePath +
				" " + consolFun + " --start " + fetchStart + " --end " + fetchEnd +
				(resolution > 1? " --resolution " + resolution: "");
		}

		internal string RrdToolCommand
		{
			get
			{
				return Dump();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public FetchData FetchData()
		{
			lock(parentDb)
			{
				return parentDb.FetchData(this);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public RrdDb ParentDb
		{
			get
			{
				return parentDb;
			}
		}

	}
}