using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Collections;
using System.Globalization;

namespace MITS.MSBuild.Tasks
{
    public class DBDeploy : Task
    {
        #region Private fields

        private string _dbProjectPath;

        private string _SqlToolsPath;

        private string _SqlDeltaPath;

        private string _ServerName;

        private string _SQLDeltaProjectName;

        private string _TFexePath;

        private string _GUID;

        private string _ProductBuildNumber;

        private string _TargetDatabase;

        private string _BlankDatabase;

        private string _ConnectionString;
        private bool _WaitForExit;
        //private ITaskItem[] _sqlScripts;       

        private string TempSQLLog;

        string DBDeployLogFile;

        string TriggersCreateFile;

        string TempCompareReportFolder;
        #endregion

        #region Public fields

        [Required]
        public string DBProjectPath
        {
            get { return _dbProjectPath; }
            set { _dbProjectPath = value; }
        }

        [Required]
        public string SqlToolsPath
        {
            get { return _SqlToolsPath; }
            set { _SqlToolsPath = value; }
        }

        [Required]
        public string SqlDeltaPath
        {
            get { return _SqlDeltaPath; }
            set { _SqlDeltaPath = value; }
        }

        [Required]
        public string ServerName
        {
            get { return _ServerName; }
            set { _ServerName = value; }
        }

        [Required]
        public string SQLDeltaProject
        {
            get { return _SQLDeltaProjectName; }
            set { _SQLDeltaProjectName = value; }
        }

        [Required]
        public string TFexe
        {
            get { return _TFexePath; }
            set { _TFexePath = value; }
        }

        [Required]
        public string GUID
        {
            get { return _GUID; }
            set { _GUID = value; }
        }

        [Required]
        public string ProductBuildNumber
        {
            get { return _ProductBuildNumber; }
            set { _ProductBuildNumber = value; }
        }

        [Required]
        public string TargetDatabase
        {
            get { return _TargetDatabase; }
            set { _TargetDatabase = value; }
        }

        [Required]
        public string BlankDatabase
        {
            get { return _BlankDatabase; }
            set { _BlankDatabase = value; }
        }

        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        public bool WaitForExit
        {
            get { return _WaitForExit; }
            set { _WaitForExit = value; }
        }

        #endregion

        public DBDeploy()
        {
            _ConnectionString = "-E";
            _WaitForExit = true;
        }

