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
using System.IO;

namespace RrdSharp.Core
{
	/// <summary>
	/// Main class used for RRD files manipulation.
	/// </summary>
	/// <remarks>
	/// Use this class to perform update and fetch operations on exisiting RRD files. This class 
	/// is also used to create new RRD file from the definition (object of class RrdDef) or 
	/// from XML file (dumped content of RRDTool's or JRobin's RRD file).
	///
	/// Note that RRDSharp uses binary format different from JRobin and RRDTool's format. You cannot 
	/// use this class to manipulate RRD files created with JRobin or RRDTool. However, if you perform 
	/// the same sequence of create, update and fetch operations, you will get exactly the same results 
	/// from RRDSharp and JRobin or RRDTool. 
	/// </remarks>
	public class RrdDb : IRrdUpdatable
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly int NO_LOCKS = 0;
		/// <summary>
		/// 
		/// </summary>
		public static readonly int WAIT_IF_LOCKED = 1;
		/// <summary>
		/// 
		/// </summary>
		public static readonly int EXCEPTION_IF_LOCKED = 2;
		
		internal static readonly string RRDTOOL = "rrdtool";
		internal static bool DEBUG = false;
		
		private string canonicalPath;
		private RrdFile file;
		private Header header;
		private Datasource [] datasources;
		private Archive [] archives;

