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
	internal class Legend : Comment 
	{
		private Color color		= Color.White;
		private int refPlotDef	= -1;
	
	
		internal Legend( string text ) : base (text)
		{
			this.commentType = Comment.CMT_LEGEND;
		}
	
		internal Legend( string text, Color color ) : base (text)
		{
			if (text == null )
				this.commentType = Comment.CMT_NOLEGEND;
			else
                this.commentType = Comment.CMT_LEGEND;
			this.color = color;
		}
	
		internal Legend( string text, Color color, int referredPlotDef) : this (text, color)
		{
			refPlotDef = referredPlotDef;
		}

		internal Color Color
		{
			get
			{
				return color;
			}
		}

		internal int PlotDefIndex
		{
			get
			{
				return refPlotDef;
			}
		}

	}
}