using System;

namespace EdgeMaster.Enums
{
	[Flags]
	public enum EdgeMasterStateMachine
	{
		None = 0,
		Running = 1,
		ReadFromEmptyBuffer = 2,
	}
}
namespace EdgeMaster.Interfaces
{
	public interface IEMLibrary
	{
		void RegisterEM(IEMDevice emdevice);
		void Reset();
	}
	public interface IEMDevice
	{

		/// <summary>
		/// Adds an action to the actions dictionary for later use. actionid is "whatever you want", the ZX side
		/// of the code MUST provide an exact match for it t be able to register it.
		/// It works on a "first come first served" basis, thus, internal id's for EM's internal libraries will
		/// always have precedence and external libraries should provide a way to "rename" their id's in case they
		/// clash with other libraries.
		/// </summary>
		/// <param name="actionid"></param>
		/// <param name="action"></param>
		bool AddAction(string actionid, Action action);
		#region IO
		//IO related methods
		byte ReadByteFromOutQueue();
		byte[] ReadBytesFromOutQueue(int numbytes);
		uint ReadULongFromOutQueue();
		string ReadStringFromOutQueue();
		double ReadZXFloatFromOutQueue();
		void WriteByteToInQueue(byte bytevalue);
		void WriteBytesToInQueue(byte[] bytevalues);
		void WriteULongToInQueue(UInt32 value);
		void WriteZXFloatToInQueue(double value);
		void WriteStringToInQueue(string content);
		#endregion
		#region QuasiDMA
		//Quasi DMA Showcase methods, development only!
		//They actually call on the host to do memory IO as if it was the Z80, subject to whatever
		//contention the Z80 would have had with the ULA. The upside is that you DON'T have the Z80
		//doing a "copy loop", reading from the bus and writing to RAM. The EM itself is writing to
		//RAM from it's own "internals", thus, at the very least, you'll get ~2x throughput.
		//You still need to wait on the EM finishing though (with current BB code). A more efficient
		//Approach would be to mark if you'd done a DMA action last, and if so, force a wait for its
		//completion before you start running the next action. For demonstration purposes though,
		//waiting as usual is good enough... Also keep in mind it acts on the machines internal tstate
		//thus you HAVE to make sure you have enough left over to complete the operation, which might
		//then make it even slower than PIO. Internal actions will wait until the machine's tstate is
		//under the value in ZXUtility.ZXMaxTStateForDMA (default set at a very consertative 10k)
		//More proper use would be to call an action from the VBI that would give you a more or less
		//fixed "window of opportunity" allowing you to calibrate how much you can transfer in a "safe"
		//fashion before the next VBI.
		void PokeByteDMA(ushort addr, byte val);
		byte PeekByteDMA(ushort addr);
		int GetTStates();
		#endregion
		#region State
		//State related methods
		void SetRunning();
		void SetNotRunning();
		#endregion
		Random Rng { get; }
	}
}
