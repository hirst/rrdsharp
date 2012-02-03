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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Collections;
using System.Text;
using System.IO;

using RrdSharp.Core;


namespace RrdSharp.Graph
{
	internal class Grapher 
	{
		internal static readonly string SPACER			= "   ";			// default comment spacer (three blank spaces)
		internal static readonly int GRAPH_RESOLUTION	= 400;				// default graph resolution
		internal static readonly int DEFAULT_WIDTH		= 400;				// default width in pixels of the chart area
		internal static readonly int DEFAULT_HEIGHT		= 100;				// default height in pixels of the chart area
		
		// Border space definitions
		internal static readonly int UBORDER_SPACE		= 10;				// padding from graph upper border
		internal static readonly int BBORDER_SPACE		= 10;				// padding from graph lower border
		internal static readonly int LBORDER_SPACE		= 10;				// padding from graph left border
		internal static readonly int RBORDER_SPACE		= 13;				// padding from graph right border
		
		internal static readonly int CHART_UPADDING		= 5;				// padding space above the chart area
		internal static readonly int CHART_BPADDING		= 25;				// default padding space below the chart area			
		internal static readonly int CHART_RPADDING		= 10;				// padding space on the right of the chart area
		internal static readonly int CHART_LPADDING		= 50;				// default padding space on the left of the chart area
		internal static readonly int CHART_BPADDING_NM	= 10;				// default padding below chart if no legend markers
		internal static readonly int CHART_LPADDING_NM	= 10;				// default padding left of chart if no legend markers
		
		internal static readonly int LINE_PADDING		= 4;				// default padding between two consecutive text lines
		
		// Default fonts 
		
		internal static readonly Font TITLE_FONT		= new Font("Lucida Console", 13, FontStyle.Bold, GraphicsUnit.Pixel);
		internal static readonly Font NORMAL_FONT		= new Font("Lucida Console", 11, FontStyle.Regular, GraphicsUnit.Pixel);
		
		private Font title_font 						= TITLE_FONT;		// font used for the title 
		private Font normal_font	 					= NORMAL_FONT;		// font used for all default text
		private Color normalFontColor;					
		private int numPoints 							= GRAPH_RESOLUTION;	// number of points used to calculate the graph
		
		private int chart_lpadding, chart_bpadding;							// calculated padding on the left and below the chart area
		private int imgWidth, imgHeight;									// dimensions of the entire image
		private int chartWidth, chartHeight;								// dimensions of the chart area within the image	
		private int nfont_width, nfont_height, tfont_width, tfont_height;	// font dimennsion specs (approximated)
		private int commentBlock;											// size in pixels of the block below the chart itself
		private int graphOriginX, graphOriginY, x_offset, y_offset;
		private SizeF stringLength						= new SizeF();
		private StringFormat sf;

		private RrdGraphDef graphDef;
		private RrdGraph rrdGraph;
		
		private Source[] sources;
		private Hashtable sourceIndex;
		private long[] timestamps;

		private ValueFormatter valueFormat;
		private Pen	defaultPen;
		private Pen dashPen;
		private SolidBrush defaultBrush;
		private ValueGrid vGrid;
		private TimeGrid tGrid;

