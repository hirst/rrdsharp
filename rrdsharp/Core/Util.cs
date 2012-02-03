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

namespace RrdSharp.Core
{
	/// <summary>
	/// Class defines various utility functions used in RRDSharp.
	/// </summary>
	public class Util 
	{
		private static string DOUBLE_FORMAT = "0.0000000000E00";
				
		/// <summary>
		/// Returns Unix time minus milliseconds (why without milliseconds?  who knows.)
		/// </summary>
		public static long Time 
		{
			get
			{
				return ((TicksToMillis(System.DateTime.UtcNow.Ticks)/ 1000L));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamp"></param>
		/// <param name="step"></param>
		/// <returns></returns>
		public static long Normalize(long timestamp, long step) 
		{
			return timestamp - timestamp % step;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static double Max(double x, double y) 
		{
			return Double.IsNaN(x)? y: Double.IsNaN(y)? x: Math.Max(x, y);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static double Min(double x, double y) 
		{
			return Double.IsNaN(x)? y: Double.IsNaN(y)? x: Math.Min(x, y);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static double Sum(double x, double y) 
		{
			return Double.IsNaN(x)? y: Double.IsNaN(y)? x: x + y;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="nanString"></param>
		/// <param name="forceExponents"></param>
		/// <returns></returns>
		public static string FormatDouble(double x, string nanString, bool forceExponents) 
		{
			// MONO FIX: 
			if (x==0) return DOUBLE_FORMAT;

			if(Double.IsNaN(x)) 
			{
				return nanString;
			}
			if(forceExponents) 
			{
				return x.ToString(DOUBLE_FORMAT);
			}
			return x.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="forceExponents"></param>
		/// <returns></returns>
		public static string FormatDouble(double x, bool forceExponents) 
		{
			return FormatDouble(x, Double.NaN.ToString(), forceExponents);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public static void Debug(string message) 
		{
			if(RrdDb.DEBUG) 
			{
				Console.WriteLine(message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamp"></param>
		/// <returns></returns>
		public static DateTime GetDate(long timestamp) 
		{
			long ticks = (MillisToTicks(timestamp*1000L));
			return new DateTime(ticks);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static long GetTimestamp(DateTime date) 
		{
			return (TicksToMillis(date.Ticks)/ 1000L);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <param name="day"></param>
		/// <param name="hour"></param>
		/// <param name="min"></param>
		/// <returns></returns>
		public static long GetTimestamp(int year, int month, int day, int hour, int min) 
		{
			DateTime dt = new DateTime(year, month, day, hour, min,0);
			return Util.GetTimestamp(dt);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <param name="day"></param>
		/// <returns></returns>
		public static long GetTimestamp(int year, int month, int day) 
		{
			return Util.GetTimestamp(year, month, day, 0, 0);
		}

		internal static double ParseDouble(string valueStr) 
		{
			double val;
			try 
			{
				val = Double.Parse(valueStr);
			}
			catch(FormatException) 
			{
				val = Double.NaN;
			}
			return val;
		}

		internal bool ParseBoolean(string valueStr)
		{
			return valueStr.ToUpper().Equals("TRUE") ||
				valueStr.ToUpper().Equals("ON") ||
				valueStr.ToUpper().Equals("YES") ||
				valueStr.ToUpper().Equals("Y") ||
				valueStr.ToUpper().Equals("1");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ticks"></param>
		/// <returns></returns>
		public static long TicksToMillis(long ticks)
		{
			return ((ticks - 621355968000000000) / 10000);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="millis"></param>
		/// <returns></returns>
		public static long MillisToTicks(long millis)
		{
			return ((millis*10000L)+621355968000000000);
		}

		
		private static DateTime lastLap = DateTime.UtcNow;

		/// <summary>
		/// 
		/// </summary>
		public static string LapTime 
		{
			get
			{
				DateTime newLap = DateTime.UtcNow;
				double seconds = (TicksToMillis(newLap.Ticks) - TicksToMillis(lastLap.Ticks)) / 1000.0;
				lastLap = newLap;
				return "[" + seconds + " sec]";
			}
		}

		internal static int GetMatchingDatasourceIndex(RrdDb rrd1, int dsIndex, RrdDb rrd2) 
		{
			string dsName = rrd1.GetDatasource(dsIndex).DsName;
			try 
			{
				return rrd2.GetDsIndex(dsName);
			} 
			catch (RrdException) 
			{
				return -1;
			}
		}

		internal static int GetMatchingArchiveIndex(RrdDb rrd1, int arcIndex, RrdDb rrd2)
		{
			Archive archive = rrd1.GetArchive(arcIndex);
			string consolFun = archive.ConsolFun;
			int steps = archive.Steps;
			try 
			{
				return rrd2.GetArcIndex(consolFun, steps);
			} 
			catch (RrdException) 
			{
				return -1;
			}
		}

	}
}