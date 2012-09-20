using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.IO;

namespace micajah.databasescripter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!string.IsNullOrEmpty(args[0]) && args[0].Contains("?"))
            {
                Console.WriteLine("Usage: micajah.databasescripter.exe servername databasename destinationfolder");
                Console.ReadKey();
                return;
            }

            string serverName = args[0],
                databaseName = args[1],
                baseDirectory = args[2];
            var server = new Server(serverName);
            Console.WriteLine("Server: {0}. Version: {1}", serverName, server.Information.Version);
            var db = server.Databases[databaseName];
            Console.WriteLine("Database name: {0}.", databaseName);
            CheckDirectoryInternalPaths(baseDirectory);
            foreach (Table table in db.Tables)
            {
                if (table.IsSystemObject)
                    continue;

                string fileName = string.Format("{0}\\{1}.{2}.sql",
                    Path.Combine(baseDirectory, TablesDirectoryName), table.Schema, table.Name);
                var script = new Script() { ScriptableObject = table };
                script.Save(fileName);
                Console.WriteLine("Script for the table {0} has been created.", table.Name);
                Console.WriteLine("------------------================================------------------");
            }

            foreach (StoredProcedure procedure in db.StoredProcedures)
            {
                if (procedure.IsSystemObject)
                    continue;

                string fileName = string.Format("{0}\\{1}.{2}.sql",
                    Path.Combine(baseDirectory, SPDirectoryName), procedure.Schema, procedure.Name);
                var script = new Script() { ScriptableObject = procedure };
                script.Save(fileName);
                Console.WriteLine("Script for the procedure {0} has been created.", procedure.Name);
                Console.WriteLine("------------------================================------------------");
            }

            foreach (UserDefinedFunction function in db.UserDefinedFunctions)
            {
                if (function.IsSystemObject)
                    continue;

                string fileName = string.Format("{0}\\{1}.{2}.sql",
                    Path.Combine(baseDirectory, FunctionDirectoryName), function.Schema, function.Name);
                var script = new Script() { ScriptableObject = function };
                script.Save(fileName);
                Console.WriteLine("Script for the function {0} has been created.", function.Name);
                Console.WriteLine("------------------================================------------------");
            }

            foreach (View view in db.Views)
            {
                if (view.IsSystemObject)
                    continue;

                string fileName = string.Format("{0}\\{1}.{2}.sql",
                    Path.Combine(baseDirectory, ViewsDirectoryName), view.Schema, view.Name);
                var script = new Script() { ScriptableObject = view };
                script.Save(fileName);
                Console.WriteLine("Script for the view {0} has been created.", view.Name);
                Console.WriteLine("------------------================================------------------");
            }
            Console.ReadKey();
        }

        private static readonly string TablesDirectoryName = "Tables";
        private static readonly string SPDirectoryName = "Stored Procedures";
        private static readonly string FunctionDirectoryName = "Functions";
        private static readonly string ViewsDirectoryName = "Views";

        private static void CheckDirectoryInternalPaths(string baseDirectory)
        {
            var tablesDir = Path.Combine(baseDirectory, TablesDirectoryName);
            var proceduresDir = Path.Combine(baseDirectory, SPDirectoryName);
            var functionsDir = Path.Combine(baseDirectory, FunctionDirectoryName);
            var viewsDir = Path.Combine(baseDirectory, ViewsDirectoryName);

            if (!Directory.Exists(tablesDir))
                Directory.CreateDirectory(tablesDir);
            if (!Directory.Exists(proceduresDir))
                Directory.CreateDirectory(proceduresDir);
            if (!Directory.Exists(viewsDir))
                Directory.CreateDirectory(viewsDir);
            if (!Directory.Exists(functionsDir))
                Directory.CreateDirectory(functionsDir);
        }
    }
}