        public override bool Execute()
        {

            #region Build reports initialization
            TempCompareReportFolder = DBProjectPath + @"\Build Reports\Beta\Build (" + ProductBuildNumber + ")";
            DBDeployLogFile = TempCompareReportFolder + @"\DBDeployLog.txt";
            Directory.CreateDirectory(TempCompareReportFolder);
            #endregion
            
            #region Log Properties
            LogDBDeployLog("DBDeploy task starting...");
            LogDBDeployLog("dbProjectPath=" + _dbProjectPath);
            LogDBDeployLog("SqlToolsPath=" + _SqlToolsPath);
            LogDBDeployLog("SqlDeltaPath=" + _SqlDeltaPath);
            LogDBDeployLog("ServerName=" + _ServerName);
            LogDBDeployLog("SQLDeltaProjectName=" + _SQLDeltaProjectName);
            LogDBDeployLog("TFexe=" + TFexe);
            LogDBDeployLog("ConnectionString=" + _ConnectionString);
            LogDBDeployLog("WaitForExit=" + _WaitForExit.ToString());
            #endregion

            try 
            {
                #region Initialization
                // Standard DB
                string[] CreateStDBScript = { DBProjectPath + @"\CreateTemporaryDatabase.sql" };
                string[] DeleteStDBScript = { DBProjectPath + @"\DeleteTemporaryDatabase.sql" };
                // db folders
                string CreateScriptsFolder = DBProjectPath + @"\Create Scripts\";
                string InsertScriptsFolder = DBProjectPath + @"\Insert Data\";
                string PreBuildScriptsFolder = DBProjectPath + @"\PreBuild Custom Scripts\";
                string PostBuildScriptsFolder = DBProjectPath + @"\PostBuild Custom Scripts\";
                string CompiledScriptsFolder = DBProjectPath + @"\Compiled Change Scripts\";
                // db project file
                string DBProjectFile = DBProjectPath + @"\db.MITS.Deploy.dbp";
                // Temp folder
                string TempFolder = DBProjectPath + @"\TempDir";
                Directory.CreateDirectory(TempFolder); 
                // temp update/rollback files
                string TempUpdateFile = TempFolder + @"\DBSchemaUpdate.sql";
                string TempRollbackFile = TempFolder + @"\DBSchemaRollBack.sql";
                string TempDataUpdateFile = TempFolder + @"\DBDataUpdate.sql";
                string TempDataRollbackFile = TempFolder + @"\DBDataRollBack.sql";                
                // Temp SQL Log
                TempSQLLog = TempFolder + @"\SQLOutputLog.txt";
                // SQL Delta Error File
                string SQLDeltaErrorFile = SqlDeltaPath.Replace("SQLDelta.exe", "CommandLineErrors.txt");
                string TempCompareReportFile = TempCompareReportFolder + @"\DBCompareReport.html";
                // scripts
                string[] CreateSQLFiles;
                string[] InsertSQLFiles;
                string[] PrebuildSQLFiles;
                string[] PostbuildSQLFiles;
                string TempSQLScript = CreateScriptsFolder + "_RunTablesScripts._ql";
                string TempSQLConScript = CreateScriptsFolder + "_RunWOTablesScripts._ql";
                TriggersCreateFile = CreateScriptsFolder + "_RunTriggerScript._ql";
                string UseBlankDB = "use [" + BlankDatabase + "] \r\n GO\r\n";
                // XML 
                string TempUpdateXML = TempFolder + @"\Updates.xml";
                string TableXMLFile = InsertScriptsFolder + "Tables.xml";
                string UpdateXMLFile = CompiledScriptsFolder + "Updates.xml";
                string UpdateComment = "One more update";
                string UpdateDeveloper = "Yuriy Mykytyuk";
                string UpdateVia = "TFS Checkin";
                #endregion

                #region Check chapter
                if (string.IsNullOrEmpty(SqlToolsPath))
                {
                    LogErrorDBDeployLog("The required properties have not been set.  Be sure that the SqlToolsPath property has been set.");
                    SaveDBDeployLog();
                    return false;
                }
                else
                {
                    if (!File.Exists(SqlToolsPath))
                    {
                        LogErrorDBDeployLog("Error: SqlToolsPath property is incorrectly set. File don't exist.");
                        SaveDBDeployLog();
                        return false;
                    }
                }
                if (string.IsNullOrEmpty(DBProjectPath))
                {
                    LogErrorDBDeployLog("The required properties have not been set.  Be sure that the DBProjectPath property has been set.");
                    SaveDBDeployLog();
                    return false;
                }
                else
                {
                    if (!Directory.Exists(DBProjectPath))
                    {
                        LogErrorDBDeployLog("Error: DBProjectPath property is incorrectly set. Directory don't exist.");
                        SaveDBDeployLog();
                        return false;
                    }
                }
                if (string.IsNullOrEmpty(SqlDeltaPath))
                {
                    LogErrorDBDeployLog("The required properties have not been set.  Be sure that the SqlDeltaPath property has been set.");
                    SaveDBDeployLog();
                    return false;
                }
                else
                {
                    if (!File.Exists(SqlDeltaPath))
                    {
                        LogErrorDBDeployLog("Error: SqlDeltaPath property is incorrectly set. File don't exist.");
                        SaveDBDeployLog();
                        return false;
                    }
                }
                if (string.IsNullOrEmpty(SQLDeltaProject))
                {
                    LogErrorDBDeployLog("The required properties have not been set.  Be sure that the SQLDeltaProject property has been set.");
                    SaveDBDeployLog();
                    return false;
                }
                else
                {
                    if (!File.Exists(SQLDeltaProject))
                    {
                        LogErrorDBDeployLog("Error: SQLDeltaProject property is incorrectly set. File don't exist.");
                        SaveDBDeployLog();
                        return false;
                    }
                }
                if (string.IsNullOrEmpty(TFexe))
                {
                    LogErrorDBDeployLog("The required properties have not been set.  Be sure that the TFexe property has been set.");
                    SaveDBDeployLog();
                    return false;
                }
                else
                {
                    if (!File.Exists(TFexe))
                    {
                        LogErrorDBDeployLog("Error: TFexe property is incorrectly set. File don't exist.");
                        SaveDBDeployLog();
                        return false;
                    }
                }
                if ((!File.Exists(CreateStDBScript[0])) || (!File.Exists(DeleteStDBScript[0])))
                {
                    LogErrorDBDeployLog("Error: Create or/and Delete Scripts for Standard DB are missed.");
                    SaveDBDeployLog();
                    return false;
                }
                if (string.IsNullOrEmpty(BlankDatabase))
                {
                    LogErrorDBDeployLog("The required properties have not been set.  Be sure that the BlankDatabase property has been set.");
                    SaveDBDeployLog();
                    return false;
                }

                if (string.IsNullOrEmpty(TargetDatabase))
                {
                    LogErrorDBDeployLog("The required properties have not been set.  Be sure that the TargetDatabase property has been set.");
                    SaveDBDeployLog();
                    return false;
                }
                #endregion

                try
                {
                    #region Create Standard Database and Running Scripts
                    // Create Temp Database
                    foreach (string f in CreateStDBScript)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }
                    #endregion

                    #region UDT scripts
                    string[] UDTFiles = Directory.GetFiles(CreateScriptsFolder, "*.udt");
                    foreach (string f in UDTFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }
                    #endregion

                    #region Tables script
                    // Run Create Scripts - Tables
                    File.Delete(TriggersCreateFile);
                    File.AppendAllText(TriggersCreateFile, UseBlankDB);
                    File.Delete(TempSQLScript);
                    string CreateTableString = "";
                    CreateSQLFiles = Directory.GetFiles(CreateScriptsFolder, "*.tab");
                    File.AppendAllText(TempSQLScript, UseBlankDB);
                    foreach (string f in CreateSQLFiles)
                    {
                        //!!!Run one script with "CREATE TABLE" statements
                        StreamReader SQLReader = File.OpenText(f);
                        CreateTableString = ExctractTable(SQLReader.ReadToEnd());
                        File.AppendAllText(TempSQLScript, CreateTableString + "\r\n");                    
                    }                   
                    if (File.Exists(TempSQLScript))
                    {
                        RunOsql(GetOsqlArguments(TempSQLScript));
                        LogSQLMessage();
                        //File.Delete(TempSQLScript);
                    }                    

                    //Run create scripts without table drop and create
                    // Run Create Scripts - Tables
                    File.Delete(TempSQLConScript);                    
                    string NoCreateTableString = "";
                    File.AppendAllText(TempSQLConScript, UseBlankDB);                    
                    foreach (string f in CreateSQLFiles)
                    {
                        //!!!Run one script without "CREATE TABLE" statement
                        StreamReader SQLWOTableReader = File.OpenText(f);
                        NoCreateTableString = ExctractWOTable(SQLWOTableReader.ReadToEnd());
                        File.AppendAllText(TempSQLConScript, NoCreateTableString + "\r\n");                    
                    }                    
                    if (File.Exists(TempSQLConScript))
                    {
                        RunOsql(GetOsqlArguments(TempSQLConScript));
                        LogSQLMessage();
                        //File.Delete(TempSQLConScript);
                    }                    
                    #endregion

                    #region Other independent scripts
                    string[] KCIFiles = Directory.GetFiles(CreateScriptsFolder, "*.kci");
                    foreach (string f in KCIFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }

                    string[] DEFFiles = Directory.GetFiles(CreateScriptsFolder, "*.def");
                    foreach (string f in DEFFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }

                    string[] RULFiles = Directory.GetFiles(CreateScriptsFolder, "*.rul");
                    foreach (string f in RULFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }

                    string[] IDNFiles = Directory.GetFiles(CreateScriptsFolder, "*.idn");
                    foreach (string f in IDNFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }

                    string[] EXTFiles = Directory.GetFiles(CreateScriptsFolder, "*.ext");
                    foreach (string f in EXTFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }

                    string[] BNDFiles = Directory.GetFiles(CreateScriptsFolder, "*.bnd");
                    foreach (string f in BNDFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }

                    string[] DRNFiles = Directory.GetFiles(CreateScriptsFolder, "*.drn");
                    foreach (string f in DRNFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }
                    #endregion

                    #region Run dependent scripts (recursive)
                    string[] FuncFiles = Directory.GetFiles(CreateScriptsFolder, "*.udf");
                    string[] ViewFiles = Directory.GetFiles(CreateScriptsFolder, "*.viw");
                    string[] SPFiles = Directory.GetFiles(CreateScriptsFolder, "*.prc");
                    ArrayList DepFiles = new ArrayList();
                    DepFiles.AddRange(FuncFiles);
                    DepFiles.AddRange(ViewFiles);
                    DepFiles.AddRange(SPFiles);                    
                    LogDBDeployLog("DepFiles=" + DepFiles.Count);
                    RunDependentScripts(DepFiles);
                    #endregion

                    #region Run trigers script
                    if (File.Exists(TriggersCreateFile))
                    {
                        RunOsql(GetOsqlArguments(TriggersCreateFile));
                        LogSQLMessage();
                        //File.Delete(TriggersCreateFile);
                    }
                    string[] TRGFiles = Directory.GetFiles(CreateScriptsFolder, "*.trg");
                    foreach (string f in TRGFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }
                    #endregion
                    
                    #region Run other sql scripts
                    string[] OtherFiles = Directory.GetFiles(CreateScriptsFolder, "*.sql");
                    foreach (string f in OtherFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }
                    #endregion

                    #region Run Insert + Prebuild Scripts
                    InsertSQLFiles = Directory.GetFiles(InsertScriptsFolder, "*.sql");
                    foreach (string f in InsertSQLFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }

                    // Run Prebuild Custom Scripts                
                    PrebuildSQLFiles = Directory.GetFiles(PreBuildScriptsFolder);
                    foreach (string f in PrebuildSQLFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }
                    #endregion
                }
                catch (CalledProcessErrorExitException e)
                {
                    #region SQL scripts failed
                    LogSQLMessage();
                    // Delete standard database
                    LogErrorDBDeployLog(e.Message);
                    LogDBDeployLog("Exception was raised during standard database construct process! Try to delete standard database...");
                    if (!DeleteStandardDB(DeleteStDBScript))
                    {
                        LogErrorDBDeployLog("After raising exception standard database cannot be deleted!");
                    }
                    else
                    {
                        LogDBDeployLog("After raising exception during SQL scripts running standard database have been deleted!");
                    }
                    SaveDBDeployLog();
                    return false;
                    #endregion
                }

                try
                {
                    #region SQL Delta chapter
                    if (File.Exists(TempUpdateFile))
                    {
                        File.Delete(TempUpdateFile);
                    }
                    if (File.Exists(TempDataUpdateFile))
                    {
                        File.Delete(TempDataUpdateFile);
                    }
                    if (File.Exists(TempCompareReportFile))
                    {
                        File.Delete(TempCompareReportFile);
                    }
                    string NewSQLDeltaProject = TempFolder + @"\SQLDeltaProjectFile_edited.sdp";
                    StreamReader ProjectReader = File.OpenText(SQLDeltaProject);                    
                    string ProjectString = ProjectReader.ReadToEnd();                    
                    ProjectString = ProjectString.Replace("UpdateScriptFileNEEDBECHANGED", TempUpdateFile);
                    ProjectString = ProjectString.Replace("RollbackScriptFileNEEDBECHANGED", TempRollbackFile);
                    ProjectString = ProjectString.Replace("DataUpdateFileHere", TempDataUpdateFile);
                    ProjectString = ProjectString.Replace("DataRollbackFileHere", TempDataRollbackFile);
                    ProjectString = ProjectString.Replace("_HereReportFile", TempCompareReportFile);
                    File.Delete(NewSQLDeltaProject);
                    File.AppendAllText(NewSQLDeltaProject, ProjectString);                    
                    
                    // Add Insert tables for data compare
                    if (!File.Exists(TableXMLFile))
                    {
                        LogDBDeployLog("File Tables.xml is missing in Insert Data folder! No dictionary data will be comparing.");
                    }
                    else
                    {
                        DataSet TablesFileSet = new DataSet("TablesList");
                        TablesFileSet.ReadXml(TableXMLFile);
                        // Edit SQL Delta project
                        XmlDocument DeltaFile = new XmlDocument();
                        DeltaFile.Load(NewSQLDeltaProject);
                        for (int i = 0; i < TablesFileSet.Tables[0].Rows.Count; i++)
                        {
                            XmlElement userElement = DeltaFile.CreateElement("SItem");

                            XmlNode newElemTable;
                            newElemTable = DeltaFile.CreateNode(XmlNodeType.Element, "Name", "");
                            newElemTable.InnerText = TablesFileSet.Tables[0].Rows[i]["TableName"].ToString();

                            XmlNode newElemKey;
                            newElemKey = DeltaFile.CreateNode(XmlNodeType.Element, "Key", "");
                            newElemKey.InnerText = TablesFileSet.Tables[0].Rows[i]["Key"].ToString();

                            XmlNode newElemColumns;
                            newElemColumns = DeltaFile.CreateNode(XmlNodeType.Element, "Columns", "");
                            newElemColumns.InnerText = TablesFileSet.Tables[0].Rows[i]["Columns"].ToString();

                            XmlNode newElemSelectedColumns;
                            newElemSelectedColumns = DeltaFile.CreateNode(XmlNodeType.Element, "SelectedColumns", "");
                            newElemSelectedColumns.InnerText = TablesFileSet.Tables[0].Rows[i]["Columns"].ToString();

                            XmlNode newElemAllColumnsSelected;
                            newElemAllColumnsSelected = DeltaFile.CreateNode(XmlNodeType.Element, "AllColumnsSelected", "");
                            newElemAllColumnsSelected.InnerText = "True";

                            XmlNode newElemInsertAll;
                            newElemInsertAll = DeltaFile.CreateNode(XmlNodeType.Element, "InsertAll", "");
                            newElemInsertAll.InnerText = "True";

                            XmlNode newElemFilterA;
                            newElemFilterA = DeltaFile.CreateNode(XmlNodeType.Element, "FilterA", "");
                            XmlNode newElemFilterB;
                            newElemFilterB = DeltaFile.CreateNode(XmlNodeType.Element, "FilterB", "");

                            XmlNode newElemUpdate;
                            newElemUpdate = DeltaFile.CreateNode(XmlNodeType.Element, "Update", "");
                            newElemUpdate.InnerText = "True";

                            XmlNode newElemUpdateModified;
                            newElemUpdateModified = DeltaFile.CreateNode(XmlNodeType.Element, "UpdateModified", "");
                            newElemUpdateModified.InnerText = "True";

                            XmlNode newElemUpdateMissing;
                            newElemUpdateMissing = DeltaFile.CreateNode(XmlNodeType.Element, "UpdateMissing", "");
                            newElemUpdateMissing.InnerText = "True";

                            XmlNode newElemUpdateAdded;
                            newElemUpdateAdded = DeltaFile.CreateNode(XmlNodeType.Element, "UpdateAdded", "");
                            newElemUpdateAdded.InnerText = "True";

                            XmlNode newElemDirection;
                            newElemDirection = DeltaFile.CreateNode(XmlNodeType.Element, "Direction", "");
                            newElemDirection.InnerText = "B";

                            XmlNode newElemDisableTriggers;
                            newElemDisableTriggers = DeltaFile.CreateNode(XmlNodeType.Element, "DisableTriggers", "");
                            newElemDisableTriggers.InnerText = "True";

                            XmlNode newElemDisableIndexesAndConstraints;
                            newElemDisableIndexesAndConstraints = DeltaFile.CreateNode(XmlNodeType.Element, "DisableIndexesAndConstraints", "");
                            newElemDisableIndexesAndConstraints.InnerText = "True";


                            userElement.AppendChild(newElemTable);
                            userElement.AppendChild(newElemKey);
                            userElement.AppendChild(newElemColumns);
                            userElement.AppendChild(newElemSelectedColumns);
                            userElement.AppendChild(newElemAllColumnsSelected);
                            userElement.AppendChild(newElemInsertAll);
                            userElement.AppendChild(newElemFilterA);
                            userElement.AppendChild(newElemFilterB);
                            userElement.AppendChild(newElemUpdate);
                            userElement.AppendChild(newElemUpdateModified);
                            userElement.AppendChild(newElemUpdateMissing);
                            userElement.AppendChild(newElemUpdateAdded);
                            userElement.AppendChild(newElemDirection);
                            userElement.AppendChild(newElemDisableTriggers);
                            userElement.AppendChild(newElemDisableIndexesAndConstraints);
                            XmlNode DataNode = DeltaFile.SelectSingleNode("/Project/Data/Selected");
                            DataNode.AppendChild(userElement);
                        }
                        DeltaFile.Save(NewSQLDeltaProject);
                    }
                    

                    // Delete CommandLineErrors.txt
                    File.Delete(SQLDeltaErrorFile);

                    // Run SQL Delta comparing
                    LogDBDeployLog("Running SQL DELTA: " + SqlDeltaPath + " " + NewSQLDeltaProject);
                    RunProcess(SqlDeltaPath, NewSQLDeltaProject);
                    #endregion
                }
                catch (CalledProcessErrorExitException e)
                {
                    #region SQL Delta failed
                    // Delete standard database
                    LogErrorDBDeployLog(e.Message);
                    LogDBDeployLog("Exception was raised when try to run SQL Delta! Try to delete standard database...");
                    if (!DeleteStandardDB(DeleteStDBScript))
                    {
                        LogErrorDBDeployLog("After raising exception standard database cannot be deleted!");
                    }
                    else
                    {
                        LogDBDeployLog("After raising exception standard database have been deleted!");
                    }
                    SaveDBDeployLog();
                    return false;
                    #endregion
                }

                #region Check SQL Delta Error File
                // Check for SQL Delta Error File!!!
                if (File.Exists(SQLDeltaErrorFile))
                {
                    LogErrorDBDeployLog("SQL Delta process generate error file:");
                    LogDeltaError();
                    LogDBDeployLog("Exception was raised when try to run SQL Delta! Try to delete standard database...");
                    if (!DeleteStandardDB(DeleteStDBScript))
                    {
                        LogErrorDBDeployLog("After raising exception standard database cannot be deleted!");
                    }
                    else
                    {
                        LogDBDeployLog("After raising exception standard database have been deleted!");
                    }
                    SaveDBDeployLog();
                    return false;
                }
                #endregion

                try
                {
                    #region Run POST SQL scripts
                    // Run Postbuild Custom Scripts                
                    PostbuildSQLFiles = Directory.GetFiles(PostBuildScriptsFolder);
                    foreach (string f in PostbuildSQLFiles)
                    {
                        RunOsql(GetOsqlArguments(f));
                        LogSQLMessage();
                    }
                    #endregion
                }
                catch (CalledProcessErrorExitException e)
                {
                    #region POST SQL scripts failed
                    LogSQLMessage();
                    // Delete standard database
                    LogErrorDBDeployLog(e.Message);
                    LogDBDeployLog("Exception was raised during executing PostBuild SQL Scripts! Try to delete standard database...");
                    if (!DeleteStandardDB(DeleteStDBScript))
                    {
                        LogErrorDBDeployLog("After raising exception standard database cannot be deleted!");
                    }
                    else
                    {
                        LogDBDeployLog("After raising exception during PostBuild SQL scripts running standard database have been deleted!");
                    }
                    SaveDBDeployLog();
                    return false;
                    #endregion
                }
                finally
                {
                    #region Delete Standard Database
                    //Delete Temp Database
                    DeleteStandardDB(DeleteStDBScript);
                    #endregion
                }

                #region Check if update script exists
                if ((!File.Exists(TempUpdateFile)) && (!File.Exists(TempDataUpdateFile)))
                {
                    // Databases are the same version
                    LogDBDeployLog("Schemes and dictionary data of standard and target databases are the same. No update script is generated!");
                    SaveDBDeployLog();
                    return true;
                }
                #endregion

                #region Get current version of database. Update for next version
                int dbCurVersion = -1, dbNextVersion = -1;
                string VersionConString = "Data Source=" + ServerName + "; Initial Catalog=" + TargetDatabase + "; Integrated Security=SSPI";
                SqlConnection VerCon = new SqlConnection(VersionConString);
                SqlCommand VerCommand = new SqlCommand("SELECT Max(Version) AS MaxVer FROM MC_DBVersion", VerCon);
                try
                {
                    VerCon.Open();
                    string ReceivedObject = VerCommand.ExecuteScalar().ToString();
                    if (string.IsNullOrEmpty(ReceivedObject))
                    {
                        LogErrorDBDeployLog("Task cannot get current DB version from mc_DBVersion table! No update script will be generated!");
                        SaveDBDeployLog();
                        return false;
                    }
                    if (!int.TryParse(ReceivedObject, out dbCurVersion))
                    {
                        LogErrorDBDeployLog("Task cannot get parse DB version from mc_DBVersion table! No update script will be generated!");
                        SaveDBDeployLog();
                        return false;
                    }
                    // Get and check GUI
                    VerCommand.CommandText = "SELECT GUID FROM MC_DBVersion WHERE Version=" + dbCurVersion.ToString();
                    ReceivedObject = VerCommand.ExecuteScalar().ToString();
                    if (string.IsNullOrEmpty(ReceivedObject))
                    {
                        LogErrorDBDeployLog("Task cannot get current product GUID from mc_DBVersion table! No update script will be generated!");
                        SaveDBDeployLog();
                        return false;
                    }
                    if (ReceivedObject != GUID)
                    {
                        LogErrorDBDeployLog("Product GUID is erroneous. No update script will be generated!");
                        SaveDBDeployLog();
                        return false;
                    }

                    dbNextVersion = dbCurVersion + 1;
                    VerCommand.CommandText = "UPDATE MC_DBVersion SET Version=" + dbNextVersion.ToString() + ", AddDate='" + DateTime.Now.ToString() + "' WHERE Version=" + dbCurVersion.ToString();
                    VerCommand.ExecuteNonQuery();
                }
                finally
                {
                    LogDBDeployLog("Try to close connection to target database...");
                    VerCon.Close();
                    LogDBDeployLog("Connection is closed.");
                }
                #endregion

                #region Merge and Add current update script
                string dbCurVersionString = dbCurVersion.ToString();
                string dbNextVersionString = dbNextVersion.ToString();
                if (dbCurVersion < 9999)
                {
                    dbCurVersionString = "00000" + dbCurVersionString;
                    dbCurVersionString = dbCurVersionString.Substring(dbCurVersionString.Length - 5);
                }
                if (dbNextVersion < 9999)
                {
                    dbNextVersionString = "00000" + dbNextVersionString;
                    dbNextVersionString = dbNextVersionString.Substring(dbNextVersionString.Length - 5);
                }
                string CopmiledScriptName = "DB_v" + dbCurVersionString + "_v" + dbNextVersionString + ".sql";
                // Check if exists the same version
                if (File.Exists(CompiledScriptsFolder + CopmiledScriptName))
                {
                    LogErrorDBDeployLog(@"The file which have the same name as generated update script already exists in \Compiled Change Scripts Folder\.");
                    try
                    {
                        LogDBDeployLog("Try to undo mc_DBVersion update...");
                        VerCon.Open();
                        VerCommand.CommandText = "UPDATE MC_DBVersion SET Version=" + dbCurVersion.ToString() + ", AddDate=NULL WHERE Version=" + dbNextVersion.ToString();
                        VerCommand.ExecuteNonQuery();
                    }
                    finally
                    {
                        LogDBDeployLog("Try to close connection to target database...");
                        VerCon.Close();
                        LogDBDeployLog("Connection is closed.");
                    }
                    SaveDBDeployLog();
                    return false;
                }
                string CompileScriptPath = "";
                try
                {
                    // Run merging SQL scripts                    
                    CompileScriptPath = CompiledScriptsFolder + CopmiledScriptName;                    
                    StreamReader UpdateReader;
                    File.AppendAllText(CompileScriptPath, "use [" + TargetDatabase + "]\r\nGO\r\n");
                    File.AppendAllText(CompileScriptPath, "BEGIN TRANSACTION\r\nGO\r\nSET TRANSACTION ISOLATION LEVEL SERIALIZABLE\r\nGO\r\n");                    
                    foreach (string f in PrebuildSQLFiles)
                    {
                        UpdateReader = File.OpenText(f);
                        File.AppendAllText(CompileScriptPath, UpdateReader.ReadToEnd() + "\r\n");
                    }
                    if (File.Exists(TempUpdateFile))
                    {// +1
                        UpdateReader = File.OpenText(TempUpdateFile);
                        string SchemeUpdateContent = UpdateReader.ReadToEnd();
                        SchemeUpdateContent = SchemeUpdateContent.Replace("@@TRANCOUNT = 1", "@@TRANCOUNT = 2");
                        File.AppendAllText(CompileScriptPath, SchemeUpdateContent + "\r\n");                        
                    }
                    if (File.Exists(TempDataUpdateFile))
                    {
                        UpdateReader = File.OpenText(TempDataUpdateFile);
                        string DataUpdateContent = UpdateReader.ReadToEnd();
                        DataUpdateContent = DataUpdateContent.Replace("@@TRANCOUNT = 1", "@@TRANCOUNT = 2");
                        File.AppendAllText(CompileScriptPath, DataUpdateContent + "\r\n");                                                
                    }
                    foreach (string f in PostbuildSQLFiles)
                    {
                        UpdateReader = File.OpenText(f);
                        File.AppendAllText(CompileScriptPath, UpdateReader.ReadToEnd() + "\r\n");
                    }
                    File.AppendAllText(CompileScriptPath, "IF @@TRANCOUNT = 1\r\nBEGIN\r\n   COMMIT TRANSACTION\r\nEND\r\n");
                    
                    // Add and Check in current update script                    
                    string AddCommand = "add \"" + CompiledScriptsFolder + CopmiledScriptName + "\" /noprompt";
                    LogDBDeployLog("Running TF.exe: " + TFexe + " " + AddCommand);
                    RunProcess(TFexe, AddCommand);

                    try
                    {
                        string ScriptCheckinCommand = "checkin \"" + CompiledScriptsFolder + CopmiledScriptName + "\" /noprompt";
                        LogDBDeployLog("Running TF.exe: " + TFexe + " " + ScriptCheckinCommand);
                        RunProcess(TFexe, ScriptCheckinCommand);
                    }
                    catch (CalledProcessErrorExitException e)
                    {
                        LogErrorDBDeployLog("Error occured when check in update script to source control. " + e.Message + " Try to undo adding...");
                        string ScriptScriptUndoCommand = "undo \"" + CompiledScriptsFolder + CopmiledScriptName + "\" /recursive";
                        LogDBDeployLog("Running TF.exe: " + TFexe + " " + ScriptScriptUndoCommand);
                        RunProcess(TFexe, ScriptScriptUndoCommand);
                        LogErrorDBDeployLog("Adding generated script is canceled.");
                        throw new CalledProcessErrorExitException();
                    }
                }
                catch
                {
                    LogErrorDBDeployLog("Error occured when adding update script to source control");
                    try
                    {
                        LogDBDeployLog("Try to undo mc_DBVersion update...");
                        VerCon.Open();
                        VerCommand.CommandText = "UPDATE MC_DBVersion SET Version=" + dbCurVersion.ToString() + ", AddDate=NULL WHERE Version=" + dbNextVersion.ToString();
                        VerCommand.ExecuteNonQuery();
                    }
                    finally
                    {
                        LogDBDeployLog("Try to close connection to target database...");
                        VerCon.Close();
                        LogDBDeployLog("Connection is closed.");
                    }
                    SaveDBDeployLog();
                    return false;
                }
                #endregion

                #region Edit DB project file
                bool checkoutdbsuccess = false;
                try
                {
                    // Check out db project                    
                    string CheckoutdbCommand = "checkout \"" + DBProjectFile + "\" /noprompt";
                    LogDBDeployLog("Running TF.exe: " + TFexe + " " + CheckoutdbCommand);
                    RunProcess(TFexe, CheckoutdbCommand);
                    checkoutdbsuccess = true;

                    // Edit db project: add new update script
                    string DBProjectString = File.ReadAllText(DBProjectFile);
                    int CompileFolderIndex = DBProjectString.IndexOf("\"Compiled Change Scripts\"");                    
                    DBProjectString = DBProjectString.Insert(CompileFolderIndex + 25, "\n      Script = \"" + CopmiledScriptName + "\"");                                                           
                    File.Delete(DBProjectFile);
                    File.AppendAllText(DBProjectFile, DBProjectString);

                    // Checkin db project
                    string CheckindbCommand = "checkin \"" + DBProjectFile + "\" /noprompt";
                    LogDBDeployLog("Running TF.exe: " + TFexe + " " + CheckindbCommand);
                    RunProcess(TFexe, CheckindbCommand);
                    checkoutdbsuccess = false;
                }
                catch
                {
                    LogErrorDBDeployLog("Error occured when try to edit db project file .dbp. Try to undo and delete generated script...");
                    if (checkoutdbsuccess)
                    {
                        string UndodbCommand = "undo \"" + DBProjectFile + "\" /recursive";
                        LogDBDeployLog("Running TF.exe: " + TFexe + " " + UndodbCommand);
                        RunProcess(TFexe, UndodbCommand);
                    }
                    // try to delete script
                    string DeleteCommand = "delete  \"" + CompiledScriptsFolder + CopmiledScriptName + "\"";
                    LogDBDeployLog("Running TF.exe: " + TFexe + " " + DeleteCommand);
                    RunProcess(TFexe, DeleteCommand);
                    try
                    {
                        string ScriptCheckinDelCommand = "checkin \"" + CompiledScriptsFolder + CopmiledScriptName + "\" /noprompt";
                        LogDBDeployLog("Running TF.exe: " + TFexe + " " + ScriptCheckinDelCommand);
                        RunProcess(TFexe, ScriptCheckinDelCommand);
                    }
                    catch (CalledProcessErrorExitException e)
                    {
                        LogErrorDBDeployLog("Error occured when check in update script to source control. " + e.Message + " Try to undo adding...");
                        string ScriptScriptUndoDelCommand = "undo \"" + CompiledScriptsFolder + CopmiledScriptName + "\" /recursive";
                        LogDBDeployLog("Running TF.exe: " + TFexe + " " + ScriptScriptUndoDelCommand);
                        RunProcess(TFexe, ScriptScriptUndoDelCommand);
                        LogErrorDBDeployLog("Deleting generated script is canceled.");
                    }
                    try
                    {
                        LogDBDeployLog("Try to undo mc_DBVersion update...");
                        VerCon.Open();
                        VerCommand.CommandText = "UPDATE MC_DBVersion SET Version=" + dbCurVersion.ToString() + ", AddDate=NULL WHERE Version=" + dbNextVersion.ToString();
                        VerCommand.ExecuteNonQuery();
                    }
                    finally
                    {
                        LogDBDeployLog("Try to close connection to target database...");
                        VerCon.Close();
                        LogDBDeployLog("Connection is closed.");
                    }
                    SaveDBDeployLog();
                    return false;
                }
                #endregion

                #region Create label for db project
                string LabelDBCommand = "";
                try
                {
                    LabelDBCommand = "label DBVersion=" + dbNextVersionString + " \"" + DBProjectPath + "\\..\" /recursive";
                    LogDBDeployLog("Running TF.exe: " + TFexe + " " + LabelDBCommand);
                    RunProcess(TFexe, LabelDBCommand);
                }
                catch (CalledProcessErrorExitException)
                {
                    LogDBDeployLog("Label DBVersion=" + dbNextVersionString + " already exists. Try to apply label DBVersion=" + dbNextVersionString + "_" + DateTime.Now.ToString().Replace(" ", "_").Replace("/", "_").Replace(":", "_"));
                    LabelDBCommand = "label DBVersion=" + dbNextVersionString + "_" + DateTime.Now.ToString().Replace(" ", "_").Replace("/", "_").Replace(":", "_") + " \"" + DBProjectPath + "\\..\" /recursive";
                    LogDBDeployLog("Running TF.exe: " + TFexe + " " + LabelDBCommand);
                    RunProcess(TFexe, LabelDBCommand);
                }
                #endregion

                #region Refresh Updates.xml
                //xml chapter
                if (!File.Exists(UpdateXMLFile))
                {
                    LogErrorDBDeployLog("File Updates.xml is missing in Compiled Change Scripts folder! Generated update script added to db project.");
                    return false;
                }
                string CheckoutUpdateCommand = "checkout \"" + UpdateXMLFile + "\" /noprompt";
                LogDBDeployLog("Running TF.exe: " + TFexe + " " + CheckoutUpdateCommand);
                RunProcess(TFexe, CheckoutUpdateCommand);
                DataSet UpdatesFileSet = new DataSet("UpdatesList");
                UpdatesFileSet.ReadXml(UpdateXMLFile);
                DataRow NewUpdateRow = UpdatesFileSet.Tables[0].NewRow();
                NewUpdateRow["ProductVersion"] = ProductBuildNumber.ToString();
                NewUpdateRow["DBVersion"] = dbNextVersion.ToString();
                NewUpdateRow["UpdateFile"] = CopmiledScriptName;
                NewUpdateRow["Comment"] = UpdateComment;
                NewUpdateRow["Developer"] = UpdateDeveloper;
                NewUpdateRow["Via"] = UpdateVia;
                UpdatesFileSet.Tables[0].Rows.Add(NewUpdateRow);
                UpdatesFileSet.Tables[0].AcceptChanges();
                UpdatesFileSet.WriteXml(UpdateXMLFile, XmlWriteMode.WriteSchema);
                string CheckinUpdateCommand = "checkin \"" + UpdateXMLFile + "\" /noprompt";
                LogDBDeployLog("Running TF.exe: " + TFexe + " " + CheckinUpdateCommand);
                RunProcess(TFexe, CheckinUpdateCommand);
                #endregion

                #region Execute update script on database
                try
                {
                    /*LogDBDeployLog("Start execute update script...");                    
                    RunOsql(GetOsqlArguments(CompileScriptPath));
                    LogSQLMessage();
                    LogDBDeployLog("Execute update script finished.");*/
                }
                catch (CalledProcessErrorExitException e)
                {
                    LogSQLMessage();                    
                    LogErrorDBDeployLog(e.Message);
                    LogDBDeployLog("Error occured during current update script " + CompileScriptPath + " executing...");
                    SaveDBDeployLog();
                    return false;
                }
                #endregion

                #region Create report and add to source control.
                SaveDBDeployLog();
                #endregion
            }
            catch (Exception ex)
            {                
                LogErrorDBDeployLog("Error running DBDeploy task" + ex.Message + "; Source=" + ex.Source + ex.ToString());
                SaveDBDeployLog();
                throw;
            }
            return true;
        }

