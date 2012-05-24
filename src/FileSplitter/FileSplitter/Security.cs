/*----------------------------------------------------------------------+
 |  filename:   Security.cs                                             |
 |----------------------------------------------------------------------|
 |  version:    2.20                                                    |
 |  revision:   23.05.2012 17:33                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ����������� ����������������� ������ (AES-256)          |
 +----------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace RecoveryStar
{
	/// <summary>
	/// ����� ����������� ����������������� ������ ������ (�������� Rijndael (AES), 256 ���)
	/// </summary>
	public class Security
	{
		#region Public Properties & Data

		/// <summary>
		/// ������ �������������
		/// </summary>
		public byte[] IV
		{
			get { return this.iv; }
		}

		/// <summary>
		/// ������ �������������
		/// </summary>
		private byte[] iv;

		/// <summary>
		/// ���� ����������
		/// </summary>
		public byte[] Key
		{
			get { return this.key; }
		}

		/// <summary>
		/// ���� ����������
		/// </summary>
		private byte[] key;

		#endregion Public Properties & Data

		#region Data

		/// <summary>
		/// ������ ���-������� (256 ���)
		/// </summary>
		private SHA256 eSHA256;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� ������
		/// </summary>
		public Security(String password)
		{
			eSHA256 = SHA256.Create();
			key = GetHash(UnicodeEncoding.Unicode.GetBytes(password));
			iv = GetHash(key);
		}

		/// <summary>
		/// ���������� ������
		/// </summary>
		~Security()
		{
			Clear();
		}

		#endregion Construction & Destruction

		#region Public Operations

		/// <summary>
		/// ���������� ����� ������ (��������� CBC)
		/// </summary>
		/// <param name="data">������� ����� ������</param>
		/// <param name="length">����� �������������� ������������������</param>
		public void Encrypt(byte[] data, int length)
		{
			Encrypt(data, length, key, iv);
		}

		/// <summary>
		/// ����������� ����� ������ (��������� CBC)
		/// </summary>
		/// <param name="data">������� ����� ������</param>
		/// <param name="length">����� �������������� ������������������</param>
		public bool Decrypt(byte[] data, int length)
		{
			return Decrypt(data, length, key, iv);
		}

		/// <summary>
		/// ����� ������� ������ ����������
		/// </summary>
		public void Clear()
		{
			for(int i = 0; i < key.Length; i++)
			{
				key[i] = 0xFF;
				iv[i] = 0xFF;
			}

			key = null;
			iv = null;
		}

		#endregion

		#region Private Operations

		/// <summary>
		/// ����� ��������� ���������� ������
		/// </summary>
		private CryptoStream GetEncrypter(Stream s)
		{
			return GetEncrypter(s, key, iv);
		}

		/// <summary>
		/// ����� ��������� ����������������� ������
		/// </summary>
		private CryptoStream GetDecrypter(Stream s)
		{
			return GetDecrypter(s, key, iv);
		}

		/// <summary>
		/// ����� "�����������"
		/// </summary>
		private void Encrypt(byte[] data, int length, byte[] key, byte[] iv)
		{
			Rijndael rijndael = new RijndaelManaged();
			rijndael.Mode = CipherMode.CBC;
			rijndael.BlockSize = 256;
			rijndael.KeySize = 256;

			ICryptoTransform ict = rijndael.CreateEncryptor(key, iv);

			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write);

			cs.Write(data, 0, length);
			cs.FlushFinalBlock();

			Array.Copy(ms.GetBuffer(), data, ms.Length);

			cs.Close();
			cs.Clear();
			rijndael.Clear();
		}

		/// <summary>
		/// ����� ��������� ���������� ������
		/// </summary>
		private CryptoStream GetEncrypter(Stream s, byte[] key, byte[] iv)
		{
			Rijndael rijndael = new RijndaelManaged();
			rijndael.Mode = CipherMode.CBC;
			rijndael.BlockSize = 256;
			rijndael.KeySize = 256;

			ICryptoTransform ict = rijndael.CreateEncryptor(key, iv);

			CryptoStream cs = new CryptoStream(s, ict, CryptoStreamMode.Write);

			return cs;
		}

		/// <summary>
		/// ����� "������������"
		/// </summary>
		private bool Decrypt(byte[] data, int length, byte[] key, byte[] iv)
		{
			Rijndael rijndael = new RijndaelManaged();
			rijndael.Mode = CipherMode.CBC;
			rijndael.BlockSize = 256;
			rijndael.KeySize = 256;

			ICryptoTransform ict = rijndael.CreateDecryptor(key, iv);

			MemoryStream ms = new MemoryStream();
			ms.Write(data, 0, length);
			ms.Flush();
			ms.Seek(0, SeekOrigin.Begin);
			CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Read);

			try
			{
				cs.Read(data, 0, length);
			}

			catch(CryptographicException)
			{
				return false;
			}

			cs.Close();
			cs.Clear();

			rijndael.Clear();

			return true;
		}

		/// <summary>
		/// ����� ��������� ����������������� ������
		/// </summary>
		private CryptoStream GetDecrypter(Stream s, byte[] key, byte[] iv)
		{
			Rijndael rijndael = new RijndaelManaged();
			rijndael.Mode = CipherMode.CBC;
			rijndael.BlockSize = 256;
			rijndael.KeySize = 256;

			ICryptoTransform ict = rijndael.CreateDecryptor(key, iv);

			CryptoStream cs = new CryptoStream(s, ict, CryptoStreamMode.Read);

			return cs;
		}

		/// <summary>
		/// ����� ��������� ���-��������
		/// </summary>
		private byte[] GetHash(byte[] b)
		{
			return eSHA256.ComputeHash(b);
		}

		#endregion Private Operations
	}
}