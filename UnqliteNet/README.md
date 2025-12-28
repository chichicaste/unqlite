# UnqliteNet - .NET Wrapper para UnQLite

Wrapper de .NET para la base de datos NoSQL embebida UnQLite.

## Características

- API simple y moderna de C# (.NET 9.0)
- Operaciones Key/Value básicas (Store, Fetch, Delete)
- Transacciones (Begin, Commit, Rollback)
- Cursor para iterar sobre todos los registros
- Soporte para bases de datos en memoria y en disco
- Thread-safe

## Compilación

1. Compilar la librería nativa de UnQLite:
```bash
cmake -S . -B build
cmake --build build --config Release
```

2. Compilar el wrapper de .NET:
```bash
cd UnqliteNet
dotnet build --configuration Release
```

## Uso

### Básico

```csharp
using UnqliteNet;

// Abrir base de datos
using var db = new UnqliteDatabase("test.db");

// Almacenar datos
db.Store("nombre", "Juan Pérez");
db.Store("edad", "30");

// Recuperar datos
string nombre = db.FetchString("nombre");
byte[] datos = db.Fetch("edad");

// Verificar si existe
bool exists = db.Contains("nombre");

// Eliminar
db.Delete("edad");
```

### Transacciones

```csharp
using (var tx = db.BeginTransaction())
{
    db.Store("clave1", "valor1");
    db.Store("clave2", "valor2");
    
    // Confirmar cambios
    tx.Commit();
    // O deshacer: tx.Rollback();
}
```

### Cursor

```csharp
// Iterar todos los registros
using var cursor = db.CreateCursor();
if (cursor.First())
{
    do
    {
        Console.WriteLine($"{cursor.Key}: {cursor.DataAsString}");
    } while (cursor.Next());
}

// O usando GetAll()
foreach (var kvp in db.GetAll())
{
    Console.WriteLine($"{kvp.Key}: {Encoding.UTF8.GetString(kvp.Value)}");
}
```

### Bases de datos en memoria

```csharp
using var db = new UnqliteDatabase(":mem:", inMemory: true);
```

## Clases Principales

- `UnqliteDatabase` - Conexión principal a la base de datos
- `UnqliteTransaction` - Manejo de transacciones
- `UnqliteCursor` - Navegación avanzada por registros
- `UnqliteException` - Excepciones específicas de UnQLite

## Ejecutar el ejemplo

```bash
cd UnqliteNet.Example
dotnet run
```

## Requisitos

- .NET 9.0
- unqlite_shared.dll (se copia automáticamente desde build/Release/)