		private long calculatedEndTime;
		
		
		internal Grapher( RrdGraphDef graphDef, RrdGraph rrdGraph )
		{
			this.graphDef = graphDef;
			this.rrdGraph = rrdGraph;
			
			// Set font dimension specifics
			if ( graphDef.DefaultFont != null )
				normal_font = graphDef.DefaultFont;
			if ( graphDef.TitleFont != null )
				title_font	= graphDef.TitleFont;
			normalFontColor	= graphDef.DefaultFontColor;
			
			nfont_height 	= (int) normal_font.GetHeight();	
			// MONO FIX: Font.GetHeight() always returns 0 on mono 1.0
			if (nfont_height == 0) nfont_height = (int)normal_font.Size;
            nfont_width		= nfont_height / 2+1 ;

			sf = StringFormat.GenericDefault;
			sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
			
			// Bold font is higher
			tfont_height	= (int) title_font.GetHeight();
			// MONO FIX: Font.GetHeight() always returns 0 on mono 1.0
			if (tfont_height == 0) tfont_height = (int)title_font.Size;
			tfont_width		= tfont_height / 2 +1;

			// Create the shared valueformatter
			valueFormat 	= new ValueFormatter( graphDef.BaseValue, graphDef.ScaleIndex );
			
			// Set default graph stroke
			defaultPen	= new Pen(Color.Black);
			dashPen = new Pen(Color.Black, 1);
			defaultBrush = new SolidBrush(Color.White);
		}
		
		
		internal Bitmap CreateImage( int cWidth, int cHeight)
		{
			// Calculate chart dimensions
			chartWidth			= ( cWidth == 0 ? DEFAULT_WIDTH : cWidth );
			chartHeight			= ( cHeight == 0 ? DEFAULT_HEIGHT : cHeight );

			if ( cWidth > 0 ) numPoints = cWidth;

			// Padding depends on grid visibility
			chart_lpadding 		= ( graphDef.MajorGridY ? graphDef.ChartLeftPadding : CHART_LPADDING_NM );
			chart_bpadding 		= ( graphDef.MajorGridX ? CHART_BPADDING : CHART_BPADDING_NM );
			
			// Size of all lines below chart
			commentBlock		= 0;
			if ( graphDef.ShowLegend )
				commentBlock 	= graphDef.CommentLineCount * (nfont_height + LINE_PADDING) - LINE_PADDING;		
		
			// x_offset and y_offset define the starting corner of the actual graph 
			x_offset			= LBORDER_SPACE;
			if ( graphDef.VerticalLabel != null ) 
				x_offset 		+= nfont_height + LINE_PADDING;
			imgWidth			= chartWidth + x_offset + RBORDER_SPACE + chart_lpadding + CHART_RPADDING;
			
			y_offset			= UBORDER_SPACE;
			if ( graphDef.GetTitle() != null )			// Title *always* gets a extra LF automatically 
				y_offset		+= ((tfont_height + LINE_PADDING) * graphDef.GetTitle().LineCount + tfont_height) + LINE_PADDING;
			imgHeight 			= chartHeight + commentBlock + y_offset + BBORDER_SPACE + CHART_UPADDING + CHART_BPADDING;
				
			// Create graphics object
			Bitmap bImg 	= new Bitmap( imgWidth, imgHeight );
			Graphics graphics	= Graphics.FromImage(bImg);
			
			// Do the actual graphing
			CalculateSeries();							// calculate all datasources
							
			PlotImageBackground( graphics );			// draw the image background
				
			PlotChart( graphics );						// draw the actual chart
				
			PlotComments( graphics );					// draw all comment lines
				
			PlotOverlay( graphics );					// draw a possible image overlay
				
			PlotSignature( graphics );					// draw the JRobin signature

			
			// Dispose graphics context
			graphics.Dispose();
			
			return bImg;
		}
		
