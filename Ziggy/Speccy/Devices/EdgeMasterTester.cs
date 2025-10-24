using System;
using System.Collections.Generic;
using System.Text;
using EdgeMaster.Core;
using SpeccyCommon;

namespace Speccy
{
	/// <summary>
	/// Simple expansion device for tests, uses port 99 by default
	/// Of use only for development as you can test "junk" here
	/// without poluting the "real" EdgeMaster
	/// </summary>
	public partial class EdgeMasterTester : IODevice
	{
		//Ports that the EdgeMasterTester responds to
		private const int dataport = 99;

		//We keep a refence to the zx for developement purposes only.
		//Should NEVER be used for any purpose that even resembles
		//normal operation
		private zx_spectrum host = null;
		
		//The data in/out buffers. Queues are perfect here because, much like
		//it would in real hardware, one side pushes, the other side pulls.
		private Queue<byte> outqueue = new Queue<byte>();
		private Queue<byte> inqueue = new Queue<byte>();
		
		//RNG to supply "floating bus"
		private Random rand = new Random();
		private byte[] randbytes=new byte[8];
		
		public EdgeMasterTester()
		{
		}

		public SPECTRUM_DEVICE DeviceID { get { return SPECTRUM_DEVICE.EDGEMASTERTESTER; } }

		public bool Responded { get; set; }

		public byte In(ushort port)
		{
			byte result = 0xff;
			Responded = false;
			switch (port)
			{
				case dataport:
					//If we're reading from an empty buffer, either we've read past the response data,
					//or the zx is reading while the EdgeMaster is running but has not produced any data yet
					if (inqueue.Count == 0)
					{
						//Return "garbage", aka floating bus values
						rand.NextBytes(randbytes);
						result = randbytes[0];
						Responded=true;
					}
					else
					{
						result=inqueue.Dequeue();
						Responded = true;
					}
					break;
			}
			return result;
		}
		public void Out(ushort port, byte val)
		{
			Responded = false;
			switch (port)
			{
				case dataport:
					outqueue.Enqueue(val);
					Responded = true;
					break;
			}
		}
		
		public void Reset()
		{
			//Reset the data buffers
			outqueue = new Queue<byte>();
			inqueue = new Queue<byte>();
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
	}
}