		private bool closed = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rrdDef"></param>
		public RrdDb(RrdDef rrdDef) 
		{
			rrdDef.Validate();
			InitializeSetup(rrdDef.Path, RrdFile.MODE_CREATE, false);
			// create header
			header = new Header(this, rrdDef);
			// create datasources
			DsDef[] dsDefs = rrdDef.DsDefs;
			datasources = new Datasource[dsDefs.Length];
			for(int i = 0; i < dsDefs.Length; i++) 
			{
				datasources[i] = new Datasource(this, dsDefs[i]);
			}
			// create archives
			ArcDef[] arcDefs = rrdDef.ArcDefs;
			archives = new Archive[arcDefs.Length];
			for(int i = 0; i < arcDefs.Length; i++)
			{
				archives[i] = new Archive(this, arcDefs[i]);
			}
			// finalize
			FinalizeSetup();
			Util.Debug(rrdDef.RrdToolCommand);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="readOnly"></param>
		public RrdDb(string path, bool readOnly)
		{
			FileInfo rrdFile = new FileInfo(path);
			if(!rrdFile.Exists) 
			{
				throw new IOException("Could not open file " + path + " [non existent]");
			}
			try 
			{			
				InitializeSetup(path, RrdFile.MODE_RESTORE, readOnly);
				// restore header
				header = new Header(this);
				// restore datasources
				int dsCount = header.DsCount;
				datasources = new Datasource[dsCount];
				for(int i = 0; i < dsCount; i++) 
				{
					datasources[i] = new Datasource(this);
				}
				// restore archives
				int arcCount = header.ArcCount;
				archives = new Archive[arcCount];
				for(int i = 0; i < arcCount; i++) 
				{
					archives[i] = new Archive(this);
				}
				FinalizeSetup();
			}
			catch(Exception e)
			{
				throw new RrdException(e);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public RrdDb(string path) : this(path, false)
		{
		}

	
		private void InitializeSetup(string path, int mode, bool readOnly)
		{
			file = new RrdFile(path, mode, readOnly);
		}

		private void FinalizeSetup()
		{
			int mode = file.RrdMode;
			if (mode == RrdFile.MODE_CREATE)
			{
				file.TruncateFile();
			}
			else if (mode == RrdFile.MODE_RESTORE)
			{
				if (!file.IsEndReached())
				{
					throw  new IOException("Extra bytes found in RRD file.  Possibly not an RRD file?");
				}
				file.RrdMode = RrdFile.MODE_NORMAL;
				canonicalPath = file.FilePath;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Close() 
		{
			lock(this)
			{
				if(!closed)
				{
					file.Close();
					closed = true;
				}
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public bool IsClosed
		{
			get
			{
				return closed;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public RrdFile RrdFile
		{
			get
			{
				return file;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Header Header
		{
			get
			{
				return header;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsIndex"></param>
		/// <returns></returns>
		public Datasource GetDatasource(int dsIndex)
		{
			return datasources[dsIndex];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arcIndex"></param>
		/// <returns></returns>
		public Archive GetArchive(int arcIndex)
		{
			return archives[arcIndex];
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string[] DsNames
		{
			get
			{
				int n = datasources.Length;
				string[] dsNames = new string[n];
				for(int i = 0; i < n; i++)
				{
					dsNames[i] = datasources[i].DsName;
				}
				return dsNames;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public Sample CreateSample(long time)
		{
			return new Sample(this, time);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Sample CreateSample()
		{
			return CreateSample(Util.Time);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consolFun"></param>
		/// <param name="fetchStart"></param>
		/// <param name="fetchEnd"></param>
		/// <param name="resolution"></param>
		/// <returns></returns>
		public FetchRequest CreateFetchRequest(string consolFun, long fetchStart, long fetchEnd, long resolution)
		{
			return new FetchRequest(this, consolFun, fetchStart, fetchEnd, resolution);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consolFun"></param>
		/// <param name="fetchStart"></param>
		/// <param name="fetchEnd"></param>
		/// <returns></returns>
		public FetchRequest CreateFetchRequest(string consolFun, long fetchStart, long fetchEnd)
		{
			return CreateFetchRequest(consolFun, fetchStart, fetchEnd, 1);
		}

		internal void Store(Sample sample)
		{
			long newTime = sample.Time;
			long lastTime = header.LastUpdateTime;
			if(lastTime >= newTime)
			{
				throw new RrdException("Bad sample timestamp " + newTime +
					". Last update time was " + lastTime + ", at least one second step is required");
			}
			double[] newValues = sample.Values;
			for(int i = 0; i < datasources.Length; i++)
			{
				double newValue = newValues[i];
				datasources[i].Process(newTime, newValue);
			}
			header.LastUpdateTime = newTime;
			Util.Debug(sample.RrdToolCommand);
		}

		internal FetchPoint[] Fetch(FetchRequest request)
		{
			Archive archive = FindMatchingArchive(request);
			FetchPoint[] points = archive.Fetch(request);
			Util.Debug(request.RrdToolCommand);
			return points;
		}


		internal FetchData FetchData(FetchRequest request) 
		{
			Archive archive = FindMatchingArchive(request);
			FetchData fetchData = archive.FetchData(request);
			Util.Debug(request.RrdToolCommand);
			return fetchData;
		}
		
		private Archive FindMatchingArchive(FetchRequest request)
		{
			string consolFun = request.ConsolFun;
			long fetchStart = request.FetchStart;
			long fetchEnd = request.FetchEnd;
			long resolution = request.Resolution;
			Archive bestFullMatch = null, bestPartialMatch = null;
			long bestStepDiff = 0, bestMatch = 0;
			for(int i = 0; i < archives.Length; i++)
			{
				if(archives[i].ConsolFun.Equals(consolFun)) 
				{
					long arcStep = archives[i].ArcStep;
					long arcStart = archives[i].StartTime - arcStep;
					long arcEnd = archives[i].EndTime;
					long fullMatch = fetchEnd - fetchStart;
					// best full match
					if(arcEnd >= fetchEnd && arcStart <= fetchStart)
					{
						long tmpStepDiff = Math.Abs(archives[i].ArcStep - resolution);
						if(bestFullMatch == null || tmpStepDiff < bestStepDiff) 
						{
							bestStepDiff = tmpStepDiff;
							bestFullMatch = archives[i];
						}
					}
						// best partial match
					else
					{
						long tmpMatch = fullMatch;
						if(arcStart > fetchStart)
						{
							tmpMatch -= (arcStart - fetchStart);
						}
						if(arcEnd < fetchEnd) 
						{
							tmpMatch -= (fetchEnd - arcEnd);
						}
						if(bestPartialMatch == null || bestMatch < tmpMatch)
						{
							bestPartialMatch = archives[i];
							bestMatch = tmpMatch;
						}
					}
				}
			}
			if(bestFullMatch != null) 
			{
				return bestFullMatch;
			}
			else if(bestPartialMatch != null)
			{
				return bestPartialMatch;
			}
			else
			{
				throw new RrdException("RRD file does not contain RRA:" + consolFun + " archive");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump() 
		{
			lock(this)
			{
				StringBuilder buffer = new StringBuilder();
				buffer.Append(header.Dump());
				for(int i = 0; i < datasources.Length; i++)
				{
					buffer.Append(datasources[i].Dump());
				}
				for(int i = 0; i < archives.Length; i++)
				{
					buffer.Append(archives[i].Dump());
				}
				return buffer.ToString();
			}
		}

		internal void Archive(Datasource datasource, double data, long numUpdates)
		{
			int dsIndex = GetDsIndex(datasource.DsName);
			for(int i = 0; i < archives.Length; i++) 
			{
				archives[i].archive(dsIndex, data, numUpdates);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <returns></returns>
		public int GetDsIndex(string dsName)
		{
			for(int i = 0; i < datasources.Length; i++) 
			{
				if(datasources[i].DsName.Equals(dsName)) 
				{
					return i;
				}
			}
			throw new RrdException("Unknown datasource name: " + dsName);
		}

		/// <summary>
		/// 
		/// </summary>
		public Datasource[] Datasources
		{
			get
			{
				return datasources;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Archive[] Archives
		{
			get
			{
				return archives;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long LastUpdateTime
		{
			get
			{
				return header.LastUpdateTime;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public RrdDef RrdDef
		{
			get
			{
				// set header
				long startTime = header.LastUpdateTime;
				long step = header.Step;
				string path = RrdFile.FilePath;
				RrdDef rrdDef = new RrdDef(path, startTime, step);
				// add datasources
				for(int i = 0; i < datasources.Length; i++)
				{
					DsDef dsDef = new DsDef(datasources[i].DsName,
						datasources[i].DsType, datasources[i].Heartbeat,
						datasources[i].MinValue, datasources[i].MaxValue);
					rrdDef.AddDatasource(dsDef);
				}
				// add archives
				for(int i = 0; i < archives.Length; i++)
				{
					ArcDef arcDef = new ArcDef(archives[i].ConsolFun,
						archives[i].Xff, archives[i].Steps, archives[i].Rows);
					rrdDef.AddArchive(arcDef);
				}
				return rrdDef;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static int LockMode
		{
			get
			{
				return RrdFile.LockMode;
			}
			set
			{
				RrdFile.LockMode = value;
			}
		}

		internal void finalize()
		{
			Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		public void CopyStateTo(IRrdUpdatable other)
		{
			if(!(other is RrdDb))
			{
				throw new RrdException("Cannot copy RrdDb object to " + other.ToString());
			}
			RrdDb otherRrd = (RrdDb) other;
			header.CopyStateTo(otherRrd.header);
			for(int i = 0; i < datasources.Length; i++) {
				int j = Util.GetMatchingDatasourceIndex(this, i, otherRrd);
				if(j >= 0) 
				{
					datasources[i].CopyStateTo(otherRrd.datasources[j]);
				}
			}
			for(int i = 0; i < archives.Length; i++) {
				int j = Util.GetMatchingArchiveIndex(this, i, otherRrd);
				if(j >= 0) 
				{
					archives[i].CopyStateTo(otherRrd.archives[j]);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsName"></param>
		/// <returns></returns>
		public Datasource GetDatasource(String dsName)
		{
			try
			{
				return GetDatasource(GetDsIndex(dsName));
			}
			catch (RrdException) 
			{
				return null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consolFun"></param>
		/// <param name="steps"></param>
		/// <returns></returns>
		public int GetArcIndex(String consolFun, int steps) 
		{
			for(int i = 0; i < archives.Length; i++) 
			{
				if(archives[i].ConsolFun.Equals(consolFun) && archives[i].Steps == steps)
				{
					return i;
				}
			}
			throw new RrdException("Could not find archive " + consolFun + "/" + steps);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consolFun"></param>
		/// <param name="steps"></param>
		/// <returns></returns>
		public Archive GetArchive(string consolFun, int steps)
		{
			try
			{
				return GetArchive(GetArcIndex(consolFun, steps));
			}
			catch (RrdException) 
			{
				return null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string CanonicalPath
		{
			get
			{
				return canonicalPath;
			}
		}

	}
}