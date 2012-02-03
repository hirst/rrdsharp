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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using RrdSharp.Core;


namespace RrdSharp.Graph
{
	/// <summary>
	/// 
	/// </summary>
	public class RrdGraph
	{
		private Grapher grapher;
		private Bitmap img;
		
		private bool useImageSize			= false;
	
	
		/// <summary>
		/// 
		/// </summary>
		public RrdGraph() 
		{	
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="graphDef"></param>
		public RrdGraph( RrdGraphDef graphDef )
		{
			grapher = new Grapher(graphDef, this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="specImgSize"></param>
		public void SpecifyImageSize( bool specImgSize )
		{
			this.useImageSize = specImgSize;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="graphDef"></param>
		public void SetGraphDef( RrdGraphDef graphDef ) 
		{
			img		= null;
			grapher = new Grapher( graphDef, this );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void SaveAsPNG( string path )
		{
			SaveAsPNG( path, 0, 0 );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void SaveAsPNG( string path, int width, int height ) 
		{
			
			Bitmap b = GetBufferedImage(width, height);
			b.Save(path, ImageFormat.Png);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void SaveAsGIF( string path ) 
		{
			SaveAsGIF( path, 0, 0 );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void SaveAsGIF(string path, int width, int height) 
		{
			Bitmap b = GetBufferedImage(width, height);
			b.Save(path, ImageFormat.Gif);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="quality"></param>
		public void SaveAsJPEG( string path, float quality ) 
		{
			SaveAsJPEG( path, 0, 0, quality );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="quality"></param>
		public void SaveAsJPEG( string path, int width, int height, float quality ) 
		{
			Bitmap b = GetBufferedImage(width, height);
			b.Save(path, ImageFormat.Jpeg);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public byte[] GetPNGBytes() 
		{
			return GetPNGBytes( 0, 0 );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public byte[] GetPNGBytes( int width, int height ) 
		{
			MemoryStream ms = new MemoryStream();
			
			Bitmap b = GetBufferedImage(width, height);
			b.Save(ms, ImageFormat.Png);
			ms.Flush();
			return ms.ToArray();

		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="quality"></param>
		/// <returns></returns>
		public byte[] GetJPEGBytes( float quality ) 
		{
			return GetJPEGBytes( 0, 0, quality );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="quality"></param>
		/// <returns></returns>
		public byte[] GetJPEGBytes( int width, int height, float quality ) 
		{
			MemoryStream ms = new MemoryStream();
			
			Bitmap b = GetBufferedImage(width, height);
			b.Save(ms, ImageFormat.Jpeg);
			ms.Flush();
			return ms.ToArray();
	
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public byte[] GetGIFBytes() 
		{
			return GetGIFBytes( 0, 0 );	
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public byte[] GetGIFBytes(int width, int height) 
		{
			MemoryStream ms = new MemoryStream();
			
			Bitmap b = GetBufferedImage(width, height);
			b.Save(ms, ImageFormat.Gif);
			ms.Flush();
			return ms.ToArray();
	
		}
		
		internal RrdDb GetRrd( string rrdFile ) 
		{
			return new RrdDb( rrdFile );
		}
			
		internal void ReleaseRrd(RrdDb rrdDb)
		{
			rrdDb.Close();
		}

		private Bitmap GetBufferedImage(int width, int height) 
		{
			// Always regenerate graph
			if ( useImageSize )
				img = grapher.CreateImageGlobal( width, height);
			else
				img = grapher.CreateImage( width, height);
				
			return img;
		}
	}

}