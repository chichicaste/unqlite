// Copyright (c) 2025 Miguel Hernández
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// Portions inspired by Lightning.NET (https://github.com/CoreyKaylor/Lightning.NET) - MIT License

using System;
using System.Collections.Generic;

namespace UnqliteNet
{
    /// <summary>
    /// Interface for database cursor operations.
    /// Provides standard methods for iterating through database records.
    /// </summary>
    /// <remarks>
    /// Note: Unlike LMDB, UnQLite is a simple key-value store and does NOT support
    /// duplicate keys (LMDB's DUP_SORT feature). Each key can have only one value.
    /// </remarks>
    public interface ICursor : IDisposable
    {
        /// <summary>
        /// Gets the current cursor position's key as a string.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the current cursor position's data as a byte array.
        /// </summary>
        byte[]? Data { get; }

        /// <summary>
        /// Gets the current cursor position's data as a string.
        /// </summary>
        string DataAsString { get; }

        /// <summary>
        /// Gets whether the cursor is currently positioned on a valid record.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Moves the cursor to the first record in the database.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false otherwise.</returns>
        bool First();

        /// <summary>
        /// Moves the cursor to the last record in the database.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false otherwise.</returns>
        bool Last();

        /// <summary>
        /// Moves the cursor to the next record.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false if at end.</returns>
        bool Next();

        /// <summary>
        /// Moves the cursor to the previous record.
        /// </summary>
        /// <returns>True if successful and cursor is positioned on a valid record, false if at beginning.</returns>
        bool Previous();

        /// <summary>
        /// Positions the cursor at the specified key using exact matching.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        bool Set(string key);

        /// <summary>
        /// Positions the cursor at or near the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="position">The seek position strategy (Exact, LessOrEqual, GreaterOrEqual).</param>
        /// <returns>True if a matching record was found, false otherwise.</returns>
        bool Set(string key, SeekPosition position);

        /// <summary>
        /// Gets the current key and data as a key-value pair.
        /// </summary>
        /// <returns>A key-value pair containing the current record, or null if cursor is not valid.</returns>
        KeyValuePair<byte[], byte[]>? GetCurrent();

        /// <summary>
        /// Deletes the record at the current cursor position.
        /// After deletion, the cursor moves to the next record.
        /// </summary>
        void Delete();

        /// <summary>
        /// Resets the cursor to an uninitialized state.
        /// </summary>
        void Reset();
    }
}
