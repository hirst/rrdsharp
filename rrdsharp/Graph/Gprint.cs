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
using System.Text.RegularExpressions;
using System.Collections;
using RrdSharp.Core;



namespace RrdSharp.Graph
{
	internal class Gprint : Comment 
	{
		private static readonly string SCALE_MARKER 			= "@s";
		private static readonly string UNIFORM_SCALE_MARKER 	= "@S";
		private static readonly string VALUE_MARKER 			= "@([0-9]*\\.[0-9]{1}|[0-9]{1}|\\.[0-9]{1})";
		private static readonly Regex VALUE_PATTERN 			= new Regex(VALUE_MARKER);
		
		private string sourceName;
		private int aggregate; 
		private int numDec									= 3;		// Show 3 decimal values by default
		private int strLen									= -1;
		private double baseValue							= -1;
		private bool normalScale							= false;
		private bool uniformScale							= false;
		
		
		internal Gprint( string sourceName, string consolFunc, string text )
		{
			this.text = text;
			CheckValuePlacement();		// First see if this GPRINT is valid
			base.ParseComment();
			
			this.commentType = Comment.CMT_GPRINT;
			this.sourceName = sourceName;
			
			if ( consolFunc.ToUpper().Equals("AVERAGE") || consolFunc.ToUpper().Equals("AVG") )
				aggregate = Source.AGG_AVERAGE;
			else if ( consolFunc.ToUpper().Equals("MAX") || consolFunc.ToUpper().Equals("MAXIMUM") )
				aggregate = Source.AGG_MAXIMUM;
			else if ( consolFunc.ToUpper().Equals("MIN") || consolFunc.ToUpper().Equals("MINIMUM") )
				aggregate = Source.AGG_MINIMUM;
			else if ( consolFunc.ToUpper().Equals("LAST") )
				aggregate = Source.AGG_LAST;
			else if ( consolFunc.ToUpper().Equals("FIRST") )
				aggregate = Source.AGG_FIRST;
			else if ( consolFunc.ToUpper().Equals("TOTAL") )
				aggregate = Source.AGG_TOTAL;
			else
				throw new RrdException( "Invalid consolidation function specified." );
		}
		
		internal Gprint (string sourceName, string consolFunc, string test, double base1) : this(sourceName, consolFunc, test)
		{
			baseValue = base1;
		}

		
		internal void SetValue( Source[] sources, Hashtable sourceIndex, ValueFormatter vFormat )
		{
			try
			{
				double val 	= sources[ (int) sourceIndex[sourceName] ].GetAggregate( aggregate );
				
				double oldBase = vFormat.Base;
				if (baseValue != -1)
					vFormat.SetBase(baseValue);

				vFormat.SetFormat( val, numDec, strLen );
				vFormat.SetScaling( normalScale, uniformScale );
				
				string valueStr = vFormat.FormattedValue;
				string prefix	= vFormat.Prefix;
				
				// Replace all values
				for (int i = 0; i < oList.Count; i += 2 )
				{
					string str = (string) oList[i];
					
					str = VALUE_PATTERN.Replace(str, valueStr);
					str = str.Replace(VALUE_MARKER, valueStr);
					if ( normalScale ) str = str.Replace(SCALE_MARKER, prefix);
					if ( uniformScale ) str = str.Replace(UNIFORM_SCALE_MARKER, prefix);
					
					oList[i] = str ;
				}

				if (baseValue != -1)
					vFormat.SetBase(oldBase);
			}
			catch (Exception) 
			{
				throw new RrdException( "Could not find datasource: " + sourceName );
			}
		}
		
		
		private void CheckValuePlacement() 
		{
			MatchCollection m = VALUE_PATTERN.Matches(text);
			
			if( m.Count > 0)
			{	
				normalScale 	= (text.IndexOf(SCALE_MARKER) >= 0);
				uniformScale	= (text.IndexOf(UNIFORM_SCALE_MARKER) >= 0);
				
				if ( normalScale && uniformScale )
					throw new RrdException( "Can't specify normal scaling and uniform scaling at the same time." );
				
				char[] seps = {'\\'};
				GroupCollection gc = m[0].Groups;
				string[] group 	=  gc[1].ToString().Split(seps);
				strLen 			= -1;
				numDec 			= 0;
		
				if ( group.Length > 1 ) 
				{
					if ( group[0].Length > 0 ) {
						strLen 	= Int32.Parse(group[0]);
						numDec 	= Int32.Parse(group[1]);
					}
					else
						numDec 	= Int32.Parse(group[1]);
				}
				else
					numDec = Int32.Parse(group[0]);
			}
			else
				throw new RrdException( "Could not find where to place value. No @ placeholder found." );
		}
		
	}
}