		internal  Bitmap CreateImageGlobal( int cWidth, int cHeight) 
		{
			imgWidth			= cWidth;
			imgHeight			= cHeight;

			if ( cWidth > 0 ) numPoints = cWidth;

			// Padding depends on grid visibility
			chart_lpadding 		= ( graphDef.MajorGridY ? graphDef.ChartLeftPadding : CHART_LPADDING_NM );
			chart_bpadding 		= ( graphDef.MajorGridX ? CHART_BPADDING : CHART_BPADDING_NM );
		
			// Size of all lines below chart
			commentBlock		= 0;
			if ( graphDef.ShowLegend )
				commentBlock 	= graphDef.CommentLineCount * (nfont_height + LINE_PADDING) - LINE_PADDING;		

			// x_offset and y_offset define the starting corner of the actual graph 
			x_offset			= LBORDER_SPACE;
			if ( graphDef.VerticalLabel != null ) 
				x_offset 		+= nfont_height + LINE_PADDING;
			chartWidth			= imgWidth - x_offset - RBORDER_SPACE - chart_lpadding - CHART_RPADDING;
		
			y_offset			= UBORDER_SPACE;
			if ( graphDef.GetTitle() != null )			// Title *always* gets a extra LF automatically 
				y_offset		+= ((tfont_height + LINE_PADDING) * graphDef.GetTitle().LineCount + tfont_height) + LINE_PADDING;
			chartHeight 		= imgHeight - commentBlock - y_offset - BBORDER_SPACE - CHART_UPADDING - CHART_BPADDING;
		
			// Create graphics object
			Bitmap bImg 	= new Bitmap( imgWidth, imgHeight );
			Graphics graphics	= Graphics.FromImage(bImg);
		
			// Do the actual graphing
			CalculateSeries();							// calculate all datasources
						
			PlotImageBackground( graphics );			// draw the image background
			
			PlotChart( graphics );						// draw the actual chart
			
			PlotComments( graphics );					// draw all comment lines
			
			PlotOverlay( graphics );					// draw a possible image overlay
			
			PlotSignature( graphics );					// draw the JRobin signature

		
			// Dispose graphics context
			graphics.Dispose();
		
			return bImg;
		}
		
		
		private void CalculateSeries() 
		{
			ValueExtractor ve;
			RrdDb rrd;
			string[] varList;
			long finalEndTime		= 0;
			bool changingEndTime	= false;
			
			long startTime 			= graphDef.StartTime;
			long endTime			= graphDef.EndTime;
			changingEndTime			= (endTime == 0);
		
			int numDefs				= graphDef.NumDefs;
			
			Cdef[] cdefList			= graphDef.Cdefs;
			int numCdefs			= cdefList.Length;

			Pdef[] pdefList			= graphDef.Pdefs;
			int numPdefs			= pdefList.Length;
		
			// Set up the array with all datasources (both Def and Cdef)
			sources 				= new Source[ numDefs + numCdefs + numPdefs ];
			sourceIndex 			= new Hashtable( numDefs + numCdefs + numPdefs );
			int tblPos				= 0;
			int vePos				= 0;
		
			ValueExtractor[] veList		= new ValueExtractor[ graphDef.FetchSources.Count ];
			ICollection fetchSources	= graphDef.FetchSources.Values;
			
			foreach (FetchSource fs in fetchSources)
			{
				// Get the rrdDb
				rrd		= rrdGraph.GetRrd( fs.RrdFile ); 

				// If the endtime is 0, use the last time a database was updated
				if (  changingEndTime )
				{
					endTime = rrd.LastUpdateTime;
					endTime -= (endTime % rrd.Header.Step);

						if ( endTime > finalEndTime)
							finalEndTime = endTime;
				}
			
				// Fetch all required datasources
				ve 		= fs.Fetch( rrd, startTime,  endTime );
				varList = ve.Names;

				// BUGFIX: Release the rrdDb
				rrdGraph.ReleaseRrd(rrd);
			
				for (int i= 0; i < varList.Length; i++)
				{
					sources[tblPos]	= new Def(varList[i], numPoints);
					sourceIndex[varList[i]] = tblPos++ ;
				}
				
				veList[ vePos++ ] = ve;
			}

			// Add all Pdefs to the source table
			for ( int i = 0; i < pdefList.Length; i++ )
			{
				pdefList[i].Prepare( numPoints );
			
				sources[tblPos] = pdefList[i];
				sourceIndex[pdefList[i].Name] = tblPos++ ;
			}

		
			// Add all Cdefs to the source table		
			// Reparse all RPN datasources to use indices of the correct variables
			for ( int i = 0; i < cdefList.Length; i++ )
			{
				cdefList[i].Prepare( sourceIndex, numPoints );
			
				sources[tblPos]	= cdefList[i];
				sourceIndex[cdefList[i].Name] =  tblPos++ ;	
			}

			// Fill the array for all datasources
			timestamps 				= new long[numPoints];

			if ( changingEndTime )
			{
				endTime				= finalEndTime;
				calculatedEndTime	= endTime;
			}
			
			// RPN calculator for the Cdefs
			RpnCalculator rpnCalc 	= new RpnCalculator( sources, (endTime - startTime)/ (double) numPoints );
				
			for (int i = 0; i < numPoints; i++) 
			{
				long t 	= (long) (startTime + i * ((endTime - startTime) / (double)(numPoints - 1)));
				int pos = 0;
			
				// Get all fetched datasources
				for (int j = 0; j < veList.Length; j++)
					pos = veList[j].Extract( t, sources, i, pos );
			
				// Get all custom datasources
				for (int j = pos; j < pos + numPdefs; j++)
					((Pdef) sources[j]).Set(i,t);
				pos += numPdefs;
				
				// Get all combined datasources
				for (int j = pos; j < sources.Length; j++)
					sources[j].Set(i, t, rpnCalc.Evaluate( (Cdef) sources[j], i, t ) );

				timestamps[i] = t;
			}
		
			// Clean up the fetched datasources forcibly
			veList = null;
		}
		
