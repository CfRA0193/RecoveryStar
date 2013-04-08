/*----------------------------------------------------------------------+
 |  filename:   CRC-64.cs                                               |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    �������� ����������� ������ � �������������� CRC-64     |
 +----------------------------------------------------------------------*/

using System;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ������� CRC-64
	/// </summary>
	public class CRC64
	{
		#region Constants

		/// <summary>
		/// ������������ ������� ��� CRC64
		/// </summary>
		private const UInt64 crc64GenPoly = 0xC96C5795D7870F42;

		/// <summary>
		/// ������ ������� ��� ������� CRC64
		/// </summary>
		private const UInt64 crc64TableSize = 256;

		/// <summary>
		/// ��������� �������� CRC64
		/// </summary>
		private const UInt64 crc64Init = 0xFFFFFFFFFFFFFFFF;

		#endregion Constants

		#region Public Properties & Data

		/// <summary>
		/// ������� ��� ������� CRC64
		/// </summary>
		private readonly UInt64[] crc64Table = new UInt64[crc64TableSize];

		/// <summary>
		/// �������� CRC64
		/// </summary>
		private UInt64 crc64value = crc64Init;

		/// <summary>
		/// �������� CRC64
		/// </summary>
		public UInt64 Value
		{
			get { return this.crc64value; }
		}

		/// <summary>
		/// ���������� ������������ ����
		/// </summary>
		private Int64 length = 0;

		/// <summary>
		/// ���������� ������������ ����
		/// </summary>
		public Int64 Length
		{
			get { return this.length; }
		}

		#endregion Public Properties & Data

		#region Construction

		/// <summary>
		/// ����������� ������
		/// </summary>
		public CRC64()
		{
			// ���������� ������� CRC64
			for(UInt64 i = 0; i < crc64TableSize; i++)
			{
				UInt64 c = i;

				for(int j = 0; j < 8; j++)
				{
					c = ((c & 0x0000000000000001) == 0) ? (c >> 1) : (c >> 1) ^ crc64GenPoly;
				}

				// ����� ������������ �������� � ������
				this.crc64Table[i] = c;
			}
		}

		#endregion Construction

		#region Public Operations

		/// <summary>
		/// ��������� CRC64
		/// </summary>
		/// <param name="buffer">������ �������� ������</param>
		/// <param name="length">����� ������� ��� ���������� CRC64</param>
		public void Calculate(byte[] buffer, int length)
		{
			UInt64 c = this.crc64value; // !

			for(int i = 0; i < length; i++)
			{
				c = (c >> 8) ^ this.crc64Table[(0x00000000000000FF & c) ^ buffer[i]];
			}

			this.crc64value = c;
			this.length += length;
		}

		/// <summary>
		/// ��������� CRC64
		/// </summary>
		/// <param name="buffer">������ �������� ������</param>
		/// <param name="offset">�������� � ������� �������� ������</param>
		/// <param name="length">����� ������� ��� ���������� CRC64</param>
		public void Calculate(byte[] buffer, int offset, int length)
		{
			UInt64 c = this.crc64value; // !

			for(int i = offset; i < (offset + length); i++)
			{
				c = (c >> 8) ^ this.crc64Table[(0x00000000000000FF & c) ^ buffer[i]];
			}

			this.crc64value = c;
			this.length += length;
		}

		/// <summary>
		/// ����� � ��������� ��������
		/// </summary>
		public void Reset()
		{
			this.crc64value = crc64Init;
			this.length = 0;
		}

		/// <summary>
		/// ������� �������� CRC64 (UInt64 - ulong) ��� �������� ������ (8 ����)
		/// </summary>
		/// <returns>� ������� �������� ����� ������ - �� �������� ������� (0) � �������� (7)</returns>
		public byte[] GetCRC64Bytes()
		{
			return DataConverter.GetBytes(this.crc64value);
		}

		#endregion Public Operations
	}
}