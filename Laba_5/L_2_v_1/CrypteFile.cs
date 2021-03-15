using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace L_2_v_1
{
	internal class CrypteFile
	{
		private const string IVFileName = "iv.key";
		private const string AESFileName = "aes.key";

		private readonly Aes _aes;

		private readonly string _folderPath;
		private readonly byte _coreAvailable;
		private byte _busyCore;
		private object _lockObject = new object();
		private readonly List<string> _sourceFiles;

		public delegate void StartCrypteFileEventHandler(object sender);
		public delegate void CompletedCrypteFileEventHandler(object sender);
		public event StartCrypteFileEventHandler StartCrypt;
		public event CompletedCrypteFileEventHandler CompletedCrypt;

		public byte NumberBusyThread { get; set; }

		public CrypteFile(string folderPath, byte coreAvailable)
		{
			_folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
			_coreAvailable = coreAvailable;
			_busyCore = 0;

			_aes = Aes.Create();

			_aes.Key = LoadKey();
			var key = _aes.Key;
			SaveKey(key);

			_aes.IV = LoadInitializationVector();
			var iv = _aes.IV;
			SaveInitializationVector(iv);

			_sourceFiles = Directory.GetFiles(_folderPath, "*", SearchOption.AllDirectories).ToList();

			StartCrypt += CrypteFile_StartCrypt;
			CompletedCrypt += CrypteFile_CompletedCrypt;
		}

		private void CrypteFile_CompletedCrypt(object sender)
		{
			lock (_lockObject)
			{
				_busyCore--;
			}

			CryptFilesInFolder();
		}

		private void CrypteFile_StartCrypt(object sender)
		{
			lock (_lockObject)
			{
				_busyCore++;
			}
		}

		private void CryptFile(string filePath)
		{
			OnStartCrypt();

			using var fs = new FileStream(filePath, FileMode.OpenOrCreate);
			var fileData = ConvertToByteArray(fs);
			var fileSize = fs.Length;
	  
			using var cryptStream = new CryptoStream(fs, _aes.CreateEncryptor(), CryptoStreamMode.Write);

			fs.Seek(0, SeekOrigin.Begin);
			cryptStream.Write(fileData, 0, fileData.Length);

			cryptStream.Close();
			fs.Close();

			PrintThreadStatus(Thread.CurrentThread, filePath, fileSize);

			OnCompletedCrypt();
		}

		public void CryptFilesInFolder()
		{
			lock (_lockObject)
			{
				var avalibleFiles = _sourceFiles.Take(_coreAvailable - _busyCore).ToList();
				foreach (var filePath in avalibleFiles)
				{
					var thread = new Thread(() => { CryptFile(filePath); });
					thread.Start();
					_sourceFiles.Remove(filePath);
				}
			}
		}

		private void SaveInitializationVector(byte[] iv)
		{
			if (!File.Exists(IVFileName))
			{
				Save(iv, IVFileName);
			}
		}

		private void SaveKey(byte[] key)
		{
			if (!File.Exists(AESFileName))
			{
				Save(key, AESFileName);
			}
		}

		private byte[] LoadInitializationVector()
		{
			var iv = Load(IVFileName);

			if (iv == null)
			{
				_aes.GenerateIV();
				iv = _aes.IV;
			}

			return iv;
		}

		private byte[] LoadKey()
		{
			var key = Load(AESFileName);

			if (key == null)
			{
				_aes.GenerateKey();
				key = _aes.Key;
			}

			return key;
		}

		private byte[] Load(string filePath)
		{
			using FileStream myStream = new FileStream(filePath, FileMode.Open);
			byte[] buffer = new byte[myStream.Length];
			myStream.Read(buffer, 0, buffer.Length);
			myStream.Flush();
			myStream.Close();
			myStream.Dispose();

			return buffer;
		}

		private void Save(byte[] data, string file)
		{
			using FileStream myStream = new FileStream(file, FileMode.OpenOrCreate);
			myStream.Write(data, 0, data.Length);
			myStream.Flush();
			myStream.Close();
			myStream.Dispose();
		}

		private void PrintThreadStatus(Thread thread, string filePath, long cryptedBytes)
		{
			var threadId = thread.ManagedThreadId;

			Console.WriteLine($"Thread ID:{threadId}\t" +
				$"Full file path:{filePath}\tCommon count crypted bytes: {cryptedBytes}");
		}

		protected virtual void OnStartCrypt()
		{
			StartCrypt?.Invoke(this);
		}

		protected virtual void OnCompletedCrypt()
		{
			CompletedCrypt?.Invoke(this);
		}

		/// <summary>
		/// Converts a Stream into a byte array.
		/// </summary>
		/// <param name="stream">The stream to convert.</param>
		/// <returns>A byte[] array representing the current stream.</returns>
		public byte[] ConvertToByteArray(Stream stream)
		{
			byte[] buffer = new byte[16 * 1024];
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}
	}
}
