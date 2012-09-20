using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ListDirectory
{
	class Program
	{
		static void Main(string[] args)
		{
			string directoryName = string.Empty;
			List<string> directories = new List<string>();
			string deployBatchFileName = string.Empty;
			string deployBatchTemplate = string.Empty;
			string remoteDirectory = string.Empty;
			string[] skipDirectories = new string[0];
			try
			{
				if(args.Length >= 4)
				{
					directoryName = args[0];
					deployBatchTemplate = args[1];
					deployBatchFileName = args[2];
					remoteDirectory = args[3];
				}
				else
					throw new FileNotFoundException("Please use the following order of the arguments:\r\n" +
						"ListDirectory.exe [source_directory_to_deploy] [template_batch_file] [result_batch_file] [destination_ftp_directory] [(source_directories_to_skip;)]");
				if (args.Length == 5)
				{
					skipDirectories = args[4].Split(new char[1] { ';' });
				}
				
				if (!File.Exists(deployBatchTemplate))
				{
					throw new FileNotFoundException("Please provide deployment file template as an argument.");
				}
				string template = File.ReadAllText(deployBatchTemplate);
				if(Directory.Exists(directoryName))
				{
					DirectoryService dir = new DirectoryService(directoryName);
					directories = dir.GetDirectories();
				}
				else
					throw new FileNotFoundException("Given argument is not a valid directory name.");
				directories.Add(directoryName);
				string customBatchText = string.Empty;
				if (skipDirectories.Length > 0)
				{
					skipDirectories.ToList<string>().ForEach(skip =>
						directories = directories.Where(dir => !dir.ToLower().StartsWith((directoryName + skip).ToLower())).ToList<string>()
						);
				}

				directories.ForEach(str => customBatchText = string.Format("{2}mkdir {0}{1}" + 
																																		"cd {0}{1}" +
																																		"mput {3}\\*{1}" +
																																		"cd {4}{1}", 
					str.Replace(directoryName + "\\", ""), 
					Environment.NewLine, 
					customBatchText, 
					str, 
					remoteDirectory));
				string batchFileContent = string.Format(template, remoteDirectory, customBatchText);
				Console.WriteLine(batchFileContent);
				File.WriteAllText(deployBatchFileName, batchFileContent);
				//Console.ReadLine();
			}
			catch(FileNotFoundException ex)
			{
				Console.WriteLine(ex.Message);
				Console.ReadLine();
				return;
			}
		}

	}
}
