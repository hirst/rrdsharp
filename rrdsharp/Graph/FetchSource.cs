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
using RrdSharp.Core;


namespace RrdSharp.Graph
{
	internal class FetchSource 
	{
		internal static readonly int AVG			= 0;
		internal static readonly int MAX 			= 1;
		internal static readonly int MIN 			= 2;
		internal static readonly int LAST			= 3;
		internal static readonly int MAX_CF 		= 4;
		
		internal static readonly string[] cfNames	= new string[] { "AVERAGE", "MAX", "MIN", "LAST" };
		
		private string rrdFile;						// Holds the name of the RRD file
		
		private int numSources					= 0;
		private ArrayList[] datasources			= new ArrayList[MAX_CF];
		
		
		internal FetchSource( string rrdFile )
		{
			this.rrdFile = rrdFile;
			
			// Initialization of datasource lists per CF
			for (int i = 0; i < datasources.Length; i++)
				datasources[i] = new ArrayList();	
		}
		
		internal FetchSource( string rrdFile, string consolFunc, string dsName, string name ) : this (rrdFile)
		{
			AddSource( consolFunc, dsName, name );	
		}
		
		
		internal void AddSource( string consolFunc, string dsName, string name )
		{
			if ( consolFunc.ToUpper().Equals("AVERAGE") || consolFunc.ToUpper().Equals("AVG") )
				datasources[AVG].Add( new string[] { dsName, name } );
			else if ( consolFunc.ToUpper().Equals("MAX") || consolFunc.ToUpper().Equals("MAXIMUM") )
				datasources[MAX].Add( new string[] { dsName, name } );
			else if ( consolFunc.ToUpper().Equals("MIN") || consolFunc.ToUpper().Equals("MINIMUM") )
				datasources[MIN].Add( new string[] { dsName, name } );
			else if ( consolFunc.ToUpper().Equals("LAST") )
				datasources[LAST].Add( new string[] { dsName, name } );
			else
				throw new RrdException( "Invalid consolidation function specified." );
			
			numSources++;				
		}
		
		internal ValueExtractor Fetch ( RrdDb rrd, long startTime, long endTime )
		{
			long rrdStep			= rrd.RrdDef.Step;
			FetchData[] result		= new FetchData[datasources.Length];
			
			string[] names 			= new string[numSources];
			int tblPos		= 0;
			
			for (int i = 0; i < datasources.Length; i++)
			{
				if ( datasources[i].Count > 0 ) 
				{
					// Set the list of ds names
					string[] dsNames 	= new string[ datasources[i].Count ];
					string[] vNames		= new string[ datasources[i].Count ];
					
					for (int j = 0; j < dsNames.Length; j++ )
					{
						string[] spair	= (string[]) datasources[i][j];
						dsNames[j]	 	= spair[0];
						vNames[j]		= spair[1];
					}
					
					// Fetch datasources
					FetchRequest request 		= rrd.CreateFetchRequest( cfNames[i], startTime, endTime + rrdStep);
					request.SetFilter(dsNames);
					
					FetchData data				= request.FetchData();
					
					for (int j = 0; j < vNames.Length; j++)
						names[ data.GetDsIndex(dsNames[j]) + tblPos ] = vNames[j];
					tblPos				+= dsNames.Length; 
					
					result[i]					= data;
				}
			}
			
			return new ValueExtractor( names, result );
		}

		internal string RrdFile
		{
			get
			{
				return rrdFile;
			}
		}	
	}

}