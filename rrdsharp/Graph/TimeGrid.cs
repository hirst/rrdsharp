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
	internal class TimeGrid 
	{
		private long startTime;
		private long endTime;
	
		private TimeAxisUnit tAxis;
	
	
		internal TimeGrid( long startTime, long endTime, TimeAxisUnit tAxis, int firstDayOfWeek )
		{
			this.startTime 	= startTime;
			this.endTime	= endTime;
			this.tAxis		= tAxis;
		
			// Set an appropriate time axis if not given yet	
			SetTimeAxis( firstDayOfWeek );
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

		internal TimeMarker[] TimeMarkers 
		{
			get
			{
				return tAxis.GetTimeMarkers( startTime, endTime );
			}
		}

		internal long MajorGridWidth 
		{
			get
			{
				return tAxis.MajorGridWidth;
			}
		}

		internal bool CenterLabels 
		{
			get
			{
				return tAxis.CenterLabels;
			}
		}	
	
	
		private void SetTimeAxis( int firstDayOfWeek)
		{
			if ( tAxis != null )
				return;
		
			double days = (endTime - startTime) / 86400.0;

			if ( days <= 0.75 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.MINUTE, 1, TimeAxisUnit.MINUTE, 5, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 2.0 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.MINUTE, 5, TimeAxisUnit.MINUTE, 10, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 3.0 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.MINUTE, 5, TimeAxisUnit.MINUTE, 20, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 5.0 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.MINUTE, 10, TimeAxisUnit.MINUTE, 30, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 10.0 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.MINUTE, 15, TimeAxisUnit.HOUR, 1, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 15.0 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.MINUTE, 30, TimeAxisUnit.HOUR, 2, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 20.0 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.HOUR, 1, TimeAxisUnit.HOUR, 1, "HH", true, firstDayOfWeek );
			}
			else if ( days <= 36.0 / 24.0 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.HOUR, 1, TimeAxisUnit.HOUR, 4, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 2 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.HOUR, 2, TimeAxisUnit.HOUR, 6, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days <= 3 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.HOUR, 3, TimeAxisUnit.HOUR, 12, "HH:mm", false, firstDayOfWeek );
			}
			else if ( days < 8 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.HOUR, 6, TimeAxisUnit.DAY, 1, "ddd dd", true, firstDayOfWeek);
			}
			else if ( days <= 14 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.HOUR, 12, TimeAxisUnit.DAY, 1, "dd", true, firstDayOfWeek );
			}
			else if ( days <= 43 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.DAY, 1, TimeAxisUnit.WEEK, 1, "'week' ww", true, firstDayOfWeek );
			}
			else if ( days <= 157 ) 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.WEEK, 1, TimeAxisUnit.WEEK, 1, "ww", true, firstDayOfWeek );
			}
			else 
			{
				tAxis = new TimeAxisUnit( TimeAxisUnit.MONTH, 1, TimeAxisUnit.MONTH, 1, "MMM", true, firstDayOfWeek );
			}
		}
	}
}