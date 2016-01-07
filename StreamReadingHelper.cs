using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibSharpHelp
{
	public class StreamReadingHelper : IDisposable
	{
		#region IDisposable implementation
		public void Dispose () { str.Dispose (); }
		#endregion

		readonly BinaryReader str;
		public StreamReadingHelper (Stream with)
		{
			str=new BinaryReader(with);
		}
		class ReadInfo
		{
			public int size;
			public Func<BinaryReader, Object> reader;
		}
		ReadInfo GetReader<RT>()
		{
			var itp = typeof(RT);
			if (itp ==  typeof(int)) return new ReadInfo { size = 4, reader = str => str.ReadInt32 () };
			if (itp == typeof(uint)) return new ReadInfo { size = 4, reader = str => str.ReadUInt32 () };
			if (itp == typeof(long)) return new ReadInfo { size = 8, reader = str => str.ReadInt64 () };
			if (itp == typeof(bool)) return new ReadInfo { size = 1, reader = str => str.ReadBoolean () };
			return null;
		}
		public bool TryRead<T>(out T value) where T : struct
		{
			value = default(T);
			var info = GetReader<T> ();
			if (info == null || str.BaseStream.Length - str.BaseStream.Position < info.size)
				return false; // not enough left in stream to read!
			value = (T)Convert.ChangeType(info.reader(str), typeof(T));
			return true;
		}
		public bool TryRead<T>(Action<T> callback) where T : struct
		{
			T dat;
			if(TryRead(out dat)) {
				callback (dat);
				return true;
			}
			return false;
		}
	}
}

