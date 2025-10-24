using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using EdgeMaster.Interfaces;
using Accord.Video.FFMPEG;
using System.Collections.Generic;

namespace EdgeMaster.Core
{
	// Benchmark Actions showcasing EM's acceleration of math intensive programs
	public partial class EMInternalLib : IEMLibrary
	{
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
		//Action to speed up Gabriel Gambetta's ZX Spectrum Raytracer
		//(https://www.gabrielgambetta.com/zx-raytracer.html)
		//While it mostly works, it has a bug somewhere and is not producing the correct
		//result. And right now i don't have time to hunt it down...
		private void TraceRay()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				
				double[,] _spheres = new double[,] { { 0, -1, 4, 1, 2 }, { 2, 0, 4, 1, 1 }, { -2, 0, 4, 1, 4 }, { 0, -5001, 0, 5000 ^ 2, 6 } };

				byte x = emdevice.ReadByteFromOutQueue();
				double RDX = ((x-16.0)/32.0);
				byte y = emdevice.ReadByteFromOutQueue();
				double RDY = ((11.0- y)/32.0);
				byte z = emdevice.ReadByteFromOutQueue();
				double RDZ = 1 * 1.0;
				double TMIN = 0;
				double TMAX = 100000;
				double ROX = 0.0;
				double ROY = 0.0;
				double ROZ = 0.0;
				int COL = -1;
				double MINT = 0;

				int spherecount=_spheres.GetLength(0);

				for (int c = 0; c < spherecount; c++)
				{
					double COX = ROX - _spheres[c,0];
					double COY = ROY - _spheres[c, 1];
					double COZ = ROZ - _spheres[c, 2];
					double EQA=RDX*RDX + RDY * RDY + RDZ * RDZ;
					double EQB = 2 * (RDX * COX + RDY * COY + RDZ * COZ);
					double EQC = (COX * COX + COY * COY + COZ * COZ) - _spheres[c, 3] * _spheres[c, 3];
					double DISC = EQB * EQB - 4 * EQA * EQC;
					if (DISC < 0) { continue; }
					double T1 = (-EQB + Math.Sqrt(DISC)) / 2 * EQA;
					double T2 = (-EQB - Math.Sqrt(DISC)) / 2 * EQA;
					if(T1>=TMIN && T1<=TMAX &&(T1<MINT || COL==-1)) { COL = (int)_spheres[c, 4]; MINT = T1; }
					if(T2 >= TMIN && T2 <= TMAX && (T2 < MINT || COL == -1)) { COL = (int)_spheres[c, 4]; MINT = T2; }
				}
				if(COL == -1) { COL = 0; }
				emdevice.WriteByteToInQueue((byte)COL);
				emdevice.SetNotRunning();
			});
		}
	}
}

