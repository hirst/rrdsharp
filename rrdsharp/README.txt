RrdSharp README
---------------

I want to give a special thanks to Sasa Markovic and 
Arne Vandamme of the JRobin project (http://www.jrobin.org)
for their fantastic Java implementation of RRDTool that
this is based on.  

The majority of RrdSharp is a port of JRobin 1.3.1 with a sprinkle 
of 1.4 (very small sprinkle).  It is not yet a complete port
however.  No XML-based features (export, import, etc) are implemented.
Also, rrd files from JRobin are not compatible with RrdSharp and
vice-versa.  This is because Java is big-endian based and .NET/Mono 
are little-endian.  I didn't feel there was reason to make them
compatible.  However, I am thinking it might be a good idea to 
add support for both file types.

I'll add instructions with the 0.1 release. 