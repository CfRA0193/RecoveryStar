/*----------------------------------------------------------------------+
 |  filename:   RSRaidEncoder.cs                                        |
 |----------------------------------------------------------------------|
 |  version:    2.20                                                    |
 |  revision:   23.05.2012 17:33                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    RAID-�������� ����� ����-�������� (16 bit, ����)        |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;

namespace RecoveryStar
{
	/// <summary>
	/// ����� RAID-��������� ������ ����-��������
	/// </summary>
	public class RSRaidEncoder : RSRaidBase
	{
		#region Construction & Destruction

		/// <summary>
		/// ����������� ������ ��-���������
		/// </summary>
		public RSRaidEncoder()
		{
			// ������� ������ ������ ������ � ���������� ���� �����
			this.eGF16 = new GF16();
		}

		/// <summary>
		/// ������� ����������� ������
		/// </summary>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		public RSRaidEncoder(int dataCount, int eccCount)
		{
			// ��������� ������������ ������
			SetConfig(dataCount, eccCount, (int)RSType.Cauchy);

			// ������� ������ ������ ������ � ���������� ���� �����
			this.eGF16 = new GF16();
		}

		/// <summary>
		/// ����������� ����������� ������
		/// </summary>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		public RSRaidEncoder(int dataCount, int eccCount, int codecType)
		{
			// ��������� ������������ ������
			SetConfig(dataCount, eccCount, codecType);

			// ������� ������ ������ ������ � ���������� ���� �����
			this.eGF16 = new GF16();
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// ��������� ������������ ������
		/// </summary>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <returns>��������� ���� �������� ��������� ������������</returns>
		public bool SetConfig(int dataCount, int eccCount, int codecType)
		{
			int maxVolCount;

			// ������������� ���������, ��������������� ���������� ������
			if(
				(codecType == (int)RSType.Dispersal)
				||
				(codecType == (int)RSType.Cauchy)
				)
			{
				maxVolCount = (int)RSConst.MaxVolCount;
			}
			else
			{
				maxVolCount = (int)RSConst.MaxVolCountAlt;
			}

			// ��������� ������������ �� ������������
			if(
				(dataCount > 0)
				&&
				(eccCount > 0)
				&&
				((dataCount + eccCount) <= maxVolCount)
				)
			{
				// ���� �������� ������������ ���������� - �������� �� ����
				if(
					(dataCount != this.n)
					||
					(eccCount != this.m)
					||
					(codecType != this.eRSType)
					)
				{
					this.mainConfigChanged = true;
				}

				// ��������� ������������
				this.n = dataCount;
				this.m = eccCount;
				this.eRSType = codecType;

				// ����� ������������� ���������� �������� ���� ������ ����������
				double n = this.n;
				double m = this.m;

				// ����������� �������� ��� �������, ����� �������� ������������ ����������
				NormalizeNM(ref n, ref m);

				// ���������� �������� �� ������ ������ ������� �� ���� ������������ �������
				if(
					(this.eRSType == (int)RSType.Alternative)
					||
					(this.eRSType == (int)RSType.Cauchy)
					)
				{
					this.iterOfFirstStage = m;
				}
				else
				{
					this.iterOfFirstStage = ((n * m) * n) + (n * ((n + m) + (n * (n + m))));
				}

				this.iterOfSecondStage = 0; // � ������ ��� �������������� �������

				this.configIsOK = true;
			}
			else
			{
				//...��������� �� ������ ������������
				this.configIsOK = false;
			}

			return this.configIsOK;
		}

		/// <summary>
		/// ����� ��������� ������� ����������� �� ������� �������������������� ������
		/// </summary>
		/// <param name="dataLog">�������������������� ������� ������ (�������� ������)</param>
		/// <param name="ecc">�������� ������ (���������� ������)</param>
		/// <returns>��������� ���� ���������� ��������</returns>
		public bool Process(int[] dataLog, int[] ecc)
		{
			// ���� ����� ��������������� �����������, ��������� ����������!
			if(!this.configIsOK)
			{
				return false;
			}

			// �������� ��������� �� ������ ��������� ��� ���������� ������� ���������
			int[] GF16Exp = this.eGF16.GFExpTable;

			// ���������� ����������� ������ � ��� ������, ����� ��� �������������
			if((this.m + this.n) >= (int)RSParallelEdge.Value)
			{
				// ���������� ���������� ��������� ������� �� ������
				AForge.Parallel.For(0, this.m, delegate(int i)
				                               	{
				                               		int mulSum = 0; // ����� ������������ ������ ������� �� �������
				                               		int i_n = i * this.n; // �������� � ������� �� ��������� i-�� ������

				                               		for(int j = 0; j < this.n; j++)
				                               		{
				                               			mulSum ^= GF16Exp[this.FLog[i_n + j] + dataLog[j]];
				                               		}

				                               		ecc[i] = mulSum;
				                               	});
			}
			else
			{
				// ���������� ���������� ��������� ������� �� ������
				for(int i = 0; i < this.m; i++)
				{
					int mulSum = 0; // ����� ������������ ������ ������� �� �������
					int i_n = i * this.n; // �������� � ������� �� ��������� i-�� ������

					for(int j = 0; j < this.n; j++)
					{
						mulSum ^= GF16Exp[this.FLog[i_n + j] + dataLog[j]];
					}

					ecc[i] = mulSum;
				}
			}

			return true;
		}

		#endregion Public Operations

		#region Private Operations

		/// <summary>
		/// ���������� ������� ����������� �������
		/// </summary>
		protected override void FillFLog()
		{
			// ���� �������� ������������ ����������...
			if(this.mainConfigChanged)
			{
				// �������� ������ ��� ������� "FLog"
				this.FLog = new int[this.m * this.n];

				switch(this.eRSType)
				{
					case (int)RSType.Dispersal:
						{
							//...���������� ������������ ���������� ������� "D"
							if(!MakeDispersalMatrix())
							{
								// ���������, ��� ����� �� ��������������� ���������
								this.configIsOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
							else
							{
								// ��������� ������� �����������
								for(int i = 0; i < this.m; i++)
								{
									// �������� � ������� �� ��������� i-�� ������
									int i_n = i * this.n;

									// ������������ ������ � ������� �����������
									for(int j = 0; j < this.n; j++)
									{
										// � ������� ����������� �������� ��������� � �������� ���������
										// (��� ��������� ��������� ������� �� ������)
										this.FLog[i_n + j] = this.eGF16.Log(this.D[((this.n + i) * this.n) + j]);
									}

									// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
									// ����� ��������, � ����� �� ����� ������ �� ��� ���������
									ManualResetEvent.WaitAll(this.executeEvent);

									// ���� �������, ��� ��������� ����� �� ������ - �������
									if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
									{
										// ���������, ��� ����� �� ��������������� ���������
										this.configIsOK = false;

										// ���������� ��������� ����������� ��������� ����������-������
										this.finished = true;

										// ������������� ������� ���������� ���������
										this.finishedEvent[0].Set();

										return;
									}
								}
							}

							break;
						}

					case (int)RSType.Alternative:
						{
							//...���������� ������������ ��������������� ���������� ������� "A"
							if(!MakeAlternativeMatrix())
							{
								// ���������, ��� ����� �� ��������������� ���������
								this.configIsOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
							else
							{
								// ��������� ������� �����������
								for(int i = 0; i < this.m; i++)
								{
									// �������� � ������� �� ��������� i-�� ������
									int i_n = i * this.n;

									// ������������ ������ � ������� �����������
									for(int j = 0; j < this.n; j++)
									{
										// ������ ����������� :)
										int idx = i_n + j;

										// � ������� ����������� �������� ��������� � �������� ���������
										// (��� ��������� ��������� ������� �� ������)
										this.FLog[idx] = this.eGF16.Log(this.A[idx]);
									}

									// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
									// ����� ��������, � ����� �� ����� ������ �� ��� ���������
									ManualResetEvent.WaitAll(this.executeEvent);

									// ���� �������, ��� ��������� ����� �� ������ - �������
									if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
									{
										// ���������, ��� ����� �� ��������������� ���������
										this.configIsOK = false;

										// ���������� ��������� ����������� ��������� ����������-������
										this.finished = true;

										// ������������� ������� ���������� ���������
										this.finishedEvent[0].Set();

										return;
									}
								}
							}

							break;
						}

					default:
					case (int)RSType.Cauchy:
						{
							//...���������� ������������ ������� ���� "C"
							if(!MakeCauchyMatrix())
							{
								// ���������, ��� ����� �� ��������������� ���������
								this.configIsOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
							else
							{
								// ��������� ������� �����������
								for(int i = 0; i < this.m; i++)
								{
									// �������� � ������� �� ��������� i-�� ������
									int i_n = i * this.n;

									// ������������ ������ � ������� �����������
									for(int j = 0; j < this.n; j++)
									{
										// ������ ����������� :)
										int idx = i_n + j;

										// � ������� ����������� �������� ��������� � �������� ���������
										// (��� ��������� ��������� ������� �� ������)
										this.FLog[idx] = this.eGF16.Log(this.C[idx]);
									}

									// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
									// ����� ��������, � ����� �� ����� ������ �� ��� ���������
									ManualResetEvent.WaitAll(this.executeEvent);

									// ���� �������, ��� ��������� ����� �� ������ - �������
									if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
									{
										// ���������, ��� ����� �� ��������������� ���������
										this.configIsOK = false;

										// ���������� ��������� ����������� ��������� ����������-������
										this.finished = true;

										// ������������� ������� ���������� ���������
										this.finishedEvent[0].Set();

										return;
									}
								}
							}

							break;
						}
				}

				// ���� ���� �������� �� �������� ����������...
				if(OnRSMatrixFormingFinish != null)
				{
					//...��������, ��� ��������� ������ ����� � ������
					OnRSMatrixFormingFinish();
				}

				//...� ���������� ����
				this.mainConfigChanged = false;
			}

			// ���� ���� �������� �� �������� ����������...
			if(OnRSMatrixFormingFinish != null)
			{
				//...��������, ��� ��������� ������ ����� � ������
				OnRSMatrixFormingFinish();
			}

			// ���������� ��������� ����������� ��������� ����������-������
			this.finished = true;

			// ������������� ������� ���������� ���������
			this.finishedEvent[0].Set();
		}

		#endregion Private Operations
	}
}