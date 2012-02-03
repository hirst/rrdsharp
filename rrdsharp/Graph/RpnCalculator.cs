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
// of the GNU Lesser General internal License as published by the Free Software Foundation;
// either version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General internal License for more details.
//
// You should have received a copy of the GNU Lesser General internal License along with this
// library; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330,
// Boston, MA 02111-1307, USA.

using System;
using System.Collections;
using RrdSharp.Core;

namespace RrdSharp.Graph
{
	internal class RpnCalculator 
	{
	
		internal const byte TKN_CONSTANT		= 0;
		internal const byte TKN_DATASOURCE	= 1;
		internal const byte TKN_PLUS			= 2;
		internal const byte TKN_MINUS			= 3;
		internal const byte TKN_MULTIPLY		= 4;
		internal const byte TKN_DIVIDE		= 5;
		internal const byte TKN_MOD			= 6;
		internal const byte TKN_SIN			= 7;
		internal const byte TKN_COS			= 8;
		internal const byte TKN_LOG			= 9;
		internal const byte TKN_EXP			= 10;
		internal const byte TKN_FLOOR			= 11;
		internal const byte TKN_CEIL			= 12;
		internal const byte TKN_ROUND			= 13;
		internal const byte TKN_POW			= 14;
		internal const byte TKN_ABS			= 15;
		internal const byte TKN_SQRT			= 16;
		internal const byte TKN_RANDOM		= 17;
		internal const byte TKN_LT			= 18;
		internal const byte TKN_LE			= 19;
		internal const byte TKN_GT			= 20;
		internal const byte TKN_GE			= 21;
		internal const byte TKN_EQ			= 22;
		internal const byte TKN_IF			= 23;
		internal const byte TKN_MIN			= 24;
		internal const byte TKN_MAX			= 25;
		internal const byte TKN_LIMIT			= 26;
		internal const byte TKN_DUP			= 27;
		internal const byte TKN_EXC			= 28;
		internal const byte TKN_POP			= 29;
		internal const byte TKN_UN			= 30;
		internal const byte TKN_UNKN			= 31;
		internal const byte TKN_NOW			= 32;
		internal const byte TKN_TIME			= 33;
		internal const byte TKN_PI			= 34;
		internal const byte TKN_E				= 35;
		internal const byte TKN_AND			= 36;
		internal const byte TKN_OR			= 37;
		internal const byte TKN_XOR			= 38;
		internal const byte TKN_SAMPLES		= 39;
		internal const byte TKN_STEP			= 40;
		
