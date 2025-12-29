// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// Portions inspired by Lightning.NET (https://github.com/CoreyKaylor/Lightning.NET) - MIT License

using System;

namespace UnqliteNet
{
    /// <summary>
    /// Fluent configuration builder for UnQLite database creation and opening.
    /// </summary>
    public class DatabaseConfiguration
    {
        private uint _openFlags = NativeMethods.UNQLITE_OPEN_READWRITE | NativeMethods.UNQLITE_OPEN_CREATE;
        private int? _maxPageCache;
        private bool? _disableAutoCommit;
        private string? _kvEngine;

        /// <summary>
        /// Creates a new database configuration with default settings (read-write, create if not exists).
        /// </summary>
        public DatabaseConfiguration()
        {
        }

        /// <summary>
        /// Gets the open flags for this configuration.
        /// </summary>
        public uint OpenFlags => _openFlags;

        /// <summary>
        /// Gets the maximum page cache size if configured.
        /// </summary>
        public int? MaxPageCache => _maxPageCache;

        /// <summary>
        /// Gets whether auto-commit is disabled.
        /// </summary>
        public bool? DisableAutoCommit => _disableAutoCommit;

        /// <summary>
        /// Gets the KV engine name if configured.
        /// </summary>
        public string? KvEngine => _kvEngine;

        /// <summary>
        /// Configures the database to open in read-only mode.
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration AsReadOnly()
        {
            _openFlags &= ~NativeMethods.UNQLITE_OPEN_READWRITE;
            _openFlags |= NativeMethods.UNQLITE_OPEN_READONLY;
            _openFlags &= ~NativeMethods.UNQLITE_OPEN_CREATE;
            return this;
        }

        /// <summary>
        /// Configures the database to open in read-write mode (default).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration AsReadWrite()
        {
            _openFlags &= ~NativeMethods.UNQLITE_OPEN_READONLY;
            _openFlags |= NativeMethods.UNQLITE_OPEN_READWRITE;
            return this;
        }

        /// <summary>
        /// Configures the database to be created if it doesn't exist (default).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration CreateIfNotExists()
        {
            _openFlags |= NativeMethods.UNQLITE_OPEN_CREATE;
            return this;
        }

        /// <summary>
        /// Configures the database to NOT be created if it doesn't exist.
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration DoNotCreate()
        {
            _openFlags &= ~NativeMethods.UNQLITE_OPEN_CREATE;
            return this;
        }

        /// <summary>
        /// Configures the database to run in-memory (no disk persistence).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration InMemory()
        {
            _openFlags |= NativeMethods.UNQLITE_OPEN_IN_MEMORY;
            return this;
        }

        /// <summary>
        /// Configures the database to run with memory-mapped I/O.
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration WithMemoryMappedIO()
        {
            _openFlags |= NativeMethods.UNQLITE_OPEN_MMAP;
            return this;
        }

        /// <summary>
        /// Configures the database to run in exclusive mode (no other process can open it).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration Exclusive()
        {
            _openFlags |= NativeMethods.UNQLITE_OPEN_EXCLUSIVE;
            return this;
        }

        /// <summary>
        /// Configures the database to operate in temporary mode (deleted on close).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration AsTemporary()
        {
            _openFlags |= NativeMethods.UNQLITE_OPEN_TEMP_DB;
            return this;
        }

        /// <summary>
        /// Configures the database to bypass the journaling system (faster but less safe).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration WithoutJournal()
        {
            _openFlags |= NativeMethods.UNQLITE_OPEN_OMIT_JOURNALING;
            return this;
        }

        /// <summary>
        /// Configures the database to run without mutex (single-threaded mode).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration WithoutMutex()
        {
            _openFlags |= NativeMethods.UNQLITE_OPEN_NOMUTEX;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of pages in the page cache.
        /// </summary>
        /// <param name="maxPages">Maximum number of pages.</param>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration WithMaxPageCache(int maxPages)
        {
            _maxPageCache = maxPages;
            return this;
        }

        /// <summary>
        /// Disables auto-commit for the database (must manually commit transactions).
        /// </summary>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration WithAutoCommitDisabled()
        {
            _disableAutoCommit = true;
            return this;
        }

        /// <summary>
        /// Selects the key-value storage engine to use.
        /// </summary>
        /// <param name="engineName">The engine name (e.g., "hash", "mem").</param>
        /// <returns>This configuration for chaining.</returns>
        public DatabaseConfiguration WithKvEngine(string engineName)
        {
            _kvEngine = engineName;
            return this;
        }

        /// <summary>
        /// Applies the configuration to an open database handle.
        /// </summary>
        /// <param name="dbHandle">The database handle.</param>
        internal void ApplyTo(IntPtr dbHandle)
        {
            if (_maxPageCache.HasValue)
            {
                int rc = NativeMethods.unqlite_config(dbHandle, NativeMethods.UNQLITE_CONFIG_MAX_PAGE_CACHE, __arglist(_maxPageCache.Value));
                UnqliteException.ThrowOnError(rc);
            }

            if (_disableAutoCommit.HasValue && _disableAutoCommit.Value)
            {
                int rc = NativeMethods.unqlite_config(dbHandle, NativeMethods.UNQLITE_CONFIG_DISABLE_AUTO_COMMIT, __arglist());
                UnqliteException.ThrowOnError(rc);
            }

            if (!string.IsNullOrEmpty(_kvEngine))
            {
                int rc = NativeMethods.unqlite_config(dbHandle, NativeMethods.UNQLITE_CONFIG_KV_ENGINE, __arglist(_kvEngine));
                UnqliteException.ThrowOnError(rc);
            }
        }

        /// <summary>
        /// Creates a default configuration (read-write, create if not exists).
        /// </summary>
        public static DatabaseConfiguration Default => new DatabaseConfiguration();

        /// <summary>
        /// Creates a configuration for read-only access.
        /// </summary>
        public static DatabaseConfiguration ReadOnly => new DatabaseConfiguration().AsReadOnly();

        /// <summary>
        /// Creates a configuration for in-memory database.
        /// </summary>
        public static DatabaseConfiguration InMemoryDatabase => new DatabaseConfiguration().InMemory();

        /// <summary>
        /// Creates a configuration for temporary database (deleted on close).
        /// </summary>
        public static DatabaseConfiguration Temporary => new DatabaseConfiguration().AsTemporary();
    }
}
