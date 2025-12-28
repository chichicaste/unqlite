using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UnqliteNet
{
    public static class NativeMethods
    {
        private const string LibraryName = "unqlite";

        #region Error Codes
        public const int UNQLITE_OK = 0;
        public const int UNQLITE_NOMEM = -1;
        public const int UNQLITE_ABORT = -2;
        public const int UNQLITE_IOERR = -3;
        public const int UNQLITE_CORRUPT = -4;
        public const int UNQLITE_LOCKED = -5;
        public const int UNQLITE_BUSY = -6;
        public const int UNQLITE_NOTFOUND = -14;
        public const int UNQLITE_CANTOPEN = -74;
        #endregion

        #region Open Flags
        public const uint UNQLITE_OPEN_READONLY = 0x00000001;
        public const uint UNQLITE_OPEN_READWRITE = 0x00000002;
        public const uint UNQLITE_OPEN_CREATE = 0x00000004;
        public const uint UNQLITE_OPEN_IN_MEMORY = 0x00000080;
        #endregion

        #region Cursor Flags
        public const int UNQLITE_CURSOR_MATCH_EXACT = 1;
        public const int UNQLITE_CURSOR_MATCH_LE = 2;
        public const int UNQLITE_CURSOR_MATCH_GE = 3;
        #endregion

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_open(out IntPtr ppDB, string zFilename, uint iMode);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_close(IntPtr pDb);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_store(IntPtr pDb, byte[] pKey, int nKeyLen, byte[] pData, long nDataLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_append(IntPtr pDb, byte[] pKey, int nKeyLen, byte[] pData, long nDataLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_fetch(IntPtr pDb, byte[] pKey, int nKeyLen, byte[] pBuf, ref long pBufLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_delete(IntPtr pDb, byte[] pKey, int nKeyLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_begin(IntPtr pDb);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_commit(IntPtr pDb);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_rollback(IntPtr pDb);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_init(IntPtr pDb, out IntPtr ppOut);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_release(IntPtr pDb, IntPtr pCur);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_seek(IntPtr pCursor, byte[] pKey, int nKeyLen, int iPos);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_first_entry(IntPtr pCursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_last_entry(IntPtr pCursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_valid_entry(IntPtr pCursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_next_entry(IntPtr pCursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_prev_entry(IntPtr pCursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_key(IntPtr pCursor, byte[] pBuf, ref int pnByte);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_data(IntPtr pCursor, byte[] pBuf, ref long pnData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_delete_entry(IntPtr pCursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_reset(IntPtr pCursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_lib_version(StringBuilder zBuf, int nBufSize);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_lib_is_threadsafe();
    }
}
