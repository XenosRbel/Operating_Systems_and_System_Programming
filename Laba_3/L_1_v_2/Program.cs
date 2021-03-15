using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace L_1_v_2
{
	class Program
	{
		static void Main(string[] args)
		{
			var sourceDir = args[0] ?? throw new ArgumentNullException("args");
			var destDir = args[1] ?? throw new ArgumentNullException("args");
			var availableThread = args[2] ?? throw new ArgumentNullException("args");

			var sourceFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories).ToList();
			var filesAtDestDir = Directory.GetFiles(destDir, "*", SearchOption.AllDirectories).ToList();

			var missingFiles = GetMissingFiles(sourceFiles, filesAtDestDir);

			CopyFiles(missingFiles, destDir, sourceDir, Convert.ToByte(availableThread));

			Console.WriteLine("Press any key for exit.");
			Console.ReadKey();
		}

		private static IList<T> GetMissingFiles<T>(IList<T> filesAtDir1, IList<T> filesAtDir2)
		{
			return filesAtDir1.Where(item => !filesAtDir2.Contains(item)).ToList();
		}

		private static string BuildPathToMissingFile(string sourceFilePath, string sourceDir)
		{
			var dirsAtSourcePath = sourceDir.Split('\\');
			var dirsAtCurrentPath = new FileInfo(sourceFilePath).Directory.FullName.Split('\\');
			var paths = dirsAtCurrentPath.Where(dir => !dirsAtSourcePath.Contains(dir)).ToArray();
			return Path.Combine(paths);
		}

		private static void CopyFile(string sourceFilePath, string destFilePath)
		{
			if (File.Exists(destFilePath))
			{
				return;
			}

			var dirPath = new FileInfo(destFilePath).Directory.FullName;
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}

			var cpProcess = Process.Start("powershell", $"cp {sourceFilePath} {destFilePath}");
			
			var size = new FileInfo(sourceFilePath).Length;

			Console.WriteLine($"PID:{cpProcess.Id}\t" +
				$"File path:{destFilePath};\t" +
				$"File size:{size}");
		}

		private static void CopyFiles(IList<string> filePaths, string destDir, string sourceDir, byte availableThread)
		{
			while (filePaths.Count != 0)
			{
				var copyTask = filePaths.Take(availableThread).ToList();

				foreach (var filePath in copyTask)
				{
					var fileName = Path.GetFileName(filePath);
					var dirPath = BuildPathToMissingFile(filePath, sourceDir);
	
					var newFilePath = Path.Combine(destDir, dirPath, fileName);

					var task = new Task(() => { CopyFile(filePath, newFilePath); });
					task.Start();

					filePaths.Remove(filePath);
				}
			}
		}
	}
}
