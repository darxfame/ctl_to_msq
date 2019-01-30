using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ctl_to_msq
{
    public partial class Form1 : Form
    {
        List<string> cteFileAddr = new List<string>();
        string uozFileName = null;
        string filtercte = "Калибровки ChipTuner(*.cte)|*.cte";
        string baseermessage = "Не верный файл калибровки ";
        string errdownlmes = "Ошибка загрузки";

        public Form1()
        {
            InitializeComponent();
        }

        //Проверка на валидность
        public bool check(string filename,string text)
        {
            StreamReader sr = new StreamReader(filename, Encoding.Default);
            string name = null;
            var count = 0;
            while(count < 2)
            {
                name = sr.ReadLine();
                count++;
               
            }
            if (name.Contains(text)) { return true; } else { return false; }

        }
        //Открытие файла и запись адреса
        public void openfile(string name,TextBox txt,int number) {
            OpenFileDialog pcn = new OpenFileDialog();
            string FileAddr = null;
            pcn.Filter = filtercte;
            pcn.FilterIndex = 1;
            if (pcn.ShowDialog() == DialogResult.OK)
            {
                FileAddr = pcn.FileName;

            }

            if (check(FileAddr, name) == true)
            {
                cteFileAddr.Add(pcn.FileName);
                txt.Text = FileAddr;
                txt.ReadOnly = false;
            }
            else
            {
                MessageBox.Show(baseermessage+name, errdownlmes, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Кнопки загрузки
        private void button1_Click(object sender, EventArgs e)
        {
            openfile(button1.Text, textBox1,0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openfile(button2.Text, textBox2,1);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            openfile(button3.Text, textBox3, 2);

//Проверка наличия адресов в списке
            richTextBox1.Clear();
                    foreach (string l in cteFileAddr)
                    richTextBox1.AppendText(l+" ");
//////////////////////////////////////////
              
        }
    }
}
