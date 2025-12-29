using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UnqliteNet
{
    public class UnqliteValue : IDisposable
    {
        internal IntPtr Handle { get; private set; }
        private readonly IntPtr _vmHandle;
        private readonly IntPtr _ctxHandle;
        private readonly bool _autoRelease;
        private bool _disposed;

        internal UnqliteValue(IntPtr handle, IntPtr vmHandle = default, IntPtr ctxHandle = default, bool autoRelease = true)
        {
            Handle = handle;
            _vmHandle = vmHandle;
            _ctxHandle = ctxHandle;
            _autoRelease = autoRelease;
        }

        // Static factory methods
        public static UnqliteValue CreateScalar(UnqliteVm vm)
        {
            IntPtr handle = NativeMethods.unqlite_vm_new_scalar(vm.Handle);
            if (handle == IntPtr.Zero)
                throw new UnqliteException(NativeMethods.UNQLITE_NOMEM, "Failed to create scalar value");
            return new UnqliteValue(handle, vm.Handle);
        }

        public static UnqliteValue CreateArray(UnqliteVm vm)
        {
            IntPtr handle = NativeMethods.unqlite_vm_new_array(vm.Handle);
            if (handle == IntPtr.Zero)
                throw new UnqliteException(NativeMethods.UNQLITE_NOMEM, "Failed to create array value");
            return new UnqliteValue(handle, vm.Handle);
        }

        // Type checking properties
        public bool IsInt => NativeMethods.unqlite_value_is_int(Handle) != 0;
        public bool IsFloat => NativeMethods.unqlite_value_is_float(Handle) != 0;
        public bool IsBool => NativeMethods.unqlite_value_is_bool(Handle) != 0;
        public bool IsString => NativeMethods.unqlite_value_is_string(Handle) != 0;
        public bool IsNull => NativeMethods.unqlite_value_is_null(Handle) != 0;
        public bool IsNumeric => NativeMethods.unqlite_value_is_numeric(Handle) != 0;
        public bool IsCallable => NativeMethods.unqlite_value_is_callable(Handle) != 0;
        public bool IsScalar => NativeMethods.unqlite_value_is_scalar(Handle) != 0;
        public bool IsJsonArray => NativeMethods.unqlite_value_is_json_array(Handle) != 0;
        public bool IsJsonObject => NativeMethods.unqlite_value_is_json_object(Handle) != 0;
        public bool IsResource => NativeMethods.unqlite_value_is_resource(Handle) != 0;
        public bool IsEmpty => NativeMethods.unqlite_value_is_empty(Handle) != 0;

        // Setters
        public void SetInt(int value)
        {
            int rc = NativeMethods.unqlite_value_int(Handle, value);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetInt64(long value)
        {
            int rc = NativeMethods.unqlite_value_int64(Handle, value);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetBool(bool value)
        {
            int rc = NativeMethods.unqlite_value_bool(Handle, value ? 1 : 0);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetNull()
        {
            int rc = NativeMethods.unqlite_value_null(Handle);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetDouble(double value)
        {
            int rc = NativeMethods.unqlite_value_double(Handle, value);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int rc = NativeMethods.unqlite_value_string(Handle, bytes, bytes.Length);
            UnqliteException.ThrowOnError(rc);
        }

        public void SetResource(IntPtr userData)
        {
            int rc = NativeMethods.unqlite_value_resource(Handle, userData);
            UnqliteException.ThrowOnError(rc);
        }

        public void Reset()
        {
            int rc = NativeMethods.unqlite_value_reset_string_cursor(Handle);
            UnqliteException.ThrowOnError(rc);
        }

        // Getters
        public int ToInt()
        {
            return NativeMethods.unqlite_value_to_int(Handle);
        }

        public long ToInt64()
        {
            return NativeMethods.unqlite_value_to_int64(Handle);
        }

        public bool ToBool()
        {
            return NativeMethods.unqlite_value_to_bool(Handle) != 0;
        }

        public double ToDouble()
        {
            return NativeMethods.unqlite_value_to_double(Handle);
        }

        public string ToString(bool safe = true)
        {
            IntPtr strPtr = NativeMethods.unqlite_value_to_string(Handle, out int len);
            if (strPtr == IntPtr.Zero || len == 0)
                return string.Empty;

            byte[] bytes = new byte[len];
            Marshal.Copy(strPtr, bytes, 0, len);
            return Encoding.UTF8.GetString(bytes);
        }

        public IntPtr ToResource()
        {
            return NativeMethods.unqlite_value_to_resource(Handle);
        }

        // Array operations (if this is a JSON array/object)
        public UnqliteValue GetArrayElement(string key)
        {
            if (!IsJsonArray && !IsJsonObject)
                throw new InvalidOperationException("Value is not a JSON array or object");

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            IntPtr elemHandle = NativeMethods.unqlite_array_fetch(Handle, keyBytes, keyBytes.Length);

            if (elemHandle == IntPtr.Zero)
                return null;

            return new UnqliteValue(elemHandle, _vmHandle, _ctxHandle, false);
        }

        public void AddArrayElement(string key, UnqliteValue value)
        {
            if (!IsJsonArray && !IsJsonObject)
                throw new InvalidOperationException("Value is not a JSON array or object");

            int rc = NativeMethods.unqlite_array_add_strkey_elem(Handle, key, value.Handle);
            UnqliteException.ThrowOnError(rc);
        }

        public void AddArrayElement(UnqliteValue key, UnqliteValue value)
        {
            if (!IsJsonArray && !IsJsonObject)
                throw new InvalidOperationException("Value is not a JSON array or object");

            int rc = NativeMethods.unqlite_array_add_elem(Handle, key.Handle, value.Handle);
            UnqliteException.ThrowOnError(rc);
        }

        public int GetArrayCount()
        {
            if (!IsJsonArray && !IsJsonObject)
                throw new InvalidOperationException("Value is not a JSON array or object");

            return NativeMethods.unqlite_array_count(Handle);
        }

        // Comparison
        public int CompareTo(UnqliteValue other, bool strict = false)
        {
            return NativeMethods.unqlite_value_compare(Handle, other.Handle, strict ? 1 : 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_autoRelease && Handle != IntPtr.Zero)
                {
                    if (_vmHandle != IntPtr.Zero)
                    {
                        NativeMethods.unqlite_vm_release_value(_vmHandle, Handle);
                    }
                    else if (_ctxHandle != IntPtr.Zero)
                    {
                        NativeMethods.unqlite_context_release_value(_ctxHandle, Handle);
                    }
                    else
                    {
                        NativeMethods.unqlite_value_release(Handle);
                    }
                }
                Handle = IntPtr.Zero;
                _disposed = true;
            }
        }

        ~UnqliteValue()
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
