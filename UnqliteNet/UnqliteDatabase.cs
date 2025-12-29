// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// Portions inspired by Lightning.NET (https://github.com/CoreyKaylor/Lightning.NET) - MIT License

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace UnqliteNet
{
    public class UnqliteDatabase : IDisposable
    {
        private readonly UnqliteDatabaseSafeHandle _dbHandle;

        /// <summary>
        /// Gets the native database handle (for internal use).
        /// </summary>
        internal IntPtr Handle => _dbHandle.DangerousGetHandle();

        public string FilePath { get; }

        /// <summary>
        /// Opens a UnQLite database with simple configuration options.
        /// </summary>
        /// <param name="filePath">Path to the database file, or null for in-memory database.</param>
        /// <param name="readOnly">Open in read-only mode.</param>
        /// <param name="createIfNotExists">Create the database if it doesn't exist.</param>
        /// <param name="inMemory">Create an in-memory database.</param>
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

            IntPtr handle;
            int rc = NativeMethods.unqlite_open(out handle, filePath ?? ":mem:", mode);
            UnqliteException.ThrowOnError(rc);

            _dbHandle = new UnqliteDatabaseSafeHandle(handle, ownsHandle: true);
        }

        /// <summary>
        /// Opens a UnQLite database using fluent configuration.
        /// </summary>
        /// <param name="filePath">Path to the database file, or null for in-memory database.</param>
        /// <param name="configuration">The database configuration.</param>
        public UnqliteDatabase(string filePath, DatabaseConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            FilePath = filePath;

            IntPtr handle;
            int rc = NativeMethods.unqlite_open(out handle, filePath ?? ":mem:", configuration.OpenFlags);
            UnqliteException.ThrowOnError(rc);

            _dbHandle = new UnqliteDatabaseSafeHandle(handle, ownsHandle: true);

            // Apply additional configuration settings
            configuration.ApplyTo(Handle);
        }

        public static string Version
        {
            get
            {
                IntPtr versionPtr = NativeMethods.unqlite_lib_version();
                return Marshal.PtrToStringAnsi(versionPtr) ?? string.Empty;
            }
        }

        public static string Signature
        {
            get
            {
                IntPtr sigPtr = NativeMethods.unqlite_lib_signature();
                return Marshal.PtrToStringAnsi(sigPtr) ?? string.Empty;
            }
        }

        public static string Identifier
        {
            get
            {
                IntPtr identPtr = NativeMethods.unqlite_lib_ident();
                return Marshal.PtrToStringAnsi(identPtr) ?? string.Empty;
            }
        }

        public static string Copyright
        {
            get
            {
                IntPtr copyrightPtr = NativeMethods.unqlite_lib_copyright();
                return Marshal.PtrToStringAnsi(copyrightPtr) ?? string.Empty;
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
            int rc = NativeMethods.unqlite_kv_store(Handle, keyBytes, keyBytes.Length, data, data.LongLength);
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
            int rc = NativeMethods.unqlite_kv_append(Handle, keyBytes, keyBytes.Length, data, data.LongLength);
            UnqliteException.ThrowOnError(rc);
        }

        public byte[]? Fetch(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            long dataLen = 0;

            int rc = NativeMethods.unqlite_kv_fetch(Handle, keyBytes, keyBytes.Length, null!, ref dataLen);

            if (rc == NativeMethods.UNQLITE_NOTFOUND)
                return null;

            UnqliteException.ThrowOnError(rc);

            if (dataLen == 0)
                return Array.Empty<byte>();

            byte[] data = new byte[dataLen];
            rc = NativeMethods.unqlite_kv_fetch(Handle, keyBytes, keyBytes.Length, data, ref dataLen);
            UnqliteException.ThrowOnError(rc);

            return data;
        }

        public string? FetchString(string key)
        {
            byte[]? data = Fetch(key);
            return data == null ? null : Encoding.UTF8.GetString(data);
        }

        public bool Contains(string key)
        {
            return Fetch(key) != null;
        }

        public void Delete(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int rc = NativeMethods.unqlite_kv_delete(Handle, keyBytes, keyBytes.Length);
            UnqliteException.ThrowOnError(rc);
        }

        public UnqliteTransaction BeginTransaction()
        {
            return new UnqliteTransaction(Handle);
        }

        public UnqliteCursor CreateCursor()
        {
            return new UnqliteCursor(Handle);
        }

        public IEnumerable<KeyValuePair<string, byte[]>> GetAll()
        {
            using var cursor = CreateCursor();
            if (!cursor.First())
                yield break;

            do
            {
                if (cursor.Data != null)
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
            int rc = NativeMethods.unqlite_compile(Handle, jx9Script, jx9Script.Length, out IntPtr vmHandle);
            UnqliteException.ThrowOnError(rc);
            return new UnqliteVm(vmHandle);
        }

        public UnqliteVm CompileFile(string filePath)
        {
            int rc = NativeMethods.unqlite_compile_file(Handle, filePath, out IntPtr vmHandle);
            UnqliteException.ThrowOnError(rc);
            return new UnqliteVm(vmHandle);
        }

        // Configuration methods
        public void SetMaxPageCache(int maxPages)
        {
            int rc = NativeMethods.unqlite_config(Handle, NativeMethods.UNQLITE_CONFIG_MAX_PAGE_CACHE, __arglist(maxPages));
            UnqliteException.ThrowOnError(rc);
        }

        public void DisableAutoCommit()
        {
            int rc = NativeMethods.unqlite_config(Handle, NativeMethods.UNQLITE_CONFIG_DISABLE_AUTO_COMMIT, __arglist());
            UnqliteException.ThrowOnError(rc);
        }

        public string GetKvEngineName()
        {
            IntPtr namePtr = IntPtr.Zero;
            int rc = NativeMethods.unqlite_config(Handle, NativeMethods.UNQLITE_CONFIG_GET_KV_NAME, __arglist(ref namePtr));
            UnqliteException.ThrowOnError(rc);

            if (namePtr == IntPtr.Zero)
                return string.Empty;

            return Marshal.PtrToStringAnsi(namePtr) ?? string.Empty;
        }

        public string GetErrorLog()
        {
            IntPtr logPtr = IntPtr.Zero;
            int logLen = 0;
            int rc = NativeMethods.unqlite_config(Handle, NativeMethods.UNQLITE_CONFIG_ERR_LOG, __arglist(ref logPtr, ref logLen));
            UnqliteException.ThrowOnError(rc);

            if (logPtr == IntPtr.Zero || logLen == 0)
                return string.Empty;

            return Marshal.PtrToStringAnsi(logPtr, logLen) ?? string.Empty;
        }

        public string GetJx9ErrorLog()
        {
            IntPtr logPtr = IntPtr.Zero;
            int logLen = 0;
            int rc = NativeMethods.unqlite_config(Handle, NativeMethods.UNQLITE_CONFIG_JX9_ERR_LOG, __arglist(ref logPtr, ref logLen));
            UnqliteException.ThrowOnError(rc);

            if (logPtr == IntPtr.Zero || logLen == 0)
                return string.Empty;

            return Marshal.PtrToStringAnsi(logPtr, logLen) ?? string.Empty;
        }

        // Utility methods
        public string GenerateRandomString(int length)
        {
            byte[] buffer = new byte[length];
            int rc = NativeMethods.unqlite_util_random_string(Handle, buffer, (uint)length);
            UnqliteException.ThrowOnError(rc);
            return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        }

        public uint GenerateRandomNumber()
        {
            return NativeMethods.unqlite_util_random_num(Handle);
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

            int rc = NativeMethods.unqlite_kv_fetch_callback(Handle, keyBytes, keyBytes.Length, consumer, IntPtr.Zero);

            if (rc == NativeMethods.UNQLITE_NOTFOUND)
                return;

            UnqliteException.ThrowOnError(rc);
        }

        // Span-based API for zero-copy operations
        /// <summary>
        /// Stores a key-value pair in the database using zero-copy Span API.
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <param name="data">The data to store as a byte span.</param>
        public unsafe void Store(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
        {
            fixed (byte* keyPtr = key)
            fixed (byte* dataPtr = data)
            {
                int rc = NativeMethods.unqlite_kv_store(Handle, keyPtr, key.Length, dataPtr, data.Length);
                UnqliteException.ThrowOnError(rc);
            }
        }

        /// <summary>
        /// Appends data to an existing value in the database using zero-copy Span API.
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <param name="data">The data to append as a byte span.</param>
        public unsafe void Append(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
        {
            fixed (byte* keyPtr = key)
            fixed (byte* dataPtr = data)
            {
                int rc = NativeMethods.unqlite_kv_append(Handle, keyPtr, key.Length, dataPtr, data.Length);
                UnqliteException.ThrowOnError(rc);
            }
        }

        /// <summary>
        /// Deletes a key-value pair from the database using zero-copy Span API.
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        public unsafe void Delete(ReadOnlySpan<byte> key)
        {
            fixed (byte* keyPtr = key)
            {
                int rc = NativeMethods.unqlite_kv_delete(Handle, keyPtr, key.Length);
                UnqliteException.ThrowOnError(rc);
            }
        }

        /// <summary>
        /// Fetches data from the database into a pre-allocated buffer using zero-copy Span API.
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <returns>The number of bytes written to the buffer, or -1 if the key was not found.</returns>
        public unsafe int TryFetch(ReadOnlySpan<byte> key, Span<byte> buffer)
        {
            long dataLen = buffer.Length;

            fixed (byte* keyPtr = key)
            fixed (byte* bufferPtr = buffer)
            {
                int rc = NativeMethods.unqlite_kv_fetch(Handle, keyPtr, key.Length, bufferPtr, ref dataLen);

                if (rc == NativeMethods.UNQLITE_NOTFOUND)
                    return -1;

                UnqliteException.ThrowOnError(rc);
                return (int)dataLen;
            }
        }

        /// <summary>
        /// Fetches data from the database and returns it as a byte array using zero-copy Span API for the key.
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <returns>The data as a byte array, or null if the key was not found.</returns>
        public unsafe byte[]? Fetch(ReadOnlySpan<byte> key)
        {
            long dataLen = 0;

            fixed (byte* keyPtr = key)
            {
                int rc = NativeMethods.unqlite_kv_fetch(Handle, keyPtr, key.Length, null, ref dataLen);

                if (rc == NativeMethods.UNQLITE_NOTFOUND)
                    return null;

                UnqliteException.ThrowOnError(rc);

                if (dataLen == 0)
                    return Array.Empty<byte>();

                byte[] data = new byte[dataLen];
                fixed (byte* dataPtr = data)
                {
                    rc = NativeMethods.unqlite_kv_fetch(Handle, keyPtr, key.Length, dataPtr, ref dataLen);
                    UnqliteException.ThrowOnError(rc);
                }

                return data;
            }
        }

        /// <summary>
        /// Checks if a key exists in the database using zero-copy Span API.
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        public bool Contains(ReadOnlySpan<byte> key)
        {
            return Fetch(key) != null;
        }

        // Try-Get pattern methods (no exceptions on expected failures)
        /// <summary>
        /// Attempts to fetch data from the database without throwing exceptions.
        /// </summary>
        /// <param name="key">The key to fetch.</param>
        /// <param name="data">The fetched data, or null if not found.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool TryFetch(string key, out byte[]? data)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            long dataLen = 0;

            int rc = NativeMethods.unqlite_kv_fetch(Handle, keyBytes, keyBytes.Length, null!, ref dataLen);

            if (rc == NativeMethods.UNQLITE_NOTFOUND)
            {
                data = null;
                return false;
            }

            if (rc != NativeMethods.UNQLITE_OK)
            {
                data = null;
                return false;
            }

            if (dataLen == 0)
            {
                data = Array.Empty<byte>();
                return true;
            }

            data = new byte[dataLen];
            rc = NativeMethods.unqlite_kv_fetch(Handle, keyBytes, keyBytes.Length, data, ref dataLen);

            if (rc != NativeMethods.UNQLITE_OK)
            {
                data = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to store data in the database without throwing exceptions.
        /// </summary>
        /// <param name="key">The key to store.</param>
        /// <param name="data">The data to store.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool TryStore(string key, byte[] data)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int rc = NativeMethods.unqlite_kv_store(Handle, keyBytes, keyBytes.Length, data, data.LongLength);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Attempts to store data in the database without throwing exceptions (Span version).
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <param name="data">The data to store as a byte span.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public unsafe bool TryStore(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
        {
            fixed (byte* keyPtr = key)
            fixed (byte* dataPtr = data)
            {
                int rc = NativeMethods.unqlite_kv_store(Handle, keyPtr, key.Length, dataPtr, data.Length);
                return rc == NativeMethods.UNQLITE_OK;
            }
        }

        /// <summary>
        /// Attempts to append data to an existing value without throwing exceptions.
        /// </summary>
        /// <param name="key">The key to append to.</param>
        /// <param name="data">The data to append.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool TryAppend(string key, byte[] data)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int rc = NativeMethods.unqlite_kv_append(Handle, keyBytes, keyBytes.Length, data, data.LongLength);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Attempts to append data to an existing value without throwing exceptions (Span version).
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <param name="data">The data to append as a byte span.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public unsafe bool TryAppend(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
        {
            fixed (byte* keyPtr = key)
            fixed (byte* dataPtr = data)
            {
                int rc = NativeMethods.unqlite_kv_append(Handle, keyPtr, key.Length, dataPtr, data.Length);
                return rc == NativeMethods.UNQLITE_OK;
            }
        }

        /// <summary>
        /// Attempts to delete a key-value pair without throwing exceptions.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool TryDelete(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int rc = NativeMethods.unqlite_kv_delete(Handle, keyBytes, keyBytes.Length);
            return rc == NativeMethods.UNQLITE_OK;
        }

        /// <summary>
        /// Attempts to delete a key-value pair without throwing exceptions (Span version).
        /// </summary>
        /// <param name="key">The key as a byte span.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public unsafe bool TryDelete(ReadOnlySpan<byte> key)
        {
            fixed (byte* keyPtr = key)
            {
                int rc = NativeMethods.unqlite_kv_delete(Handle, keyPtr, key.Length);
                return rc == NativeMethods.UNQLITE_OK;
            }
        }

        /// <summary>
        /// Releases the resources used by the database.
        /// </summary>
        public void Dispose()
        {
            _dbHandle?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
