/*----------------------------------------------------------------------+
 |  filename:   DataConverter.cs                                        |
 |----------------------------------------------------------------------|
 |  version:    2.20                                                    |
 |  revision:   23.05.2012 17:33                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ��������� ������                                        |
 +----------------------------------------------------------------------*/

using System;

namespace RecoveryStar
{
	/// <summary>
	/// ��������� ��������� ����� ������
	/// </summary>
	public static class DataConverter
	{
		/// <summary>
		/// ������������� UInt64 (ulong) � ������ �� 8 ����
		/// </summary>
		/// <returns>� ������� �������� ����� ������ - �� �������� ������� (0) � �������� (7)</returns>
		public static byte[] GetBytes(UInt64 value)
		{
			byte[] bytes = new byte[8];

			for(int i = 0; i < 8; i++)
			{
				bytes[i] = (byte)(0x00000000000000FF & (value >> (i << 3)));
			}

			return bytes;
		}

		/// <summary>
		/// ������������� Int64 (long) � ������ �� 8 ����
		/// </summary>
		/// <returns>� ������� �������� ����� ������ - �� �������� ������� (0) � �������� (7)</returns>
		public static byte[] GetBytes(Int64 value)
		{
			byte[] bytes = new byte[8];

			for(int i = 0; i < 8; i++)
			{
				bytes[i] = (byte)(0x00000000000000FF & (value >> (i << 3)));
			}

			return bytes;
		}

		/// <summary>
		/// ������������� ������ �� 8 ���� � UInt64 (ulong)
		/// </summary>
		/// <param name="bytes">� ������� �������� ����� ������ - �� �������� ������� (0) � �������� (7)</param>
		public static UInt64 GetUInt64(byte[] bytes)
		{
			UInt64 value = 0x0000000000000000;

			for(int i = 0; i < 8; i++)
			{
				value |= ((UInt64)bytes[i]) << (i << 3);
			}

			return value;
		}

		/// <summary>
		/// ������������� ������ �� 8 ���� � Int64 (long)
		/// </summary>
		/// <param name="bytes">� ������� �������� ����� ������ - �� �������� ������� (0) � �������� (7)</param>
		public static Int64 GetInt64(byte[] bytes)
		{
			Int64 value = 0x0000000000000000;

			for(int i = 0; i < 8; i++)
			{
				value |= ((Int64)bytes[i]) << (i << 3);
			}

			return value;
		}
	}
}