/*----------------------------------------------------------------------+
 |  filename:   FileSplitter.cs                                         |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ���������� (����������) ������-����� �� ���������       |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;
using System.IO;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ��� ���������� (����������) ������ �� ���������
	/// </summary>
	public class FileSplitter
	{
		#region Delegates

		/// <summary>
		/// ������� ���������� ��������� ��������� (����������) �����
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateFileSplittingProgress;

		/// <summary>
		/// ������� ���������� �������� ��������� (����������) �����
		/// </summary>
		public OnEventHandler OnFileSplittingFinish;

		#endregion Delegates

		#region Constants

		/// <summary>
		/// ��������� ������ CBC-����� � ���������� ��-��������� (128 ��)
		/// </summary>
		private const int defCbcBlockSize = 1 << 17;

		/// <summary>
		/// ������ ����� 256 ��� � ������
		/// </summary>
		private const int bits256 = 32;

		#endregion Constants

		#region Public Properties & Data

		/// <summary>
		/// ��������� �������� "���� ��������������?"
		/// </summary>
		public bool InProcessing
		{
			get
			{
				if(
					(this.thrFileSplitter != null)
					&&
					(
						(this.thrFileSplitter.ThreadState == ThreadState.Running)
						||
						(this.thrFileSplitter.ThreadState == ThreadState.WaitSleepJoin)
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
		/// ��������� ������ ����������� ���������?"
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
		/// ��������� ����������� ���������?
		/// </summary>
		private bool processedOK;

		/// <summary>
		/// ����������������� ������ ������
		/// </summary>
		public Security Security
		{
			get
			{
				if(!InProcessing)
				{
					return this.eSecurity;
				}
				else
				{
					return null;
				}
			}

			set
			{
				if(!InProcessing)
				{
					this.eSecurity = value;
				}
			}
		}

		/// <summary>
		/// ��������� ������ ����������������� ������ ������
		/// </summary>
		private Security eSecurity;

		/// <summary>
		/// ������ CBC-����� (��), ������������ ��� ����������
		/// </summary>
		public int CBCBlockSize
		{
			get
			{
				if(!InProcessing)
				{
					return this.cbcBlockSize;
				}
				else
				{
					return -1;
				}
			}

			set
			{
				if(!InProcessing)
				{
					if(value > 0)
					{
						this.cbcBlockSize = value;
					}
					else
					{
						this.cbcBlockSize = defCbcBlockSize;
					}
				}
			}
		}

		/// <summary>
		/// ������ CBC-����� (��), ������������ ��� ����������
		/// </summary>
		private int cbcBlockSize;

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
					(this.thrFileSplitter != null)
					&&
					(this.thrFileSplitter.IsAlive)
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
					this.thrFileSplitter.Priority = this.threadPriority;
				}
			}
		}

		/// <summary>
		/// ��������� �������� ��������� (����������) �����
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

		#endregion Public Properties & Data

		#region Data

		/// <summary>
		/// ��������� ������ ��� ������������ ����� ����
		/// </summary>
		private FileNamer eFileNamer;

		/// <summary>
		/// ���� � ������ ��� ���������
		/// </summary>
		private String path;

		/// <summary>
		/// ��� �����
		/// </summary>
		private String fileName;

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		private int dataCount;

		/// <summary>
		/// ���������� ����� ��� ��������������
		/// </summary>
		private int eccCount;

		/// <summary>
		/// ��� ������ ����-�������� (�� ���� ������������ ������� �����������)
		/// </summary>
		private int codecType;

		/// <summary>
		/// �������� �����
		/// </summary>
		private byte[] buffer;

		/// <summary>
		/// ����� ��������� (����������) ����� �� ���������
		/// </summary>
		private Thread thrFileSplitter;

		/// <summary>
		/// ������� ����������� ��������� ������
		/// </summary>
		private ManualResetEvent[] exitEvent;

		/// <summary>
		/// ������� ����������� ��������� ������
		/// </summary>
		private ManualResetEvent[] executeEvent;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� ������
		/// </summary>
		public FileSplitter()
		{
			// ������������� ������ CBC-����� ��-���������
			this.cbcBlockSize = defCbcBlockSize;

			// ������� ��������� ������ ��� ������������ ����� ����
			this.eFileNamer = new FileNamer();

			// ���� � ������ ��� ��������� ��-��������� ������
			this.path = "";

			// �������������� ��� ����� ��-���������
			this.fileName = "NONAME";

			// �������� ������ ��������� �������� ���������?
			this.finished = true;

			// ��������� ����������� ���������?
			this.processedOK = false;

			// ��-��������� ��������������� ������� ���������
			this.threadPriority = 0;

			// �������������� ������� ����������� ��������� ������
			this.exitEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������������� c������ ����������� ��������� ������
			this.executeEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������, ��������������� �� ���������� ���������
			this.finishedEvent = new ManualResetEvent[] {new ManualResetEvent(true)};
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// ��������� ��������� ����� �� ��������� (����)
		/// </summary>
		/// <param name="path">���� � ������ ��� ���������</param>
		/// <param name="fileName">��� ����� ��� ���������</param>
		/// <param name="dataCount">������������ ���������� �������� �����</param>
		/// <param name="eccCount">������������ ���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToSplit(String path, String fileName, int dataCount, int eccCount, int codecType, bool runAsSeparateThread)
		{
			// ���� ����� ��������� ����� �� ��������� �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			// ��������� ���� � ������ ��� ���������
			if(path == null)
			{
				this.path = "";
			}
			else
			{
				// ���������� ��������� ���� �� "path" � ������,
				// ���� ���� ���� �������� ������ ���
				this.path = this.eFileNamer.GetPath(path);
			}

			if(fileName == null)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ���������� ��������� ��������� ����� ����� �� "fileName" � ������,
			// ���� ���� ��� �������� ������ ���
			this.fileName = this.eFileNamer.GetShortFileName(fileName);

			// ��������� �� ������������ ������������
			if(
				(dataCount <= 0)
				||
				(eccCount <= 0)
				||
				((dataCount + eccCount) > (int)RSConst.MaxVolCountAlt)
				)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ��������� ���������� �������� �����
			this.dataCount = dataCount;

			// ��������� ���������� ����� ��� ��������������
			this.eccCount = eccCount;

			// ��������� ��� ������ ����-��������
			this.codecType = codecType;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������� �������� ���� �� ���������
				Split();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ��������� ����� �� ���������...
			this.thrFileSplitter = new Thread(new ThreadStart(Split));

			//...����� ���� ��� ���...
			this.thrFileSplitter.Name = "FileSplitter.Split()";

			//...������������� ��������� ��������� ������...
			this.thrFileSplitter.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileSplitter.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// ���������� ����� �� ����������
		/// </summary>
		/// <param name="path">���� � ������ ��� ���������</param>
		/// <param name="fileName">��� ����� ������ �� �������� �����</param>
		/// <param name="dataCount">������������ ���������� �������� �����</param>
		/// <param name="eccCount">������������ ���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToGlue(String path, String fileName, int dataCount, int eccCount, int codecType, bool runAsSeparateThread)
		{
			// ���� ����� ���������� ����� �� ���������� �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			// ��������� ���� � ������ ��� ���������
			if(path == null)
			{
				this.path = "";
			}
			else
			{
				// ���������� ��������� ���� �� "path" � ������,
				// ���� ���� ���� �������� ������ ���
				this.path = this.eFileNamer.GetPath(path);
			}

			if(fileName == null)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ���������� ��������� ��������� ����� ����� �� "fileName" � ������,
			// ���� ���� ���� �������� ������ ���
			this.fileName = this.eFileNamer.GetShortFileName(fileName);

			// ��������� �� ������������ ������������
			if(
				(dataCount <= 0)
				||
				(eccCount <= 0)
				||
				((dataCount + eccCount) > (int)RSConst.MaxVolCountAlt)
				)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ��������� ���������� �������� �����
			this.dataCount = dataCount;

			// ��������� ���������� ����� ��� ��������������
			this.eccCount = eccCount;

			// ��������� ��� ������ ����-��������
			this.codecType = codecType;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������� "����������" ������ �� ���������� � ��������
				Glue();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ���������� ������ �� ����������...
			this.thrFileSplitter = new Thread(new ThreadStart(Glue));

			//...����� ���� ��� ���...
			this.thrFileSplitter.Name = "FileSplitter.Glue()";

			//...������������� ��������� ��������� ������...
			this.thrFileSplitter.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileSplitter.Start();

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
		/// ��������� ����� �� ���������
		/// </summary>
		private void Split()
		{
			// ���������� �������� ������� (�������� � �������)
			FileStream fileStreamSource = null;
			FileStream fileStreamTarget = null;

			// ��������� ������ ������ ������� ����� ������ � �������� ����
			BinaryWriter eBinaryWriter = null;

			try
			{
				// ��� ����� ��� ���������
				String fileName;

				// ��������� ������ ��� �����
				fileName = this.path + this.fileName;

				// ��������� ����� ��������� ����� �� ������
				fileStreamSource = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read);

				// ��������� ����� ��������� ����
				long volumeLength = fileStreamSource.Length / this.dataCount;

				// ��������� ������� �������������� ���� (�� ��������� ������ � �������)
				Int64 unwrittenCounter = fileStreamSource.Length;

				// ���� ��� ��������� ����� ���� ��� ������ �� ��������� � ����� �����,
				// ��������� ��� �� ������ ����� � ������� ����. ��� ���� � ����� ����� ����
				// ���� �� ����� ������������
				if((fileStreamSource.Length % this.dataCount) != 0)
				{
					volumeLength++;
				}

				// ����� �� ���������� RAID-��������� ������ ����-�������� �������� ������ ������
				// �����. ������������ ���.
				if((volumeLength % 2) != 0)
				{
					volumeLength++;
				}

				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ���������� ��� ����� �� "volNum"
				int progressMod1 = this.dataCount / 100;

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod1 == 0)
				{
					progressMod1 = 1;
				}

				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ���������� ��� ����� �� "i" ������ ����� �� "volNum"
				int progressMod2 = (int)((fileStreamSource.Length / this.bufferLength) / 100);

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod2 == 0)
				{
					progressMod2 = 1;
				}

				// C������ ���������� ���������� ���� � �������� �����
				Int64 volumeWriteCounter = 0;

				// ������������� ��������� ������������� ������ ������
				// (� ������ ������ ������ �� ��������� � �������� CBC-����� AES-256)
				// ������ ������ ����������� � ����������!
				if(this.eSecurity != null)
				{
					this.bufferLength = (this.cbcBlockSize * 1024);
				}

				// �������� ������ ��� �������� �����
				this.buffer = new byte[this.bufferLength + bits256]; // bits256 - ������� ��� ������������ ������ CryptoStream

				// ������������� ������ ���� ��� ������ ���������� ������
				long secVolumeLength = 0;

				// �������� �� ����� ��������� ������ (+1 ��������� �������� ��� ������ �������)
				for(int volNum = 0; volNum <= this.dataCount; volNum++)
				{
					// ���� �� ��������� �� �� ������ ��������, �� ��������� ��������
					// �������� ����� ��������� ������ � ��������� �������� �������
					// ������������ ������
					if(volNum != 0)
					{
						// ���������� ��������� �������� �������� ����� ��� �������������
						// ���������� ������ ������ ������� ����� ������ � �������� ����
						eBinaryWriter = new BinaryWriter(fileStreamTarget);

						if(eBinaryWriter != null)
						{
							// ������������ �� ����� �����...
							eBinaryWriter.Seek(0, SeekOrigin.End);

							//...� ����� � ��� ����� ����� ����� �������� ������...
							eBinaryWriter.Write(volumeWriteCounter);

							//...�, �����, �������� �������� ��������
							volumeWriteCounter = 0;

							// ���������� ����� "BinaryWriter"
							eBinaryWriter.Flush();

							// ���������
							eBinaryWriter.Close();
							eBinaryWriter = null;
						}

						if(fileStreamTarget != null)
						{
							//...� ��������� �������� �����
							fileStreamTarget.Close();
							fileStreamTarget = null;
						}
					}

					// ���� ������ ������� ���������� - �� ��������� �� ��������� ��������,
					// � ��������� ����� �� ����� (�.�. ��� ���� ��� ����������)
					if(volNum == this.dataCount)
					{
						if(fileStreamSource != null)
						{
							// ����� ������� ��������� ����� ��������� �����
							fileStreamSource.Close();
							fileStreamSource = null;
						}

						// ��������, ��� ��������� ����� ���������
						if(OnFileSplittingFinish != null)
						{
							OnFileSplittingFinish();
						}

						break;
					}

					// ��������� �������������� ��� �����
					fileName = this.fileName;

					// ����������� �������� ��� ����� � ���������� ������
					// (��� �������� ���� ����� � �����)
					if(!this.eFileNamer.Pack(ref fileName, volNum, this.dataCount, this.eccCount, this.codecType))
					{
						// ��������� �������� � ������� �������� ������
						if(fileStreamSource != null)
						{
							fileStreamSource.Close();
							fileStreamSource = null;
						}

						if(fileStreamTarget != null)
						{
							fileStreamTarget.Close();
							fileStreamTarget = null;
						}

						// ��������� �� ��, ��� ��������� ������ ������ � �������
						this.processedOK = false;

						// ���������� ��������� ����������� ��������� ����������-������
						this.finished = true;

						// ������������� ������� ���������� ���������
						this.finishedEvent[0].Set();

						return;
					}

					// ��������� ������ ��� �����
					fileName = this.path + fileName;

					// ���������� ���� �� ������� �����...
					if(File.Exists(fileName))
					{
						//...���� ������� �������, ������ �� ���� ��������
						// ��-���������...
						File.SetAttributes(fileName, FileAttributes.Normal);
					}

					// ...����� ��������� ����� �������� ����� �� ������
					fileStreamTarget = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write);

					// ���������� �������� �������� (����������� ������ �������)
					Int64 nIterations = -1;

					// �������, �� ����������� ��� ���������� �������� ��������
					int iterRest = -1;

					// ���� ���� ��� ��������
					if(unwrittenCounter > 0)
					{
						// ���� ������� �������������� ���� ������ ���� ����� ������� ���� -
						// ����� ��������� ������� ����������� � ����������� ��������� ��������
						// ������� ���� � ��������� �� ����� ��������
						if(unwrittenCounter >= volumeLength)
						{
							// ������ ���������� �������� �������� (����������� ������ �������)
							nIterations = volumeLength / this.bufferLength;

							// ��������� �������, �� ����������� ��� ���������� �������� ��������
							iterRest = (int)(volumeLength - (nIterations * this.bufferLength));

							// ���� ��������� �� ������ �������� - ��������� ������ ����
							if(volNum == 0)
							{
								// ������ ������������������ �������� ����� ����
								secVolumeLength = volumeLength;

								// ���� ���������� ����� ����������� ������ ������, ����������
								// ���������� ��������� bits256 (��� ������������ �� ������� ������
								// ����������, 256 ���)
								if((secVolumeLength % bits256) != 0)
								{
									secVolumeLength += (bits256 - (secVolumeLength % bits256));
								}

								// bits256 - ������� ��� ������������ ������ CryptoStream,
								// � �� ������ �������� ������ ������� �� ����
								secVolumeLength += (bits256 * nIterations);

								// ���� ������� ��� ��������� ��������
								if(iterRest != 0)
								{
									secVolumeLength += bits256;
								}
							}
						}
						else
						{
							// ������ ���������� �������� �������� (����������� ������ �������)
							nIterations = unwrittenCounter / this.bufferLength;

							// ��������� �������, �� ����������� ��� ���������� �������� ��������
							iterRest = (int)(unwrittenCounter - (nIterations * this.bufferLength));

							// ���� ��������� �� ������ �������� - ��������� ������ ����
							if(volNum == 0)
							{
								// ������ ������������������ �������� ����� ����
								secVolumeLength = unwrittenCounter;

								// ���� ���������� ����� ����������� ������ ������, ����������
								// ���������� ��������� bits256 (��� ������������ �� ������� ������
								// ����������, 256 ���)
								if((secVolumeLength % bits256) != 0)
								{
									secVolumeLength += (bits256 - (secVolumeLength % bits256));
								}

								// bits256 - ������� ��� ������������ ������ CryptoStream,
								// � �� ������ �������� ������ ������� �� ����
								secVolumeLength += (bits256 * nIterations);

								// ���� ������� ��� ��������� ��������
								if(iterRest != 0)
								{
									secVolumeLength += bits256;
								}
							}

							if(this.eSecurity != null)
							{
								// ��������� ������ ��������� ������ �� "secVolumeLength"
								fileStreamTarget.SetLength(secVolumeLength);
							}
							else
							{
								// ��������� ������ ��������� ������ �� "volumeLength"
								fileStreamTarget.SetLength(volumeLength);
							}
						}

						// ������ � ��������������� �������� (������ � �������� ���������)
						for(Int64 i = 0; i < nIterations; i++)
						{
							// ������ ������ � �����
							int dataLen = this.bufferLength;
							int readed = 0;
							int toRead = 0;
							while((toRead = dataLen - (readed += fileStreamSource.Read(this.buffer, readed, toRead))) != 0) ;

							if(this.eSecurity != null)
							{
								// ������� ������, ���� ��� ���������...
								this.eSecurity.Encrypt(this.buffer, this.bufferLength);
								fileStreamTarget.Write(this.buffer, 0, (this.bufferLength + bits256)); // bits256 - ������� ��� ������������ ������ CryptoStream
							}
							else
							{
								//...� ����� ����� � �������� ����
								fileStreamTarget.Write(this.buffer, 0, this.bufferLength);
							}

							volumeWriteCounter += this.bufferLength;
							unwrittenCounter -= this.bufferLength;

							// ������� �������� ���������
							if(
								((((volNum * nIterations) + i) % progressMod2) == 0)
								&&
								(OnUpdateFileSplittingProgress != null)
								)
							{
								OnUpdateFileSplittingProgress(((double)((volNum * nIterations) + (i + 1)) / (double)(this.dataCount * nIterations)) * 100.0);
							}

							// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
							// ����� ��������, � ����� �� ����� ������ �� ��� ���������
							ManualResetEvent.WaitAll(this.executeEvent);

							// ���� �������, ��� ��������� ����� �� ������ - �������
							if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
							{
								// ��������� �������� � ������� �������� ������
								if(fileStreamSource != null)
								{
									fileStreamSource.Close();
									fileStreamSource = null;
								}

								if(fileStreamTarget != null)
								{
									fileStreamTarget.Close();
									fileStreamTarget = null;
								}

								// ��������� �� ��, ��� ��������� ���� ��������
								this.processedOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
						}

						// ������������ ������� (���� �� ����)
						if(iterRest > 0)
						{
							// ������ ������ � �����
							int dataLen = iterRest;
							int readed = 0;
							int toRead = 0;
							while((toRead = dataLen - (readed += fileStreamSource.Read(this.buffer, readed, toRead))) != 0) ;

							if(this.eSecurity != null)
							{
								// ������������� ������ ���������� ����� ���������� ������
								// �������� ������ ����������������� �������, �� ����� ���
								// ������������ �� ��������� ���������� � ��������� 256 �����.
								int lastBlockSize = iterRest;

								// ���� ���������� ����� ����������� ������ ������, ����������
								// ���������� ��������� bits256 (��� ������������ �� ������� ������
								// ����������, 256 ���)
								if((lastBlockSize % bits256) != 0)
								{
									lastBlockSize += (bits256 - (lastBlockSize % bits256));
								}

								// ������� ������, ���� ��� ���������...
								this.eSecurity.Encrypt(this.buffer, lastBlockSize);
								fileStreamTarget.Write(this.buffer, 0, (lastBlockSize + bits256)); // bits256 - ������� ��� ������������ ������ CryptoStream
							}
							else
							{
								//...� ����� ����� � �������� ����
								fileStreamTarget.Write(this.buffer, 0, iterRest);
							}

							volumeWriteCounter += iterRest;
							unwrittenCounter -= iterRest;

							// ������� �������� ���������
							if(
								((volNum % progressMod1) == 0)
								&&
								(OnUpdateFileSplittingProgress != null)
								)
							{
								OnUpdateFileSplittingProgress(((double)(volNum + 1) / (double)this.dataCount) * 100.0);
							}

							// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
							// ����� ��������, � ����� �� ����� ������ �� ��� ���������
							ManualResetEvent.WaitAll(this.executeEvent);

							// ���� �������, ��� ��������� ����� �� ������ - �������
							if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
							{
								// ��������� �������� � ������� �������� ������
								if(fileStreamSource != null)
								{
									fileStreamSource.Close();
									fileStreamSource = null;
								}

								if(fileStreamTarget != null)
								{
									fileStreamTarget.Close();
									fileStreamTarget = null;
								}

								// ��������� �� ��, ��� ��������� ���� ��������
								this.processedOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
						}

						continue;
					}

					// ���� � �������� ������ ������ ������ ���, ����� ������ ���������
					// ���� ��� ������, �� ����� ��� �������������� 8 ���� ����������,
					// ����������� �� ���������� ������������ ������
					if(unwrittenCounter == 0)
					{
						if(this.eSecurity != null)
						{
							// ��������� ������ ��������� ������ �� "secVolumeLength"
							fileStreamTarget.SetLength(secVolumeLength);
						}
						else
						{
							// ��������� ������ ��������� ������ �� "volumeLength"
							fileStreamTarget.SetLength(volumeLength);
						}

						// ������� �������� ���������
						if(
							((volNum % progressMod1) == 0)
							&&
							(OnUpdateFileSplittingProgress != null)
							)
						{
							OnUpdateFileSplittingProgress(((double)(volNum + 1) / (double)this.dataCount) * 100.0);
						}

						// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
						// ����� ��������, � ����� �� ����� ������ �� ��� ���������
						ManualResetEvent.WaitAll(this.executeEvent);

						// ���� �������, ��� ��������� ����� �� ������ - �������
						if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
						{
							// ��������� �� ��, ��� ��������� ���� ��������
							this.processedOK = false;

							// ���������� ��������� ����������� ��������� ����������-������
							this.finished = true;

							// ������������� ������� ���������� ���������
							this.finishedEvent[0].Set();

							return;
						}

						continue;
					}
				}
			}

			// ���� ���� ���� �� ���� ���������� - ��������� �������� ������ �
			// �������� �� ������
			catch
			{
				// ��������� �� ��, ��� ��������� ������ ������ � �������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			finally
			{
				// ��������� �������� � ������� �������� ������
				if(fileStreamSource != null)
				{
					fileStreamSource.Close();
					fileStreamSource = null;
				}

				if(fileStreamTarget != null)
				{
					fileStreamTarget.Close();
					fileStreamTarget = null;
				}
			}

			// ��������� �� ��, ��� ��������� ���� ����������� ���������
			this.processedOK = true;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		/// <summary>
		/// ���������� ������ �� ����������
		/// </summary>
		private void Glue()
		{
			// ���������� �������� ������� (�������� � �������)
			FileStream fileStreamSource = null;
			FileStream fileStreamTarget = null;

			// ����� �������� ����
			int volNum;

			// ��� ����� ��� ���������
			String fileName;

			try
			{
				// ��������� ������ ��� �����
				fileName = this.path + this.fileName;

				// ���������� ���� �� ������� �����...
				if(File.Exists(fileName))
				{
					//...���� ������� �������, ������ �� ���� ��������
					// ��-���������...
					File.SetAttributes(fileName, FileAttributes.Normal);
				}

				// ...����� ��������� ����� �������� ����� �� ������
				fileStreamTarget = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write);

				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ���������� ��� ����� �� "volNum"
				int progressMod1 = this.dataCount / 100;

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod1 == 0)
				{
					progressMod1 = 1;
				}

				// ��������� �������������� ��� �����
				fileName = this.fileName;

				// ����������� �������� ��� ����� � ���������� ������ ��� ��������� ����� ������� ����
				this.eFileNamer.Pack(ref fileName, 0, this.dataCount, this.eccCount, this.codecType);

				// ��������� ������ ��� �����
				fileName = this.path + fileName;

				// ��������� ����� ��������� ����� �� ������...
				fileStreamSource = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read);

				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ���������� ��� ����� �� "i" ������ ����� �� "volNum"
				int progressMod2 = (int)(((fileStreamSource.Length - 8) / this.bufferLength) / 100);

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod2 == 0)
				{
					progressMod2 = 1;
				}

				// ��������� ����� ��������� �����
				if(fileStreamSource != null)
				{
					fileStreamSource.Close();
					fileStreamSource = null;
				}

				// ������������� ��������� ������������� ������ ������
				// (� ������ ������ ������ �� ��������� � �������� CBC-����� AES-256)
				// ������ ������ ����������� � ����������!
				if(this.eSecurity != null)
				{
					this.bufferLength = (this.cbcBlockSize * 1024);
				}

				// �������� ������ ��� �������� �����
				this.buffer = new byte[this.bufferLength + bits256]; // bits256 - ������� ��� ������������ ������ CryptoStream

				// �������� �� ����� ��������� ������
				for(volNum = 0; volNum < this.dataCount; ++volNum)
				{
					// ��������� �������������� ��� �����
					fileName = this.fileName;

					// ����������� �������� ��� ����� � ���������� ������
					// (��� �������� ���� ����� � �����)
					this.eFileNamer.Pack(ref fileName, volNum, this.dataCount, this.eccCount, this.codecType);

					// ��������� ������ ��� �����
					fileName = this.path + fileName;

					// ���� �������� ���� �� ����������, �������� �� ������
					if(!File.Exists(fileName))
					{
						// ��������� �� ��, ��� ��������� ���� ��������
						this.processedOK = false;

						// ���������� ��������� ����������� ��������� ����������-������
						this.finished = true;

						// ������������� ������� ���������� ���������
						this.finishedEvent[0].Set();

						return;
					}

					// ��������� ����� ��������� ����� �� ������...
					fileStreamSource = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read);

					//...� ��������� ���������������� �� ����� �����, ����� �������
					// ���������� �������� ���� � ������ ����
					fileStreamSource.Seek(((Int64)fileStreamSource.Length - (8 + 8)), SeekOrigin.Begin);

					// ����� ��� �������������� ��������� ������������� ������� �������� ������
					byte[] dataLengthArr = new byte[8];

					// ������ ����������� � ����� ����� �������� CRC-64...
					int dataLen = 8;
					int readed = 0;
					int toRead = 0;
					while((toRead = dataLen - (readed += fileStreamSource.Read(dataLengthArr, readed, toRead))) != 0) ;

					// ������������� ������ � ����� �� ������
					fileStreamSource.Seek(0, SeekOrigin.Begin);

					// ����������� � ����� �������� ���������� �������� ���� � ������ ����
					UInt64 dataLength;

					// ������ ����������� ������ byte[] � Int64
					dataLength = DataConverter.GetUInt64(dataLengthArr);

					// ������, ����� �� ����� ���������� �������� ���� � ������ ����, �� ��� �����
					// �������� � ������� ����
					// ������ ���������� �������� �������� (����������� ������ �������)
					Int64 nIterations = (Int64)(dataLength / (UInt64)this.bufferLength);

					// ��������� �������, �� ����������� ��� ���������� �������� ��������
					int iterRest = (int)((Int64)dataLength - (nIterations * this.bufferLength));

					// ������ � ��������������� �������� (������ � �������� ���������)
					for(Int64 i = 0; i < nIterations; i++)
					{
						if(this.eSecurity != null)
						{
							// ������ ������ � ����� (� ������ �������)
							dataLen = (this.bufferLength + bits256);
							readed = 0;
							toRead = 0;
							while((toRead = dataLen - (readed += fileStreamSource.Read(this.buffer, readed, toRead))) != 0) ;

							// �������������� ������, ���� ��� ���������...
							if(!this.eSecurity.Decrypt(this.buffer, (this.bufferLength + bits256)))
							{
								// ��������� �������� � ������� �������� ������
								if(fileStreamSource != null)
								{
									fileStreamSource.Close();
									fileStreamSource = null;
								}

								if(fileStreamTarget != null)
								{
									fileStreamTarget.Close();
									fileStreamTarget = null;
								}

								// ��������� �� ��, ��� ��������� ������ ������ � �������
								this.processedOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}

							fileStreamTarget.Write(this.buffer, 0, this.bufferLength);
						}
						else
						{
							// ������ ������ � �����
							dataLen = this.bufferLength;
							readed = 0;
							toRead = 0;
							while((toRead = dataLen - (readed += fileStreamSource.Read(this.buffer, readed, toRead))) != 0) ;

							//...� ����� ����� ��� ����������� (�� ��������� � �������)
							fileStreamTarget.Write(this.buffer, 0, this.bufferLength);
						}

						// ������� �������� ���������
						if(
							((((volNum * nIterations) + i) % progressMod2) == 0)
							&&
							(OnUpdateFileSplittingProgress != null)
							)
						{
							OnUpdateFileSplittingProgress(((double)((volNum * nIterations) + (i + 1)) / (double)(this.dataCount * nIterations)) * 100.0);
						}

						// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
						// ����� ��������, � ����� �� ����� ������ �� ��� ���������
						ManualResetEvent.WaitAll(this.executeEvent);

						// ���� �������, ��� ��������� ����� �� ������ - �������
						if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
						{
							// ��������� �������� � ������� �������� ������
							if(fileStreamSource != null)
							{
								fileStreamSource.Close();
								fileStreamSource = null;
							}

							if(fileStreamTarget != null)
							{
								fileStreamTarget.Close();
								fileStreamTarget = null;
							}

							// ��������� �� ��, ��� ��������� ���� ��������
							this.processedOK = false;

							// ���������� ��������� ����������� ��������� ����������-������
							this.finished = true;

							// ������������� ������� ���������� ���������
							this.finishedEvent[0].Set();

							return;
						}
					}

					// ������������ ������� (���� �� ����)
					if(iterRest > 0)
					{
						if(this.eSecurity != null)
						{
							// ������������� ������ ���������� ����� �������������� ������
							// �������� ������ ����������������� �������, �� ����� ���
							// ������������ �� ��������� ���������� � ��������� 256 �����.
							int lastBlockSize = iterRest;

							// ���� ���������� ����� ����������� ������ ������, ����������
							// ���������� ��������� bits256 (��� ������������ �� ������� ������
							// ����������, 256 ���)
							if((lastBlockSize % bits256) != 0)
							{
								lastBlockSize += (bits256 - (lastBlockSize % bits256));
							}

							// ������ ������ � �����
							dataLen = (lastBlockSize + bits256);
							readed = 0;
							toRead = 0;
							while((toRead = dataLen - (readed += fileStreamSource.Read(this.buffer, readed, toRead))) != 0) ;

							//...�������������� ������...
							if(!this.eSecurity.Decrypt(this.buffer, (lastBlockSize + bits256)))
							{
								// ��������� �������� � ������� �������� ������
								if(fileStreamSource != null)
								{
									fileStreamSource.Close();
									fileStreamSource = null;
								}

								if(fileStreamTarget != null)
								{
									fileStreamTarget.Close();
									fileStreamTarget = null;
								}

								// ��������� �� ��, ��� ��������� ������ ������ � �������
								this.processedOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}

							//...� ����� � ������� ���� ��� ��� ������������!
							fileStreamTarget.Write(this.buffer, 0, iterRest);
						}
						else
						{
							// ������ ������ � �����
							dataLen = iterRest;
							readed = 0;
							toRead = 0;
							while((toRead = dataLen - (readed += fileStreamSource.Read(this.buffer, readed, toRead))) != 0) ;

							//...� ����� � ������� ����
							fileStreamTarget.Write(this.buffer, 0, iterRest);
						}

						// ������� �������� ���������
						if(
							((volNum % progressMod1) == 0)
							&&
							(OnUpdateFileSplittingProgress != null)
							)
						{
							OnUpdateFileSplittingProgress(((double)(volNum + 1) / (double)this.dataCount) * 100.0);
						}

						// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
						// ����� ��������, � ����� �� ����� ������ �� ��� ���������
						ManualResetEvent.WaitAll(this.executeEvent);

						// ���� �������, ��� ��������� ����� �� ������ - �������
						if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
						{
							// ��������� �������� � ������� �������� ������
							if(fileStreamSource != null)
							{
								fileStreamSource.Close();
								fileStreamSource = null;
							}

							if(fileStreamTarget != null)
							{
								fileStreamTarget.Close();
								fileStreamTarget = null;
							}

							// ��������� �� ��, ��� ��������� ���� ��������
							this.processedOK = false;

							// ���������� ��������� ����������� ��������� ����������-������
							this.finished = true;

							// ������������� ������� ���������� ���������
							this.finishedEvent[0].Set();

							return;
						}
					}

					// ��������� ���� ��������� ����
					if(fileStreamSource != null)
					{
						fileStreamSource.Close();
						fileStreamSource = null;
					}
				}

				// ��������, ��� ��������� ����� ���������
				if(OnFileSplittingFinish != null)
				{
					OnFileSplittingFinish();
				}
			}

			// ���� ���� ���� �� ���� ���������� - ��������� �������� ������ �
			// �������� �� ������
			catch
			{
				// ��������� �� ��, ��� ��������� ������ ������ � �������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			finally
			{
				// ��������� �������� � ������� �������� ������
				if(fileStreamSource != null)
				{
					fileStreamSource.Close();
					fileStreamSource = null;
				}

				if(fileStreamTarget != null)
				{
					fileStreamTarget.Flush();
					fileStreamTarget.Close();
					fileStreamTarget = null;
				}
			}

			// ��������� �� ��, ��� ��������� ���� ����������� ���������
			this.processedOK = true;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		#endregion Private Operations
	}
}