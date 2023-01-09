using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        List<OpenFileDialog> lOfd = new List<OpenFileDialog>();
        List<SaveFileDialog> lSfd = new List<SaveFileDialog>();

        List<List<TPortalStruct>> loadedTcds = new List<List<TPortalStruct>>();

        public class CBinaryReader : BinaryReader
        {
            public CBinaryReader(Stream input)
                : base(input)
            {
            }

            public CBinaryReader(Stream input, Encoding encoding) 
                : base(input, encoding)
            {
            }

            public CBinaryReader(Stream input, Encoding encoding, bool leaveOpen) 
                : base(input, encoding, leaveOpen)
            {
            }

            public string ReadCString()
            {
                ushort length = base.ReadByte();
                if (length >= byte.MaxValue)
                    length = base.ReadUInt16();
                return Encoding.Default.GetString(base.ReadBytes(length));
            }
        }

        public class CBinaryWriter : BinaryWriter
        {
            public CBinaryWriter(Stream input) 
                : base(input)
            {
            }

            public CBinaryWriter(Stream input, Encoding encoding) 
                : base(input, encoding)
            {
            }

            public CBinaryWriter(Stream input, Encoding encoding, bool leaveOpen) 
                : base(input, encoding, leaveOpen)
            {
            }

            public void WriteUInt16Length(int length)
            {
                base.Write(byte.MaxValue);
                base.Write((ushort)length);
            }

            public void WriteByteLength(int length)
            {
                base.Write((byte)length);
            }

            public void WriteCString(string text)
            {
                int length = text.Length;
                if (length >= byte.MaxValue)
                    WriteUInt16Length(length);
                else
                    WriteByteLength(length);
                base.Write(Encoding.Default.GetBytes(text));
            }
        }

        public struct TPortalStruct
        {
            // Lists of Not Used
            public List<byte> lByteNotUsed;

            public ushort m_wPortalID { get; set; }
            public string m_strNAME { get; set; }
            public ushort m_wPortalRegionID { get; set; }
            public ushort m_wMapID { get; set; }
            public float m_fPosX { get; set; }
            public float m_fPosZ { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Tcd files (TPortal.tcd)|TPortal.tcd";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            lOfd.Clear();
            loadedTcds.Clear();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                lOfd.Add(ofd);
                ReadPortals();
                FillListBox();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "All files (*.*)|*.*";
            sfd.FilterIndex = 2;
            sfd.RestoreDirectory = true;

            lSfd.Clear();

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                lSfd.Add(sfd);
                WritePortals();
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadedTcds.Count > 0)
            {
                TPortalStruct sPortal = new TPortalStruct();

                sPortal.lByteNotUsed = new List<byte>();
                sPortal.lByteNotUsed.Add(new byte());
                sPortal.lByteNotUsed.Add(new byte());

                loadedTcds[0].Insert(listBox1.SelectedIndex + 1, sPortal);
                FillListBox();
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadedTcds.Count > 0)
            {
                int nSelectedIndex = listBox1.SelectedIndex;
                bool bIndex = nSelectedIndex >= 0 ? true : false;
                if (bIndex)
                    loadedTcds[0].RemoveAt(listBox1.SelectedIndex);
                else
                    MessageBox.Show("You have not selected any item to remove");

                if ((nSelectedIndex - 1) < 0)
                    FillListBox(0);
                else
                    FillListBox(nSelectedIndex - 1);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadedTcds.Count > 0)
            {
                TPortalStruct sPortal = loadedTcds[0][listBox1.SelectedIndex];
                textBox1.Text = sPortal.m_wPortalID.ToString();
                textBox2.Text = sPortal.m_strNAME;
                textBox3.Text = sPortal.m_wPortalRegionID.ToString();
                textBox4.Text = sPortal.m_wMapID.ToString();
                textBox5.Text = sPortal.m_fPosX.ToString();
                textBox6.Text = sPortal.m_fPosZ.ToString();
                textBox7.Text = sPortal.lByteNotUsed[0].ToString();
                textBox8.Text = sPortal.lByteNotUsed[1].ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loadedTcds.Count > 0)
            {
                try
                {
                    TPortalStruct sPortal = new TPortalStruct();

                    sPortal.lByteNotUsed = new List<byte>();
                    sPortal.lByteNotUsed.Add(new byte());
                    sPortal.lByteNotUsed.Add(new byte());

                    sPortal.m_wPortalID = Convert.ToUInt16(textBox1.Text);
                    sPortal.m_strNAME = textBox2.Text;
                    sPortal.m_wPortalRegionID = Convert.ToUInt16(textBox3.Text);
                    sPortal.m_wMapID = Convert.ToUInt16(textBox4.Text);
                    sPortal.m_fPosX = Convert.ToSingle(textBox5.Text);
                    sPortal.m_fPosZ = Convert.ToSingle(textBox6.Text);
                    sPortal.lByteNotUsed[0] = Convert.ToByte(textBox7.Text);
                    sPortal.lByteNotUsed[1] = Convert.ToByte(textBox8.Text);

                    int nSelectedIndex = listBox1.SelectedIndex;
                    loadedTcds[0][nSelectedIndex] = sPortal;
                    FillListBox(nSelectedIndex);

                    MessageBox.Show("Selected portal saved!");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    MessageBox.Show("You have not selected any item to save");
                }
            }
        }

        private void ReadPortals()
        {
            for (int i = 0; i < lOfd.Count; i++)
            {
                ReadPortal(lOfd[i].OpenFile());
            }
        }

        private void WritePortals()
        {
            for (int i = 0; i < lSfd.Count; i++)
            {
                WritePortal(lSfd[i].OpenFile());
            }
        }

        private void FillListBox(int nSelectedIndex = -1)
        {
            listBox1.Items.Clear();
            for (int i = 0; i < loadedTcds[0].Count; i++)
            {
                string strPortal = i + ": " + loadedTcds[0][i].m_strNAME;
                listBox1.Items.Add(strPortal);
            }
            if (nSelectedIndex != -1)
                listBox1.SelectedIndex = nSelectedIndex;
        }

        private void ReadPortal(Stream stream)
        {
            CBinaryReader br = new CBinaryReader(stream);
            List<TPortalStruct> loadedTcd = new List<TPortalStruct>();

            ushort wCount = br.ReadUInt16();
            for (ushort i = 0; i < wCount; i++)
            {
                TPortalStruct sPortal = new TPortalStruct();

                // Lists of Not Used
                sPortal.lByteNotUsed = new List<byte>();

                sPortal.m_wPortalID = br.ReadUInt16();
                sPortal.m_strNAME = br.ReadCString();
                sPortal.m_wPortalRegionID = br.ReadUInt16();
                sPortal.m_wMapID = br.ReadUInt16();
                sPortal.m_fPosX = br.ReadSingle();
                sPortal.m_fPosZ = br.ReadSingle();

                sPortal.lByteNotUsed.Add(br.ReadByte());
                sPortal.lByteNotUsed.Add(br.ReadByte());

                loadedTcd.Add(sPortal);
            }

            loadedTcds.Add(loadedTcd);

            br.Close();
        }

        private void WritePortal(Stream stream)
        {
            CBinaryWriter bw = new CBinaryWriter(stream);

            if (loadedTcds.Count > 0)
            {
                List<TPortalStruct> lPortal = new List<TPortalStruct>(loadedTcds[0]);

                ushort nTotalPortals = (ushort)lPortal.Count;

                bw.Write(nTotalPortals);

                for (ushort i = 0; i < nTotalPortals; i++)
                {
                    bw.Write(lPortal[i].m_wPortalID);
                    bw.WriteCString(lPortal[i].m_strNAME);
                    bw.Write(lPortal[i].m_wPortalRegionID);
                    bw.Write(lPortal[i].m_wMapID);
                    bw.Write(lPortal[i].m_fPosX);
                    bw.Write(lPortal[i].m_fPosZ);
                    bw.Write(lPortal[i].lByteNotUsed[0]);
                    bw.Write(lPortal[i].lByteNotUsed[1]);
                }
            }

            bw.Close();
            MessageBox.Show("TPortal saved!");
        }
    }
}
