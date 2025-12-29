using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UnqliteNet
{
    public class UnqliteVm : IDisposable
    {
        internal IntPtr Handle { get; private set; }
        private bool _disposed;
        private readonly List<GCHandle> _pinnedHandles = new List<GCHandle>();

        internal UnqliteVm(IntPtr handle)
        {
            Handle = handle;
        }

        // Configuration methods
        public void SetOutputHandler(Action<string> outputHandler)
        {
            var callback = new NativeMethods.UnqliteOutputConsumer((pOutput, nLen, pUserData) =>
            {
                if (pOutput == IntPtr.Zero || nLen == 0)
                    return 0;

                byte[] buffer = new byte[nLen];
                Marshal.Copy(pOutput, buffer, 0, (int)nLen);
                string output = Encoding.UTF8.GetString(buffer);
                outputHandler?.Invoke(output);
                return 0;
            });

            GCHandle handle = GCHandle.Alloc(callback);
            _pinnedHandles.Add(handle);

            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_OUTPUT, __arglist(
                Marshal.GetFunctionPointerForDelegate(callback),
                IntPtr.Zero));
            UnqliteException.ThrowOnError(rc);
        }

        public void SetImportPath(string path)
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_IMPORT_PATH, __arglist(path));
            UnqliteException.ThrowOnError(rc);
        }

        public void EnableErrorReport()
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_ERR_REPORT, __arglist());
            UnqliteException.ThrowOnError(rc);
        }

        public void SetRecursionDepth(int maxDepth)
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_RECURSION_DEPTH, __arglist(maxDepth));
            UnqliteException.ThrowOnError(rc);
        }

        public void CreateVariable(string name, UnqliteValue value)
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_CREATE_VAR, __arglist(name, value.Handle));
            UnqliteException.ThrowOnError(rc);
        }

        public void AddArgument(string value)
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_ARGV_ENTRY, __arglist(value));
            UnqliteException.ThrowOnError(rc);
        }

        public void SetHttpRequest(string rawRequest)
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_HTTP_REQUEST, __arglist(rawRequest, rawRequest.Length));
            UnqliteException.ThrowOnError(rc);
        }

        public void SetServerAttribute(string key, string value)
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_SERVER_ATTR, __arglist(key, value, value.Length));
            UnqliteException.ThrowOnError(rc);
        }

        public void SetEnvironmentAttribute(string key, string value)
        {
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_ENV_ATTR, __arglist(key, value, value.Length));
            UnqliteException.ThrowOnError(rc);
        }

        // Execution methods
        public void Execute()
        {
            int rc = NativeMethods.unqlite_vm_exec(Handle);
            UnqliteException.ThrowOnError(rc);
        }

        public void Reset()
        {
            int rc = NativeMethods.unqlite_vm_reset(Handle);
            UnqliteException.ThrowOnError(rc);
        }

        // Variable extraction
        public UnqliteValue ExtractVariable(string name)
        {
            IntPtr valueHandle = NativeMethods.unqlite_vm_extract_variable(Handle, name);
            if (valueHandle == IntPtr.Zero)
                return null;

            return new UnqliteValue(valueHandle, Handle, autoRelease: false);
        }

        // Output extraction
        public string GetOutput()
        {
            IntPtr outputPtr = IntPtr.Zero;
            uint outputLen = 0;

            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_CONFIG_EXTRACT_OUTPUT, __arglist(ref outputPtr, ref outputLen));
            UnqliteException.ThrowOnError(rc);

            if (outputPtr == IntPtr.Zero || outputLen == 0)
                return string.Empty;

            byte[] buffer = new byte[outputLen];
            Marshal.Copy(outputPtr, buffer, 0, (int)outputLen);
            return Encoding.UTF8.GetString(buffer);
        }

        public uint GetOutputLength()
        {
            uint length = 0;
            int rc = NativeMethods.unqlite_vm_config(Handle, NativeMethods.UNQLITE_VM_OUTPUT_LENGTH, __arglist(ref length));
            UnqliteException.ThrowOnError(rc);
            return length;
        }

        // Dump VM state
        public string Dump()
        {
            StringBuilder output = new StringBuilder();

            var callback = new NativeMethods.UnqliteDataConsumer((pData, nDataLen, pUserData) =>
            {
                if (pData == IntPtr.Zero || nDataLen == 0)
                    return 0;

                byte[] buffer = new byte[nDataLen];
                Marshal.Copy(pData, buffer, 0, (int)nDataLen);
                output.Append(Encoding.UTF8.GetString(buffer));
                return 0;
            });

            int rc = NativeMethods.unqlite_vm_dump(Handle, callback, IntPtr.Zero);
            UnqliteException.ThrowOnError(rc);

            return output.ToString();
        }

        // Value creation
        public UnqliteValue CreateScalar()
        {
            return UnqliteValue.CreateScalar(this);
        }

        public UnqliteValue CreateArray()
        {
            return UnqliteValue.CreateArray(this);
        }

        // Foreign function registration
        public void CreateFunction(string name, Func<UnqliteContext, UnqliteValue[], int> function, IntPtr userData = default)
        {
            var wrapper = new NativeMethods.UnqliteForeignFunction((pCtx, argc, argv) =>
            {
                try
                {
                    var context = new UnqliteContext(pCtx);
                    var arguments = new UnqliteValue[argc];

                    for (int i = 0; i < argc; i++)
                    {
                        arguments[i] = new UnqliteValue(argv[i], autoRelease: false);
                    }

                    return function(context, arguments);
                }
                catch (Exception ex)
                {
                    var ctx = new UnqliteContext(pCtx);
                    ctx.ThrowError($"Error in function '{name}': {ex.Message}");
                    return NativeMethods.UNQLITE_ABORT;
                }
            });

            GCHandle handle = GCHandle.Alloc(wrapper);
            _pinnedHandles.Add(handle);

            int rc = NativeMethods.unqlite_create_function(Handle, name, wrapper, userData);
            UnqliteException.ThrowOnError(rc);
        }

        public void DeleteFunction(string name)
        {
            int rc = NativeMethods.unqlite_delete_function(Handle, name);
            UnqliteException.ThrowOnError(rc);
        }

        public void CreateConstant(string name, Action<UnqliteValue> expander, IntPtr userData = default)
        {
            var wrapper = new NativeMethods.UnqliteConstantExpand((pValue, pUserData) =>
            {
                try
                {
                    var value = new UnqliteValue(pValue, autoRelease: false);
                    expander(value);
                }
                catch
                {
                    // Ignore errors in constant expansion
                }
            });

            GCHandle handle = GCHandle.Alloc(wrapper);
            _pinnedHandles.Add(handle);

            int rc = NativeMethods.unqlite_create_constant(Handle, name, wrapper, userData);
            UnqliteException.ThrowOnError(rc);
        }

        public void DeleteConstant(string name)
        {
            int rc = NativeMethods.unqlite_delete_constant(Handle, name);
            UnqliteException.ThrowOnError(rc);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (Handle != IntPtr.Zero)
                {
                    NativeMethods.unqlite_vm_release(Handle);
                    Handle = IntPtr.Zero;
                }

                foreach (var handle in _pinnedHandles)
                {
                    if (handle.IsAllocated)
                        handle.Free();
                }
                _pinnedHandles.Clear();

                _disposed = true;
            }
        }

        ~UnqliteVm()
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
