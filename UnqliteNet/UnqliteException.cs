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
                NativeMethods.UNQLITE_DONE => "Operation done",
                NativeMethods.UNQLITE_PERM => "Permission denied",
                NativeMethods.UNQLITE_NOTIMPLEMENTED => "Method not implemented",
                NativeMethods.UNQLITE_NOTFOUND => "Key not found",
                NativeMethods.UNQLITE_NOOP => "No-op operation",
                NativeMethods.UNQLITE_INVALID => "Invalid parameter",
                NativeMethods.UNQLITE_EOF => "End of input",
                NativeMethods.UNQLITE_UNKNOWN => "Unknown configuration option",
                NativeMethods.UNQLITE_LIMIT => "Database limit reached",
                NativeMethods.UNQLITE_EXISTS => "Record already exists",
                NativeMethods.UNQLITE_EMPTY => "Empty record",
                NativeMethods.UNQLITE_COMPILE_ERR => "Jx9 compilation error",
                NativeMethods.UNQLITE_VM_ERR => "Virtual machine error",
                NativeMethods.UNQLITE_FULL => "Database full",
                NativeMethods.UNQLITE_CANTOPEN => "Cannot open database",
                NativeMethods.UNQLITE_READ_ONLY => "Read-only database",
                NativeMethods.UNQLITE_LOCKERR => "Locking protocol error",
                _ => $"Unknown error (code: {errorCode})"
            };

            throw new UnqliteException(errorCode, message);
        }
    }
}