        #region Private methods
        private bool DeleteStandardDB(string[] DeleteScripts)
        {
            try
            {
                foreach (string f in DeleteScripts)
                {
                    RunOsql(GetOsqlArguments(f));
                    LogSQLMessage();
                }
            }
            catch (CalledProcessErrorExitException e)
            {
                LogErrorDBDeployLog(e.Message);
                LogErrorDBDeployLog("Standard database cannot be deleted!");
                return false;
            }
            return true;
        }

        private string ExctractTable(string SQLScript)
        { //+ PRIMARY KEY
            string TableConst = "CREATE TABLE";
            string PrimaryConst = "PRIMARY KEY";
            string UniqueConst = "UNIQUE";
            if (!SQLScript.Contains(TableConst))
            {
                return "-- No \"CREATE TABLE\" statement";
            }
            if ((SQLScript.IndexOf(TableConst, StringComparison.OrdinalIgnoreCase)) != (SQLScript.LastIndexOf(TableConst, StringComparison.OrdinalIgnoreCase)))
            {
                throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere two or more \"CREATE TABLE\" statement!");
            }
            int CreateIndex = SQLScript.IndexOf(TableConst, StringComparison.OrdinalIgnoreCase);
            string BeforeCreate = SQLScript.Remove(CreateIndex);
            int BeforeGOIndex = BeforeCreate.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
            if (BeforeGOIndex < 0) { BeforeGOIndex = -2; }
            int AfterGOIndex = SQLScript.IndexOf("GO", CreateIndex, StringComparison.OrdinalIgnoreCase);
            if (AfterGOIndex < 0)
            {
                throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after CREATE TABLE!");
            }
            string CreateExpression = SQLScript.Substring(BeforeGOIndex + 2, AfterGOIndex - BeforeGOIndex + 1);

