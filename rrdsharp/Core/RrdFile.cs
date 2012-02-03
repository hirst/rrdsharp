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
using System.IO;
using System.Text;
using System.Threading;

namespace RrdSharp.Core
{
	/// <summary>
	/// Class to represent RRD file on the disk.
	/// </summary>
	public class RrdFile 
	{
		internal static readonly int MODE_NORMAL	= 0;
		internal static readonly int MODE_RESTORE	= 1;
		internal static readonly int MODE_CREATE	= 2;

		internal static readonly FileAccess ACCESS_READ_WRITE = FileAccess.ReadWrite;
		internal static readonly FileAccess ACCESS_READ_ONLY  = FileAccess.Read;

		static readonly int LOCK_DELAY = 100; // 0.1 sec
		static int lockMode = RrdDb.NO_LOCKS;

		private FileStream fs;
		private BinaryWriter fileOut;
		private BinaryReader fileIn;

		private bool safeMode = true;
		private string filePath;
		private bool fileLock = false;

		private int rrdMode;
		private long nextPointer = 0L;


		internal RrdFile(string filePath, int rrdMode, bool readOnly)
		{
			this.filePath = filePath;
			this.rrdMode = rrdMode;
			this.fs = new FileStream(filePath, FileMode.OpenOrCreate, (readOnly? ACCESS_READ_ONLY : ACCESS_READ_WRITE));
			this.fileOut = new BinaryWriter(fs, Encoding.Unicode);
			this.fileIn = new BinaryReader(fs, Encoding.Unicode);
			LockFile();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Close()
		{
			UnlockFile();
			if (fs != null)
			{
				fs.Close();
				fileOut.Close();
				fileIn.Close();
			}
		}

		private void LockFile()
		{
			if(lockMode == RrdDb.WAIT_IF_LOCKED || lockMode == RrdDb.EXCEPTION_IF_LOCKED) 
			{
				do {	
					try
					{
						fs.Lock(0,fs.Length);
						fileLock = true;
					}
					catch (IOException)
					{
					}

					// could not obtain lock
					if(lockMode == RrdDb.WAIT_IF_LOCKED)
					{
						// wait a little, than try again
						Thread.Sleep(LOCK_DELAY);
					}
					else
					{
						throw new IOException("Access denied. " +
							"File [" + filePath + "] already locked");
					}
					
				} while(fileLock == false);
			}
		}
		
		private void UnlockFile()
        {
			if(fileLock != false) 
			{
				fs.Unlock(0,fs.Length);
				fileLock = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		~RrdFile()
		{
			Close();
		}


		internal void TruncateFile()
		{
			fs.SetLength(nextPointer);
		}


		internal bool IsEndReached()
		{
			return nextPointer == fs.Length;
		}


		internal void Allocate(RrdPrimitive primitive, int byteCount)
		{
			primitive.Pointer = nextPointer;
			primitive.ByteCount = byteCount;
			nextPointer += byteCount;
		}

		internal void Seek(long pointer)
		{
			fs.Seek(pointer, SeekOrigin.Begin);
		}

		internal int Read(ref byte[] data)
		{
			int bytesRead = 0;
			fileIn.ReadBytes(data.Length);
			bytesRead = data.Length;

			return bytesRead; 
		}

		internal int ReadInt()
		{
			int result = fileIn.ReadInt32();
			fileIn.Read();
			return result;
		}

		internal long ReadLong()
		{
			long result = (long)fileIn.ReadInt64();
			return result;
		}

		internal double[] ReadDouble(int count)
		{
			double[] results = new double[count];
			try
			{
				for (int i = 0; i < count; i++)
				{
					results[i] = (double)fileIn.ReadDouble();
				}
			}
			catch (IOException)
			{
				throw new IOException("End of file reached");
			}
			return results;
		}

		internal double ReadDouble()
		{
			double result = (double)fileIn.ReadDouble();
			return result;
		}

		internal char ReadChar()
		{
			char result = (char)fileIn.ReadChar();
			return result;
		}

		internal void Write(byte[] data)
		{
			fileOut.Write(data);
		}

		internal void WriteInt(int data)
		{
			fileOut.Write(data);
		}

		internal void WriteLong(long data)
		{
			fileOut.Write(data);
		}

		internal void WriteDouble(double data)
		{
			fileOut.Write(data);
		}

		internal void WriteDouble(double val, int count)
		{
			for (int i = 0; i < count; i++)
			{
				fileOut.Write(val);
			}
		}

		internal void WriteChar(char data)
		{
			fileOut.Write(data);
		}
		
		internal bool SafeMode 
		{
			get
			{
				return safeMode;
			}
			set
			{
				this.safeMode = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string FilePath
		{
			get
			{
				return filePath;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long FileSize
		{
			get
			{
				return fs.Length;
			}
		}

		internal static int LockMode
		{
			get
			{
				return lockMode;
			}
			set
			{
				RrdFile.lockMode = value;
			}
		}

		internal int RrdMode
		{
			get
			{
				return rrdMode;
			}
			set
			{
				this.rrdMode = value;
			}
		}

		
	}
}