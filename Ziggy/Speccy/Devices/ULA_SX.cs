using SpeccyCommon;
using System.Collections.Generic;

namespace Speccy
{
	/// <summary>
	/// ULA_SX (simple extension) is a different take on the ULA-X concept. It also works
	/// (when enabled) by repurpusing the BRIGHT and FLASH bits in a different fashion like
	/// the ULA-X but, unlike the ULA-X, it doesn't use them as "bank index", it uses them to
	/// extend the foreground (INK) to 5 bits. While the ULA-X scheme has more "base" colors,
	/// 64 (4*(8+8)), compared to 40(32+8), it has less combinations, 64(8*8) vs 256(32*8)
	/// because you can't mix two banks in the same block.
	/// It also is, imho, easier on the graphics artist (feel free to disagree) as the collor
	/// palette is "constant", you don't have to worry about two colors being in the same bank,
	/// you can use any foreground/background combination in any block.
	/// All colors are user definable in a RGB888 color space.
	/// There's no "special" provisions for anything other than 8x8 blocks, ie, it uses the
	/// exact same memory space as the normal ZX would, and interaction with "rainbow engines"
	/// (if enabled) is unknown.
	/// Do remember that "ULA-SX mode" needs to be explicitly enabled so, it will have no impact
	/// on "normal" ZX use. Only programs that WANT to be in "ULA-SX mode" will be so.
	/// </summary>

	//NOTE: a potential "derivative" would be a ULA-NSX, not so simple expansion, that would simply
	//let you define how many bits to allocate to foreground/background. That would allow more
	//freedom as you'd have 128+2,64+4,32+8,16+16,etc, color modes to choose from. By all means
	//knock yourself out and write the ULA-NSX ;)
	public class ULA_SX : IODevice
    {
		private Queue<byte> outqueue = new Queue<byte>();
		public bool Responded { get; set; }

        // Following values taken from generic colour palette from ULA plus site
        public int[] PaletteInk = new int[32]
		{
			0x000000,0x0000d8,0xd80000,0xd800d8,0x00d800,0x00d8d8,0xd8d800,0xd8d8d8,
			0x000000,0x0000ff,0xff0000,0xff00ff,0x00ff00,0x00ffff,0xffff00,0xffffff,
			0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,
			0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,0xffffff,
		};
		public int[] PalettePaper = new int[8]
		{
			0x000000,0x0000d8,0xd80000,0xd800d8,0x00d800,0x00d8d8,0xd8d800,0xd8d8d8,
			//0x000000,0x0000ff,0xff0000,0xff00ff,0x00ff00,0x00ffff,0xffff00,0xffffff,
		};

		public bool Enabled { get; private set; } = false;
        
        public SPECTRUM_DEVICE DeviceID { get { return SPECTRUM_DEVICE.ULA_SX; } }

        public byte In(ushort port)
		{
            byte result = 0xff;
            Responded = false;
            if (port == 0xff3b)
			{
                Responded = true;
            }
            return result;
        }

        public void Out(ushort port, byte val)
		{
            Responded = false;
			if (port == 0xbf3b) //48955 dataport
			{
				outqueue.Enqueue(val);
				Responded = true;
			}
			else if (port == 0xff3b) //65339
			{
				switch (val)
				{
					case 0:
						Enabled = false;
						break;
					case 1:
						Enabled = true;
						break;
					case 2:
						ChangeInkPalette();
						break;
					case 3:
						ChangePaperPalette();
						break;
				}
				Responded = true;
			}
		}
		private void ChangeInkPalette()
		{
			if(outqueue.Count<4) return;
			byte index = outqueue.Dequeue();
			byte r= outqueue.Dequeue();
			byte g= outqueue.Dequeue();
			byte b= outqueue.Dequeue();
			if (index > 31) { return; } //Ink is 0-31
			PaletteInk[index] = r * (256 * 256) + g * 256 + b;
		}
		private void ChangePaperPalette()
		{
			if (outqueue.Count < 4) return;
			byte index = outqueue.Dequeue();
			byte r = outqueue.Dequeue();
			byte g = outqueue.Dequeue();
			byte b = outqueue.Dequeue();
			if (index > 7) { return; } //Paper is 0-7
			PalettePaper[index] = r * (256 * 256) + g * 256 + b;
		}
		public void RegisterDevice(zx_spectrum speccyModel)
		{
            speccyModel.io_devices.Remove(this);
            speccyModel.io_devices.Add(this);
        }

        public void Reset()
		{
            Enabled = false;
        }

        public void UnregisterDevice(zx_spectrum speccyModel)
		{
            speccyModel.io_devices.Remove(this);
            Enabled = false;
        }
    }
}
