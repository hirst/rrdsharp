// Bindings for rrdtool
// Testing
//
// Distributed under LGPL
//


using System;
using System.Runtime.InteropServices;

namespace RrdSharpBindings
{

	public class NativeMethods
	{
		
#if WIN32
		const string libname = "rrd.lib";
#else
		const string libname = "librrd";
#endif

		[DllImport(libname, EntryPoint="rrd_create")]
		private extern static int rrd_create (int argc, string[] argv);

		[DllImport(libname, EntryPoint="rrd_update")]
		private extern static int rrd_update (int argc, string[] argv);

		[DllImport(libname, EntryPoint="rrd_graph")]
		private extern static int rrd_graph (int argc, string[] argv, 
			ref string[] prdata, out int xsize, out int ysize);

		[DllImport(libname, EntryPoint="rrd_fetch")]
		private extern static int rrd_fetch (int argc, string[] argv, 
			out uint starttime, out uint endtime, out uint step, 
			out uint dscount, ref string[] dsnames, double [] data );

		[DllImport(libname, EntryPoint="rrd_restore")]
		private extern static int rrd_restore (int argc, string[] argv);

		[DllImport(libname, EntryPoint="rrd_dump")]
		private extern static int rrd_dump (int argc, string[] argv);

		[DllImport(libname, EntryPoint="rrd_tune")]
		private extern static int rrd_tune (int argc, string[] argv);

		[DllImport(libname, EntryPoint="rrd_last")]
		private extern static uint rrd_last (int argc, string[] argv);

		[DllImport(libname, EntryPoint="rrd_resize")]
		private extern static int rrd_resize (int argc, string[] argv);

		[DllImport(libname, EntryPoint="rrd_xport")]
		private extern static int rrd_xport (int argc, string[] argv, 
			out int xsize, out uint start, out uint end, out uint step, 
			out uint colcount, ref string[] legend, double [] data );
	}
}