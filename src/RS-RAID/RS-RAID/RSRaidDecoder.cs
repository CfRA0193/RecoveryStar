/*----------------------------------------------------------------------+
 |  filename:   RSRaidDecoder.cs                                        |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    RAID-�������� ������� ����-�������� (16 bit, ����)      |
 +----------------------------------------------------------------------*/

using System;
using System.Threading;

namespace RecoveryStar
{
	/// <summary>
	/// ����� RAID-��������� �������� ����-��������
	/// </summary>
	public class RSRaidDecoder : RSRaidBase
	{
		#region Data

		/// <summary>
		/// ������ ��������� ��������� "������ ������� "FLog" ����������?"
		/// </summary>
		private bool[] FLogRowIsTrivial;

		/// <summary>
		/// ������ ���������� ������� ��������� ����� (��������� � ����)
		/// </summary>
		private int[] volList;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� �������� ��-���������
		/// </summary>
		public RSRaidDecoder()
		{
			// ������� ������ ������ ������ � ���������� ���� �����
			this.eGF16 = new GF16();
		}

		/// <summary>
		/// ������� ����������� ��������
		/// </summary>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="volList">������ ���������� ������� ��������� �����</param>
		public RSRaidDecoder(int dataCount, int eccCount, int[] volList)
		{
			// ��������� ������������ ������
			SetConfig(dataCount, eccCount, volList, (int)RSType.Cauchy);

			// ������� ������ ������ ������ � ���������� ���� �����
			this.eGF16 = new GF16();
		}

		/// <summary>
		/// ����������� ����������� ��������
		/// </summary>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="volList">������ ���������� ������� ��������� �����</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		public RSRaidDecoder(int dataCount, int eccCount, int[] volList, int codecType)
		{
			// ��������� ������������ ������
			SetConfig(dataCount, eccCount, volList, codecType);

			// ������� ������ ������ ������ � ���������� ���� �����
			this.eGF16 = new GF16();
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// ��������� ������������ ��������
		/// </summary>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="volList">������ ���������� ������� ��������� �����</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <returns>��������� ���� �������� ��������� ������������</returns>
		public bool SetConfig(int dataCount, int eccCount, int[] volList, int codecType)
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
				&&
				(volList.Length >= dataCount)
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

				// ���������� ������������� ���������� �������� �� ������ ������
				// ������� �� ���� ������������ �������
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

				this.iterOfSecondStage = (n * (((n - 1) * (n - 1)) + (n * n)));

				// �������� ������ ��� ������ ��������� ��������� "������ ������� "FLog" ����������?"
				this.FLogRowIsTrivial = new bool[dataCount];

				// ��������� ������ ��������� �����
				this.volList = volList;

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
		/// <param name="dataEccLog">�������������������� ������� ������ (������ + ecc)</param>
		/// <param name="data">�������� ������ (��������������� �������� ������)</param>
		/// <returns>��������� ���� ���������� ��������</returns>
		public bool Process(int[] dataEccLog, int[] data)
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
				AForge.Parallel.For(0, this.n, delegate(int i)
				                               	{
				                               		// ���� ������� ������ ������� �� �������� �����������, ���������� ���������
				                               		if(!this.FLogRowIsTrivial[i])
				                               		{
				                               			int mulSum = 0; // ����� ������������ ������ ������� �� �������
				                               			int i_n = i * this.n; // �������� � ������� �� ��������� i-�� ������

				                               			for(int j = 0; j < this.n; j++)
				                               			{
				                               				mulSum ^= GF16Exp[this.FLog[i_n + j] + dataEccLog[j]];
				                               			}

				                               			data[i] = mulSum;
				                               		}
				                               		else
				                               		{
				                               			data[i] = GF16Exp[dataEccLog[i]];
				                               		}
				                               	});
			}
			else
			{
				// ���������� ���������� ��������� ������� �� ������
				for(int i = 0; i < this.n; i++)
				{
					// ���� ������� ������ ������� �� �������� �����������, ���������� ���������
					if(!this.FLogRowIsTrivial[i])
					{
						int mulSum = 0; // ����� ������������ ������ ������� �� �������
						int i_n = i * this.n; // �������� � ������� �� ��������� i-�� ������

						for(int j = 0; j < this.n; j++)
						{
							mulSum ^= GF16Exp[this.FLog[i_n + j] + dataEccLog[j]];
						}

						data[i] = mulSum;
					}
					else
					{
						data[i] = GF16Exp[dataEccLog[i]];
					}
				}
			}

