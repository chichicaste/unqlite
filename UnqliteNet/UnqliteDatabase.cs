using System;
using System.Text;
using System.Collections.Generic;

namespace UnqliteNet
{
    public class UnqliteDatabase : IDisposable
    {
        private IntPtr _dbHandle;
        private bool _disposed;

        public string FilePath { get; }

        public UnqliteDatabase(string filePath, bool readOnly = false, bool createIfNotExists = true, bool inMemory = false)
        {
            FilePath = filePath;

            uint mode = 0;
            if (readOnly)
                mode |= NativeMethods.UNQLITE_OPEN_READONLY;
            else
                mode |= NativeMethods.UNQLITE_OPEN_READWRITE;

            if (createIfNotExists)
                mode |= NativeMethods.UNQLITE_OPEN_CREATE;

            if (inMemory)
                mode |= NativeMethods.UNQLITE_OPEN_IN_MEMORY;

            int rc = NativeMethods.unqlite_open(out _dbHandle, filePath ?? ":mem:", mode);
            UnqliteException.ThrowOnError(rc);
        }

        public static string Version
        {
            get
            {
                var sb = new StringBuilder(32);
                NativeMethods.unqlite_lib_version(sb, sb.Capacity);
                return sb.ToString();
            }
        }

        public static bool IsThreadSafe => NativeMethods.unqlite_lib_is_threadsafe() != 0;

        public void Store(string key, byte[] data)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int rc = NativeMethods.unqlite_kv_store(_dbHandle, keyBytes, keyBytes.Length, data, data.LongLength);
            UnqliteException.ThrowOnError(rc);
        }

        public void Store(string key, string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            Store(key, dataBytes);
        }

        public void Append(string key, byte[] data)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int rc = NativeMethods.unqlite_kv_append(_dbHandle, keyBytes, keyBytes.Length, data, data.LongLength);
            UnqliteException.ThrowOnError(rc);
        }

        public byte[] Fetch(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            long dataLen = 0;

            int rc = NativeMethods.unqlite_kv_fetch(_dbHandle, keyBytes, keyBytes.Length, null, ref dataLen);
            
            if (rc == NativeMethods.UNQLITE_NOTFOUND)
                return null;

            UnqliteException.ThrowOnError(rc);

            if (dataLen == 0)
                return Array.Empty<byte>();

            byte[] data = new byte[dataLen];
            rc = NativeMethods.unqlite_kv_fetch(_dbHandle, keyBytes, keyBytes.Length, data, ref dataLen);
            UnqliteException.ThrowOnError(rc);

            return data;
        }

        public string FetchString(string key)
        {
            byte[] data = Fetch(key);
            return data == null ? null : Encoding.UTF8.GetString(data);
        }

        public bool Contains(string key)
        {
            return Fetch(key) != null;
        }

        public void Delete(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int rc = NativeMethods.unqlite_kv_delete(_dbHandle, keyBytes, keyBytes.Length);
            UnqliteException.ThrowOnError(rc);
        }

        public UnqliteTransaction BeginTransaction()
        {
            return new UnqliteTransaction(_dbHandle);
        }

        public UnqliteCursor CreateCursor()
        {
            return new UnqliteCursor(_dbHandle);
        }

        public IEnumerable<KeyValuePair<string, byte[]>> GetAll()
        {
            using var cursor = CreateCursor();
            if (!cursor.First())
                yield break;

            do
            {
                yield return new KeyValuePair<string, byte[]>(cursor.Key, cursor.Data);
            } while (cursor.Next());
        }

        public IEnumerable<string> GetAllKeys()
        {
            using var cursor = CreateCursor();
            if (!cursor.First())
                yield break;

            do
            {
                yield return cursor.Key;
            } while (cursor.Next());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                if (_dbHandle != IntPtr.Zero)
                {
                    NativeMethods.unqlite_close(_dbHandle);
                    _dbHandle = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        ~UnqliteDatabase()
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