		private double step;
		private Source[] sources;
		private ArrayList stack = new ArrayList();
		
		
		internal RpnCalculator( Source[] sources, double step )
		{
			this.sources	= sources;
			this.step		= step;
		}
		
		
		internal double Evaluate( Cdef cdef, int row, long timestamp )
		{
			stack.Clear();
			
			byte[] tokens 		= cdef.Tokens;
			int[] dsIndices		= cdef.DsIndices;
			double[] constants	= cdef.Constants;

			Random autoRand = new Random();
			
			double x1, x2, x3;
			
			for ( int i = 0; i < tokens.Length; i++ ) 
			{
				switch ( tokens[i] )
				{
					case TKN_CONSTANT:
						Push( constants[i] );
						break;
						
					case TKN_DATASOURCE:
						Push( sources[ dsIndices[i] ].Get(row) );
						break;
						
					case TKN_PLUS:
						Push(Pop() + Pop());
						break;
						
					case TKN_MINUS:
						x2 = Pop();
						x1 = Pop();
						Push(x1 - x2);
						break;
						
					case TKN_MULTIPLY:
						Push(Pop() * Pop());
						break;
						
					case TKN_DIVIDE:
						x2 = Pop();
						x1 = Pop();
						Push(x1 / x2);
						break;
						
					case TKN_MOD:
						x2 = Pop();
						x1 = Pop();
						Push(x1 % x2);
						break;
						
					case TKN_SIN:
						Push(Math.Sin(Pop()));
						break;
						
					case TKN_COS:
						Push(Math.Cos(Pop()));
						break;
						
					case TKN_LOG:
						Push(Math.Log(Pop()));
						break;
						
					case TKN_EXP:
						Push(Math.Exp(Pop()));
						break;
						
					case TKN_FLOOR:
						Push(Math.Floor(Pop()));
						break;
						
					case TKN_CEIL:
						Push(Math.Ceiling(Pop()));
						break;
						
					case TKN_ROUND:
						Push(Math.Round(Pop()));
						break;
						
					case TKN_POW:
						x2 = Pop();
						x1 = Pop();
						Push(Math.Pow(x1, x2));
						break;
						
					case TKN_ABS:
						Push(Math.Abs(Pop()));
						break;
						
					case TKN_SQRT:
						Push(Math.Sqrt(Pop()));
						break;
						
					case TKN_RANDOM:
						Push(autoRand.Next());
						break;
						
					case TKN_LT:
						x2 = Pop();
						x1 = Pop();
						Push(x1 < x2? 1: 0);
						break;
						
					case TKN_LE:
						x2 = Pop();
						x1 = Pop();
						Push(x1 <= x2? 1: 0);
						break;
						
					case TKN_GT:
						x2 = Pop();
						x1 = Pop();
						Push(x1 > x2? 1: 0);
						break;
						
					case TKN_GE:
						x2 = Pop();
						x1 = Pop();
						Push(x1 >= x2? 1: 0);
						break;
						
					case TKN_EQ:
						x2 = Pop();
						x1 = Pop();
						Push(x1 == x2? 1: 0);
						break;
						
					case TKN_IF:
						x3 = Pop();
						x2 = Pop();
						x1 = Pop();
						Push(x1 != 0 ? x2: x3);
						break;
						
					case TKN_MIN:
						Push(Math.Min(Pop(), Pop()));
						break;
						
					case TKN_MAX:
						Push(Math.Max(Pop(), Pop()));
						break;
						
					case TKN_LIMIT:
						double high = Pop(), low = Pop(), val = Pop();
						Push(val < low || val > high? Double.NaN: val);
						break;
						
					case TKN_DUP:
						double x = Pop();
						Push(x);
						Push(x);
						break;
						
					case TKN_EXC:
						x2 = Pop();
						x1 = Pop();
						Push(x2);
						Push(x1);
						break;
						
					case TKN_POP:
						Pop();
						break;
						
					case TKN_UN:
						Push(Double.IsNaN(Pop())? 1: 0);
						break;
						
					case TKN_UNKN:
						Push(Double.NaN);
						break;
						
					case TKN_NOW:
						Push(Util.Time);
						break;
						
					case TKN_TIME:
						Push(timestamp);
						break;
						
					case TKN_PI:
						Push(Math.PI);
						break;
						
					case TKN_E:
						Push(Math.E);
						break;
						
					case TKN_AND:
						x2 = Pop();
						x1 = Pop();
						Push((x1 != 0 && x2 != 0)? 1: 0);
						break;
						
					case TKN_OR:
						x2 = Pop();
						x1 = Pop();
						Push((x1 != 0 || x2 != 0)? 1: 0);
						break;
						
					case TKN_XOR:
						x2 = Pop();
						x1 = Pop();
						Push(((x1 != 0 && x2 == 0) || (x1 == 0 && x2 != 0))? 1: 0);
						break;

					case TKN_SAMPLES:
						Push (cdef.SampleCount);
						break;

					case TKN_STEP:
						Push( step );
						break;
				}
			}
			
			if (stack.Count != 1)
				throw new RrdException("RPN error, invalid stack length");
			
			return Pop();
		}
		
		
		private void Push( double val ) 
		{
			stack.Add( val );
		}

		private double Pop()
		{
			int last = stack.Count - 1;
			if ( last < 0 )
				throw new RrdException("POP failed, stack empty");
			
			double lastValue = (double) stack[last];
			stack.Remove(lastValue);
		
			return lastValue;
		}
	}
}
