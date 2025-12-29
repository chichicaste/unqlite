using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace UnqliteNet
{
    public class UnqliteDatabase : IDisposable
    {
        internal IntPtr _dbHandle;
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
                IntPtr versionPtr = NativeMethods.unqlite_lib_version();
                return Marshal.PtrToStringAnsi(versionPtr);
            }
        }

        public static string Signature
        {
            get
            {
                IntPtr sigPtr = NativeMethods.unqlite_lib_signature();
                return Marshal.PtrToStringAnsi(sigPtr);
            }
        }

        public static string Identifier
        {
            get
            {
                IntPtr identPtr = NativeMethods.unqlite_lib_ident();
                return Marshal.PtrToStringAnsi(identPtr);
            }
        }

        public static string Copyright
        {
            get
            {
                IntPtr copyrightPtr = NativeMethods.unqlite_lib_copyright();
                return Marshal.PtrToStringAnsi(copyrightPtr);
            }
        }

        public static bool IsThreadSafe => NativeMethods.unqlite_lib_is_threadsafe() != 0;

        public static void InitializeLibrary()
        {
            int rc = NativeMethods.unqlite_lib_init();
            UnqliteException.ThrowOnError(rc);
        }

        public static void ShutdownLibrary()
        {
            int rc = NativeMethods.unqlite_lib_shutdown();
            UnqliteException.ThrowOnError(rc);
        }

        public static void SetThreadingModeSingle()
        {
            int rc = NativeMethods.unqlite_lib_config(NativeMethods.UNQLITE_LIB_CONFIG_THREAD_LEVEL_SINGLE, __arglist());
            UnqliteException.ThrowOnError(rc);
        }

        public static void SetThreadingModeMulti()
        {
            int rc = NativeMethods.unqlite_lib_config(NativeMethods.UNQLITE_LIB_CONFIG_THREAD_LEVEL_MULTI, __arglist());
            UnqliteException.ThrowOnError(rc);
        }

        public static void SetGlobalPageSize(int pageSize)
        {
            int rc = NativeMethods.unqlite_lib_config(NativeMethods.UNQLITE_LIB_CONFIG_PAGE_SIZE, __arglist(pageSize));
            UnqliteException.ThrowOnError(rc);
        }

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

        // Jx9 VM compilation and execution
        public UnqliteVm Compile(string jx9Script)
        {
            int rc = NativeMethods.unqlite_compile(_dbHandle, jx9Script, jx9Script.Length, out IntPtr vmHandle);
            UnqliteException.ThrowOnError(rc);
            return new UnqliteVm(vmHandle);
        }

        public UnqliteVm CompileFile(string filePath)
        {
            int rc = NativeMethods.unqlite_compile_file(_dbHandle, filePath, out IntPtr vmHandle);
            UnqliteException.ThrowOnError(rc);
            return new UnqliteVm(vmHandle);
        }

        // Configuration methods
        public void SetMaxPageCache(int maxPages)
        {
            int rc = NativeMethods.unqlite_config(_dbHandle, NativeMethods.UNQLITE_CONFIG_MAX_PAGE_CACHE, __arglist(maxPages));
            UnqliteException.ThrowOnError(rc);
        }

        public void DisableAutoCommit()
        {
            int rc = NativeMethods.unqlite_config(_dbHandle, NativeMethods.UNQLITE_CONFIG_DISABLE_AUTO_COMMIT, __arglist());
            UnqliteException.ThrowOnError(rc);
        }

        public string GetKvEngineName()
        {
            IntPtr namePtr = IntPtr.Zero;
            int rc = NativeMethods.unqlite_config(_dbHandle, NativeMethods.UNQLITE_CONFIG_GET_KV_NAME, __arglist(ref namePtr));
            UnqliteException.ThrowOnError(rc);

            if (namePtr == IntPtr.Zero)
                return string.Empty;

            return Marshal.PtrToStringAnsi(namePtr);
        }

        public string GetErrorLog()
        {
            IntPtr logPtr = IntPtr.Zero;
            int logLen = 0;
            int rc = NativeMethods.unqlite_config(_dbHandle, NativeMethods.UNQLITE_CONFIG_ERR_LOG, __arglist(ref logPtr, ref logLen));
            UnqliteException.ThrowOnError(rc);

            if (logPtr == IntPtr.Zero || logLen == 0)
                return string.Empty;

            return Marshal.PtrToStringAnsi(logPtr, logLen);
        }

        public string GetJx9ErrorLog()
        {
            IntPtr logPtr = IntPtr.Zero;
            int logLen = 0;
            int rc = NativeMethods.unqlite_config(_dbHandle, NativeMethods.UNQLITE_CONFIG_JX9_ERR_LOG, __arglist(ref logPtr, ref logLen));
            UnqliteException.ThrowOnError(rc);

            if (logPtr == IntPtr.Zero || logLen == 0)
                return string.Empty;

            return Marshal.PtrToStringAnsi(logPtr, logLen);
        }

        // Utility methods
        public string GenerateRandomString(int length)
        {
            byte[] buffer = new byte[length];
            int rc = NativeMethods.unqlite_util_random_string(_dbHandle, buffer, (uint)length);
            UnqliteException.ThrowOnError(rc);
            return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        }

        public uint GenerateRandomNumber()
        {
            return NativeMethods.unqlite_util_random_num(_dbHandle);
        }

        public static byte[] LoadMemoryMappedFile(string filePath, out long fileSize)
        {
            int rc = NativeMethods.unqlite_util_load_mmaped_file(filePath, out IntPtr mapPtr, out fileSize);
            UnqliteException.ThrowOnError(rc);

            if (mapPtr == IntPtr.Zero || fileSize == 0)
                return Array.Empty<byte>();

            byte[] data = new byte[fileSize];
            Marshal.Copy(mapPtr, data, 0, (int)fileSize);

            NativeMethods.unqlite_util_release_mmaped_file(mapPtr, fileSize);
            return data;
        }

        // Callback-based fetch for better performance with large data
        public void FetchCallback(string key, Action<byte[]> callback)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            var consumer = new NativeMethods.UnqliteDataConsumer((pData, nDataLen, pUserData) =>
            {
                if (pData == IntPtr.Zero || nDataLen == 0)
                    return 0;

                byte[] data = new byte[nDataLen];
                Marshal.Copy(pData, data, 0, (int)nDataLen);
                callback(data);
                return 0;
            });

            int rc = NativeMethods.unqlite_kv_fetch_callback(_dbHandle, keyBytes, keyBytes.Length, consumer, IntPtr.Zero);

            if (rc == NativeMethods.UNQLITE_NOTFOUND)
                return;

            UnqliteException.ThrowOnError(rc);
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
