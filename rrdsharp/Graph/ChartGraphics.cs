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
	internal class ChartGraphics 
	{
		private Graphics g;
		private int width, height;
		private long xStart, xEnd;
		private double yStart, yEnd;
		private double widthDelta = 1.0d, heightDelta = 3.0d;
		private SolidBrush b;
		private Pen p;
		private Color c;

		
		internal ChartGraphics( Graphics graphics )
		{
			g = graphics;
			p = new Pen(System.Drawing.Color.Black);
			b = new SolidBrush(System.Drawing.Color.White);
		}


		internal void DrawLine(int x1, int y1, int x2, int y2)
		{
			g.DrawLine(p, x1, -y1, x2, -y2 );
		}

		internal void FillRect(int x1, int y1, int x2, int y2)
		{
			g.FillRectangle( b, x1, -y2, x2 - x1, (y2 - y1) );
		}
	
		
		internal Color Color
		{
			set
			{
				c = value;
				p.Color = value;
				b.Color = value;
			}
		}

		
		internal void SetDimensions( int width, int height )
		{
			this.width  = width;
			this.height	= height;
		}

		internal void SetXRange( long start, long end )
		{
			xStart 	= start;
			xEnd	= end; 
	
			if ( xEnd != xStart )
				widthDelta = width * 1.0d / (( xEnd - xStart) * 1.0d);
			else
				widthDelta = 1.0d;
		}

		internal void SetYRange( double lower, double upper )
		{
			yStart 	= lower;
			yEnd	= upper; 
	
			if ( yEnd != yStart )
				heightDelta = height * 1.0d / (( yEnd - yStart) * 1.0d);
			else
				heightDelta = 1.0d;
		}

		internal int GetX( long timestamp )
		{
			return (int) ((timestamp - xStart) * widthDelta);
		}

		internal int GetY( double data )
		{
			if ( Double.IsNaN(data) ) return Int32.MinValue;
	
			int tmp = (int) ((data - ( yStart < 0 ? 0 : Math.Abs(yStart) ) ) * heightDelta);
	
			return ( tmp > data * heightDelta ? tmp -1 : tmp); 
		}

		
		internal Pen Pen
		{
			set
			{
				p = value;
			}
		}
	
		
		internal int MinX
		{
			get
			{
				return 0;
			}
		}
	
		internal int MaxX
		{
			get
			{
				return 0 + width;
			}
		}
	
		internal int MinY
		{
			get
			{
				return 0;
			}
		}
	
		internal int MaxY
		{
			get
			{
				return 0 + height;
			}
		}
	
		internal Graphics Graphics
		{
			get
			{
				return g;
			}
		}
	}
	
}