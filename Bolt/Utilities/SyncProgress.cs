using System.ComponentModel;

namespace Bolt.Utilities
{
    /// <summary>
    /// A synchronous progress reporter that marshals updates back to a specific synchronization context.
    /// </summary>
    internal class SyncProgress<T>(Action<T> action, ISynchronizeInvoke syncObject) : IProgress<T>
    {
        private readonly Action<T> _action = action ?? throw new ArgumentNullException(nameof(action));
        private readonly ISynchronizeInvoke _syncObject = syncObject ?? throw new ArgumentNullException(nameof(syncObject));

        public void Report(T value)
        {
            // Safety check to avoid ObjectDisposedException if the underlying object is a WinForms Control.
            // Using pattern matching keeps the abstraction clean while still protecting the UI thread.
            if (_syncObject is Control control && (!control.IsHandleCreated || control.IsDisposed))
                return;

            if (_syncObject.InvokeRequired)
                _syncObject.Invoke(_action, [value]);
            else
                _action(value);
        }
    }
}