		private void PlotImageBackground( Graphics g )
		{
			// Draw general background color
			defaultBrush.Color = graphDef.BackColor;
			g.FillRectangle(defaultBrush, 0, 0, imgWidth, imgHeight );
		
			// Draw a background image, if background image fails, just continue
			try 
			{
				FileStream bgImage = graphDef.GetBackground();
				if ( bgImage != null ) 
				{
					Bitmap img = (Bitmap)Bitmap.FromStream(bgImage);
					g.DrawImage( img, 0, 0 );
				}
			} 
			catch (IOException)
			{
			}
		
			// Set the image border
			Color bc 		= graphDef.BorderColor;
			Pen bp			= graphDef.BorderPen;

			if ( bp != null && !bc.IsEmpty )				// custom single line border
			{
				// Check for 'visible' line width
				int w = (int) bp.Width;
				if ( w > 0 ) 
					g.DrawRectangle( bp, w / 2, w / 2, imgWidth - w, imgHeight - w);
			}
			else										// default slightly beveled border
			{
				defaultBrush.Color = Color.FromArgb(220, 220, 220);
				
				g.FillRectangle(defaultBrush, 0, 0, 2, imgHeight - 1 );
				g.FillRectangle(defaultBrush, 0, 0, imgWidth - 1, 2 );
				defaultPen.Color = Color.Gray;
				g.DrawLine( defaultPen, 0, imgHeight - 1, imgWidth, imgHeight - 1 );
				g.DrawLine( defaultPen, imgWidth - 1, 0, imgWidth - 1, imgHeight );
				g.DrawLine( defaultPen, 1, imgHeight - 2, imgWidth, imgHeight - 2 );
				g.DrawLine( defaultPen, imgWidth - 2, 1, imgWidth - 2, imgHeight );
			}
		
			PlotImageTitle( g );
			
			PlotVerticalLabel( g );
		}
		
		private void PlotChart( Graphics graphics )
		{
			int lux		= x_offset + chart_lpadding;
			int luy		= y_offset + CHART_UPADDING;
			//defaultBrush.Color = Color.White;
			//defaultPen.Color = Color.Black;

			//if ( graphDef.Background == null )
			//{
				defaultBrush.Color = graphDef.CanvasColor;
				graphics.FillRectangle(defaultBrush, lux, luy, chartWidth, chartHeight );
			//}
		
			// Draw the chart area frame
			defaultPen.Color = graphDef.FrameColor;
			graphics.DrawRectangle(defaultPen, lux, luy, chartWidth, chartHeight );
				
			double val;
			double[] tmpSeries 	= new double[numPoints];
			
			GridRange range		= graphDef.GridRange;
			bool rigid			= ( range != null ? range.Rigid : false );
			double lowerValue	= ( range != null ? range.LowerValue : Double.MaxValue );
			double upperValue	= ( range != null ? range.UpperValue : Double.MinValue );
			
			// For autoscale, detect lower and upper limit of values
			PlotDef[] plotDefs 	= graphDef.PlotDefs;
			for ( int i = 0; i < plotDefs.Length; i++ )
			{
				plotDefs[i].SetSource( sources, sourceIndex );
				Source src = plotDefs[i].Source;
			
				// Only try autoscale when we do not have a rigid grid
				if ( !rigid && src != null )
				{
					double min = src.GetAggregate( Source.AGG_MINIMUM );
					double max = src.GetAggregate( Source.AGG_MAXIMUM );
				
					// If the plotdef is a stack, evaluate ALL previous values to find a possible max
					if ( plotDefs[i].plotType == PlotDef.PLOT_STACK && i >= 1 ) 
					{
						if ( plotDefs[i - 1].plotType == PlotDef.PLOT_STACK ) {		// Use this source plus stack of previous ones
						
							for (int j = 0; j < tmpSeries.Length; j++)
							{
								val = tmpSeries[j] + plotDefs[i].GetValue(j, timestamps);
		
								if ( val < lowerValue ) lowerValue = val;
								if ( val > upperValue ) upperValue = val;
		
								tmpSeries[j] = val;
							}
						}
						else {														// Use this source plus the previous one
						
							for (int j = 0; j < tmpSeries.Length; j++)
							{
								val = plotDefs[i - 1].GetValue(j, timestamps) + plotDefs[i].GetValue(j, timestamps);
							
								if ( val < lowerValue ) lowerValue = val;
								if ( val > upperValue ) upperValue = val;
							
								tmpSeries[j] = val;
							}
		
						}
					}
					else		// Only use min/max of a single datasource
					{
						if ( min < lowerValue ) lowerValue 	= min;
						if ( max > upperValue ) upperValue	= max;
					}
				}
			
			}
			
			vGrid 			= new ValueGrid( range, lowerValue, upperValue, graphDef.ValueAxis, graphDef.BaseValue );
			tGrid			= new TimeGrid( graphDef.StartTime, (graphDef.EndTime != 0? graphDef.EndTime : calculatedEndTime), graphDef.TimeAxis, graphDef.FirstDayOfWeek );
			
			lowerValue		= vGrid.LowerValue;
			upperValue		= vGrid.UpperValue;
							
			// Use a special graph 'object' that takes care of resizing and reversing y coordinates
			ChartGraphics g 	= new ChartGraphics( graphics );
			g.SetDimensions( chartWidth, chartHeight );
			g.SetXRange( tGrid.StartTime, tGrid.EndTime );
			g.SetYRange( lowerValue, upperValue );
			
			// Set the chart origin point
			double diff = 1.0d;
			if ( lowerValue < 0 )
				diff = 1.0d - ( lowerValue / ( -upperValue + lowerValue ));
			graphOriginX = lux;
			graphOriginY = (int) (luy + chartHeight *  diff);

			// If the grid is behind the plots, draw it first
			if ( !graphDef.FrontGrid ) PlotChartGrid( g );

			// Use AA if necessary
			if ( graphDef.AntiAliasing )
				graphics.SmoothingMode = SmoothingMode.AntiAlias;

			// Prepare clipping area and origin
			graphics.SetClip(new System.Drawing.Rectangle( lux, luy, chartWidth, chartHeight));
			graphics.TranslateTransform( graphOriginX, graphOriginY );
	 
			int lastPlotType 	= PlotDef.PLOT_LINE;
			int[] parentSeries 	= new int[numPoints];

			// Pre calculate x positions of the corresponding timestamps
			int[] xValues		= new int[timestamps.Length];
			for (int i = 0; i < timestamps.Length; i++)
				xValues[i]		= g.GetX(timestamps[i]);
		
			// Draw all graphed values
			for (int i = 0; i < plotDefs.Length; i++) 
			{
				plotDefs[i].Draw( g, xValues, parentSeries, lastPlotType );
				if(plotDefs[i].PlotType != PlotDef.PLOT_STACK) 
				{
					lastPlotType = plotDefs[i].PlotType;
				}
			}
			
			// Reset clipping area, origin and AA settings
			graphics.TranslateTransform( (float)-graphOriginX, (float)-graphOriginY );
			graphics.SetClip(new System.Drawing.Rectangle(0, 0, imgWidth, imgHeight));
			graphics.SmoothingMode = SmoothingMode.None;

			// If the grid is in front of the plots, draw it now
			if ( graphDef.FrontGrid ) PlotChartGrid( g );
		}
		
