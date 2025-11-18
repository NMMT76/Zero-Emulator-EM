using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using EdgeMaster.Interfaces;
using Accord.Video.FFMPEG;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EdgeMaster.Core
{
	// Benchmark Actions showcasing EM's acceleration of math intensive programs
	public partial class EMInternalLib : IEMLibrary
	{
		List<byte[]> videoframes = new List<byte[]>();
		int lastvideoframe = -1;
		private void PlayVideoFileBW()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				string video = emdevice.ReadStringFromOutQueue();
				byte quant=emdevice.ReadByteFromOutQueue();
				byte frameskip=emdevice.ReadByteFromOutQueue();

				if (File.Exists(video))
				{
					VideoFileReader reader = new VideoFileReader();
					reader.Open(video);
					if (reader.FrameCount > 0)
					{
						List<byte> framebytelist = new List<byte>();
						int totalframes = 0;
						for (int i = 0; i < reader.FrameCount; i++)
						{
							Bitmap frame = reader.ReadVideoFrame();
							if (frame != null)
							{
								if (i % frameskip == 0)
								{
									totalframes++;
									byte[] framebytes = ZXUtility.BitmapToScreenMemory(frame, quant);
									for (int c = 0; c < framebytes.Length; c++)
									{
										framebytelist.Add((byte)(~framebytes[c]));
									}
								}
							}
							frame.Dispose();
						}
						emdevice.WriteULongToInQueue((UInt32)totalframes);
						emdevice.WriteBytesToInQueue(framebytelist.ToArray());
					}
					else
					{
						//Write zero length, no frames
						emdevice.WriteBytesToInQueue(new byte[] { 0, 0, 0, 0 });
					}
					reader.Close();
				}
				else
				{
					//Write zero length, no file
					emdevice.WriteBytesToInQueue(new byte[] { 0,0,0,0});
				}
				emdevice.SetNotRunning();
			});
		}
		private void LoadVideoFileBW()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				videoframes = new List<byte[]>();
				lastvideoframe = -1;

				string video = emdevice.ReadStringFromOutQueue();
				byte quant = emdevice.ReadByteFromOutQueue();
				byte invert = emdevice.ReadByteFromOutQueue();
				byte frameskip = emdevice.ReadByteFromOutQueue();

				if (File.Exists(video))
				{
					VideoFileReader reader = new VideoFileReader();
					reader.Open(video);
					if (reader.FrameCount > 0)
					{
						int totalframes = 0;
						for (int i = 0; i < reader.FrameCount; i++)
						{
							Bitmap frame = reader.ReadVideoFrame();
							if (frame != null)
							{
								if (i % frameskip == 0)
								{
									totalframes++;
									byte[] framebytes = ZXUtility.BitmapToScreenMemory(frame, quant);
									if (invert !=0)
									{
										for (int c = 0; c < framebytes.Length; c++)
										{
											framebytes[c] = (byte)(~framebytes[c]);
										}
									}
									videoframes.Add(framebytes);
								}
							}
							frame.Dispose();
						}
						//NOTE on the bellow: Tests with random video files showed that on real world cases,
						//the average transfer would float around 3K if transfering only the changed bytes,
						//meaning that with a properly curated video file this "DMA simulacrum" is good enough
						//as a showcase of what a DMA device could do IF its running from interrupt code AND
						//is taking less than the VBI period would allow it. Still not cycle accurate so, fun
						//purposes only

						//Just a "developement file" with the frame to frame delta values so we can externally
						//graph the number of bytes that actually need to be written to memory. All things being
						//equal, on average it should be WAY bellow a full frame meaning that for the most part
						//we should be well clear of the "danger zone" when it comes to holding the bus too long.
						//int cb = 0;
						//if (totalframes > 0)
						//{
						//	string deltafile = Path.Combine(Application.StartupPath, "delta.csv");
						//	Console.WriteLine(deltafile);
						//	if (File.Exists(deltafile))
						//	{
						//		File.Delete(deltafile);
						//	}
						//	for (int frame = 1; frame < videoframes.Count; frame++)
						//	{
						//		cb = 0;
						//		for (int c = 0; c < ZXUtility.MemoryScreenLength; c++)
						//		{
						//			if (videoframes[frame - 1][c] != videoframes[frame][c])
						//			{
						//				cb++;
						//			}
						//		}
						//		File.AppendAllText(deltafile, $"{cb};{System.Environment.NewLine}");
						//	}
						//}
						emdevice.WriteULongToInQueue((uint)totalframes);
					}
					else
					{
						//Write zero length, no frames
						emdevice.WriteBytesToInQueue(new byte[] { 0, 0, 0, 0 });
					}
					reader.Close();
				}
				else
				{
					//Write zero length, no file
					emdevice.WriteBytesToInQueue(new byte[] { 0, 0, 0, 0 });
				}
				emdevice.SetNotRunning();
			});
		}
		public void TransferVideoFrameBWDMA()
		{
			emdevice.SetRunning();
			if (lastvideoframe == -1) //Full frame copy, no other way, hope for best
			{
				for (int c = 0; c < ZXUtility.MemoryScreenLength; c++)
				{
					emdevice.PokeByteDMA((ushort)(ZXUtility.MemoryScreenStart + c), videoframes[0][c]);
				}
				lastvideoframe = 0;
			}
			else //Copy ONLY bytes that changed
			{
				for (int c = 0; c < ZXUtility.MemoryScreenLength; c++)
				{
					if (videoframes[lastvideoframe][c] != videoframes[lastvideoframe + 1][c])
					{
						emdevice.PokeByteDMA((ushort)(ZXUtility.MemoryScreenStart + c), videoframes[lastvideoframe+1][c]);
					}
				}
				lastvideoframe +=1;
			}
			Console.WriteLine($"Last frame sent {lastvideoframe}");
			emdevice.SetNotRunning();
		}
		private void ADDSUBMULDIV()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				//Get first float value
				double f1 = emdevice.ReadZXFloatFromOutQueue();
				//Get second float value
				double f2 = emdevice.ReadZXFloatFromOutQueue();
				//Get third float value
				double f3 = emdevice.ReadZXFloatFromOutQueue();
				//Get fourth float value
				double f4 = emdevice.ReadZXFloatFromOutQueue();
				//Get fifth float value
				double f5 = emdevice.ReadZXFloatFromOutQueue();
				//ADD SUB MUL DIV
				double addv = (((f1+f2)-f3)*f4)/f5;
				emdevice.WriteZXFloatToInQueue(addv);
				emdevice.SetNotRunning();
			});
		}
		private void AHLSBENCH()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				//Outer loop count
				uint olc=emdevice.ReadULongFromOutQueue();
				//Inner loop count
				uint ilc = emdevice.ReadULongFromOutQueue();

				double s =0;
				double r = 0;
				double a = 0;

				for (int ol = 1; ol <= olc; ol++)
				{
					a = ol;
					for (int il = 1; il <= ilc; il++)
					{
						a=Math.Sqrt(a);
						r = r + emdevice.Rng.NextDouble();
					}
					for (int il = 1; il <= ilc; il++)
					{
						a = Math.Pow(a, 2);
						r = r + emdevice.Rng.NextDouble();
					}
					s = s + a;
				}
				Console.WriteLine($"s : {Math.Abs(1010 - s / 5.0)} - r : {Math.Abs(1000 - r)}");
				emdevice.WriteZXFloatToInQueue(Math.Abs(1010-s/5.0));
				emdevice.WriteZXFloatToInQueue(Math.Abs(1000 - r));
				emdevice.SetNotRunning();
			});
		}
		#region PCW Benchmark
		private void PCWBM3()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double inputvalue = emdevice.ReadZXFloatFromOutQueue();
				double result = ((((inputvalue/inputvalue)*inputvalue)+inputvalue)-inputvalue);
				emdevice.WriteZXFloatToInQueue(result);
				emdevice.SetNotRunning();
			});
		}
		private void PCWBM4567()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double inputvalue = emdevice.ReadZXFloatFromOutQueue();
				double result = ((((inputvalue/2.0)*3.0) + 4.0)-5.0);
				emdevice.WriteZXFloatToInQueue(result);
				emdevice.SetNotRunning();
			});
		}
		#endregion
		#region Benchmark1
		private void LOGCOSSRQTSINFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double angle = emdevice.ReadZXFloatFromOutQueue();
				double sin = Math.Log(Math.Cos(Math.Sqrt(Math.Sin(angle))));
				emdevice.WriteZXFloatToInQueue(sin);
				emdevice.SetNotRunning();
			});
		}
		private void COSSRQTSINFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double angle = emdevice.ReadZXFloatFromOutQueue();
				double sin = Math.Cos(Math.Sqrt(Math.Sin(angle)));
				emdevice.WriteZXFloatToInQueue(sin);
				emdevice.SetNotRunning();
			});
		}
		private void SRQTSINFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double angle = emdevice.ReadZXFloatFromOutQueue();
				double sin = Math.Sqrt(Math.Sin(angle));
				emdevice.WriteZXFloatToInQueue(sin);
				emdevice.SetNotRunning();
			});
		}
		#endregion
		#region Benchmark2
		private void ButterflyCurve()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				double t = emdevice.ReadZXFloatFromOutQueue();
				double x = Math.Sin(t)*(Math.Exp(Math.Cos(t))-2*Math.Cos(4*t)-Math.Pow(Math.Sin(t/12),5));
				double y = Math.Cos(t)*(Math.Exp(Math.Cos(t))-2*Math.Cos(4*t)-Math.Pow(Math.Sin(t/12),5));
				emdevice.WriteZXFloatToInQueue(x);
				emdevice.WriteZXFloatToInQueue(y);
				emdevice.SetNotRunning();
			});
		}
		private void HalfPythagoreanTheorem()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				double a=emdevice.ReadZXFloatFromOutQueue();
				double b = emdevice.ReadZXFloatFromOutQueue();
				double halfc = (Math.Sqrt(a*a+b*b)/2.0);
				emdevice.WriteZXFloatToInQueue(halfc);
				emdevice.SetNotRunning();
			});
		}
		#endregion
		//Action to speed up ZX Basic's (https://github.com/boriel-basic/zxbasic)
		//mandel.bas sample program program
		private void MandelCalc()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				byte colour= emdevice.ReadByteFromOutQueue();
				byte iter=emdevice.ReadByteFromOutQueue();
				byte[] xar=emdevice.ReadBytesFromOutQueue(4);
				double x=ZXUtility.BBFixedToFloat(xar);
				byte[] yar = emdevice.ReadBytesFromOutQueue(4);
				double y = ZXUtility.BBFixedToFloat(yar);
				double newz, newzi;
				double z=0,zi =0;
				byte inset = 1;
				for (int k = 0; k < iter; k++)
				{
					newz = (z * z) - (zi * zi) + x;
					newzi = 2 * z * zi + y;
					z = newz;
					zi = newzi;
					if ((z * z) + (zi * zi) > 4)
					{
						inset = 0;
						colour = (byte)k;
						break;
					}
				}
				emdevice.WriteByteToInQueue(colour);
				emdevice.WriteByteToInQueue(inset);
				emdevice.SetNotRunning();
			});
		}
	}
}

