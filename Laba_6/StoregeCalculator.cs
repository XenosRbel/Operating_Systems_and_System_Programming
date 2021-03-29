using L_4_t_1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace L_3_t_6
{
	partial class StoregeCalculator : ICalculatorInfo
	{
		public string MemoryVolume { get => _memoryVolume.ToString(); }
		public string FileCounts { get => _fileCounts.ToString(); }
		public string ScanDirectoryPath { get; private set; }
		public byte AvalibleThread { get; private set; }

		public delegate void StartCalculateVolumeEventHandler(object sender);
		public delegate void CompletedCalculateVolumeEventHandler(object sender);
		public event StartCalculateVolumeEventHandler StartCalculate;
		public event CompletedCalculateVolumeEventHandler CompletedCompleted;

		#region Private
		private byte _busyThread;
		private BigInteger _memoryVolume;
		private BigInteger _fileCounts;
		private object _lockObject = new object();
		private readonly Thread _mainThead;

		private List<string> _sourceFiles;
		private static byte _environmentProcessorCount = (byte)Environment.ProcessorCount;

		private const string REPORT_FILE_PATH = "report.json";
		#endregion


		public StoregeCalculator(string scanDirectoryPath, byte avalibleThread)
		{
			ScanDirectoryPath = scanDirectoryPath ?? throw new ArgumentNullException(nameof(scanDirectoryPath));
			AvalibleThread = avalibleThread > 0 && avalibleThread <= _environmentProcessorCount ? avalibleThread : throw new ArgumentOutOfRangeException(nameof(avalibleThread));

			_memoryVolume = 0;
			_fileCounts = 0;

			_mainThead = Thread.CurrentThread;

			_sourceFiles = Directory.GetFiles(ScanDirectoryPath, "*", SearchOption.AllDirectories).ToList();

			StartCalculate += StoregeCalculatorStartCalculate;
			CompletedCompleted += StoregeCalculatorCompletedCompleted;
		}

		private void StoregeCalculatorCompletedCompleted(object sender)
		{
			lock (_lockObject)
			{
				_busyThread--;
			}

			SaveResults();

			Execute();

#if DEBUG
			lock (_mainThead)
			{
				if (_sourceFiles.Count == 0)
				{
					Console.WriteLine($"Task is completed!\n" +
						$"{nameof(MemoryVolume)}:{MemoryVolume}\t{nameof(FileCounts)}:{FileCounts}.\n");
				}
			}
#endif
		}

		private void StoregeCalculatorStartCalculate(object sender)
		{
			lock (_lockObject)
			{
				_busyThread++;
			}
		}

		public void Execute()
		{
			if (_busyThread > AvalibleThread)
			{
				return;
			}

			lock (_mainThead)
			{
				var avaliblePathToTask = _sourceFiles.Take(AvalibleThread - _busyThread);
				foreach (var filePath in avaliblePathToTask)
				{
					var thread = new Thread(() => { Calculate(filePath); });
					thread.Start();

					_sourceFiles.Remove(filePath);				
				}
			}
		}

		private void Calculate(string filePath)
		{	
			lock (_lockObject)
			{
				OnStartCalculate();

				var fileInfo = new FileInfo(filePath);
				_memoryVolume += fileInfo.Length;
				_fileCounts++;

				OnCompletedCompleted();
			}
		}

		private void SaveResults()
		{
			lock (_lockObject)
			{
				var jsonResultData = JsonSerializer.Serialize((ICalculatorInfo)this);
				using var fileStream = new FileStream(REPORT_FILE_PATH, FileMode.OpenOrCreate);

				var bytesJson = Encoding.UTF8.GetBytes(jsonResultData);
				fileStream.Write(bytesJson, 0, bytesJson.Length);
				fileStream.Flush();
				fileStream.Close();
			}
		}

		protected virtual void OnStartCalculate()
		{
			StartCalculate?.Invoke(this);
		}

		protected virtual void OnCompletedCompleted()
		{
			CompletedCompleted?.Invoke(this);
		}
	}
}
