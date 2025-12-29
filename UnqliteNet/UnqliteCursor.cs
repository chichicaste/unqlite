// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// Portions inspired by Lightning.NET (https://github.com/CoreyKaylor/Lightning.NET) - MIT License

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UnqliteNet
{
    /// <summary>
    /// Provides cursor-based iteration over database records.
    /// </summary>
    /// <remarks>
    /// Cursors allow sequential or random access to database records.
    /// Always dispose cursors when done to release native resources.
    /// Note: UnQLite does not support duplicate keys like LMDB.
    /// </remarks>
    public class UnqliteCursor : ICursor, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        private readonly IntPtr _dbHandle;
        private IntPtr _cursorHandle;
        private bool _disposed;

        internal UnqliteCursor(IntPtr dbHandle)
        {
            _dbHandle = dbHandle;
            int rc = NativeMethods.unqlite_kv_cursor_init(_dbHandle, out _cursorHandle);
            UnqliteException.ThrowOnError(rc);
        }

        /// <summary>
        /// Positions the cursor at or near the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="position">The seek position strategy (Exact, LessOrEqual, GreaterOrEqual).</param>
        /// <returns>True if a matching record was found, false otherwise.</returns>
        public bool Seek(string key, SeekPosition position = SeekPosition.Exact)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int pos = position switch
            {
                SeekPosition.Exact => NativeMethods.UNQLITE_CURSOR_MATCH_EXACT,
                SeekPosition.LessOrEqual => NativeMethods.UNQLITE_CURSOR_MATCH_LE,
                SeekPosition.GreaterOrEqual => NativeMethods.UNQLITE_CURSOR_MATCH_GE,
                _ => NativeMethods.UNQLITE_CURSOR_MATCH_EXACT
            };

            int rc = NativeMethods.unqlite_kv_cursor_seek(_cursorHandle, keyBytes, keyBytes.Length, pos);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Positions the cursor at the specified key using exact matching.
        /// This is an alias for Seek(key, SeekPosition.Exact).
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool Set(string key)
        {
            return Seek(key, SeekPosition.Exact);
        }

        /// <summary>
        /// Positions the cursor at or near the specified key.
        /// This is an alias for Seek(key, position).
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="position">The seek position strategy (Exact, LessOrEqual, GreaterOrEqual).</param>
        /// <returns>True if a matching record was found, false otherwise.</returns>
        public bool Set(string key, SeekPosition position)
        {
            return Seek(key, position);
        }

        /// <summary>
        /// Moves the cursor to the first record in the database.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false otherwise.</returns>
        public bool First()
        {
            int rc = NativeMethods.unqlite_kv_cursor_first_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Moves the cursor to the last record in the database.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false otherwise.</returns>
        public bool Last()
        {
            int rc = NativeMethods.unqlite_kv_cursor_last_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Gets whether the cursor is currently positioned on a valid record.
        /// </summary>
        public bool IsValid
        {
            get
            {
                int rc = NativeMethods.unqlite_kv_cursor_valid_entry(_cursorHandle);
                return rc != 0;
            }
        }

        /// <summary>
        /// Moves the cursor to the next record.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false if at end.</returns>
        public bool Next()
        {
            int rc = NativeMethods.unqlite_kv_cursor_next_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Moves the cursor to the previous record.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false if at beginning.</returns>
        public bool Previous()
        {
            int rc = NativeMethods.unqlite_kv_cursor_prev_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Gets the current cursor position's key as a string.
        /// </summary>
        public string Key
        {
            get
            {
                int keyLen = 0;
                int rc = NativeMethods.unqlite_kv_cursor_key(_cursorHandle, null!, ref keyLen);
                UnqliteException.ThrowOnError(rc);

                if (keyLen == 0)
                    return string.Empty;

                byte[] keyBytes = new byte[keyLen];
                rc = NativeMethods.unqlite_kv_cursor_key(_cursorHandle, keyBytes, ref keyLen);
                UnqliteException.ThrowOnError(rc);

                return Encoding.UTF8.GetString(keyBytes);
            }
        }

        /// <summary>
        /// Gets the current cursor position's data as a byte array.
        /// Returns null if the cursor is not positioned on a valid record.
        /// </summary>
        public byte[]? Data
        {
            get
            {
                if (!IsValid)
                    return null;

                long dataLen = 0;
                int rc = NativeMethods.unqlite_kv_cursor_data(_cursorHandle, null!, ref dataLen);
                UnqliteException.ThrowOnError(rc);

                if (dataLen == 0)
                    return Array.Empty<byte>();

                byte[] data = new byte[dataLen];
                rc = NativeMethods.unqlite_kv_cursor_data(_cursorHandle, data, ref dataLen);
                UnqliteException.ThrowOnError(rc);

                return data;
            }
        }

        /// <summary>
        /// Gets the current cursor position's data as a string.
        /// </summary>
        public string DataAsString
        {
            get
            {
                byte[]? data = Data;
                return data == null || data.Length == 0 ? string.Empty : Encoding.UTF8.GetString(data);
            }
        }

        /// <summary>
        /// Gets the current key and data as a key-value pair.
        /// </summary>
        /// <returns>A key-value pair containing the current record, or null if cursor is not valid.</returns>
        public KeyValuePair<byte[], byte[]>? GetCurrent()
        {
            if (!IsValid)
                return null;

            byte[]? data = Data;
            if (data == null)
                return null;

            byte[] keyBytes = Encoding.UTF8.GetBytes(Key);
            return new KeyValuePair<byte[], byte[]>(keyBytes, data);
        }

        /// <summary>
        /// Tries to copy the current key into the provided buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write the key into.</param>
        /// <param name="bytesWritten">The number of bytes actually written.</param>
        /// <returns>True if successful, false if buffer is too small or cursor is invalid.</returns>
        public unsafe bool TryGetKey(Span<byte> buffer, out int bytesWritten)
        {
            bytesWritten = 0;

            if (!IsValid)
                return false;

            int keyLen = 0;
            int rc = NativeMethods.unqlite_kv_cursor_key(_cursorHandle, null!, ref keyLen);
            if (rc != NativeMethods.UNQLITE_OK || keyLen == 0)
                return false;

            if (buffer.Length < keyLen)
                return false;

            byte[] tempBuffer = new byte[keyLen];
            rc = NativeMethods.unqlite_kv_cursor_key(_cursorHandle, tempBuffer, ref keyLen);
            if (rc != NativeMethods.UNQLITE_OK)
                return false;

            tempBuffer.AsSpan(0, keyLen).CopyTo(buffer);
            bytesWritten = keyLen;
            return true;
        }

        /// <summary>
        /// Tries to copy the current data into the provided buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="bytesWritten">The number of bytes actually written.</param>
        /// <returns>True if successful, false if buffer is too small or cursor is invalid.</returns>
        public unsafe bool TryGetData(Span<byte> buffer, out int bytesWritten)
        {
            bytesWritten = 0;

            if (!IsValid)
                return false;

            long dataLen = 0;
            int rc = NativeMethods.unqlite_kv_cursor_data(_cursorHandle, null!, ref dataLen);
            if (rc != NativeMethods.UNQLITE_OK || dataLen == 0)
                return false;

            if (buffer.Length < (int)dataLen)
                return false;

            byte[] tempBuffer = new byte[dataLen];
            rc = NativeMethods.unqlite_kv_cursor_data(_cursorHandle, tempBuffer, ref dataLen);
            if (rc != NativeMethods.UNQLITE_OK)
                return false;

            tempBuffer.AsSpan(0, (int)dataLen).CopyTo(buffer);
            bytesWritten = (int)dataLen;
            return true;
        }

        /// <summary>
        /// Deletes the record at the current cursor position.
        /// After deletion, the cursor moves to the next record.
        /// </summary>
        public void Delete()
        {
            int rc = NativeMethods.unqlite_kv_cursor_delete_entry(_cursorHandle);
            UnqliteException.ThrowOnError(rc);
        }

        /// <summary>
        /// Resets the cursor to an uninitialized state.
        /// </summary>
        public void Reset()
        {
            int rc = NativeMethods.unqlite_kv_cursor_reset(_cursorHandle);
            UnqliteException.ThrowOnError(rc);
        }

        // Callback-based access for better performance with large data
        public void GetKeyCallback(Action<byte[]> callback)
        {
            var consumer = new NativeMethods.UnqliteDataConsumer((pData, nDataLen, pUserData) =>
            {
                if (pData == IntPtr.Zero || nDataLen == 0)
                    return 0;

                byte[] data = new byte[nDataLen];
                Marshal.Copy(pData, data, 0, (int)nDataLen);
                callback(data);
                return 0;
            });

            int rc = NativeMethods.unqlite_kv_cursor_key_callback(_cursorHandle, consumer, IntPtr.Zero);
            UnqliteException.ThrowOnError(rc);
        }

        public void GetDataCallback(Action<byte[]> callback)
        {
            var consumer = new NativeMethods.UnqliteDataConsumer((pData, nDataLen, pUserData) =>
            {
                if (pData == IntPtr.Zero || nDataLen == 0)
                    return 0;

                byte[] data = new byte[nDataLen];
                Marshal.Copy(pData, data, 0, (int)nDataLen);
                callback(data);
                return 0;
            });

            int rc = NativeMethods.unqlite_kv_cursor_data_callback(_cursorHandle, consumer, IntPtr.Zero);
            UnqliteException.ThrowOnError(rc);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the database records.
        /// </summary>
        /// <returns>An enumerator for database records.</returns>
        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            // Start from the first entry
            if (!First())
                yield break;

            do
            {
                var current = GetCurrent();
                if (current.HasValue)
                    yield return current.Value;
                else
                    break;
            }
            while (Next());
        }

        /// <summary>
        /// Returns an enumerator that iterates through the database records.
        /// </summary>
        /// <returns>An enumerator for database records.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // 1. Liberar recursos NO administrados (Punteros C)
                // Esto se debe hacer SIEMPRE, sea disposing true o false
                if (_cursorHandle != IntPtr.Zero)
                {
                    NativeMethods.unqlite_kv_cursor_release(_dbHandle, _cursorHandle);
                    _cursorHandle = IntPtr.Zero;
                }

                // 2. Validación de buenas prácticas (Solo Debug)
                if (!disposing)
                {
                    // Loggear a consola de debug, pero NO lanzar excepción
                    System.Diagnostics.Debug.WriteLine("Recurso UnqliteCursor fugado (no se llamó a Dispose)");
                }

                _disposed = true;
            }
        }

        ~UnqliteCursor()
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

    public enum SeekPosition
    {
        Exact,
        LessOrEqual,
        GreaterOrEqual
    }
}
