using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EdgeMaster.Interfaces;

namespace EdgeMaster.Core
{
	public partial class EMInternalLib : IEMLibrary
	{
		//EM as pseudo-RAM. Setting it to 16MB in a 24b address space was a qausi arbitrary
		//limit, because i was thinking about using something "cheap" like a ESP32S3 with 16MB
		//PSRAM for a "real EM". It is also why the "mass read/write" function are only set
		//to read/write 64K blocks at most, as the ESP32S3 doesn't have that much RAM by itself.
		//If considering something like the Pi Zero 2W, this is a non issue as 512MB is plenty
		//of RAM for all and sundry.
		private byte[] emram = new byte[256 * 256 * 256];

		//FUTURE: have functions to set these "registers" so that you can read/write to the
		//pseudo-RAM  in situations where you're going for max thoughput with loop unrolling,
		//as you can preset the high/mid bytes of the address and then read/write up to 256
		//bytes by sending just one address byte instead of three on the "request".
		private int highbyte = 0;
		private int midbyte = 0;
		private int lowbyte = 0;
		private void ReadRamByte()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				byte[] addressbytes = emdevice.ReadBytesFromOutQueue(3);
				int address=addressbytes[0]*256*256+ addressbytes[1] * 256+ addressbytes[2];
				emdevice.WriteByteToInQueue(emram[address]);
				emdevice.SetNotRunning();
			});
		}
		private void WriteRamByte()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				byte[] addressbytes = emdevice.ReadBytesFromOutQueue(3);
				byte value = emdevice.ReadByteFromOutQueue();
				int address = addressbytes[0] * 256 * 256 + addressbytes[1] * 256 + addressbytes[2];
				emram[address]=value;
				emdevice.SetNotRunning();
			});
		}
		private void ReadRamBytes()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				byte[] addressbytes = emdevice.ReadBytesFromOutQueue(3);
				byte[] lengthbytes = emdevice.ReadBytesFromOutQueue(2);
				int address = addressbytes[0] * 256 * 256 + addressbytes[1] * 256 + addressbytes[2];
				int length = lengthbytes[0] * 256 + lengthbytes[1];
				for (int c = 0; c < length; c++)
				{
					emram[address+c]=emdevice.ReadByteFromOutQueue();
				}
				emdevice.SetNotRunning();
			});
		}
		private void WriteRamBytes()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				byte[] addressbytes = emdevice.ReadBytesFromOutQueue(3);
				byte[] lengthbytes = emdevice.ReadBytesFromOutQueue(2);
				int address = addressbytes[0] * 256 * 256 + addressbytes[1] * 256 + addressbytes[2];
				int length = lengthbytes[0] * 256 + lengthbytes[1];
				for (int c = 0; c < length; c++)
				{
					emdevice.WriteByteToInQueue(emram[address + c]);
				}
				emdevice.SetNotRunning();
			});
		}
	}
}

