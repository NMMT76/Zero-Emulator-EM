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
using Speccy.Devices.EdgeMasterSupport;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using NAudio.Wave;

namespace EdgeMaster.Core
{
	public partial class EMInternalLib : IEMLibrary
	{
		//DirectShowLib-2005 camera and control
		private Capture cam;
		private Control camcontrol;
		// enumerate video devices
		const string VIDEODEVICE = "Logi C270 HD WebCam"; // name of camera to use
		const int VIDEOWIDTH = 640; // Depends on video device caps
		const int VIDEOHEIGHT = 480; // Depends on video device caps
		const int VIDEOBITSPERPIXEL = 24; // BitsPerPixel values determined by device

		//File path prefix. NOT using one has security implications. You DON'T want your
		//ZX being able to touch just about anything it wants so, you restrict it to some
		//fixed starting point in the filesystem by using a path prefix AND sanitizing the
		//input

		private IEMDevice emdevice=null;

		public EMInternalLib()
		{
			//DirectShowLib-2005 camera and control initialization
			camcontrol = new Control();
			cam = Capture.New(VIDEODEVICE, VIDEOWIDTH, VIDEOHEIGHT, VIDEOBITSPERPIXEL, camcontrol);
		}
		public void Reset()
		{
			cam?.Dispose();
			camcontrol?.Dispose();
		}
		public void RegisterEM(IEMDevice emdevice)
		{
			this.emdevice = emdevice;
			#region MiscTag
			emdevice.AddAction("GUID", GetGUID);
			emdevice.AddAction("GUIDS", GetGUIDS);
			emdevice.AddAction("RND8S", RND8S);
			emdevice.AddAction("RND8SB", RND8SB);
			#endregion
			#region MathTag
			emdevice.AddAction("ADDFL", ADDFL);
			emdevice.AddAction("SUBFL", SUBFL);
			emdevice.AddAction("MULFL", MULFL);
			emdevice.AddAction("DIVFL", DIVFL);
			emdevice.AddAction("SQRTFL", SQRTFL);
			emdevice.AddAction("SINFL", SINFL);
			emdevice.AddAction("COSFL", COSFL);
			emdevice.AddAction("TANFL", TANFL);
			emdevice.AddAction("ASINFL", ASINFL);
			emdevice.AddAction("ACOSFL", ACOSFL);
			emdevice.AddAction("ATANFL", ATANFL);
			emdevice.AddAction("EXPFL", EXPFL);
			emdevice.AddAction("LNFL", LNFL);
			emdevice.AddAction("POW2FL", POW2FL);
			emdevice.AddAction("SIN8", SIN8);
			emdevice.AddAction("SINFX", SINFX);
			emdevice.AddAction("SINS", SINS);
			emdevice.AddAction("COS8", COS8);
			emdevice.AddAction("COSS", COSS);
			emdevice.AddAction("SQRTS", SQRTS);
			#endregion
			#region FileIOTag
			emdevice.AddAction("FREAD", FileRead);
			emdevice.AddAction("FWRITE", FileWrite);
			#endregion
			#region RAM
			emdevice.AddAction("READRAMBYTE", ReadRamByte);
			emdevice.AddAction("WRITERAMBYTE", WriteRamByte);
			emdevice.AddAction("READRAMBYTES", ReadRamBytes);
			emdevice.AddAction("WRITERAMBYTES", WriteRamBytes);
			
			#endregion
			#region MediaTag
			emdevice.AddAction("CAPTUREIMAGEBWPIO", CaptureImageBWPIO);
			emdevice.AddAction("CAPTUREIMAGEBWDMA", CaptureImageBWDMA);
			emdevice.AddAction("IMAGELOADBWPIO", IMAGELOADBWPIO);
			emdevice.AddAction("IMAGELOADBWDMA", IMAGELOADBWDMA);
			emdevice.AddAction("PLAYAUDIOSYNC", PlayAudioSync);
			emdevice.AddAction("PLAYVIDEOFILEBW", PlayVideoFileBW);
			#endregion
			//Benchmarks and tests
			#region BenchmarkTestTag
			emdevice.AddAction("ADDSUBMULDIV", ADDSUBMULDIV);
			emdevice.AddAction("TRACERAY", TraceRay);
			emdevice.AddAction("SRQTSINFL", SRQTSINFL);
			emdevice.AddAction("COSSRQTSINFL", COSSRQTSINFL);
			emdevice.AddAction("LOGCOSSRQTSINFL", LOGCOSSRQTSINFL);
			emdevice.AddAction("HAPYTH", HalfPythagoreanTheorem);
			emdevice.AddAction("BUTTERFLY", ButterflyCurve);
			emdevice.AddAction("MANDELCALC", MandelCalc);
			emdevice.AddAction("PCWBM3", PCWBM3);
			emdevice.AddAction("PCWBM4567", PCWBM4567);
			#endregion
		}
		#region Misc
		private void GetGUID()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				byte[] data = Guid.NewGuid().ToByteArray();
				emdevice.WriteBytesToInQueue(data);
				emdevice.SetNotRunning();
			});
		}
		private void GetGUIDS()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				byte[] data = ZXUtility.StringToByteArray(Guid.NewGuid().ToString(), Encoding.ASCII);
				emdevice.WriteBytesToInQueue(data);
				emdevice.SetNotRunning();
			});
		}
		private void RND8S()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				//Get first float value
				byte[] rndvalues = new byte[1];
				emdevice.Rng.NextBytes(rndvalues);
				emdevice.WriteByteToInQueue(rndvalues[0]);
				emdevice.SetNotRunning();
			});
		}
		private void RND8SB()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				//Get low and high bounds
				byte[] bounds = emdevice.ReadBytesFromOutQueue(2);
				byte[] rndvalues = new byte[1];
				do
				{
					emdevice.Rng.NextBytes(rndvalues);
				}
				while (rndvalues[0] < bounds[0] || rndvalues[0] > bounds[1]);
				emdevice.WriteByteToInQueue(rndvalues[0]);
				emdevice.SetNotRunning();
			});
		}
		#endregion
		#region Media
		private void IMAGELOADBW(Action<byte[]> completion)
		{
			string filename = emdevice.ReadStringFromOutQueue();
			if (!string.IsNullOrWhiteSpace(filename))
			{
				Image sourceimage = null;
				if (filename.Contains("http://") || filename.Contains("https://"))
				{
					var client = new HttpClient();
					var task = Task.Run(() => client.GetAsync(filename));
					task.Wait();
					var response = task.Result;
					var task2 = Task.Run(() => response.Content.ReadAsByteArrayAsync());
					task2.Wait();
					byte[] responsedata = task2.Result;
					MemoryStream ms = new MemoryStream(responsedata);
					sourceimage = Image.FromStream(ms);
				}
				else
				{
					sourceimage = Image.FromFile(filename);
				}
				if (sourceimage != null)
				{
					Bitmap loadedjpeg = (Bitmap)sourceimage;
					Bitmap scaled = loadedjpeg.Resize(new Size(ZXUtility.ZXResolutionWidth, ZXUtility.ZXResolutionHeight), ScalingMode.Auto, false);
					Bitmap quantized = scaled.ConvertPixelFormat(PixelFormat.Format1bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), ErrorDiffusionDitherer.FloydSteinberg);
					Bitmap reduced = quantized.Clone(new System.Drawing.Rectangle(0, 0, ZXUtility.ZXResolutionWidth, ZXUtility.ZXResolutionHeight), PixelFormat.Format1bppIndexed);
					byte[] bytes = ZXUtility.BitmapToByteArray(reduced);
					reduced.Dispose();
					quantized.Dispose();
					scaled.Dispose();
					loadedjpeg.Dispose();
					byte[] screenmemoryarray = ZXUtility.ByteArrayToScreenMemoryArray(bytes);
					completion.Invoke(screenmemoryarray);
				}
			}
		}
		/// <summary>
		/// Loads an image onto the ZX memory using basic PIO
		/// </summary>
		private void IMAGELOADBWPIO()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				IMAGELOADBW(ToInQueue);
				emdevice.SetNotRunning();
			});
			void ToInQueue(byte[] bytes)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					emdevice.WriteByteToInQueue((byte)(~bytes[i]));
				}
			}
		}
		/// <summary>
		/// Loads an image onto the ZX memory using Quasi-DMA, with its limitations
		/// </summary>
		private void IMAGELOADBWDMA()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				IMAGELOADBW(ToZXRAM);
				emdevice.SetNotRunning();
			});
			void ToZXRAM(byte[] bytes)
			{
				while (emdevice.GetTStates() > ZXUtility.ZXMaxTStateForDMA)
				{
					Thread.Sleep(5);
				}
				for (int i = 0; i < bytes.Length; i++)
				{
					emdevice.PokeByteDMA((ushort)(ZXUtility.MemoryScreenStart+i), (byte)(~bytes[i]));
				}
			}
		}
		public void PlayAudioSync()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				string audiofile=emdevice.ReadStringFromOutQueue();
				using (var audioFile = new AudioFileReader(audiofile))
				{
					using (var outputDevice = new WaveOutEvent())
					{
						outputDevice.Init(audioFile);
						outputDevice.Play();
						while (outputDevice.PlaybackState == PlaybackState.Playing)
						{
							Thread.Sleep(1000);
						}
					}
				}
				emdevice.SetNotRunning();
			});
		}
		#endregion
		private void DecompressGZIP()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				//Input length
				int inputlength = (int)(emdevice.ReadULongFromOutQueue());

				byte[] inbuffer = emdevice.ReadBytesFromOutQueue(inputlength);
				byte[] outbuffer = ZXUtility.DecompressGZIP(inbuffer);
				
				UInt32 decompressedlength = (UInt32)(outbuffer.Length);
				emdevice.WriteULongToInQueue(decompressedlength);
				emdevice.SetNotRunning();
			});
		}
		private void CompressGZIP()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				//Input length
				int length = (int)(emdevice.ReadULongFromOutQueue());

				byte[] inbuffer = emdevice.ReadBytesFromOutQueue(length);
				byte[] outbuffer=ZXUtility.CompressGZIP(inbuffer);

				UInt32 compressedlength = (UInt32)(outbuffer.Length);
				emdevice.WriteULongToInQueue(compressedlength);
				emdevice.WriteBytesToInQueue(outbuffer);
				emdevice.SetNotRunning();
			});
		}
		private void CaptureImageBW(Action<byte[]> completion)
		{
			byte quant = emdevice.ReadByteFromOutQueue();
			if (cam != null)
			{
				IntPtr m_ip = m_ip = cam.Click();
				Bitmap captured = new Bitmap(cam.Width, cam.Height, cam.Stride, PixelFormat.Format24bppRgb, m_ip);
				captured.RotateFlip(RotateFlipType.Rotate180FlipNone);

				Bitmap scaled = captured.Resize(new Size(ZXUtility.ZXResolutionWidth, ZXUtility.ZXResolutionHeight), ScalingMode.Auto, false);
				Bitmap quantized = null;
				switch (quant)
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
				captured.Dispose();
				Marshal.FreeCoTaskMem(m_ip);
				byte[] screenmemoryarray = ZXUtility.ByteArrayToScreenMemoryArray(bytes);
				completion.Invoke(screenmemoryarray);
			}
		}
		public void CaptureImageBWPIO()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				CaptureImageBW(ToInQueue);
				emdevice.SetNotRunning();
			});
			void ToInQueue(byte[] bytes)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					emdevice.WriteByteToInQueue((byte)(~bytes[i]));
				}
			}
		}
		public void CaptureImageBWDMA()
		{
			emdevice.SetRunning();

			Task.Run(() =>
			{
				CaptureImageBW(ToZXRAM);
				emdevice.SetNotRunning();
			});
			void ToZXRAM(byte[] bytes)
			{
				while (emdevice.GetTStates()> ZXUtility.ZXMaxTStateForDMA)
				{
					Thread.Sleep(1);
				}
				for (int i = 0; i < bytes.Length; i++)
				{
					emdevice.PokeByteDMA((ushort)(ZXUtility.MemoryScreenStart + i), (byte)(~bytes[i]));
				}
			}
		}
		private void FileRead()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				byte[] filebytes = null;

				string filename = emdevice.ReadStringFromOutQueue();

				if (!string.IsNullOrWhiteSpace(filename) && File.Exists(filename))
				{
					filebytes = File.ReadAllBytes(filename);
				}

				UInt32 filelength;

				if (filebytes != null)
				{
					filelength = ((UInt32)(filebytes.Length));
				}
				else
				{
					filelength = 0;
				}
				emdevice.WriteULongToInQueue(filelength);
				if (filebytes != null && filebytes.Length > 0)
				{
					emdevice.WriteBytesToInQueue(filebytes);
				}
				emdevice.SetNotRunning();
			});
		}
		private void FileWrite()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				string filename = emdevice.ReadStringFromOutQueue();
				UInt32 filelength = 0;
				if (!string.IsNullOrWhiteSpace(filename))
				{
					filelength = emdevice.ReadULongFromOutQueue();
				}
				if (filelength != 0)
				{
					byte[] filecontent = new byte[filelength];
					for (int c = 0; c < filecontent.Length; c++)
					{
						filecontent[c] = emdevice.ReadByteFromOutQueue();
					}
					File.WriteAllBytes(filename, filecontent);
				}
				emdevice.SetNotRunning();
			});
		}
	}
}

