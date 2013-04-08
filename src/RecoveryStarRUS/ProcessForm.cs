/*----------------------------------------------------------------------+
 |  filename:   ProcessForm.cs                                          |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ������ � �������                                        |
 +----------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace RecoveryStar
{
	public partial class ProcessForm : Form
	{
		#region Public Properties & Data

		/// <summary>
		/// �������� �������
		/// </summary>
		public FileBrowser.Browser Browser { get; set; }

		/// <summary>
		/// ����������������� ������ ������
		/// </summary>
		public Security Security
		{
			get
			{
				if(this.eRecoveryStarCore != null)
				{
					return this.eRecoveryStarCore.Security;
				}
				else
				{
					return null;
				}
			}

			set
			{
				if(this.eRecoveryStarCore != null)
				{
					this.eRecoveryStarCore.Security = value;
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
				if(this.eRecoveryStarCore != null)
				{
					return this.eRecoveryStarCore.CBCBlockSize;
				}
				else
				{
					return -1;
				}
			}

			set
			{
				if(this.eRecoveryStarCore != null)
				{
					this.eRecoveryStarCore.CBCBlockSize = value;
				}
			}
		}

		/// <summary>
		/// ������ ������ ���� ������ ��� ���������
		/// </summary>
		public ArrayList FileNamesToProcess
		{
			get { return this.fileNamesToProcess; }

			set { this.fileNamesToProcess = value; }
		}

		/// <summary>
		/// ������ ������ ���� ������ ��� ���������
		/// </summary>
		private ArrayList fileNamesToProcess;

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		public int DataCount
		{
			get { return this.dataCount; }

			set { this.dataCount = value; }
		}

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		private int dataCount;

		/// <summary>
		/// ���������� ����� ��� ��������������
		/// </summary>
		public int EccCount
		{
			get { return this.eccCount; }

			set { this.eccCount = value; }
		}

		/// <summary>
		/// ���������� ����� ��� ��������������
		/// </summary>
		private int eccCount;

		/// <summary>
		/// ��� ������ (�� ���� ������������ �������)
		/// </summary>
		public int CodecType
		{
			get { return this.codecType; }

			set { this.codecType = value; }
		}

		/// <summary>
		/// ��� ������ ����-�������� (�� ���� ������������ ������� �����������)
		/// </summary>
		private int codecType;

		/// <summary>
		/// ������������ ����� ��������� ������
		/// </summary>
		public RSMode Mode
		{
			get { return this.mode; }

			set { this.mode = value; }
		}

		/// <summary>
		/// ������������ ����� ���������
		/// </summary>
		private RSMode mode;

		/// <summary>
		/// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
		/// </summary>
		public bool FastExtraction
		{
			get { return this.fastExtraction; }

			set { this.fastExtraction = value; }
		}

		/// <summary>
		/// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
		/// </summary>
		private bool fastExtraction;

		#endregion Public Properties & Data

		#region Data

		/// <summary>
		/// ��������� ������ ��� �������� (����������) ����� ����� � ���������� ������
		/// </summary>
		private FileNamer eFileNamer;

		/// <summary>
		/// ���� ������� ����������������� �����������
		/// </summary>
		private RecoveryStarCore eRecoveryStarCore;

		/// <summary>
		/// ������� ��������� ������������ ������
		/// </summary>
		private int OKCount;

		/// <summary>
		/// ������� ����������� ������������ ������
		/// </summary>
		private int errorCount;

		/// <summary>
		/// ����� ��������� ������
		/// </summary>
		private Thread thrRecoveryStarProcess;

		/// <summary>
		/// ��������� �������� ��������� ������
		/// </summary>
		private ThreadPriority threadPriority;

		/// <summary>
		/// ������� ����������� ���������
		/// </summary>
		private ManualResetEvent[] exitEvent;

		/// <summary>
		/// ������� ����������� ���������
		/// </summary>
		private ManualResetEvent[] executeEvent;

		/// <summary>
		/// ������� "�����������" ����� ��������
		/// </summary>
		private ManualResetEvent[] wakeUpEvent;

		/// <summary>
		/// ����� ��� ������ ��� ���������� ��������� (��������� ��������)
		/// </summary>
		private String processGroupBoxText;

		/// <summary>
		/// �������� ��������� (��������� ��������)
		/// </summary>
		private int processProgressBarValue;

		/// <summary>
		/// ������� ��� �������������� ���������� ��������/�������� ��� ������ �� �����������
		/// </summary>
		private Semaphore processStatSema;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� �����
		/// </summary>
		public ProcessForm()
		{
			InitializeComponent();

			// ��-��������� ����� ��������� �� ����������
			this.mode = RSMode.None;

			// �������������� ��������� ������ ��� �������� (����������) ����� �����
			// � ���������� ������
			this.eFileNamer = new FileNamer();

			// ������� ��������� ������ ���� RecoveryStar
			this.eRecoveryStarCore = new RecoveryStarCore();

			// ������������� �� ��������� ���������
			this.eRecoveryStarCore.OnUpdateFileSplittingProgress = new OnUpdateDoubleValueHandler(OnUpdateFileSplittingProgress);
			this.eRecoveryStarCore.OnFileSplittingFinish = new OnEventHandler(OnFileSplittingFinish);
			this.eRecoveryStarCore.OnUpdateRSMatrixFormingProgress = new OnUpdateDoubleValueHandler(OnUpdateRSMatrixFormingProgress);
			this.eRecoveryStarCore.OnRSMatrixFormingFinish = new OnEventHandler(OnRSMatrixFormingFinish);
			this.eRecoveryStarCore.OnUpdateFileStreamsOpeningProgress = new OnUpdateDoubleValueHandler(OnUpdateFileStreamsOpeningProgress);
			this.eRecoveryStarCore.OnFileStreamsOpeningFinish = new OnEventHandler(OnFileStreamsOpeningFinish);
			this.eRecoveryStarCore.OnStartedRSCoding = new OnEventHandler(OnStartedRSCoding);
			this.eRecoveryStarCore.OnUpdateFileCodingProgress = new OnUpdateDoubleValueHandler(OnUpdateFileCodingProgress);
			this.eRecoveryStarCore.OnFileCodingFinish = new OnEventHandler(OnFileCodingFinish);
			this.eRecoveryStarCore.OnUpdateFileStreamsClosingProgress = new OnUpdateDoubleValueHandler(OnUpdateFileStreamsClosingProgress);
			this.eRecoveryStarCore.OnFileStreamsClosingFinish = new OnEventHandler(OnFileStreamsClosingFinish);
			this.eRecoveryStarCore.OnUpdateFileAnalyzeProgress = new OnUpdateDoubleValueHandler(OnUpdateFileAnalyzeProgress);
			this.eRecoveryStarCore.OnFileAnalyzeFinish = new OnEventHandler(OnFileAnalyzeFinish);
			this.eRecoveryStarCore.OnGetDamageStat = new OnUpdateTwoIntDoubleValueHandler(OnGetDamageStat);

			// ��������� �������� �������� - 1, �������� 1 ����.
			this.processStatSema = new Semaphore(1, 1);

			// �������������� ������ ������ ��� ���������
			this.fileNamesToProcess = new ArrayList();

			// ��������� �������� � �������� ����������, �������������� ��
			// ��������� �������� ��������� ������
			SetThreadPriority(processPriorityComboBox.SelectedIndex);

			// �������������� ������� ����������� ��������� �����
			this.exitEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������������� c������ ����������� ��������� �����
			this.executeEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������������� c������ "�����������" ����� ��������
			this.wakeUpEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// ������������� �������� ��-��������� ��� ����������
			processPriorityComboBox.Text = "��-���������";

			this.processProgressBarValue = -1;
		}

		#endregion Construction & Destruction

		#region Public Operations

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
		/// ����� ��������� ������
		/// </summary>
		private void Process()
		{
			// ���������� ������ ������ ��������� ���������
			this.Invoke(((OnEventHandler)delegate() { processTimer.Start(); }), new object[] {});

			// ����� ��������������� �����
			int fileNum = 0;

			// ���������� ���������� ���������
			this.OKCount = 0;
			this.errorCount = 0;

			// ������, �������� �������� ������ ���������� �������������� ������
			String filesTotal = Convert.ToString(this.fileNamesToProcess.Count);

			// ������, �������� ����� ��� ������ �� �����
			String textToOut = "";

			// ������������ ��� ����� �� ��������������� ������
			foreach(String fullFileName in this.fileNamesToProcess)
			{
				// �������� �������� ������� �������� �����
				String shortFileName = this.eFileNamer.GetShortFileName(fullFileName);

				// ��� ����� ��� ������ �� �����
				String unpackedFileName = shortFileName;

				// ���� ������������ ����� �� ������ ������, � ������,
				// ��������� ���������� ����� �� ����������� �������
				if(this.mode != RSMode.Protect)
				{
					// ������������� �������� ������� ����� � ���������� �������������
					unpackedFileName = shortFileName;

					// ���� �� ������� ��������� ����������� �������� ��� - ���������
					// �� ��������� ��������
					if(!this.eFileNamer.Unpack(ref unpackedFileName))
					{
						continue;
					}
				}

				// �������������� ����� ��� ������ � ��������� �����
				switch(this.mode)
				{
					case RSMode.Protect:
						{
							textToOut = " ������ ����� \"";
							break;
						}

					case RSMode.Recover:
						{
							textToOut = " ���������� ����� \"";
							break;
						}

					case RSMode.Repair:
						{
							textToOut = " ������� ����� ����� \"";
							break;
						}

					default:
					case RSMode.Test:
						{
							textToOut = " ������������ ����� \"";
							break;
						}
				}

				textToOut += unpackedFileName + "\" (" + Convert.ToString(++fileNum) + " �� " + filesTotal + ")";

				// ������� ����� � ��������� �����
				this.Invoke(((OnUpdateStringValueHandler)delegate(String value) { this.Text = value; }), new object[] {textToOut});

				// ������������ ��������� ���������
				switch(this.mode)
				{
					case RSMode.Protect:
						{
							// ��������� �� �������� ����������, ������� �� �����
							// �������������� � ��������� �������� ��������
							fileAnalyzeStatGroupBox.Invoke(((OnEventHandler)delegate() { fileAnalyzeStatGroupBox.Enabled = false; }), new object[] {});
							percOfDamageLabel_.Invoke(((OnEventHandler)delegate() { percOfDamageLabel_.Enabled = false; }), new object[] {});
							percOfAltEccLabel_.Invoke(((OnEventHandler)delegate() { percOfAltEccLabel_.Enabled = false; }), new object[] {});
							percOfDamageLabel.Invoke(((OnEventHandler)delegate() { percOfDamageLabel.Enabled = false; }), new object[] {});
							percOfAltEccLabel.Invoke(((OnEventHandler)delegate() { percOfAltEccLabel.Enabled = false; }), new object[] {});

							// ��������� ���������������� �����������
							this.eRecoveryStarCore.StartToProtect(fullFileName, this.dataCount, this.eccCount, this.codecType, true);

							break;
						}

					case RSMode.Recover:
						{
							// ��������� �������������� ������
							this.eRecoveryStarCore.StartToRecover(fullFileName, this.fastExtraction, true);

							break;
						}

					case RSMode.Repair:
						{
							// ��������� ������� ������
							this.eRecoveryStarCore.StartToRepair(fullFileName, this.fastExtraction, true);

							break;
						}

					default:
					case RSMode.Test:
						{
							// ��������� ������������ ������
							this.eRecoveryStarCore.StartToTest(fullFileName, this.fastExtraction, true);

							break;
						}
				}

				// ���� ��������� ���������
				while(true)
				{
					// ���� �� ���������� �������������� ������� "executeEvent",
					// �� ������������ �����, ����� �� ��������� ��������� �� ����� -
					if(!ManualResetEvent.WaitAll(this.executeEvent, 0, false))
					{
						//...���������������� ������ ��������������� ���������...
						this.eRecoveryStarCore.Pause();

						//...� ���� ��������
						ManualResetEvent.WaitAll(this.executeEvent);

						// � ����� ����������, ���������, ��� ��������� ������ ������������
						this.eRecoveryStarCore.Continue();
					}

					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.wakeUpEvent[0], this.exitEvent[0], this.eRecoveryStarCore.FinishedEvent[0]});

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
						this.eRecoveryStarCore.Stop();

						return;
					}

					//...���� �������� ������ � ���������� ��������� ��������� ����������...
					if(eventIdx == 2)
					{
						//...������� �� ����� �������� ���������� (����� � ����� � while(true)!)
						break;
					}
				} // while(true)

				// � ����� � ��������� �������� ���������� �������� �������
				// ���������� ��������� ������ ���������, ��������� �������
				// ����������� � ���� ������. ����� ��� �� ��������, ��
				// ������������� �� ��������� ��������, ��������, ���
				// "�� ����������"
				for(int i = 0; i < (int)WaitCount.MaxWaitCount; i++)
				{
					if(!this.eRecoveryStarCore.Finished)
					{
						Thread.Sleep((int)WaitTime.MinWaitTime);
					}
					else
					{
						break;
					}
				}

				// ���������� �������� �� ������������ ���������
				if(this.eRecoveryStarCore.ProcessedOK)
				{
					// ���� ��������� ����� ��������� ���������...
					OnUpdateLogListBox(this.Text.Substring(1) + ": OK!");

					// �������� ����������
					this.OKCount++;

					// ��������� ����� ��� ������ �� �����
					textToOut = Convert.ToString(this.OKCount);

					okCountLabel.Invoke(((OnUpdateStringValueHandler)delegate(String value) { okCountLabel.Text = value; }), new object[] {textToOut});
				}
				else
				{
					// ���� ��������� ����� ��������� �����������...
					OnUpdateLogListBox(this.Text.Substring(1) + ": Error!");

					// �������� ����������
					this.errorCount++;

					// ��������� ����� ��� ������ �� �����
					textToOut = Convert.ToString(this.errorCount);

					errorCountLabel.Invoke(((OnUpdateStringValueHandler)delegate(String value) { errorCountLabel.Text = value; }), new object[] {textToOut});
				}

				// ������� ������ ��� ��������� ������ ����� ���� �� �����
				OnUpdateLogListBox("");
			}

			// ���� ������������ ����� �������������� ������ � ��������� ������ ���������, ��
			// ����� ���������� ��� �������� ����� ���������
			if(
				(this.mode == RSMode.Recover)
				&&
				(this.eRecoveryStarCore.ProcessedOK)
				)
			{
				try
				{
					foreach(String fullFileName in this.fileNamesToProcess)
					{
						// ���������� ��������� ���� �� ������� ����� �����
						String path = this.eFileNamer.GetPath(fullFileName);

						// ���������� ��������� ����� �� ������� ����� �����
						String fileName = this.eFileNamer.GetShortFileName(fullFileName);

						// ���� ��� ��������� ��������������� - ��� �������� ��������,
						// �.�. �������� ��� �� ������ ���������� ����������� ���������
						if(!this.eFileNamer.Unpack(ref fileName, ref this.dataCount, ref this.eccCount, ref this.codecType))
						{
							continue;
						}

						// ������������ ��� �����
						for(int i = 0; i < (this.dataCount + this.eccCount); i++)
						{
							// ��������� �������������� ��� �����,...
							String volumeName = fileName;

							//...����������� ��� � ���������� ������...
							this.eFileNamer.Pack(ref volumeName, i, this.dataCount, this.eccCount, this.codecType);

							//...��������� ������ ��� �����...
							volumeName = path + volumeName;

							//...���������� ���� �� ������� �����...
							if(File.Exists(volumeName))
							{
								//...���� ������� �������, ������ �� ���� ��������
								// ��-���������...
								File.SetAttributes(volumeName, FileAttributes.Normal);

								//...� ����� �������
								File.Delete(volumeName);
							}
						}
					}
				}
				catch
				{
				}
			}

			// ��������� ����� ��� ������...
			textToOut = "�������";

			//...�������� ������� �� ������ ����������� ���������...
			stopButtonXP.Invoke(((OnUpdateStringValueHandler)delegate(String value) { stopButtonXP.Text = value; }), new object[] {textToOut});

			//...��������� ������ "�����"...
			pauseButtonXP.Invoke(((OnEventHandler)delegate() { pauseButtonXP.Enabled = false; }), new object[] {});

			//...� ������ ���������� ���������...
			Thread.Sleep(2 * processTimer.Interval);
			this.Invoke(((OnEventHandler)delegate() { processTimer.Stop(); }), new object[] {});

			//...� ���������� ������ ������ ���������� ��������...
			processPriorityComboBox.Invoke(((OnEventHandler)delegate() { processPriorityComboBox.Enabled = false; }), new object[] {});

			//...�� �������� ������ �������� �����
			stopButtonXP.Invoke(((OnEventHandler)delegate() { stopButtonXP.Enabled = true; }), new object[] {});
		}

		/// <summary>
		/// ����� ��������� ���������� �������� ��������� �� ��������� ����������� �������� int
		/// </summary>
		/// <param name="value">��� ���������� ��������</param>
		private void SetThreadPriority(int value)
		{
			if(
				(this.thrRecoveryStarProcess != null)
				&&
				(this.thrRecoveryStarProcess.IsAlive)
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

				// ������������� ��������� ���������
				this.thrRecoveryStarProcess.Priority = this.threadPriority;

				this.eRecoveryStarCore.ThreadPriority = value;
			}
		}

		/// <summary>
		/// ����� ���������� �������� � ��������� �������� ���������� "processGroupBox"
		/// </summary>
		/// <param name="text">�����, ����������� ���������� ��������</param>
		/// <param name="progress">���������� �������� ���������</param>
		private void OnUpdateProgressGroupBox(String text, double progress)
		{
			// ������� ����������� ���� � ����������� �������...
			if(this.processStatSema.WaitOne(0, false))
			{
				this.processGroupBoxText = text + ": " + Convert.ToString((int)(progress) + " %");

				// ����� �� ����������� �������
				this.processStatSema.Release();
			}
		}

		/// <summary>
		/// ����� ���������� ��������, ������������ ���������
		/// � ��������� �������� ���������� "processGroupBox"
		/// </summary>
		/// <param name="text">����� ��� ������</param>
		private void OnFinishProgressGroupBox(String text)
		{
			// ���� � ����������� �������...
			if(this.processStatSema.WaitOne())
			{
				this.processGroupBoxText = text + ": ���������";
				this.processProgressBarValue = 100;

				// ����� �� ����������� �������
				this.processStatSema.Release();
			}
		}

		/// <summary>
		/// ����� �������� ��������� � ������� ���������� "processProgressBar"
		/// </summary>
		/// <param name="progress">���������� �������� ���������</param>
		private void OnUpdateProcessProgressBar(double progress)
		{
			// ������� ����������� ���� � ����������� �������...
			if(this.processStatSema.WaitOne(0, false))
			{
				this.processProgressBarValue = (int)progress;

				// ����� �� ����������� �������
				this.processStatSema.Release();
			}
		}

		/// <summary>
		/// ���������� ������� "���������� ��������� ��������� �����"
		/// </summary>
		/// <param name="progress">�������� ��������� � ���������</param>
		private void OnUpdateFileSplittingProgress(double progress)
		{
			OnUpdateProgressGroupBox("��������� �����", progress);

			OnUpdateProcessProgressBar(progress);
		}

		/// <summary>
		/// ���������� ������� "���������� �������� ��������� �����"
		/// </summary>
		private void OnFileSplittingFinish()
		{
			OnFinishProgressGroupBox("��������� �����");
		}

		/// <summary>
		/// ���������� ������� "���������� ��������� ������� ������� ����������� ����-��������"
		/// </summary>
		private void OnUpdateRSMatrixFormingProgress(double progress)
		{
			OnUpdateProgressGroupBox("������ ������� �����������", progress);

			OnUpdateProcessProgressBar(progress);
		}

		/// <summary>
		/// ���������� ������� "���������� ������� ������� ����������� ����-��������"
		/// </summary>
		private void OnRSMatrixFormingFinish()
		{
			OnFinishProgressGroupBox("������ ������� �����������");
		}

		/// <summary>
		/// ���������� ������� "���������� ��������� �������� �������� �������"
		/// </summary>
		private void OnUpdateFileStreamsOpeningProgress(double progress)
		{
			OnUpdateProgressGroupBox("�������� �������� �������", progress);

			OnUpdateProcessProgressBar(progress);
		}

		/// <summary>
		/// ���������� ������� "���������� �������� �������� �������� �������"
		/// </summary>
		private void OnFileStreamsOpeningFinish()
		{
			OnFinishProgressGroupBox("�������� �������� �������");
		}

		/// <summary>
		/// ���������� ������� "������ ����������� ����-��������"
		/// </summary>
		private void OnStartedRSCoding()
		{
			if(processGroupBox.InvokeRequired) processGroupBox.Invoke(new OnEventHandler(OnStartedRSCoding), new object[] {});
			else
			{
				processGroupBox.Text = "����������� ����������� �������� ������� (���� ������� ����� ������ ��������� �����)";
			}
		}

		/// <summary>
		/// ���������� ������� "���������� ��������� �������� ����������� �����"
		/// </summary>
		private void OnUpdateFileCodingProgress(double progress)
		{
			OnUpdateProgressGroupBox("����������� ����-��������", progress);

			OnUpdateProcessProgressBar(progress);
		}

		/// <summary>
		/// ���������� ������� "���������� �������� ����������� �����"
		/// </summary>
		private void OnFileCodingFinish()
		{
			OnFinishProgressGroupBox("����������� ����-��������");
		}

		/// <summary>
		/// ���������� ������� "���������� ��������� �������� �������� �������"
		/// </summary>
		private void OnUpdateFileStreamsClosingProgress(double progress)
		{
			OnUpdateProgressGroupBox("�������� �������� �������", progress);

			OnUpdateProcessProgressBar(progress);
		}

		/// <summary>
		/// ���������� ������� "���������� �������� �������� �������� �������"
		/// </summary>
		private void OnFileStreamsClosingFinish()
		{
			OnFinishProgressGroupBox("�������� �������� �������");
		}

		/// <summary>
		/// ���������� ������� "���������� ��������� �������� ������� �����"
		/// </summary>
		private void OnUpdateFileAnalyzeProgress(double progress)
		{
			OnUpdateProgressGroupBox("�������� ����������� ������", progress);

			OnUpdateProcessProgressBar(progress);
		}

		/// <summary>
		/// ���������� ������� "���������� �������� ������� �����"
		/// </summary>
		private void OnFileAnalyzeFinish()
		{
			OnFinishProgressGroupBox("�������� ����������� ������");
		}

		/// <summary>
		/// ���������� ������� "��������� ���������� ����������� �����"
		/// </summary>
		private void OnGetDamageStat(int allVolMissCount, int altEccVolPresentCount, double percOfDamage, double percOfAltEcc)
		{
			if(this.InvokeRequired) this.Invoke(new OnUpdateTwoIntDoubleValueHandler(OnGetDamageStat), new object[] {allVolMissCount, altEccVolPresentCount, percOfDamage, percOfAltEcc});
			else
			{
				// ������� ���������� �����������
				percOfDamageLabel.Text = Convert.ToString((int)(percOfDamage)) + " %  (" + Convert.ToString(allVolMissCount) + ");";
				percOfAltEccLabel.Text = Convert.ToString((int)(percOfAltEcc)) + " %  (" + Convert.ToString(altEccVolPresentCount) + ");";
				logListBox.Items.Add("����� ������������ �����: " + percOfDamageLabel.Text);
				logListBox.Items.Add("������ ����� ��� ��������������: " + percOfAltEccLabel.Text);
			}
		}

		/// <summary>
		/// ���������� ������� "���������� ���� �������� ���������"
		/// </summary>
		private void OnUpdateLogListBox(String text)
		{
			if(logListBox.InvokeRequired) logListBox.Invoke(new OnUpdateStringValueHandler(OnUpdateLogListBox), new object[] {text});
			else
			{
				logListBox.Items.Add(text);
			}
		}

		/// <summary>
		/// ���������� ������� "������� ��������� ��������"
		/// </summary>
		private void processPriorityComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// ��������� �������� � �������� ����������, �������������� ��
			// ��������� �������� ��������� ������
			SetThreadPriority(processPriorityComboBox.SelectedIndex);

			pauseButtonXP.Focus();
		}

		/// <summary>
		/// ���������� ��������� �� �����
		/// </summary>
		private void pauseButtonXP_Click(object sender, EventArgs e)
		{
			if(pauseButtonXP.Text == "�����")
			{
				pauseButtonXP.Text = "����������";

				// ��������� ������ ������ ��������� ���������...
				this.Invoke(((OnEventHandler)delegate() { processTimer.Stop(); }), new object[] {});

				// ������ ��������� �� �����...
				Pause();
			}
			else
			{
				pauseButtonXP.Text = "�����";

				// ������� ��������� � �����
				Continue();

				// ���������� ������ ������ ��������� ���������...
				this.Invoke(((OnEventHandler)delegate() { processTimer.Start(); }), new object[] {});
			}
		}

		/// <summary>
		/// ����� ��������� ��������� - ������ ������ ���������, �, �����,
		/// ��������� ������, �� ����������� �������� � ���������� �������� �����
		/// </summary>
		private void stopButtonXP_Click(object sender, EventArgs e)
		{
			// ���� ������ �� �������� �������� �� ����� ��������� -
			// ����� �������������� ������
			if(this.stopButtonXP.Text == "�������� ���������")
			{
				string message = "�� ������������� ������ �������� ���������?";
				string caption = " Recovery Star 2.22";
				MessageBoxButtons buttons = MessageBoxButtons.YesNo;
				DialogResult result = MessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

				// ���� ������������ ����� �� ������ "No" - ������� �� �����������
				if(result == DialogResult.No)
				{
					return;
				}
			}

			// ������� ��������� ������ ���������� ���������...
			stopButtonXP.Enabled = false;

			// ����������� ��������� ������...
			Stop();

			// ���������� ������ ������ �� ���������...
			closingTimer.Start();
		}

		/// <summary>
		/// ���������� �������, �������������� �� ����������� ������ �� ���������
		/// </summary>
		private void closingTimer_Tick(object sender, EventArgs e)
		{
			// ���� ���� ������� �� ��������� ���� ������ - ��������� ����� ���� ��� ������!
			if(!this.eRecoveryStarCore.Finished)
			{
				return;
			}

			// ������������ ������...
			closingTimer.Stop();

			// ��������� �����
			Close();

			// ���������, ��� ��������� �� ������������
			this.mode = RSMode.None;

			// �������� ������� �� ������� �����
			this.Browser.Enabled = true;

			// ���������� ������ ������
			GC.Collect();
		}

		/// <summary>
		/// ���������� ������� "��� ������� ��� ���������� ���������� ���������"
		/// </summary>
		private void processTimer_Tick(object sender, EventArgs e)
		{
			// ���� � ����������� �������...
			if(this.processStatSema.WaitOne())
			{
				if(this.processGroupBoxText != null)
				{
					processGroupBox.Text = this.processGroupBoxText;
				}

				if(this.processProgressBarValue != -1)
				{
					processProgressBar.Value = this.processProgressBarValue;
				}

				// ����� �� ����������� �������
				this.processStatSema.Release();
			}
		}

		/// <summary>
		/// ���������� ������� "�������� ����� ���������"
		/// </summary>
		private void ProcessForm_Load(object sender, EventArgs e)
		{
			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();
			this.wakeUpEvent[0].Reset();

			// ������� ����� ��������� ������...
			this.thrRecoveryStarProcess = new Thread(new ThreadStart(Process));

			//...����� ���� ��� ���...
			this.thrRecoveryStarProcess.Name = "RecoveryStar.Process()";

			//...������������� ��������� ��������� ������...
			this.thrRecoveryStarProcess.Priority = this.threadPriority;

			// ��������� ������� �� ������� �����
			this.Browser.Enabled = false;

			//...� ��������� ���
			this.thrRecoveryStarProcess.Start();
		}

		#endregion Private Operations
	}
}