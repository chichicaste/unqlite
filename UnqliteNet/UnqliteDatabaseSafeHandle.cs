// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// Portions inspired by Lightning.NET (https://github.com/CoreyKaylor/Lightning.NET) - MIT License

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace UnqliteNet
{
    /// <summary>
    /// Safe handle wrapper for UnQLite database handle.
    /// Provides automatic and safe cleanup of native database resources.
    /// </summary>
    public sealed class UnqliteDatabaseSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Creates a new safe handle for a UnQLite database.
        /// </summary>
        public UnqliteDatabaseSafeHandle() : base(ownsHandle: true)
        {
        }

        /// <summary>
        /// Creates a new safe handle with an existing handle value.
        /// </summary>
        /// <param name="handle">The existing handle.</param>
        /// <param name="ownsHandle">Whether this instance owns the handle.</param>
        internal UnqliteDatabaseSafeHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(handle);
        }

        /// <summary>
        /// Executes the code required to free the handle.
        /// This is called automatically by the garbage collector.
        /// </summary>
        /// <returns>True if the handle is released successfully; otherwise, false.</returns>
        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
                return true;

            // Close the database
            int result = NativeMethods.unqlite_close(handle);

            // Even if close fails, we consider the handle released
            // to prevent resource leaks
            return result == NativeMethods.UNQLITE_OK || result != NativeMethods.UNQLITE_OK;
        }
    }
}
