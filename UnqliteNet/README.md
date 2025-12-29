# UnqliteNet - .NET Wrapper for UnQLite

A high-performance, idiomatic .NET wrapper for **UnQLite** — the embeddable NoSQL database engine.

**UnqliteNet** brings the power of UnQLite (Key-Value Store + Document Store + Jx9 Scripting) to the .NET ecosystem, focusing on zero-allocation APIs, memory safety, and seamless interoperability.

## Key technical achievements

This wrapper is not just a raw P/Invoke binding. It implements advanced .NET performance patterns inspired by libraries like `Lightning.NET`.

- Simple and modern C# API (.NET 9.0)
- Basic Key/Value operations (Store, Fetch, Delete)
- Transactions (Begin, Commit, Rollback)
- Cursor to iterate over all records
- Support for in-memory and disk-based databases
- Thread-safe

### 1. Zero-Copy architecture

* **Span-Based API:** heavily utilizes `ReadOnlySpan<byte>` and `Span<byte>` to interact with the native engine.
* **Direct Memory Access:** Uses `unsafe` and `fixed` pointers to pass data to C-land without intermediate `byte[]` allocations, significantly reducing Garbage Collector (GC) pressure during high-throughput read/write operations.
* **Stack Allocation:** Intelligent usage of `stackalloc` for small keys to avoid heap allocations entirely.

### 2. Robust resource management

* **SafeHandles:** Encapsulates native pointers (`unqlite*`, `unqlite_vm*`) using `SafeHandleZeroOrMinusOneIsInvalid`, ensuring critical resources are released deterministically even during catastrophic failures.
* **Disposable Pattern:** Full implementation of `IDisposable` across the hierarchy (`Database` → `Transaction` → `Cursor`).
* **Auto-Rollback:** Transactions automatically issue a `ROLLBACK` if disposed without an explicit `Commit()`, preventing database corruption due to unhandled exceptions.

### 3. Advanced interoperability (Jx9)

* **Foreign Functions:** Allows extending the Jx9 scripting language with C# methods.
* **Delegate Pinning:** Implements a robust `GCHandle` pinning strategy to prevent the Garbage Collector from relocating C# delegates passed to the native UnQLite engine as callbacks.
* **Dynamic Value Mapping:** Seamlessly converts between UnQLite dynamic types (Scalar, Array, JSON) and .NET types.

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

## Installation

*(Add NuGet instructions here once published)*

```bash
dotnet add package UnqliteNet

```

## Quick Start

### Key-Value store (Zero-Copy)

```csharp
using UnqliteNet;
using System.Text;

using var db = new UnqliteDatabase("data.db");

// Store using Spans (Zero allocation overhead)
Span<byte> key = Encoding.UTF8.GetBytes("user:101");
Span<byte> value = Encoding.UTF8.GetBytes("{ 'name': 'Miguel' }");

db.Store(key, value);

// Fetch
if (db.TryFetch(key, out byte[] result))
{
    Console.WriteLine($"Found: {Encoding.UTF8.GetString(result)}");
}

```

### Jx9 Document store & scripting

UnqliteNet exposes the full power of the Jx9 VM, allowing you to run complex logic inside the database engine.

```csharp
using var db = new UnqliteDatabase("users.db");

// Jx9 script to store a JSON document
string script = @"
    $user = { 'id': 1, 'name': 'Alice', 'role': 'admin' };
    db_store('users', $user);
    print 'User stored successfully';
";

using var vm = db.Compile(script);

// Capture script output from C#
vm.SetOutputHandler(output => Console.WriteLine($"VM Output: {output}"));

vm.Execute();

```

### C# foreign functions in Jx9

Extend the database language with .NET logic:

```csharp
using var vm = db.Compile("$result = my_csharp_func(10, 20); print $result;");

// Register a C# function accessible from the script
vm.CreateFunction("my_csharp_func", (ctx, args) => 
{
    int a = args[0].ToInt();
    int b = args[1].ToInt();
    
    // Return result back to Jx9 engine
    ctx.SetResultInt(a + b);
    return NativeMethods.UNQLITE_OK;
});

vm.Execute(); // Prints "30"

```

## Architecture

The library is organized into three layers:

1. **Native Layer (`NativeMethods`):** Raw P/Invoke signatures matching the C header files.
2. **Safe Layer (`SafeHandles`):** Wrappers that guarantee handle lifespan and cleanup.
3. **Idiomatic Layer:** The public API (`UnqliteDatabase`, `UnqliteCursor`) that provides .NET idioms like `IEnumerable`, Exceptions instead of error codes, and Fluent Configuration.

## Roadmap & Future improvements

We welcome contributions! Here are the key areas suggested for future development:

### 1. LINQ provider for Jx9

* **Goal:** Allow developers to write LINQ queries in C# that compile down to Jx9 scripts.
* **Implementation:** Build an `IQueryable` provider that translates `db.Collection("users").Where(u => u.Age > 18)` into Jx9 filter logic.

### 2. POCO serialization

* **Goal:** Automatic mapping between C# Classes and UnQLite JSON documents.
* **Implementation:** Integrate `System.Text.Json` or `Newtonsoft` to allow generic methods like `db.Store<User>("key", userObj)`.

### 3. Async/Await Wrappers

* **Goal:** Although UnQLite is synchronous by nature (embedded), providing `Task`-based wrappers (via `Task.Run`) could improve responsiveness in UI applications, though care must be taken with thread affinity.

### 4. Comparison

* **Goal:** Benchmarks comparing raw KV performance. UnQLite is feature-rich, but knowing the throughput difference helps developers choose the right tool.

## License

Licensed under the MIT License. See [LICENSE](https://www.google.com/search?q=LICENSE) for details.

---

**Copyright (c) 2025 Miguel Hernández**
