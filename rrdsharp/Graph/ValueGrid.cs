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
	internal class ValueGrid 
	{
		private bool rigid;
		private double lower;
		private double upper;

		private double baseValue		= ValueFormatter.DEFAULT_BASE;
		private double[] scaleValues	= new double[] { 1e18, 1e15, 1e12, 1e9, 1e6, 1e3, 1e0, 1e-3, 1e-6, 1e-9, 1e-12, 1e-15 };
	
		private ValueAxisUnit vAxis;
	
	
		internal ValueGrid( GridRange gr, double low, double up, ValueAxisUnit vAxis, double basevalue )
		{
			double grLower = Double.MaxValue;
			double grUpper = Double.MinValue;
		
			if ( gr != null )
			{
				this.rigid		= gr.Rigid;
				grLower			= gr.LowerValue;
				grUpper			= gr.UpperValue; 	
			}
		
			this.lower	= low;
			this.upper	= up;
			this.vAxis	= vAxis;
			this.baseValue	= basevalue;
		
			// Fill in the scale values
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

			// Set an appropriate value axis it not given yet
			SetValueAxis();
		
			if ( !rigid ) 
			{
				this.lower		= ( lower == grLower ? grLower : this.vAxis.GetNiceLower( lower ) );
				this.upper		= ( upper == grUpper ? grUpper : this.vAxis.GetNiceHigher( upper ) );
			}	
		}
	
	
		internal double LowerValue 
		{
			get
			{
				return lower;
			}
		}

		internal double UpperValue 
		{
			get
			{
				return upper;
			}
		}

		internal ValueMarker[] ValueMarkers 
		{
			get
			{
				return vAxis.GetValueMarkers( lower, upper );
			}
		}
	
		
		private void SetValueAxis()
		{
			if ( vAxis != null )
				return;
		
			if ( upper == Double.NaN  || upper == Double.MinValue || upper == Double.MaxValue )
				upper = 0.9;
			if ( lower == Double.NaN || lower == Double.MaxValue || lower == Double.MinValue )
				lower = 0;
		
			if ( !rigid && upper == 0 && upper == lower )
				upper = 0.9;
		
			// Determine nice axis grid
			double shifted = Math.Abs(upper - lower);
			if ( shifted == 0 )			// Special case, no 'range' available
				shifted = upper;
		
			// Find the scaled unit for this range
			double mod		= 1.0;
			int scaleIndex 	=  scaleValues.Length - 1;
			while ( scaleIndex >= 0 && scaleValues[scaleIndex] < shifted ) 
				scaleIndex--;
		
			// Keep the rest of division
			shifted 		= shifted / scaleValues[++scaleIndex];

			// While rest > 10, divide by 10
			while ( shifted > 10.0 ) 
			{
				shifted /= 10;
				mod	*= 10;
			}
		
			while ( shifted < 1.0 ) 
			{
				shifted *= 10;
				mod /= 10;
			}
	
			// Create nice grid based on 'fixed' ranges
			if ( shifted <= 1.5 )
				vAxis = new ValueAxisUnit( 0.1 * mod * scaleValues[scaleIndex], 0.5 * mod * scaleValues[scaleIndex] );
			else if ( shifted <= 3 )
				vAxis = new ValueAxisUnit( 0.2 * mod * scaleValues[scaleIndex], 1.0 * mod * scaleValues[scaleIndex] );
			else if ( shifted <= 5 )
				vAxis = new ValueAxisUnit( 0.5 * mod * scaleValues[scaleIndex], 1.0 * mod * scaleValues[scaleIndex] );
			else 
				vAxis = new ValueAxisUnit( 0.5 * mod * scaleValues[scaleIndex], 2.0 * mod * scaleValues[scaleIndex]);
		}
	}

}