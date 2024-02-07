using System.Diagnostics;

namespace MauiNurAppTemplate.Helpers
{
    /// <summary>
    /// This is used for detecting when slider movement is done and ready to set new txlevel to reader.
    /// Can be used for cases like "do action when no data received within x milliseconds"
    /// </summary>
    public class ExpirationWatcher
    {
        /// <summary>
        /// This event fire when Reset() has not been called within specified time.
        /// </summary>
        public event EventHandler<EventArgs>? Expired;

        private CancellationTokenSource _cancel;
        private AutoResetEvent _resetEvent;
        private Task expireWatchTask;
        private int _expireTimeMs;
                
        private bool _expired;
        private bool _reset;

        public ExpirationWatcher() 
        {            
            _expired = true;
            _reset = false;
            _expireTimeMs = 1000;
            _cancel = new CancellationTokenSource();
            _resetEvent = new AutoResetEvent(false);

            expireWatchTask = Task.Factory.StartNew(() =>
            {
                ExpireWatchTask();
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {            
            _cancel.Cancel();
            _resetEvent.Set();
        }

        /// <summary>
        /// Call this when need to start measure time from the beginning.
        /// </summary>
        /// <param name="ms">time in milliseconds</param>
        public void Reset(int ms)
        {            
            _expireTimeMs = ms;
            _reset = true;
            _expired = false;
            _resetEvent.Set();
            
        }

        private void ExpireWatchTask()
        {
            Debug.WriteLine("ENTER EXPRIRE");

            while (true)
            {
                _reset = false;
                _resetEvent.WaitOne(_expireTimeMs);
                if (_cancel.IsCancellationRequested)
                    break;

                if (_reset)
                    continue;

                if (_expired)
                    continue;

                Expired.Invoke(this, EventArgs.Empty);
                _expired = true;

            }

            Debug.WriteLine("EXIT EXPRIRE");
        }
    }
        
}
