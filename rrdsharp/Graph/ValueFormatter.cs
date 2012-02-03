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
using System.Text;

namespace RrdSharp.Graph
{
	internal class ValueFormatter 
	{
		
		internal static readonly int NO_SCALE 		= -1;
		internal static readonly double DEFAULT_BASE	= 1000.0;
		
		private double base1						= DEFAULT_BASE;
		private double[] scaleValues				= new double[] {
														1e18, 1e15, 1e12, 1e9, 1e6, 1e3, 1e0, 1e-3, 1e-6, 1e-9, 1e-12, 1e-15
														};
		internal static string[] PREFIXES 			= new string[] {
														"E",  "P",  "T",  "G", "M", "k", " ",  "m", "µ", "n", "p",  "f"
														};
												
		private double val;
		private string decFormat;
		private int formattedStrLen;
		
		private double scaledValue;
		private int scaleIndex						= NO_SCALE;		// Last used scale index
		private int fixedIndex						= NO_SCALE;
		private string prefix;
		
		private bool scale						= false;
		
		
		internal ValueFormatter() 
		{
		}
		
		
		internal ValueFormatter( double base1, int scaleIndex ) 
		{
			SetBase( base1 );
			this.fixedIndex	= scaleIndex;
		}
		
		
		internal void SetFormat( double val, int numDec, int strLen )
		{
			this.val 				= val;
			this.decFormat			= GetDecimalFormat( numDec );
			this.formattedStrLen 	= strLen;	
		}
		
		internal void SetScaling( bool normalScale, bool uniformScale )
		{
			if ( !uniformScale ) 
				scaleIndex = NO_SCALE;
			
			if ( fixedIndex >= 0 ) {
				scale 		= true;
				scaleIndex	= fixedIndex;
			}
			else {
				scale = (normalScale || uniformScale);
			}
		}
		
		/**
		* Formats the value with the given options and returns the result as a text string.
		* @return String containing the formatted value.
		*/
		internal string FormattedValue
		{
			get
			{
				string valueStr = val.ToString();
				
				if ( !Double.IsNaN(val) )
				{
					if ( scale ) 
					{
						ScaleValue( scaleIndex );
						valueStr = scaledValue.ToString(decFormat);
					}
					else
						valueStr = val.ToString(decFormat);
				}
				
				// Fix the formatted string to the correct length
				int diff = formattedStrLen - valueStr.Length;
				
				StringBuilder preSpace = new StringBuilder("");
				for (int i = 0; i < diff; i++)
					preSpace.Append(' ');
					
				valueStr = preSpace.Append(valueStr).ToString();
				
				return valueStr;
			}
		}
		
		internal string ScaledValue
		{
			get
			{
				int tsv 	= scaleIndex;
				ScaleValue( tsv );
				long intVal = (long) scaledValue;
				
				scaleIndex	= tsv;
				
				if ( intVal == scaledValue )
					return intVal.ToString();
				else
					return scaledValue.ToString();
			}
		}
		
		internal void SetBase( double baseValue ) 
		{
			if ( baseValue == this.base1 )
				return;
			
			this.base1			= baseValue;
			double tmp 			= 1;
			for (int i = 1; i < 7; i++) 
			{
				tmp 				*= baseValue;
				scaleValues[6 - i] 	= tmp;
			}
			tmp = 1;
			for (int i = 7; i < scaleValues.Length; i++)
			{
				tmp					*= baseValue;
				scaleValues[i]	 	= ( 1 / tmp );
			}
		}
		
		internal double Base
		{
			get
			{
				return base1;
			}
		}
		
		internal string Prefix
		{ 
			get
			{
				return prefix;
			}
		}	
		
		
		private void ScaleValue( int scaleIndex)
		{
			double absValue = Math.Abs(val);
			if (scaleIndex == NO_SCALE) 
			{
				this.prefix 		= " ";
				this.scaledValue 	= val;
			
				for (int i = 0; i < scaleValues.Length; i++) 
				{
					if (absValue >= scaleValues[i] && absValue < scaleValues[i] * base1) 
					{
						if ( scaleValues[i] != 1e-3 )	// Special case
						{
							this.prefix 		= PREFIXES[i];
							this.scaledValue 	= val / scaleValues[i];
							this.scaleIndex		= i;
							return;
						}
					}
				}
			}
			else {
				this.prefix 		= PREFIXES[scaleIndex];
				this.scaledValue 	= val / scaleValues[scaleIndex];
				this.scaleIndex 	= scaleIndex;
			}
		}
		
		/**
		* Retrieves a <code>DecimalFormat</code> string to format the value, based on a given number of decimals that should
		* be used.
		* @param numDec Number of decimals to use in the formatted value.
		* @return DecimalFormat to use for formatting.
		*/
		private string GetDecimalFormat( int numDec ) 
		{
			StringBuilder formatStr = new StringBuilder("0");		// "#,##0", removed the 'grouping' separator
			for(int i = 0; i < numDec; i++) {
				if(i == 0) {
					formatStr.Append('.');
				}
				formatStr.Append('0');
			}

			return formatStr.ToString();
		}
	}
}