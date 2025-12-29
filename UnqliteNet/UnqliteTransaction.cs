// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// Portions inspired by Lightning.NET (https://github.com/CoreyKaylor/Lightning.NET) - MIT License

using System;

namespace UnqliteNet
{
    /// <summary>
    /// Represents the state of a transaction.
    /// </summary>
    public enum TransactionState
    {
        /// <summary>
        /// Transaction is active and can perform operations.
        /// </summary>
        Active,

        /// <summary>
        /// Transaction has been committed.
        /// </summary>
        Committed,

        /// <summary>
        /// Transaction has been rolled back.
        /// </summary>
        RolledBack,

        /// <summary>
        /// Transaction has been disposed.
        /// </summary>
        Disposed
    }

    public class UnqliteTransaction : IDisposable
    {
        private readonly IntPtr _dbHandle;
        private bool _disposed;
        private TransactionState _state;

        /// <summary>
        /// Gets the current state of the transaction.
        /// </summary>
        public TransactionState State => _state;

        /// <summary>
        /// Gets whether the transaction is active and can perform operations.
        /// </summary>
        public bool IsActive => _state == TransactionState.Active && !_disposed;

        public UnqliteTransaction(IntPtr dbHandle)
        {
            _dbHandle = dbHandle;
            int rc = NativeMethods.unqlite_begin(_dbHandle);
            UnqliteException.ThrowOnError(rc);
            _state = TransactionState.Active;
        }

        /// <summary>
        /// Commits the transaction, making all changes permanent.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the transaction is not in Active state.</exception>
        public void Commit()
        {
            if (_state != TransactionState.Active)
                throw new InvalidOperationException($"Cannot commit transaction in {_state} state. Transaction must be Active.");

            if (_disposed)
                throw new ObjectDisposedException(nameof(UnqliteTransaction));

            int rc = NativeMethods.unqlite_commit(_dbHandle);
            UnqliteException.ThrowOnError(rc);
            _state = TransactionState.Committed;
        }

        /// <summary>
        /// Rolls back the transaction, discarding all changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the transaction is not in Active state.</exception>
        public void Rollback()
        {
            if (_state != TransactionState.Active)
                throw new InvalidOperationException($"Cannot rollback transaction in {_state} state. Transaction must be Active.");

            if (_disposed)
                throw new ObjectDisposedException(nameof(UnqliteTransaction));

            int rc = NativeMethods.unqlite_rollback(_dbHandle);
            UnqliteException.ThrowOnError(rc);
            _state = TransactionState.RolledBack;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // 1. Auto-rollback si la transacción está activa
                // Esto se debe hacer SIEMPRE, sea disposing true o false
                if (_state == TransactionState.Active)
                {
                    try
                    {
                        NativeMethods.unqlite_rollback(_dbHandle);
                        _state = TransactionState.RolledBack;
                    }
                    catch
                    {
                        // Suppress rollback errors during dispose
                    }
                }

                // 2. Validación de buenas prácticas (Solo Debug)
                if (!disposing)
                {
                    // Loggear a consola de debug, pero NO lanzar excepción
                    System.Diagnostics.Debug.WriteLine("Recurso UnqliteTransaction fugado (no se llamó a Dispose)");
                }

                _state = TransactionState.Disposed;
                _disposed = true;
            }
        }

        ~UnqliteTransaction()
        {
            try
            {
                Dispose(false);
            }
            catch
            {
                // Suppress exceptions from finalizer to prevent crashing the application
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
