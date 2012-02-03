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
	/// Class to represent internal RRD archive state for a single datasource.
	/// </summary>
	/// <remarks>Objects of this class are never manipulated directly, it's up to the RRDSharp 
	/// framework to manage internal arcihve states.
	/// </remarks>
	public class ArcState : IRrdUpdatable
	{
		private Archive parentArc;

		private RrdDouble accumValue;
		private RrdLong nanSteps;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentArc"></param>
		public ArcState(Archive parentArc) 
		{
			this.parentArc = parentArc;
			if(RrdFile.RrdMode == RrdFile.MODE_CREATE) 
			{
				// should initialize
				Header header = parentArc.ParentDb.Header;
				long step = header.Step;
				long lastUpdateTime = header.LastUpdateTime;
				long arcStep = parentArc.ArcStep;
				long nan = (Util.Normalize(lastUpdateTime, step) -
					Util.Normalize(lastUpdateTime, arcStep)) / step;
				accumValue = new RrdDouble(Double.NaN, this);
				nanSteps = new RrdLong(nan, this);
			}
			else 
			{
				accumValue = new RrdDouble(this);
				nanSteps = new RrdLong(this);
			}
			
		}

		/// <summary>
		/// 
		/// </summary>
		public RrdFile RrdFile
		{
			get
			{
				return parentArc.ParentDb.RrdFile;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Dump()
		{
			return "accumValue:" + accumValue.Get() + " nanSteps:" + nanSteps.Get() + "\n";
		}

		/// <summary>
		/// 
		/// </summary>
		public long NanSteps
		{
			get
			{
				return nanSteps.Get();
			}	
			set
			{
				nanSteps.Set(value);
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
			set
			{
				accumValue.Set(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Archive Parent 
		{
			get
			{
				return parentArc;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		public void CopyStateTo(IRrdUpdatable other)
		{
			if(!(other is ArcState)) 
			{
				throw new RrdException("Cannot copy ArcState object to " + other.ToString());
			}
			ArcState arcState = (ArcState) other;
			arcState.accumValue.Set(accumValue.Get());
			arcState.nanSteps.Set(nanSteps.Get());
}

	}
}