		private void PlotChartGrid( ChartGraphics chartGraph )
		{
			Graphics g = chartGraph.Graphics;
			Font ft = normal_font;
			//defaultPen.Color = Color.Black;

			int lux = x_offset + chart_lpadding;
			int luy = y_offset + CHART_UPADDING;

			bool minorX	= graphDef.MinorGridX;
			bool minorY	= graphDef.MinorGridY;
			bool majorX	= graphDef.MajorGridX;
			bool majorY	= graphDef.MajorGridY;
			
			Color minColor	= graphDef.MinorGridColor;
			Color majColor	= graphDef.MajorGridColor;
			
			// Dashed line
			dashPen.DashStyle = DashStyle.Dot;
			
			// Draw basic axis
			int tmpx = lux + chartWidth;
			int tmpy = luy + chartHeight;

			// Draw X axis with arrow
			defaultPen.Color = graphDef.AxisColor;
			g.DrawLine( defaultPen, lux - 4, tmpy, tmpx + 4, tmpy );
			defaultPen.Color = graphDef.ArrowColor;
			g.DrawLine( defaultPen, tmpx + 4, tmpy - 3, tmpx + 4, tmpy + 3 );
			g.DrawLine( defaultPen, tmpx + 4, tmpy - 3, tmpx + 9, tmpy );
			g.DrawLine( defaultPen, tmpx + 4, tmpy + 3, tmpx + 9, tmpy );

			// Draw X axis time grid and labels
			if ( graphDef.GridX )
			{
				TimeMarker[] timeList	= tGrid.TimeMarkers;
				bool labelCentered		= tGrid.CenterLabels;
				long labelGridWidth		= tGrid.MajorGridWidth;
				
				int pixWidth 			= 0;
				if ( labelCentered )
					pixWidth = ( chartGraph.GetX( labelGridWidth ) - chartGraph.GetX( 0 ) );
				
				for (int i = 0; i < timeList.Length; i++)
				{
					long secTime 	= timeList[i].Timestamp;
					int posRel 		= chartGraph.GetX(secTime);
					int pos 		= lux + posRel;
					string label	= timeList[i].Label;
					stringLength	= g.MeasureString(label,ft,1000,sf);
					
					if ( posRel >= 0 ) {
						if ( majorX && timeList[i].IsLabel )
						{
							dashPen.Color =  majColor ;
							g.DrawLine( dashPen, pos, luy, pos, luy + chartHeight );
							defaultPen.Color = majColor;
							g.DrawLine( defaultPen, pos, luy - 2, pos, luy + 2 );
							g.DrawLine( defaultPen, pos, luy + chartHeight - 2, pos, luy + chartHeight + 2 );
							// Only draw label itself if we are far enough from the side axis
							// Use extra 2 pixel padding (3 pixels from border total at least)
							int txtDistance = (int) (stringLength.Width) / 2;
					
							if ( labelCentered )
							{
								if ( pos + pixWidth <= lux + chartWidth )
									GraphString( g, ft, label, pos + 2 + pixWidth/2 - txtDistance, luy + chartHeight + LINE_PADDING );
							}
							else if ( (pos - lux > txtDistance + 2) && (pos + txtDistance + 2 < lux + chartWidth) )	
								GraphString( g, ft, label, pos - txtDistance, luy + chartHeight + LINE_PADDING );
						}
						else if ( minorX )
						{	
							dashPen.Color = minColor;
							g.DrawLine( dashPen, pos, luy, pos, luy + chartHeight );
							defaultPen.Color = minColor;
							g.DrawLine( defaultPen, pos, luy - 1, pos, luy + 1 );
							g.DrawLine( defaultPen, pos, luy + chartHeight - 1, pos, luy + chartHeight + 1 );
				
						}
					}
				}
			}
			
			// Draw Y axis value grid and labels
			valueFormat.SetScaling( true, false );			// always scale the label values
			if ( graphDef.GridY )
			{
				ValueMarker[] valueList = vGrid.ValueMarkers;
				
				for (int i = 0; i < valueList.Length; i++)
				{
					int valRel 		= chartGraph.GetY( valueList[i].Value );
					
					valueFormat.SetFormat( valueList[i].Value, 2, 0 );
					string label	= (valueFormat.ScaledValue + " " + valueFormat.Prefix).Trim();
					stringLength	= g.MeasureString(label,ft,1000,sf);
		
					if ( majorY && valueList[i].IsMajor )
					{
						dashPen.Color = majColor;
						g.DrawLine( dashPen, graphOriginX, graphOriginY - valRel, graphOriginX + chartWidth, graphOriginY - valRel );
						defaultPen.Color = majColor;
						// solid dashes at ends of lines
						g.DrawLine( defaultPen, graphOriginX - 2, graphOriginY - valRel, graphOriginX + 2, graphOriginY - valRel );
						g.DrawLine( defaultPen, graphOriginX + chartWidth - 2, graphOriginY - valRel, graphOriginX + chartWidth + 2, graphOriginY - valRel );
						GraphString( g, ft, label, graphOriginX - (int)(stringLength.Width) - 7, graphOriginY - valRel - (int)(nfont_height/2)  );
					}
					else if ( minorY )
					{
						dashPen.Color = minColor;
						g.DrawLine( dashPen, graphOriginX, graphOriginY - valRel, graphOriginX + chartWidth, graphOriginY - valRel );
						defaultPen.Color = minColor;
						g.DrawLine( defaultPen, graphOriginX - 1, graphOriginY - valRel, graphOriginX + 1, graphOriginY - valRel );
						g.DrawLine( defaultPen, graphOriginX + chartWidth - 1, graphOriginY - valRel, graphOriginX + chartWidth + 1, graphOriginY - valRel );
					}

				}
			}
			
		}
		
