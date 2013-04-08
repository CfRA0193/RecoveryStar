/*----------------------------------------------------------------------+
 |  filename:   FileCodec.cs                                            |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ����������� ��������� ������ � RAID-�������� �����      |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;
using System.IO;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ��� ����������� ������ � RAID-�������� �����
	/// </summary>
	public class FileCodec
	{
		#region Delegates

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

		#endregion Delegates

		#region Constants

		/// <summary>
		/// ����������� ����� ������ ����������� - 64 ��
		/// </summary>
		private const int minTotalBufferSize = 1 << 26;

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
					(this.thrFileCodec != null)
					&&
					(
						(this.thrFileCodec.ThreadState == ThreadState.Running)
						||
						(this.thrFileCodec.ThreadState == ThreadState.WaitSleepJoin)
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
		/// ������ ������ ��������� ������ (��������������� ����� ��������)
		/// </summary>
		public int TotalBufferSize
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� ��������...
				if(!InProcessing)
				{
					return this.maxTotalBufferSize;
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
					//... �� ������ ���� ��� �� �������� ����������� ������ ������ - 64 ��
					if(value > minTotalBufferSize)
					{
						this.maxTotalBufferSize = value;
					}
					else
					{
						this.maxTotalBufferSize = minTotalBufferSize;
					}
				}
			}
		}

		/// <summary>
		/// ������������ ��������� ������� ������?
		/// </summary>
		public bool AutoBuffering
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� ��������...
				if(!InProcessing)
				{
					return this.autoBuffering;
				}
				else
				{
					//...� ����� �������� �� ��������
					return false;
				}
			}

			set
			{
				// ���� ����� �� ����� ���������� - ������������� ��������...
				if(!InProcessing)
				{
					this.autoBuffering = value;
				}
			}
		}

		/// <summary>
		/// ����������� ���������� ������ ��� ���������������
		/// </summary>
		public double MemConsumeCoeff
		{
			get
			{
				// ���� ����� �� ����� ���������� - ���������� ��������...
				if(!InProcessing)
				{
					return this.memConsumeCoeff;
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
					if(
						(value >= 0.1)
						&&
						(value <= 1)
						)
					{
						this.memConsumeCoeff = value;
					}
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
					(this.thrFileCodec != null)
					&&
					(this.thrFileCodec.IsAlive)
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
					this.thrFileCodec.Priority = this.threadPriority;

					if(this.eRSRaidEncoder != null)
					{
						this.eRSRaidEncoder.ThreadPriority = value;
					}

					if(this.eRSRaidDecoder != null)
					{
						this.eRSRaidDecoder.ThreadPriority = value;
					}
				}
			}
		}

		/// <summary>
		/// �������, ��������������� �� ���������� ���������
		/// </summary>
		public ManualResetEvent[] FinishedEvent
		{
			get { return this.finishedEvent; }
		}

		#endregion Public Properties & Data

		#region Data

		/// <summary>
		/// ������ ��� �������� (����������) ����� � ���������� ������
		/// </summary>
		private FileNamer eFileNamer;

		/// <summary>
		/// RAID-�������� ����� ����-��������
		/// </summary>
		private RSRaidEncoder eRSRaidEncoder;

		/// <summary>
		/// RAID-�������� ������� ����-��������
		/// </summary>
		private RSRaidDecoder eRSRaidDecoder;

		/// <summary>
		/// ���������� ���� �����
		/// </summary>
		private GF16 eGF16;

		/// <summary>
		/// ������ ������ ��������� ������ (�� ��� ������)
		/// </summary>
		private int maxTotalBufferSize;

		/// <summary>
		/// ������������ ��������� ������� ������?
		/// </summary>
		private bool autoBuffering;

		/// <summary>
		/// ����������� ���������� ������ ��� ���������������
		/// </summary>
		private double memConsumeCoeff;

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
		/// ������, ����������� �� ������ �����
		/// </summary>
		private int[] volList;

		/// <summary>
		/// �������� ������ ��������� �������� ���������?
		/// </summary>
		private bool finished;

		/// <summary>
		/// ��������� ����������� ���������?
		/// </summary>
		private bool processedOK;

		/// <summary>
		/// ����� ����������� ������
		/// </summary>
		private Thread thrFileCodec;

		/// <summary>
		/// ��������� �������� ��������� (����������) �����
		/// </summary>
		private ThreadPriority threadPriority;

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

		/// <summary>
		/// �������, ��������������� �� ���������� ���������
		/// </summary>
		private ManualResetEvent[] finishedEvent;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� ������
		/// </summary>
		public FileCodec()
		{
			// �������������� ��������� ������ ��� �������� (����������) ����� �����
			// � ���������� ������
			this.eFileNamer = new FileNamer();

			// ���� � ������ ��� ��������� ��-��������� ������
			this.path = "";

			// �������������� ��� ����� ��-���������
			this.fileName = "NONAME";

			// ������� ��������� ��� ��������� ��������� ����������
			SystemInfo eSystemInfo = new SystemInfo();

			// ������� ����� ���������� ���� �����
			this.eGF16 = new GF16();

			// ������ ��������� ������ ��� ���� ������� ��� ����������������
			// ������ ��-��������� ���������� 1 / 8 ����������� ������ ������
			TotalBufferSize = (int)(eSystemInfo.TotalPhysicalMemory / 8);
			TotalBufferSize = (TotalBufferSize < 0) ? int.MaxValue : TotalBufferSize;

			// ��-��������� ������������� �������������� ����������� �� ������
			this.autoBuffering = true;

			// ����������� ���������� ������ ��� ��������������� ��-��������� ���������� 0.5
			this.memConsumeCoeff = 0.5;

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
		/// ���������� ���������� ��� �������������� �������� �����
		/// </summary>
		/// <param name="path">���� � ������ ��� ���������</param>
		/// <param name="fileName">��� �����, �������� ����������� ��������� �����</param>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToEncode(String path, String fileName, int dataCount, int eccCount, int codecType, bool runAsSeparateThread)
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
			this.wakeUpEvent[0].Reset();
			this.finishedEvent[0].Reset();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// �������� ����� ����� � ���������� ����� ��� ��������������
				Encode();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� ����������� ������...
			this.thrFileCodec = new Thread(new ThreadStart(Encode));

			//...����� ���� ��� ���...
			this.thrFileCodec.Name = "FileCodec.Encode()";

			//...������������� ��������� ��������� ������...
			this.thrFileCodec.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileCodec.Start();

			// ��������, ��� ��� ���������
			return true;
		}

		/// <summary>
		/// �������������� ��������� �������� �����
		/// </summary>
		/// <param name="path">���� � ������ ��� ���������</param>
		/// <param name="fileName">��� �����, �������� ����������� ��������� �����</param>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="volList">������ ������� ��������� �����</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool StartToDecode(String path, String fileName, int dataCount, int eccCount, int[] volList, int codecType, bool runAsSeparateThread)
		{
			// ���� ����� ������������� ����� �������� - �� ��������� ��������� ������
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

			// ��������� ������ ������� ��������� �����
			this.volList = volList;

			// ��������� ��� ������ ����-��������
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
				// ���������� ������������������ ������ � ��������������� �������� �����
				Decode();

				// ���������� ��������� ���������
				return this.processedOK;
			}

			// ������� ����� �������������� �������� �����...
			this.thrFileCodec = new Thread(new ThreadStart(Decode));

			//...����� ���� ��� ���...
			this.thrFileCodec.Name = "FileCodec.Decode()";

			//...������������� ��������� ��������� ������...
			this.thrFileCodec.Priority = this.threadPriority;

			//...� ��������� ���
			this.thrFileCodec.Start();

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
		/// ����������� ������������������ ������
		/// </summary>
		private void Encode()
		{
			// ������� RAID-�������� ����� ����-��������
			if(this.eRSRaidEncoder == null)
			{
				this.eRSRaidEncoder = new RSRaidEncoder(this.dataCount, this.eccCount, this.codecType);
			}
			else
			{
				this.eRSRaidEncoder.SetConfig(this.dataCount, this.eccCount, this.codecType);
			}

			// ������������� �� ���������
			this.eRSRaidEncoder.OnUpdateRSMatrixFormingProgress = OnUpdateRSMatrixFormingProgress;
			this.eRSRaidEncoder.OnRSMatrixFormingFinish = OnRSMatrixFormingFinish;

			// ��������� ���������� RAID-��������� ������ ����-��������
			if(this.eRSRaidEncoder.Prepare(true))
			{
				// ���� �������� ���������� ���������� ������ ����-�������� � ������
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eRSRaidEncoder.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eRSRaidEncoder.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eRSRaidEncoder.FinishedEvent[0]});

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
						this.eRSRaidEncoder.Stop();

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

			// ����� ����� ��� �� ��������, ������������� �� ��������� ��������,
			// ��������, ��� "�� ����������"
			for(int i = 0; i < (int)WaitCount.MaxWaitCount; i++)
			{
				if(!this.eRSRaidEncoder.Finished)
				{
					Thread.Sleep((int)WaitTime.MinWaitTime);
				}
				else
				{
					break;
				}
			}

			// ���� ����� �� ����������������� ��������� - �������...
			if(!this.eRSRaidEncoder.ConfigIsOK)
			{
				//...�������� �� ������
				this.processedOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// �������� ������ ��� ������� ������� � �������� ������ ������
			int[] sourceLog = new int[this.dataCount];
			int[] target = new int[this.eccCount];

			// ����� ��� ������ � ������ ����
			byte[] wordBuff = new byte[2];

			// �������� ������ ��� ������� �������� �������
			BufferedStream[] fileStreamSourceArr = new BufferedStream[this.dataCount];
			BufferedStream[] fileStreamTargetArr = new BufferedStream[this.eccCount];

			// ������� ������ ������ (����� ���������� ��������� ��-���������, ����
			// ������������ ���������, ������ �� ���������� ������ ���������� ������)
			int currentTotalBufferSize = -1;

			try
			{
				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ���������� ��� ����� �� "volNum"
				int progressMod1 = (this.dataCount + this.eccCount) / 100;

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod1 == 0)
				{
					progressMod1 = 1;
				}

				// ��� ����� ��� ���������
				String fileName;

				// ����� �������� ����
				int volNum;

				// ������� ��������� ��� ��������� ��������� ����������
				SystemInfo eSystemInfo = new SystemInfo();

				// �������� ������������ ������ ������ ��� �������� �������...
				if(this.autoBuffering)
				{
					//...����� ����� ��������� ���������� ������...
					currentTotalBufferSize = (int)(this.memConsumeCoeff * (double)eSystemInfo.FreePhysicalMemory);
					currentTotalBufferSize = (currentTotalBufferSize < 0) ? int.MaxValue : currentTotalBufferSize;
				}
				else
				{
					//...���� ���������� ������� ����� ��������
					currentTotalBufferSize = this.TotalBufferSize;
				}

				// ������ ����� ��������, ����� �� ����� ������� ����� ��� ������ �
				// ������ ������� �����. ��� ����� ������� ����� �� �����, � ����� ��� ������
				// �������� ����� ��������� ����� �� ������

				// ��������� �������������� ��� �����,...
				fileName = this.fileName;

				//...����������� ��� � ���������� ������...
				this.eFileNamer.Pack(ref fileName, 0, this.dataCount, this.eccCount, this.codecType);

				//...��������� ������ ��� �����...
				fileName = this.path + fileName;

				//...� ����� ��������� �������� �����, ����� ������ ������ ������� ����
				FileStream eFileStream = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read);

				// ������ ��������� ������������ ����� ������ ������
				long totalBufferSizeNeeded = eFileStream.Length * (this.dataCount + this.eccCount);

				// ��������� �������� �����
				eFileStream.Close();

				// ���� ��������� ������, ��� ��������������, ������������ ������ ��������
				if(totalBufferSizeNeeded < currentTotalBufferSize)
				{
					currentTotalBufferSize = (int)totalBufferSizeNeeded;
				}

				// ��������� ������ ������ �� ���
				int currentVolumeBufferSize = currentTotalBufferSize / (this.dataCount + this.eccCount);

				// �������������� ������� �������� ������� �������� �����
				for(volNum = 0; volNum < this.dataCount; volNum++)
				{
					// ��������� �������������� ��� �����,...
					fileName = this.fileName;

					//...����������� ��� � ���������� ������...
					this.eFileNamer.Pack(ref fileName, volNum, this.dataCount, this.eccCount, this.codecType);

					//...��������� ������ ��� �����...
					fileName = this.path + fileName;

					//...� ��������� �� ��� ������ ������� �������� �����
					fileStreamSourceArr[volNum] = new BufferedStream(new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read), currentVolumeBufferSize);

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((volNum % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsOpeningProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsOpeningProgress(((double)(volNum + 1) / (double)(this.dataCount + this.eccCount)) * 100);
					}

					// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
					// ����� ��������, � ����� �� ����� ������ �� ��� ���������
					ManualResetEvent.WaitAll(this.executeEvent);

					// ���� �������, ��� ��������� ����� �� ������ - �������
					if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
					{
						// ���������, ��� ��������� ����������� �����������
						this.processedOK = false;

						// ���������� ��������� ����������� ��������� ����������-������
						this.finished = true;

						// ������������� ������� ���������� ���������
						this.finishedEvent[0].Set();

						return;
					}
				}

				// �������������� ������� �������� ������� ����� ��� ��������������
				for(int eccNum = 0; volNum < (this.dataCount + this.eccCount); volNum++, eccNum++)
				{
					// ��������� �������������� ��� �����...
					fileName = this.fileName;

					//...����������� ��� � ���������� ������...
					this.eFileNamer.Pack(ref fileName, volNum, this.dataCount, this.eccCount, this.codecType);

					//...��������� ������ ��� �����...
					fileName = this.path + fileName;

					// ...����� ���������� ���� �� ������� �����...
					if(File.Exists(fileName))
					{
						//...���� ������� �������, ������ �� ���� ��������
						// ��-���������...
						File.SetAttributes(fileName, FileAttributes.Normal);
					}

					//...� ��������� �� ��� ������ �������� �������� �����
					fileStreamTargetArr[eccNum] = new BufferedStream(new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write), (currentTotalBufferSize / (this.dataCount + this.eccCount)));

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((volNum % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsOpeningProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsOpeningProgress(((double)(volNum + 1) / (double)(this.dataCount + this.eccCount)) * 100);
					}

					// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
					// ����� ��������, � ����� �� ����� ������ �� ��� ���������
					ManualResetEvent.WaitAll(this.executeEvent);

					// ���� �������, ��� ��������� ����� �� ������ - �������
					if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
					{
						// ���������, ��� ��������� ����������� �����������
						this.processedOK = false;

						// ���������� ��������� ����������� ��������� ����������-������
						this.finished = true;

						// ������������� ������� ���������� ���������
						this.finishedEvent[0].Set();

						return;
					}
				}

				// ��������� ������, ��� ��� �������� ������ �������
				if(OnFileStreamsOpeningFinish != null)
				{
					OnFileStreamsOpeningFinish();
				}

				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ����������
				int progressMod2 = (int)(fileStreamSourceArr[0].Length / (2 * 100));

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod2 == 0)
				{
					progressMod2 = 1;
				}

				// ��������� ������������ �� ��, ��� ����������� ��� ��������
				if(OnStartedRSCoding != null)
				{
					OnStartedRSCoding();
				}

				// �������� �� ����� ������� ��� ���� � �������� �������
				for(int i = 0; i < (fileStreamSourceArr[0].Length / 2); i++)
				{
					// ��������� ������ �������� ������ ������ ������� �������� �����
					for(int j = 0; j < this.dataCount; j++)
					{
						// ������ ���� ���� �� �������� ������
                        int dataLen = 2;
						int readed = 0;
						int toRead = 0;
                        while((toRead = dataLen - (readed += fileStreamSourceArr[j].Read(wordBuff, readed, toRead))) != 0) ;

						// ���������� ������� ���� �������� byte � int
						sourceLog[j] = this.eGF16.Log((int)(((uint)(wordBuff[0] << 0) & 0x00FF)
						                                    |
						                                    ((uint)(wordBuff[1] << 8) & 0xFF00)));
					}

					// �������� ������ (�������� ���� ��� ��������������)
					this.eRSRaidEncoder.Process(sourceLog, target);

					// ������� � ����� ������ ���������� ������ (ecc)
					for(int j = 0; j < this.eccCount; j++)
					{
						// ���������� ���������� ������ �������� �� ��� (int16 �� ��� byte)
						wordBuff[0] = (byte)((target[j] >> 0) & 0x00FF);
						wordBuff[1] = (byte)((target[j] >> 8) & 0x00FF);

						// ������ ����� ���� ���� � �������� �����
						fileStreamTargetArr[j].Write(wordBuff, 0, 2);
					}

					// ������� �������� ��������� ����� ������ �������
					if(
						(i != 0)
						&&
						((i % progressMod2) == 0)
						&&
						(OnUpdateFileCodingProgress != null)
						)
					{
						OnUpdateFileCodingProgress(((double)(i + 1) / (double)fileStreamSourceArr[0].Length) * 200.0);
					}

					// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
					// ����� ��������, � ����� �� ����� ������ �� ��� ���������
					ManualResetEvent.WaitAll(this.executeEvent);

					// ���� �������, ��� ��������� ����� �� ������ - �������
					if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
					{
						// ��������� ������� �������� ������
						for(int j = 0; j < this.dataCount; j++)
						{
							if(fileStreamSourceArr[j] != null)
							{
								fileStreamSourceArr[j].Close();
								fileStreamSourceArr[j] = null;
							}
						}

						// ��������� �������� �������� ������
						for(int j = 0; j < this.eccCount; j++)
						{
							if(fileStreamTargetArr[j] != null)
							{
								fileStreamTargetArr[j].Close();
								fileStreamTargetArr[j] = null;
							}
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

				// ��������, ��� ��������� ������ ���������
				if(OnFileCodingFinish != null)
				{
					OnFileCodingFinish();
				}

				// ���������� ����� ����
				volNum = -1;

				// ��������� ������� �������� ������
				for(int i = 0; i < this.dataCount; i++)
				{
					if(fileStreamSourceArr[i] != null)
					{
						fileStreamSourceArr[i].Close();
						fileStreamSourceArr[i] = null;
					}

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((++volNum % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsClosingProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsClosingProgress(((double)(volNum + 1) / (double)(this.dataCount + this.eccCount)) * 100);
					}
				}

				// ��������� �������� �������� ������
				for(int i = 0; i < this.eccCount; i++)
				{
					if(fileStreamTargetArr[i] != null)
					{
						fileStreamTargetArr[i].Flush();
						fileStreamTargetArr[i].Close();
						fileStreamTargetArr[i] = null;
					}

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((++volNum % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsClosingProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsClosingProgress(((double)(volNum + 1) / (double)(this.dataCount + this.eccCount)) * 100);
					}
				}

				// ��������, ��� �������� �������� ������� �����������
				if(OnFileStreamsClosingFinish != null)
				{
					OnFileStreamsClosingFinish();
				}
			}

			// ���� ���� ���� �� ���� ���������� - ��������� �������� ����� �
			// �������� �� ������
			catch
			{
				// ��������� ������� �������� ������
				for(int i = 0; i < this.dataCount; i++)
				{
					if(fileStreamSourceArr[i] != null)
					{
						fileStreamSourceArr[i].Close();
						fileStreamSourceArr[i] = null;
					}
				}

				// ��������� �������� �������� ������
				for(int i = 0; i < this.eccCount; i++)
				{
					if(fileStreamTargetArr[i] != null)
					{
						fileStreamTargetArr[i].Close();
						fileStreamTargetArr[i] = null;
					}
				}

				// ��������� �� ��, ��� ��������� ������ ������ � �������
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
		private void Decode()
		{
			// ������ ������������ �������� �����
			int[] damagedVolList = new int[this.dataCount];

			// ������� ���������� ������������ �����
			int damagedVolCount = 0;

			// ������� RAID-�������� ������� ����-��������
			if(this.eRSRaidDecoder == null)
			{
				this.eRSRaidDecoder = new RSRaidDecoder(this.dataCount, this.eccCount, this.volList, this.codecType);
			}
			else
			{
				this.eRSRaidDecoder.SetConfig(this.dataCount, this.eccCount, this.volList, this.codecType);
			}

			// ������������� �� ���������
			this.eRSRaidDecoder.OnUpdateRSMatrixFormingProgress = OnUpdateRSMatrixFormingProgress;
			this.eRSRaidDecoder.OnRSMatrixFormingFinish = OnRSMatrixFormingFinish;

			// ��������� ���������� RAID-��������� �������� ����-��������
			if(this.eRSRaidDecoder.Prepare(true))
			{
				// ���� �������� ���������� ���������� �������� ����-�������� � ������
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eRSRaidDecoder.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eRSRaidDecoder.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eRSRaidDecoder.FinishedEvent[0]});

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
						this.eRSRaidDecoder.Stop();

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

				return;
			}

			// ����� ����� ��� �� ��������, ������������� �� ��������� ��������,
			// ��������, ��� "�� ����������"
			for(int i = 0; i < (int)WaitCount.MaxWaitCount; i++)
			{
				if(!this.eRSRaidDecoder.Finished)
				{
					Thread.Sleep((int)WaitTime.MinWaitTime);
				}
				else
				{
					break;
				}
			}

			// �������� ������ ��� ������� ������� � �������� ������ ������
			int[] sourceLog = new int[this.dataCount];
			int[] target = new int[this.dataCount];

			// ����� ��� ������ � ������ ����
			byte[] wordBuff = new byte[2];

			// �������� ������ ��� ������� �������� �������
			BufferedStream[] fileStreamSourceArr = new BufferedStream[this.dataCount];
			BufferedStream[] fileStreamTargetArr = new BufferedStream[this.dataCount];

			// ������� ������ ������ (����� ���������� ��������� ��-���������, ����
			// ������������ ���������, ������ �� ���������� ������ ���������� ������)
			int currentTotalBufferSize = -1;

			try
			{
				// ����������, ����� �� �������� ����� (�� ������ "volList") ����������,
				// � ����� - ���
				for(int i = 0; i < this.volList.Length; i++)
				{
					// ��������� ����� �������� ����
					int currVol = Math.Abs(this.volList[i]);

					// ���� ������ ��� �� �������� ��������...
					if(currVol >= this.dataCount)
					{
						//...���������, �� ������ ����
						damagedVolList[damagedVolCount++] = i;
					}
				}

				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ���������� ��� ����� �� "volCount"
				int progressMod1 = (this.dataCount + damagedVolCount) / 100;

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod1 == 0)
				{
					progressMod1 = 1;
				}

				// ������� �������� �������� �������
				int volCount = -1;

				// ��� ����� ��� ���������
				String fileName;

				// ������� ��������� ��� ��������� ��������� ����������
				SystemInfo eSystemInfo = new SystemInfo();

				// �������� ������������ ������ ������ ��� �������� �������...
				if(this.autoBuffering)
				{
					//...����� ����� ��������� ���������� ������...
					currentTotalBufferSize = (int)(this.memConsumeCoeff * (double)eSystemInfo.FreePhysicalMemory);
					currentTotalBufferSize = (currentTotalBufferSize < 0) ? int.MaxValue : currentTotalBufferSize;
				}
				else
				{
					//...���� ���������� ������� ����� ��������
					currentTotalBufferSize = this.TotalBufferSize;
				}

				// ������ ����� ��������, ����� �� ����� ������� ����� ��� ������ �
				// ������ ������� �����. ��� ����� ������� ����� �� �����, � ����� ��� ������
				// �������� ����� ��������� ����� �� ������

				// ��������� �������������� ��� �����,...
				fileName = this.fileName;

				//...����������� ��� � ���������� ������...
				this.eFileNamer.Pack(ref fileName, this.volList[0], this.dataCount, this.eccCount, this.codecType);

				//...��������� ������ ��� �����...
				fileName = this.path + fileName;

				//...� ����� ��������� �������� �����, ����� ������ ������ ������� ����
				FileStream eFileStream = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read);

				// ������ ��������� ������������ ����� ������ ������
				long totalBufferSizeNeeded = eFileStream.Length * (this.dataCount + damagedVolCount);

				// ��������� �������� �����
				eFileStream.Close();

				// ���� ��������� ������, ��� ��������������, ������������ ������ ��������
				if(totalBufferSizeNeeded < currentTotalBufferSize)
				{
					currentTotalBufferSize = (int)totalBufferSizeNeeded;
				}

				// ��������� ������ ������ �� ���
				int currentVolumeBufferSize = currentTotalBufferSize / (this.dataCount + damagedVolCount);

				// ��������� ������� �������� ������
				for(int i = 0; i < this.dataCount; i++)
				{
					// ��������� �������������� ��� �����,...
					fileName = this.fileName;

					//...����������� ��� � ���������� ������...
					this.eFileNamer.Pack(ref fileName, this.volList[i], this.dataCount, this.eccCount, this.codecType);

					//...��������� ������ ��� �����...
					fileName = this.path + fileName;

					//...���������� ���� �� ������� �����...
					if(!File.Exists(fileName))
					{
						// ��������� �� ��, ��� ��������� ������ ������ � �������
						this.processedOK = false;

						// ���������� ��������� ����������� ��������� ����������-������
						this.finished = true;

						// ������������� ������� ���������� ���������
						this.finishedEvent[0].Set();

						return;
					}

					//...� ��������� �� ��� ������ ������� �������� �����
					fileStreamSourceArr[i] = new BufferedStream(new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read), currentVolumeBufferSize);

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((++volCount % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsOpeningProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsOpeningProgress(((double)(volCount + 1) / (double)(this.dataCount + damagedVolCount)) * 100);
					}

					// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
					// ����� ��������, � ����� �� ����� ������ �� ��� ���������
					ManualResetEvent.WaitAll(this.executeEvent);

					// ���� �������, ��� ��������� ����� �� ������ - �������
					if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
					{
						// ���������, ��� ��������� ����������� �����������
						this.processedOK = false;

						// ���������� ��������� ����������� ��������� ����������-������
						this.finished = true;

						// ������������� ������� ���������� ���������
						this.finishedEvent[0].Set();

						return;
					}
				}

				// ��������� �������� �������� ������ ��� ������������ ������
				for(int i = 0; i < damagedVolCount; i++)
				{
					// ��������� �������������� ��� �����,...
					fileName = this.fileName;

					//...����������� ��� � ���������� ������...
					this.eFileNamer.Pack(ref fileName, damagedVolList[i], this.dataCount, this.eccCount, this.codecType);

					//...��������� ������ ��� �����...
					fileName = this.path + fileName;

					// ...����� ���������� ���� �� ������� �����...
					if(File.Exists(fileName))
					{
						//...���� ������� �������, ������ �� ���� ��������
						// ��-���������...
						File.SetAttributes(fileName, FileAttributes.Normal);
					}

					//...� ��������� �� ��� ������ �������� �������� �����
					fileStreamTargetArr[damagedVolList[i]] = new BufferedStream(new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write), (currentTotalBufferSize / (this.dataCount + damagedVolCount)));

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((++volCount % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsOpeningProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsOpeningProgress(((double)(volCount + 1) / (double)(this.dataCount + damagedVolCount)) * 100);
					}

					// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
					// ����� ��������, � ����� �� ����� ������ �� ��� ���������
					ManualResetEvent.WaitAll(this.executeEvent);

					// ���� �������, ��� ��������� ����� �� ������ - �������
					if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
					{
						// ���������, ��� ��������� ����������� �����������
						this.processedOK = false;

						// ���������� ��������� ����������� ��������� ����������-������
						this.finished = true;

						// ������������� ������� ���������� ���������
						this.finishedEvent[0].Set();

						return;
					}
				}

				// ��������� ������, ��� ��� �������� ������ �������
				if(OnFileStreamsOpeningFinish != null)
				{
					OnFileStreamsOpeningFinish();
				}

				// ��������� �������� ������, ������� �������� �������� ������� ���������
				// ����� ��� ��������� ����������
				int progressMod2 = (int)(fileStreamSourceArr[0].Length / (2 * 100));

				// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
				// �������� ��������� �� ������ �������� (���� ����� ���������)
				if(progressMod2 == 0)
				{
					progressMod2 = 1;
				}

				// ��������� ������������ �� ��, ��� ����������� ��� ��������
				if(OnStartedRSCoding != null)
				{
					OnStartedRSCoding();
				}

				// �������� �� ����� ������� ��� ���� � �������� �������
				for(int i = 0; i < ((fileStreamSourceArr[0].Length - 8) / 2); i++)
				{
					// ��������� ������ �������� ������ ������ ������� �������� �����
					for(int j = 0; j < this.dataCount; j++)
					{
						// ������ ���� ���� �� �������� ������
						int dataLen = 2;
						int readed = 0;
						int toRead = 0;
						while((toRead = dataLen - (readed += fileStreamSourceArr[j].Read(wordBuff, readed, toRead))) != 0) ;

						// ���������� ������� ���� �������� byte � int
						sourceLog[j] = this.eGF16.Log((int)(((uint)(wordBuff[0] << 0) & 0x00FF)
						                                    |
						                                    ((uint)(wordBuff[1] << 8) & 0xFF00)));
					}

					// ���������� ������ (�������� ������ ���������� ������ �������� �����)
					this.eRSRaidDecoder.Process(sourceLog, target);

					// ������� ���������� �������� ������� �������� ������
					for(int j = 0; j < damagedVolCount; j++)
					{
						// ���������� ���������� ������ �������� �� ��� (int16 �� ��� byte)
						wordBuff[0] = (byte)((target[damagedVolList[j]] >> 0) & 0x00FF);
						wordBuff[1] = (byte)((target[damagedVolList[j]] >> 8) & 0x00FF);

						// ������ ����� ���� ���� � �������� �����
						fileStreamTargetArr[damagedVolList[j]].Write(wordBuff, 0, 2);
					}

					// ������� �������� ��������� ����� ������ �������
					if(
						(i != 0)
						&&
						((i % progressMod2) == 0)
						&&
						(OnUpdateFileCodingProgress != null)
						)
					{
						OnUpdateFileCodingProgress(((double)(i + 1) / (double)fileStreamSourceArr[0].Length) * 200.0);
					}

					// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
					// ����� ��������, � ����� �� ����� ������ �� ��� ���������
					ManualResetEvent.WaitAll(this.executeEvent);

					// ���� �������, ��� ��������� ����� �� ������ - �������
					if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
					{
						// ��������� ������� �������� ������
						for(int j = 0; j < this.dataCount; j++)
						{
							if(fileStreamSourceArr[j] != null)
							{
								fileStreamSourceArr[j].Close();
								fileStreamSourceArr[j] = null;
							}
						}

						// ��������� �������� �������� ������
						for(int j = 0; j < this.eccCount; j++)
						{
							if(fileStreamTargetArr[j] != null)
							{
								fileStreamTargetArr[j].Close();
								fileStreamTargetArr[j] = null;
							}
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

				// ��������, ��� ��������� ������ ���������
				if(OnFileCodingFinish != null)
				{
					OnFileCodingFinish();
				}

				// ���������� ����� ����
				int volNum = -1;

				// ��������� ������� �������� ������
				for(int i = 0; i < this.dataCount; i++)
				{
					if(fileStreamSourceArr[i] != null)
					{
						fileStreamSourceArr[i].Close();
						fileStreamSourceArr[i] = null;
					}

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((++volNum % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsClosingProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsClosingProgress(((double)(volNum + 1) / (double)(this.dataCount + this.eccCount)) * 100);
					}
				}

				// ��������� �������� �������� ������ ���:
				for(int i = 0; i < damagedVolCount; i++)
				{
					// ������� ����� ��������� 8 ���� ������ �������� CRC-64,
					// �, �����, ��������� ����.
					if(fileStreamTargetArr[damagedVolList[i]] != null)
					{
						fileStreamTargetArr[damagedVolList[i]].Write(new byte[8], 0, 8);
						fileStreamTargetArr[damagedVolList[i]].Flush();
						fileStreamTargetArr[damagedVolList[i]].Close();
						fileStreamTargetArr[damagedVolList[i]] = null;
					}

					// ���� ���� �������� �� �������� ���������� ��������� -...
					if(
						((++volNum % progressMod1) == 0)
						&&
						(OnUpdateFileStreamsClosingProgress != null)
						)
					{
						//...������� ������
						OnUpdateFileStreamsClosingProgress(((double)(volNum + 1) / (double)(this.dataCount + this.eccCount)) * 100);
					}
				}

				// ��������, ��� �������� �������� ������� �����������
				if(OnFileStreamsClosingFinish != null)
				{
					OnFileStreamsClosingFinish();
				}
			}

			// ���� ���� ���� �� ���� ���������� - ��������� �������� ����� �
			// �������� �� ������
			catch
			{
				// ��������� ������� �������� ������
				for(int i = 0; i < this.dataCount; i++)
				{
					if(fileStreamSourceArr[i] != null)
					{
						fileStreamSourceArr[i].Close();
						fileStreamSourceArr[i] = null;
					}
				}

				// ��������� �������� �������� ������
				for(int i = 0; i < damagedVolCount; i++)
				{
					if(fileStreamTargetArr[damagedVolList[i]] != null)
					{
						fileStreamTargetArr[damagedVolList[i]].Flush();
						fileStreamTargetArr[damagedVolList[i]].Close();
						fileStreamTargetArr[damagedVolList[i]] = null;
					}
				}

				// ��������� �� ��, ��� ��������� ������ ������ � �������
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