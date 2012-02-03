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
	internal class Source 
	{
		internal const int AGG_MINIMUM	= 0;
		internal const int AGG_MAXIMUM	= 1;
		internal const int AGG_AVERAGE	= 2;
		internal const int AGG_FIRST	= 3;
		internal const int AGG_LAST		= 4;
		internal const int AGG_TOTAL	= 5;

	
		internal static readonly string[] aggregates = { "MINIMUM", "MAXIMUM", "AVERAGE", "FIRST", "LAST", "TOTAL" };
		internal double[] values;
	
		private string name;
		private double min						= Double.NaN;
		private double max						= Double.NaN;
		private double lastValue 				= Double.NaN;
		private double totalValue				= 0;
		
		private long lastTime					= 0;
		private long totalTime					= 0;
 
			
		internal Source( string name )
		{
			this.name = name;
		}
	
	
		internal virtual void Set( int pos, long time, double val )
		{
			Aggregate( time, val );		
		}
	
		internal double Get( int pos ) 
		{
			return values[pos];
		}
	
		internal double GetAggregate( int aggType )
		{
			switch ( aggType )
			{
				case AGG_MINIMUM:
					return min;
				
				case AGG_MAXIMUM:
					return max;
				
				case AGG_AVERAGE:
					if ( totalTime > 0 )
						return totalValue / totalTime;
					break;
				
				case AGG_FIRST:
					if ( values != null && values.Length > 0)
						return values[0];
					break;
				
				case AGG_LAST:
					if ( values != null && values.Length > 0)
						return values[values.Length - 1];
					break;	

				case AGG_TOTAL:
					return totalValue;
			}
		
			return Double.NaN;
		}
	
		internal string Name
		{
			get
			{
				return name;	
			}
		}
	
		internal double[] Values
		{
			get
			{
				return values;
			}
		}

		internal long SampleCount
		{
			get
			{
				return (values != null? values.Length : 0);
			}
		}

		private void Aggregate( long time, double val ) 
		{
			min = Util.Min( min, val );
			max = Util.Max( max, val );
		
			if ( !Double.IsNaN(lastValue) && !Double.IsNaN(val) )
			{
				long timeDelta 	= time - lastTime;
				totalValue		+= timeDelta * ( val + lastValue ) / 2.0;
				totalTime		+= timeDelta;
			}
		
			lastTime	= time;
			lastValue	= val;
		}
	}

}