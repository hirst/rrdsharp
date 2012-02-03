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
using System.Globalization;
using System.Collections;
using RrdSharp.Core;


namespace RrdSharp.Graph
{
	/// <summary>
	/// 
	/// </summary>
	public class TimeAxisUnit
	{
		private static readonly int[] calendarUnit =
			{
				13,
				12,
				11,
				5,
				3,
				2,
				1	
			};
		
		// Indices in the calendarUnit table
		/// <summary>
		/// Constant for seconds
		/// </summary>
		public const int SECOND	= 0;			
		/// <summary>
		/// 
		/// </summary>
		public const int MINUTE	= 1;		
		/// <summary>
		/// 
		/// </summary>
		public const int HOUR 	= 2;		
		/// <summary>
		/// 
		/// </summary>
		public const int DAY	= 3;		
		/// <summary>
		/// 
		/// </summary>
		public const int WEEK 	= 4;		
		/// <summary>
		/// 
		/// </summary>
		public const int MONTH 	= 5;		
		/// <summary>
		/// 
		/// </summary>
		public const int YEAR 	= 6;		

		// Days of the week - replace with enum
		/// <summary>
		/// 
		/// </summary>
		public const int MONDAY			= 2;
		/// <summary>
		/// 
		/// </summary>
		public const int TUESDAY		= 3;
		/// <summary>
		/// 
		/// </summary>
		public const int WEDNESDAY		= 4;
		/// <summary>
		/// 
		/// </summary>
		public const int THURSDAY		= 5;
		/// <summary>
		/// 
		/// </summary>
		public const int FRIDAY			= 6;
		/// <summary>
		/// 
		/// </summary>
		public const int SATURDAY		= 7;
		/// <summary>
		/// 
		/// </summary>
		public const int SUNDAY			= 1;

		private static readonly string[] UNIT_NAMES = {
		"SECOND", "MINUTE", "HOUR", "DAY", "WEEK", "MONTH", "YEAR" };
	
		private static readonly string[] DAY_NAMES	= {
		"SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY" };
	
		private int minGridTimeUnit			= HOUR;			// minor grid
		private int minGridUnitSteps		= 1;
		private int majGridTimeUnit			= HOUR;			// major grid
		private int majGridUnitSteps		= 6;
	
		private bool centerLabels			= false; 
		private string df 					= "HH:mm";
		
		private int firstDayOfWeek			= MONDAY;

		private DateTime dtNow				= DateTime.UtcNow;
 		private GregorianCalendar gc		= new GregorianCalendar();
	
		internal TimeAxisUnit( int minGridTimeUnit, int minGridUnitSteps, int majGridTimeUnit, int majGridUnitSteps, string df, bool centerLabels, int firstDayOfWeek )
		{
			this.minGridTimeUnit	= minGridTimeUnit;
			this.minGridUnitSteps	= minGridUnitSteps;
			this.majGridTimeUnit	= majGridTimeUnit;
			this.majGridUnitSteps	= majGridUnitSteps;
			this.df					= df;
			this.centerLabels		= centerLabels;
			this.firstDayOfWeek		= firstDayOfWeek;
		}
	
	
		internal TimeMarker[] GetTimeMarkers( long start, long stop )
		{
			start 	*= 1000;								// Discard milliseconds
			stop	*= 1000;
	
			DateTime cMaj	= DateTime.UtcNow;
			DateTime cMin	= DateTime.UtcNow;
	
			// Set the start calculation point for the grids
			SetStartPoint(ref cMaj, majGridTimeUnit, start);
			SetStartPoint(ref cMin, minGridTimeUnit, start);
	
			// Find first visible grid point
			long minPoint = Util.TicksToMillis(cMin.Ticks);
			long majPoint = Util.TicksToMillis(cMaj.Ticks);
	
			while ( majPoint < start )
				majPoint = GetNextPoint(ref cMaj, majGridTimeUnit, majGridUnitSteps);
			while ( minPoint < start )
				minPoint = GetNextPoint(ref cMin, minGridTimeUnit, minGridUnitSteps);
	
			ArrayList markerList = new ArrayList();
			
			while ( minPoint <= stop && majPoint <= stop )
			{
				if ( minPoint < majPoint )
				{
					markerList.Add( new TimeMarker( minPoint, "", false ) );
					minPoint = GetNextPoint( ref cMin, minGridTimeUnit, minGridUnitSteps );	
				}
				else if ( minPoint == majPoint )	// Special case, but will happen most of the time
				{
					markerList.Add( new TimeMarker( majPoint, cMaj.ToString(df), true ) );
					majPoint = GetNextPoint( ref cMaj, majGridTimeUnit, majGridUnitSteps );
					minPoint = GetNextPoint( ref cMin, minGridTimeUnit, minGridUnitSteps );
				}
				else
				{
					markerList.Add( new TimeMarker( majPoint, cMaj.ToString(df), true ) );
					majPoint = GetNextPoint( ref cMaj, majGridTimeUnit, majGridUnitSteps );
				}
			}

			while ( minPoint <= stop )
			{
				markerList.Add( new TimeMarker( minPoint, "", false ) );
				minPoint = GetNextPoint( ref cMin, minGridTimeUnit, minGridUnitSteps );
			}
	
			while ( majPoint <= stop )
			{
				markerList.Add( new TimeMarker( majPoint, cMaj.ToString(df), true ) );
				majPoint = GetNextPoint( ref cMaj, majGridTimeUnit, majGridUnitSteps );
			}
	
			return (TimeMarker[]) markerList.ToArray(typeof(TimeMarker));
		}


