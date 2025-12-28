using System;
using UnqliteNet;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            using var db = new UnqliteDatabase("test.db", inMemory: false);

            Console.WriteLine("=== Store ===");
            db.Store("nombre", "Juan Pérez");
            db.Store("edad", "30");
            db.Store("ciudad", "Madrid");
            Console.WriteLine("Datos almacenados");

            Console.WriteLine();
            Console.WriteLine("=== Fetch (existing keys) ===");
            Console.WriteLine($"Nombre: {db.FetchString("nombre")}");
            Console.WriteLine($"Edad: {db.FetchString("edad")}");
            Console.WriteLine($"Ciudad: {db.FetchString("ciudad")}");

            Console.WriteLine();
            Console.WriteLine("=== GetAll (itera todas las claves) ===");
            foreach (var kvp in db.GetAll())
            {
                Console.WriteLine($"  {kvp.Key}: {System.Text.Encoding.UTF8.GetString(kvp.Value)}");
            }

            Console.WriteLine();
            Console.WriteLine("=== Cursor Navigation ===");
            using (var cursor = db.CreateCursor())
            {
                if (cursor.First())
                {
                    do
                    {
                        Console.WriteLine($"  {cursor.Key}: {cursor.DataAsString}");
                    } while (cursor.Next());
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== Transaction ===");
            using (var tx = db.BeginTransaction())
            {
                db.Store("pais", "España");
                db.Store("ocupacion", "Desarrollador");
                Console.WriteLine("Datos agregados en transacción");
                tx.Commit();
                Console.WriteLine("Transacción confirmada");
            }

            Console.WriteLine();
            Console.WriteLine("=== Verify transaction ===");
            Console.WriteLine($"País: {db.FetchString("pais")}");
            Console.WriteLine($"Ocupación: {db.FetchString("ocupacion")}");

            Console.WriteLine();
            Console.WriteLine("=== Delete ===");
            db.Delete("ciudad");
            Console.WriteLine("Registro 'ciudad' eliminado");

            Console.WriteLine();
            Console.WriteLine("Prueba completada exitosamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            if (ex is UnqliteException uex)
            {
                Console.WriteLine($"Error code: {uex.ErrorCode}");
            }
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
