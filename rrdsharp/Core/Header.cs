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
	/// Class to represent RRD file header.
	/// </summary>
	/// <remarks>
	/// Header information is mainly static (once set, it cannot be changed), with the exception of 
	/// last update time (this value is changed whenever RRD file gets updated).
	///
	/// Normally, you don't need to manipulate the Header object directly - the RRDSharp 
	/// framework does it for you.
	/// </remarks>
	public class Header : IRrdUpdatable
	{
		internal static readonly string SIGNATURE = "RRDSharp version 0.1";
		//static readonly string RRDTOOL_VERSION = "0001";

		private RrdDb parentDb;

		private RrdString signature;
		private RrdLong step;
		private RrdInt dsCount, arcCount;
		private RrdLong lastUpdateTime;

		internal Header(RrdDb parentDb)
		{
			this.parentDb = parentDb;
			signature = new RrdString(this);
			if(!signature.Get().Equals(SIGNATURE))
			{
				throw new RrdException("Not an RRDSharp file");
			}
			step = new RrdLong(this);
			dsCount = new RrdInt(this);
			arcCount = new RrdInt(this);
			lastUpdateTime = new RrdLong(this);
		}

		internal Header(RrdDb parentDb, RrdDef rrdDef)
		{
			this.parentDb = parentDb;
			signature = new RrdString(SIGNATURE, this);
			step = new RrdLong(rrdDef.Step, this);
			dsCount = new RrdInt(rrdDef.DsCount, this);
			arcCount = new RrdInt(rrdDef.ArcCount, this);
			lastUpdateTime = new RrdLong(rrdDef.StartTime, this);
		}

		/// <summary>
		/// 
		/// </summary>
		public string Signature 
		{
			get
			{
				return signature.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long LastUpdateTime 
		{
			get
			{
				return lastUpdateTime.Get();
			}
			set
			{
				this.lastUpdateTime.Set(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long Step 
		{
			get
			{
				return step.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int DsCount 
		{
			get
			{
				return dsCount.Get();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int ArcCount 
		{
			get
			{
				return arcCount.Get();
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

		internal string Dump()
		{
			return "== HEADER ==\n" +
				"signature:" + Signature +
				" lastUpdateTime:" + LastUpdateTime +
				" step:" + Step +
				" dsCount:" + DsCount +
				" arcCount:" + ArcCount + "\n";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		public void CopyStateTo(IRrdUpdatable other)
		{
			if(!(other is Header)) 
			{
				throw new RrdException("Cannot copy Header object to " + other.ToString());
			}
			Header header = (Header) other; 
			header.lastUpdateTime.Set(lastUpdateTime.Get());
		}
	}
}