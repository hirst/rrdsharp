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
	internal class Line : PlotDef
	{
		protected int lineWidth		= 1;			// Default line width of 1 pixel
		
		
		internal Line():base()
		{
		} 

		internal Line( string sourceName, Color color ) : base( sourceName, color)
		{
		}
		
		internal Line( string sourceName, Color color, int lineWidth ) : this (sourceName, color)
		{
			this.lineWidth	= lineWidth;
		}
		
		internal Line( Source source, Color color, bool stacked, bool visible ) : base(source, color, stacked, visible)
		{
		}
		
		internal override void Draw( ChartGraphics g, int[] xValues, int[] stackValues, int lastPlotType )
		{
			g.Pen =  new Pen(color, lineWidth);
			g.Color = color;

			double[] values = source.Values;

			int ax = 0, ay = 0;
			int nx = 0, ny = 0;
		
			for (int i = 0; i < xValues.Length; i++)
			{
				nx = xValues[i];
				ny = g.GetY(values[i]);
			
				if ( stacked && ny != Int32.MinValue )
					ny += stackValues[i];
			
				if ( visible && ny != Double.NaN && nx != 0 && ay != Int32.MinValue && ny != Int32.MinValue)
					g.DrawLine( ax, ay, nx, ny );
			
				
				stackValues[i] 	= ny;
				ax 				= nx;
				ay 				= ny;
			}
		
			//g.Pen =  new Pen();
		}
		
		internal int LineWidth
		{
			get
			{
				return lineWidth;
			}
		}
	}

}