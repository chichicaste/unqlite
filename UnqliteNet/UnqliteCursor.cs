using System;
using System.Text;

namespace UnqliteNet
{
    public class UnqliteCursor : IDisposable
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

        public bool First()
        {
            int rc = NativeMethods.unqlite_kv_cursor_first_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        public bool Last()
        {
            int rc = NativeMethods.unqlite_kv_cursor_last_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        public bool IsValid()
        {
            int rc = NativeMethods.unqlite_kv_cursor_valid_entry(_cursorHandle);
            return rc != 0;
        }

        public bool Next()
        {
            int rc = NativeMethods.unqlite_kv_cursor_next_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        public bool Previous()
        {
            int rc = NativeMethods.unqlite_kv_cursor_prev_entry(_cursorHandle);
            return rc == NativeMethods.UNQLITE_OK;
        }

        public string Key
        {
            get
            {
                int keyLen = 0;
                int rc = NativeMethods.unqlite_kv_cursor_key(_cursorHandle, null, ref keyLen);
                UnqliteException.ThrowOnError(rc);

                if (keyLen == 0)
                    return string.Empty;

                byte[] keyBytes = new byte[keyLen];
                rc = NativeMethods.unqlite_kv_cursor_key(_cursorHandle, keyBytes, ref keyLen);
                UnqliteException.ThrowOnError(rc);

                return Encoding.UTF8.GetString(keyBytes);
            }
        }

        public byte[] Data
        {
            get
            {
                long dataLen = 0;
                int rc = NativeMethods.unqlite_kv_cursor_data(_cursorHandle, null, ref dataLen);
                UnqliteException.ThrowOnError(rc);

                if (dataLen == 0)
                    return Array.Empty<byte>();

                byte[] data = new byte[dataLen];
                rc = NativeMethods.unqlite_kv_cursor_data(_cursorHandle, data, ref dataLen);
                UnqliteException.ThrowOnError(rc);

                return data;
            }
        }

        public string DataAsString
        {
            get
            {
                byte[] data = Data;
                return data.Length == 0 ? string.Empty : Encoding.UTF8.GetString(data);
            }
        }

        public void Delete()
        {
            int rc = NativeMethods.unqlite_kv_cursor_delete_entry(_cursorHandle);
            UnqliteException.ThrowOnError(rc);
        }

        public void Reset()
        {
            int rc = NativeMethods.unqlite_kv_cursor_reset(_cursorHandle);
            UnqliteException.ThrowOnError(rc);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_cursorHandle != IntPtr.Zero)
                {
                    NativeMethods.unqlite_kv_cursor_release(_dbHandle, _cursorHandle);
                    _cursorHandle = IntPtr.Zero;
                }
                _disposed = true;
            }
        }

        ~UnqliteCursor()
        {
            Dispose(false);
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
