# UnqliteNet - .NET Wrapper for UnQLite

.NET wrapper for the UnQLite embedded NoSQL database.

## Features

- Simple and modern C# API (.NET 9.0)
- Basic Key/Value operations (Store, Fetch, Delete)
- Transactions (Begin, Commit, Rollback)
- Cursor to iterate over all records
- Support for in-memory and disk-based databases
- Thread-safe

## Building

1. Build the native UnQLite library:
```bash
cmake -S . -B build
cmake --build build --config Release
```

2. Build the .NET wrapper:
```bash
cd UnqliteNet
dotnet build --configuration Release
```

## Usage

### Basic

```csharp
using UnqliteNet;

// Open database
using var db = new UnqliteDatabase("test.db");

// Store data
db.Store("name", "Juan Pérez");
db.Store("age", "30");

// Retrieve data
string name = db.FetchString("name");
byte[] data = db.Fetch("age");

// Check if exists
bool exists = db.Contains("name");

// Delete
db.Delete("age");
```

### Transactions

```csharp
using (var tx = db.BeginTransaction())
{
    db.Store("key1", "value1");
    db.Store("key2", "value2");
    
    // Commit changes
    tx.Commit();
    // Or rollback: tx.Rollback();
}
```

### Cursor

```csharp
// Iterate all records
using var cursor = db.CreateCursor();
if (cursor.First())
{
    do
    {
        Console.WriteLine($"{cursor.Key}: {cursor.DataAsString}");
    } while (cursor.Next());
}

// Or using GetAll()
foreach (var kvp in db.GetAll())
{
    Console.WriteLine($"{kvp.Key}: {Encoding.UTF8.GetString(kvp.Value)}");
}
```

### In-Memory Databases

```csharp
using var db = new UnqliteDatabase(":mem:", inMemory: true);
```

## Main Classes

- `UnqliteDatabase` - Main database connection
- `UnqliteTransaction` - Transaction handling
- `UnqliteCursor` - Advanced record navigation
- `UnqliteException` - UnQLite specific exceptions

## Run Example

```bash
cd UnqliteNet.Example
dotnet run
```

## Requirements

- .NET 9.0
- unqlite_shared.dll (automatically copied from build/Release/)