            int PrimIndex = SQLScript.IndexOf(PrimaryConst, StringComparison.OrdinalIgnoreCase);
            string PrimaryExpression = "";
            if (PrimIndex > 0)
            {
                string BeforePrimary = SQLScript.Remove(PrimIndex);
                int BeforePrimGOIndex = BeforePrimary.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
                if (BeforePrimGOIndex < 0) { BeforePrimGOIndex = -2; }
                int AfterPrimGOIndex = SQLScript.IndexOf("GO", PrimIndex, StringComparison.OrdinalIgnoreCase);
                if (AfterPrimGOIndex < 0)
                {
                    throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after ALTER TABLE....PRIMARY KEY!");
                }
                PrimaryExpression = SQLScript.Substring(BeforePrimGOIndex + 2, AfterPrimGOIndex - BeforePrimGOIndex + 1);
                if (!((PrimaryExpression.IndexOf("ALTER TABLE", StringComparison.OrdinalIgnoreCase) > 0) && (PrimaryExpression.IndexOf("ADD", StringComparison.OrdinalIgnoreCase) > 0) && ((PrimaryExpression.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > 0))))
                {
                    PrimaryExpression = "";
                    //return CreateExpression + "\r\n" + PrimaryExpression;
                }
                
            }

            int UniqueIndex = SQLScript.IndexOf(UniqueConst, StringComparison.OrdinalIgnoreCase);
            string UniqueExpression = "";
            if (UniqueIndex > 0)
            {
                string BeforeUnique = SQLScript.Remove(UniqueIndex);
                int BeforeUniqueGOIndex = BeforeUnique.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
                if (BeforeUniqueGOIndex < 0) { BeforeUniqueGOIndex = -2; }
                int AfterUniqueGOIndex = SQLScript.IndexOf("GO", UniqueIndex, StringComparison.OrdinalIgnoreCase);
                if (AfterUniqueGOIndex < 0)
                {
                    throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after ALTER TABLE....UNIQUE!");
                }
                UniqueExpression = SQLScript.Substring(BeforeUniqueGOIndex + 2, AfterUniqueGOIndex - BeforeUniqueGOIndex + 1);
                if (!((UniqueExpression.IndexOf("ALTER TABLE", StringComparison.OrdinalIgnoreCase) > 0) && (UniqueExpression.IndexOf("ADD", StringComparison.OrdinalIgnoreCase) > 0) && ((UniqueExpression.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > 0))))
                {
                    UniqueExpression = "";
                    //return CreateExpression + "\r\n" + PrimaryExpression + "\r\n" + UniqueExpression; 
                }
            }

