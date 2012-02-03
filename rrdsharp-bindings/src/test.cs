// Test.cs

using System;
using RrdSharpBindings;

public class Tester
{


	public static void Main()
	{
		string cmd1 = "--start 999999999 --step 300 DS:temp1:GAUGE:1800:U:U RRA:AVERAGE:0.5:1:600 RRA:AVERAGE:0.5:6:700";	
		string cmd2 = "--title=\"Demo\" DEF:myspeed=temperature.rrd:temp1:AVERAGE LINE1:myspeed#FF0000";

		//RrdCreate("temperature.rrd", cmd1);
		RrdGraph("temp.gif", cmd2);
					
	}
}