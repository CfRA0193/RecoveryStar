/*----------------------------------------------------------------------+
 |  filename:   Program.cs                                              |
 |----------------------------------------------------------------------|
 |  version:    2.20                                                    |
 |  revision:   23.05.2012 17:33                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ���� RAID-��������� �������� ����-�������� (����)       |
 +----------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace RecoveryStar
{
	class Program
	{
		private static void Main(string[] args)
		{
			RSRaidDecoder eRSRaidDecoder = new RSRaidDecoder();
			Random eRandom = new Random();

			Console.Clear();
			Console.WriteLine("");
			Console.WriteLine("Recovery Star 2.20 (Cauchy Reed-Solomon Decoder Test)");
			Console.WriteLine("");

			// ��������� ����������� ���������� �����
			Console.Write("Enter MINIMUM count of volumes: ");
			int minVolCount = Convert.ToInt16(Console.ReadLine());

			// ��������� ������������ ���������� �����
			Console.Write("Enter MAXIMUM count of volumes: ");
			int maxVolCount = Convert.ToInt16(Console.ReadLine());
			Console.WriteLine("");

			// ���� ������������ ��������� �������� � ������� - ������ �� �������
			if(maxVolCount < minVolCount)
			{
				int temp = maxVolCount;
				maxVolCount = minVolCount;
				minVolCount = temp;
			}

			// ���������� �������� c ������ OK
			int OKCount = 0;

			// ���������� �������� c ������ Error
			int ErrorCount = 0;

			// ����� ���������� ��������
			int TotalCount = 0;

			while(true)
			{
				// ������������� ��������� ���������� �����
				int allVolCount = minVolCount + eRandom.Next((maxVolCount - minVolCount) + 1);

				// ���������� ����� ��� �������������� �� ��������� ���������� ����� ������
				int eccCount = 1 + eRandom.Next((allVolCount / 2) - 1);

				// ���������� ����� ������ ������� �� ����������� ��������
				int dataCount = allVolCount - eccCount;

				// ��������� ������ ����� (������ � ������������)
				ArrayList allVolList = new ArrayList(allVolCount);
				for(int i = 0; i < allVolCount; i++) allVolList.Add(i);

				// ����������� ��� ���� ��� �������������� ��� ��������� �����������
				// ��������� ����������� ����������
				int nErasures = eccCount;

				// ���������� ������ ���� ������!
				for(int i = 0; i < nErasures; i++) allVolList.RemoveAt(eRandom.Next(allVolList.Count - eccCount));

				// ��������� ������� ��� ��������...
				int[] volList = new int[dataCount];

				// �������� ������ ������ �������� ����� � ������ ��� ��������...
				for(int i = 0; i < dataCount; i++) volList[i] = (int)allVolList[i];

				// ������������� ������������ ��������...
				eRSRaidDecoder.SetConfig(dataCount, eccCount, volList, (int)RSType.Cauchy);

				// �������������� ����� �������� �������...
				if(!eRSRaidDecoder.Prepare(false))
				{
					// ���������� � ���� ������ ��������� ������������
					String logFileName = "Error " + DateTime.Now.ToString().Replace(':', '.') + ".txt";

					File.AppendAllText(logFileName, "dataCount:" + Convert.ToString(dataCount + "; "), Encoding.ASCII);
					File.AppendAllText(logFileName, "eccCount:" + Convert.ToString(eccCount + "; "), Encoding.ASCII);

					for(int i = 0; i < dataCount; i++)
					{
						if(volList[i] < dataCount)
						{
							File.AppendAllText(logFileName, "d:" + Convert.ToString(volList[i] + "; "), Encoding.ASCII);
						}
						else
						{
							File.AppendAllText(logFileName, "e:" + Convert.ToString(volList[i] + "; "), Encoding.ASCII);
						}
					}

					ErrorCount++;
				}

				TotalCount++;
				OKCount = TotalCount - ErrorCount;

				if((TotalCount % 100) == 0)
				{
					Console.WriteLine("OK: " + Convert.ToString(OKCount) + ", " +
					                  "Errors: " + Convert.ToString(ErrorCount) + ";");
				}
			}
		}
	}
}