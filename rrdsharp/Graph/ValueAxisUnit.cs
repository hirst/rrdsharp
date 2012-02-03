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
using System.Collections;

namespace RrdSharp.Graph
{
	internal class ValueAxisUnit 
	{
		//private double labelStep 	= 2;
		//private double markStep		= 1;
		//private int roundStep 		= 2;
	
		private double gridStep		= 2;
		private double labelStep	= 10;
	
	
		internal ValueAxisUnit( double gridStep, double labelStep )
		{
			this.gridStep	= gridStep;
			this.labelStep	= labelStep;
		}

		internal double GridStep
		{
			get
			{
				return gridStep;
			}
		}

		internal double LabelStep
		{
			get
			{
				return labelStep;
			}
		}
	
	
		internal ValueMarker[] GetValueMarkers( double lower, double upper )
		{
			double minPoint	= 0.0d;
			double majPoint	= 0.0d;
		
			// Find the first visible gridpoint
			if ( lower > 0 ) 
			{
				while ( minPoint < lower ) minPoint += gridStep;
				while ( majPoint < lower ) majPoint += labelStep;
			} 
			else 
			{
				while ( minPoint > lower ) minPoint -= gridStep;
				while ( majPoint > lower ) majPoint -= labelStep;
				// Go one up to make it visible
				if (minPoint != lower ) minPoint += gridStep;
				if (majPoint != lower ) majPoint += labelStep;
			}
		
			// Now get all time markers.
			// Again we choose to use a series of loops as to avoid unnecessary drawing.		
			ArrayList markerList	= new ArrayList();
		
			while ( minPoint <= upper && majPoint <= upper )
			{
                if (gridStep <= 0d) gridStep = 0.5d;
                if (labelStep <= 0d) gridStep = 1d;
                while (gridStep < 0.5)
                {
                    gridStep *= 10;
                }
                while (labelStep < 1)
                {
                    labelStep *= 10;
                }

				if ( minPoint < majPoint )
				{
					markerList.Add( new ValueMarker(minPoint, false) );
                    minPoint = Math.Round(minPoint + gridStep, MidpointRounding.AwayFromZero);	
				}
				else
				{
					if ( minPoint == majPoint )	// Special case, but will happen most of the time
					{
						markerList.Add( new ValueMarker(majPoint, true) );
                        minPoint = Math.Round(minPoint + gridStep, MidpointRounding.AwayFromZero);
                        majPoint = Math.Round(majPoint + labelStep, MidpointRounding.AwayFromZero);
					}
					else
					{
						markerList.Add( new ValueMarker(majPoint, true) );
                        majPoint = Math.Round(majPoint + labelStep, MidpointRounding.AwayFromZero);
					}
				}
			}

			while ( minPoint <= upper )
			{
				markerList.Add( new ValueMarker(minPoint, false) );
                minPoint = Math.Round(minPoint + gridStep, MidpointRounding.AwayFromZero);
			}

			while ( majPoint <= upper )
			{
				markerList.Add( new ValueMarker(majPoint, true) );
                majPoint = Math.Round(majPoint + labelStep, MidpointRounding.AwayFromZero);
			}
		
			return (ValueMarker[]) markerList.ToArray(typeof(ValueMarker));
		}
	
		
		internal double GetNiceLower( double ovalue )
		{
			// Add some checks
			double gridFactor	= 1.0;
			double mGridFactor	= 1.0;
		
			double gridStep		= this.gridStep;
			double mGridStep	= this.labelStep;
		
			if ( gridStep < 1.0 ) 
			{
				gridStep		*= 100;
				gridFactor		= 100;
			}
		
			if ( mGridStep < 1.0 ) 
			{
				mGridStep		*= 100;
				mGridFactor		= 100;
			}
		
			double val			= ovalue * gridFactor;
			int valueInt		= (int) val;
			int roundStep		= (int) gridStep;
			if ( roundStep == 0 ) roundStep = 1;
			int num 			= valueInt / roundStep; 
			//int mod 			= valueInt % roundStep;
			double gridValue	= (roundStep * num) * 1.0d;
			if ( gridValue > val )
				gridValue		-= roundStep;
		
			if ( num == 0 && val >= 0 )
				gridValue		= 0.0;
			else if ( Math.Abs(gridValue - val) < (gridStep) / 16 )
				gridValue		-= roundStep;
		
			val					= ovalue * mGridFactor;
			roundStep			= (int) mGridStep;
			if ( roundStep == 0 ) roundStep = 1;
			num					= valueInt / roundStep;
			//mod					= valueInt % roundStep;
			double mGridValue	= (roundStep * num) * 1.0d;
			if ( mGridValue > val )
				mGridValue		-= roundStep;
		
			if ( val != 0.0d )
			{
				if ( Math.Abs(mGridValue - gridValue) < (mGridStep) / 2)
					return mGridValue / mGridFactor;
				else
					return gridValue / gridFactor;
			}

			return ovalue;
		}
	
		internal double GetNiceHigher( double ovalue )
		{
			// Add some checks
			double gridFactor	= 1.0;
			double mGridFactor	= 1.0;
		
			double gridStep		= this.gridStep;
			double mGridStep	= this.labelStep;
		
			if ( gridStep < 1.0 ) 
			{
				gridStep 		*= 100;
				gridFactor		= 100;
			}
	
			if ( mGridStep < 1.0 ) 
			{
				mGridStep	*= 100;
				mGridFactor		= 100;
			}
		
			double val			= ovalue * gridFactor;
			long valueInt		= (long) val;
			long roundStep		= (long) gridStep;
			if ( roundStep == 0 ) roundStep = 1;
			long num 			=  valueInt / roundStep; 
			//int mod 			= valueInt % roundStep;
			double gridValue	= (roundStep * (num + 1)) * 1.0d;
			if ( gridValue - val < (gridStep) / 8 )
				gridValue		+= roundStep;
		
			val					= ovalue * mGridFactor;
			roundStep			= (long) mGridStep;
			if ( roundStep == 0 ) roundStep = 1;
			num					=  valueInt / roundStep;
			//mod					= valueInt % roundStep;
			double mGridValue	= (roundStep * (num + 1)) * 1.0d;
		
			if ( val != 0.0d )
			{
				if ( Math.Abs(mGridValue - gridValue) < (mGridStep) / 2)
					return mGridValue / mGridFactor;
				else
					return gridValue / gridFactor;
			}
		
			return ovalue;
		}
	
		
		private double Round( double val )
		{
            return Math.Round(val, 14, MidpointRounding.AwayFromZero);		// Big precision
		}
	
		private double Round( double val, int numDecs )
		{
            return Math.Round(val, numDecs, MidpointRounding.AwayFromZero);
		}
	}

}