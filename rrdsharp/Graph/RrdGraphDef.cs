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
using System.Globalization;
using System.IO;
using System.Collections;
using RrdSharp.Core;


namespace RrdSharp.Graph
{
	/// <summary>
	/// 
	/// </summary>
	public class RrdGraphDef
	{
		private long endTime				= Util.TicksToMillis(DateTime.UtcNow.Ticks);					// default time spam of the last 24 hours
		private long startTime				= Util.TicksToMillis(DateTime.UtcNow.Ticks) - 86400L;
		
		private Title title					= null;							// no title
		private string valueAxisLabel		= null;							// no vertical label
				
		private bool gridX					= true;							// hide entire X axis grid (default: no)
		private bool gridY					= true;							// hide entire Y axis grid (default: no)
		private bool minorGridX				= true;							// hide minor X axis grid (default: no)
		private bool minorGridY				= true;							// hide minor Y axis grid (default: no)
		private bool majorGridX				= true;							// hide major X axis grid with labels (default: no)
		private bool majorGridY				= true;							// hide major Y axis grid with labels (default: no)
		private bool frontGrid				= true;							// show grid in front of the chart (default: yes)
		private bool antiAliasing			= true;							// use anti-aliasing for the chart (default: yes)
		private bool showLegend				= true;							// show legend and comments (default: yes)
		private bool drawSignature			= true;							// show RrdShap url signature (default: yes)
			
		private Color backColor				= Color.FromArgb(245,245,245);	// variation of light gray
		private Color canvasColor			= Color.White;					// white
		private Color borderColor			= Color.LightGray;				// light gray, only applicable with a borderStroke
		private Color normalFontColor		= Color.Black;					// black
		private Color titleFontColor		= Color.Black;					// black
		private Color majorGridColor		= Color.FromArgb(130,30,30);	// variation of dark red
		private Color minorGridColor		= Color.FromArgb(140,140,140);	// variation of gray
		private Color axisColor				= Color.FromArgb(130,30,30);	// variation of dark red
		private Color arrowColor			= Color.Red;					// red
		private Color frameColor			= Color.LightGray;				// light gray
		
		private Font titleFont 				= null;							// use default 'grapher' font
		private Font normalFont 			= null;							// use default 'grapher' font
		
		private FileStream background		= null;							// no background image by default
		private FileStream overlay			= null;							// no overlay image by default
		
		private int chart_lpadding			= Grapher.CHART_LPADDING;		// padding space on the left of the chart area

		private int firstDayOfWeek			= TimeAxisUnit.MONDAY;
		
		private double baseValue			= ValueFormatter.DEFAULT_BASE;	// unit base value to use (default: 1000)
		private int scaleIndex				= ValueFormatter.NO_SCALE;		// fixed units exponent value to use
		
		private Pen borderPen				= null;							// defaults to standard beveled border
		private TimeAxisUnit tAxis			= null;							// custom time axis grid, defaults to no custom
		private ValueAxisUnit vAxis			= null;							// custom value axis grid, defaults to no custom
		private GridRange gridRange			= null;							// custom value range definition, defaults to auto-scale
		
		// -- Non-settable members
		private int numDefs					= 0;							// number of Def datasources added
		private int commentLines			= 0;							// number of complete lines in the list of comment items
		private int commentLineShift		= 0;							// modifier to add to get minimum one complete line of comments
		
