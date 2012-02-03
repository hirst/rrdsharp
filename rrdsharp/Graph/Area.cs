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

namespace RrdSharp.Graph
{
	
	internal class Area : PlotDef
	{
		protected int lineWidth		= 1;			// Default line width of 1 pixel
	
		internal Area( string sourceName, Color color ) : base (sourceName, color)
		{
			this.plotType	= PlotDef.PLOT_AREA;
		}
	
		internal Area( Source source, Color color, bool stacked, bool visible ) : base (source, color, stacked, visible)
		{	
		}
	
		internal override void Draw( ChartGraphics g, int[] xValues, int[] stackValues, int lastPlotType )
		{
			g.Pen =  new Pen(color, lineWidth);
			g.Color = color;
		
			double [] values = source.Values;

			int ax = 0, ay = 0, py;
			int nx = 0, ny = 0;

			for (int i = 0; i < xValues.Length; i++)
			{
				py = 0;

				nx = xValues[i];
				ny = g.GetY(values[i]);
			
				if ( !Double.IsNaN(values[i]) )
				{
					if ( stacked )
					{
						py 	= stackValues[i];
						ny += ( stackValues[i] == Int32.MinValue ? Int32.MinValue : stackValues[i] );
					}

					if ( visible )
					{
						if (nx > ax + 1)	// More than one pixel hop, draw intermediate pixels too
						{
							// For each pixel between nx and ax, calculate the y, plot the line
							int co 	= (ny - ay) / (nx - ax);
							int j 	= (ax > 0 ? ax : 1 );		// Skip 0 
				
							for (j = ax; j <= nx; j++)
								if ( ay != Int32.MinValue && ny != Int32.MinValue )
									g.DrawLine( j, py, j, ( co * (j - ax) + ay) );
						}
						else if ( nx != 0 && py != Int32.MinValue && ny != Int32.MinValue )
							g.DrawLine( nx, py, nx, ny );
					}
			
				
				}
			
				// Special case with NaN doubles
			
				stackValues[i] 	= ny;
				ax 				= nx;
				ay 				= ny;
			}
		}
	}
}
