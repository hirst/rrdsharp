// ============================================================
//  RRDSharp-bindings: Bindings for RRDTool for .NET/Mono
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

namespace RrdSharpBindings
{

	public class RrdSharp
	{
		
		
		public static void RrdCreate(string filename, string parameters)
		{
			int returnValue;
			parameters.Trim();
			parameters = "create " + filename + " " + parameters;
			string [] pieces = parameters.Split();
		
			returnValue = NativeMethods.rrd_create(pieces.Length, pieces);

		}

		public static void RrdUpdate(string filename, string parameters)
		{
			parameters.Trim();
			parameters = "update " + filename + " " + parameters;
			string [] pieces = parameters.Split();
		
			NativeMethods.rrd_update(pieces.Length, pieces);

		}

		public static void RrdGraph(string filename, string parameters)
		{
			parameters.Trim();
			parameters = "graph " + filename + " " + parameters;
			string [] pieces = parameters.Split();
		
			IntPtr prdata = IntPtr.Zero; 
			int xsize, ysize;
			NativeMethods.rrd_graph(pieces.Length, pieces, ref prdata, out xsize, out ysize); 
			//Marshal.FreeHGlobal(prdata);
			//Console.WriteLine("{0}x{1}",xsize,ysize);

		
		}

		public static void RrdFetch()
		{
			Console.WriteLine("La La");
		}

		public static void RrdRestore(string filename, string parameters)
		{
			parameters.Trim();
			parameters = "restore " + filename + " " + parameters;
			string [] pieces = parameters.Split();
		
			rrd_restore(pieces.Length, pieces);

		}

		public static void RrdDump()
		{
		}

		public static void RrdTune()
		{
		}

		public static void RrdLast()
		{
		}

		public static void RrdXport()
		{
		}


	}
}
