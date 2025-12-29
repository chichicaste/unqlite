using System;

namespace UnqliteNet.Examples
{
    /// <summary>
    /// Examples demonstrating the Enterprise features of UnqliteNet
    /// </summary>
    public static class Examples
    {
        /// <summary>
        /// Example 1: Basic Document Store using Jx9 scripting
        /// </summary>
        public static void DocumentStoreExample()
        {
            using var db = new UnqliteDatabase("test.db");

            // Compile and execute Jx9 script to store JSON documents
            string jx9Script = @"
                // Create a JSON document
                $user = {
                    'id': 1,
                    'name': 'John Doe',
                    'email': 'john@example.com',
                    'age': 30
                };

                // Store the document
                db_store('users', $user);

                // Retrieve and output
                $retrieved = db_fetch('users');
                print $retrieved.name;
            ";

            using var vm = db.Compile(jx9Script);

            // Set output handler to capture script output
            vm.SetOutputHandler(output => Console.WriteLine($"Script output: {output}"));

            // Execute the script
            vm.Execute();

            // Get the output
            string output = vm.GetOutput();
            Console.WriteLine($"Final output: {output}");
        }

        /// <summary>
        /// Example 2: Working with UnqliteValue for dynamic data
        /// </summary>
        public static void ValueManagementExample()
        {
            using var db = new UnqliteDatabase("test.db");
            using var vm = db.Compile("/* placeholder */");

            // Create scalar values
            using var intValue = UnqliteValue.CreateScalar(vm);
            intValue.SetInt(42);
            Console.WriteLine($"Integer value: {intValue.ToInt()}");

            using var stringValue = UnqliteValue.CreateScalar(vm);
            stringValue.SetString("Hello, UnQLite!");
            Console.WriteLine($"String value: {stringValue.ToString()}");

            // Create JSON array/object
            using var arrayValue = UnqliteValue.CreateArray(vm);

            using var nameValue = UnqliteValue.CreateScalar(vm);
            nameValue.SetString("John");
            arrayValue.AddArrayElement("name", nameValue);

            using var ageValue = UnqliteValue.CreateScalar(vm);
            ageValue.SetInt(30);
            arrayValue.AddArrayElement("age", ageValue);

            Console.WriteLine($"Array count: {arrayValue.GetArrayCount()}");

            // Retrieve elements
            using var retrievedName = arrayValue.GetArrayElement("name");
            if (retrievedName != null)
            {
                Console.WriteLine($"Name from array: {retrievedName.ToString()}");
            }
        }

        /// <summary>
        /// Example 3: Foreign Functions - Extending Jx9 with custom functions
        /// </summary>
        public static void ForeignFunctionsExample()
        {
            using var db = new UnqliteDatabase("test.db");

            string jx9Script = @"
                // Call our custom function
                $result = custom_multiply(6, 7);
                print 'Result: ' .. $result;

                // Use custom constant
                print '\nPI value: ' .. MY_PI;
            ";

            using var vm = db.Compile(jx9Script);

            // Register a custom function
            vm.CreateFunction("custom_multiply", (ctx, args) =>
            {
                if (args.Length != 2)
                {
                    ctx.ThrowError("custom_multiply() expects 2 arguments");
                    return NativeMethods.UNQLITE_INVALID;
                }

                int a = args[0].ToInt();
                int b = args[1].ToInt();
                int result = a * b;

                ctx.SetResultInt(result);
                return NativeMethods.UNQLITE_OK;
            });

            // Register a custom constant
            vm.CreateConstant("MY_PI", (value) =>
            {
                value.SetDouble(3.14159265359);
            });

            vm.SetOutputHandler(output => Console.WriteLine(output));
            vm.Execute();
        }

        /// <summary>
        /// Example 4: Advanced Configuration
        /// </summary>
        public static void ConfigurationExample()
        {
            using var db = new UnqliteDatabase("test.db");

            // Set maximum page cache (performance tuning)
            db.SetMaxPageCache(1000);

            // Disable auto-commit for manual transaction control
            db.DisableAutoCommit();

            // Get storage engine name
            string engineName = db.GetKvEngineName();
            Console.WriteLine($"Storage Engine: {engineName}");

            // Manual transaction
            using var transaction = db.BeginTransaction();
            db.Store("key1", "value1");
            db.Store("key2", "value2");
            transaction.Commit();
        }

