using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Buildersoft.Andy.X.Storage.Model.Threading
{
    public class ThreadingPool
    {
        public ConcurrentDictionary<Guid, ThreadDetails> Threads { get; set; }
        public bool AreThreadsRunning { get; set; }
        public ThreadingPool(int size)
        {
            AreThreadsRunning = false;
            Threads = new ConcurrentDictionary<Guid, ThreadDetails>();
            for (int i = 0; i < size; i++)
            {
                var details = new ThreadDetails();
                details.UpdateMainThreadRunningStatus += Details_UpdateMainThreadRunningStatus;
                Threads.TryAdd(Guid.NewGuid(), details);
            }
        }

        private void Details_UpdateMainThreadRunningStatus()
        {
            // check if all isThreadWorking eq false, update AreThreadsRunning to false;
            if (Threads.Where(x => x.Value.IsThreadWorking == true).Count() == 0)
            {
                AreThreadsRunning = false;
            }
        }
    }

    public class ThreadDetails
    {
        public delegate void UpdateMainThreadRunningStatusHandler();
        public event UpdateMainThreadRunningStatusHandler UpdateMainThreadRunningStatus;

        public Thread Thread { get; set; }

        private bool isThreadWorking = false;
        public bool IsThreadWorking
        {
            get
            {
                return isThreadWorking;
            }
            set
            {
                isThreadWorking = value;
                UpdateMainThreadRunningStatus?.Invoke();
            }
        }

        public ThreadDetails()
        {
            Thread = null;
        }
    }
}
