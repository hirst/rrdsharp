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
	internal class ValueExtractor 
	{
		private string[] varNames;			// Name of the variable, NOT it's dsName in the file

		private int[] tPos;
		private long[][] timestamps;
		private double[][][] dsValues;
		
		
		internal ValueExtractor( string[] names, FetchData[] values )
		{
			this.varNames	= names;
			
			// Set timestamps
			tPos			= new int[values.Length];
			timestamps 		= new long[values.Length][];
			dsValues		= new double[values.Length][][];
			
			for (int i = 0; i < timestamps.Length; i++) 
			{
				if ( values[i] != null ) 
				{
					timestamps[i] 	= values[i].Timestamps;
					dsValues[i]		= values[i].Values;
				}
			}


		}




		internal int Extract( long timestamp, Source[] sources, int row, int offset )
		{
			int tblPos 	= offset;
				
			for ( int i = 0; i < dsValues.Length; i++ ) 
			{
				if ( dsValues[i] == null )
					continue;
				
				int tIndex	= tPos[i];
				
				if ( timestamp < timestamps[i][ tIndex ] )
					throw new RrdException("Backward reading not allowed");
				
				while ( tIndex < timestamps[i].Length - 1 )
				{
					if ( timestamps[i][ tIndex ] <= timestamp && timestamp < timestamps[i][ tIndex + 1] ) 
					{
						for (int j = 0; j < dsValues[i].Length; j++)
							sources[tblPos++].Set( row, timestamp, dsValues[i][j][ tIndex + 1 ] );
						break;				
					}
					else {
						tIndex++;
					}
				}
				
				tPos[i] = tIndex;
			}
			
			return tblPos;
		}
		
		internal string[] Names
		{
			get
			{
				return varNames;
			}
		}
	}
}