			return true;
		}

		#endregion Public Operations

		#region Private Operations

		/// <summary>
		/// ����� �������, �������� � "FLog", ������� ���������� ����������
		/// (������ ����������� ������ ����� �������������� ������ � ��� �������,
		/// ����� (-a) = (a), �.�. �� ������������� ��������� ������ ��������� ���������),
		/// ����� ����, ����������� ����� ���������� ������������ �������� (� ������
		/// ������ � �������� �����������, ������� ���� �� ��������� - ���� ������,
		/// ������� �������� � ������������ ���� �������������� ������������� ��� ������
		/// </summary>
		/// <returns>��������� ���� ���������� ��������</returns>
		private bool FInv()
		{
			// ��������� ������������� ��������� �������� �� ������� ���
			// ���������� ��������� ���������
			double allStageIter = this.iterOfFirstStage + this.iterOfSecondStage;
			int percOfFirstStage = (int)((100.0 * this.iterOfFirstStage) / allStageIter);
			int percOfSecondStage = (int)((100.0 * this.iterOfSecondStage) / allStageIter);

			// ������ ������ ������ �������� ���� �� ���� �������
			// (��� ������������ ��������)
			if(percOfSecondStage == 0)
			{
				percOfSecondStage = 1;
			}

			// ��������� �������� ������, ������� �������� �������� ������� ���������
			// ����� ��� ��������� ���������� ��� ����� �� "k"
			int progressMod1 = this.n / percOfSecondStage;

			// ���� ������ ����� ����, �� ����������� ��� �� �������� "1", �����
			// �������� ��������� �� ������ ��������
			if(progressMod1 == 0)
			{
				progressMod1 = 1;
			}

			// ���� ������ ������������ �������� "pivot"
			for(int k = 0; k < this.n; k++)
			{
				// ���� ������ ������ ����������� - ������ ��������� �� ����� ��������
				if(this.FLogRowIsTrivial[k])
				{
					continue;
				}

				// �������� � ������� �� ��������� k-�� ������
				int k_n = k * this.n;

				// ������ ������������ ��������
				int pivotIdx = k_n + k;

				// ��������� ����������� �������...
				int pivot = this.FLog[pivotIdx];

				// ���� ����������� ������� ����� ���� - ������� �� ����� ��������!
				if(pivot == 0)
				{
					//...��������� �� ������ ������������
					this.configIsOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return false;
				}

				// ����� ���������� ������������ �������� �������� �� ��� ����� "1"
				this.FLog[pivotIdx] = 1;

				// �������� �� �������� �� �����������...
				for(int i = 0; i < k; i++)
				{
					// �������� � ������� �� ��������� i-�� ������
					int i_n = i * this.n;

					// ��������� ������� [i,k]...
					int FLog_i_k = this.FLog[i_n + k];

					// �������� �� ���������...
					for(int j = 0; j < this.n; j++)
					{
						// ����������� :)
						int fIdx = i_n + j;

						// ���������� ��������� �������� ��� ��������: "A[i,j] = A[i,j] * pivot + A[i,k] * A[k,j]"
						this.FLog[fIdx] = this.eGF16.Mul(this.FLog[fIdx], pivot) ^ this.eGF16.Mul(FLog_i_k, this.FLog[k_n + j]);
					}

					// ��������������� ������� [i,k]...
					this.FLog[i_n + k] = FLog_i_k;
				}

				// �������� �� �������� ����� �����������...
				for(int i = (k + 1); i < this.n; i++)
				{
					// �������� � ������� �� ��������� i-�� ������
					int i_n = i * this.n;

					// ��������� ������� [i,k]...
					int FLog_i_k = this.FLog[i_n + k];

					// �������� �� ���������...
					for(int j = 0; j < this.n; j++)
					{
						// ����������� :)
						int fIdx = i_n + j;

						// ���������� ��������� �������� ��� ��������: "A[i,j] = A[i,j] * pivot + A[i,k] * A[k,j]"
						this.FLog[fIdx] = this.eGF16.Mul(this.FLog[fIdx], pivot) ^ this.eGF16.Mul(FLog_i_k, this.FLog[k_n + j]);
					}

					// ��������������� ������� [i,k]...
					this.FLog[i_n + k] = FLog_i_k;
				}

				// ������� ������� �� ����������� ������� �������� � ���������� �� ��������...
				int pivotInv = this.eGF16.Inv(pivot);

				for(int i = 0; i < (this.n * this.n); i++)
				{
					this.FLog[i] = this.eGF16.Mul(this.FLog[i], pivotInv);
				}

				// ���� ���� �������� �� �������� ���������� ��������� -...
				if(
					((k % progressMod1) == 0)
					&&
					(OnUpdateRSMatrixFormingProgress != null)
					)
				{
					//...������� ������
					OnUpdateRSMatrixFormingProgress((((double)(k + 1) / (double)this.n) * percOfSecondStage) + percOfFirstStage);
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
		/// ���������� ���������� �������� ��������������� �������
		/// <summary>
		private void LogFCalc()
		{
			for(int i = 0; i < (this.n * this.n); i++)
			{
				this.FLog[i] = this.eGF16.Log(this.FLog[i]);
			}
		}

		/// <summary>
		/// ���������� ������� "FLog" (������� ��������) �������
		/// </summary>
		protected override void FillFLog()
		{
			// ���� ����� ������� ��������� ����� ������ ����������,
			// ���������� ��� ��������������...
			if(this.volList.Length < this.n)
			{
				//...��������� �� ������ ������������
				this.configIsOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// �������� ������ ��� ������� "FLog"
			this.FLog = new int[this.n * this.n];

			// ������ ��������� ���� �����...
			int[] allVolCount = new int[this.n + this.m];

			//...� ������ ecc-����� ��� "���������" ��������, ���������
			// ���������� ��������� ������
			int[] eccVolToFix = new int[this.m];

			// ������� ���������� ������� �������� �����
			int dataVolMissCount = this.n;

			// �������������� ������ ��������� ���� �����
			for(int i = 0; i < (this.n + this.m); i++)
			{
				allVolCount[i] = 0;
			}

			// �������� ������ ������� �������������� ����� �� ������� ������� ��������
			for(int i = 0; i < this.n; i++)
			{
				// ��������� ����� �������� ����
				int currVol = Math.Abs(this.volList[i]);

				// ���� ����� ���� ������������� ����������� ���������
				if(currVol < (this.n + this.m))
				{
					++allVolCount[currVol];

					// ���� ������� ��� �������� ��������, ��������� ������ ����
					if(currVol < this.n)
					{
						--dataVolMissCount;
					}
				}
				else
				{
					// ��������� �� ������ ������������
					this.configIsOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return;
				}
			}

			// ��������� �������� ����� �� ��������� ������������
			for(int i = 0; i < (this.n + this.m); i++)
			{
				// ���� ��������� ��� ��� ������ ����� ��� ���� ���...
				if(allVolCount[i] > 1)
				{
					//...��������� �� ������ ������������
					this.configIsOK = false;

					// ���������� ��������� ����������� ��������� ����������-������
					this.finished = true;

					// ������������� ������� ���������� ���������
					this.finishedEvent[0].Set();

					return;
				}
			}

			// ���� �������� �� ������������������ �� ������� �������, ��������
			// ����������� ������� "FLog"

			// ���� �������� ������������ ����������...
			if(this.mainConfigChanged)
			{
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

							break;
						}

					default:
					case (int)RSType.Cauchy:
						{
							//...���������� ������������ ���������� ������� ���� "�"
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

							break;
						}
				}

				//...� ���������� ����
				this.mainConfigChanged = false;
			}

			// ��� ������� ���������� ��������� ���� ���� ��� ��� ��������������
			for(int i = 0, j = 0; i < dataVolMissCount; i++)
			{
				// �������� �� ������ ����� �� ��� ���, ���� �� ������ ��� ���
				// �������������� ��� ��������� "�����" (�������� ���� ����� ������
				// ������ this.n (��� ��������� � ����!))
				while(this.volList[j] < this.n)
				{
					j++;
				}

				// ��������� ����� ���� ��� ������ ���������� ��������� ����
				eccVolToFix[i] = this.volList[j];

				j++; // j++ ��������� ������� � ������������ ������
			}

			switch(this.eRSType)
			{
				case (int)RSType.Dispersal:
					{
						// �������� �� ������� ������� (� ������, ��� ������ ������ �����������
						// �������� � �������� �� ������� ���������, ��� ������������� ����������
						// �����������, �� allVolCount ������, ��� ������� ���� � �������� �����)
						for(int i = 0, e = 0; i < this.n; i++)
						{
							// ������ ������ �� ���������� �������, ������� ����� �������� � ������� �����������
							int DRowIdx;

							// �������� � ������� �� ��������� i-�� ������
							int i_n = i * this.n;

							// ���� �������� ��� �����������, ��������� ������ ������� �����������
							if(allVolCount[i] == 0)
							{
								// ��������� ����� ������ ������� �����������, ������� ����� ��������
								// �� ����� ������ ������ ����������� ������� "FLog"
								DRowIdx = eccVolToFix[e++];

								// ���������, ��� ������ ������ ������� "FLog" �� ����������
								this.FLogRowIsTrivial[i] = false;
							}
							else
							{
								// ��������� � ������� "FLog" ������� ������ � �������� �� ������� ���������
								// (������������� ���������� ��������� ����)
								DRowIdx = i;

								// ���������, ��� ������ ������ ������� "FLog" ����������
								this.FLogRowIsTrivial[i] = true;
							}

							// ����������� :)
							int bs = DRowIdx * this.n;

							// ������������ ������ � ������� �����������
							// ("�����������" ������ ��� ���������� � ������� "D", ��� ����������
							// "�������������" �� ���������� ����� ��������� MakeDispersal())
							for(int j = 0; j < this.n; j++)
							{
								this.FLog[i_n + j] = this.D[bs + j];
							}

							// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
							// ����� ��������, � ����� �� ����� ������ �� ��� ���������
							ManualResetEvent.WaitAll(this.executeEvent);

							// ���� �������, ��� ��������� ����� �� ������ - �������
							if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
							{
								//...��������� �� ������ ������������
								this.configIsOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
						}

						break;
					}

				case (int)RSType.Alternative:
					{
						// �������� �� ������� ������� (� ������, ��� ������ ������ �����������
						// �������� � �������� �� ������� ���������, ��� ������������� ����������
						// �����������, �� allVolCount ������, ��� ������� ���� � �������� �����)
						for(int i = 0, e = 0; i < this.n; i++)
						{
							// ������ ������ �� �������������� �������, ������� ����� �������� � ������� �����������
							int ARowIdx;

							// �������� � ������� �� ��������� i-�� ������
							int i_n = i * this.n;

							// ���� �������� ��� �����������, ��������� ������ ������� �����������
							if(allVolCount[i] == 0)
							{
								// ��������� ����� ������ �������������� �������, ������� ����� ��������
								// �� ����� ������ ������ ����������� ������� "FLog"
								ARowIdx = eccVolToFix[e++] - this.n;

								// ���������, ��� ������ ������ ������� "FLog" �� ����������
								this.FLogRowIsTrivial[i] = false;
							}
							else
							{
								// ��������� � ������� "FLog" ������� ������ � �������� �� ������� ���������
								// (������������� ���������� ��������� ����)
								ARowIdx = i;

								// ���������, ��� ������ ������ ������� "FLog" ����������
								this.FLogRowIsTrivial[i] = true;
							}

							// ���� ��� ��������� - ��������� "�����������" ������...
							if(this.FLogRowIsTrivial[i])
							{
								for(int j = 0; j < this.n; j++)
								{
									this.FLog[i_n + j] = 0;
								}

								this.FLog[i_n + i] = 1;
							}
							else
							{
								// ����������� :)
								int bs = ARowIdx * this.n;

								//...�, �����, ����� ������ ������� �����������
								for(int j = 0; j < this.n; j++)
								{
									this.FLog[i_n + j] = this.A[bs + j];
								}
							}

							// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
							// ����� ��������, � ����� �� ����� ������ �� ��� ���������
							ManualResetEvent.WaitAll(this.executeEvent);

							// ���� �������, ��� ��������� ����� �� ������ - �������
							if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
							{
								//...��������� �� ������ ������������
								this.configIsOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
						}

						break;
					}

				case (int)RSType.Cauchy:
					{
						// �������� �� ������� ������� (� ������, ��� ������ ������ �����������
						// �������� � �������� �� ������� ���������, ��� ������������� ����������
						// �����������, �� allVolCount ������, ��� ������� ���� � �������� �����)
						for(int i = 0, e = 0; i < this.n; i++)
						{
							// ������ ������ �� ������� ����, ������� ����� �������� � ������� �����������
							int CRowIdx;

							// �������� � ������� �� ��������� i-�� ������
							int i_n = i * this.n;

							// ���� �������� ��� �����������, ��������� ������ ������� �����������
							if(allVolCount[i] == 0)
							{
								// ��������� ����� ������ ������� ����, ������� ����� ��������
								// �� ����� ������ ������ ����������� ������� "FLog"
								CRowIdx = eccVolToFix[e++] - this.n;

								// ���������, ��� ������ ������ ������� "FLog" �� ����������
								this.FLogRowIsTrivial[i] = false;
							}
							else
							{
								// ��������� � ������� "FLog" ������� ������ � �������� �� ������� ���������
								// (������������� ���������� ��������� ����)
								CRowIdx = i;

								// ���������, ��� ������ ������ ������� "FLog" ����������
								this.FLogRowIsTrivial[i] = true;
							}

							// ���� ��� ��������� - ��������� "�����������" ������...
							if(this.FLogRowIsTrivial[i])
							{
								for(int j = 0; j < this.n; j++)
								{
									this.FLog[i_n + j] = 0;
								}

								this.FLog[i_n + i] = 1;
							}
							else
							{
								// ����������� :)
								int bs = CRowIdx * this.n;

								//...�, �����, ����� ������ ������� �����������
								for(int j = 0; j < this.n; j++)
								{
									this.FLog[i_n + j] = this.C[bs + j];
								}
							}

							// � ������, ���� ��������� ���������� �� �����, ������� "executeEvent"
							// ����� ��������, � ����� �� ����� ������ �� ��� ���������
							ManualResetEvent.WaitAll(this.executeEvent);

							// ���� �������, ��� ��������� ����� �� ������ - �������
							if(ManualResetEvent.WaitAll(this.exitEvent, 0, false))
							{
								//...��������� �� ������ ������������
								this.configIsOK = false;

								// ���������� ��������� ����������� ��������� ����������-������
								this.finished = true;

								// ������������� ������� ���������� ���������
								this.finishedEvent[0].Set();

								return;
							}
						}

						break;
					}
			}

			// ������� �������� ������� ��� "FLog"
			if(!FInv())
			{
				// ���������, ��� ����� �� ��������������� ���������
				this.configIsOK = false;

				// ���������� ��������� ����������� ��������� ����������-������
				this.finished = true;

				// ������������� ������� ���������� ���������
				this.finishedEvent[0].Set();

				return;
			}

			// ��������� ��������� ��������� ��������������� �������
			LogFCalc();

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

		#region Public Properties

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

		#endregion Public Properties
	}
}