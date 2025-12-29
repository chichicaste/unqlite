// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UnqliteNet
{
    public class UnqliteContext
    {
        internal IntPtr Handle { get; }

        internal UnqliteContext(IntPtr handle)
        {
            Handle = handle;
        }

        // Value creation
        public UnqliteValue CreateScalar()
        {
            IntPtr valueHandle = NativeMethods.unqlite_context_new_scalar(Handle);
            if (valueHandle == IntPtr.Zero)
                throw new UnqliteException(NativeMethods.UNQLITE_NOMEM, "Failed to create scalar value");
            return new UnqliteValue(valueHandle, ctxHandle: Handle);
        }

        public UnqliteValue CreateArray()
        {
            IntPtr valueHandle = NativeMethods.unqlite_context_new_array(Handle);
            if (valueHandle == IntPtr.Zero)
                throw new UnqliteException(NativeMethods.UNQLITE_NOMEM, "Failed to create array value");
            return new UnqliteValue(valueHandle, ctxHandle: Handle);
        }

        // Output methods
        public void Output(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            int rc = NativeMethods.unqlite_context_output(Handle, bytes, bytes.Length);
            UnqliteException.ThrowOnError(rc);
        }

        public void Output(byte[] data)
        {
            int rc = NativeMethods.unqlite_context_output(Handle, data, data.Length);
            UnqliteException.ThrowOnError(rc);
        }

        // Error handling
        public void ThrowError(string message, ContextErrorLevel level = ContextErrorLevel.Error)
        {
            int rc = NativeMethods.unqlite_context_throw_error(Handle, (int)level, message);
            UnqliteException.ThrowOnError(rc);
        }

        // Random utilities
        public uint RandomNumber()
        {
            return NativeMethods.unqlite_context_random_num(Handle);
        }

        public string RandomString(int length)
        {
            byte[] buffer = new byte[length];
            int rc = NativeMethods.unqlite_context_random_string(Handle, buffer, length);
            UnqliteException.ThrowOnError(rc);
            return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        }

        // User data
        public IntPtr GetUserData()
        {
            return NativeMethods.unqlite_context_user_data(Handle);
        }

        public void PushAuxData(IntPtr userData)
        {
            int rc = NativeMethods.unqlite_context_push_aux_data(Handle, userData);
            UnqliteException.ThrowOnError(rc);
        }

        public IntPtr PeekAuxData()
        {
            return NativeMethods.unqlite_context_peek_aux_data(Handle);
        }

        // Memory management
        public IntPtr AllocateMemory(uint size, bool zeroMemory = false, bool autoRelease = true)
        {
            return NativeMethods.unqlite_context_alloc_chunk(Handle, size, zeroMemory ? 1 : 0, autoRelease ? 1 : 0);
        }

        public IntPtr ReallocateMemory(IntPtr memory, uint newSize)
        {
            return NativeMethods.unqlite_context_realloc_chunk(Handle, memory, newSize);
        }

        public void FreeMemory(IntPtr memory)
        {
            NativeMethods.unqlite_context_free_chunk(Handle, memory);
        }

        // Result buffer
        public uint ResultBufferLength => NativeMethods.unqlite_context_result_buf_length(Handle);

        public string FunctionName
        {
            get
            {
                IntPtr namePtr = NativeMethods.unqlite_function_name(Handle);
                if (namePtr == IntPtr.Zero)
                    return string.Empty;
                return Marshal.PtrToStringAnsi(namePtr) ?? string.Empty;
            }
        }

        // Result setters
        public void SetResultInt(int value)
        {
            int rc = NativeMethods.unqlite_result_int(Handle, value);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResultInt64(long value)
        {
            int rc = NativeMethods.unqlite_result_int64(Handle, value);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResultBool(bool value)
        {
            int rc = NativeMethods.unqlite_result_bool(Handle, value ? 1 : 0);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResultDouble(double value)
        {
            int rc = NativeMethods.unqlite_result_double(Handle, value);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResultNull()
        {
            int rc = NativeMethods.unqlite_result_null(Handle);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResultString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int rc = NativeMethods.unqlite_result_string(Handle, bytes, bytes.Length);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResultValue(UnqliteValue value)
        {
            int rc = NativeMethods.unqlite_result_value(Handle, value.Handle);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResultResource(IntPtr userData)
        {
            int rc = NativeMethods.unqlite_result_resource(Handle, userData);
            UnqliteException.ThrowOnError(rc);
        }
    }

    public enum ContextErrorLevel
    {
        Error = NativeMethods.UNQLITE_CTX_ERR,
        Warning = NativeMethods.UNQLITE_CTX_WARNING,
        Notice = NativeMethods.UNQLITE_CTX_NOTICE
    }
}
