using System;

namespace UnqliteNet
{
    public class UnqliteException : Exception
    {
        public int ErrorCode { get; }

        public UnqliteException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public static void ThrowOnError(int errorCode)
        {
            if (errorCode == NativeMethods.UNQLITE_OK)
                return;

            string message = errorCode switch
            {
                NativeMethods.UNQLITE_NOMEM => "Out of memory",
                NativeMethods.UNQLITE_ABORT => "Operation aborted",
                NativeMethods.UNQLITE_IOERR => "IO error",
                NativeMethods.UNQLITE_CORRUPT => "Database corrupted",
                NativeMethods.UNQLITE_LOCKED => "Database locked",
                NativeMethods.UNQLITE_BUSY => "Database busy",
                NativeMethods.UNQLITE_NOTFOUND => "Key not found",
                NativeMethods.UNQLITE_CANTOPEN => "Cannot open database",
                _ => $"Unknown error (code: {errorCode})"
            };

            throw new UnqliteException(errorCode, message);
        }
    }
}
