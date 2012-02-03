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

namespace RrdSharp.Graph
{
	internal class CustomArea : PlotDef
	{
		private long xVal1;
		private long xVal2;
		
		private double yVal1;
		private double yVal2;

		protected int lineWidth		= 1;

		internal CustomArea( long startTime, double startValue, long endTime, double endValue, Color color )
		{
			this.color = color;
			if ( color.IsEmpty )
				visible = false;
			
			this.xVal1 = startTime;
			this.xVal2 = endTime;
			this.yVal1 = startValue;
			this.yVal2 = endValue;
		}


		internal override void Draw( ChartGraphics g, int[] xValues, int[] stackValues, int lastPlotType )
		{
			g.Pen =  new Pen(color, lineWidth);
			g.Color =  color;
		
			int ax, ay, nx, ny;
		
			// Get X positions
			if ( xVal1 == Int64.MinValue )
				ax = g.MinX; 
			else if ( xVal1 == Int64.MaxValue )
				ax = g.MaxX;
			else
				ax = g.GetX( xVal1 );
		
			if ( xVal2 == Int64.MinValue )
				nx = g.MinX;
			else if ( xVal2 == Int64.MaxValue )
				nx = g.MaxX;
			else
				nx = g.GetX( xVal2 );
		
			// Get Y positions
			if ( yVal1 == Double.MinValue )
				ay = g.MinY;
			else if ( yVal1 == Double.MaxValue )
				ay = g.MaxY;
			else
				ay = g.GetY( yVal1 );
		
			if ( yVal2 == Double.MinValue )
				ny = g.MinY;
			else if ( yVal2 == Double.MaxValue )
				ny = g.MaxY;
			else
				ny = g.GetY( yVal2 );

			// Draw the area
			if ( visible )
			{
				if ( ny > ay )
					g.FillRect( ax, ay, nx, ny );
				else
					g.FillRect( ax, ny, nx, ay );
			}
			
			// Set the stackvalues
			// Always use the y value of the second specified point to stack on
			if ( yVal2 != Double.MaxValue )
				for (int i = 0; i < stackValues.Length; i++)
					if ( xValues[i] < ax || xValues[i] > nx ) 
						stackValues[i] = 0;
					else
						stackValues[i] = ny;
		}
		
		internal new double GetValue( int tblPos, long[] timestamps )
		{
			long time = timestamps[tblPos];
		
			// Out of range
			if ( time > xVal2 || time < xVal1 )
				return Double.NaN;
		
			if ( yVal2 == Double.MaxValue )
				return Double.NaN;
			
			return yVal2;
		}

		// Stubbed method, irrelevant for this PlotDef
		internal override void SetSource( Source[] sources, Hashtable sourceIndex )
		{
		}
	}

}