using EdgeMaster.Enums;
using EdgeMaster.Interfaces;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using Speccy;
using SpeccyCommon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeMaster.Core
{
	/// <summary>
	/// The EdgeMaster device simulates a device connected to the edge expansion port of the ZX Spectrum
	/// It's a "toy device" where functions could be added as needed, and expansion is virtually unlimited
	/// In real life it could be implemented with an MCU attached to the bus, or, more likelly, with
	/// a CPLD mediating bus/MCU communication.
	/// 
	/// The baseline concept is that by having a simple form of RPC based on a simple stream of data in/out
	/// of the device and a command and control mechanism, we can infinitelly expand the base machine with
	/// functions it itself could not perform (as fast, as well)
	///
	/// The Edgemaster works with two ports, 63 and 191, both being "readwrite"
	/// Port 63 is the "data port", used to push/pull data to/from the EdgeMaster
	/// Port 191 is the Command port when written to and the Status port when read from
	///  
	/// Sending command 0 to port 191 ALWAYS resets the EdgeMaster to its default state
	///  
	/// Some EdgeMaster actions are sync, and WILL hold the bus. In that case, such actions
	/// would be deemed to be fast enough that holding the bus for a few cycles would be
	/// better than the alternative. In that case, while the EdgeMaster DOES set the Running flag,
	/// it will clear it before it returns.
	///  
	/// Some EdgeMaster actions are async, the EdgeMaster will launch a Task to execute them
	/// and the Running flag will be set until the Task ends. 
	///  
	/// For the sake of simplicity, i decided againt using any kind of synchronization. Thus, it is a
	/// REQUIREMENT that the ZX code waits for the EM to finish its current action and reads the output
	/// data before a new action is started.
	///  
	/// While we do keep a reference to the zx host, the EM should NOT use any form of direct access as
	/// a means of simulating bus mastering/DMA for anything but showcasing, never general use.
	/// 
	/// p.s. Yes, ZX Basic (Boriel's Basic) is a must, unless you directly write z80 code or don't mind
	/// "Slow as Turtle" Basic, aka Sinclair Basic. The speed difference is so stark its... mind bending.
	/// Also, actions might depend on pushing UByte/ULong/Float/etc values, and while there's possibly
	/// a way to do it from Sinclair Basic, I'm pretty sure it won't be a "simple" one.
	/// 
	/// </summary>
	public partial class EdgeMaster : IODevice, IEMDevice
	{
		//Ports that the EdgeMaster responds to
		private const int dataport = 63;
		private const int commandport = 191;

		//We keep a refence to the zx so we can do "non trivial PIO" operations, ie, interact with the machine
		//itself. NOT for "general use", this is here just so we could tentativelly infer what we could do if
		//we had proper bus mastering/dma and/or running from interrupt code
		private zx_spectrum host = null;

		//State of the EM device. Since we don't have control over the bus in a realistic way, this works
		//as a PIO polling mechanism. ZX code MUST poll the command port after a command is called and wait
		//until the EM signals EdgeMasterStateMachine.None (0), meaning the EM is finished processing
		private EdgeMasterStateMachine state = EdgeMasterStateMachine.Running;

		//EM's private RNG, both serves as a real RNG and as a source of "floating bus" values
		private Random _rng = new Random(DateTime.UtcNow.Millisecond);
		public Random Rng { get { return _rng; } }
		private byte[] rngbuffer = new byte[1024];
		private int rngcounter = 0;

		//The data in/out buffers. Queues are perfect here because, much like
		//it would in real hardware, one side pushes, the other side pulls.
		private Queue<byte> outqueue = new Queue<byte>();
		private Queue<byte> inqueue = new Queue<byte>();

		//Loaded libraries, needed for Reset() events
		List<IEMLibrary> libraries = new List<IEMLibrary>();

		//The action virtual table. Mind you that all actions but 0-2 can be redefined as the EM is running
		//altough its highly doubtful any program would even need more than 256 extra "commands"
		private Action[] vtable=new Action[256];

		//The action dictionary, filled in by the libraries as they load. All action in the virtual table map
		//to an action in this dictionary
		private Dictionary<string, Action> availableactions = new Dictionary<string, Action>();

		public EdgeMaster()
		{
			//Initialize rng buffer
			_rng.NextBytes(rngbuffer);
			//Initialize function vtable
			InitializeVTable();
		}
		private void InitializeVTable()
		{
			//Function 0 is ALWAYS "Reset" and can NOT be changed
			//Function 1 is ALWAYS "Register action" and can NOT be changed
			//Function 2 is ALWAYS "Load library" and can NOT be changed

			//All other actions start as null and need to be registered before they are used
			
			//All EM internal libraries will self-register, no need to use "Load library" for them
			//All external libraries, whichever way they are implemented, use "Load library"
			//to register themselves

			//Add standard immutable actions
			vtable[0] = Reset;
			availableactions.Add("Reset", Reset);
			vtable[1] = RegisterAction;
			availableactions.Clear();
			availableactions.Add("RegisterAction", RegisterAction);
			vtable[2] = LoadLibrary;
			availableactions.Add("LoadLibrary", LoadLibrary);
			
			//Instantiate the EM's internal actions library and let it register itself
			IEMLibrary emintlib = new EMInternalLib();
			libraries.Add(emintlib);
			emintlib.RegisterEM(this);
		}

		private void LoadLibrary()
		{
			this.SetRunning();
			Task.Run(() =>
			{
				//TODO: Implement a "standard" way to use Assembly.LoadFile() to load dlls containing a class 
				//conforming to IEMLibrary
				this.SetNotRunning();
			});
		}

		private byte GetRNGByte()
		{
			if (rngcounter < 0)
			{
				//Refill buffer
				_rng.NextBytes(rngbuffer);
				rngcounter = 1023;
			}
			rngcounter--;
			return rngbuffer[rngcounter + 1];
		}
		#region IODevice section
		public SPECTRUM_DEVICE DeviceID { get { return SPECTRUM_DEVICE.EDGEMASTER; } }

		public bool Responded { get; set; }

		public byte In(ushort port)
		{
			byte result = 0xff;
			Responded = false;
			switch (port)
			{
				case dataport: //Data port
					//If we're reading from an empty buffer, either we've read past the response data,
					//or the ZX is reading while the EdgeMaster is running but has not produced any data yet
					if (inqueue.Count == 0)
					{
						//Set the ReadPastData flag
						state |= EdgeMasterStateMachine.ReadFromEmptyBuffer;
						//Return "garbage", aka floating bus values
						result = GetRNGByte();
						Responded=true;
					}
					else
					{
						//Clear the ReadPastData flag
						state &= ~EdgeMasterStateMachine.ReadFromEmptyBuffer;
						result=inqueue.Dequeue();
						Responded = true;
					}
					break;
				case commandport: //Command port
					result = (byte)state;
					Responded = true;
					break;
			}
			return result;
		}
		public void Out(ushort port, byte val)
		{
			//63 : Data port. Used to buffer actions and data to process
			//191 : Command port. Used to trigger action
			Responded = false;
			switch (port)
			{
				case dataport:
					outqueue.Enqueue(val);
					Responded = true;
					break;
				case commandport:
					ExecuteAction(val);
					Responded = true;
					break;
			}
		}
		public void Reset()
		{
			//Reset the data buffers
			outqueue = new Queue<byte>();
			inqueue = new Queue<byte>();
			//Reset the state machine
			state = EdgeMasterStateMachine.None;
			//Send Reset() to all libraries, should they need to free resources
			foreach (IEMLibrary lib in libraries)
			{
				lib.Reset();
			}
			//Clear the library list
			libraries.Clear();
			//Initialize vtable
			InitializeVTable();
		}

		public void RegisterDevice(zx_spectrum hostmachine)
		{
			hostmachine.io_devices.Remove(this);
			hostmachine.io_devices.Add(this);
			host = hostmachine;
		}

		public void UnregisterDevice(zx_spectrum hostmachine)
		{
			host = null;
			hostmachine.io_devices.Remove(this);
		}
		#endregion
		private void RegisterAction()
		{
			state |= EdgeMasterStateMachine.Running;
			byte actionnumber = ReadByteFromOutQueue();
			if (actionnumber < 3)
			{
				//Error in ZX code, flush queue
				outqueue.Clear();
			}
			else
			{
				string actionid = ReadStringFromOutQueue();
				if (availableactions.ContainsKey(actionid))
				{
					vtable[actionnumber] = availableactions[actionid];
				}
			}
			state &= ~EdgeMasterStateMachine.Running;
		}
		public void ExecuteAction(byte actionnumber)
		{
			//Process the action
			if (vtable[actionnumber] != null)
			{
				vtable[actionnumber].Invoke();
			}
		}
		public bool AddAction(string actionid, Action action)
		{
			if (!availableactions.ContainsKey(actionid))
			{
				availableactions.Add(actionid, action);
				return true;
			}
			return false;
		}
		public byte ReadByteFromOutQueue()
		{
			//Get first byte from buffer
			byte outbyte = outqueue.Dequeue();
			return outbyte;
		}
		public void WriteByteToInQueue(byte bytevalue)
		{
			inqueue.Enqueue(bytevalue);
		}
		public byte[] ReadBytesFromOutQueue(int numbytes)
		{
			byte[] bytes = new byte[numbytes];
			for (int c = 0; c < numbytes; c++)
			{
				bytes[c] = outqueue.Dequeue();
			}
			return bytes;
		}
		public uint ReadULongFromOutQueue()
		{
			byte[] bytes = ReadBytesFromOutQueue(4);
			return (uint)(bytes[0]+ bytes[1] * 256+ bytes[2] * 256*256+ bytes[3] * 256 * 256 * 256);
		}
		public void WriteULongToInQueue(UInt32 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytesToInQueue(bytes);
		}
		public void WriteBytesToInQueue(byte[] bytevalues)
		{
			for (int c = 0; c < bytevalues.Length; c++)
			{
				inqueue.Enqueue(bytevalues[c]);
			}
		}
		public string ReadStringFromOutQueue()
		{
			List<byte> listbytes = new List<byte>();
			byte byteread;
			//Read from buffer until we get a 0
			do
			{
				byteread = ReadByteFromOutQueue();
				if (byteread != 0)
				{
					listbytes.Add(byteread);
				}
			}
			while (byteread != 0);
			byte[] arraybytes =listbytes.ToArray();
			string asciistring = Encoding.ASCII.GetString(arraybytes);
			return asciistring;
		}
		public void WriteStringToInQueue(string content)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(content);
			byte[] asciiArray = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);

			for (int c = 0; c < asciiArray.Length; c++)
			{
				inqueue.Enqueue(asciiArray[c]);
			}
			//Zero terminate
			inqueue.Enqueue(0);
		}
		public void SetRunning()
		{
			state |= EdgeMasterStateMachine.Running;
		}
		public void SetNotRunning()
		{
			state &= ~EdgeMasterStateMachine.Running;
		}
		public Random GetRandom()
		{
			return _rng;
		}
		public double ReadZXFloatFromOutQueue()
		{
			double retval = 0.0;
			byte[] bytes = this.ReadBytesFromOutQueue(5);
			retval=ZXUtility.ZXFloatToDouble(bytes);
			return retval;
		}
		public void WriteZXFloatToInQueue(double value)
		{
			byte[] bytes=ZXUtility.DoubleToZXFloat(Math.Round(value,8));
			WriteBytesToInQueue(bytes);
		}
		public void PokeByteDMA(ushort addr, byte val)
		{
			host.PokeByte(addr, val);
		}
		public byte PeekByteDMA(ushort addr)
		{
			return host.PeekByte(addr);
		}

		public int GetTStates()
		{
			return host.cpu.t_states;
		}
	}
	/// <summary>
	/// 16 bit precalculated bitmasks for a 16bit value with a single bit set
	/// </summary>
	[Flags]
	public enum Bit16
	{
		Bit0 = 0x001,
		Bit1 = 1 << 1,
		Bit2 = 1 << 2,
		Bit3 = 1 << 3,
		Bit4 = 1 << 4,
		Bit5 = 1 << 5,
		Bit6 = 1 << 6,
		Bit7 = 1 << 7,
		Bit8 = 1 << 8,
		Bit9 = 1 << 9,
		Bit10 = 1 << 10,
		Bit11 = 1 << 11,
		Bit12 = 1 << 12,
		Bit13 = 1 << 13,
		Bit14 = 1 << 14,
		Bit15 = 1 << 15
	};
	/// <summary>
	/// "Helper" constants and methods that needed but wanted to keep OUT of EdgeMaster itself
	/// aka, the trashcan of development (as all helper/utility classes end up being...)
	/// </summary>
	public static class ZXUtility
	{
		//ZX memory constants
		public const int MemoryScreenStart = 16384;
		public const int MemoryScreenEnd = 22528;
		public const int MemoryScreenLength = 6144;
		//ZX "utility constants"
		public const int ZXResolutionWidth = 256;
		public const int ZXResolutionHeight = 192;
		//ZX "safe" Tstate to begin doing Quasi-DMA
		public const int ZXMaxTStateForDMA = 10000;
		//ZX memory "line offsets", allow calculating any 8x1 "pixel block positions" by simply
		//doing MemoryScreenStart+PreCalculatedScreenMemoryOffsets[y]+(xoffset/8)
		//Mind you that drawing to the screen using direct memory access would require you to do
		//both a read and a write to that block if you wanted to change a single pixel.
		//Highly inneficient without coprocessors helping, and one of the reasons bitplanes totally
		//fell out of "grace" once you needed 256 or more colors as changing 8 bitplanes was much
		//more expensive than writing a single byte value
		public static readonly int[] PreCalculatedScreenMemoryOffsets;
		//"utility constants"
		public const float RadPerDegree = (float)(Math.PI / 180.0);
		static ZXUtility()
		{
			PreCalculatedScreenMemoryOffsets = new int[192]
			{
				0000, 0256, 0512, 0768, 1024, 1280, 1536, 1792, 0032, 0288, 0544, 0800, 1056, 1312, 1568, 1824,
				0064, 0320, 0576, 0832, 1088, 1344, 1600, 1856, 0096, 0352, 0608, 0864, 1120, 1376, 1632, 1888,
				0128, 0384, 0640, 0896, 1152, 1408, 1664, 1920, 0160, 0416, 0672, 0928, 1184, 1440, 1696, 1952,
				0192, 0448, 0704, 0960, 1216, 1472, 1728, 1984, 0224, 0480, 0736, 0992, 1248, 1504, 1760, 2016,
				2048, 2304, 2560, 2816, 3072, 3328, 3584, 3840, 2080, 2336, 2592, 2848, 3104, 3360, 3616, 3872,
				2112, 2368, 2624, 2880, 3136, 3392, 3648, 3904, 2144, 2400, 2656, 2912, 3168, 3424, 3680, 3936,
				2176, 2432, 2688, 2944, 3200, 3456, 3712, 3968, 2208, 2464, 2720, 2976, 3232, 3488, 3744, 4000,
				2240, 2496, 2752, 3008, 3264, 3520, 3776, 4032, 2272, 2528, 2784, 3040, 3296, 3552, 3808, 4064,
				4096, 4352, 4608, 4864, 5120, 5376, 5632, 5888, 4128, 4384, 4640, 4896, 5152, 5408, 5664, 5920,
				4160, 4416, 4672, 4928, 5184, 5440, 5696, 5952, 4192, 4448, 4704, 4960, 5216, 5472, 5728, 5984,
				4224, 4480, 4736, 4992, 5248, 5504, 5760, 6016, 4256, 4512, 4768, 5024, 5280, 5536, 5792, 6048,
				4288, 4544, 4800, 5056, 5312, 5568, 5824, 6080, 4320, 4576, 4832, 5088, 5344, 5600, 5856, 6112
			};
		}
		public static int GetScreenMemoryLineOffsetFromBase(int y)
		{
			/*
			Byte 0	Byte 1
			15	14	13	12	11	10	9	8	7	6	5	4	3	2	1	0
			0	1	0	Y7	Y6	Y2	Y1	Y0	Y5	Y4	Y3	0	0	0	0	0
			*/
			int offset = 0b1111111111111111;

			int mask210 = (int)(Bit16.Bit2 | Bit16.Bit1 | Bit16.Bit0);
			int mask543 = (int)(Bit16.Bit5 | Bit16.Bit4 | Bit16.Bit3);
			int mask76 = (int)(Bit16.Bit7 | Bit16.Bit6);

			int bits210 = (y & mask210) << 8;
			int bits543 = (y & mask543) << 2;
			int bits76 = (y & mask76) << 5;

			offset &= (bits210 | bits543 | bits76 | (int)Bit16.Bit14);
			offset -= 16384;

			return offset;
		}
		public static bool IsBitSet(byte b, int pos)
		{
			return (b & (1 << pos)) != 0;
		}
		public static void DrawLineIndexedColor(byte x, byte y, byte x2, byte y2, byte color, byte[,] array)
		{
			int w = x2 - x;
			int h = y2 - y;
			int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
			if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
			if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
			if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
			int longest = Math.Abs(w);
			int shortest = Math.Abs(h);
			if (longest <= shortest)
			{
				longest = Math.Abs(h);
				shortest = Math.Abs(w);
				if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
				dx2 = 0;
			}
			int numerator = longest >> 1;
			for (int i = 0; i <= longest; i++)
			{
				array[x, y] = color;
				numerator += shortest;
				if (numerator >= longest)
				{
					numerator -= longest;
					x = (byte)(x + dx1);
					y = (byte)(y + dy1);
				}
				else
				{
					x = (byte)(x + dx2);
					y = (byte)(y + dy2);
				}
			}
		}
		public static void ListAdd(List<byte> bytelist, int x, int y)
		{
			bytelist.Add((byte)(x));
			bytelist.Add((byte)(y));
		}
		public static byte[] MidpointCircleAlgorithm(int screenwidth, int screenheight, int centerx, int centery, int radius)
		{
			List<byte> pointlist = new List<byte>();
			List<Tuple<int, int>> mpcapoints = new List<Tuple<int, int>>();
			int x = 0;
			int y = radius;
			float decisionParameter = 1 - radius;

			PlotMPCA(x, y, centerx, centery); // Plot the initial point

			while (x < y)
			{
				x++;

				if (decisionParameter < 0)
				{
					decisionParameter += 2 * x + 1;
				}
				else
				{
					y--;
					decisionParameter += 2 * (x - y) + 1;
				}

				PlotMPCA(x, y, centerx, centery);
			}
			foreach (Tuple<int, int> kvp in mpcapoints)
			{
				if (kvp.Item1 >= 0 && kvp.Item1 <= screenwidth - 1 && kvp.Item2 >= 0 && kvp.Item2 <= screenheight - 1)
				{
					pointlist.Add((byte)kvp.Item1);
					pointlist.Add((byte)kvp.Item2);
				}
			}
			return pointlist.ToArray();
			void PlotMPCA(int xt, int yt, int cxt, int cyt)
			{
				// Example using a list to store coordinates.  Replace with your drawing logic
				mpcapoints.Add(new Tuple<int, int>(cxt + xt, cyt + yt));
				mpcapoints.Add(new Tuple<int, int>(cxt - xt, cyt + yt));
				mpcapoints.Add(new Tuple<int, int>(cxt + xt, cyt - yt));
				mpcapoints.Add(new Tuple<int, int>(cxt - xt, cyt - yt));
				mpcapoints.Add(new Tuple<int, int>(cxt + yt, cyt + xt));
				mpcapoints.Add(new Tuple<int, int>(cxt - yt, cyt + xt));
				mpcapoints.Add(new Tuple<int, int>(cxt + yt, cyt - xt));
				mpcapoints.Add(new Tuple<int, int>(cxt - yt, cyt - xt));
			}
		}

		public static byte[] BitmapToByteArray(Bitmap bitmap)
		{
			BitmapData bmpdata = null;
			try
			{
				bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
				int numbytes = (bmpdata.Stride * bitmap.Height);
				byte[] bytedata = new byte[numbytes];
				IntPtr ptr = bmpdata.Scan0;
				Marshal.Copy(ptr, bytedata, 0, numbytes);
				return bytedata;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				if (bmpdata != null) { bitmap.UnlockBits(bmpdata); }
			}
			return null;
		}
		
		public static byte[] StringToByteArray(string str, Encoding enc)
		{
			if (enc == Encoding.ASCII)
			{
				return Encoding.ASCII.GetBytes(str);
			}
			if (enc == Encoding.UTF8)
			{
				return Encoding.UTF8.GetBytes(str);
			}
			return null;
		}
		public static byte[] ByteArrayToScreenMemoryArray(byte[] input)
		{
			byte[] retval = new byte[input.Length];
			int coffset = 0;
			for (int y = 0; y < 192; y++)
			{
				for (int c = 0; c < 32; c++)
				{
					retval[ZXUtility.PreCalculatedScreenMemoryOffsets[y] + c] = input[coffset + c];
				}
				coffset += 32;
			}
			return retval;
		}
		public static List<byte> ByteArrayToQuarterScreenMemoryArray(byte[] input)
		{
			//Quarter resolution with 2 extra bytes per line, which will be the memory offset for the next 16bytes
			List<byte> retval = new List<byte>();
			for (int y = 48; y < 48 + 96; y++)
			{
				//Add the memory offset, which is row offset plus 12bytes
				UInt16 rowoffset = (UInt16)(ZXUtility.PreCalculatedScreenMemoryOffsets[y] + 8);
				byte lowbyte = (byte)(rowoffset & 0x00FF);
				byte highbyte = (byte)((rowoffset & 0xFF00) >> 8);
				retval.Add(lowbyte);
				retval.Add(highbyte);
				for (int c = 8; c < 24; c++)
				{
					retval.Add(input[c - 8]);
				}
			}
			return retval;
		}
		public static byte[] CompressGZIP(byte[] bytes)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
				{
					gzipStream.Write(bytes, 0, bytes.Length);
				}
				return memoryStream.ToArray();
			}
		}
		public static byte[] DecompressGZIP(byte[] bytes)
		{
			using (var memoryStream = new MemoryStream(bytes))
			{
				using (var outputStream = new MemoryStream())
				{
					using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
					{
						decompressStream.CopyTo(outputStream);
					}
					return outputStream.ToArray();
				}
			}
		}
		public static float BBFixedToFloat(byte[] bytes)
		{
			float retval;
			Int16 ip;
			float fp;
			//Fractional part
			fp = (float)((bytes[0] + bytes[1] * 256) / 65536.0f);
			//Check sign
			if (bytes[3] < 128)
			{
				ip = (Int16)(bytes[2] + bytes[3] * 256);
			}
			else
			{
				ip = (Int16)(BitConverter.ToInt16(bytes, 2));
			}
			if (bytes[0] == 0 && bytes[1] == 0)
			{
				retval = ip;
			}
			else
			{
				retval = fp + ip;
			}	
			return retval;
		}
		public static byte[] FloatToBBFixed(float dval)
		{
			byte[] retavl;
			//get ip and fp parts
			Int16 ip = (Int16)dval;
			float tempfp= (float)(dval-ip);
			UInt16 fp;
			fp = (UInt16)((dval - ip) * 65536.0f);
			if (dval < 0 && fp != 0)
			{
				ip = (Int16)(ip - 1);
			}
			//get bytes
			byte[] ipar = BitConverter.GetBytes(ip);
			byte[] fpar = BitConverter.GetBytes(fp);
			retavl = new byte[4] { fpar[0], fpar[1], ipar[0], ipar[1]};
			return retavl;
		}
		// Decode ZX 5-byte (or small-int) -> exact double
		public static double ZXFloatToDouble(byte[] zx)
		{
			if (zx == null || zx.Length != 5) throw new ArgumentException("ZX float must be exactly 5 bytes", nameof(zx));

			// small-int special case (first byte == 0)
			if (zx[0] == 0x00)
			{
				if (zx[1] == 0x00) // positive small int
					return (double)(zx[2] | (zx[3] << 8));
				if (zx[1] == 0xFF) // negative small int (two's complement stored)
				{
					int stored = zx[2] | (zx[3] << 8);
					return (double)(stored - 65536);
				}
				// defensive fallback
				return (double)(zx[2] | (zx[3] << 8));
			}

			bool negative = (zx[1] & 0x80) != 0;
			uint b1 = (uint)(zx[1] & 0x7F);
			uint b2 = zx[2];
			uint b3 = zx[3];
			uint b4 = zx[4];

			// N = b1<<24 | b2<<16 | b3<<8 | b4  (0 .. 2^31-1)
			uint N = (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;

			// M_num = 2^31 + N
			uint M_num = (1u << 31) + N;

			// value = M_num * 2^(zxExp - 160)
			int zxExp = zx[0];
			double value = (double)M_num * Math.Pow(2.0, zxExp - 160);

			return negative ? -value : value;
		}

		// Encode double -> ZX 5 bytes. If preferSmallInt=true, exact integer in -65536..65535 will use small-int encoding.
		public static byte[] DoubleToZXFloat(double value, bool preferSmallInt = false)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
				throw new ArgumentOutOfRangeException(nameof(value), "ZX format can't represent NaN/Infinity.");

			if (value == 0.0)
				return new byte[5]; // {0,0,0,0,0}

			// small-int encoding optional
			if (preferSmallInt)
			{
				double rounded = Math.Round(value);
				if (Math.Abs(value - rounded) < 1e-12) // exact integer within rounding tolerance
				{
					long n = (long)rounded;
					if (n >= -65536 && n <= 65535)
					{
						byte[] outBytes = new byte[5];
						outBytes[0] = 0x00;
						outBytes[1] = (byte)(n < 0 ? 0xFF : 0x00);
						ushort stored = (ushort)n; // two's complement for negative
						outBytes[2] = (byte)(stored & 0xFF);
						outBytes[3] = (byte)((stored >> 8) & 0xFF);
						outBytes[4] = 0x00;
						return outBytes;
					}
				}
			}

			bool negative = value < 0.0;
			double absVal = negative ? -value : value;

			// compute exponent so mantissa in [0.5, 1.0)
			int eUnbiased = (int)Math.Floor(Math.Log(absVal, 2.0)); // floor(log2(absVal))
			int zxExp = eUnbiased + 1 + 128;

			// mantissa = absVal / 2^(zxExp - 128)  -> should be in [0.5, 1.0)
			double mantissa = absVal / Math.Pow(2.0, zxExp - 128);

			// M_num = round(mantissa * 2^32)  (should be in [2^31 .. 2^32-1])
			double MnumD = Math.Round(mantissa * Math.Pow(2.0, 32));
			// handle rounding overflow (mantissa rounded to 1.0)
			if (MnumD >= Math.Pow(2.0, 32))
			{
				// increment exponent and use M_num = 2^31 (i.e. fractional part 0)
				zxExp++;
				MnumD = (double)(1u << 31);
			}
			if (MnumD < (double)(1u << 31)) MnumD = (double)(1u << 31); // guard

			uint Mnum = (uint)MnumD;
			uint N = Mnum - (1u << 31); // 31-bit stored mantissa

			byte[] res = new byte[5];
			res[0] = (byte)(zxExp & 0xFF);
			res[1] = (byte)(((N >> 24) & 0x7Fu) | (negative ? 0x80u : 0u));
			res[2] = (byte)((N >> 16) & 0xFFu);
			res[3] = (byte)((N >> 8) & 0xFFu);
			res[4] = (byte)(N & 0xFFu);
			return res;
		}
		public static string SanitizeFilename(string filename)
		{
			string retval = string.Empty;
			if (string.IsNullOrWhiteSpace(filename)) { return retval; }
			retval=filename;
			//No colons
			retval = retval.Replace(":", "");
			//No \\
			while (retval.Contains("\\\\"))
			{
				retval = retval.Replace("\\\\","");
			}
			//No //
			while (retval.Contains("//"))
			{
				retval = retval.Replace("//", "");
			}
			//No ..
			while (retval.Contains(".."))
			{
				retval = retval.Replace("..", "");
			}
			//No ?
			retval = retval.Replace("?", "");
			//If first char is / or \, strip it
			if (retval[0] == '\\' || retval[0] == '/')
			{
				retval=retval.Substring(1, retval.Length - 1);
			}
			//No non ASCII 'human chars', restrictive yes, but safe
			for (int c = 0; c < retval.Length; c++)
			{
				if (retval[c] < 32 || retval[c] > 126)
				{
					retval=retval.Replace(retval[c], ' ');
				}
			}
			return retval;
		}
		public static byte[] BitmapToScreenMemory(Bitmap source,int quantizer)
		{
			Bitmap scaled = source.Resize(new Size(ZXUtility.ZXResolutionWidth, ZXUtility.ZXResolutionHeight), ScalingMode.Auto, false);
			Bitmap quantized = null;
			switch (quantizer)
			{
				case 0:
					quantized = scaled.ConvertPixelFormat(PixelFormat.Format1bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.Bayer2x2);
					break;
				case 1:
					quantized = scaled.ConvertPixelFormat(PixelFormat.Format1bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.Bayer4x4);
					break;
				case 2:
					quantized = scaled.ConvertPixelFormat(PixelFormat.Format1bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.Bayer8x8);
					break;
				case 3:
					quantized = scaled.ConvertPixelFormat(PixelFormat.Format1bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), ErrorDiffusionDitherer.FloydSteinberg);
					break;
				case 4:
					quantized = scaled.ConvertPixelFormat(PixelFormat.Format1bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.DottedHalftone);
					break;
			}
			Bitmap reduced = quantized.Clone(new System.Drawing.Rectangle(0, 0, ZXUtility.ZXResolutionWidth, ZXUtility.ZXResolutionHeight), PixelFormat.Format1bppIndexed);
			byte[] bytes = ZXUtility.BitmapToByteArray(reduced);
			reduced.Dispose();
			quantized.Dispose();
			scaled.Dispose();
			source.Dispose();
			byte[] screenmemoryarray = ZXUtility.ByteArrayToScreenMemoryArray(bytes);
			return screenmemoryarray;
		}
	}
}


