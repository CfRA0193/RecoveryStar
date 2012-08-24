/*----------------------------------------------------------------------+
 |  filename:   MainForm.cs                                             |
 |----------------------------------------------------------------------|
 |  version:    2.21                                                    |
 |  revision:   24.08.2012 15:52                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ���������������� ����������� �� ���� RAID-������        |
 +----------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RecoveryStar
{
	public partial class MainForm : Form
	{
		#region Data

		/// <summary>
		/// ������ ��� �������� (����������) ����� � ���������� ������
		/// </summary>
		private FileNamer eFileNamer;

		/// <summary>
		/// ������ �������� �������� ��� ��������� ���������� "TrackBar" (���������� �����)
		/// </summary>
		private int[] allVolCountTrackBarValuesArr;

		/// <summary>
		/// ������ �������� �������� ��� ��������� ���������� "TrackBar" (������������)
		/// </summary>
		private int[] redundancyTrackBarValuesArr;

		/// <summary>
		/// ����� ���������� �����
		/// </summary>
		private int allVolCount;

		/// <summary>
		/// ������������ �����������
		/// </summary>
		private int redundancy;

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		private int dataCount;

		/// <summary>
		/// ���������� ����� ��� ��������������
		/// </summary>
		private int eccCount;

		/// <summary>
		/// ����� ����� ������
		/// </summary>
		private PasswordForm ePasswordForm;

		/// <summary>
		/// ����� ��������� ������
		/// </summary>
		private ProcessForm eProcessForm;

		/// <summary>
		/// ����� ������������
		/// </summary>
		private BenchmarkForm eBenchmarkForm;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� �����
		/// </summary>
		public MainForm()
		{
			InitializeComponent();

			// ������� ����� ����� ������
			this.ePasswordForm = new PasswordForm();

			// �������������� ��������� ������ ��� �������� (����������) ����� �����
			// � ���������� ������
			this.eFileNamer = new FileNamer();

			// �������������� �������, �������� ����������� �������� ������� ���� � ������������,
			// ��������� ������������
			this.allVolCountTrackBarValuesArr = new int[(allVolCountMacTrackBar.Maximum + 1)];

			// ������ ��������� �������� ������� ������ � ��������� �������� ��������
			int p1 = 2, p2 = 3;
			for(int i = 0; i < allVolCountMacTrackBar.Maximum; i += 2)
			{
				// ��� ��������� �������� �������� ���������� ����� �������������� ���
				// ������� ������, ��� ������ �������� - ��� ��������� ������������� �����
				// ��������
				this.allVolCountTrackBarValuesArr[i + 0] = p1;
				this.allVolCountTrackBarValuesArr[i + 1] = p2;

				// ���������� ������� ��������
				p1 <<= 1;
				p2 <<= 1;
			}

			// ���������� �� ������������ � ����� �������
			this.allVolCountTrackBarValuesArr[allVolCountMacTrackBar.Maximum] = p1;

			this.redundancyTrackBarValuesArr = new int[(redundancyMacTrackBar.Maximum + 1)];

			for(int i = 0; i <= redundancyMacTrackBar.Maximum; i++)
			{
				this.redundancyTrackBarValuesArr[i] = (i + 1) * 5;
			}
		}

		#endregion Construction & Destruction

		#region Private Operations

		/// <summary>
		/// ����� ��������� ������ � ��������� ����������
		/// </summary>
		private void ProcessFiles()
		{
			// ���� � �������� � �������� �������� �������� ������� ����������
			if(browser.SelectedItem.IsFolder)
			{
				// ���� � ������ ������ ����� ������� - ������ �������
				if(
					(this.eProcessForm != null)
					&&
					(this.eProcessForm.Visible)
					)
				{
					return;
				}

				// ������������� ��������� ������
				this.eProcessForm.DataCount = this.dataCount;
				this.eProcessForm.EccCount = this.eccCount;
				this.eProcessForm.CodecType = (int)RSType.Cauchy;

				// ������ ������ � ������� ����������
				FileInfo[] fileInfos;
				try
				{
					fileInfos = new DirectoryInfo(browser.SelectedItem.Path).GetFiles();
				}
				catch
				{
					// C��������� ������������� �����
					this.eProcessForm.Mode = RSMode.None;

					return;
				}

				// ��������� ������ �� �������...
				this.eProcessForm.Browser = browser;

				// �������� ����� ������ ��� ���������
				for(int i = 0; i < fileInfos.Length; i++)
				{
					// ��������� ��������� ��� �� ������...
					String fullFileName = fileInfos[i].DirectoryName + @"\" + fileInfos[i].Name;

					// �������� �������� ������� ����� �����
					String shortFileName = this.eFileNamer.GetShortFileName(fullFileName);

					// ���� ��� ��������� ����� ��������� 50 �������� - �� �� ����� ���� ���������
					// (������ ��� ��� ���������� �������� �� ������ ��������� ����� 64-� ��������)
					if(shortFileName.Length > 50)
					{
						string message = "����� ����� ����� \"" + shortFileName + "\" ��������� 50 ��������! ���������� ���� ���� � ���������� ������� ������������ ������ ��� ���������?";
						string caption = " Recovery Star 2.21";
						MessageBoxButtons buttons = MessageBoxButtons.YesNo;
						DialogResult result = MessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

						// ���� ������������ ����� �� ������ "No" - ��������� ���������...
						if(result == DialogResult.No)
						{
							//...�������������� ������� ������������� �����
							this.eProcessForm.Mode = RSMode.None;

							return;
						}
					}
					else
					{
						// ���� ���� ������������ - ��������� ��� � ������ �� ���������
						if(File.Exists(fullFileName))
						{
							this.eProcessForm.FileNamesToProcess.Add(fullFileName);
						}
					}
				}

				// ���� ������ ������ ��� ��������� �� ����� ����
				// (�.�. ���� ��� ������������) - ����� ������������ ���������
				if(this.eProcessForm.FileNamesToProcess.ToArray().Length != 0)
				{
					// ���������� ���� ���������
					this.eProcessForm.Show();
				}
				else
				{
					string message = "� ��������� ���������� �� ������� ��������� ������ ��� ���������!";
					string caption = " Recovery Star 2.21";
					MessageBoxButtons buttons = MessageBoxButtons.OK;
					MessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

					// C��������� ������������� �����
					this.eProcessForm.Mode = RSMode.None;
				}
			}
			else
			{
				// C��������� ������������� �����
				this.eProcessForm.Mode = RSMode.None;
			}
		}

		/// <summary>
		/// ����� ��������� ������ � ��������� ���������� � ������ ����������� �����,
		/// ����������� ������������������
		/// </summary>
		private void ProcessUniqueFiles()
		{
			// ���� � �������� � �������� �������� �������� ������� ����������
			if(browser.SelectedItem.IsFolder)
			{
				// ���� � ������ ������ ����� ������� - ������ �������
				if(
					(this.eProcessForm != null)
					&&
					(this.eProcessForm.Visible)
					)
				{
					return;
				}

				// ������ ���������� ���� ������ ��� ���������
				ArrayList uniqueNamesToProcess = new ArrayList();

				// ������ ������ � ������� ����������
				FileInfo[] fileInfos;
				try
				{
					fileInfos = new DirectoryInfo(browser.SelectedItem.Path).GetFiles();
				}
				catch
				{
					// C��������� ������������� �����
					this.eProcessForm.Mode = RSMode.None;

					return;
				}

				// ��������� ������ �� �������...
				this.eProcessForm.Browser = browser;

				// �������� ����� ������ ��� ���������
				for(int i = 0; i < fileInfos.Length; i++)
				{
					// ��������� ��������� ��� �� ������...
					String fullFileName = fileInfos[i].DirectoryName + @"\" + fileInfos[i].Name;

					//...�������� ��� �������� �������...
					String shortFileName = this.eFileNamer.GetShortFileName(fullFileName);

					//...� ������������� ��� � ���������� ������������� �����...
					String unpackedFileName = shortFileName;

					// ���� �� ������� ��������� ����������� �������� ��� - ���������
					// �� ��������� ��������
					if(!this.eFileNamer.Unpack(ref unpackedFileName))
					{
						continue;
					}

					//...����� ��������� ��� �� ������������ - ���� ����� ��� ��� ����
					// � ������� "uniqueNamesToProcess", �� ��������� ��� �� �����

					// ������� ������������, ��� ������������� ��� ����� ���������
					bool unpackedFileNameIsUnique = true;

					// ���������� ���� ��������� ������ ���������� ����
					foreach(String currUnpackedFileName in uniqueNamesToProcess)
					{
						// ���� ���������� ���������� - ��� �� ���������,
						// �������� �� ���� � ������� �� ������
						if(currUnpackedFileName == unpackedFileName)
						{
							unpackedFileNameIsUnique = false;

							break;
						}
					}

					// ���� ������������� ���� ��������...
					if(unpackedFileNameIsUnique)
					{
						// ���� ���� ������������...
						if(File.Exists(fullFileName))
						{
							//...��������� ��� � ������ ���������� ����...
							uniqueNamesToProcess.Add(unpackedFileName);

							//...��������� ��� � ������ ��� ���������...
							this.eProcessForm.FileNamesToProcess.Add(fullFileName);
						}
					}
				}

				// ���� ������ ������ ��� ��������� �� ����� ����
				// (�.�. ���� ��� ������������) - ����� ������������ ���������
				if(this.eProcessForm.FileNamesToProcess.ToArray().Length != 0)
				{
					// ���������� ���� ���������
					this.eProcessForm.Show();
				}
				else
				{
					string message = "� ��������� ���������� �� ������� ��������� ������ ��� ���������!";
					string caption = " Recovery Star 2.21";
					MessageBoxButtons buttons = MessageBoxButtons.OK;
					MessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

					// C��������� ������������� �����
					this.eProcessForm.Mode = RSMode.None;
				}
			}
			else
			{
				// C��������� ������������� �����
				this.eProcessForm.Mode = RSMode.None;
			}
		}

		/// <summary>
		/// ������ ������ ����������� �����
		/// </summary>
		private void protectButton_Click(object sender, EventArgs e)
		{
			// ����� ����� �� ������ ��������� ����� �� �������� �������
			browser.Focus();

			// ���� ����� ��������� ����� ��������� ����� � � ��� ���������� ����� -
			// ��� ��������� � ��������� � ������!
			if(
				(this.eProcessForm != null)
				&&
				(this.eProcessForm.Mode != RSMode.None)
				)
			{
				return;
			}

			// ������� ����� ����������� �����
			this.eProcessForm = new ProcessForm();

			// ������ ����� ��������� ����������� � ������� �����
			this.eProcessForm.Owner = this;

			// ��������� �����, ���� ���� ������� � ����� ���������
			if(this.eProcessForm.Visible)
			{
				this.eProcessForm.Close();
			}

			// ���� ������ �� ���� - ������������� ������������
			if(this.ePasswordForm.Password.Length != 0)
			{
				this.eProcessForm.Security = new Security(this.ePasswordForm.Password);
				this.eProcessForm.CBCBlockSize = this.ePasswordForm.CBCBlockSize;
			}

			// ������������� ����� ������
			this.eProcessForm.Mode = RSMode.Protect;

			// ��������� ��������� �����
			ProcessFiles();
		}

		/// <summary>
		/// ������ ������ �������������� ������ �����
		/// </summary>
		private void recoverButton_Click(object sender, EventArgs e)
		{
			// ����� ����� �� ������ ��������� ����� �� �������� �������
			browser.Focus();

			// ���� ����� ��������� ����� ��������� ����� � � ��� ���������� ����� -
			// ��� ��������� � ��������� � ������!
			if(
				(this.eProcessForm != null)
				&&
				(this.eProcessForm.Mode != RSMode.None)
				)
			{
				return;
			}

			// ������� ����� ����������� �����
			this.eProcessForm = new ProcessForm();

			// ������ ����� ��������� ����������� � ������� �����
			this.eProcessForm.Owner = this;

			// ��������� �����, ���� ���� ������� � ����� ���������
			if(this.eProcessForm.Visible)
			{
				this.eProcessForm.Close();
			}

			// ���� ������ �� ���� - ������������� ������������
			if(this.ePasswordForm.Password.Length != 0)
			{
				this.eProcessForm.Security = new Security(this.ePasswordForm.Password);
				this.eProcessForm.CBCBlockSize = this.ePasswordForm.CBCBlockSize;
			}

			// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
			this.eProcessForm.FastExtraction = �����������������ToolStripMenuItem.Checked;

			// ������������� ����� ������
			this.eProcessForm.Mode = RSMode.Recover;

			// ��������� ��������� ��������� ���������� ���� ������ (��� ����� ���������)
			ProcessUniqueFiles();
		}

		/// <summary>
		/// ������ ������ ������� ������ ������ �����
		/// </summary>
		private void repairButton_Click(object sender, EventArgs e)
		{
			// ����� ����� �� ������ ��������� ����� �� �������� �������
			browser.Focus();

			// ���� ����� ��������� ����� ��������� ����� � � ��� ���������� ����� -
			// ��� ��������� � ��������� � ������!
			if(
				(this.eProcessForm != null)
				&&
				(this.eProcessForm.Mode != RSMode.None)
				)
			{
				return;
			}

			// ������� ����� ����������� �����
			this.eProcessForm = new ProcessForm();

			// ������ ����� ��������� ����������� � ������� �����
			this.eProcessForm.Owner = this;

			// ��������� �����, ���� ���� ������� � ����� ���������
			if(this.eProcessForm.Visible)
			{
				this.eProcessForm.Close();
			}

			// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
			this.eProcessForm.FastExtraction = �����������������ToolStripMenuItem.Checked;

			// ������������� ����� ������
			this.eProcessForm.Mode = RSMode.Repair;

			// ��������� ��������� ��������� ���������� ���� ������ (��� ����� ���������)
			ProcessUniqueFiles();
		}

		/// <summary>
		/// ������ ������ ������������ ������ ������ �����
		/// </summary>
		private void testButton_Click(object sender, EventArgs e)
		{
			// ����� ����� �� ������ ��������� ����� �� �������� �������
			browser.Focus();

			// ���� ����� ��������� ����� ��������� ����� � � ��� ���������� ����� -
			// ��� ��������� � ��������� � ������!
			if(
				(this.eProcessForm != null)
				&&
				(this.eProcessForm.Mode != RSMode.None)
				)
			{
				return;
			}

			// ������� ����� ����������� �����
			this.eProcessForm = new ProcessForm();

			// ������ ����� ��������� ����������� � ������� �����
			this.eProcessForm.Owner = this;

			// ��������� �����, ���� ���� ������� � ����� ���������
			if(this.eProcessForm.Visible)
			{
				this.eProcessForm.Close();
			}

			// ������������ ������� ���������� �� ����� (��� �������� CRC-64)?
			this.eProcessForm.FastExtraction = �����������������ToolStripMenuItem.Checked;

			// ������������� ����� ������
			this.eProcessForm.Mode = RSMode.Test;

			// ��������� ��������� ��������� ���������� ���� ������ (��� ����� ���������)
			ProcessUniqueFiles();
		}

		/// <summary>
		/// ������ ��������� �����������
		/// </summary>
		private void ������������������ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// ���� ����� ��������� ����� ��������� ����� � � ��� ���������� ����� -
			// ��� ��������� � �� ������ ������!
			if(
				(this.eProcessForm != null)
				&&
				(this.eProcessForm.Mode != RSMode.None)
				)
			{
				return;
			}

			// ������� ����� ����������� �����
			this.eBenchmarkForm = new BenchmarkForm();

			// ������������� ��������� ������
			this.eBenchmarkForm.DataCount = this.dataCount;
			this.eBenchmarkForm.EccCount = this.eccCount;
			this.eBenchmarkForm.CodecType = (int)RSType.Cauchy;

			// ������ ����� ��������� ����������� � ������� �����
			this.eBenchmarkForm.Owner = this;

			// ��������� �����, ���� ���� ������� � ����� ���������
			if(this.eBenchmarkForm.Visible)
			{
				this.eBenchmarkForm.Close();
			}

			// ���������� ���������� ���� ���������
			this.eBenchmarkForm.ShowDialog();
		}

		/// <summary>
		/// ����� ��������� ������������ ������ � ������������ ���������������
		/// ��������� �� �����
		/// </summary>
		private void SetCoderConfig()
		{
			// ������� ������ � ��������� ����������
			this.allVolCount = this.allVolCountTrackBarValuesArr[allVolCountMacTrackBar.Value];
			this.redundancy = this.redundancyTrackBarValuesArr[redundancyMacTrackBar.Value];

			// ������������� ��������� �����, ��������������� ��������� �������� ����������
			allVolCountGroupBox.Text = "����� ���������� �����: " + System.Convert.ToString(this.allVolCount);
			redundancyGroupBox.Text = "������������ �����������: " + System.Convert.ToString(this.redundancy) + " %";

			// ���������� �������� ���������� ��������� �� ���
			double percByVol = (double)this.allVolCount / (double)(100 + this.redundancy);

			// ��������� ���������� ����� ��� ��������������
			this.eccCount = (int)((double)this.redundancy * percByVol); // ����� ��� ��������������

			// � ������ ������������� ������������ ���������� ����� ��� ��������������
			if(this.eccCount < 1)
			{
				this.eccCount = 1;
			}

			// ���������� �������� ����� ������� �� ����������� ��������
			this.dataCount = this.allVolCount - this.eccCount;

			// ��������� ����������� ������
			double outX = ((double)(this.dataCount + this.eccCount)) / (double)this.dataCount;

			// ��������� �������������� �����
			String outXStr = System.Convert.ToString(outX);

			// ����� ���������, ���������� ��-���������
			int subStrLen = 3;

			// ��� ����������� �������� ����� ����� ������������ ����� ����� ���������
			// �� ���� ������ ������
			if(outX >= 10)
			{
				subStrLen++;
			}

			// ������������ (� ������ ����������) ����� ����������� ���������
			if(outXStr.Length < subStrLen)
			{
				subStrLen = outXStr.Length;
			}

			// �������� ��������� ������������� ������
			outXStr = outXStr.Substring(0, subStrLen);

			// ����������� � ����� ��������� �������� ������������, ������� ������ ������������
			double visibleX = System.Convert.ToDouble(outXStr);

			// ���� � ���������� �������������� ���� ������� �������� �����, ��������� 0.1
			// � ���������� ��������
			if(visibleX != outX)
			{
				outX += 0.1;
			}

			// ��������� �������������� �����
			outXStr = System.Convert.ToString(outX);

			// ����� ���������, ���������� ��-���������
			subStrLen = 3;

			// ��� ����������� �������� ����� ����� ������������ ����� ����� ���������
			// �� ���� ������ ������
			if(outX >= 10)
			{
				subStrLen++;
			}

			// ������������ (� ������ ����������) ����� ����������� ���������
			if(outXStr.Length < subStrLen)
			{
				subStrLen = outXStr.Length;
			}

			// �������� ��������� ������������� ������
			outXStr = outXStr.Substring(0, subStrLen);

			// ������� ������������ ������
			coderConfigGroupBox.Text = "������������ ������ (�������� �����: " + System.Convert.ToString(this.dataCount)
			                           + "; ����� ��� ��������������: " + System.Convert.ToString(this.eccCount)
			                           + "; ����� ������: " + outXStr + " X)";
		}

		/// <summary>
		/// ���������� ������� ��������� ������ ���������� �����
		/// </summary>
		private void allVolCountMacTrackBar_ValueChanged(object sender, decimal value)
		{
			// ������� ������ � �������� ����������
			this.allVolCount = this.allVolCountTrackBarValuesArr[allVolCountMacTrackBar.Value];

			// ������������� ��������� �����, ��������������� ��������� �������� ����������
			allVolCountGroupBox.Text = "����� ���������� �����: " + System.Convert.ToString(this.allVolCount);

			// ������������� ������������ ������
			SetCoderConfig();
		}

		/// <summary>
		/// ���������� ������� ��������� ������������ �����������
		/// </summary>
		private void redundancyMacTrackBar_ValueChanged(object sender, decimal value)
		{
			// ������� ������ � �������� ����������
			this.redundancy = this.redundancyTrackBarValuesArr[redundancyMacTrackBar.Value];

			// ������������� ��������� �����, ��������������� ��������� �������� ����������
			redundancyGroupBox.Text = "������������ �����������: " + System.Convert.ToString(this.redundancy) + " %";

			// ������������� ������������ ������
			SetCoderConfig();
		}

		/// <summary>
		/// ���� ������
		/// </summary>
		private void ���������������ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(���������������ToolStripMenuItem.Checked == true)
			{
				���������������ToolStripMenuItem.Checked = false;

				// ������� ������
				this.ePasswordForm.ClearPassword();
			}
			else
			{
				// ������� ������ ����� ������
				this.ePasswordForm.ShowDialog();

				// ���� ������ ��� ���������� - ��������� ���
				if(this.ePasswordForm.Password.Length != 0)
				{
					���������������ToolStripMenuItem.Checked = true;
				}
			}
		}

		/// <summary>
		/// ��������� ������ �������� ���������� ������ (��� �������� CRC-64)
		/// </summary>
		private void �����������������ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(�����������������ToolStripMenuItem.Checked == true)
			{
				�����������������ToolStripMenuItem.Checked = false;
			}
			else
			{
				�����������������ToolStripMenuItem.Checked = true;
			}
		}

		/// <summary>
		/// ����� �������
		/// </summary>
		private void ������������ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				// ��������� ���� �������
				System.Diagnostics.Process.Start("HelpRUS.mht");
			}
			catch
			{
				string message = "�� ���� ������� \"HelpRUS.mht\"!";
				string caption = " Recovery Star 2.21";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
			}
		}

		/// <summary>
		/// � ���������
		/// </summary>
		private void ����������ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm eAboutForm = new AboutForm();
			eAboutForm.ShowDialog();
		}

		/// <summary>
		/// �����
		/// </summary>
		private void �����ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// ���� ����� ��������� ����� ��������� ����� � � ��� ���������� ����� -
			// ��� ��������� � �� ������ ������!
			if(
				(this.eProcessForm != null)
				&&
				(this.eProcessForm.Mode != RSMode.None)
				)
			{
				return;
			}

			Close();
		}

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// ���� ����� ��������� ����� ��������� ����� - � ����� �������
			if(this.eProcessForm != null)
			{
				// �� ����������� Close() ���������� ��������� ��������
				this.eProcessForm.Close();
			}
		}

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		private void MainForm_Load(object sender, EventArgs e)
		{
			// ������������� ����� ���������� ����� ��-��������� - 1024
			allVolCountMacTrackBar.Value = 18;

			// ������������ ������������ ����������� - 100%
			redundancyMacTrackBar.Value = 19;

			// ������������� ������������ ������
			SetCoderConfig();
		}

		#endregion Private Operations
	}
}