        /// <summary>
        /// Example 5: Callback-based operations for better performance
        /// </summary>
        public static void CallbackExample()
        {
            using var db = new UnqliteDatabase("test.db");

            // Store large data
            byte[] largeData = new byte[1_000_000]; // 1MB
            new Random().NextBytes(largeData);
            db.Store("large_key", largeData);

            // Fetch using callback (no intermediate buffer allocation)
            db.FetchCallback("large_key", data =>
            {
                Console.WriteLine($"Received {data.Length} bytes via callback");
            });

            // Cursor with callbacks
            using var cursor = db.CreateCursor();
            if (cursor.First())
            {
                cursor.GetKeyCallback(keyData =>
                {
                    Console.WriteLine($"Key size: {keyData.Length} bytes");
                });

                cursor.GetDataCallback(valueData =>
                {
                    Console.WriteLine($"Value size: {valueData.Length} bytes");
                });
            }
        }

        /// <summary>
        /// Example 6: Utility functions
        /// </summary>
        public static void UtilityExample()
        {
            using var db = new UnqliteDatabase("test.db");

            // Generate random data
            string randomString = db.GenerateRandomString(16);
            Console.WriteLine($"Random string: {randomString}");

            uint randomNumber = db.GenerateRandomNumber();
            Console.WriteLine($"Random number: {randomNumber}");

            // Library information
            Console.WriteLine($"Version: {UnqliteDatabase.Version}");
            Console.WriteLine($"Signature: {UnqliteDatabase.Signature}");
            Console.WriteLine($"Copyright: {UnqliteDatabase.Copyright}");
            Console.WriteLine($"Thread-safe: {UnqliteDatabase.IsThreadSafe}");
        }

        /// <summary>
        /// Example 7: Complex Document Store scenario
        /// </summary>
        public static void ComplexDocumentStoreExample()
        {
            using var db = new UnqliteDatabase("users.db");

            string jx9Script = @"
                // Create a collection of users
                $users = [
                    {
                        'id': 1,
                        'name': 'Alice',
                        'email': 'alice@example.com',
                        'role': 'admin',
                        'created': '2025-01-01'
                    },
                    {
                        'id': 2,
                        'name': 'Bob',
                        'email': 'bob@example.com',
                        'role': 'user',
                        'created': '2025-01-02'
                    },
                    {
                        'id': 3,
                        'name': 'Charlie',
                        'email': 'charlie@example.com',
                        'role': 'user',
                        'created': '2025-01-03'
                    }
                ];

                // Store each user
                foreach($users as $user) {
                    $key = 'user:' .. $user.id;
                    db_store($key, $user);
                }

                // Query users
                print 'Total users stored: ' .. count($users) .. '\n';

                // Retrieve a specific user
                $alice = db_fetch('user:1');
                print 'Retrieved user: ' .. $alice.name .. ' (' .. $alice.email .. ')\n';

                // Filter users by role
                $admins = [];
                foreach($users as $user) {
                    if ($user.role == 'admin') {
                        array_push($admins, $user.name);
                    }
                }

                print 'Admins: ' .. implode(', ', $admins);
            ";

            using var vm = db.Compile(jx9Script);
            vm.SetOutputHandler(output => Console.Write(output));
            vm.Execute();
        }

        /// <summary>
        /// Example 8: Error handling and logging
        /// </summary>
        public static void ErrorHandlingExample()
        {
            try
            {
                using var db = new UnqliteDatabase("test.db");

                // Try to compile invalid Jx9 script
                string invalidScript = @"
                    $invalid syntax here
                ";

                using var vm = db.Compile(invalidScript);
                vm.Execute();
            }
            catch (UnqliteException ex)
            {
                Console.WriteLine($"UnQLite Error: {ex.Message}");
                Console.WriteLine($"Error Code: {ex.ErrorCode}");
            }

            // Get error logs
            using var db2 = new UnqliteDatabase("test.db");
            string errorLog = db2.GetErrorLog();
            string jx9ErrorLog = db2.GetJx9ErrorLog();

            if (!string.IsNullOrEmpty(errorLog))
                Console.WriteLine($"Error Log: {errorLog}");

            if (!string.IsNullOrEmpty(jx9ErrorLog))
                Console.WriteLine($"Jx9 Error Log: {jx9ErrorLog}");
        }
    }
}
