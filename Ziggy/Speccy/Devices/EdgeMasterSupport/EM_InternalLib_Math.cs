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
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using NAudio.Wave;

namespace EdgeMaster.Core
{
	public partial class EMInternalLib : IEMLibrary
	{
		private void ADDFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double f1 = emdevice.ReadZXFloatFromOutQueue();
				double f2 = emdevice.ReadZXFloatFromOutQueue();
				double addv = f1 + f2;
				emdevice.WriteZXFloatToInQueue(addv);
				emdevice.SetNotRunning();
			});
		}
		private void SUBFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double f1 = emdevice.ReadZXFloatFromOutQueue();
				double f2 = emdevice.ReadZXFloatFromOutQueue();
				double addv = f1 - f2;
				emdevice.WriteZXFloatToInQueue(addv);
				emdevice.SetNotRunning();
			});
		}
		private void MULFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double f1 = emdevice.ReadZXFloatFromOutQueue();
				double f2 = emdevice.ReadZXFloatFromOutQueue();
				double addv = f1 * f2;
				emdevice.WriteZXFloatToInQueue(addv);
				emdevice.SetNotRunning();
			});
		}
		private void DIVFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double f1 = emdevice.ReadZXFloatFromOutQueue();
				double f2 = emdevice.ReadZXFloatFromOutQueue();
				double addv = f1 / f2;
				emdevice.WriteZXFloatToInQueue(addv);
				emdevice.SetNotRunning();
			});
		}
		private void SQRTFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double value = emdevice.ReadZXFloatFromOutQueue();
				double sqrt = Math.Sqrt(value);
				emdevice.WriteZXFloatToInQueue(sqrt);
				emdevice.SetNotRunning();
			});
		}
		private void SINFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double angle = emdevice.ReadZXFloatFromOutQueue();
				double sin = Math.Sin(angle);
				emdevice.WriteZXFloatToInQueue(sin);
				emdevice.SetNotRunning();
			});
		}
		private void COSFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double angle = emdevice.ReadZXFloatFromOutQueue();
				double cos = Math.Cos(angle);
				emdevice.WriteZXFloatToInQueue(cos);
				emdevice.SetNotRunning();
			});
		}
		private void TANFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double angle = emdevice.ReadZXFloatFromOutQueue();
				double tan = Math.Tan(angle);
				emdevice.WriteZXFloatToInQueue(tan);
				emdevice.SetNotRunning();
			});
		}
		private void ASINFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double value = emdevice.ReadZXFloatFromOutQueue();
				double asin = Math.Asin(value);
				emdevice.WriteZXFloatToInQueue(asin);
				emdevice.SetNotRunning();
			});
		}
		private void ACOSFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double value = emdevice.ReadZXFloatFromOutQueue();
				double acos = Math.Acos(value);
				emdevice.WriteZXFloatToInQueue(acos);
				emdevice.SetNotRunning();
			});
		}
		private void ATANFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double angle = emdevice.ReadZXFloatFromOutQueue();
				double atan = Math.Atan(angle);
				emdevice.WriteZXFloatToInQueue(atan);
				emdevice.SetNotRunning();
			});
		}
		private void EXPFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double value = emdevice.ReadZXFloatFromOutQueue();
				double exp = Math.Exp(value);
				emdevice.WriteZXFloatToInQueue(exp);
				emdevice.SetNotRunning();
			});
		}
		private void LNFL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double value = emdevice.ReadZXFloatFromOutQueue();
				double ln = Math.Log(value);
				emdevice.WriteZXFloatToInQueue(ln);
				emdevice.SetNotRunning();
			});
		}
		private void POW2FL()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				double value = emdevice.ReadZXFloatFromOutQueue();
				double pow2 = Math.Pow(value, 2);
				emdevice.WriteZXFloatToInQueue(pow2);
				emdevice.SetNotRunning();
			});
		}		
		private void SIN8()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				//Get fixed value
				byte[] anglebytes = emdevice.ReadBytesFromOutQueue(1);
				int angle = anglebytes[0];
				byte scaledcos;
				//We deal with special cases separatelly because we don't want rounding errors there...
				if (angle == 0)
				{
					scaledcos = 0;
				}
				else if (angle == 90)
				{
					scaledcos = 200;
				}
				else
				{
					float floatcos = (float)Math.Sin(angle * (ZXUtility.RadPerDegree));
					scaledcos = (byte)(floatcos * 200);
				}
				emdevice.WriteByteToInQueue(scaledcos);
				emdevice.SetNotRunning();
			});
		}
		private void SINFX()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				byte[] anglebytes = emdevice.ReadBytesFromOutQueue(4);
				float angle = ZXUtility.BBFixedToFloat(anglebytes);
				float sin = (float)(Math.Sin(angle));
				Console.WriteLine($"Angle : {angle} - Sin : {sin}");
				byte[] outbytes = ZXUtility.FloatToBBFixed(sin);
				emdevice.WriteBytesToInQueue(outbytes);
				emdevice.SetNotRunning();
			});
		}
		private void SINS()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				string anglestring = emdevice.ReadStringFromOutQueue();
				double angle = double.Parse(anglestring);
				double sin = Math.Sin(angle);
				string sinstring = sin.ToString();
				emdevice.WriteStringToInQueue(sinstring);
				emdevice.SetNotRunning();
			});
		}		
		private void COS8()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				//Get first float value
				byte[] anglebytes = emdevice.ReadBytesFromOutQueue(1);
				int angle = anglebytes[0];
				byte scaledcos;
				//We deal with special cases separatelly because we don't want rounding errors there...
				if (angle == 0)
				{
					scaledcos = 200;
				}
				else if (angle == 90)
				{
					scaledcos = 0;
				}
				else
				{
					float floatcos = (float)Math.Cos(angle * (ZXUtility.RadPerDegree));
					scaledcos = (byte)(floatcos * 200);
				}
				emdevice.WriteByteToInQueue(scaledcos);
				emdevice.SetNotRunning();
			});
		}
		private void COSS()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				string anglestring = emdevice.ReadStringFromOutQueue();
				double angle = double.Parse(anglestring);
				double cos = Math.Cos(angle);
				string cosstring = cos.ToString();
				emdevice.WriteStringToInQueue(cosstring);
				emdevice.SetNotRunning();
			});
		}		
		private void SQRTS()
		{
			emdevice.SetRunning();
			Task.Run(() =>
			{
				string anglestring = emdevice.ReadStringFromOutQueue();
				double angle = double.Parse(anglestring);
				double sqrt = Math.Sqrt(angle);
				string sqrtstring = sqrt.ToString();
				emdevice.WriteStringToInQueue(sqrtstring);
				emdevice.SetNotRunning();
			});
		}
	}
}

