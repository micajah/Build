using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.IO;

namespace micajah.databasescripter
{
    internal class Script
    {
        internal IScriptable ScriptableObject;
        internal string FileName;

        public void Save(string fileName)
        {
            FileName = fileName;
            Save();
        }

        public void Save()
        {
            if(string.IsNullOrEmpty(FileName))
                throw new ArgumentException("FileName should be initialized.");

            var options = new ScriptingOptions();
            options.FileName = FileName;
            options.Indexes = true;
            options.ToFileOnly = true;
            options.Triggers = true;
            options.XmlIndexes = true;
            options.ClusteredIndexes = true;
            options.DriAllConstraints = true;
            options.DriAllKeys = true;
            options.DriChecks = true;
            options.DriPrimaryKey = true;
            options.DriUniqueKeys = true;
            options.ScriptSchema = true;

            ScriptableObject.Script(options);
        }

        public void AppendFile()
        {
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentException("FileName should be initialized.");

            var options = new ScriptingOptions();
            
            foreach (var testRow in ScriptableObject.Script(options))
            {
                File.AppendAllText(FileName, testRow, Encoding.Unicode);
            }
        }
    }
}
