/*----------------------------------------------------------------------+
 |  filename:   SystemInfo.cs                                           |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    �������������� ���������� � ��������� ��������          |
 +----------------------------------------------------------------------*/

using System;
using System.Management;

namespace RecoveryStar
{
	/// <summary>
	/// �������������� ���������� � ��������� ��������
	/// </summary>
	public class SystemInfo
	{
		#region Public Properties & Data

		/// <summary>
		/// ����� ���������� ������
		/// </summary>
		public ulong TotalPhysicalMemory
		{
			get { return this.totalPhysicalMemory; }
		}

		/// <summary>
		/// ����� ���������� ������
		/// </summary>
		private ulong totalPhysicalMemory = 1 << 26; // 64 �����

		/// <summary>
		/// �������� ���������� ������
		/// </summary>
		public ulong FreePhysicalMemory
		{
			get { return this.freePhysicalMemory; }
		}

		/// <summary>
		/// �������� ���������� ������
		/// </summary>
		private ulong freePhysicalMemory = 1 << 25; // 32 �����

		#endregion Public Properties & Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� ������
		/// </summary>
		public SystemInfo()
		{
			try
			{
				ManagementScope managementScope1 = new ManagementScope();
				ManagementObjectSearcher managementObjectSearcher1 = new ManagementObjectSearcher(managementScope1, new ObjectQuery("SELECT * FROM Win32_PhysicalMemory"));
				foreach(ManagementObject BankRAM in managementObjectSearcher1.Get()) this.totalPhysicalMemory = (ulong)BankRAM.GetPropertyValue("Capacity");
			}
			catch
			{
				this.totalPhysicalMemory = 1 << 26; // 64 �����
			}

			try
			{
				ManagementScope managementScope2 = new ManagementScope();
				ManagementObjectSearcher managementObjectSearcher2 = new ManagementObjectSearcher(managementScope2, new ObjectQuery("SELECT * FROM Win32_OperatingSystem"));
				foreach(ManagementObject OS in managementObjectSearcher2.Get())
				{
					// << 10 - ��������� ��������� � ����� (2^10 = 1024)
					this.freePhysicalMemory = (ulong)OS.GetPropertyValue("FreePhysicalMemory") << 10;
					break;
				}
			}
			catch
			{
				this.freePhysicalMemory = 1 << 25; // 32 �����
			}
		}

		#endregion Construction & Destruction
	}
}