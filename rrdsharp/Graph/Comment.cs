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
using System.Collections;

namespace RrdSharp.Graph
{
	internal class Comment 
	{
		internal static readonly int CMT_DEFAULT	= 0;
		internal static readonly int CMT_LEGEND		= 1;
		internal static readonly int CMT_GPRINT		= 2;
		internal static readonly int CMT_NOLEGEND	= 3;
	
		internal static readonly byte TKN_ALF		= (byte) 1;		// Align left with Linefeed
		internal static readonly byte TKN_ARF		= (byte) 2;		// Align right with linefeed
		internal static readonly byte TKN_ACF		= (byte) 3;		// Align center with linefeed
		internal static readonly byte TKN_AL		= (byte) 4;		// Align right no linefeed
		internal static readonly byte TKN_AR		= (byte) 5;		// Align left no linefeed
		internal static readonly byte TKN_AC		= (byte) 6;		// Align center no linefeed
		internal static readonly byte TKN_NULL		= (byte) 0;
	
		internal int lineCount 				= 0;
		internal bool endLf					= false;
		internal bool addSpacer				= true;
		internal bool trimString			= false;
		internal int commentType			= CMT_DEFAULT;
		internal byte lfToken				= TKN_ALF;
	
		internal string text;
		internal ArrayList oList 			= new ArrayList();


		internal Comment( ) 
		{		
		}
	
		internal Comment( string text )
		{
			this.text = text;
			ParseComment();		
		}


		internal void ParseComment()
		{
			// Get off the last token to see for spacer suppressing
			string text	= this.text;
		
			int mpos	= text.IndexOf("@g");
			if ( mpos >= 0 && mpos == (text.Length - 2) ) 
			{
				addSpacer 	= false;
				trimString	= true;
				text 		= text.Substring( 0,  (text.Length - 2) - (0));
			}
			else 
			{
				mpos	= text.IndexOf("@G");
				if ( mpos >= 0 && mpos == (text.Length - 2) ) 
				{
					addSpacer 	= false;
					trimString	= false;
					text 		= text.Substring( 0, (text.Length - 2) - (0));
				}
			}
		
			// @l and \n are the same
			Byte tkn;
			int lastPos	= 0;
			mpos 		= text.IndexOf("@");
			int lfpos	= text.IndexOf("\n");
			if ( mpos == text.Length ) mpos = -1;
			if ( lfpos == text.Length ) lfpos = -1;
	
			while ( mpos >= 0 || lfpos >= 0 )
			{
				if ( mpos >= 0 && lfpos >= 0 ) 
				{
					if ( mpos < lfpos ) 
					{
						tkn = GetToken( text[mpos + 1] );
						if ( tkn != TKN_NULL ) 
						{
							oList.Add( text.Substring(lastPos, (mpos) - (lastPos)) );
							oList.Add( tkn );
							lastPos = mpos + 2;
							mpos	= text.IndexOf("@", lastPos);
						}
						else 
						{
							mpos	= text.IndexOf("@", mpos + 1);
						}
					}
					else 
					{
						oList.Add( text.Substring(lastPos, (lfpos) - (lastPos)) );
						oList.Add( lfToken );
						endLf = true;
						lineCount++;
						lastPos = lfpos + 1;
						lfpos	= text.IndexOf("\n", lastPos); 
					}
				}
				else if ( mpos >= 0 ) 
				{
					tkn = GetToken( text[mpos + 1] );
					if ( tkn != TKN_NULL ) 
					{
						oList.Add( text.Substring(lastPos, (mpos) - (lastPos)) );
						oList.Add( tkn );
						lastPos = mpos + 2;
						mpos	= text.IndexOf("@", lastPos);
					}
					else
						mpos	= text.IndexOf("@", mpos + 1);
				}
				else 
				{
					oList.Add( text.Substring(lastPos, (lfpos) - (lastPos)) );
					oList.Add( lfToken );
					endLf = true;
					lineCount++;
					lastPos = lfpos + 1;
					lfpos	= text.IndexOf("\n", lastPos); 
				}
		
				// Check if the 'next token', isn't at end of string
				if ( mpos == text.Length ) mpos = -1;
				if ( lfpos == text.Length ) lfpos = -1;
			}
	
			// Add last part of the string if necessary
			if ( lastPos < text.Length )
			{
				oList.Add( text.Substring(lastPos) );
				oList.Add( TKN_NULL );
			}
		}
	
		internal byte GetToken( char tokenChar )
		{
			switch ( tokenChar )
			{
				case 'l':
					lineCount++;
					endLf = true;
					return TKN_ALF;
				case 'L':
					return TKN_AL;
				case 'r':
					lineCount++;
					endLf = true;
					return TKN_ARF;
				case 'R':
					return TKN_AR;
				case 'c':
					lineCount++;
					endLf = true;
					return TKN_ACF;
				case 'C':
					return TKN_AC;
				default:
					return TKN_NULL;
			}
		}
	
		internal bool CompleteLine
		{
			get
			{
				return endLf;
			}
		}
	
		internal ArrayList Tokens
		{
			get
			{
				return oList;
			}
		}
	
		internal int LineCount
		{
			get
			{
				return lineCount;
			}
		}
	
		internal bool AddSpacer() 
		{
			return addSpacer;
		}
	
		internal bool TrimString() 
		{
			return trimString;
		}

		internal string Text
		{
			get
			{
				return text;
			}
		}

	}
}