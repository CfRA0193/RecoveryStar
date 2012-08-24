/*----------------------------------------------------------------------+
 |  filename:   GF16.cs                                                 |
 |----------------------------------------------------------------------|
 |  version:    2.21                                                    |
 |  revision:   24.08.2012 15:52                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ����� ���������� ���� ����� (16 bit)                    |
 +----------------------------------------------------------------------*/

using System;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ���� �����
	/// </summary>
	public class GF16
	{
		#region Constants

		/// <summary>
		/// ������������ ����������� ������� GF(2^16)
		/// </summary>
		private const int RSPrimPoly = 0x1100B;

		/// <summary>
		/// ������� ���� �����
		/// </summary>
		private const int GFPower = 16;

		/// <summary>
		/// ������ ���� �����
		/// </summary>
		private const int GFSize = ((1 << GFPower) - 1);

		#endregion Constants

		#region Public Properties & Data

		/// <summary>
		/// ������� ����������� ���������������� GF(2^16)
		/// </summary>
		public int[] GFLogTable
		{
			get { return this.GFLog; }
		}

		/// <summary>
		/// ������� ����������� �������������� GF(2^16)
		/// </summary>
		public int[] GFExpTable
		{
			get { return this.GFExp; }
		}

		#endregion Public Properties & Data

		#region Data

		/// <summary>
		/// ������� "����������������"
		/// </summary>
		private int[] GFLog;

		/// <summary>
		/// ������� "��������������"
		/// </summary>
		private int[] GFExp;

		#endregion Data

		#region Construction & Destruction

		public GF16()
		{
			// �������������� ������� "����������������" � "��������������"
			GFInit();
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// �������� ��������� ���� �����
		/// </summary>
		public int Add(int a, int b)
		{
			return a ^ b;
		}

		/// <summary>
		/// ��������� ��������� ���� �����
		/// </summary>
		public int Sub(int a, int b)
		{
			return a ^ b;
		}

		/// <summary>
		/// ���������������� ��������� ��������� ���� ����� (��� �������� ���������� �� ����)
		/// </summary>
		public int Mul(int a, int b)
		{
			return this.GFExp[this.GFLog[a] + this.GFLog[b]];
		}

		/// <summary>
		/// ���������������� ������� ��������� ���� ����� (��� �������� ������� ��������� �� ����)
		/// </summary>
		public int Div(int a, int b)
		{
			// �� ���� ������ ������!
			if(b == 0)
			{
				return -1;
			}

			// ��������� "+this.GFSize" ����������� ��������������� �������� �������
			return this.GFExp[this.GFLog[a] - this.GFLog[b] + GFSize];
		}

		/// <summary>
		/// ���������� � ������� �������� ���� �����
		/// </summary>
		public int Pow(int a, int p)
		{
			// ���� ���������� ������� ����� "0", �� ��������� - "1"
			if(p == 0)
			{
				return 1;
			}

			// ���� ��������� ������� ����� "0", �� ��������� - "0"
			if(a == 0)
			{
				return 0;
			}

			// ������� ����� ����� ���� ������������ ��� ������������
			// ��������� ��������� � ���������� ������� (� ����������� ���������������)
			int pow = this.GFLog[a] * p;

			// �������� ��������� � �������� ���� (������� ����� ���������� � ��������)
			// � ���������� �������� ����������
			return this.GFExp[((pow >> GFPower) & GFSize) + (pow & GFSize)];
		}

		/// <summary>
		/// ���������� ��������� �������� ���� �����
		/// </summary>
		public int Inv(int a)
		{
			// �� ���� ������ ������!
			if(a == 0)
			{
				return -1;
			}

			return this.GFExp[GFSize - this.GFLog[a]];
		}

		/// <summary>
		/// ���������� ��������� �������� ���� �����
		/// </summary>
		public int Log(int a)
		{
			return this.GFLog[a];
		}

		/// <summary>
		/// ���������� ���������� �������� ���� �����
		/// </summary>
		public int Exp(int a)
		{
			return this.GFExp[a];
		}

		#endregion Public Operations

		#region Private Operations

		/// <summary>
		/// ������������� ������ "����������������" � "��������������"
		/// </summary>
		private void GFInit()
		{
			// ������� "����������������"
			this.GFLog = new int[GFSize + 1];

			// ������� "��������������"
			this.GFExp = new int[(4 * GFSize) + 1];

			// �������� ���� ������ �����, ����� �������� ��������� �� �������
			// ������� ������� ������� ���������, ����, ��� ����������� ���� ((0 * 0) = 0)
			this.GFLog[0] = (2 * GFSize);

			// ��������� ������� ���������������� � ��������������
			for(int log = 0, b = 1; log < GFSize; ++log)
			{
				this.GFLog[b] = log;
				this.GFExp[log] = b;
				this.GFExp[log + GFSize] = b; // �������������� ����� ������� ���������
				// �������� ���������� � ������� ���� �����
				// ������������ ����������� ����������������

				// ��������� �������� �������� ����, ��� �������� �������� �������
				b <<= 1;

				// ���� ����� �� ������� ���� GF(2^16), �������� �������� � ����
				if(b > GFSize)
				{
					b ^= RSPrimPoly;
				}
			}

			// ��������� ������ ����� ������� ������ (����� ��� �����������
			// ���������, �������� ���� ������ �����, ����� �������� ��������� �� �������
			// ������� ������� ������� ���������, ����, ��� ����������� ���� ((0 * 0) = 0)
			for(int i = (2 * GFSize); i < ((4 * GFSize) + 1); i++)
			{
				this.GFExp[i] = 0;
			}
		}

		#endregion Private Operations
	}
}