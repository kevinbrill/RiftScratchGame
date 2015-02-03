using System;
using System.Threading;

#if __MonoCS__
using Mono.Unix;
using Mono.Unix.Native;
#endif

namespace RiftScratchGame
{
	public class Waiter
	{
		public Waiter ()
		{
		}

		public void Wait()
		{
			#if __MonoCS__

			var signals = new UnixSignal[] { 
				new UnixSignal(Signum.SIGINT), 
				new UnixSignal(Signum.SIGTERM), 
			};

			// Wait for a unix signal
			for (bool exit = false; !exit; )
			{
				int id = UnixSignal.WaitAny(signals);

				if (id >= 0 && id < signals.Length)
				{
					if (signals[id].IsSet) exit = true;
				}
			}

			#else

			var _quitEvent = new ManualResetEvent(false);

			Console.CancelKeyPress += (sender, eArgs) => {
				_quitEvent.Set();
				eArgs.Cancel = true;
			};

			_quitEvent.WaitOne();

			#endif
		}
	}
}

