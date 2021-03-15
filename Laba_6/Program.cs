using System;

namespace L_3_t_6
{
	class Program
	{
		private static string _folderPath;
		private static byte _coreAvailable;
		private static byte _environmentProcessorCount;

		private static void Main(string[] args)
		{
			_environmentProcessorCount = (byte)Environment.ProcessorCount;

			Console.Write("Введите путь к папке для сканирвоания:");
			_folderPath = Console.ReadLine();

			Console.Write($"Доступно потоков:{_environmentProcessorCount}\n" +
				$"Введите число одновременно работающих потоков для шифрования файлов:");
			var coreAvailable = Console.ReadLine();
			_coreAvailable = Convert.ToByte(string.IsNullOrWhiteSpace(coreAvailable) ? throw new ArgumentNullException(nameof(coreAvailable)) : coreAvailable );

			var storegeCalculator = new StoregeCalculator(_folderPath, _coreAvailable);
			storegeCalculator.Execute();
		}
	}
}
