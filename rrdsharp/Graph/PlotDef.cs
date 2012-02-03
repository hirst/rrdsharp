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
using System.Drawing;
using System.Collections;
using RrdSharp.Core;

namespace RrdSharp.Graph
{
	internal  abstract class PlotDef
	{
		internal static readonly int PLOT_LINE 	= 0;
		internal static readonly int PLOT_AREA 	= 1;
		internal static readonly int PLOT_STACK	= 2;
		
		internal bool visible 					= true;
		internal bool stacked					= false;
		internal int plotType					= PLOT_LINE;	// Unknown plotdef is a line
			
		internal string sourceName				= "";
		internal Source source					= null;
		internal Color color					= Color.Black;	// Default color is black

		
		internal PlotDef() 
		{
		}
		
		internal  PlotDef( string sourceName, Color color )
		{
			this.sourceName = sourceName;
			this.color		= color;
			
			// If no color is given, we should not plot this source
			if ( color.IsEmpty ) 
				visible = false;	
		}
		
		internal PlotDef( Source source, Color color, bool stacked, bool visible )
		{
			this.source		= source;
			this.color		= color;
			this.stacked	= stacked;
			this.visible	= visible;
		}
		
		
		internal virtual void SetSource( Source[] sources, Hashtable sourceIndex )
		{
			if ( sourceIndex.ContainsKey(sourceName) )
			{
				source = sources[ ((int) sourceIndex[sourceName]) ];
			}
			else
				throw new RrdException( "Invalid DEF or CDEF: " + sourceName );
		}

		internal virtual double GetValue( int tblPos, long[] timestamps )
		{
			return source.values[tblPos];	
		}
		
		
		internal abstract void Draw( ChartGraphics g, int[] xValues, int[] stackValues, int lastPlotType ) ;
		
		internal Source Source
		{
			get
			{
				return source;
			}
		}
		
		internal string SourceName
		{
			get
			{
				return sourceName;
			}
		}
		
		internal int PlotType
		{
			get
			{
				return plotType;
			}
		}
		
		internal Color Color
		{
			get
			{
				return color;
			}
		}
	}
}