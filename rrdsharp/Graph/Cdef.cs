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
	internal class Cdef : Source
	{
	
		private string[] strTokens;
		private double[] constants;
		private int[] dsIndices;
		private byte[] tokens;
	
	
		internal Cdef( string name, string rpn ) : base (name)
		{
			char[] seps = { ',', '\n','\r' };
			strTokens = rpn.Split(seps);
			int count = strTokens.Length;
		}


		internal virtual void Prepare( Hashtable sourceIndex, int numPoints )
		{
			// Create values table of correct size
			values = new double[numPoints];
		
			// Parse rpn expression for better performance
			string tkn;
		
			constants 	= new double[ strTokens.Length ];
			dsIndices	= new int[ strTokens.Length ];
			tokens		= new byte[ strTokens.Length ];
		
			for (int i = 0; i < strTokens.Length; i++)
			{
				tkn = strTokens[i];
			
				if ( IsNumber(tkn) )
				{
					tokens[i]		= RpnCalculator.TKN_CONSTANT;
					constants[i]	= Double.Parse(tkn);
				}
				else if ( sourceIndex.ContainsKey(tkn) )
				{
					tokens[i]		= RpnCalculator.TKN_DATASOURCE;
					dsIndices[i]	= ( (int) sourceIndex[tkn]);
				}
				else if ( tkn.Equals("+") )
					tokens[i]		= RpnCalculator.TKN_PLUS;
				else if ( tkn.Equals("-") )
					tokens[i]		= RpnCalculator.TKN_MINUS;
				else if ( tkn.Equals("*") )
					tokens[i]		= RpnCalculator.TKN_MULTIPLY;
				else if ( tkn.Equals("/") )
					tokens[i]		= RpnCalculator.TKN_DIVIDE;
				else if ( tkn.Equals("%") )
					tokens[i]		= RpnCalculator.TKN_MOD;
				else if ( tkn.Equals("SIN") )
					tokens[i]		= RpnCalculator.TKN_SIN;
				else if ( tkn.Equals("COS") )
					tokens[i]		= RpnCalculator.TKN_COS;
				else if ( tkn.Equals("LOG") )
					tokens[i]		= RpnCalculator.TKN_LOG;
				else if ( tkn.Equals("EXP") )
					tokens[i]		= RpnCalculator.TKN_EXP;
				else if ( tkn.Equals("FLOOR") )
					tokens[i]		= RpnCalculator.TKN_FLOOR;
				else if ( tkn.Equals("CEIL") )
					tokens[i]		= RpnCalculator.TKN_CEIL;
				else if ( tkn.Equals("ROUND") )
					tokens[i]		= RpnCalculator.TKN_ROUND;
				else if ( tkn.Equals("POW") )
					tokens[i]		= RpnCalculator.TKN_POW;
				else if ( tkn.Equals("ABS") )
					tokens[i]		= RpnCalculator.TKN_ABS;
				else if ( tkn.Equals("SQRT") )
					tokens[i]		= RpnCalculator.TKN_SQRT;
				else if ( tkn.Equals("RANDOM") )
					tokens[i]		= RpnCalculator.TKN_RANDOM;
				else if ( tkn.Equals("LT") )
					tokens[i]		= RpnCalculator.TKN_LT;
				else if ( tkn.Equals("LE") )
					tokens[i]		= RpnCalculator.TKN_LE;
				else if ( tkn.Equals("GT") )
					tokens[i]		= RpnCalculator.TKN_GT;
				else if ( tkn.Equals("GE") )
					tokens[i]		= RpnCalculator.TKN_GE;
				else if ( tkn.Equals("EQ") )
					tokens[i]		= RpnCalculator.TKN_EQ;
				else if ( tkn.Equals("IF") )
					tokens[i]		= RpnCalculator.TKN_IF;
				else if ( tkn.Equals("MIN") )
					tokens[i]		= RpnCalculator.TKN_MIN;
				else if ( tkn.Equals("MAX") )
					tokens[i]		= RpnCalculator.TKN_MAX;
				else if ( tkn.Equals("LIMIT") )
					tokens[i]		= RpnCalculator.TKN_LIMIT;
				else if ( tkn.Equals("DUP") )
					tokens[i]		= RpnCalculator.TKN_DUP;
				else if ( tkn.Equals("EXC") )
					tokens[i]		= RpnCalculator.TKN_EXC;
				else if ( tkn.Equals("POP") )
					tokens[i]		= RpnCalculator.TKN_POP;
				else if ( tkn.Equals("UN") )
					tokens[i]		= RpnCalculator.TKN_UN;
				else if ( tkn.Equals("UNKN") )
					tokens[i]		= RpnCalculator.TKN_UNKN;
				else if ( tkn.Equals("NOW") )
					tokens[i]		= RpnCalculator.TKN_NOW;
				else if ( tkn.Equals("TIME") )
					tokens[i]		= RpnCalculator.TKN_TIME;
				else if ( tkn.Equals("PI") )
					tokens[i]		= RpnCalculator.TKN_PI;
				else if ( tkn.Equals("E") )
					tokens[i]		= RpnCalculator.TKN_E;
				else if ( tkn.Equals("AND") )
					tokens[i]		= RpnCalculator.TKN_AND;
				else if ( tkn.Equals("OR") )
					tokens[i]		= RpnCalculator.TKN_OR;
				else if ( tkn.Equals("XOR") )
					tokens[i]		= RpnCalculator.TKN_XOR;
				else
					throw new RrdException("Unknown token enocuntered: " + tkn);	
			
			}
		}

	
		internal override void Set( int pos, long timestamp, double val )
		{
			base.Set( pos, timestamp, val );
			values[pos] = val;
		}
	
		internal byte[] Tokens 
		{
			get
			{
				return tokens;
			}
		}
	
		internal double[] Constants
		{
			get
			{
				return constants;
			}
		}
	
		internal int[] DsIndices
		{
			get
			{
				return dsIndices;
			}
		}
	
	
		
		private bool IsNumber( string token ) 
		{
			try 
			{
				Double.Parse(token);
			
				return true;
			}
			catch (FormatException) 
			{
				return false;
			}
		}
	}
	
}