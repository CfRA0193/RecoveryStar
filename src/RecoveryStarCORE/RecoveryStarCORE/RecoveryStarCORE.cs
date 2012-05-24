/*----------------------------------------------------------------------+
 |  filename:   RecoveryStarCore.cs                                     |
 |----------------------------------------------------------------------|
 |  version:    2.20                                                    |
 |  revision:   23.05.2012 17:33                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ���������������� ����������� �� ���� RAID-������        |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;
using System.IO;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ��� ����������� ������ � RAID-�������� �������
	/// </summary>
	public class RecoveryStarCore
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

		/// <summary>
		/// ������� ���������� �������� ������������ ������� "F"
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateRSMatrixFormingProgress;

		/// <summary>
		/// ������� ���������� �������� ������������ ������� "F"
		/// </summary>
		public OnEventHandler OnRSMatrixFormingFinish;

		/// <summary>
		/// ������� ���������� ��������� �������� �������� �������
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateFileStreamsOpeningProgress;

		/// <summary>
		/// ������� ���������� �������� �������� �������� �������
		/// </summary>
		public OnEventHandler OnFileStreamsOpeningFinish;

		/// <summary>
		/// ������� ������ ����������� ����-��������
		/// </summary>
		public OnEventHandler OnStartedRSCoding;

		/// <summary>
		/// ������� ���������� ��������� ����������� ������
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateFileCodingProgress;

		/// <summary>
		/// ������� ���������� �������� ����������� ������
		/// </summary>
		public OnEventHandler OnFileCodingFinish;

		/// <summary>
		/// ������� ���������� ��������� �������� �������� �������
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateFileStreamsClosingProgress;

		/// <summary>
		/// ������� ���������� �������� �������� �������� �������
		/// </summary>
		public OnEventHandler OnFileStreamsClosingFinish;

		/// <summary>
		/// ������� ���������� �������� �������� ����������� ������
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
					(this.thrRecoveryStarCore != null)
					&&
					(
						(this.thrRecoveryStarCore.ThreadState == ThreadState.Running)
						||
						(this.thrRecoveryStarCore.ThreadState == ThreadState.WaitSleepJoin)
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
				if(this.eFileSplitter != null)
				{
					return this.eFileSplitter.Security;
				}
				else
				{
					return null;
				}
			}

			set
			{
				if(this.eFileSplitter != null)
				{
					this.eFileSplitter.Security = value;
				}
			}
		}

		/// <summary>
		/// ������ CBC-����� (��), ������������ ��� ����������
		/// </summary>
		public int CBCBlockSize
		{
			get
			{
				if(this.eFileSplitter != null)
				{
					return this.eFileSplitter.CBCBlockSize;
				}
				else
				{
					return -1;
				}
			}

			set
			{
				if(this.eFileSplitter != null)
				{
					this.eFileSplitter.CBCBlockSize = value;
				}
			}
		}

		/// <summary>
		/// ��������� ��������
		/// </summary>
		public int ThreadPriority
		{
			get { return (int)this.threadPriority; }

			set
			{
				if(
					(this.thrRecoveryStarCore != null)
					&&
					(this.thrRecoveryStarCore.IsAlive)
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
					this.thrRecoveryStarCore.Priority = this.threadPriority;

					// ��������� ��������� ��������� ��� �������������� ��������
					if(this.eFileAnalyzer != null)
					{
						this.eFileAnalyzer.ThreadPriority = value;
					}

					if(this.eFileCodec != null)
					{
						this.eFileCodec.ThreadPriority = value;
					}

					if(this.eFileSplitter != null)
					{
						this.eFileSplitter.ThreadPriority = value;
					}
				}
			}
		}

		/// <summary>
		/// ��������� �������� ��������� �����
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
		/// ������ ���������� � �������� ��������� ����������� ����� CRC-64
		/// </summary>
		private FileAnalyzer eFileAnalyzer;

		/// <summary>
		/// RAID-�������� �������� �����
		/// </summary>
		private FileCodec eFileCodec;

		/// <summary>
		/// ������ ��������� (����������) ������ �� ����
		/// </summary>
		private FileSplitter eFileSplitter;

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
		/// ���� � ������ ��� ���������
		/// </summary>
		private String path;

		/// <summary>
		/// ��� ��������� ����� ��� ���������
		/// </summary>
		private String fileName;

		/// <summary>
		/// �������� ������ ��������� �������� ���������?
		/// </summary>
		private bool finished;

		/// <summary>
		/// ����� ����������� ������
		/// </summary>
		private Thread thrRecoveryStarCore;

		/// <summary>
		/// ������� ����������� ��������� �����
		/// </summary>
		private ManualResetEvent[] exitEvent;

		/// <summary>
		/// ������� ����������� ��������� �����
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
		public RecoveryStarCore()
		{
			// ������ ��� �������� (����������) ����� ����� � ���������� ������
			this.eFileNamer = new FileNamer();

			// ������ ���������� � �������� ��������� ����������� ����� CRC-64
			this.eFileAnalyzer = new FileAnalyzer();

			// RAID-�������� �������� �����
			this.eFileCodec = new FileCodec();

			// ������ ��������� (����������) ������ �� ����
			this.eFileSplitter = new FileSplitter();

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

			// �������������� c������ "�����������" ����� ��������
			this.wakeUpEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������, ��������������� �� ���������� ���������
			this.finishedEvent = new ManualResetEvent[] {new ManualResetEvent(true)};
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// ���������������� ����������� ����� �� ���� RAID
		/// </summary>
		/// <param name="fullFileName">������ ��� ����� ��� ����������������� �����������</param>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToProtect(String fullFileName, int dataCount, int eccCount, int codecType,
		                           bool runAsSeparateThread)
		{
			// ���� ����� ����������� ����� �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			// ���� ��� ����� �� �����������
			if(
				(fullFileName == null)
				||
				(fullFileName == "")
				)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ���������� ��������� ���� �� ������� ����� �����
			this.path = this.eFileNamer.GetPath(fullFileName);

			// ���������� ��������� ����� �� ������� ����� �����
			this.fileName = this.eFileNamer.GetShortFileName(fullFileName);

			// ���� �������� ���� �� ����������, �������� �� ������
			if(!File.Exists(this.path + this.fileName))
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

			// ������������� �� ���������
			this.eFileSplitter.OnUpdateFileSplittingProgress = OnUpdateFileSplittingProgress;
			this.eFileSplitter.OnFileSplittingFinish = OnFileSplittingFinish;

			this.eFileCodec.OnUpdateRSMatrixFormingProgress = OnUpdateRSMatrixFormingProgress;
			this.eFileCodec.OnRSMatrixFormingFinish = OnRSMatrixFormingFinish;
			this.eFileCodec.OnUpdateFileStreamsOpeningProgress = OnUpdateFileStreamsOpeningProgress;
			this.eFileCodec.OnFileStreamsOpeningFinish = OnFileStreamsOpeningFinish;
			this.eFileCodec.OnStartedRSCoding = OnStartedRSCoding;
			this.eFileCodec.OnUpdateFileCodingProgress = OnUpdateFileCodingProgress;
			this.eFileCodec.OnFileCodingFinish = OnFileCodingFinish;
			this.eFileCodec.OnUpdateFileStreamsClosingProgress = OnUpdateFileStreamsClosingProgress;
			this.eFileCodec.OnFileStreamsClosingFinish = OnFileStreamsClosingFinish;

			this.eFileAnalyzer.OnUpdateFileAnalyzeProgress = OnUpdateFileAnalyzeProgress;
			this.eFileAnalyzer.OnFileAnalyzeFinish = OnFileAnalyzeFinish;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.wakeUpEvent[0].Reset();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// �������� ���� �� ����������� (�������� ���)
				Protect();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ����������� ������...
			this.thrRecoveryStarCore = new Thread(new ThreadStart(Protect));

			//...����� ���� ��� ���...
			this.thrRecoveryStarCore.Name = "RecoveryStarCore.Protect()";

			//...������������� ��������� ��������� ������...
			this.thrRecoveryStarCore.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrRecoveryStarCore.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// ���������������� ������������� �����
		/// </summary>
		/// <param name="fullFileName">������ ��� ����� ��� ��������������</param>
		/// <param name="fastExtraction">������������ ������� ���������� �� ����� (��� �������� CRC-64)?</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToRecover(String fullFileName, bool fastExtraction, bool runAsSeparateThread)
		{
			// ���� ����� ����������� ����� �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			// ���� ��� ����� �� �����������
			if(
				(fullFileName == null)
				||
				(fullFileName == "")
				)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ���������� ��������� ���� �� ������� ����� �����
			this.path = this.eFileNamer.GetPath(fullFileName);

			// ���������� ��������� ����� �� ������� ����� �����
			this.fileName = this.eFileNamer.GetShortFileName(fullFileName);

			// ���� �������� ���� �� ����������, �������� �� ������
			if(!File.Exists(this.path + this.fileName))
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ������������� �������� ��� ����� �� ����������� �������,
			// � � ���������� �������� ��������� "fileName", "dataCount", "eccCount", "codecType"
			if(!this.eFileNamer.Unpack(ref this.fileName, ref this.dataCount, ref this.eccCount, ref this.codecType))
			{
				return false;
			}

			// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
			this.fastExtraction = fastExtraction;

			// ������������� �� ���������
			this.eFileSplitter.OnUpdateFileSplittingProgress = OnUpdateFileSplittingProgress;
			this.eFileSplitter.OnFileSplittingFinish = OnFileSplittingFinish;

			this.eFileCodec.OnUpdateRSMatrixFormingProgress = OnUpdateRSMatrixFormingProgress;
			this.eFileCodec.OnRSMatrixFormingFinish = OnRSMatrixFormingFinish;
			this.eFileCodec.OnUpdateFileStreamsOpeningProgress = OnUpdateFileStreamsOpeningProgress;
			this.eFileCodec.OnFileStreamsOpeningFinish = OnFileStreamsOpeningFinish;
			this.eFileCodec.OnStartedRSCoding = OnStartedRSCoding;
			this.eFileCodec.OnUpdateFileCodingProgress = OnUpdateFileCodingProgress;
			this.eFileCodec.OnFileCodingFinish = OnFileCodingFinish;
			this.eFileCodec.OnUpdateFileStreamsClosingProgress = OnUpdateFileStreamsClosingProgress;
			this.eFileCodec.OnFileStreamsClosingFinish = OnFileStreamsClosingFinish;

			this.eFileAnalyzer.OnUpdateFileAnalyzeProgress = OnUpdateFileAnalyzeProgress;
			this.eFileAnalyzer.OnFileAnalyzeFinish = OnFileAnalyzeFinish;
			this.eFileAnalyzer.OnGetDamageStat = OnGetDamageStat;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.wakeUpEvent[0].Reset();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������������� ���� �� ������������ ������ � ���������� ������
				Recover();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� �������������� ������...
			this.thrRecoveryStarCore = new Thread(new ThreadStart(Recover));

			//...����� ���� ��� ���...
			this.thrRecoveryStarCore.Name = "RecoveryStarCore.Recover()";

			//...������������� ��������� ��������� ������...
			this.thrRecoveryStarCore.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrRecoveryStarCore.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// �������������� ����������������� ������ ������
		/// </summary>
		/// <param name="fullFileName">������ ��� ����� ��� ��������������</param>
		/// <param name="fastExtraction">������������ ������� ���������� �� ����� (��� �������� CRC-64)?</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToRepair(String fullFileName, bool fastExtraction, bool runAsSeparateThread)
		{
			// ���� ����� ����������� ����� �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			// ���� ��� ����� �� �����������
			if(
				(fullFileName == null)
				||
				(fullFileName == "")
				)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ���������� ��������� ���� �� ������� ����� �����
			this.path = this.eFileNamer.GetPath(fullFileName);

			// ���������� ��������� ����� �� ������� ����� �����
			this.fileName = this.eFileNamer.GetShortFileName(fullFileName);

			// ���� �������� ���� �� ����������, �������� �� ������
			if(!File.Exists(this.path + this.fileName))
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ������������� �������� ��� ����� �� ����������� �������,
			// � � ���������� �������� ��������� "fileName", "dataCount", "eccCount", "codecType"
			if(!this.eFileNamer.Unpack(ref this.fileName, ref this.dataCount, ref this.eccCount, ref this.codecType))
			{
				return false;
			}

			// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
			this.fastExtraction = fastExtraction;

			// ������������� �� ���������
			this.eFileCodec.OnUpdateRSMatrixFormingProgress = OnUpdateRSMatrixFormingProgress;
			this.eFileCodec.OnRSMatrixFormingFinish = OnRSMatrixFormingFinish;
			this.eFileCodec.OnUpdateFileStreamsOpeningProgress = OnUpdateFileStreamsOpeningProgress;
			this.eFileCodec.OnFileStreamsOpeningFinish = OnFileStreamsOpeningFinish;
			this.eFileCodec.OnStartedRSCoding = OnStartedRSCoding;
			this.eFileCodec.OnUpdateFileCodingProgress = OnUpdateFileCodingProgress;
			this.eFileCodec.OnFileCodingFinish = OnFileCodingFinish;
			this.eFileCodec.OnUpdateFileStreamsClosingProgress = OnUpdateFileStreamsClosingProgress;
			this.eFileCodec.OnFileStreamsClosingFinish = OnFileStreamsClosingFinish;

			this.eFileAnalyzer.OnUpdateFileAnalyzeProgress = OnUpdateFileAnalyzeProgress;
			this.eFileAnalyzer.OnFileAnalyzeFinish = OnFileAnalyzeFinish;
			this.eFileAnalyzer.OnGetDamageStat = OnGetDamageStat;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.wakeUpEvent[0].Reset();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������������� ���� �� ������������ ������ � ���������� ������
				Repair();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� �������������� ������...
			this.thrRecoveryStarCore = new Thread(new ThreadStart(Repair));

			//...����� ���� ��� ���...
			this.thrRecoveryStarCore.Name = "RecoveryStarCore.Repair()";

			//...������������� ��������� ��������� ������...
			this.thrRecoveryStarCore.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrRecoveryStarCore.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// ������������ ����������������� ������ ������
		/// </summary>
		/// <param name="fullFileName">������ ��� ����� ��� ������������</param>
		/// <param name="fastExtraction">������������ ������� ���������� �� ����� (��� �������� CRC-64)?</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToTest(String fullFileName, bool fastExtraction, bool runAsSeparateThread)
		{
			// ���� ����� ����������� ����� �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���������� ���� ������������ ���������� ����� �������� ������
			this.processedOK = false;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			// ���� ��� ����� �� �����������
			if(
				(fullFileName == null)
				||
				(fullFileName == "")
				)
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ���������� ��������� ���� �� ������� ����� �����
			this.path = this.eFileNamer.GetPath(fullFileName);

			// ���������� ��������� ����� �� ������� ����� �����
			this.fileName = this.eFileNamer.GetShortFileName(fullFileName);

			// ���� �������� ���� �� ����������, �������� �� ������
			if(!File.Exists(this.path + this.fileName))
			{
				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return false;
			}

			// ������������� �������� ��� ����� �� ����������� �������,
			// � � ���������� �������� ��������� "fileName", "dataCount", "eccCount", "codecType"
			if(!this.eFileNamer.Unpack(ref this.fileName, ref this.dataCount, ref this.eccCount, ref this.codecType))
			{
				return false;
			}

			// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
			this.fastExtraction = fastExtraction;

			// ������������� �� ���������
			this.eFileAnalyzer.OnUpdateFileAnalyzeProgress = OnUpdateFileAnalyzeProgress;
			this.eFileAnalyzer.OnFileAnalyzeFinish = OnFileAnalyzeFinish;
			this.eFileAnalyzer.OnGetDamageStat = OnGetDamageStat;

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.wakeUpEvent[0].Reset();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������������� ���� �� ������������ ������ � ���������� ������
				Test();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� �������������� ������...
			this.thrRecoveryStarCore = new Thread(new ThreadStart(Test));

			//...����� ���� ��� ���...
			this.thrRecoveryStarCore.Name = "RecoveryStarCore.Test()";

			//...������������� ��������� ��������� ������...
			this.thrRecoveryStarCore.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrRecoveryStarCore.Start();

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
		/// ���������������� ����������� ����� �� ���� RAID
		/// </summary>
		private void Protect()
		{
			// ��������� �������� ���� �� ���������
			if(this.eFileSplitter.StartToSplit(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, true))
			{
				// ���� �������� ���������� ����� ��������� ��������� ����� �� ����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileSplitter.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileSplitter.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileSplitter.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileSplitter.Stop();

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
				if(!this.eFileSplitter.Finished)
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
			if(!this.eFileSplitter.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ������� ���� ��� ��������������
			if(this.eFileCodec.StartToEncode(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, true))
			{
				// ���� �������� ���������� ����� ����������� �����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileCodec.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileCodec.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileCodec.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileCodec.Stop();

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
				if(!this.eFileCodec.Finished)
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
			if(!this.eFileCodec.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ������������ ���������� �������� ����������� CRC-64 ��� ����� ������ �����
			if(this.eFileAnalyzer.StartToWriteCRC64(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, true))
			{
				// ���� �������� ���������� �������� ������� �������� ����������� �����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileAnalyzer.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileAnalyzer.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileAnalyzer.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileAnalyzer.Stop();

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
				if(!this.eFileAnalyzer.Finished)
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
			if(!this.eFileAnalyzer.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ��������� �� ��, ��� ��������� ���� ����������� ���������
			this.processedOK = true;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		/// <summary>
		/// ������������� ������������������ ������
		/// </summary>
		private void Recover()
		{
			// ������ �����, ������������ ��� ��������������
			int[] volList;

			// ������������ �������� �������� ����������� CRC-64 ��� ����� ������ �����
			if(this.eFileAnalyzer.StartToAnalyzeCRC64(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, this.fastExtraction, true))
			{
				// ���� �������� ���������� �������� ������� �������� ����������� �����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileAnalyzer.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileAnalyzer.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileAnalyzer.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileAnalyzer.Stop();

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
				if(!this.eFileAnalyzer.Finished)
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
			if(!this.eFileAnalyzer.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ������, ����� ��������� ���������, ���������� ����������������
			// ���������� ������ "volList"
			volList = this.eFileAnalyzer.VolList;

			// ���������� ������������, ��� �������������� ������ �� �����������
			bool needToRecover = false;

			// ��������� ������ �� ������� � ��� ����� ��� ��������������
			for(int dataNum = 0; dataNum < this.dataCount; ++dataNum)
			{
				// ���� ���������� ��� ��� ��������������, �����
				// �������� ����� ���������� � ��������� ���������� "FileCodec"
				if(volList[dataNum] != dataNum)
				{
					needToRecover = true;

					break;
				}
			}

			// ���� ��������� �������������� �������� �����, ��������� ���
			if(needToRecover)
			{
				// ��������������� ��������� �������� ����
				if(this.eFileCodec.StartToDecode(this.path, this.fileName, this.dataCount, this.eccCount, volList, this.codecType, true))
				{
					// ���� �������� ���������� ����� ������������� �����
					while(true)
					{
						// ���� �� ���������� �������������� ������� "executeEvent",
						// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
						if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
						{
							//...���������������� ������ ��������������� ���������...
							this.eFileCodec.Pause();

							//...� ���� ��������
							ManualResetEvent.WaitAll(this.executeEvent);

							// � ����� ����������, ���������, ��� ��������� ������ ������������
							this.eFileCodec.Continue();
						}

						// ���� ����� �� ������������� �������...
						int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileCodec.FinishedEvent[0]});

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
							///...������������� �������������� ��������
							this.eFileCodec.Stop();

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
					if(!this.eFileCodec.Finished)
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
				if(!this.eFileCodec.ProcessedOK)
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

			// ��������� �������� ���� �� ��������������� �������� �����
			if(this.eFileSplitter.StartToGlue(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, true))
			{
				// ���� �������� ���������� ����� ���������� ��������� ����� �� �����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileSplitter.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileSplitter.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileSplitter.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileSplitter.Stop();

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
				if(!this.eFileSplitter.Finished)
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
			if(!this.eFileSplitter.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ��������� �� ��, ��� ��������� ���� ����������� ���������
			this.processedOK = true;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		/// <summary>
		/// "�������" ������ ������
		/// </summary>
		private void Repair()
		{
			// ������ �����, ������������ ��� ��������������
			int[] volList;

			// ������������ �������� �������� ����������� CRC-64 ��� ����� ������ �����
			if(this.eFileAnalyzer.StartToAnalyzeCRC64(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, this.fastExtraction, true))
			{
				// ���� �������� ���������� �������� ������� �������� ����������� �����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileAnalyzer.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileAnalyzer.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileAnalyzer.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileAnalyzer.Stop();

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
				if(!this.eFileAnalyzer.Finished)
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
			if(!this.eFileAnalyzer.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ������, ����� ��������� ���������, ���������� ����������������
			// ���������� ������ "volList"
			volList = this.eFileAnalyzer.VolList;

			// ���������� ������������, ��� �������������� ������ �� �����������
			bool needToRecover = false;

			// ��������� ������ �� ������� � ��� ����� ��� ��������������
			for(int dataNum = 0; dataNum < this.dataCount; ++dataNum)
			{
				// ���� ���������� ��� ��� ��������������, �����
				// �������� ����� ���������� � ��������� ���������� "FileCodec"
				if(volList[dataNum] != dataNum)
				{
					needToRecover = true;

					break;
				}
			}

			// ���� ��������� �������������� �������� �����, ��������� ���
			if(needToRecover)
			{
				// ��������������� ��������� �������� ����
				if(this.eFileCodec.StartToDecode(this.path, this.fileName, this.dataCount, this.eccCount, volList, this.codecType, true))
				{
					// ���� �������� ���������� ����� ������������� �����
					while(true)
					{
						// ���� �� ���������� �������������� ������� "executeEvent",
						// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
						if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
						{
							//...���������������� ������ ��������������� ���������...
							this.eFileCodec.Pause();

							//...� ���� ��������
							ManualResetEvent.WaitAll(this.executeEvent);

							// � ����� ����������, ���������, ��� ��������� ������ ������������
							this.eFileCodec.Continue();
						}

						// ���� ����� �� ������������� �������...
						int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileCodec.FinishedEvent[0]});

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
							///...������������� �������������� ��������
							this.eFileCodec.Stop();

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
					if(!this.eFileCodec.Finished)
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
				if(!this.eFileCodec.ProcessedOK)
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

			// �������� ����� (��������� ��� ����, ����� ��������� ������ �� ������ ������)
			FileStream eFileStream = null;

			try
			{
				// ��� ����� ��� ���������
				String fileName;

				// ������������ ��� �����
				for(int i = 0; i < (this.dataCount + this.eccCount); i++)
				{
					// ��������� �������������� ��� �����,...
					fileName = this.fileName;

					//...����������� ��� � ���������� ������...
					this.eFileNamer.Pack(ref fileName, i, this.dataCount, this.eccCount, this.codecType);

					//...��������� ������ ��� �����...
					fileName = this.path + fileName;

					//...���������� ���� �� ������� �����...
					if(File.Exists(fileName))
					{
						//...���� ������� �������, ������ �� ���� ��������
						// ��-���������
						File.SetAttributes(fileName, FileAttributes.Normal);

						//...��������� �������� ����� �� ������...
						eFileStream = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Write);

						if(eFileStream != null)
						{
							//...����������� ��� ����� �� 8 ���� (������ CRC-64)...
							eFileStream.SetLength(eFileStream.Length - 8);

							//...������� �������� �����...
							eFileStream.Flush();

							//...� ��������� ����
							eFileStream.Close();

							// ���� ������� ����� - ����������� ��� null, ����� � ������
							// �������������� �������� ��������� ������������ ���������� ������
							eFileStream = null;
						}
					}
				}
			}

				// ���� ���� ���� �� ���� ���������� - ��������� �������� ����� �
				// �������� �� ������
			catch
			{
				// ��������� �������� �����
				if(eFileStream != null)
				{
					eFileStream.Close();
					eFileStream = null;
				}

				// ��������� �� ��, ��� ������� "�������" ������ ������ ������ �����������
				this.processedOK = false;

				// ������������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				return;
			}

			// ���� � ���������� ������� ������ ����� ���� �����������, ���
			// ��� ���� ��� �������������� �������� ���������������,
			// ��� ����������� � �� ��������� ��������
			if(!this.eFileAnalyzer.AllEccVolsOK)
			{
				// ������� ���� ��� ��������������
				if(this.eFileCodec.StartToEncode(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, true))
				{
					// ���� �������� ���������� ����� ����������� �����
					while(true)
					{
						// ���� �� ���������� �������������� ������� "executeEvent",
						// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
						if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
						{
							//...���������������� ������ ��������������� ���������...
							this.eFileCodec.Pause();

							//...� ���� ��������
							ManualResetEvent.WaitAll(this.executeEvent);

							// � ����� ����������, ���������, ��� ��������� ������ ������������
							this.eFileCodec.Continue();
						}

						// ���� ����� �� ������������� �������...
						int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileCodec.FinishedEvent[0]});

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
							///...������������� �������������� ��������
							this.eFileCodec.Stop();

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
					if(!this.eFileCodec.Finished)
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
				if(!this.eFileCodec.ProcessedOK)
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

			// ������������ ���������� �������� ����������� CRC-64 ��� ����� ������ �����
			if(this.eFileAnalyzer.StartToWriteCRC64(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, true))
			{
				// ���� �������� ���������� �������� ������� �������� ����������� �����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileAnalyzer.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileAnalyzer.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileAnalyzer.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileAnalyzer.Stop();

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
				if(!this.eFileAnalyzer.Finished)
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
			if(!this.eFileAnalyzer.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ��������� �� ��, ��� ��������� ���� ����������� ���������
			this.processedOK = true;

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		/// <summary>
		/// ������������ ������ ������
		/// </summary>
		private void Test()
		{
			// ������������ �������� �������� ����������� CRC-64 ��� ����� ������ �����
			if(this.eFileAnalyzer.StartToAnalyzeCRC64(this.path, this.fileName, this.dataCount, this.eccCount, this.codecType, this.fastExtraction, true))
			{
				// ���� �������� ���������� �������� ������� �������� ����������� �����
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eFileAnalyzer.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eFileAnalyzer.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eFileAnalyzer.FinishedEvent[0]});

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
						///...������������� �������������� ��������
						this.eFileAnalyzer.Stop();

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
				if(!this.eFileAnalyzer.Finished)
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
			if(!this.eFileAnalyzer.ProcessedOK)
			{
				// ��������� �� ��, ��� ��������� ���� ��������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
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