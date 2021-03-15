using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace L_2_v_1
{
	class Program
	{
		private static string _folderPath;
		private static byte _coreAvailable;
		private static byte _environmentProcessorCount;

		static void Main(string[] args)
		{
			try
			{
				_environmentProcessorCount = (byte)Environment.ProcessorCount;
				Initialization(args);
			}
			catch (ArgumentNullException e)
			{
				Console.WriteLine($"Ошибка ввода аргументов программы.\n{e.Message}");
			}
		}

		private static void Initialization(string[] args)
		{
			if (IsValidProgramArgument(args))
			{
				CryptFiles();
			}
			else
			{
				Console.Write("Введите путь к папке для шифрования:");
				_folderPath = Console.ReadLine();

				Console.Write($"Доступно потоков:{_environmentProcessorCount}\n" +
					$"Введите число одновременно работающих потоков для шифрования файлов:");
				_coreAvailable = Convert.ToByte(Console.ReadLine());

				ValidateManualParams();

				CryptFiles();
			}
		}

		private static void CryptFiles()
		{
			var crypto = new CrypteFile(_folderPath, _coreAvailable);
			crypto.CryptFilesInFolder();
		}

		private static bool IsValidProgramArgument(string[] args)
		{
			return args.Length == 2;
		}

		private static void ValidateManualParams()
		{
			var invalidParams = new List<string>();

			if (_coreAvailable <= 0 || _coreAvailable > _environmentProcessorCount)
			{
				invalidParams.Add(nameof(_coreAvailable));
			}

			if (!Directory.Exists(_folderPath))
			{
				invalidParams.Add(nameof(_folderPath));
			}

			if (invalidParams.Count > 0)
			{
				throw new ArgumentNullException(string.Join(",", invalidParams),"Неправильные аргументы приложения.");
			}
		}
	}
}
