// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// Portions inspired by Lightning.NET (https://github.com/CoreyKaylor/Lightning.NET) - MIT License

using System;
using System.Runtime.CompilerServices;

namespace UnqliteNet
{
    /// <summary>
    /// Result code wrapper for fluent error handling.
    /// </summary>
    public readonly struct UnqliteResult
    {
        /// <summary>
        /// The result code from the native operation.
        /// </summary>
        public readonly int ResultCode;

        /// <summary>
        /// Creates a new result wrapper.
        /// </summary>
        /// <param name="resultCode">The native result code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnqliteResult(int resultCode)
        {
            ResultCode = resultCode;
        }

        /// <summary>
        /// Implicit conversion from int to UnqliteResult.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnqliteResult(int resultCode) => new(resultCode);

        /// <summary>
        /// Implicit conversion from UnqliteResult to int.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(UnqliteResult result) => result.ResultCode;

        /// <summary>
        /// Returns true if the operation succeeded.
        /// </summary>
        public bool IsSuccess => ResultCode == NativeMethods.UNQLITE_OK;

        /// <summary>
        /// Returns true if the operation failed with NOTFOUND error.
        /// </summary>
        public bool IsNotFound => ResultCode == NativeMethods.UNQLITE_NOTFOUND;

        /// <summary>
        /// Returns true if the operation failed with EOF error.
        /// </summary>
        public bool IsEof => ResultCode == NativeMethods.UNQLITE_EOF;

        /// <summary>
        /// Returns true if the operation was a no-op.
        /// </summary>
        public bool IsNoop => ResultCode == NativeMethods.UNQLITE_NOOP;
    }

    /// <summary>
    /// Extension methods for fluent error handling and convenience operations.
    /// </summary>
    public static class UnqliteExtensions
    {
        /// <summary>
        /// Throws an exception if the result code indicates an error.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <returns>The same result for chaining.</returns>
        /// <exception cref="UnqliteException">Thrown when the result indicates an error.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnqliteResult ThrowOnError(this UnqliteResult result)
        {
            if (result.ResultCode != NativeMethods.UNQLITE_OK)
            {
                UnqliteException.ThrowOnError(result.ResultCode);
            }
            return result;
        }

        /// <summary>
        /// Throws an exception if the result code indicates an error.
        /// Convenience overload for int result codes.
        /// </summary>
        /// <param name="resultCode">The result code to check.</param>
        /// <returns>A UnqliteResult wrapper for chaining.</returns>
        /// <exception cref="UnqliteException">Thrown when the result indicates an error.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnqliteResult ThrowOnError(this int resultCode)
        {
            return new UnqliteResult(resultCode).ThrowOnError();
        }

        /// <summary>
        /// Throws an exception if the result code indicates an error,
        /// except for NOTFOUND which is treated as a valid read result.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <returns>The same result for chaining.</returns>
        /// <exception cref="UnqliteException">Thrown when the result indicates an error (except NOTFOUND).</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnqliteResult ThrowOnReadError(this UnqliteResult result)
        {
            if (result.ResultCode != NativeMethods.UNQLITE_OK &&
                result.ResultCode != NativeMethods.UNQLITE_NOTFOUND)
            {
                UnqliteException.ThrowOnError(result.ResultCode);
            }
            return result;
        }

        /// <summary>
        /// Throws an exception if the result code indicates an error,
        /// except for NOTFOUND which is treated as a valid read result.
        /// Convenience overload for int result codes.
        /// </summary>
        /// <param name="resultCode">The result code to check.</param>
        /// <returns>A UnqliteResult wrapper for chaining.</returns>
        /// <exception cref="UnqliteException">Thrown when the result indicates an error (except NOTFOUND).</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnqliteResult ThrowOnReadError(this int resultCode)
        {
            return new UnqliteResult(resultCode).ThrowOnReadError();
        }

        /// <summary>
        /// Throws an exception if the result code indicates an error,
        /// except for expected iteration end conditions (EOF, NOTFOUND, NOOP).
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <returns>The same result for chaining.</returns>
        /// <exception cref="UnqliteException">Thrown when the result indicates an unexpected error.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnqliteResult ThrowOnIterationError(this UnqliteResult result)
        {
            if (result.ResultCode != NativeMethods.UNQLITE_OK &&
                result.ResultCode != NativeMethods.UNQLITE_NOTFOUND &&
                result.ResultCode != NativeMethods.UNQLITE_EOF &&
                result.ResultCode != NativeMethods.UNQLITE_NOOP)
            {
                UnqliteException.ThrowOnError(result.ResultCode);
            }
            return result;
        }

        /// <summary>
        /// Throws an exception if the result code indicates an error,
        /// except for expected iteration end conditions (EOF, NOTFOUND, NOOP).
        /// Convenience overload for int result codes.
        /// </summary>
        /// <param name="resultCode">The result code to check.</param>
        /// <returns>A UnqliteResult wrapper for chaining.</returns>
        /// <exception cref="UnqliteException">Thrown when the result indicates an unexpected error.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnqliteResult ThrowOnIterationError(this int resultCode)
        {
            return new UnqliteResult(resultCode).ThrowOnIterationError();
        }

        /// <summary>
        /// Returns true if the result indicates success.
        /// </summary>
        /// <param name="result">The result to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSuccess(this UnqliteResult result) => result.IsSuccess;

        /// <summary>
        /// Returns true if the result indicates success.
        /// Convenience overload for int result codes.
        /// </summary>
        /// <param name="resultCode">The result code to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSuccess(this int resultCode) => resultCode == NativeMethods.UNQLITE_OK;

        /// <summary>
        /// Returns true if the result indicates a not found error.
        /// </summary>
        /// <param name="result">The result to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotFound(this UnqliteResult result) => result.IsNotFound;

        /// <summary>
        /// Returns true if the result indicates a not found error.
        /// Convenience overload for int result codes.
        /// </summary>
        /// <param name="resultCode">The result code to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotFound(this int resultCode) => resultCode == NativeMethods.UNQLITE_NOTFOUND;
    }
}
