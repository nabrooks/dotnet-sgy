using System;
using System.Collections.Generic;
using System.Text;

namespace NbApps.Seismic.FileIo.Utility
{
    /// <summary>
    /// Abstract Base class for <see cref="IDisposable"/> objects that do not need a different base class.  This class properly implements the disposable pattern
    /// allowing you to simply override the <see cref="DisposeManagedResources"/> or <see cref="DisposeUnmanagedResources"/>
    /// methods appropriately. The <see cref="ThrowExceptionIfDisposed"/> method allows you to throw a consistent exception if your
    /// object has been disposed and can no longer function.
    /// </summary>
    /// <remarks>
    /// Code acquired from: http://davybrion.com/blog/2008/06/disposing-of-the-idisposable-implementation/
    /// </remarks>
    public abstract class Disposable : IDisposable
    {
        private State _state;

        /// <summary>
        /// The different states of the disposable
        /// </summary>
        private enum State : byte
        {
            /// <summary>
            /// Object is still active and in use
            /// </summary>
            Active,

            /// <summary>
            /// The Dispose process has started but not yet completed
            /// </summary>
            Disposing,

            /// <summary>
            /// The object has been disposed
            /// </summary>
            Disposed
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get { return _state == State.Disposed; }
        }

        /// <summary>
        /// Gets a value indicating whether this object is in the process of being disposed.
        /// </summary>
        protected bool IsDisposing
        {
            get { return _state == State.Disposing; }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Actually disposes the object.  Call this method from your finalizer (if defined), with <paramref name="disposing"/> set to false.
        /// </summary>
        /// <param name="disposing">true if called from <see cref="Dispose()"/>; otherwise, false</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This method is not meant to be overridden since the purpose of this base class is to ensure Dispose is done correctly.")]
        protected void Dispose(bool disposing)
        {
            if (_state == State.Active)
            {
                _state = State.Disposing; // only disposing at this stage so that subclasses can access members without throwing an ObjectDisposedException, yet we are protected from recursive Dispose calls.

                try
                {
                    if (disposing)
                    {
                        DisposeManagedResources();
                    }

                    DisposeUnmanagedResources();
                }
                finally
                {
                    _state = State.Disposed; // and now we are fully disposed
                }
            }
        }

        /// <summary>
        /// Implement this method to dispose of your managed resources.  Called automatically by <see cref="Dispose(bool)"/>.
        /// </summary>
        protected abstract void DisposeManagedResources();

        /// <summary>
        /// Override this method to dispose of your unmanaged resources.  Called automatically by <see cref="Dispose(bool)"/>.
        /// </summary>
        protected virtual void DisposeUnmanagedResources()
        {
        }

        /// <summary>
        /// Helper method you can call from your methods.  This method will throw an exception if this object has already been disposed.
        /// </summary>
        protected void ThrowExceptionIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        /// Helper method you can call from your methods.  This method will throw an exception if this object has already been disposed or is currently being disposed.
        /// Note: do not use this on methods you call from your DisposeManagedResources method otherwise you will get the exception during your dispose.
        /// </summary>
        protected void ThrowExceptionIfDisposedOrDisposing()
        {
            if (IsDisposed || IsDisposing)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
