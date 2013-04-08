/*----------------------------------------------------------------------+
 |  filename:   BenchmarkForm.cs                                        |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ���� ��������������                                     |
 +----------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace RecoveryStar
{
	public partial class BenchmarkForm : Form
	{
		#region Public Properties & Data

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

		#endregion Public Properties & Data

		#region Data

		/// <summary>
		/// RAID-�������� ����� ����-��������
		/// </summary>
		private RSRaidEncoder eRSRaidEncoder;

		/// <summary>
		/// ���������� ���� �����
		/// </summary>
		private GF16 eGF16;

		/// <summary>
		/// �����, ������� ��������� ����
		/// </summary>
		private double timeInTest;

		/// <summary>
		/// ������������ ����� ������ � ����������
		/// </summary>
		private double processedDataCount;

		/// <summary>
		/// ����� ��������� ������
		/// </summary>
		private Thread thrBenchmarkProcess;

		/// <summary>
		/// ������� ����������� ���������
		/// </summary>
		private ManualResetEvent[] exitEvent;

		/// <summary>
		/// ������� ����������� ���������
		/// </summary>
		private ManualResetEvent[] executeEvent;

		/// <summary>
		/// ������� ��� �������������� ���������� ��������/�������� ��� ������ �� �����������
		/// </summary>
		private Semaphore coderStatSema;

		/// <summary>
		/// ��������� �������� ������� �� ������ ������� �����
		/// </summary>
		private long DateTimeTicksOnStart;

		/// <summary>
		/// ������� ���������� ������������ ����
		/// </summary>
		private long processedBytesCount;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� �����
		/// </summary>
		public BenchmarkForm()
		{
			InitializeComponent();

			// �������������� ������� ����������� ��������� ������
			this.exitEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������������� c������ ����������� ��������� ������
			this.executeEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// ��������� �������� �������� - 1, �������� 1 ����.
			this.coderStatSema = new Semaphore(1, 1);

			// ������� ����� ���������� ���� �����
			this.eGF16 = new GF16();
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
		/// ���� �� �������������� (����� ����-��������)
		/// </summary>
		private void Benchmark()
		{
			// ������� RAID-�������� ����� ����-��������
			if(this.eRSRaidEncoder == null)
			{
				this.eRSRaidEncoder = new RSRaidEncoder(this.dataCount, this.eccCount, this.codecType);
			}

			// ������������� �� ���������
			this.eRSRaidEncoder.OnUpdateRSMatrixFormingProgress = new OnUpdateDoubleValueHandler(OnUpdateRSMatrixFormingProgress);
			this.eRSRaidEncoder.OnRSMatrixFormingFinish = new OnEventHandler(OnRSMatrixFormingFinish);

			// ��������� ���������� RAID-��������� ������ ����-��������
			if(this.eRSRaidEncoder.Prepare(true))
			{
				// ���� �������� ���������� ���������� ������ ����-�������� � ������
				while(true)
				{
					// ���� ����� �� ������������� �������...
					int eventIdx = ManualResetEvent.WaitAny(new ManualResetEvent[] {this.exitEvent[0], this.eRSRaidEncoder.FinishedEvent[0]});

					//...���� �������� ������ � ������ �� ���������...
					if(eventIdx == 0)
					{
						//...������������� �������������� ��������
						this.eRSRaidEncoder.Stop();

						return;
					}

					//...���� �������� ������ � ���������� ��������� ��������� ����������...
					if(eventIdx == 1)
					{
						//...������� �� ����� �������� ���������� (����� � ����� � while(true)!)
						break;
					}
				} // while(true)
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
				string message = "Reed-Solomon Coder configuration error!";
				string caption = " Recovery Star 2.22";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

				return;
			}

			// �������� ������ ��� ������� ������� � �������� ������ ������
			int[] sourceLog = new int[this.dataCount];
			int[] target = new int[this.eccCount];

			// ����� ��� ������ � ������ ����
			byte[] wordBuff = new byte[2];

			// ������ ��� �������� ����������� �������� �������
			byte[] fileStreamImitator_0 = new byte[this.dataCount];
			byte[] fileStreamImitator_1 = new byte[this.dataCount];

			// �������������� ��������� ��������� �����
			Random eRandom = new Random((int)System.DateTime.Now.Ticks);

			// ��������� ������ ���������� �������
			eRandom.NextBytes(fileStreamImitator_0);
			eRandom.NextBytes(fileStreamImitator_1);

			// ��������� �������� ������� �� ������������ ������ �������
			long DateTimeTicksNow = -1;

			// ���������� ������� ���������� ������������ ����
			this.processedBytesCount = 0;

			// ��������� ��������� �������� ������� �� ������ ������� �����
			this.DateTimeTicksOnStart = System.DateTime.Now.Ticks;

			// ����������� ���� ������������ ������ ����-��������
			while(true)
			{
				// ��������� ������ �������� ������ ������ ������� �������� �����
				for(int j = 0; j < this.dataCount; j++)
				{
					// ������ ���� ���� �� �������� ������
					wordBuff[0] = fileStreamImitator_0[j];
					wordBuff[1] = fileStreamImitator_1[j];

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

					// ������ ����� ���� ���� � �������� �����...
					// �.�. ��� ���� - �������� ������ ������ �� ���� �� ������������
				}

				// ��������� �������� �������
				DateTimeTicksNow = System.DateTime.Now.Ticks;

				// ������������� ���������� ������������ ����
				this.processedBytesCount += (2 * this.dataCount);

				// ������� ����������� ���� � ����������� �������...
				if(this.coderStatSema.WaitOne(0, false))
				{
					// ��������� ����� ������ ����� � �������� (1 ��� == 10^-07 �������)
					// 0x01 - ����� �� �������� ������� �� ����
					this.timeInTest = ((double)((DateTimeTicksNow - this.DateTimeTicksOnStart) | 0x01) / 10000000.0);

					// ��������� ������������ ����� ������ � ����������
					this.processedDataCount = ((double)this.processedBytesCount / (double)(1024 * 1024));

					// ���������� ��������� "V" �� ��������...
					this.coderStatSema.Release();
				}

				// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
				// ����� ��������, � ����� �� ����� ������ �� ��� ���������
				ManualResetEvent.WaitAll(this.executeEvent);

				// ���� �������� ������ � ������ �� ���������...
				if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
				{
					return;
				}
			} // while(true)
		}

		/// <summary>
		/// ���������� ������� "���������� ��������� ������� ������� ����������� ����-��������"
		/// </summary>
		private void OnUpdateRSMatrixFormingProgress(double progress)
		{
			// ��������� ����� ��� ������...
			String textToOut = "Preparing: " + System.Convert.ToString((int)progress) + " %";

			//...�������� ������� ��������� ����
			this.Invoke(((OnUpdateStringValueHandler)delegate(String value) { this.Text = value; }), new object[] {textToOut});
		}

		/// <summary>
		/// ���������� ������� "���������� ������� ������� ����������� ����-��������"
		/// </summary>
		private void OnRSMatrixFormingFinish()
		{
			// ��������� ����� ��� ������...
			String textToOut = "Benchmarking...";

			//...�������� ������� ��������� ����
			this.Invoke(((OnUpdateStringValueHandler)delegate(String value) { this.Text = value; }), new object[] {textToOut});
		}

		/// <summary>
		/// ���������� ���� ������� - ����� ���������� �� �����
		/// </summary>
		private void BenchmarkTimer_Tick(object sender, EventArgs e)
		{
			// ���� � ����������� �������...
			if(this.coderStatSema.WaitOne())
			{
				// ������� ���������� �� ������� ������ �����
				timeInTestLabel.Text = System.Convert.ToString((int)this.timeInTest) + " s";

				// ������� ���������� �� ������ ������������ �������� �� ����� ������ �����
				processedDataCountLabel.Text = System.Convert.ToString((int)this.processedDataCount) + " Mbytes";

				// ��������� �������� �����������
				double speed = this.processedDataCount / this.timeInTest;

				// ��������� ������ ����������� ��������
				// (����� � �����, �������� ����� ������, ���� �����������
				// ������������ ������ IndexOf())
				String outSpeedStr = System.Convert.ToString(speed) + ',';

				// ���������� ��������� ����������� �����������
				int indexOfPoint = outSpeedStr.IndexOf(',');

				// ����� ���������� ���������
				int subStrLen = -1;

				// ���� ������������ ������������ ����� � ����� ������ -
				// ��� ��������� ����� � � �����������
				if(indexOfPoint == (outSpeedStr.Length - 1))
				{
					subStrLen = (outSpeedStr.Length - 1);
				}
				else
				{
					if(indexOfPoint < (outSpeedStr.Length - 2))
					{
						subStrLen = (indexOfPoint + 1) + 2;
					}
				}

				// ������� ���������� �� �������� �����������
				coderSpeedGroupBox.Text = "Speed: " + outSpeedStr.Substring(0, subStrLen) + " Mbytes/s";

				// ���������� ��������� "V" �� ��������...
				this.coderStatSema.Release();
			}
		}

		/// <summary>
		/// ���������� ������������ ������������������ �� �����
		/// </summary>
		private void pauseButtonXP_Click(object sender, EventArgs e)
		{
			if(pauseButtonXP.Text == "Pause")
			{
				pauseButtonXP.Text = "Continue";

				// ��������� ������ ���������� ������ �����...
				benchmarkTimer.Stop();

				// ...� ������ ��������� �� �����
				Pause();
			}
			else
			{
				pauseButtonXP.Text = "Pause";

				// ���������� ������� ���������� ������������ ����
				this.processedBytesCount = 0;

				// ��������� ��������� �������� ������� �� ������ ������������� �����
				this.DateTimeTicksOnStart = System.DateTime.Now.Ticks;

				// ������� ��������� � �����...
				Continue();

				// ...� �������� ������ ���������� ������ �����
				benchmarkTimer.Start();
			}
		}

		/// <summary>
		/// ����� ���������� ������������ - ������ ������ ���������, �, �����,
		/// ��������� ������, �� ����������� �������� � ���������� �������� �����
		/// </summary>
		private void closeButtonXP_Click(object sender, EventArgs e)
		{
			// ������� ��������� ������ ���������� ������������...
			closeButtonXP.Enabled = false;

			// ����������� ��������� ������...
			Stop();

			// ���������� ������ ������ �� ���������...
			closingTimer.Start();
		}

		/// <summary>
		/// ���������� �������, �������������� �� ����������� ������ ��
		/// ������������ ������������������
		/// </summary>
		private void closingTimer_Tick(object sender, EventArgs e)
		{
			// ������������ ������...
			closingTimer.Stop();

			// ��������� �����
			Close();

			// ���������� ������ ������
			GC.Collect();
		}

		/// <summary>
		/// ���������� ������� "�������� ����� ������������ ������������������"
		/// </summary>
		private void BenchmarkForm_Load(object sender, EventArgs e)
		{
			// ������������� ��������������� ��������� ����� (������������ ������)
			dataCountLabel.Text = System.Convert.ToString(this.dataCount);
			eccCountLabel.Text = System.Convert.ToString(this.eccCount);

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();

			// ������� ����� ��������� ������...
			this.thrBenchmarkProcess = new Thread(new ThreadStart(Benchmark));

			//...����� ���� ��� ���...
			this.thrBenchmarkProcess.Name = "RecoveryStar.Benchmark()";

			//...� ��������� ���
			this.thrBenchmarkProcess.Start();

			// �������� ������ ���������� ����������� �����
			benchmarkTimer.Start();
		}

		#endregion Private Operations
	}
}