		private void PlotComments( Graphics g ) 
		{
			if ( !graphDef.ShowLegend ) return;
			
			ArrayList markerList = new ArrayList();
			
			// Position the cursor just below the chart area
			int posy			= y_offset + chartHeight + CHART_UPADDING + CHART_BPADDING + ( graphDef.MajorGridX ? nfont_height : 0 );
			int posx			= LBORDER_SPACE;

			defaultPen.Color = normalFontColor;
						
			Comment[] clist		= graphDef.Comments;
			StringBuilder tmpStr	= new StringBuilder("");

			bool newLine	= false;
			bool drawText	= false;
			
			for (int i = 0; i < clist.Length; i++)
			{
				stringLength	= g.MeasureString(tmpStr.ToString(),normal_font, 1000, sf);

				if ( clist[i].commentType == Comment.CMT_LEGEND ) 
				{
					markerList.Add( new LegendMarker( (int)stringLength.Width, ((Legend) clist[i]).Color ) );
					// Determine length of a space in particular font
					stringLength = g.MeasureString(" ",normal_font, 1000, sf);
					// Now multiply that length as many times as needed to get past
					// the 10 pixel width of the legend marker plus 15 pixels for padding
					for (int j = 0; j < (int)Math.Ceiling(25/stringLength.Width); j++)
					{
						tmpStr.Append( " " );
					}
				} 
				else if ( clist[i].commentType == Comment.CMT_GPRINT )
					((Gprint) clist[i]).SetValue( sources, sourceIndex, valueFormat );
				
				ArrayList tknpairs = clist[i].Tokens;
				
				for (int j = 0; j < tknpairs.Count; j++)
				{
					string str 	= (string) tknpairs[j++];
					byte tkn	= (byte) tknpairs[j];
					
					if ( clist[i].TrimString() )
						tmpStr.Append( str.Trim() );
					else
						tmpStr.Append( str );

					stringLength	= g.MeasureString(tmpStr.ToString(),normal_font, 1000, sf);
						
					if ( tkn != Comment.TKN_NULL )
					{
						drawText = true;
						if ( tkn == Comment.TKN_ALF ) {
							newLine	= true;
							posx	= LBORDER_SPACE;					
						} 
						else if ( tkn == Comment.TKN_ARF ) {
							newLine	= true;
							posx 	= imgWidth - RBORDER_SPACE - (int)(stringLength.Width);
						}
						else if ( tkn == Comment.TKN_ACF ) {
							newLine	= true;
							posx 	= imgWidth / 2 - (int)(stringLength.Width) / 2;
						}
						else if ( tkn == Comment.TKN_AL )
							posx	= LBORDER_SPACE;
						else if ( tkn == Comment.TKN_AR )
							posx 	= imgWidth - RBORDER_SPACE - (int)(stringLength.Width);
						else if ( tkn == Comment.TKN_AC )
							posx 	= imgWidth / 2 - (int)(stringLength.Width) / 2;
					}
					
					if ( !newLine && clist[i].AddSpacer() )
						tmpStr.Append( SPACER );
									
					// Plot the string
					if ( drawText ) {
						GraphString( g, normal_font, tmpStr.ToString(), posx, posy-nfont_height );
						tmpStr		= new StringBuilder(""); 
						drawText	= false;

						// Plot the markers	
						while ( !(markerList.Count == 0) ) 
						{
							LegendMarker lm = (LegendMarker) markerList[0];
							markerList.RemoveAt(0);
							defaultBrush.Color =  lm.Color;
							g.FillRectangle(defaultBrush, posx + lm.XPosition, posy - 9, 10, 10 );
							defaultPen.Color = normalFontColor;
							g.DrawRectangle( defaultPen, posx + lm.XPosition, posy - 9, 10, 10 );
						}
					}
					
					if ( newLine ) {
						posy 	+= nfont_height + LINE_PADDING;
						newLine	= false;
					}
					
				}
			}
			
			if ( tmpStr.Length > 0)
			{
				posx		= LBORDER_SPACE;
				GraphString( g, normal_font, tmpStr.ToString(), posx, posy - BBORDER_SPACE );
				tmpStr		= new StringBuilder(""); 
				drawText	= false;

				// Plot the markers	
				while ( !(markerList.Count == 0) )
				{
					LegendMarker lm = (LegendMarker) markerList[0];
					markerList.RemoveAt(0);
					defaultBrush.Color = lm.Color;
					g.FillRectangle(defaultBrush, posx + lm.XPosition, posy - BBORDER_SPACE, 10, 10 );
					defaultPen.Color = normalFontColor;
					g.DrawRectangle(defaultPen, posx + lm.XPosition, posy - BBORDER_SPACE, 10, 10 );
				}
			}
		}
		