		internal long MajorGridWidth
		{
			get
			{
				DateTime d = DateTime.UtcNow;
				long now = Util.TicksToMillis(d.Ticks)/1000;
				
				switch (majGridTimeUnit)
				{
					case 0:
						d = gc.AddSeconds(d,majGridUnitSteps);
						break;
					case 1:
						d = gc.AddMinutes(d,majGridUnitSteps);
						break;
					case 2:
						d = gc.AddHours(d,majGridUnitSteps);
						break;
					case 3:
						d = gc.AddDays(d,majGridUnitSteps);
						break;
					case 4:
						d = gc.AddWeeks(d,majGridUnitSteps);
						break;
					case 5:
						d = gc.AddMonths(d,majGridUnitSteps);
						break;
					case 6:
						d = gc.AddYears(d,majGridUnitSteps);
						break;
					default:
						break;
				}

				return ((Util.TicksToMillis(d.Ticks)/1000) - now);
			}
		}

		internal bool CenterLabels 
		{
			get
			{
				return centerLabels;
			}
		}

		internal int MinGridTimeUnit
		{
			get
			{
				return minGridTimeUnit;
			}
		}

		internal int MinGridTimeSteps
		{
			get
			{
				return minGridUnitSteps;
			}
		}

		internal int MajGridTimeUnit
		{
			get
			{
				return majGridTimeUnit;
			}
		}

		internal int MajGridTimeSteps
		{
			get
			{
				return majGridUnitSteps;
			}
		}

		internal bool IsCenterLabels
		{
			get
			{
				return centerLabels;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string DateFormat
		{
			get
			{
				return df;
			}
		}
	
		
		// This so totally needs to be redone the correct way.  
		private void SetStartPoint( ref DateTime t, int unit, long exactStart )
		{
			DateTime startDT = new DateTime(Util.MillisToTicks( exactStart ));
			DateTime roundedDT = startDT;

			switch (unit)
			{
				case 0:
					roundedDT = new DateTime(startDT.Year, startDT.Month, startDT.Day, startDT.Hour, startDT.Minute, startDT.Second);
					break;
				case 1:
					roundedDT = new DateTime(startDT.Year, startDT.Month, startDT.Day, startDT.Hour, startDT.Minute, 0);
					break;
				case 2:
					roundedDT = new DateTime(startDT.Year, startDT.Month, startDT.Day, startDT.Hour, 0, 0);
					break;
				case 3:
					roundedDT = new DateTime(startDT.Year, startDT.Month, startDT.Day, 0, 0, 0);
					break;
				case 4: // need to fix this
					roundedDT = new DateTime(startDT.Year, startDT.Month, 1, 0, 0, 0);
					break;
				case 5:
					roundedDT = new DateTime(startDT.Year, startDT.Month, 1, 0, 0, 0);
					break;
				case 6:
					roundedDT = new DateTime(startDT.Year, 1, 1, 0, 0, 0);
					break;
				default:
					break;
			}
			// Gotta fix the day of week thing
			t = roundedDT;		
		}
	
		private long GetNextPoint( ref DateTime t, int unit, int unitSteps )
		{
			switch (unit)
			{
				case 0:
					t = gc.AddSeconds(t,unitSteps);
					break;
				case 1:
					t = gc.AddMinutes(t,unitSteps);
					break;
				case 2:
					t = gc.AddHours(t,unitSteps);
					break;
				case 3:
					t = gc.AddDays(t,unitSteps);
					break;
				case 4:
					t = gc.AddWeeks(t,unitSteps);
					break;
				case 5:
					t = gc.AddMonths(t,unitSteps);
					break;
				case 6:
					t = gc.AddYears(t,unitSteps);
					break;
				default:
					break;
			}
	
			return (Util.TicksToMillis(t.Ticks));
		}

		internal static string GetUnitName(int unit)
		{
			return UNIT_NAMES[unit];
		}

		internal static string GetDayName(int dayIndex)
		{
			return DAY_NAMES[dayIndex];
		}
	
	
	}
}