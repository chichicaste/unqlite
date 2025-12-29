// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Text;

namespace UnqliteNet
{
    /// <summary>
    /// Utility to detect which features are enabled in the native UnQLite library
    /// </summary>
    public static class FeatureDetector
    {
        /// <summary>
        /// Checks if the native library was compiled with thread support
        /// </summary>
        public static bool IsThreadSafeEnabled()
        {
            return NativeMethods.unqlite_lib_is_threadsafe() != 0;
        }

        /// <summary>
        /// Tests if Jx9 VM is available and functional
        /// </summary>
        public static bool IsJx9VmEnabled()
        {
            try
            {
                using var db = new UnqliteDatabase(":mem:");

                // Try to compile a simple Jx9 script
                string testScript = "print 'test';";
                int rc = NativeMethods.unqlite_compile(db.Handle, testScript, testScript.Length, out IntPtr vmHandle);

                if (rc == NativeMethods.UNQLITE_OK && vmHandle != IntPtr.Zero)
                {
                    NativeMethods.unqlite_vm_release(vmHandle);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tests if built-in Jx9 functions are available
        /// </summary>
        public static bool AreJx9BuiltinFunctionsEnabled()
        {
            try
            {
                using var db = new UnqliteDatabase(":mem:");

                // Test a built-in function (strlen)
                string testScript = "$result = strlen('test');";
                using var vm = db.Compile(testScript);

                int rc = NativeMethods.unqlite_vm_exec(vm.Handle);
                return rc == NativeMethods.UNQLITE_OK;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tests if Jx9 math functions are enabled
        /// </summary>
        public static bool AreJx9MathFunctionsEnabled()
        {
            try
            {
                using var db = new UnqliteDatabase(":mem:");

                // Test a math function (sqrt)
                string testScript = "$result = sqrt(16);";
                using var vm = db.Compile(testScript);

                int rc = NativeMethods.unqlite_vm_exec(vm.Handle);
                return rc == NativeMethods.UNQLITE_OK;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tests if Jx9 hash functions are enabled
        /// </summary>
        public static bool AreJx9HashFunctionsEnabled()
        {
            try
            {
                using var db = new UnqliteDatabase(":mem:");

                // Test a hash function (md5)
                string testScript = "$result = md5('test');";
                using var vm = db.Compile(testScript);

                int rc = NativeMethods.unqlite_vm_exec(vm.Handle);
                return rc == NativeMethods.UNQLITE_OK;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tests if Jx9 disk I/O functions are enabled
        /// </summary>
        public static bool AreJx9DiskIoFunctionsEnabled()
        {
            try
            {
                using var db = new UnqliteDatabase(":mem:");

                // Test a disk I/O function (getcwd)
                string testScript = "$result = getcwd();";
                using var vm = db.Compile(testScript);

                int rc = NativeMethods.unqlite_vm_exec(vm.Handle);
                return rc == NativeMethods.UNQLITE_OK;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a comprehensive feature report
        /// </summary>
        public static string GenerateFeatureReport()
        {
            var report = new StringBuilder();

            report.AppendLine("═══════════════════════════════════════════════════════");
            report.AppendLine("           UnQLite Native Library Features");
            report.AppendLine("═══════════════════════════════════════════════════════");
            report.AppendLine();

            // Library Info
            report.AppendLine("Library Information:");
            report.AppendLine($"  Version:    {UnqliteDatabase.Version}");
            report.AppendLine($"  Signature:  {UnqliteDatabase.Signature}");
            report.AppendLine($"  Identifier: {UnqliteDatabase.Identifier}");
            report.AppendLine();

            // Thread Safety
            bool isThreadSafe = IsThreadSafeEnabled();
            report.AppendLine("Thread Safety:");
            report.AppendLine($"  UNQLITE_ENABLE_THREADS: {FormatStatus(isThreadSafe)}");
            if (!isThreadSafe)
            {
                report.AppendLine("  ⚠ WARNING: Library not compiled with thread support!");
                report.AppendLine("            Unsafe for multithreaded use.");
            }
            report.AppendLine();

            // Jx9 VM
            bool jx9Enabled = IsJx9VmEnabled();
            bool builtinEnabled = false;
            bool mathEnabled = false;
            bool hashEnabled = false;
            bool diskIoEnabled = false;

            report.AppendLine("Jx9 Virtual Machine:");
            report.AppendLine($"  Jx9 VM Available: {FormatStatus(jx9Enabled)}");

            if (jx9Enabled)
            {
                // Built-in Functions
                builtinEnabled = AreJx9BuiltinFunctionsEnabled();
                report.AppendLine($"  Built-in Functions (312+): {FormatStatus(builtinEnabled)}");
                if (!builtinEnabled)
                {
                    report.AppendLine("  ⚠ JX9_DISABLE_BUILTIN_FUNC is enabled");
                    report.AppendLine("    Most string, array, and utility functions disabled");
                }

                // Math Functions
                mathEnabled = AreJx9MathFunctionsEnabled();
                report.AppendLine($"  Math Functions: {FormatStatus(mathEnabled)}");
                if (!mathEnabled)
                {
                    report.AppendLine("  ℹ JX9_ENABLE_MATH_FUNC is NOT enabled");
                    report.AppendLine("    sqrt(), abs(), log(), etc. not available");
                }

                // Hash Functions
                hashEnabled = AreJx9HashFunctionsEnabled();
                report.AppendLine($"  Hash Functions: {FormatStatus(hashEnabled)}");
                if (!hashEnabled)
                {
                    report.AppendLine("  ℹ UNQLITE_ENABLE_JX9_HASH_IO is NOT enabled");
                    report.AppendLine("    md5(), sha1(), crc32(), etc. not available");
                }

                // Disk I/O
                diskIoEnabled = AreJx9DiskIoFunctionsEnabled();
                report.AppendLine($"  Disk I/O Functions: {FormatStatus(diskIoEnabled)}");
                if (!diskIoEnabled)
                {
                    report.AppendLine("  ℹ JX9_DISABLE_DISK_IO is enabled");
                    report.AppendLine("    getcwd(), mkdir(), unlink(), etc. disabled");
                }
            }
            else
            {
                report.AppendLine("  ⚠ CRITICAL: Jx9 VM is not available!");
                report.AppendLine("             Document Store features will not work.");
            }

            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════");

            // Summary
            report.AppendLine();
            report.AppendLine("Summary:");
            int enabledCount = 0;
            int totalCount = 5;

            if (jx9Enabled) enabledCount++;
            if (jx9Enabled && builtinEnabled) enabledCount++;
            if (jx9Enabled && mathEnabled) enabledCount++;
            if (jx9Enabled && hashEnabled) enabledCount++;
            if (jx9Enabled && diskIoEnabled) enabledCount++;

            report.AppendLine($"  Core Features: {enabledCount}/{totalCount} enabled");

            if (enabledCount == totalCount)
            {
                report.AppendLine("  ✓ Full-featured build - All capabilities available");
            }
            else if (enabledCount >= 3)
            {
                report.AppendLine("  ⚠ Partial build - Some features disabled");
            }
            else
            {
                report.AppendLine("  ✗ Minimal build - Limited functionality");
            }

            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════");

            return report.ToString();
        }

        private static string FormatStatus(bool enabled)
        {
            return enabled ? "✓ Enabled" : "✗ Disabled";
        }

        /// <summary>
        /// Exposes internal handle for testing (package-private)
        /// </summary>
        internal static IntPtr GetDbHandle(UnqliteDatabase db)
        {
            return db.Handle;
        }
    }
}
