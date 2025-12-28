using System;

namespace UnqliteNet
{
    public class UnqliteTransaction : IDisposable
    {
        private readonly IntPtr _dbHandle;
        private bool _disposed;
        private bool _committed;

        public UnqliteTransaction(IntPtr dbHandle)
        {
            _dbHandle = dbHandle;
            int rc = NativeMethods.unqlite_begin(_dbHandle);
            UnqliteException.ThrowOnError(rc);
        }

        public void Commit()
        {
            if (_committed || _disposed)
                throw new InvalidOperationException("Transaction already completed");

            int rc = NativeMethods.unqlite_commit(_dbHandle);
            UnqliteException.ThrowOnError(rc);
            _committed = true;
        }

        public void Rollback()
        {
            if (_committed || _disposed)
                throw new InvalidOperationException("Transaction already completed");

            int rc = NativeMethods.unqlite_rollback(_dbHandle);
            UnqliteException.ThrowOnError(rc);
            _committed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (!_committed)
                {
                    try
                    {
                        NativeMethods.unqlite_rollback(_dbHandle);
                    }
                    catch
                    {
                    }
                }
                _disposed = true;
            }
        }

        ~UnqliteTransaction()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