		private void PlotOverlay( Graphics g )
		{
			// If overlay drawing fails, just ignore it
			try 
			{
				FileStream overlayImg = graphDef.GetOverlay();
				if ( overlayImg != null )
				{
					Bitmap img 	= (Bitmap)Bitmap.FromStream(overlayImg);
				
					int w 				= img.Width;
					int h 				= img.Height;
					int rgbWhite 		= Color.White.ToArgb(); 
					int pcolor, red, green, blue;

					// For better performance we might want to load all color
					// ints of the overlay in one go
					for (int i = 0; i < w; i++) {
						for (int j = 0; j < h; j++) {
							pcolor = img.GetPixel(i, j).ToArgb();
							if ( pcolor != rgbWhite ) 
							{
								red 	= (pcolor >> 16) & 0xff;
								green 	= (pcolor >> 8) & 0xff;
								blue 	= pcolor & 0xff;

								defaultPen.Color = Color.FromArgb(red, green, blue);
								g.DrawLine( defaultPen, i, j, i, j );
							}
						}
					}
				}
			}
			catch (IOException) 
			{
			}	
		} 
		
		private void PlotImageTitle( Graphics g )
		{
			Title graphTitle	= graphDef.GetTitle();
			
			// No title to draw
			if ( graphTitle == null )
				return;
			
			// Position the cursor just above the chart area
			int posy			= UBORDER_SPACE;
			int posx			= LBORDER_SPACE;

			// Set drawing specifics
			defaultBrush.Color = graphDef.TitleFontColor;
			

			// Parse and align the title text
			StringBuilder tmpStr	= new StringBuilder("");
			bool newLine		= false;

			ArrayList tknpairs = graphTitle.Tokens;
			for (int j = 0; j < tknpairs.Count; j++)
			{
				string str 	= (string) tknpairs[j++];
				byte tkn	= (byte) tknpairs[j];

				tmpStr.Append( str );
				stringLength	= g.MeasureString(tmpStr.ToString(),title_font, 1000, sf);
				if ( tkn != Comment.TKN_NULL )
				{
					if ( tkn == Comment.TKN_ALF )
					{
						newLine	= true;
						posx	= LBORDER_SPACE;					
					} 
					else if ( tkn == Comment.TKN_ARF ) 
					{
						newLine	= true;
						posx 	= imgWidth - RBORDER_SPACE - (int)(stringLength.Width);
					}
					else if ( tkn == Comment.TKN_ACF ) 
					{
						newLine	= true;
						posx 	= imgWidth / 2 - (int)(stringLength.Width) / 2;
					}
					else if ( tkn == Comment.TKN_AL )
						posx	= LBORDER_SPACE;
					else if ( tkn == Comment.TKN_AR )
						posx 	= imgWidth - RBORDER_SPACE - (int)(stringLength.Width);
					else if ( tkn == Comment.TKN_AC )
						posx 	= imgWidth / 2 - (int)(stringLength.Width) / 2;
				}
				else {		// default is a center alignment for title
					posx 	= imgWidth / 2 - (int)(stringLength.Width) / 2;
				}

				// Plot the string
				g.DrawString( tmpStr.ToString(), title_font, defaultBrush, posx, posy );
				tmpStr		= new StringBuilder(""); 

				// Go to next line
				if ( newLine )
				{
					posy += tfont_height + LINE_PADDING;
					newLine	= false;
				}
			}
			
		}
		
