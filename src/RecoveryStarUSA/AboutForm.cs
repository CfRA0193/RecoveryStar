/*----------------------------------------------------------------------+
 |  filename:   AboutForm.cs                                            |
 |----------------------------------------------------------------------|
 |  version:    2.22                                                    |
 |  revision:   02.04.2013 17:00                                        |
 |  authors:    �������� ���� ��������� (DrAF),                        |
 |              RUSpectrum (�. ��������).                               |
 |  e-mail:     draf@mail.ru                                            |
 |  purpose:    ���������������� ����������� �� ���� RAID-������        |
 +----------------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;

namespace RecoveryStar
{
	public partial class AboutForm : Form
	{
		#region Data

		/// <summary>
		/// ������ �����������
		/// </summary>
		private Bitmap[] images;

		/// <summary>
		/// ������ ���������� �����������
		/// </summary>
		private int imageIndex;

		#endregion Data

		#region Construction & Destruction

		/// <summary>
		/// ����������� �����
		/// </summary>
		public AboutForm()
		{
			InitializeComponent();
			this.imageIndex = 0;
		}

		#endregion Construction & Destruction

		#region Private Operations

		/// <summary>
		/// ���������� ������� "���� �� HTML-������"
		/// </summary>
		private void imitLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				// ���������� ������� �� ��������������� �����-������
				Process.Start(imitLinkLabel.Text);
			}
			catch
			{
			}
		}

		/// <summary>
		/// ���������� ������� "���� �� ������ OK"
		/// </summary>
		private void okButtonXP_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// ���������� ������� "�������� �����"
		/// </summary>
		private void AboutForm_Load(object sender, EventArgs e)
		{
			try
			{
				this.RSIconsLoad();
				GC.Collect();
				this.imageIndex = 0;
				this.RSIconTimer.Start();
			}
			catch
			{
				string message = "Can't load \"RSIcons.dat\"!";
				string caption = " Recovery Star 2.22";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
			}
		}

		/// <summary>
		/// �������� ������ �� ������
		/// </summary>
		private void RSIconsLoad()
		{
			using(Stream stream = new FileStream("RSIcons.dat", FileMode.Open, FileAccess.Read))
			{
				using(GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress))
				{
					using(BinaryReader reader = new BinaryReader(gzStream))
					{
						this.images = new Bitmap[reader.ReadUInt16()];
						ushort width = reader.ReadUInt16();
						ushort height = reader.ReadUInt16();
						ushort colorCount = reader.ReadUInt16();
						byte[] A = reader.ReadBytes(colorCount);
						byte[] R = reader.ReadBytes(colorCount);
						byte[] G = reader.ReadBytes(colorCount);
						byte[] B = reader.ReadBytes(colorCount);
						using(Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
						{
							Color colorTransparent = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
							for(int y = 0; y < height; y++) for(int x = 0; x < height; x++) bitmap.SetPixel(x, y, colorTransparent);

							for(int bitmapIndex = 0; bitmapIndex < this.images.Length; bitmapIndex++)
							{
								for(int y = 0; y < height; y++)
								{
									for(int x = 0; x < width; x++)
									{
										ushort colorIndex = reader.ReadUInt16();
										if(colorIndex == 0) continue;
										Color colorPixel = Color.FromArgb(A[colorIndex], R[colorIndex], G[colorIndex], B[colorIndex]);
										bitmap.SetPixel(x, y, colorPixel);
									}
								}
								this.images[bitmapIndex] = (Bitmap)bitmap.Clone();
							}
						}
						reader.Close();
					}
				}
				stream.Close();
			}
		}

		/// <summary>
		/// ���������� ������� "�������� �����"
		/// </summary>
		private void AboutForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				this.RSIconTimer.Stop();
			}
			catch
			{
			}
		}

		/// <summary>
		/// ���������� ��������
		/// </summary>
		private void RSIconTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				this.logoPictureBox.Image = this.images[this.imageIndex++];
				if(this.imageIndex >= this.images.Length) this.imageIndex = 0;
				GC.Collect();
			}
			catch
			{
				if(this.RSIconTimer != null) this.RSIconTimer.Stop();
			}
		}

		#endregion Private Operations
	}
}