/*----------------------------------------------------------------------+
 |  filename:   FileNamer.cs                                            |
 |----------------------------------------------------------------------|
 |  version:    2.21                                                    |
 |  revision:   24.08.2012 15:52                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    �������� (����������) ����� ����� � ���������� ������   |
 +----------------------------------------------------------------------*/

using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ��� ������������ ����� ���� �� ������ ����������� ������ � ������������ ������
	/// </summary>
	public class FileNamer
	{
		#region Data

		/// <summary>
		/// ������ ��������
		/// </summary>
		private String packFormat = "{0}{1:X4}{2:X4}{3:X4}.{4}";

		/// <summary>
		/// ���������� ��������� ��� ���������� ����� �����
		/// </summary>
		private Regex unpackRegex = new Regex("^(?<codecPrefix>[@, A, C])(?<volNum>[0-9A-F]{4})(?<dataCount>[0-9A-F]{4})(?<eccCount>[0-9A-F]{4})\\.(?<fileName>.+)", RegexOptions.IgnoreCase);

		#endregion Data

		#region Public Operations

		/// <summary>
		/// ���������� ����, ������� ��� �� ������� ����� �����
		/// </summary>
		/// <param name="fullFileName">������ ��� �����</param>
		/// <returns>����</returns>
		public string GetPath(String fullFileName)
		{
			return Path.GetDirectoryName(fullFileName) + "\\";
		}

		/// <summary>
		/// ���������� �������� ��� �����, ������� ����
		/// </summary>
		/// <param name="fullFileName">������ ��� �����</param>
		/// <returns>�������� ��� �����</returns>
		public string GetShortFileName(String fullFileName)
		{
			return Path.GetFileName(fullFileName);
		}

		/// <summary>
		/// "��������" ��������� ����� ����� � ���������� ������
		/// </summary>
		/// <param name="fileName">��� ����� ��� "��������"</param>
		/// <param name="volNum">����� �������� ����</param>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <returns>��������� ���� ��������</returns>
		public bool Pack(ref String fileName, int volNum, int dataCount, int eccCount, int codecType)
		{
			char codecPrefix;

			switch(codecType)
			{
				case (int)RSType.Dispersal:
					{
						codecPrefix = '@';
						break;
					}

				case (int)RSType.Alternative:
					{
						codecPrefix = 'A';
						break;
					}

				default:
				case (int)RSType.Cauchy:
					{
						codecPrefix = 'C';
						break;
					}
			}

			fileName = string.Format(this.packFormat, codecPrefix, volNum, dataCount, eccCount, fileName);

			return true;
		}

		/// <summary>
		/// "����������" ����� ����� �� ����������� ������� � ��������
		/// </summary>
		/// <param name="fileName">��� ����� ��� "����������"</param>
		/// <param name="volNum">����� �������� ����</param>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <returns>��������� ���� ��������</returns>
		public bool Unpack(ref String fileName, ref int volNum, ref int dataCount, ref int eccCount, ref int codecType)
		{
			try
			{
				Match regexMatch = this.unpackRegex.Match(fileName);

				if(!regexMatch.Success) return false;

				// ���������� ��� ������ �� �������� ���� (���� ������� ������������� - ������� � �������)
				if(regexMatch.Groups["codecPrefix"].Value == "C") codecType = (int)RSType.Cauchy;
				else if(regexMatch.Groups["codecPrefix"].Value == "A") codecType = (int)RSType.Alternative;
				else if(regexMatch.Groups["codecPrefix"].Value == "@") codecType = (int)RSType.Dispersal;
				else return false;

				volNum = int.Parse(regexMatch.Groups["volNum"].Value, NumberStyles.HexNumber);
				dataCount = int.Parse(regexMatch.Groups["dataCount"].Value, NumberStyles.HexNumber);
				eccCount = int.Parse(regexMatch.Groups["eccCount"].Value, NumberStyles.HexNumber);
				fileName = regexMatch.Groups["fileName"].Value;

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// "����������" ����� ����� �� ����������� ������� � ��������
		/// </summary>
		/// <param name="fileName">��� ����� ��� "����������"</param>
		/// <param name="dataCount">���������� �������� �����</param>
		/// <param name="eccCount">���������� ����� ��� ��������������</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <returns>��������� ���� ��������</returns>
		public bool Unpack(ref String fileName, ref int dataCount, ref int eccCount, ref int codecType)
		{
			int volNum = 0;
			return Unpack(ref fileName, ref volNum, ref dataCount, ref eccCount, ref codecType);
		}

		/// <summary>
		/// "����������" ����� ����� �� ����������� ������� � ��������
		/// </summary>
		/// <param name="fileName">��� ����� ��� "����������"</param>
		/// <param name="codecType">��� ������ ����-�������� (�� ���� �������)</param>
		/// <returns>��������� ���� ��������</returns>
		public bool Unpack(ref String fileName, ref int codecType)
		{
			int volNum = 0;
			int dataCount = 0;
			int eccCount = 0;
			return Unpack(ref fileName, ref volNum, ref dataCount, ref eccCount, ref codecType);
		}

		/// <summary>
		/// "����������" ����� ����� �� ����������� ������� � ��������
		/// </summary>
		/// <param name="fileName">��� ����� ��� "����������"</param>
		/// <returns>��������� ���� ��������</returns>
		public bool Unpack(ref String fileName)
		{
			int volNum = 0;
			int dataCount = 0;
			int eccCount = 0;
			int codecType = 0;
			return Unpack(ref fileName, ref volNum, ref dataCount, ref eccCount, ref codecType);
		}

		#endregion Public Operations
	}
}