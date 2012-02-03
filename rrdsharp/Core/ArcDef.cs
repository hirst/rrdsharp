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

namespace RrdSharp.Core
{
	/// <summary>
	/// Class to represent single archive definition within the RRD.
	/// </summary>
	public class ArcDef
	{
		/// <summary>
		/// Valid consolidation function names.
		/// </summary>
		public static readonly string [] CONSOL_FUNS = { "AVERAGE", "MAX", "MIN", "LAST" };

		private string consolFun;
		private double xff;
		private int steps, rows;

		/// <summary>
		/// Creates a new Archive Definition object.
		/// </summary>
		/// <param name="consolFun">Consolidation Function</param>
		/// <param name="xff">X-files factor (between 0 and 1)</param>
		/// <param name="steps">Number of Archive steps</param>
		/// <param name="rows">Number of Archive rows</param>
		public ArcDef(string consolFun, double xff, int steps, int rows) 
		{
			this.consolFun = consolFun;
			this.xff = xff;
			this.steps = steps;
			this.rows = rows;
			Validate();
		}

		/// <summary>
		/// Returns consolidation function.
		/// </summary>
		public string ConsolFun
		{
			get
			{
				return consolFun;
			}
		}

		/// <summary>
		/// Returns X-files factor.
		/// </summary>
		public double Xff
		{
			get
			{
				return xff;
			}
		}

		/// <summary>
		/// Returns number of Archive steps.
		/// </summary>
		public int Steps
		{
			get
			{
				return steps;
			}
		}

		/// <summary>
		/// Returns number of Archive rows.
		/// </summary>
		public int Rows  
		{
			set
			{
				this.Rows = value;
			}
			get
			{
				return rows;
			}
		}

		
		private void Validate() 
		{
			if (!IsValidConsolFun(consolFun))
			{
				throw new RrdException("Invalid consolidation function specified: " + consolFun);
			}
			if (Double.IsNaN(xff) || xff < 0.0 || xff >= 1.0) 
			{
				throw new RrdException("Invalid xff, must be >= 0 and < 1: " + xff);
			}
			if (steps <= 0 || rows <= 0)
			{
				throw new RrdException("Invalid steps/rows number: " + steps + "/" + rows);
			}
		}

		/// <summary>
		/// Returns a string representing the Archive Definition (RRDTool format).
		/// </summary>
		/// <returns>string representation of Archive Definition.</returns>
		public string Dump() 
		{
			return "RRA:" + consolFun + ":" + xff + ":" +	steps + ":" + rows;
		}

		/// <summary>
		/// Checks of two Archive Definitions are equal.Archive definitions are considered equal 
		/// if they have the same number of steps and the same consolidation function. It is not 
		/// possible to create RRD file with two equal archive definitions.
		/// </summary>
		/// <param name="obj">Archive Definition to compare to.</param>
		/// <returns>True if equal, false otherwise.</returns>
		public new bool Equals(object obj)
		{
			if(obj is ArcDef) 
			{
				ArcDef arcObj = (ArcDef) obj;
				return consolFun.Equals(arcObj.consolFun) && steps == arcObj.steps;
			}
			return false;
		}

		/// <summary>
		/// Checks if consolFun represents a valid consolidation function name.
		/// </summary>
		/// <param name="consolFun">Name of a consolidation function.</param>
		/// <returns>True if consolFun is a valid consolidation function name, false otherwise.</returns>
		public static bool IsValidConsolFun(string consolFun)
		{
			for(int i = 0; i < CONSOL_FUNS.Length; i++) 
			{
				if(CONSOL_FUNS[i].Equals(consolFun))
				{
					return true;
				}
			}
			return false;
		}

	}
}