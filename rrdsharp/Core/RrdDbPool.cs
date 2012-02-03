// Current with JRobin 1.2.2

using System;
using System.Collections;
using System.IO;

namespace RrdSharp.Core
{

	public class RrdDbPool 
	{
		private static RrdDbPool ourInstance;
		private static readonly bool DEBUG = false;
		
        public static readonly int INITIAL_CAPACITY = 50;
		private static int capacity = INITIAL_CAPACITY;

		private HashTable rrdMap = new HashTable();

		public static RrdDbPool GetInstance() 
		{
			lock(this)
			{
				if (ourInstance == null) 
				{
					ourInstance = new RrdDbPool();
				}
				return ourInstance;
			}
		}

		private RrdDbPool()
		{
		}

		public RrdDb RequestRrdDb(string path)
		{
			lock(this)
			{
				string keypath = GetCanonicalPath(path);
				if(rrdMap.ContainsKey(keypath)) 
				{
					// already open
					RrdEntry rrdEntry = (RrdEntry) rrdMap[keypath];
					rrdEntry.ReportUsage();
					Debug("EXISTING: " + rrdEntry.Dump());
					return rrdEntry.RrdDb;
				}
				else 
				{
					// not found, open it
					RrdDb rrdDb = new RrdDb(path);
					Put(keypath, rrdDb);
					return rrdDb;
				}
			}
		}

		
		public RrdDb RequestRrdDb(RrdDef rrdDef)
		{
			string path = rrdDef.Path;
			string keypath = GetCanonicalPath(path);
			ValidateInactive(keypath);
			RrdDb rrdDb = new RrdDb(rrdDef);
			Put(keypath, rrdDb);
			return rrdDb;
		}

		private void Add(string keypath, RrdDb rrdDb)
		{
			RrdEntry newEntry = new RrdEntry(rrdDb);
			Debug("NEW: " + newEntry.Dump());
			rrdMap.Add(keypath, newEntry);
			GC();
		}

		private void ValidateInactive(string keypath)
		{
			if(rrdMap.ContainsKey(keypath)) 
			{
				// already open, check if active (not released)
				RrdEntry rrdEntry = (RrdEntry) rrdMap[keypath];
				if(!rrdEntry.IsReleased())
				{
					// not released, not allowed here
					throw new RrdException("VALIDATOR: Cannot create new RrdDb file. " +
						"File " + keypath + " already active in pool");
				}
				else {
					// open but released... safe to close it
					Debug("WILL BE RECREATED: " + rrdEntry.Dump());
					rrdEntry.Close();
					rrdMap.Remove(keypath);
				}
			}
		}

		
		public void Release(RrdDb rrdDb)
		{
			if(rrdDb == null)
			{
				// we don't want NullPointerException
				return;
			}
			RrdFile rrdFile = rrdDb.RrdFile;
			if(rrdFile == null) 
			{
				throw new RrdException("Cannot release: already closed");
			}
			string path = rrdFile.FilePath;
			string keypath = GetCanonicalPath(path);
			if(rrdMap.ContainsKey(keypath))
			{
				RrdEntry rrdEntry = (RrdEntry) rrdMap[keypath];
				rrdEntry.Release();
				Debug("RELEASED: " + rrdEntry.Dump());
			}
			else 
			{
				throw new RrdException("RrdDb with path " + keypath + " not in pool");
			}
			GC();
		}

		private void GC()
		{
			int mapSize = rrdMap.Count;
			if(mapSize <= capacity)
			{
				Debug("GC: no need to run");
				return;
			}
			
			Debug("GC: finding the oldest released entry");
			ArrayList valueList = rrdMap.Values;
			ArrayList releasedEntries = new ArrayList();
			foreach (RrdEntry rrdEntry in valueList)
			{
				if(rrdEntry.IsReleased())
				{
					releasedEntries.Add(rrdEntry);
				}
			}
			if(releasedEntries.Count == 0)
			{
				Debug("GC: no released entries found, nothing to do");
				return;
			}
			Debug("GC: found " + releasedEntries.Count + " released entries");
			
            RrdEntry oldestEntry = (RrdEntry)
				Collections.min(releasedEntries, releaseDateComparator);
			Debug("GC: oldest released entry found: " + oldestEntry.Dump());
			oldestEntry.Close();
			rrdMap.Remove(oldestEntry);
			Debug("GC: oldest entry closed and removed. ");
			Debug("GC: number of entries reduced from " + mapSize + " to " + rrdMap.Count);
		}

		~RrdDbPool()
		{
			Reset();
		}

		
		
		
		public void Reset()
		{
			lock(this)
			{
				ArrayList al = rrdMap.Values;
				foreach (RrdEntry rrdEntry in al) 
				{
					rrdEntry.Close();
				}
				rrdMap.Clear();
				Debug("Nothing left in the pool");
			}
		}

		static string GetCanonicalPath(string path)
		{
			return new FileInfo(path).FullName;
		}

		internal static void Debug(string msg) {
			if(DEBUG) {
				Console.WriteLine("POOL: " + msg);
			}
		}

		public string Dump()
		{
			lock(this)
			{
				StringBuilder buff = new StringBuilder();
				ArrayList al = rrdMap.Values;
				foreach (RrdEntry rrdEntry in al)
				{
					buff.Append(rrdEntry.Dump());
					buff.Append("\n");
				}
				return buff.ToString();
			}
		}

		public static int Capacity
		{
			get
			{
				return capacity;
			}
			set
			{
				RrdDbPool.capacity = value;
			}
		}

		private class RrdEntry : IComparable
		{
			private RrdDb rrdDb;
			private Date releaseDate;
			private int usageCount;

			public RrdEntry(RrdDb rrdDb)
			{
				this.rrdDb = rrdDb;
				ReportUsage();
			}

			public int CompareTo(Object rhs)
			{
				RrdEntry r2 = (RrdEntry) rhs;
				long diff = this.ReleaseDate.Ticks - r2.ReleaseDate.Ticks;
				return diff < 0? -1: (diff == 0? 0: +1);
			}
			
			public RrdDb getRrdDb() 
			{
				return rrdDb;
			}

			internal void ReportUsage()
			{
				releaseDate = null;
				usageCount++;
			}

			internal void Release()
			{
				if(usageCount > 0) {
					usageCount--;
					if(usageCount == 0)
					{
						releaseDate = new DateTime();
					}
				}
			}

			internal bool IsReleased()
			{
				return usageCount == 0;
			}

			internal int UsageCount
			{
				get
				{
					return usageCount;
				}
			}

			internal DateTime ReleaseDate
			{
				get
				{
					return releaseDate;
				}
			}

			internal void Close()
			{
				rrdDb.Close();
			}

			internal string Dump()
			{
				string keypath = GetCanonicalPath(rrdDb.RrdFile.FilePath);
				return keypath + " [" + usageCount + "]";
			}
			
		}
	}


}