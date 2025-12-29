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
        public const int UNQLITE_ABORT = -10;
        public const int UNQLITE_IOERR = -2;
        public const int UNQLITE_CORRUPT = -24;
        public const int UNQLITE_LOCKED = -4;
        public const int UNQLITE_BUSY = -14;
        public const int UNQLITE_DONE = -28;
        public const int UNQLITE_PERM = -19;
        public const int UNQLITE_NOTIMPLEMENTED = -17;
        public const int UNQLITE_NOTFOUND = -6;
        public const int UNQLITE_NOOP = -20;
        public const int UNQLITE_INVALID = -9;
        public const int UNQLITE_EOF = -18;
        public const int UNQLITE_UNKNOWN = -13;
        public const int UNQLITE_LIMIT = -7;
        public const int UNQLITE_EXISTS = -11;
        public const int UNQLITE_EMPTY = -3;
        public const int UNQLITE_COMPILE_ERR = -70;
        public const int UNQLITE_VM_ERR = -71;
        public const int UNQLITE_FULL = -73;
        public const int UNQLITE_CANTOPEN = -74;
        public const int UNQLITE_READ_ONLY = -75;
        public const int UNQLITE_LOCKERR = -76;
        #endregion

        #region Open Flags
        public const uint UNQLITE_OPEN_READONLY = 0x00000001;
        public const uint UNQLITE_OPEN_READWRITE = 0x00000002;
        public const uint UNQLITE_OPEN_CREATE = 0x00000004;
        public const uint UNQLITE_OPEN_EXCLUSIVE = 0x00000008;
        public const uint UNQLITE_OPEN_TEMP_DB = 0x00000010;
        public const uint UNQLITE_OPEN_NOMUTEX = 0x00000020;
        public const uint UNQLITE_OPEN_OMIT_JOURNALING = 0x00000040;
        public const uint UNQLITE_OPEN_IN_MEMORY = 0x00000080;
        public const uint UNQLITE_OPEN_MMAP = 0x00000100;
        #endregion

        #region Cursor Flags
        public const int UNQLITE_CURSOR_MATCH_EXACT = 1;
        public const int UNQLITE_CURSOR_MATCH_LE = 2;
        public const int UNQLITE_CURSOR_MATCH_GE = 3;
        #endregion

        #region Config Verbs
        public const int UNQLITE_CONFIG_JX9_ERR_LOG = 1;
        public const int UNQLITE_CONFIG_MAX_PAGE_CACHE = 2;
        public const int UNQLITE_CONFIG_ERR_LOG = 3;
        public const int UNQLITE_CONFIG_KV_ENGINE = 4;
        public const int UNQLITE_CONFIG_DISABLE_AUTO_COMMIT = 5;
        public const int UNQLITE_CONFIG_GET_KV_NAME = 6;
        #endregion

        #region VM Config Verbs
        public const int UNQLITE_VM_CONFIG_OUTPUT = 1;
        public const int UNQLITE_VM_CONFIG_IMPORT_PATH = 2;
        public const int UNQLITE_VM_CONFIG_ERR_REPORT = 3;
        public const int UNQLITE_VM_CONFIG_RECURSION_DEPTH = 4;
        public const int UNQLITE_VM_OUTPUT_LENGTH = 5;
        public const int UNQLITE_VM_CONFIG_CREATE_VAR = 6;
        public const int UNQLITE_VM_CONFIG_HTTP_REQUEST = 7;
        public const int UNQLITE_VM_CONFIG_SERVER_ATTR = 8;
        public const int UNQLITE_VM_CONFIG_ENV_ATTR = 9;
        public const int UNQLITE_VM_CONFIG_EXEC_VALUE = 10;
        public const int UNQLITE_VM_CONFIG_IO_STREAM = 11;
        public const int UNQLITE_VM_CONFIG_ARGV_ENTRY = 12;
        public const int UNQLITE_VM_CONFIG_EXTRACT_OUTPUT = 13;
        #endregion

        #region Context Error Levels
        public const int UNQLITE_CTX_ERR = 1;
        public const int UNQLITE_CTX_WARNING = 2;
        public const int UNQLITE_CTX_NOTICE = 3;
        #endregion

        #region Library Config Verbs
        public const int UNQLITE_LIB_CONFIG_USER_MALLOC = 1;
        public const int UNQLITE_LIB_CONFIG_MEM_ERR_CALLBACK = 2;
        public const int UNQLITE_LIB_CONFIG_USER_MUTEX = 3;
        public const int UNQLITE_LIB_CONFIG_THREAD_LEVEL_SINGLE = 4;
        public const int UNQLITE_LIB_CONFIG_THREAD_LEVEL_MULTI = 5;
        public const int UNQLITE_LIB_CONFIG_VFS = 6;
        public const int UNQLITE_LIB_CONFIG_STORAGE_ENGINE = 7;
        public const int UNQLITE_LIB_CONFIG_PAGE_SIZE = 8;
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

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_lib_version();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_lib_signature();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_lib_ident();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_lib_copyright();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_lib_is_threadsafe();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_lib_init();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_lib_shutdown();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_lib_config(int nConfigOp, __arglist);

        // Delegates for callbacks
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int UnqliteDataConsumer(IntPtr pData, uint nDataLen, IntPtr pUserData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int UnqliteOutputConsumer(IntPtr pOutput, uint nLen, IntPtr pUserData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int UnqliteForeignFunction(IntPtr pCtx, int argc, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] argv);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UnqliteConstantExpand(IntPtr pValue, IntPtr pUserData);

        // Config functions
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_config(IntPtr pDb, int nOp, __arglist);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_config(IntPtr pDb, int iOp, __arglist);

        // Formatted store/append
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_kv_store_fmt(IntPtr pDb, byte[] pKey, int nKeyLen, string zFormat, __arglist);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_kv_append_fmt(IntPtr pDb, byte[] pKey, int nKeyLen, string zFormat, __arglist);

        // Callback versions
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_fetch_callback(IntPtr pDb, byte[] pKey, int nKeyLen, UnqliteDataConsumer xConsumer, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_key_callback(IntPtr pCursor, UnqliteDataConsumer xConsumer, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_kv_cursor_data_callback(IntPtr pCursor, UnqliteDataConsumer xConsumer, IntPtr pUserData);

        // Utility functions
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_util_load_mmaped_file(string zFile, out IntPtr ppMap, out long pFileSize);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_util_release_mmaped_file(IntPtr pMap, long iFileSize);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_util_random_string(IntPtr pDb, byte[] zBuf, uint buf_size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint unqlite_util_random_num(IntPtr pDb);

        // VM functions
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_compile(IntPtr pDb, string zJx9, int nByte, out IntPtr ppOut);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_compile_file(IntPtr pDb, string zPath, out IntPtr ppOut);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_vm_config(IntPtr pVm, int iOp, __arglist);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_vm_exec(IntPtr pVm);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_vm_reset(IntPtr pVm);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_vm_release(IntPtr pVm);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_vm_dump(IntPtr pVm, UnqliteDataConsumer xConsumer, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr unqlite_vm_extract_variable(IntPtr pVm, string zVarname);

        // Foreign functions
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_create_function(IntPtr pVm, string zName, UnqliteForeignFunction xFunc, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_delete_function(IntPtr pVm, string zName);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_create_constant(IntPtr pVm, string zName, UnqliteConstantExpand xExpand, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_delete_constant(IntPtr pVm, string zName);

        // Value allocation
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_vm_new_scalar(IntPtr pVm);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_vm_new_array(IntPtr pVm);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_vm_release_value(IntPtr pVm, IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_context_new_scalar(IntPtr pCtx);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_context_new_array(IntPtr pCtx);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void unqlite_context_release_value(IntPtr pCtx, IntPtr pValue);

        // Value setters
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_int(IntPtr pVal, int iValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_int64(IntPtr pVal, long iValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_bool(IntPtr pVal, int iBool);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_null(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_double(IntPtr pVal, double Value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_string(IntPtr pVal, byte[] zString, int nLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_value_string_format(IntPtr pVal, string zFormat, __arglist);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_reset_string_cursor(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_resource(IntPtr pVal, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_release(IntPtr pVal);

        // Value getters
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_to_int(IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_to_bool(IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern long unqlite_value_to_int64(IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double unqlite_value_to_double(IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_value_to_string(IntPtr pValue, out int pLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_value_to_resource(IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_compare(IntPtr pLeft, IntPtr pRight, int bStrict);

        // Value type checks
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_int(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_float(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_bool(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_string(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_null(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_numeric(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_callable(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_scalar(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_json_array(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_json_object(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_resource(IntPtr pVal);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_value_is_empty(IntPtr pVal);

        // Array management
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_array_fetch(IntPtr pArray, byte[] zKey, int nByte);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_array_walk(IntPtr pArray, IntPtr xWalk, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_array_add_elem(IntPtr pArray, IntPtr pKey, IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_array_add_strkey_elem(IntPtr pArray, string zKey, IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_array_count(IntPtr pArray);

        // Context functions
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_context_output(IntPtr pCtx, byte[] zString, int nLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_context_output_format(IntPtr pCtx, string zFormat, __arglist);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_context_throw_error(IntPtr pCtx, int iErr, string zErr);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_context_throw_error_format(IntPtr pCtx, int iErr, string zFormat, __arglist);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint unqlite_context_random_num(IntPtr pCtx);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_context_random_string(IntPtr pCtx, byte[] zBuf, int nBuflen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_context_user_data(IntPtr pCtx);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_context_push_aux_data(IntPtr pCtx, IntPtr pUserData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_context_peek_aux_data(IntPtr pCtx);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint unqlite_context_result_buf_length(IntPtr pCtx);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_function_name(IntPtr pCtx);

        // Context memory management
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_context_alloc_chunk(IntPtr pCtx, uint nByte, int ZeroChunk, int AutoRelease);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr unqlite_context_realloc_chunk(IntPtr pCtx, IntPtr pChunk, uint nByte);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void unqlite_context_free_chunk(IntPtr pCtx, IntPtr pChunk);

        // Result setters
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_int(IntPtr pCtx, int iValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_int64(IntPtr pCtx, long iValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_bool(IntPtr pCtx, int iBool);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_double(IntPtr pCtx, double Value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_null(IntPtr pCtx);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_string(IntPtr pCtx, byte[] zString, int nLen);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int unqlite_result_string_format(IntPtr pCtx, string zFormat, __arglist);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_value(IntPtr pCtx, IntPtr pValue);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unqlite_result_resource(IntPtr pCtx, IntPtr pUserData);
    }
}
