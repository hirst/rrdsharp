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

namespace RrdSharp.Graph
{
	internal class Pdef : Source
	{
		private Plottable plottable;
	
		private int index 			= 0;
		private string sourceName	= null;
		private bool indexed 		= false;
		private bool named			= false;
		
		internal Pdef( string name, Plottable plottable ) : base(name)
		{
			this.plottable = plottable;
		}
	
		internal Pdef( string name, Plottable plottable, int index ) : base(name)
		{
			this.plottable 	= plottable;
			this.index		= index;
			indexed			= true;
		}
	
		internal Pdef( string name, Plottable plottable, string sourceName)  : base(name)
		{
			this.plottable 	= plottable;
			this.sourceName	= sourceName;
			named			= true;
		}

		internal void Prepare( int numPoints )
		{
			// Create values table of correct size
			values = new double[numPoints];
		}
	
		internal void Set( int pos, long timestamp )
		{
			double val = Double.NaN;
		
			if ( indexed )
				val = plottable.GetValue( timestamp, index );
			else if ( named )
				val = plottable.GetValue( timestamp, sourceName );
			else
				val = plottable.GetValue( timestamp );
		
			base.Set( pos, timestamp, val );
		
			values[pos] = val;
		}
	}
}