            return CreateExpression + "\r\n" + PrimaryExpression + "\r\n" + UniqueExpression;
        }

        private string ExctractWOTable(string SQLScript)
        {
            string TableConst = "CREATE TABLE";
            string DropConst = "DROP TABLE";
            string PrimaryConst = "PRIMARY KEY";
            string UniqueConst = "UNIQUE";
            string TrigerConst = "CREATE TRIGGER";
            if (!(SQLScript.Contains(TableConst)) && !(SQLScript.Contains(DropConst)) && !(SQLScript.Contains(PrimaryConst)))
            {
                return SQLScript;
            }
            if ((SQLScript.IndexOf(DropConst)) != (SQLScript.LastIndexOf(DropConst)))
            {
                throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere two or more \"DROP TABLE\" statement!");
            }
            // Comment Drop statement
            int DropIndex = SQLScript.IndexOf(DropConst, StringComparison.OrdinalIgnoreCase);
            //SQLScript = SQLScript.Insert(DropIndex, "--");            // Remove do GO
            string BeforeDrop = SQLScript.Remove(DropIndex);
            int BeforeGODropIndex = BeforeDrop.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
            if (BeforeGODropIndex < 0) { BeforeGODropIndex = -2; }

            int AfterGODROPIndex = SQLScript.IndexOf("GO", DropIndex, StringComparison.OrdinalIgnoreCase);
            if (AfterGODROPIndex < 0)
            {// Error       
                throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after DROP TABLE!");
            }
            SQLScript = SQLScript.Remove(BeforeGODropIndex + 2, AfterGODROPIndex - BeforeGODropIndex + 1);


            int CreateIndex = SQLScript.IndexOf(TableConst, StringComparison.OrdinalIgnoreCase);

            string BeforeCreate = SQLScript.Remove(CreateIndex);
            int BeforeGOIndex = BeforeCreate.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
            if (BeforeGOIndex < 0) { BeforeGOIndex = -2; }

            int AfterGOIndex = SQLScript.IndexOf("GO", CreateIndex, StringComparison.OrdinalIgnoreCase);
            if (AfterGOIndex < 0)
            {
                throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after CREATE TABLE!");
            }
            SQLScript = SQLScript.Remove(BeforeGOIndex + 2, AfterGOIndex - BeforeGOIndex + 1);
            //return SQLScript;

            // remove ALTER Table + primary key
            int PrimIndex = SQLScript.IndexOf(PrimaryConst, StringComparison.OrdinalIgnoreCase);
            if (PrimIndex > 0)
            {
                string BeforePrim = SQLScript.Remove(PrimIndex);
                int BeforePrimGOIndex = BeforePrim.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
                if (BeforePrimGOIndex < 0) { BeforePrimGOIndex = -2; }

                int AfterPrimGOIndex = SQLScript.IndexOf("GO", PrimIndex, StringComparison.OrdinalIgnoreCase);
                if (AfterPrimGOIndex < 0)
                {
                    throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after PRIMARY KEY!");
                }
                string sub = SQLScript.Substring(BeforePrimGOIndex + 2, AfterPrimGOIndex - BeforePrimGOIndex + 1);
                if ((sub.IndexOf("ALTER TABLE", StringComparison.OrdinalIgnoreCase) > 0) && (sub.IndexOf("ADD", StringComparison.OrdinalIgnoreCase) > 0) && (sub.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > 0))
                {
                    SQLScript = SQLScript.Remove(BeforePrimGOIndex + 2, AfterPrimGOIndex - BeforePrimGOIndex + 1);
                }
            }

            // remove ALTER Table + UNIQUE
            int UniqueIndex = SQLScript.IndexOf(UniqueConst, StringComparison.OrdinalIgnoreCase);
            if (UniqueIndex > 0)
            {
                string BeforeUnique = SQLScript.Remove(UniqueIndex);
                int BeforeUniqueGOIndex = BeforeUnique.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
                if (BeforeUniqueGOIndex < 0) { BeforeUniqueGOIndex = -2; }

                int AfterUniqueGOIndex = SQLScript.IndexOf("GO", UniqueIndex, StringComparison.OrdinalIgnoreCase);
                if (AfterUniqueGOIndex < 0)
                {
                    throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after UNIQUE!");
                }
                string sub = SQLScript.Substring(BeforeUniqueGOIndex + 2, AfterUniqueGOIndex - BeforeUniqueGOIndex + 1);
                if ((sub.IndexOf("ALTER TABLE", StringComparison.OrdinalIgnoreCase) > 0) && (sub.IndexOf("ADD", StringComparison.OrdinalIgnoreCase) > 0) && (sub.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > 0))
                {
                    SQLScript = SQLScript.Remove(BeforeUniqueGOIndex + 2, AfterUniqueGOIndex - BeforeUniqueGOIndex + 1);
                }
            }

            //return SQLScript;
            // remove trigger script to another file
            int TrigIndex = SQLScript.IndexOf(TrigerConst, StringComparison.OrdinalIgnoreCase);
            if (TrigIndex > 0)
            {
                string BeforeTrig = SQLScript.Remove(TrigIndex);
                int BeforeTrigGOIndex = BeforeTrig.LastIndexOf("GO", StringComparison.OrdinalIgnoreCase);
                if (BeforeTrigGOIndex < 0) { BeforeTrigGOIndex = -2; }

                int AfterTrigGOIndex = SQLScript.IndexOf("GO", TrigIndex, StringComparison.OrdinalIgnoreCase);
                if (AfterTrigGOIndex < 0)
                {
                    throw new CalledProcessErrorExitException("In the script \r\n" + SQLScript.Replace("\r\n", " ") + "\r\nthere is no GO statement after CREATE TRIGGER!");
                }
                string TriggerExpression = SQLScript.Substring(BeforeTrigGOIndex + 2, AfterTrigGOIndex - BeforeTrigGOIndex + 0);
                SQLScript = SQLScript.Remove(BeforeTrigGOIndex + 2, AfterTrigGOIndex - BeforeTrigGOIndex + 0);                
                // write Trigger to file                
                File.AppendAllText(TriggersCreateFile, TriggerExpression);
            }
            return SQLScript;                   
        }

        private string GetOsqlArguments(string CurrentSQLScriptFile)
        {
            StringBuilder osqlArguments = new StringBuilder();
            osqlArguments.AppendFormat("{0} -S {1} -n -b -e ", ConnectionString, _ServerName);
            /*if (!string.IsNullOrEmpty(DatabaseName))
            {
                osqlArguments.AppendFormat("-d {0} ", DatabaseName);
            }*/
            osqlArguments.AppendFormat("-i \"{0}\"", CurrentSQLScriptFile/*sqlScript.ItemSpec*/);
            osqlArguments.AppendFormat(" -o \"{0}\"", TempSQLLog);
            return osqlArguments.ToString();
        }

        private int RunOsql(string osqlArguments)
        {
            //LogDBDeployLog("Running sql script {0} with osql arguments: {1}", SqlToolsPath, osqlArguments);
            return RunProcess(SqlToolsPath, osqlArguments);
        }

        private int RunProcess(string ExePath, string Arguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = ExePath;
            proc.StartInfo.Arguments = Arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            LogDBDeployLog("Running process " + proc.StartInfo.FileName + " with arguments: " + proc.StartInfo.Arguments);
            proc.Start();
            if (WaitForExit)
            {
                proc.WaitForExit();
            }
            LogDBDeployLog("Process exited with code: " + proc.ExitCode.ToString());
            if (proc.ExitCode != 0)
            {
                throw new CalledProcessErrorExitException("Process " + ExePath + " ran with arguments " + Arguments + " exited with error code " + proc.ExitCode.ToString() + ". CalledProcessErrorExitException is raised.");
            }
            return proc.ExitCode;
        }

        private void LogSQLMessage()
        {
            StreamReader sr = File.OpenText(TempSQLLog);              
            LogDBDeployLog("-----< OSQL log copy >------------------------------");
            LogDBDeployLog(sr.ReadToEnd());
            LogDBDeployLog("----------------------------------------------------");
            sr.Close();
        }

        private void LogDeltaError()
        {
            StreamReader sr;
            sr = File.OpenText(SqlDeltaPath.Replace("SQLDelta.exe", "CommandLineErrors.txt"));            
            LogDBDeployLog(sr.ReadToEnd());
            sr.Close();            
        }

        private void LogDBDeployLog(string Message)
        {
            if (!File.Exists(DBDeployLogFile))
            {
                File.AppendAllText(DBDeployLogFile, Message + "\r\n");
            }
            else
            {
                if ((File.GetAttributes(DBDeployLogFile)) != FileAttributes.ReadOnly)
                {
                    File.AppendAllText(DBDeployLogFile, Message + "\r\n");
                }
            }
            Log.LogMessage(Message);           
        }

        private void LogErrorDBDeployLog(string Message)
        {
            if (!File.Exists(DBDeployLogFile))
            {
                File.AppendAllText(DBDeployLogFile, "Error: \r\n" + Message + "\r\n");
            }
            else
            {
                if ((File.GetAttributes(DBDeployLogFile)) != FileAttributes.ReadOnly)
                {
                    File.AppendAllText(DBDeployLogFile, "Error: \r\n" + Message + "\r\n");
                }
            }
            Log.LogError(Message);
        }

        private void SaveDBDeployLog()
        {
            string[] CurrentReports = Directory.GetFiles(TempCompareReportFolder, "*.*", SearchOption.AllDirectories);
            foreach (string f in CurrentReports)
            {
                // Add and Check in current update script                    
                string AddReportCommand = "add \"" + f + "\" /noprompt";
                LogDBDeployLog("Running TF.exe: " + TFexe + " " + AddReportCommand);
                RunProcess(TFexe, AddReportCommand);
                string CheckinReportCommand = "checkin \"" + f + "\" /noprompt";
                LogDBDeployLog("Running TF.exe: " + TFexe + " " + CheckinReportCommand);
                RunProcess(TFexe, CheckinReportCommand);
            }
        }

        private void RunDependentScripts(ArrayList ScriptCollection)
        {
            System.Collections.ArrayList UncorrectViews = new ArrayList(ScriptCollection);
            System.Collections.ArrayList UncorrectViews_Next = new ArrayList();
            bool WhatArray = true;
            int RecCount = 0;
            int ErrorsCount = 0, PreviousCycleErrors = 0;
            bool IsError = false;
            while (true)
            {
                RecCount++;
                ErrorsCount = 0;
                foreach (object f in (WhatArray ? UncorrectViews : UncorrectViews_Next))
                {
                    IsError = false;
                    try
                    {
                        RunOsql(GetOsqlArguments(f.ToString()));
                    }
                    catch (CalledProcessErrorExitException)
                    {
                        IsError = true;
                        ErrorsCount++;
                        if (WhatArray)
                        {
                            UncorrectViews_Next.Add(f);
                        }
                        else
                        {
                            UncorrectViews.Add(f);
                        }
                    }
                    if (IsError)
                    {
                        LogDBDeployLog("Exited code may be not 0. This cycle of dependent scripts creation. Next Error from OSQL log is only for depend script!");
                        LogDBDeployLog("Next error occured in dependent script. Script will be run again...");
                    }
                    LogSQLMessage();
                }
                if (ErrorsCount == 0)
                {
                    break;
                }
                if (PreviousCycleErrors >= ErrorsCount)
                {//error
                    LogErrorDBDeployLog("Count of create errors isn't decrement in dependent scripts!");
                    throw new CalledProcessErrorExitException();
                }
                PreviousCycleErrors = ErrorsCount;
                if (WhatArray)
                {
                    UncorrectViews.Clear();
                }
                else
                {
                    UncorrectViews_Next.Clear();
                }
                WhatArray = !WhatArray;
                if (RecCount > 33)
                {
                    // error too much iterations
                    LogErrorDBDeployLog("Cycle of view creation: too much iterations!");
                    throw new CalledProcessErrorExitException();
                }
            }
        }
        #endregion
    }

    public class CalledProcessErrorExitException : System.Exception
    {
        public CalledProcessErrorExitException() { }
        public CalledProcessErrorExitException(string message)
            : base(message) { }
    }
}