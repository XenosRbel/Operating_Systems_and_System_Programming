using System;
using System.IO;
using System.Security.Cryptography;

namespace L_2_v_1
{
	internal class CrypteFile
	{
		private readonly Aes _aes;

		private readonly string _folderPath;
		private readonly byte _coreAvailable;
		private readonly byte _environmentProcessorCount;
		private object _lockObject;
		
		public byte NumberBusyThread { get; set; }

		public CrypteFile(string folderPath, byte coreAvailable, byte environmentProcessorCount)
		{
			_folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
			_coreAvailable = coreAvailable;
			_environmentProcessorCount = environmentProcessorCount;

			_aes = Aes.Create();

			_aes.GenerateKey();
			var key = _aes.Key;
			SaveKey(key);

			_aes.GenerateIV();
			var iv = _aes.IV;
			SaveInitializationVector(iv);
		}

		private void Crypt(string filePath)
		{
			using var fs = new FileStream(filePath, FileMode.Open);
	  
			using var cryptStream = new CryptoStream(
				fs,
				_aes.CreateEncryptor(),
				CryptoStreamMode.Write);

			cryptStream.Dispose();
		}

		private void CryptFilesInFolder()
		{
			lock (_lockObject)
			{

			}
		}

		private void SaveInitializationVector(byte[] iv)
		{
			Save(iv, "iv.key");
		}

		private void SaveKey(byte[] key)
		{
			Save(key, "aes.key");
		}

		private void Save(byte[] data, string file)
		{
			using FileStream myStream = new FileStream(file, FileMode.OpenOrCreate);
			myStream.Write(data, 0, data.Length);
			myStream.Close();
			myStream.Dispose();
		}

		public void PrintThreadStatus()
		{
			var threadId = int.MaxValue;
			var filePath = string.Empty;
			var cryptedBytes = int.MaxValue;

			Console.WriteLine($"Thread ID:{threadId}\t" +
				$"Full file path:{filePath}\tCommon count crypted bytes: {cryptedBytes}");
		}
	}
}