		private Hashtable fetchSources		= new Hashtable();				// holds the list of FetchSources
		private ArrayList cdefList			= new ArrayList();				// holds the list of Cdef datasources
		private ArrayList pdefList			= new ArrayList();
		private ArrayList plotDefs			= new ArrayList();				// holds the list of PlotDefs
		private ArrayList comments			= new ArrayList();				// holds the list of comment items
		
			
		/// <summary>
		/// 
		/// </summary>
		public RrdGraphDef()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		public RrdGraphDef( long startTime, long endTime ) 
		{
			SetTimePeriod( startTime, endTime );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public RrdGraphDef( DateTime start, DateTime end)
		{
			SetTimePeriod( start, end );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		public void SetTimePeriod( long startTime, long endTime )
		{
			if ( startTime < 0 || endTime <= startTime )
				throw new RrdException( "Invalid graph start/end time: " + startTime + "/" + endTime );
			
			this.startTime 	= startTime;
			this.endTime 	= endTime;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public void SetTimePeriod( DateTime start, DateTime end )
		{
			SetTimePeriod(Util.TicksToMillis(start.Ticks)/1000L, Util.TicksToMillis(end.Ticks)/1000L );
		}

		
		internal Title GetTitle()
		{
			return this.title;
			
		}

		/// <summary>
		/// 
		/// </summary>
		public string Title
		{
			set
			{
				this.title = new Title( value );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string VerticalLabel
		{
			get
			{
				return valueAxisLabel;
			}
			set
			{
				this.valueAxisLabel = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string TimeAxisLabel
		{
			set
			{
				if ( value != null )
				{	
					TimeAxisLabel cmt	= new TimeAxisLabel( value );
					commentLines 		+= cmt.LineCount;
					commentLineShift	= (cmt.CompleteLine ? 0 : 1); 
						
					comments.Add( cmt );
				}
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Color BackColor
		{
			get
			{
				return this.backColor;
			}
			set
			{
				this.backColor = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color CanvasColor
		{
			get
			{
				return this.canvasColor;
			}
			set
			{
				this.canvasColor = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Color TitleFontColor
		{
			get
			{
				return this.titleFontColor;
			}
			set
			{
				this.titleFontColor = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Color DefaultFontColor
		{
			get
			{
				return this.normalFontColor;
			}
			set
			{
				this.normalFontColor = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Font TitleFont
		{
			get
			{
				return this.titleFont;
			}
			set
			{
				this.titleFont = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Font DefaultFont
		{
			get
			{
				return this.normalFont;
			}
			set
			{
				this.normalFont = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Color MajorGridColor
		{
			get
			{
				return this.majorGridColor;
			}
			set
			{
				this.majorGridColor = value;	
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color MinorGridColor
		{
			get
			{
				return this.minorGridColor;
			}
			set
			{
				this.minorGridColor = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color FrameColor
		{
			get
			{
				return this.frameColor;
			}
			set
			{
				this.frameColor = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color AxisColor
		{
			get
			{
				return this.axisColor;
			}
			set
			{
				this.axisColor = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color ArrowColor
		{
			get
			{
				return this.arrowColor;
			}
			set
			{
				this.arrowColor = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public bool MinorGridX
		{
			get
			{
				return this.minorGridX;
			}
			set
			{
				this.minorGridX = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool MinorGridY
		{
			get
			{
				return this.minorGridY;
			}
			set
			{
				this.minorGridY = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool MajorGridX
		{
			get
			{
				return this.majorGridX;
			}
			set
			{
				this.majorGridX = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool MajorGridY
		{
			get
			{
				return this.majorGridY;
			}
			set
			{
				this.majorGridY = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool GridX
		{
			get
			{
				return this.gridX;
			}
			set
			{
				this.gridX		= value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool GridY
		{
			get
			{
				return this.gridY;
			}
			set
			{
				this.gridY		= value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool FrontGrid
		{
			get
			{
				return this.frontGrid;
			}
			set
			{
				this.frontGrid = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool ShowLegend
		{
			get
			{
				return this.showLegend;
			}
			set
			{
				this.showLegend	= value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public bool ShowSignature
		{
			get
			{
				return this.drawSignature;
			}
			set
			{
				this.drawSignature = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public bool AntiAliasing
		{
			get
			{
				return this.antiAliasing;
			}
			set
			{
				this.antiAliasing = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public int ChartLeftPadding
		{
			get
			{
				return this.chart_lpadding;
			}
			set
			{
				this.chart_lpadding = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public FileStream GetBackground()
		{
			return this.background;
		}

		/// <summary>
		/// 
		/// </summary>
		public string Background
		{
			set
			{
				if ( File.Exists(value) )
					this.background = new FileStream(value, FileMode.Open, FileAccess.Read);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public FileStream GetOverlay()
		{
			return this.overlay;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string Overlay
		{
			set
			{
				if ( File.Exists(value) )
					this.overlay = new FileStream(value, FileMode.Open, FileAccess.Read);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double BaseValue
		{
			get
			{
				return this.baseValue;
			}
			set
			{
				this.baseValue = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int UnitsExponent
		{
			set
			{
				this.scaleIndex = (6 - value / 3);	// Index in the scale table
			}
		}
		
		internal long StartTime
		{
			get
			{
				return startTime;
			}
		}
		
		internal long EndTime
		{
			get
			{
				return endTime;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Color BorderColor
		{
			get
			{
				return borderColor;
			}
		}
		
		internal Pen BorderPen
		{
			get
			{
				return borderPen;
			}
		}
		
		internal int ScaleIndex
		{
			get
			{
				return scaleIndex;
			}
		}

		internal GridRange GridRange
		{
			get
			{
				return gridRange;
			}
		}
		
		internal ValueAxisUnit ValueAxis
		{
			get
			{
				return vAxis;
			}
		}
		
		internal TimeAxisUnit TimeAxis
		{
			get
			{
				return tAxis;
			}
		}
		
		internal PlotDef[] PlotDefs
		{
			get
			{
				if (plotDefs.Count == 0)
					return new PlotDef[0];
				else
                    return (PlotDef[])plotDefs.ToArray(typeof(PlotDef));
			}
		}
		
		internal Comment[] Comments
		{
			get
			{
				if (comments.Count == 0)
					return new Comment[0];
				else
                    return (Comment[]) comments.ToArray(typeof(Comment));
			}
		}
		
		internal int CommentLineCount
		{
			get
			{
				return ( comments.Count > 0 ? commentLines + commentLineShift : 0 ); 
			}
		}
		
		internal int NumDefs
		{
			get
			{
				return numDefs;
			}
		}
		
		internal Cdef[] Cdefs
		{
			get
			{
				if (cdefList.Count == 0)
					return new Cdef[0];
				else
					return (Cdef[]) cdefList.ToArray(typeof(Cdef));
			}
		}

		internal Pdef[] Pdefs
		{
			get
			{
				if (pdefList.Count == 0)
					return new Pdef[0];
				else
                    return (Pdef[]) pdefList.ToArray(typeof(Pdef));
			}
		}
		
			internal Hashtable FetchSources
		{
			get
			{
				return fetchSources;
			}
		}

		internal int FirstDayOfWeek
		{
			get
			{
				return firstDayOfWeek;
			}
			set
			{
				firstDayOfWeek = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="c"></param>
		/// <param name="w"></param>
		public void SetImageBorder( Color c, int w ) 
		{
			this.borderPen		= new Pen( c, w );
			if ( c.IsEmpty )
				this.borderColor	= Color.Black;
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lower"></param>
		/// <param name="upper"></param>
		/// <param name="rigid"></param>
		public void SetGridRange(double lower, double upper, bool rigid) 
		{
			gridRange = new GridRange( lower, upper, rigid );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gridStep"></param>
		/// <param name="labelStep"></param>
		public void SetValueAxis( double gridStep, double labelStep ) 
		{
			vAxis = new ValueAxisUnit( gridStep, labelStep );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="minGridTimeUnit"></param>
		/// <param name="minGridUnitSteps"></param>
		/// <param name="majGridTimeUnit"></param>
		/// <param name="majGridUnitSteps"></param>
		/// <param name="dateFormat"></param>
		/// <param name="centerLabels"></param>
		public void SetTimeAxis( int minGridTimeUnit, int minGridUnitSteps, int majGridTimeUnit, int majGridUnitSteps, 
								string dateFormat, bool centerLabels ) 
		{
			this.tAxis = new TimeAxisUnit( minGridTimeUnit, minGridUnitSteps, majGridTimeUnit, majGridUnitSteps, 
										dateFormat, centerLabels, firstDayOfWeek );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="file"></param>
		/// <param name="dsName"></param>
		/// <param name="consolFunc"></param>
		public void Datasource( string name, string file, string dsName, string consolFunc )
		{
			if ( fetchSources.ContainsKey(file) )
			{
				FetchSource rf = (FetchSource) fetchSources[file];
				rf.AddSource( consolFunc, dsName, name );	
			}
			else
				fetchSources[file] =  new FetchSource(file, consolFunc, dsName, name);
			
			numDefs++;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="rpn"></param>
		public void Datasource( string name, string rpn ) 
		{
			cdefList.Add( new Cdef(name, rpn) );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="plottable"></param>
		public void Datasource( string name, Plottable plottable )
		{
			pdefList.Add( new Pdef(name, plottable) );
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="plottable"></param>
		/// <param name="index"></param>
		public void Datasource( string name, Plottable plottable, int index )
		{
			pdefList.Add( new Pdef(name, plottable, index) );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="plottable"></param>
		/// <param name="sourceName"></param>
		public void Datasource( string name, Plottable plottable, string sourceName )
		{
			pdefList.Add( new Pdef(name, plottable, sourceName) );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		public void Line( string sourceName, Color color, string legend )
		{
			plotDefs.Add( new Line(sourceName, color) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		/// <param name="lineWidth"></param>
		public void Line( string sourceName, Color color, string legend, int lineWidth )
		{
			plotDefs.Add( new Line(sourceName, color, lineWidth) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="v1"></param>
		/// <param name="t2"></param>
		/// <param name="v2"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		/// <param name="lineWidth"></param>
		public void Line( DateTime t1, double v1, DateTime t2, double v2, Color color, string legend, int lineWidth )
		{
			plotDefs.Add( new CustomLine( Util.TicksToMillis(t1.Ticks) / 1000, v1, Util.TicksToMillis(t2.Ticks) / 1000, v2, color, lineWidth ) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		public void Area( string sourceName, Color color, string legend )
		{
			plotDefs.Add( new Area(sourceName, color) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="v1"></param>
		/// <param name="t2"></param>
		/// <param name="v2"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		public void Area( DateTime t1, double v1, DateTime t2, double v2, Color color, string legend )
		{
			plotDefs.Add( new CustomArea( Util.TicksToMillis(t1.Ticks) / 1000, v1, Util.TicksToMillis(t2.Ticks) / 1000, v2, color ) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		public void Stack( string sourceName, Color color, string legend )
		{
			plotDefs.Add( new Stack(sourceName, color) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		public void Hrule(double val, Color color, string legend)
		{
			plotDefs.Add( new CustomLine( Int64.MinValue, val, Int64.MaxValue, val, color ) );
			AddLegend( legend, color );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		/// <param name="lineWidth"></param>
		public void Hrule(double val, Color color, string legend, int lineWidth)
		{
			plotDefs.Add( new CustomLine( Int64.MinValue, val, Int64.MaxValue, val, color, lineWidth ) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamp"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		public void Vrule( DateTime timestamp, Color color, string legend )
		{
			long timeSecs = Util.TicksToMillis(timestamp.Ticks) / 1000;
			plotDefs.Add( new CustomLine( timeSecs, Double.MinValue, timeSecs, Double.MaxValue, color ) );
			AddLegend( legend, color );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamp"></param>
		/// <param name="color"></param>
		/// <param name="legend"></param>
		/// <param name="lineWidth"></param>
		public void Vrule( DateTime timestamp, Color color, string legend, int lineWidth )
		{
			long timeSecs = Util.TicksToMillis(timestamp.Ticks) / 1000;
			plotDefs.Add( new CustomLine( timeSecs, Double.MinValue, timeSecs, Double.MaxValue, color, lineWidth ) );
			AddLegend( legend, color );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		public void Comment(string text)
		{
			AddComment( new Comment(text) );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="consolFun"></param>
		/// <param name="format"></param>
		public void Gprint(string sourceName, string consolFun, string format)
		{
			AddComment( new Gprint(sourceName, consolFun, format) );
		}
		
			
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="consolFun"></param>
		/// <param name="format"></param>
		/// <param name="base1"></param>
		public void Gprint( string sourceName, string consolFun, string format, double base1)
		{
			AddComment( new Gprint(sourceName, consolFun, format, base1) );
		}

		private void AddComment( Comment cmt )
		{
			commentLines 		+= cmt.LineCount;
			commentLineShift	= (cmt.CompleteLine ? 0 : 1); 
			comments.Add( cmt );
		}
		
		private void AddLegend( string legend, Color color )
		{
			AddComment( new Legend(legend, color, plotDefs.Count -1 ) );
		}
	}

}