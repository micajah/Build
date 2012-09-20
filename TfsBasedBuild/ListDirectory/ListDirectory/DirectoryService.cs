using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ListDirectory
{
	public class DirectoryService
	{
		public DirectoryService(string directoryName)
		{
			rootDirectoryName = directoryName;
		}
		
		private string rootDirectoryName;

		public List<string> GetListing()
		{
			return GetDirectoryFilesListing(rootDirectoryName);
		}

		public List<string> GetDirectories()
		{
			return GetDirectoriesListing(rootDirectoryName);
		}

		private List<string> GetDirectoriesListing(string directory)
		{
			List<string> subDirectoriesCollection = new List<string>();
			string[] subdirectories = Directory.GetDirectories(directory);
			foreach (var directoryName in subdirectories)
			{
				subDirectoriesCollection.Add(directoryName);
				subDirectoriesCollection.AddRange(GetDirectoriesListing(directoryName));
			}
			return subDirectoriesCollection;
		}

		private List<string> GetDirectoryFilesListing(string directoryName)
		{
			List<string> subDirectoryFiles = new List<string>();
			var resultFileList = new List<string>();
			string[] subdirectories = Directory.GetDirectories(directoryName);
			foreach (var directory in subdirectories)
			{
				subDirectoryFiles.AddRange(GetDirectoryFilesListing(directory));
			}
			if(subDirectoryFiles != null && subDirectoryFiles.Count > 0)
			{
				resultFileList.AddRange(subDirectoryFiles);
			}
			resultFileList.AddRange(Directory.GetFiles(directoryName));
			return resultFileList;
		}
	}
}
