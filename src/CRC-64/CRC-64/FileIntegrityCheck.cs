/*----------------------------------------------------------------------+
 |  filename:   FileIntegrityCheck.cs                                   |
 |----------------------------------------------------------------------|
 |  version:    2.21                                                    |
 |  revision:   24.08.2012 15:52                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    �������� ����������� ������                             |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;
using System.IO;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ���������� � �������� ����������� ����� �� ������ CRC-64
	/// </summary>
	public class FileIntegrityCheck
	{
		#region Delegates

		/// <summary>
		/// ������� ���������� ��������� �������� ����������� �����
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateFileIntegrityCheckProgress;

		/// <summary>
		/// ������� ���������� �������� �������� ����������� �����
		/// </summary>
		public OnEventHandler OnFileIntegrityCheckFinish;

		#endregion Delegates

		#region Public Properties & Data

		/// <summary>
		/// ��������� �������� "���� ��������������?"
		/// </summary>
		public bool InProcessing
		{
			get
			{
				if(
					(this.thrFileIntegrityCheck != null)
					&&
					(
						(this.thrFileIntegrityCheck.ThreadState == ThreadState.Running)
						||
						(this.thrFileIntegrityCheck.ThreadState == ThreadState.WaitSleepJoin)
					)
					)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// ��������� �������� "��������� ������ �������� ���������
		/// (����� ���������� ��������� ����������-������)?"
		/// </summary>
		public bool Finished
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� ��������
				if(!InProcessing)
				{
					return this.finished;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// �������� ������ ��������� �������� ���������?
		/// </summary>
		private bool finished;

		/// <summary>
		/// ��������� �������� "CRC-64 ����� ��������� ���������?"
		/// </summary>
		public bool ProcessedOK
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� ��������
				if(!InProcessing)
				{
					return this.processedOK;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// CRC-64 ��������� ���������?
		/// </summary>
		private bool processedOK;

		/// <summary>
		/// ��� �����, ������������� ���������
		/// </summary>
		public String FullFilename
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� ��������...
				if(!InProcessing)
				{
					return this.fullFilename;
				}
				else
				{
					//...� ����� �������� �� ��������
					return null;
				}
			}
		}

		/// <summary>
		/// ��� ����� ��� ���������
		/// </summary>
		private String fullFilename;

		/// <summary>
		/// ��� �����, ������������� ���������
		/// </summary>
		public UInt64 CRC64
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� �������� ��������...
				if(!InProcessing)
				{
					return this.crc64;
				}
				else
				{
					///...� ����� �������� �� ��������
					return 0xFFFFFFFFFFFFFFFF;
				}
			}
		}

		/// <summary>
		/// �������� CRC-64, ��������������� "fullFilename"
		/// </summary>
		private UInt64 crc64;

		/// <summary>
		/// ������ ��������� ������
		/// </summary>
		public int BufferLength
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� ��������...
				if(!InProcessing)
				{
					return this.bufferLength;
				}
				else
				{
					//...� ����� �������� �� ��������
					return -1;
				}
			}

			set
			{
				// ���� ����� �� ����� ���������� - ������������� ��������...
				if(!InProcessing)
				{
					this.bufferLength = value - (value % 8);
				}
			}
		}

		/// <summary>
		/// ������ ��������� ������
		/// </summary>
		private int bufferLength = 1 << 26; // 64 ��;

		/// <summary>
		/// ��������� ��������
		/// </summary>
		public int ThreadPriority
		{
			get { return (int)this.threadPriority; }

			set
			{
				if(
					(this.thrFileIntegrityCheck != null)
					&&
					(this.thrFileIntegrityCheck.IsAlive)
					)
				{
					switch(value)
					{
						default:
						case 0:
							{
								this.threadPriority = System.Threading.ThreadPriority.Lowest;

								break;
							}

						case 1:
							{
								this.threadPriority = System.Threading.ThreadPriority.BelowNormal;

								break;
							}

						case 2:
							{
								this.threadPriority = System.Threading.ThreadPriority.Normal;

								break;
							}

						case 3:
							{
								this.threadPriority = System.Threading.ThreadPriority.AboveNormal;

								break;
							}

						case 4:
							{
								this.threadPriority = System.Threading.ThreadPriority.Highest;

								break;
							}
					}

					// ������������� ��������� ��������� ��������
					this.thrFileIntegrityCheck.Priority = this.threadPriority;
				}
			}
		}

		/// <summary>
		/// ��������� �������� ������� CRC-64
		/// </summary>
		private ThreadPriority threadPriority;

		/// <summary>
		/// �������, ��������������� �� ���������� ���������
		/// </summary>
		public ManualResetEvent[] FinishedEvent
		{
			get { return this.finishedEvent; }
		}

		/// <summary>
		/// �������, ��������������� �� ���������� ���������
		/// </summary>
		private ManualResetEvent[] finishedEvent;

		#endregion Data & Public Properties

		#region Data

		/// <summary>
		/// ��������� ������ ������� CRC-64
		/// </summary>
		private CRC64 eCRC64;

		/// <summary>
		/// �������� �����
		/// </summary>
		private byte[] buffer;

		/// <summary>
		/// ����� ���������� CRC-64 ����� "fullFilename" � ����������� ���������� � "processedOK"
		/// </summary>
		private Thread thrFileIntegrityCheck;

		/// <summary>
		/// ������� ����������� ��������� �����
		/// </summary>
		private ManualResetEvent[] exitEvent;

		/// <summary>
		/// ������� ����������� ��������� �����
		/// </summary>
		private ManualResetEvent[] executeEvent;

		#endregion Data

		#region Construction

		/// <summary>
		/// ����������� ������
		/// </summary>
		public FileIntegrityCheck()
		{
			// ������� ��������� ������ ������� CRC-64
			this.eCRC64 = new CRC64();

			// �������������� ��� ����� ��-���������
			this.fullFilename = "NONAME";

			// �������� ������ ��� �������� �����
			this.buffer = new byte[this.bufferLength];

			// �������� ������ ��������� �������� ���������?
			this.finished = true;

			// ��������� ����������� ���������?
			this.processedOK = false;

			// ��-��������� ��������������� ������� ���������
			this.threadPriority = 0;

			// �������������� ������� ����������� ��������� �����
			this.exitEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������������� c������ ����������� ��������� �����
			this.executeEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������, ��������������� �� ���������� ���������
			this.finishedEvent = new ManualResetEvent[] {new ManualResetEvent(true)};
		}

		#endregion Construction

		#region Public Operations

		/// <summary>
		/// ����� ������� ������ ��������� ���������� � ������ CRC64 � ����� �����
		/// </summary>
		/// <param name="fullFilename">��� ����� ��� ���������</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToWriteCRC64(String fullFilename, bool runAsSeparateThread)
		{
			// ���� ����� ���������� CRC-64 �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			if(fullFilename == null)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				return false;
			}

			// ���� �������� ���� �� ����������, �������� �� ������
			if(!File.Exists(fullFilename))
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				return false;
			}

			// ��������� ��� �����
			this.fullFilename = fullFilename;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������� ���������� � ������ CRC-64 � ����� �����
				WriteCRC64();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ���������� � ������ CRC-64...
			this.thrFileIntegrityCheck = new Thread(new ThreadStart(WriteCRC64));

			//...����� ���� ��� ���...
			this.thrFileIntegrityCheck.Name = "FileIntegrityCheck.WriteCRC64()";

			//...������������� ��������� ��������� ������...
			this.thrFileIntegrityCheck.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileIntegrityCheck.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// ����� ������� ������ ��������� �������� CRC64, ����������� � ����� �����
		/// </summary>
		/// <param name="fullFilename">��� ����� ��� ���������</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToCheckCRC64(String fullFilename, bool runAsSeparateThread)
		{
			// ���� ����� ���������� CRC-64 �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			if(fullFilename == null)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ���� �������� ���� �� ����������, �������� �� ������
			if(!File.Exists(fullFilename))
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ��������� ��� �����
			this.fullFilename = fullFilename;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������� ���������� � �������� �������� CRC-64
				CheckCRC64();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ���������� � �������� CRC-64...
			this.thrFileIntegrityCheck = new Thread(new ThreadStart(CheckCRC64));

			//...����� ���� ��� ���...
			this.thrFileIntegrityCheck.Name = "FileIntegrityCheck.CheckCRC64()";

			//...������������� ��������� ��������� ������...
			this.thrFileIntegrityCheck.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileIntegrityCheck.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// ����� ��������� ������
		/// </summary>
		public void Stop()
		{
			// ���������, ��� ����� ��������� ������ �� ������ �����������
			this.exitEvent[0].Set();

			// ������������� ������� � �����
			this.executeEvent[0].Set();
		}

		/// <summary>
		/// ���������� ������ ��������� �� �����
		/// </summary>
		public void Pause()
		{
			// ������ �� �����
			this.executeEvent[0].Reset();
		}

		/// <summary>
		/// ������ ������ ��������� � �����
		/// </summary>
		public void Continue()
		{
			// ������� ��������� c �����
			this.executeEvent[0].Set();
		}

		#endregion Public Operations

		#region Private Operations

		/// <summary>
		/// ���������� CRC-64 ���������� �����
		/// </summary>
		/// <param name="fullFileName">��� ����� ��� ���������</param>
		/// <param name="endOffset">��������, "����������������" ��� �����������, � ����� �����</param>
		/// <returns>��������� ���� ��������</returns>
		public bool CalcCRC64(String fullFileName, int endOffset)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(fullFileName);
				if(!fileInfo.Exists) return false;
				if(fileInfo.Length <= endOffset) return false;
				return CalcCRC64(fileInfo, endOffset);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// ���������� CRC-64 �����
		/// </summary>
		/// <param name="fileInfo">������, �������� ���������� � ����� ��� ���������</param>
		/// <param name="endOffset">��������, "����������������" ��� �����������, � ����� ������</param>
		/// <returns>��������� ���� ��������</returns>
		public bool CalcCRC64(FileInfo fileInfo, int endOffset)
		{
			try
			{
				using(FileStream fileStream = fileInfo.OpenRead()) return CalcCRC64(fileStream, endOffset, fileInfo.Length);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// ���������� CRC-64 �� ������
		/// </summary>
		/// <param name="stream">�������� �����</param>
		/// <param name="endOffset">��������, "����������������" ��� �����������, � ����� ������</param>
		/// <param name="length">����� "���� ��������" � �������� ������</param>
		/// <returns>��������� ���� ��������</returns>
		public bool CalcCRC64(Stream stream, int endOffset, long length)
		{
			try
			{
				// dataLength - ����� ������, ���������� ��������� (����� ����������� ����� ����� �������� � �����)
				long dataLength = length - endOffset;
				int needRead = 0;
				int readLength = 0;

				// ���������� CRC-64 � ��������� ��������
				this.eCRC64.Reset();

				while((needRead = (dataLength < this.bufferLength) ? (int)dataLength : this.bufferLength) > 0 && (readLength = stream.Read(this.buffer, 0, needRead)) > 0)
				{
					// ��������� CRC-64 � ���� ������, ������� ������� �������
					this.eCRC64.Calculate(this.buffer, 0, readLength);

					// ��������� ������ ������, ���������� ���������
					dataLength -= readLength;
				}

				this.crc64 = this.eCRC64.Value;

				return true;
			}

			catch
			{
				return false;
			}

			finally
			{
				try
				{
					if(stream != null) stream.Close();
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// ���������� � ������ � ����� ����� �������� CRC-64
		/// </summary>
		private void WriteCRC64()
		{
			// ��������� �������� CRC-64
			this.crc64 = 0xFFFFFFFFFFFFFFFF;

			// ���� ���������� CRC-64 � ������� ����� ������ ���������...
			if(CalcCRC64(this.fullFilename, 0))
			{
				// ��������� ��������� ������
				FileStream eFileStream = null;

				// ��������� ������ ������ ������� ����� ������ � �������� ����
				BinaryWriter eBinaryWriter = null;

				try
				{
					// ���������� ���� �� ������� �����...
					if(File.Exists(this.fullFilename))
					{
						//...���� ������� �������, ������ �� ���� ��������
						// ��-���������...
						File.SetAttributes(this.fullFilename, FileAttributes.Normal);
					}

					//...��������� �������� ����� �� ������...
					eFileStream = new FileStream(this.fullFilename, FileMode.Append, System.IO.FileAccess.Write);

					//...� ���������� ��� ��� ������������� ���������� ������ ������
					// ������� ����� ������ � �������� ����
					eBinaryWriter = new BinaryWriter(eFileStream);

					// ������������ �� ����� �����...
					eBinaryWriter.Seek(0, SeekOrigin.End);

					//...� ����� � ��� ����� ����������� �������� CRC-64,...
					eBinaryWriter.Write(this.crc64);

					//...������� �������� �����...
					eBinaryWriter.Flush();

					//...� ��������� ����
					if(eBinaryWriter != null)
					{
						eBinaryWriter.Close();
						eBinaryWriter = null;
					}
				}

					// ���� ���� ���� �� ���� ���������� - ��������� �������� ����� �
					// �������� �� ������
				catch
				{
					// ��������� ����
					if(eBinaryWriter != null)
					{
						eBinaryWriter.Close();
						eBinaryWriter = null;
					}

					// ���������� ���� ������������ ����������
					this.processedOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return;
				}

				// ��������� �� ��, ��� CRC-64 � ������� ����� ���� ��������� ���������
				this.processedOK = true;
			}
			else
			{
				// ��������� �� ��, ��� CRC-64 � ������� ����� ���� ��������� �����������
				this.processedOK = false;
			}

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		/// <summary>
		/// ���������� � �������� �������� CRC-64, ����������� � ����� �����
		/// </summary>
		private void CheckCRC64()
		{
			// ��������� �������� CRC-64
			this.crc64 = 0xFFFFFFFFFFFFFFFF;

			// ������, �������� ����� ����������� CRC-64
			byte[] crc64Arr = new byte[8];

			// ����������� � ����� �������� CRC-64:
			UInt64 crc64;

			// ��������� ��������� ������
			FileStream eFileStream = null;

			// ���� ���������� CRC-64 � ������� ����� �� ��������� "8" ������ ���������...
			if(CalcCRC64(this.fullFilename, 8))
			{
				try
				{
					//...�� ��������� �������� ����� �� ������...
					eFileStream = new FileStream(this.fullFilename, FileMode.Open, System.IO.FileAccess.Read);

					//...� ��������� ���������������� �� ����� �����, ����� ������� �������� CRC-64
					eFileStream.Seek(-8, SeekOrigin.End);

					// ������ ����������� � ����� ����� �������� CRC-64...
					int readed = 0;
					int toRead = 8;
					while((toRead -= (readed += eFileStream.Read(crc64Arr, readed, toRead))) != 0) ;

					//...� ��������� ����
					if(eFileStream != null)
					{
						eFileStream.Close();
						eFileStream = null;
					}
				}

					// ���� ���� ���� �� ���� ���������� - ��������� �������� ����� �
					// �������� �� ������
				catch
				{
					if(eFileStream != null)
					{
						eFileStream.Close();
						eFileStream = null;
					}

					// ���������� ���� ������������ ����������
					this.processedOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return;
				}

				// ������ ����������� ������ byte[] � UInt64
				crc64 = DataConverter.GetUInt64(crc64Arr);

				// ���� ����������� �������� CRC-64 �� ������� � �����������,
				// ��������� �� ������
				this.processedOK = (this.crc64 == crc64);
			}
			else
			{
				// ��������� �� ��, ��� ������ CRC-64 ����� ������ �� ���������
				this.processedOK = false;
			}

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		#endregion Private Operations
	}
}