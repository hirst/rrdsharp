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
using RrdSharp.Core;

namespace RrdSharp.Graph
{
	internal class Stack : PlotDef
	{
		internal Stack( string sourceName, Color color ) : base ( sourceName, color)
		{
			this.plotType	= PlotDef.PLOT_STACK;
		}


	
		internal override void Draw( ChartGraphics g, int[] xValues, int[] stackValues, int lastPlotType )
		{
			PlotDef stack = null;
				
			try
			{
				if ( lastPlotType == PlotDef.PLOT_LINE )
					stack = new Line( source, color, true, visible );	
				else if ( lastPlotType == PlotDef.PLOT_AREA )
					stack = new Area( source, color, true, visible );
				
				stack.Draw( g, xValues, stackValues, lastPlotType );
			}
			catch (Exception) 
			{
				throw new RrdException( "Could not stack source: " + sourceName );
			}
			
		}
	}

}