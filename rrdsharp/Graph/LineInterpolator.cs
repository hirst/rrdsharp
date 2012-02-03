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
using RrdSharp.Core;

namespace RrdSharp.Graph
{
	/// <summary>
	/// 
	/// </summary>
	public class LinearInterpolator : Plottable 
	{
		/// <summary>
		/// 
		/// </summary>
		public const int INTERPOLATE_LEFT = 0;
		/// <summary>
		/// 
		/// </summary>
		public const int INTERPOLATE_RIGHT = 1;
		/// <summary>
		/// 
		/// </summary>
		public const int INTERPOLATE_LINEAR = 2;

		private int lastIndexUsed = 0;

		private int interpolationMethod = INTERPOLATE_LINEAR;

		private long[] timestamps;
		private double[] values;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamps"></param>
		/// <param name="values"></param>
		public LinearInterpolator(long[] timestamps, double[] values)
		{
			this.timestamps = timestamps;
			this.values = values;
			Validate();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dates"></param>
		/// <param name="values"></param>
		public LinearInterpolator(DateTime[] dates, double[] values)
		{
			this.values = values;
			timestamps = new long[dates.Length];
			for(int i = 0; i < dates.Length; i++) {
				timestamps[i] = Util.GetTimestamp(dates[i]);
			}
			Validate();
		}

		private void Validate()
		{
			bool ok = true;
			if(timestamps.Length != values.Length || timestamps.Length < 2) 
			{
				ok = false;
			}
			for(int i = 0; i < timestamps.Length - 1 && ok; i++)
			{
				if(timestamps[i] >= timestamps[i + 1])
				{
					ok = false;
				}
			}
			if(!ok)
			{
				throw new RrdException("Invalid plottable data supplied");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int InterpolationMethod
		{
			set
			{
				this.interpolationMethod = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamp"></param>
		/// <returns></returns>
		public override double GetValue(long timestamp)
		{
			int count = timestamps.Length;
			// check if out of range
			if(timestamp < timestamps[0] || timestamp > timestamps[count - 1])
			{
				return Double.NaN;
			}
			// find matching segment
			int startIndex = lastIndexUsed;
			if(timestamp < timestamps[lastIndexUsed])
			{
				// backward reading, shift to the first timestamp
				startIndex = 0;
			}
			for(int i = startIndex; i < count; i++)
			{
				if(timestamps[i] == timestamp) {
					return values[i];
				}
				if(i < count - 1 && timestamps[i] < timestamp && timestamp < timestamps[i + 1]) 
				{
					// matching segment found
					lastIndexUsed = i;
					switch(interpolationMethod)
					{
						case INTERPOLATE_LEFT:
							return values[i];
						case INTERPOLATE_RIGHT:
							return values[i + 1];
						case INTERPOLATE_LINEAR:
							double slope = (values[i + 1] - values[i]) /
								(timestamps[i + 1] - timestamps[i]);
							return values[i] + slope * (timestamp - timestamps[i]);
						default:
							return Double.NaN;
					}
				}
			}
			// should not be here ever, but let's satisfy the compiler
			return Double.NaN;
		}
	}
}