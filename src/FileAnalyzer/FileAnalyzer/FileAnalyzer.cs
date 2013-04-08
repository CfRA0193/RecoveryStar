/*----------------------------------------------------------------------+
 |  filename:   FileAnalyzer.cs                                         |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    �������� ����������� ������ � RAID-�������� �����       |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;
using System.IO;

namespace RecoveryStar
{
	/// <summary>
	/// ����� �������� ����������� ������ ������-�����
	/// </summary>
	public class FileAnalyzer
	{
		#region Delegates

		/// <summary>
		/// ������� ���������� ��������� �������� ����������� ������
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateFileAnalyzeProgress;

		/// <summary>
		/// ������� ���������� �������� �������� ����������� ������
		/// </summary>
		public OnEventHandler OnFileAnalyzeFinish;

		/// <summary>
		/// ������� ��������� ���������� ����������� ������������ ������
		/// </summary>
		public OnUpdateTwoIntDoubleValueHandler OnGetDamageStat;

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
					(this.thrFileAnalyzer != null)
					&&
					(
						(this.thrFileAnalyzer.ThreadState == ThreadState.Running)
						||
						(this.thrFileAnalyzer.ThreadState == ThreadState.WaitSleepJoin)
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
		/// ��������� �������� "��������� ������ ���������� ���������?"
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
		/// ��������� ������ ������ ����������� ���������?
		/// </summary>
		private bool processedOK;

		/// <summary>
		/// ������ ���������� ������� ��������� �����
		/// </summary>
		public int[] VolList
		{
			get
			{
				if(!InProcessing)
				{
					return this.volList;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// ������, ����������� �� ������ �����
		/// </summary>
		private int[] volList;

		/// <summary>
		/// ��� ���� ��� �������������� ���������?
		/// </summary>
		public bool AllEccVolsOK
		{
			get
			{
				if(!InProcessing)
				{
					return this.allEccVolsOK;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// ��� ���� ��� �������������� ���������?
		/// </summary>
		private bool allEccVolsOK;

		/// <summary>
		/// ��������� ��������
		/// </summary>
		public int ThreadPriority
		{
			get { return (int)this.threadPriority; }

			set
			{
				if(
					(this.thrFileAnalyzer != null)
					&&
					(this.thrFileAnalyzer.IsAlive)
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
					this.thrFileAnalyzer.Priority = this.threadPriority;

					// ��������� ��������� ��������� ��� ��������������� �������
					if(this.eFileIntegrityCheck != null)
					{
						this.eFileIntegrityCheck.ThreadPriority = value;
					}
				}
			}
		}

		/// <summary>
		/// ��������� �������� �������� ����������� ������
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
		/// ������ ��� �������� (����������) ����� ����� � ���������� ������
		/// </summary>
		private FileNamer eFileNamer;

		/// <summary>
		/// ��������� ������ �������� ����������� ������ ������
		/// </summary>
		private FileIntegrityCheck eFileIntegrityCheck;

		/// <summary>
		/// ���� � ������ ��� ���������
		/// </summary>
		private String path;

		/// <summary>
		/// ��� �����, �������� ����������� ��������� �����
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
		/// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
		/// </summary>
		private bool fastExtraction;

		/// <summary>
		/// ����� �������� ����������� �����
		/// </summary>
		private Thread thrFileAnalyzer;

		/// <summary>
		/// ������� ����������� ��������� ������
		/// </summary>
		private ManualResetEvent[] exitEvent;

		/// <summary>
		/// ������� ����������� ��������� ������
		/// </summary>
		private ManualResetEvent[] executeEvent;

		/// <summary>
		/// ������� "�����������" ����� ��������
		/// </summary>
		private ManualResetEvent[] wakeUpEvent;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� ������
		/// </summary>
		public FileAnalyzer()
		{
			// ������ ��� �������� (����������) ����� ����� � ���������� ������
			this.eFileNamer = new FileNamer();

			// ������� ��������� ������ �������� ����������� ������ ������
			this.eFileIntegrityCheck = new FileIntegrityCheck();

			// ���� � ������ ��� ��������� ��-��������� ������
			this.path = "";

			// �������������� ��� ����� ��-���������
			this.fileName = "NONAME";

			// ���������� ��� ���� ��� �������������� ������� �������������
			this.allEccVolsOK = false;

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

			// �������������� c������ "�����������" ����� ��������
			this.wakeUpEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������, ��������������� �� ���������� ���������
			this.finishedEvent = new ManualResetEvent[] {new ManualResetEvent(true)};
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// ����� ������� ������ ��������� ���������� � ������ CRC64 � ����� ������
		/// </summary>
		/// <param name="path">���� � ������ ��� ���������</param>
		/// <param name="fileName">��� ����� ��� ���������</param>
		/// <param name="dataCount">������������ ���������� �������� �����</param>
		/// <param name="eccCount">������������ ���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToWriteCRC64(String path, String fileName, int dataCount, int eccCount, int codecType, bool runAsSeparateThread)
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

			// ��������� ��� ������ ����-�������� (�� ���� ������������ ������� �����������)
			this.codecType = codecType;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.wakeUpEvent[0].Reset();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������� CRC-64 ��� ������� �� ������ ������
				WriteCRC64();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ���������� � ������ CRC-64...
			this.thrFileAnalyzer = new Thread(new ThreadStart(WriteCRC64));

			//...����� ���� ��� ���...
			this.thrFileAnalyzer.Name = "FileAnalyzer.WriteCRC64()";

			//...������������� ��������� ��������� ������...
			this.thrFileAnalyzer.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileAnalyzer.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// ����� ������� ������ ��������� �������� CRC64, ����������� � �����
		/// ������� �� ������ ������, � �������������� ������ ��������� ����� "volList",
		/// ������� ����� ����������� ��������� ��� �������������� ������
		/// </summary>
		/// <param name="path">���� � ������ ��� ���������</param>
		/// <param name="fileName">��� ����� ��� ���������</param>
		/// <param name="dataCount">������������ ���������� �������� �����</param>
		/// <param name="eccCount">������������ ���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <param name="fastExtraction">������������ ������� ���������� �� ����� (��� �������� CRC-64)?</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToAnalyzeCRC64(String path, String fileName, int dataCount, int eccCount, int codecType, bool fastExtraction, bool runAsSeparateThread)
		{
			// ���� ����� ���������� CRC-64 �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ��� ���� ��� �������������� ������� �������������
			this.allEccVolsOK = false;

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

			// ��������� ��� ������ ����-�������� (�� ���� ������������ ������� �����������)
			this.codecType = codecType;

			// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
			this.fastExtraction = fastExtraction;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.wakeUpEvent[0].Reset();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������� � ��������� CRC-64 ��� ������� �� ������ ������ � �����������
				// �������� VolList
				AnalyzeCRC64();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ���������� � �������� CRC-64...
			this.thrFileAnalyzer = new Thread(new ThreadStart(AnalyzeCRC64));

			//...����� ���� ��� ���...
			this.thrFileAnalyzer.Name = "FileAnalyzer.AnalyzeCRC64()";

			//...������������� ��������� ��������� ������...
			this.thrFileAnalyzer.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileAnalyzer.Start();

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

			// ������� � �������� � �����
			this.wakeUpEvent[0].Set();
		}

		/// <summary>
		/// ���������� ������ ��������� �� �����
		/// </summary>
		public void Pause()
		{
			// ������ �� �����
			this.executeEvent[0].Reset();

			// ������� � �������� � �����
			this.wakeUpEvent[0].Set();
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
		/// ���������� � ������ � ����� ������ �������� CRC-64
		/// </summary>
		private void WriteCRC64()
		{
			// ��������� �������� ������, ������� �������� �������� ������� ���������
			// ����� ��� ��������� ���������� ��� ����� �� "i"
			int progressMod1 = (this.dataCount + this.eccCount) / 100;

			// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
			// �������� ��������� �� ������ �������� (���� ����� ���������)
			if(progressMod1 == 0)
			{
				progressMod1 = 1;
			}

			// ���������� ��������� ��� ����
			for(int volNum = 0; volNum < (this.dataCount + this.eccCount); volNum++)
			{
				// ��������� �������������� ��� �����
				String fileName = this.fileName;

				// �������� ��� ��������� ����� � ���������� �����
				this.eFileNamer.Pack(ref fileName, volNum, this.dataCount, this.eccCount, this.codecType);

				// ��������� ������ ��� �����
				fileName = this.path + fileName;

				// ���������� ���������� CRC-64 ��� ������� �����
				if(this.eFileIntegrityCheck.StartToWriteCRC64(fileName, true))
				{
					// ���� �������� ���������� ��������� �����
					while(true)
					{
						// ���� �� ���������� �������������� ������� "executeEvent",
						// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
						if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
						{
							//...���������������� ������ ��������������� ���������...
							this.eFileIntegrityCheck.Pause();

							//...� ���� ��������
							ManualResetEvent.WaitAll(this.executeEvent);

							// � ����� ����������, ���������, ��� ��������� ������ ������������
							this.eFileIntegrityCheck.Continue();
						}

						// ���� ����� �� ������������� �������...
						int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileIntegrityCheck.FinishedEvent[0]});

						//...���� �������� ������ � ����, ����� ���������� -
						// ��������� �� ����� ��������, �.�. �����������
						// ����� ����������� �� �����...
						if(eventIdx == 0)
						{
							//...�������������� ������� �������, ����������� ��� ����������
							this.wakeUpEvent[0].Reset();

							continue;
						}

						//...���� �������� ������ � ������ �� ���������...
						if(eventIdx == 1)
						{
							//...������������� �������������� ��������
							this.eFileIntegrityCheck.Stop();

							// ��������� �� ��, ��� ��������� ���� ��������
							this.processedOK = false;

							// ���������� ��������� ����������� ��������� ����������-������
							this.finished = true;

							// ������������� ������� ���������� ���������
							this.finishedEvent[0].Set();

							return;
						}

						//...���� �������� ������ � ���������� ��������� ��������� ����������...
						if(eventIdx == 2)
						{
							//...������� �� ����� �������� ���������� (����� � ����� � while(true)!)
							break;
						}
					} // while(true)
				}
				else
				{
					// ���������� ���� ������������ ����������
					this.processedOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return;
				}

				// � ����� � ��������� �������� ���������� �������� �������
				// ���������� ��������� ������ ���������, ��������� �������
				// ����������� � ���� ������. ����� ��� �� ��������, ��
				// ������������� �� ��������� ��������, ��������, ���
				// "�� ����������"
				for(int i = 0; i < (int)WaitCount.MaxWaitCount; i++)
				{
					if(!this.eFileIntegrityCheck.Finished)
					{
						Thread.Sleep((int)WaitTime.MinWaitTime);
					}
					else
					{
						break;
					}
				}

				// ���� ����� �������� �������� �������� ������� �� ������� � ���������
				// ���������� - ��� ������
				if(!this.eFileIntegrityCheck.ProcessedOK)
				{
					// ��������� �� ��, ��� ��������� �� ���� ��������� ���������
					this.processedOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return;
				}

				// ������� �������� ���������
				if(
					((volNum % progressMod1) == 0)
					&&
					(OnUpdateFileAnalyzeProgress != null)
					)
				{
					OnUpdateFileAnalyzeProgress(((double)(volNum + 1) / (double)(this.dataCount + this.eccCount)) * 100.0);
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
			}

			// �������� �� ��������� �������� ���������
			if(OnFileAnalyzeFinish != null)
			{
				OnFileAnalyzeFinish();
			}

			// ��������, ��� ��������� ������ ���������
			this.processedOK = true;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		/// <summary>
		/// ���������� � �������� �������� CRC-64, ����������� � ����� �����
		/// </summary>
		private void AnalyzeCRC64()
		{
			// ��������� �������� ������, ������� �������� �������� ������� ���������
			// ����� ��� ��������� ���������� ��� ����� �� "i"
			int progressMod1 = (this.dataCount + this.eccCount) / 100;

			// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
			// �������� ��������� �� ������ �������� (���� ����� ���������)
			if(progressMod1 == 0)
			{
				progressMod1 = 1;
			}

			// �������� ������ ��� "volList"
			this.volList = new int[this.dataCount];

			// �������� ������ ��� "eccList"
			int[] eccList = new int[this.eccCount];

			// ������ � ������� �����
			int volListIdx = 0;

			// ������ � ������� ����� ��� ��������������
			int eccListIdx = 0;

			// ������� ���������� ������������ �������� �����
			int dataVolMissCount = 0;

			// ������� ���������� ��������� ����� ��� ��������������
			int eccVolPresentCount = 0;

			// ��� ����� ��� ���������
			String fileName;

			// ���������� �������� ��� �������� ����
			for(int dataNum = 0; dataNum < this.dataCount; dataNum++)
			{
				// ���������� ������������, ��� ������� ��� ���������
				bool dataVolIsOK = false;

				// ��������� �������������� ��� �����
				fileName = this.fileName;

				// �������� ��� ��������� ����� � ���������� �����
				this.eFileNamer.Pack(ref fileName, dataNum, this.dataCount, this.eccCount, this.codecType);

				// ��������� ������ ��� �����
				fileName = this.path + fileName;

				// ���� �������� ���� ����������...
				if(File.Exists(fileName))
				{
					// ���� �� ������������ ������� ���������� - ��������� �� �����������
					// CRC-64, ����� ��������, ��� �� ��������� (����������� ���� �����
					// �� ����� ��� �������)
					if(!this.fastExtraction)
					{
						//...- ���������� ��� ��������
						if(this.eFileIntegrityCheck.StartToCheckCRC64(fileName, true))
						{
							// ���� �������� ���������� ��������� �����
							while(true)
							{
								// ���� �� ���������� �������������� ������� "executeEvent",
								// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
								if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
								{
									//...���������������� ������ ��������������� ���������...
									this.eFileIntegrityCheck.Pause();

									//...� ���� ��������
									ManualResetEvent.WaitAll(this.executeEvent);

									// � ����� ����������, ���������, ��� ��������� ������ ������������
									this.eFileIntegrityCheck.Continue();
								}

								// ���� ����� �� ������������� �������...
								int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileIntegrityCheck.FinishedEvent[0]});

								//...���� �������� ������ � ����, ����� ���������� -
								// ��������� �� ����� ��������, �.�. �����������
								// ����� ����������� �� �����...
								if(eventIdx == 0)
								{
									//...�������������� ������� �������, ����������� ��� ����������
									this.wakeUpEvent[0].Reset();

									continue;
								}

								//...���� �������� ������ � ������ �� ���������...
								if(eventIdx == 1)
								{
									//...������������� �������������� ��������
									this.eFileIntegrityCheck.Stop();

									// ��������� �� ��, ��� ��������� ���� ��������
									this.processedOK = false;

									// ���������� ��������� ����������� ��������� ����������-������
									this.finished = true;

									// ������������� ������� ���������� ���������
									this.finishedEvent[0].Set();

									return;
								}

								//...���� �������� ������ � ���������� ��������� ��������� ����������...
								if(eventIdx == 2)
								{
									//...������� �� ����� �������� ���������� (����� � ����� � while(true)!)
									break;
								}
							} // while(true)
						}
						else
						{
							// ���������� ���� ������������ ����������
							this.processedOK = false;

							// ���������� ��������� ����������� ��������� ����������-������
							this.finished = true;

							// ������������� ������� ���������� ���������
							this.finishedEvent[0].Set();

							return;
						}

						// � ����� � ��������� �������� ���������� �������� �������
						// ���������� ��������� ������ ���������, ��������� �������
						// ����������� � ���� ������. ����� ��� �� ��������, ��
						// ������������� �� ��������� ��������, ��������, ���
						// "�� ����������"
						for(int i = 0; i < (int)WaitCount.MaxWaitCount; i++)
						{
							if(!this.eFileIntegrityCheck.Finished)
							{
								Thread.Sleep((int)WaitTime.MinWaitTime);
							}
							else
							{
								break;
							}
						}

						// ���������, ��� �������� ��� ���������
						if(this.eFileIntegrityCheck.ProcessedOK)
						{
							dataVolIsOK = true;
						}
					}
					else
					{
						// ���������, ��� �������� ��� ���������
						dataVolIsOK = true;
					}

					// ������� �������� ���������
					if(
						((dataNum % progressMod1) == 0)
						&&
						(OnUpdateFileAnalyzeProgress != null)
						)
					{
						OnUpdateFileAnalyzeProgress(((double)(dataNum + 1) / (double)(this.dataCount + this.eccCount)) * 100.0);
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
				}

				// ���� ������ �������� ��� �� ���������, ���������� ��� � "volList",
				// � ����� ����������� ������� ������������ ����� � ������ �� �����
				// ������ ���� �������� "-1", ������� ������ �� ������������� �����������
				// ���� ��� ��������������
				if(dataVolIsOK)
				{
					this.volList[volListIdx++] = dataNum;
				}
				else
				{
					this.volList[volListIdx++] = -1;

					// ����������� ������� ���������� ������������ �������� �����
					dataVolMissCount++;
				}
			}

			// ������, ����� ����� ���������� ������������ �������� �����,
			// ����� �������������� ��� ����� ��� ��������������, � ����������
			// ��������� �� ����� � ������ �����, � "�������" ��������� �
			// ������ �������������� ����� ��� ��������������
			for(int eccNum = this.dataCount; eccNum < (this.dataCount + this.eccCount); eccNum++)
			{
				// ���������� ������������, ��� ������� ��� ���������
				bool eccVolIsOK = false;

				// ��������� �������������� ��� �����
				fileName = this.fileName;

				// �������� ��� ��������� ����� � ���������� �����
				this.eFileNamer.Pack(ref fileName, eccNum, this.dataCount, this.eccCount, this.codecType);

				// ��������� ������ ��� �����
				fileName = this.path + fileName;

				// ���� �������� ���� ����������...
				if(File.Exists(fileName))
				{
					// ���� �� ������������ ������� ���������� - ��������� �� �����������
					// CRC-64, ����� ��������, ��� �� ��������� (����������� ���� �����
					// �� ����� ��� �������)
					if(!this.fastExtraction)
					{
						//...- ���������� ��� ��������
						if(this.eFileIntegrityCheck.StartToCheckCRC64(fileName, true))
						{
							// ���� �������� ���������� ��������� �����
							while(true)
							{
								// ���� �� ���������� �������������� ������� "executeEvent",
								// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
								if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
								{
									//...���������������� ������ ��������������� ���������...
									this.eFileIntegrityCheck.Pause();

									//...� ���� ��������
									ManualResetEvent.WaitAll(this.executeEvent);

									// � ����� ����������, ���������, ��� ��������� ������ ������������
									this.eFileIntegrityCheck.Continue();
								}

								// ���� ����� �� ������������� �������...
								int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileIntegrityCheck.FinishedEvent[0]});

								//...���� �������� ������ � ����, ����� ���������� -
								// ��������� �� ����� ��������, �.�. �����������
								// ����� ����������� �� �����...
								if(eventIdx == 0)
								{
									//...�������������� ������� �������, ����������� ��� ����������
									this.wakeUpEvent[0].Reset();

									continue;
								}

								//...���� �������� ������ � ������ �� ���������...
								if(eventIdx == 1)
								{
									//...������������� �������������� ��������
									this.eFileIntegrityCheck.Stop();

									// ��������� �� ��, ��� ��������� ���� ��������
									this.processedOK = false;

									// ���������� ��������� ����������� ��������� ����������-������
									this.finished = true;

									// ������������� ������� ���������� ���������
									this.finishedEvent[0].Set();

									return;
								}

								//...���� �������� ������ � ���������� ��������� ��������� ����������...
								if(eventIdx == 2)
								{
									//...������� �� ����� �������� ���������� (����� � ����� � while(true)!)
									break;
								}
							} // while(true)
						}
						else
						{
							// ���������� ���� ������������ ����������
							this.processedOK = false;

							// ���������� ��������� ����������� ��������� ����������-������
							this.finished = true;

							// ������������� ������� ���������� ���������
							this.finishedEvent[0].Set();

							return;
						}

						// � ����� � ��������� �������� ���������� �������� �������
						// ���������� ��������� ������ ���������, ��������� �������
						// ����������� � ���� ������. ����� ��� �� ��������, ��
						// ������������� �� ��������� ��������, ��������, ���
						// "�� ����������"
						for(int i = 0; i < (int)WaitCount.MaxWaitCount; i++)
						{
							if(!this.eFileIntegrityCheck.Finished)
							{
								Thread.Sleep((int)WaitTime.MinWaitTime);
							}
							else
							{
								break;
							}
						}

						// ���������, ��� ��� ��� �������������� ���������
						if(this.eFileIntegrityCheck.ProcessedOK)
						{
							eccVolIsOK = true;
						}
					}
					else
					{
						// ���������, ��� ��� ��� �������������� ���������
						eccVolIsOK = true;
					}

					// ������� �������� ���������
					if(
						((eccNum % progressMod1) == 0)
						&&
						(OnUpdateFileAnalyzeProgress != null)
						)
					{
						OnUpdateFileAnalyzeProgress(((double)(eccNum + 1) / (double)(this.dataCount + this.eccCount)) * 100.0);
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
				}

				// ���� ��� ��� �������������� �������...
				if(eccVolIsOK)
				{
					//...- ��������� ��� � ������
					eccList[eccListIdx++] = eccNum;

					// ����������� ������� ���������� ����� ��� ��������������
					eccVolPresentCount++;
				}
				else
				{
					//...� ����� ���������, ��� ��� ���������
					eccList[eccListIdx++] = -1;
				}
			}

			// ���� �������� �������� ���������� ���������� ����� ��� �������������� ���������
			// �� ��������� �������� ����� ��� �������������� ������������ - ��� ���� ���
			// �������������� �������� ���������������
			if(eccVolPresentCount == this.eccCount)
			{
				this.allEccVolsOK = true;
			}

			// ������� ���������� �����������
			if(OnGetDamageStat != null)
			{
				// ��������� ����� ������� ����������� (����� ����������� �������� ����� � ����� ��� ��������������
				// ����� �� ����� ���������� �����)
				double percOfDamage = ((double)(dataVolMissCount + (this.eccCount - eccVolPresentCount)) / (double)(this.dataCount + this.eccCount)) * 100;

				// ��������� ������� "��������" �������������� ����� ��� ��������������
				// �������������� ���� - ��� ���������� �� ����, ������� �� ����������� ������������ ��� ��������������
				double percOfAltEcc = ((double)(eccVolPresentCount - dataVolMissCount) / (double)this.eccCount) * 100;

				// ������� ���������� �����������
				OnGetDamageStat(dataVolMissCount + (this.eccCount - eccVolPresentCount), (eccVolPresentCount - dataVolMissCount), percOfDamage, percOfAltEcc);
			}

			// ���� ��� ������������ �������� �����, ������ �������
			if(dataVolMissCount == 0)
			{
				// �������� �� ��������� �������� ���������
				if(OnFileAnalyzeFinish != null)
				{
					OnFileAnalyzeFinish();
				}

				// ��������� �� ��, ��� ������ �� ����������
				this.processedOK = true;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ���� �� �� ������ ������������ �����������...
			if(eccVolPresentCount < dataVolMissCount)
			{
				// �������� �� ��������� �������� ���������
				if(OnFileAnalyzeFinish != null)
				{
					OnFileAnalyzeFinish();
				}

				//...��������� �� ��, ��� ������ �� ����� ���� �������������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ������������ �� ������ ������ ����� ��� ��������������
			eccListIdx = 0;

			// ������ ����������� �� ������� "volList", � ������ ������� �� �������� "-1"
			// ����������� ��������� �������� �� ���������� ���������
			for(int i = 0; i < this.dataCount; i++)
			{
				if(this.volList[i] == -1)
				{
					// ����������� �� ������� ����� ��� ��������������,
					// �������������� �� ���������� ���� ��� ��������������
					while(eccList[eccListIdx] == -1)
					{
						eccListIdx++;
					}

					// ����������� �� ����� ������������� ��������� ����
					// ��� ��� ��������������,...
					this.volList[i] = eccList[eccListIdx];

					//...������ �������������� ��� �� ������
					eccList[eccListIdx] = -1;
				}
			}

			// �������� �� ��������� �������� ���������
			if(OnFileAnalyzeFinish != null)
			{
				OnFileAnalyzeFinish();
			}

			// ��������, ��� ��������� ������ ���������
			this.processedOK = true;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		#endregion Private Operations
	}
}