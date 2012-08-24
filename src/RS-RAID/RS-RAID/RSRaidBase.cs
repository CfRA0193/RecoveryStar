/*----------------------------------------------------------------------+
 |  filename:   RSRaidBase.cs                                           |
 |----------------------------------------------------------------------|
 |  version:    2.21                                                    |
 |  revision:   24.08.2012 15:52                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ������� ����� ������ ����-�������� (16 bit, ����)       |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ������� ����� RAID-��������� ������ ����-��������
	/// </summary>
	public abstract class RSRaidBase
	{
		#region Delegates

		/// <summary>
		/// ������� ���������� �������� ������������ ������� "FLog"
		/// </summary>
		public OnUpdateDoubleValueHandler OnUpdateRSMatrixFormingProgress;

		/// <summary>
		/// ������� ���������� �������� ������������ ������� "FLog"
		/// </summary>
		public OnEventHandler OnRSMatrixFormingFinish;

		#endregion Delegates

		#region Public Properties & Data

		/// <summary>
		/// ��������� ������ ����� ����������?
		/// </summary>
		public bool InProcessing
		{
			get
			{
				if(
					(this.thrRSMatrixForming != null)
					&&
					(
						(this.thrRSMatrixForming.ThreadState == ThreadState.Running)
						||
						(this.thrRSMatrixForming.ThreadState == ThreadState.WaitSleepJoin)
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
		/// ��������� ������ ��������������� ���������?
		/// </summary>
		public bool ConfigIsOK
		{
			get
			{
				if(!InProcessing)
				{
					return this.configIsOK;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// ��������� ������ ��������������� ��������� (�������� � ������)?
		/// </summary>
		protected bool configIsOK;

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
		protected bool finished;

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		public int DataCount
		{
			get
			{
				if(!InProcessing)
				{
					return this.n;
				}
				else
				{
					return -1;
				}
			}
		}

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		protected int n;

		/// <summary>
		/// ���������� ����� ��� ��������������
		/// </summary>
		public int EccCount
		{
			get
			{
				if(!InProcessing)
				{
					return this.m;
				}
				else
				{
					return -1;
				}
			}
		}

		/// <summary>
		/// ���������� ����� ��� ��������������
		/// </summary>
		protected int m;

		/// <summary>
		/// ��� ������ (�� ���� ������������ �������)
		/// </summary>
		public int CodecType
		{
			get
			{
				if(!InProcessing)
				{
					return this.eRSType;
				}
				else
				{
					return -1;
				}
			}
		}

		/// <summary>
		/// ��� ������ ����-�������� (�� ���� ������������ ������� �����������)
		/// </summary>
		protected int eRSType;

		/// <summary>
		/// ��������� ��������
		/// </summary>
		public int ThreadPriority
		{
			get { return (int)this.threadPriority; }

			set
			{
				if(
					(this.thrRSMatrixForming != null)
					&&
					(this.thrRSMatrixForming.IsAlive)
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
					this.thrRSMatrixForming.Priority = this.threadPriority;
				}
			}
		}

		/// <summary>
		/// ��������� �������� ���������� ������� �����������
		/// </summary>
		protected ThreadPriority threadPriority;

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
		protected ManualResetEvent[] finishedEvent;

		#endregion Public Properties & Data

		#region Data

		/// <summary>
		/// ������ ������ ������ � ���������� ���� �����
		/// </summary>
		protected GF16 eGF16;

		/// <summary>
		/// ������� RAID-��������� ������ ����-��������
		/// </summary>
		protected int[] FLog;

		/// <summary>
		/// ���������� �������
		/// </summary>
		protected int[] D;

		/// <summary>
		/// "��������������" �������
		/// </summary>
		protected int[] A;

		/// <summary>
		/// ������� ����
		/// </summary>
		protected int[] C;

		/// <summary>
		/// �������� ������������ ���������?
		/// </summary>
		protected bool mainConfigChanged;

		/// <summary>
		/// ���������� �������� ������ ������ ���������� ������� �����������
		/// </summary>
		protected double iterOfFirstStage;

		/// <summary>
		/// ���������� �������� ������ ������ ���������� ������� �����������
		/// </summary>
		protected double iterOfSecondStage;

		/// <summary>
		/// ����� ���������� ������� "FLog" ����� ����������� ����������� / �������������
		/// </summary>
		protected Thread thrRSMatrixForming;

		/// <summary>
		/// ������� ����������� ���������� ������� �����������
		/// </summary>
		protected ManualResetEvent[] exitEvent;

		/// <summary>
		/// ������� ����������� ���������� ������� �����������
		/// </summary>
		protected ManualResetEvent[] executeEvent;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� �������� ������ �������� "RAID-�������� ����� ����-��������"
		/// </summary>
		public RSRaidBase()
		{
			// ������� ��������� ������ ��� ������ � ����������� ���� ����� (2^16)
			this.eGF16 = new GF16();

			// �������� ������ ��������� �������� ���������?
			this.finished = true;

			// �������� ������������ ���������?
			this.mainConfigChanged = true;

			// ��������� ������ ��������������� ��������� (�������� � ������)?
			this.configIsOK = false;

			// ��-��������� ��������������� ������� ���������
			this.threadPriority = 0;

			// �������������� ������� ����������� ��������� �����
			this.exitEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������������� c������ ����������� ��������� �����
			this.executeEvent = new ManualResetEvent[] {new ManualResetEvent(false)};

			// �������, ��������������� �� ���������� ���������
			this.finishedEvent = new ManualResetEvent[] {new ManualResetEvent(true)};
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// ������ �������� ���������� ������� "FLog" �������
		/// </summary>
		/// <param name="runAsSeparateThread">��������� � ��������� ������?</param>
		/// <returns>��������� ���� ��������</returns>
		public bool Prepare(bool runAsSeparateThread)
		{
			// ���� ����� ������������ ������� "FLog" �������� - �� ��������� ��������� ������
			if(InProcessing)
			{
				return false;
			}

			// ���� ������������ ����������� ����������� - �������
			if(!this.configIsOK)
			{
				return false;
			}

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = false;

			// ���������� ������� ���������� ���������
			this.finishedEvent[0].Reset();

			// ���������, ��� ����� ������ �����������
			this.exitEvent[0].Reset();
			this.executeEvent[0].Set();

			// ���� �������, ��� �� ��������� ������ � ��������� ������,
			// ��������� � ������
			if(!runAsSeparateThread)
			{
				// ��������� ������� �����������
				FillFLog();

				// ���������� ��������� ���������
				return this.configIsOK;
			}

			// ������� ����� ������������ ������� "FLog"...
			this.thrRSMatrixForming = new Thread(new ThreadStart(FillFLog));

			//...����� ���� ��� ���...
			this.thrRSMatrixForming.Name = "RSRaid.FillFLog()";

			//...������������� ��������� ��������� ������...
			this.thrRSMatrixForming.Priority = this.threadPriority;

			//...� ���������
			this.thrRSMatrixForming.Start();

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

		#region Protected Operations

		/// <summary>
		/// ������������ �������� "n" � "m" c ����� �������������� ������������ ����������,
		/// �������� ����� ���������� ��������
		/// </summary>
		protected void NormalizeNM(ref double n, ref double m)
		{
			double maxVal = 0;

			if(n > m)
			{
				maxVal = n;
			}
			else
			{
				maxVal = m;
			}

			double divider = maxVal / 100.0;

			if(divider > 1)
			{
				n /= divider;
				m /= divider;
			}
		}

		/// <summary>
		/// ����� ������ ������� ������,
		/// </summary>
		/// <param name="rowNum">����� ������</param>
		/// <returns>������ ������, ��������� ��� ������</returns>
		protected int FindSwapRow(int rowNum)
		{
			// ��������� �� ���� ��������� ������� �������
			// � ��������� �������
			for(int i = rowNum; i < (this.n + this.m); i++)
			{
				if(this.D[(i * this.n) + rowNum] != 0)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// ����� ������������ ���� ����� �������
		/// </summary>
		/// <param name="rowNum1">������ ������ ������</param>
		/// <param name="rowNum2">������ ������ ������</param>
		protected void SwapRows(int rowNum1, int rowNum2)
		{
			// ��������� �������� �� ��������� i-�� ������
			int rowNum1this_n = rowNum1 * this.n;
			int rowNum2this_n = rowNum2 * this.n;

			for(int j = 0; j < this.n; j++)
			{
				int dIdx1 = rowNum1this_n + j;
				int dIdx2 = rowNum2this_n + j;

				int tmp = this.D[dIdx1];
				this.D[dIdx1] = this.D[dIdx2];
				this.D[dIdx2] = tmp;
			}
		}

		/// <summary>
		/// ����� ��������� ���������� ������� "D"
		/// </summary>
		/// <returns>��������� ���� ���������� ��������</returns>
		protected bool MakeDispersalMatrix()
		{
			// �������� ������ ��� ������� "FLog"
			this.D = new int[(this.n + this.m) * this.n];

			// ��������� ������� ������� (��������� ������� �����������)
			for(int i = 0; i < (this.n + this.m); i++)
			{
				// �������� � ������� �� ��������� i-�� ������
				int i_n = i * this.n;

				// ���������� ������ ������� ����������� (���� ���� ����������
				// ����� ���� ���������� � ��� ������������� ������� ����������
				// �������� � �������, �� ������� ���������� ������������ �������
				// �������� � ����������)
				for(int j = 0; j < this.n; j++)
				{
					this.D[i_n + j] = this.eGF16.Pow(i, j);
				}
			}

			// ��������� ������������� ��������� �������� �� ������� ���
			// ���������� ��������� ���������
			double allStageIter = this.iterOfFirstStage + this.iterOfSecondStage;
			int percOfFirstStage = (int)((100.0 * this.iterOfFirstStage) / allStageIter);

			// ������ ������ ������ �������� ���� �� ���� �������
			// (��� ������������ ��������)
			if(percOfFirstStage == 0)
			{
				percOfFirstStage = 1;
			}

			// ��������� �������� ������, ������� �������� �������� ������� ���������
			// ����� ��� ��������� ���������� ��� ����� �� "i"
			int progressMod1 = this.n / percOfFirstStage;

			// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
			// �������� ��������� �� ������ ��������
			if(progressMod1 == 0)
			{
				progressMod1 = 1;
			}

			// ���� ������ ������������� ��������
			for(int k = 1; k < this.n; k++)
			{
				// ���� ������, � ������� ������� �� �������
				// ��������� ��� �� ���� ���������
				int swapIdx = FindSwapRow(k);

				// ���� ���������� ������ �� ����� ���� ������� -
				// ��� ������ - ...
				if(swapIdx == -1)
				{
					//...��������� �� ������ ������������
					this.configIsOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return false;
				}

				// ���� ���� ������� ������, �������� �� �������...
				if(swapIdx != k)
				{
					//...������ ������ �������
					SwapRows(swapIdx, k);
				}

				int k_n = k * this.n;

				// ��������� ������������ �������
				int diagElem = this.D[k_n + k];

				// ���� ������������ ������� �� ����� "1", �������� ���� �������
				// �� �������� ��� �������, ��������� ������������ � "1"
				if(diagElem != 1)
				{
					// ��������� �������� ������� ��� "diagElem"
					int diagElemInv = this.eGF16.Inv(diagElem);

					// ���������� ��������� ��������� ��������� ������� -
					// �������� ��� �� �������, �������� "diagElem"
					for(int i = k; i < (this.n + this.m); i++)
					{
						int dIdx = (i * this.n) + k;

						this.D[dIdx] = this.eGF16.Mul(this.D[dIdx], diagElemInv);
					}
				}

				// ��� ���� ��������...
				for(int j = 0; j < this.n; j++)
				{
					// ��������� ��������� �������� �������
					int colMult = this.D[k_n + j];

					//...�� ���������� ��������� ������������ ��������...
					if(
						(j != k)
						&&
						(colMult != 0)
						)
					{
						for(int i = k; i < (this.n + this.m); i++)
						{
							int i_n = i * this.n;
							int dIdx = i_n + j;

							//...���������� ������ Cj = Cj - Dk,j * Ck
							this.D[dIdx] = this.D[dIdx] ^ this.eGF16.Mul(colMult, this.D[i_n + k]);
						}
					}
				}

				// ���� ���� �������� �� �������� ���������� ��������� -...
				if(
					((k % progressMod1) == 0)
					&&
					(OnUpdateRSMatrixFormingProgress != null)
					)
				{
					//...������� ������
					OnUpdateRSMatrixFormingProgress(((double)(k + 1) / (double)this.n) * percOfFirstStage);
				}

				// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
				// ����� ��������, � ����� �� ����� ������ �� ��� ���������
				ManualResetEvent.WaitAll(this.executeEvent);

				// ���� �������, ��� ��������� ����� �� ������ - �������
				if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
				{
					// ���������, ��� ������� �� ��������������� ���������
					this.configIsOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// ����� ��������� "��������������" ������� "A" : � ��� ��� ���������� ������� �����������
		/// �� 65535 �������� ���������� 32768, �����, ����� �������� ������ �� ��� ��� �������
		/// ������� �� ��������� "65535", �.�. ����� �� ��� (���������� ����� ��������) ��� �����
		/// "1". ��������� ����� ������� �������� � ������������� ��������� ������� �����������,
		/// �, ��������������, � ������������� �������������� ������)
		/// </summary>
		/// <returns>��������� ���� ���������� ��������</returns>
		protected bool MakeAlternativeMatrix()
		{
			// ������������ �������� ���������, � ����� ����������� ��������� ���������
			// ��� ��������� � ������� ����� ��� ��������������
			int logBase = 0;

			// ����������������� �� "logBase" ��������� ������� ��� ������������ ������
			// ������� �����������
			int powBase = 0;

			// �������� ������ ��� ������� "FLog"
			this.A = new int[this.m * this.n];

			// ��������� ������������� ��������� �������� �� ������� ���
			// ���������� ��������� ���������
			double allStageIter = this.iterOfFirstStage + this.iterOfSecondStage;
			int percOfFirstStage = (int)((100.0 * this.iterOfFirstStage) / allStageIter);

			// ������ ������ ������ �������� ���� �� ���� �������
			// (��� ������������ ��������)
			if(percOfFirstStage == 0)
			{
				percOfFirstStage = 1;
			}

			// ��������� �������� ������, ������� �������� �������� ������� ���������
			// ����� ��� ��������� ���������� ��� ����� �� "i"
			int progressMod1 = this.m / percOfFirstStage;

			// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
			// �������� ��������� �� ������ ��������
			if(progressMod1 == 0)
			{
				progressMod1 = 1;
			}

			// ��������� ������� ������� (��������� ������� �����������)
			for(int i = 0; i < this.m; i++)
			{
				// ���� "logBase" �� ������� ������ � "65535"...
				while(
					((logBase % 3) == 0)
					||
					((logBase % 5) == 0)
					||
					((logBase % 17) == 0)
					||
					((logBase % 257) == 0)
					)
				{
					++logBase;
				}

				//...�����, ��������������� ��� ��������...
				powBase = this.eGF16.Exp(logBase++);

				// �������� � ������� �� ��������� i-�� ������
				int i_n = i * this.n;

				for(int j = 0; j < this.n; j++)
				{
					//...� ���������� ��� ������������ ������ ������� �����������
					this.A[i_n + j] = this.eGF16.Pow(powBase, j);
				}

				// ���� ���� �������� �� �������� ���������� ��������� -...
				if(
					((i % progressMod1) == 0)
					&&
					(OnUpdateRSMatrixFormingProgress != null)
					)
				{
					//...������� ������
					OnUpdateRSMatrixFormingProgress(((double)(i + 1) / (double)this.m) * percOfFirstStage);
				}

				// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
				// ����� ��������, � ����� �� ����� ������ �� ��� ���������
				ManualResetEvent.WaitAll(this.executeEvent);

				// ���� �������, ��� ��������� ����� �� ������ - �������
				if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
				{
					// ���������, ��� ������� �� ��������������� ���������
					this.configIsOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// ����� ��������� ������� ����
		/// </summary>
		/// <returns>��������� ���� ���������� ��������</returns>
		protected bool MakeCauchyMatrix()
		{
			// �������� ������ ��� ������� "FLog"
			this.C = new int[this.m * this.n];

			// ��������� ������������� ��������� �������� �� ������� ���
			// ���������� ��������� ���������
			double allStageIter = this.iterOfFirstStage + this.iterOfSecondStage;
			int percOfFirstStage = (int)((100.0 * this.iterOfFirstStage) / allStageIter);

			// ������ ������ ������ �������� ���� �� ���� �������
			// (��� ������������ ��������)
			if(percOfFirstStage == 0)
			{
				percOfFirstStage = 1;
			}

			// ��������� �������� ������, ������� �������� �������� ������� ���������
			// ����� ��� ��������� ���������� ��� ����� �� "i"
			int progressMod1 = this.m / percOfFirstStage;

			// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
			// �������� ��������� �� ������ ��������
			if(progressMod1 == 0)
			{
				progressMod1 = 1;
			}

			// ��������� ������� ������� (��������� ������� ����)
			for(int i = 0; i < this.m; i++)
			{
				// �������� � ������� �� ��������� i-�� ������
				int i_n = i * this.n;

				// ����������� :)
				int i_pl_n = i + this.n;

				for(int j = 0; j < this.n; j++)
				{
					// ��������� ������ ������� ����...
					this.C[i_n + j] = this.eGF16.Inv(this.eGF16.Exp(i_pl_n) ^ this.eGF16.Exp(j));
				}

				// ���� ���� �������� �� �������� ���������� ��������� -...
				if(
					((i % progressMod1) == 0)
					&&
					(OnUpdateRSMatrixFormingProgress != null)
					)
				{
					//...������� ������
					OnUpdateRSMatrixFormingProgress(((double)(i + 1) / (double)this.m) * percOfFirstStage);
				}

				// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
				// ����� ��������, � ����� �� ����� ������ �� ��� ���������
				ManualResetEvent.WaitAll(this.executeEvent);

				// ���� �������, ��� ��������� ����� �� ������ - �������
				if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
				{
					// ���������, ��� ������� �� ��������������� ���������
					this.configIsOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// ���������� ������� "FLog" �������
		/// </summary>
		protected virtual void FillFLog()
		{
		}

		#endregion Protected Operations
	}
}