		private void PlotVerticalLabel( Graphics g )
		{
			string valueAxisLabel 	= graphDef.VerticalLabel;
			
			if ( valueAxisLabel == null )
				return;

			stringLength	= g.MeasureString(valueAxisLabel,normal_font, 1000, sf);
			
			defaultBrush.Color = normalFontColor;
			int labelWidth			= (int)stringLength.Width;

			// draw a rotated label text as vertical label
			g.RotateTransform( (float) -90.0 );
			GraphString( g, normal_font, valueAxisLabel, - y_offset - CHART_UPADDING
											- chartHeight / 2 
											- labelWidth / 2,
											LBORDER_SPACE 
											);
			g.RotateTransform( (float) 90.0 );
		}

		private void PlotSignature( Graphics g )
		{
			float angle;
			if ( !graphDef.ShowSignature )
				return;
			
			string sig = "RrdSharp v. 0.1"; 
			defaultBrush.Color = Color.Gray;
			Font sigfnt =  new Font("Courier New", 9, FontStyle.Regular,GraphicsUnit.Pixel);
		
			angle = (float) 90.0;
			//g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			g.RotateTransform( angle );
			g.DrawString( sig, sigfnt, defaultBrush, 5,  -imgWidth + 2 );	
			//g.TextRenderingHint = TextRenderingHint.AntiAlias;
			g.RotateTransform((float) -90.0 );
		}	

		private void GraphString( Graphics g, Font fnt, string str, int x, int y )
		{
			Color co = defaultBrush.Color;		
			defaultBrush.Color =  normalFontColor;
			g.DrawString( str,fnt,defaultBrush, x, y );
			defaultBrush.Color = co;
		}	
	}
}