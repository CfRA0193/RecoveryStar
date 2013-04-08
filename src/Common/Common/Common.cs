/*----------------------------------------------------------------------+
 |  filename:   Common.cs                                               |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ����� �������� � ����������� ������� Recovery Star      |
 +----------------------------------------------------------------------*/

using System;

namespace RecoveryStar
{
	/// <summary>
	/// ������� ���������� ���������� ��������
	/// </summary>
	public delegate void OnUpdateStringValueHandler(String text);

	/// <summary>
	/// ������� ���������� ��������� ��������
	/// </summary>
	public delegate void OnUpdateDoubleValueHandler(double value);

	/// <summary>
	/// ������� ������ ���������� �������� ������ � ��������
	/// </summary>
	public delegate void OnUpdateStringAndDoubleValueHandler(String text, double value);

	/// <summary>
	/// ������� ������ ���� �������� ���� double
	/// </summary>
	public delegate void OnUpdateTwoDoubleValueHandler(double value1, double value2);

	/// <summary>
	/// ������� ������ ���� �������� ���� int � double
	/// </summary>
	public delegate void OnUpdateTwoIntDoubleValueHandler(int intValue1, int intValue2, double doubleValue1, double doubleValue2);

	/// <summary>
	/// ������� ��� ����������
	/// </summary>
	public delegate void OnEventHandler();

	/// <summary>
	/// ������������ ��������� ������ �������� ���������� ������ (� ������)
	/// </summary>
	public enum WaitCount
	{
		MinWaitCount = 600,
		MaxWaitCount = 6000
	};

	/// <summary>
	/// ������������ ��������� ������ ��������� ������ (� �������� ����� �������)
	/// </summary>
	public enum WaitTime
	{
		MinWaitTime = 100,
		MaxWaitTime = 1000
	};

	/// <summary>
	/// ������ ������ ("�� ����������", "������", "��������������", "�������", "������������")
	/// </summary>
	public enum RSMode
	{
		None,
		Protect,
		Recover,
		Repair,
		Test
	};

	/// <summary>
	/// ���� ������ ����-�������� (�� ���� ������������ ������� �����������)
	/// </summary>
	public enum RSType
	{
		Dispersal,
		Alternative,
		Cauchy
	};

	/// <summary>
	/// ��������� ������ ����-�������� (MaxVolCount - ������������ ���������� ����� ��� ������
	/// � ���������� �������� � ����, MaxVolCountAlt - ������������ ���������� ����� ��� "���������������"
	/// ������)
	/// </summary>
	public enum RSConst
	{
		MaxVolCount = 65535,
		MaxVolCountAlt = 32768
	};

	/// <summary>
	/// ����� ����� �� ���������� �����, ���� �������� ������������ ���������� �� ������������
	/// </summary>
	public enum RSParallelEdge
	{
		Value = 128
	};
}