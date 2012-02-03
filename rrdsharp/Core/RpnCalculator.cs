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

	internal class RpnCalculator
	{
		const string VAR_PLACEHOLDER = "value";

		private const byte TOK_VAR = 0;
		private const byte TOK_NUM = 1;
		private const byte TOK_PLUS = 2;
		private const byte TOK_MINUS = 3;
		private const byte TOK_MULT = 4;
		private const byte TOK_DIV = 5;
		private const byte TOK_MOD = 6;
		private const byte TOK_SIN = 7;
		private const byte TOK_COS = 8;
		private const byte TOK_LOG = 9;
		private const byte TOK_EXP = 10;
		private const byte TOK_FLOOR = 11;
		private const byte TOK_CEIL = 12;
		private const byte TOK_ROUND = 13;
		private const byte TOK_POW = 14;
		private const byte TOK_ABS = 15;
		private const byte TOK_SQRT = 16;
		private const byte TOK_RANDOM = 17;
		private const byte TOK_LT = 18;
		private const byte TOK_LE = 19;
		private const byte TOK_GT = 20;
		private const byte TOK_GE = 21;
		private const byte TOK_EQ = 22;
		private const byte TOK_IF = 23;
		private const byte TOK_MIN = 24;
		private const byte TOK_MAX = 25;
		private const byte TOK_LIMIT = 26;
		private const byte TOK_DUP = 27;
		private const byte TOK_EXC = 28;
		private const byte TOK_POP = 29;
		private const byte TOK_UN = 30;
		private const byte TOK_UNKN = 31;
		// private const byte TOK_NOW = 32;
		// private const byte TOK_TIME = 33;
		private const byte TOK_PI = 34;
		private const byte TOK_E = 35;
		private const byte TOK_AND = 36;
		private const byte TOK_OR = 37;
		private const byte TOK_XOR = 38;

		private string[] tokens;
		private byte[] tokenCodes;
		private double[] parsedDoubles;
		private RpnStack stack = new RpnStack();

		private string rpnExpression;
		private double val;
		// private long timestamp;

		internal RpnCalculator(string rpnExpression) 
		{
			this.rpnExpression = rpnExpression;
			CreateTokens();
		}

		internal double Value
		{
			get
			{
				return val;
			}
			set
			{
				this.val = value;
			}
		}

		/* not supported yet
		public void setTimestamp(long timestamp) {
			this.timestamp = timestamp;
		}
		*/

		private void CreateTokens()
		{
			char[] seps = { ',', '\n','\r' };
			tokens = rpnExpression.Split(seps);
			int count = tokens.Length;
			tokenCodes = new byte[count];
			parsedDoubles = new double[count];
			for(int i = 0; i < count; i++)
			{
				byte tokenCode = FindTokenCode(tokens[i]);
				tokenCodes[i] = tokenCode;
				if(tokenCode == TOK_NUM) 
				{
					parsedDoubles[i] = Double.Parse(tokens[i]);
				}
			}
		}

		private byte FindTokenCode(string token)
		{
			if(IsVariable(token)) {
				return TOK_VAR;
			}
			else if(IsNumber(token)) {
				return TOK_NUM;
			}
			else if(token.Equals("+")) {
				return TOK_PLUS;
			}
			else if(token.Equals("-")) {
				return TOK_MINUS;
			}
			else if(token.Equals("*")) {
				return TOK_MULT;
			}
			else if(token.Equals("/")) {
				return TOK_DIV;
			}
			else if(token.Equals("%")) {
				return TOK_MOD;
			}
			else if(token.Equals("SIN")) {
				return TOK_SIN;
			}
			else if(token.Equals("COS")) {
				return TOK_COS;
			}
			else if(token.Equals("LOG")) {
				return TOK_LOG;
			}
			else if(token.Equals("EXP")) {
				return TOK_EXP;
			}
			else if(token.Equals("FLOOR")) {
				return TOK_FLOOR;
			}
			else if(token.Equals("CEIL")) {
				return TOK_CEIL;
			}
			else if(token.Equals("ROUND")) {
				return TOK_ROUND;
			}
			else if(token.Equals("POW")) {
				return TOK_POW;
			}
			else if(token.Equals("ABS")) {
				return TOK_ABS;
			}
			else if(token.Equals("SQRT")) {
				return TOK_SQRT;
			}
			else if(token.Equals("RANDOM")) {
				return TOK_RANDOM;
			}
			else if(token.Equals("LT")) {
				return TOK_LT;
			}
			else if(token.Equals("LE")) {
				return TOK_LE;
			}
			else if(token.Equals("GT")) {
				return TOK_GT;
			}
			else if(token.Equals("GE")) {
				return TOK_GE;
			}
			else if(token.Equals("EQ")) {
				return TOK_EQ;
			}
			else if(token.Equals("IF")) {
				return TOK_IF;
			}
			else if(token.Equals("MIN")) {
				return TOK_MIN;
			}
			else if(token.Equals("MAX")) {
				return TOK_MAX;
			}
			else if(token.Equals("LIMIT")) {
				return TOK_LIMIT;
			}
			else if(token.Equals("DUP")) {
				return TOK_DUP;
			}
			else if(token.Equals("EXC")) {
				return TOK_EXC;
			}
			else if(token.Equals("POP")) {
				return TOK_POP;
			}
			else if(token.Equals("UN")) {
				return TOK_UN;
			}
			else if(token.Equals("UNKN")) {
				return TOK_UNKN;
			}

			/* not supported yet
			else if(token.Equals("NOW")) {
				return TOK_NOW;
			}
			else if(token.Equals("TIME")) {
				return TOK_TIME;
			}
			*/
			else if(token.Equals("PI")) {
				return TOK_PI;
			}
			else if(token.Equals("E")) {
				return TOK_E;
			}
			else if(token.Equals("AND")) {
				return TOK_AND;
			}
			else if(token.Equals("OR")) {
				return TOK_OR;
			}
			else if(token.Equals("XOR")) {
				return TOK_XOR;
			}
			else {
				throw new RrdException("Unknown RPN token encountered: " + token);
			}
		}

		private static bool IsNumber(string token)
		{
			try
			{
				Double.Parse(token);
				return true;
			}
			catch(FormatException)
			{
				return false;
			}
		}

		private static bool IsVariable(string token)
		{
			return token.Equals(VAR_PLACEHOLDER);
		}

		internal double Calculate() 
		{
			ResetCalculator();
			Random autoRand = new Random();
			for(int i = 0; i < tokenCodes.Length; i++)
			{
				byte tokenCode = tokenCodes[i];
				double x1, x2, x3;
				switch(tokenCode)
				{
					case TOK_NUM:
						Push(parsedDoubles[i]); break;
					case TOK_VAR:
						Push(val); break;
					case TOK_PLUS:
						Push(Pop() + Pop()); break;
					case TOK_MINUS:
						x2 = Pop(); x1 = Pop(); Push(x1 - x2); break;
					case TOK_MULT:
						Push(Pop() * Pop()); break;
					case TOK_DIV:
						x2 = Pop(); x1 = Pop();	Push(x1 / x2); break;
					case TOK_MOD:
						x2 = Pop(); x1 = Pop(); Push(x1 % x2); break;
					case TOK_SIN:
						Push(Math.Sin(Pop())); break;
					case TOK_COS:
						Push(Math.Cos(Pop())); break;
					case TOK_LOG:
						Push(Math.Log(Pop())); break;
					case TOK_EXP:
						Push(Math.Exp(Pop())); break;
					case TOK_FLOOR:
						Push(Math.Floor(Pop())); break;
					case TOK_CEIL:
						Push(Math.Ceiling(Pop())); break;
					case TOK_ROUND:
						Push(Math.Round(Pop())); break;
					case TOK_POW:
						x2 = Pop(); x1 = Pop();	Push(Math.Pow(x1, x2)); break;
					case TOK_ABS:
						Push(Math.Abs(Pop())); break;
					case TOK_SQRT:
						Push(Math.Sqrt(Pop())); break;
					case TOK_RANDOM:
						Push(autoRand.Next()); break;
					case TOK_LT:
						x2 = Pop(); x1 = Pop(); Push(x1 < x2? 1: 0); break;
					case TOK_LE:
						x2 = Pop(); x1 = Pop(); Push(x1 <= x2? 1: 0); break;
					case TOK_GT:
						x2 = Pop(); x1 = Pop(); Push(x1 > x2? 1: 0); break;
					case TOK_GE:
						x2 = Pop(); x1 = Pop(); Push(x1 >= x2? 1: 0); break;
					case TOK_EQ:
						x2 = Pop(); x1 = Pop();	Push(x1 == x2? 1: 0); break;
					case TOK_IF:
						x3 = Pop(); x2 = Pop(); x1 = Pop();	Push(x1 != 0? x2: x3); break;
					case TOK_MIN:
						Push(Math.Min(Pop(), Pop())); break;
					case TOK_MAX:
						Push(Math.Max(Pop(), Pop())); break;
					case TOK_LIMIT:
						x3 = Pop(); x2 = Pop(); x1 = Pop();
						Push(x1 < x2 || x1 > x3? Double.NaN: x1); break;
					case TOK_DUP:
						x1 = Pop(); Push(x1); Push(x1); break;
					case TOK_EXC:
						x2 = Pop(); x1 = Pop();	Push(x2); Push(x1); break;
					case TOK_POP:
						Pop(); break;
					case TOK_UN:
						Push(Double.IsNaN(Pop())? 1: 0); break;
					case TOK_UNKN:
						Push(Double.NaN); break;
					/* not supported yet
					case TOK_NOW:
						Push(Util.getTime()); break;
					case TOK_TIME:
						Push(timestamp); break;
					*/
					case TOK_PI:
						Push(Math.PI); break;
					case TOK_E:
						Push(Math.E); break;
					case TOK_AND:
						x2 = Pop(); x1 = Pop();	Push((x1 != 0 && x2 != 0)? 1: 0); break;
					case TOK_OR:
						x2 = Pop(); x1 = Pop();	Push((x1 != 0 || x2 != 0)? 1: 0); break;
					case TOK_XOR:
						x2 = Pop(); x1 = Pop();
						Push(((x1 != 0 && x2 == 0) || (x1 == 0 && x2 != 0))? 1: 0); break;
					default:
						throw new RrdException("Unexpected RPN token encountered [" +
							tokenCode + "]");
				}
			}
			double retVal = Pop();
			if(!IsStackEmpty())
			{
				throw new RrdException("Stack not empty at the end of calculation. " +
					"Probably bad RPN expression");
			}
			return retVal;
		}

		internal void Push(double x)
		{
			stack.Push(x);
		}

		internal double Pop()
		{
			return stack.Pop();
		}

		internal void ResetCalculator()
		{
			stack.Reset();
		}

		internal bool IsStackEmpty()
		{
			return stack.IsEmpty();
		}

		internal class RpnStack 
		{
			const int MAX_STACK_SIZE = 1000;
			private double[] stack = new double[MAX_STACK_SIZE];
			private int pos = 0;

			internal void Push(double x) 
			{
				if(pos >= MAX_STACK_SIZE)
				{
					throw new RrdException(
						"PUSH failed, RPN stack full [" + MAX_STACK_SIZE + "]");
				}
				stack[pos++] = x;
			}

			internal double Pop()
			{
				if(pos <= 0)
				{
					throw new RrdException("POP failed, RPN stack is empty ");
				}
				return stack[--pos];
			}

			internal void Reset()
			{
				pos = 0;
			}

			internal bool IsEmpty()
			{
				return pos == 0;
			}
		}

	}


}