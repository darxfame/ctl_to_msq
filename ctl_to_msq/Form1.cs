using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ctl_to_msq
{
    public partial class Form1 : Form
    {
        List<string> cteFileAddr = new List<string>();
        string filtercte = "Калибровки ChipTuner(*.cte)|*.cte";
        string baseermessage = "Не верный файл калибровки ";
        string errdownlmes = "Ошибка загрузки";
        string savefolder;
        //Для формирования XML файла, разметка
        string[] RPMValue = new[] { "600.0", "800.0", "1000.0", "1200.0", "1600.0", "2000.0", "2520.0", "3000.0", "3520.0", "4000.0", "4520.0", "5000.0", "5520.0", "6000.0", "7000.0", "8000.0" };
        string[] TPSLoad = new[] { "0.00", "2.00", "4.00", "6.00", "8.00", "10.00", "14.00", "18.00", "23.00", "29.00", "37.00", "46.00", "56.00", "66.00", "80.00", "100.00" };
        double[,] mass = new double[16, 16];

        public Form1()
        {
            InitializeComponent();
            foreach (Button butt in Controls.OfType<Button>())
            {
                if (butt.TabIndex != 10) {
                    butt.Enabled = false;
                }
            }
        }

        public void parceCTE(TextBox txt) //Функция разбора файла калибровки ChiptunerPRO
        {
            StreamReader cte = new StreamReader(txt.Text, Encoding.Default);
            Regex regex = new Regex(@"^X([0-9]+)Z([0-9]+)=(.*)$", RegexOptions.Compiled);
            Regex names = new Regex(@"^Name=(.*)$", RegexOptions.Compiled);
            MatchCollection matches = null;
            MatchCollection namemat = null;
            string CalName = null;
            while (cte.EndOfStream == false)
            {
                string a = cte.ReadLine();
                matches = regex.Matches(a);
                namemat = names.Matches(a);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        mass[Convert.ToInt32(match.Groups[2].Value)-1, Convert.ToInt32(match.Groups[1].Value)-1] = Convert.ToDouble(match.Groups[3].Value);
                        // richTextBox1.AppendText(match.Groups[1] +" " +match.Groups[2] + Environment.NewLine);
                    }
                }

                if (namemat.Count > 0)
                {
                    foreach (Match name in namemat)
                    {
                       // richTextBox1.AppendText(Tr(name.Groups[1].Value)+" - Successfully"+Environment.NewLine);
                        CalName = Tr(name.Groups[1].Value);
                    }
                }
            }
            xmlcreate(CalName);
            // richTextBox1.AppendText(xmlzValuesmass(mass)); //Проверка строк\столбцов                -сюда выводится

        }

        private static string xmlzValuesmass(double[,] ms,string name) //Создание массива данных zValues
        {
            string stroka = null;
            double[,] bit = new double[16, 16];
            string[,] bitst = new string[16, 16];
            for (var i=0; i<16; i++)
            {
                stroka = stroka + Environment.NewLine+ new string(' ', 9);
                for (var j = 0; j < 16; j++)
                {
                    if (name == "Popravka CN ot drosselya")
                    {
                        bit[i, j] = Math.Round(ms[i, j] * 100, 2);
                    }
                    else
                    {
                        bit[i, j] = Math.Round(ms[i, j], 2);
                    }
                    int count = BitConverter.GetBytes(decimal.GetBits((decimal)bit[i, j])[3])[2];
                    bitst[i, j] = Convert.ToString(bit[i, j]);
                    Regex pattern = new Regex("[ , ]");
                    bitst[i, j] = pattern.Replace(bitst[i, j], ".");
                    if (count < 2)
                        {
                            if (count < 1)
                            {
                                stroka = stroka + bitst[i, j] + ".00 ";
                            }
                            else
                            {
                                stroka = stroka + bitst[i, j] + "0 ";
                            }
                        }
                    else
                        {
                                stroka = stroka + bitst[i, j] + " ";
                        }
                }
            }
            return stroka; 
        }

        public void xmlcreate(string name) //Создание xml
        {
            //SaveFolder
            string fileName = savefolder + "/" + name + ".table";
            /////////////////////////
            XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "no"));
            XElement tableData = new XElement("tableData");
            doc.Add(tableData);


            XElement bibliography = new XElement("bibliography");
            
            bibliography.Add(new XAttribute("author", "EFI Analytics - philip.tobin@yahoo.com"),
                new XAttribute("company", "EFI Analytics, copyright 2010, All Rights Reserved."),
                new XAttribute("writeDate", Convert.ToString(DateTime.UtcNow)));


            XElement versionInfo = new XElement("versionInfo");
            versionInfo.Add(new XAttribute("fileFormat", "1.0"));

            XElement table = new XElement("table");
            table.Add(new XAttribute("cols", "16"),
                      new XAttribute("rows", "16")
                );
           //структура данных оси Х (обороты)
            XElement xAxis = new XElement("xAxis");
            xAxis.Add(new XAttribute("cols", "16"),
                      new XAttribute("name", "RPMValue"));
            //Заменить на Массив
            for (var i = 0; i < RPMValue.Count();i++)
            {
                xAxis.Value = xAxis.Value + Environment.NewLine + new string(' ', 9) + RPMValue[i]; //Формирование строки новым способом
                if(i == RPMValue.Count()-1)
                {
                    xAxis.Value = xAxis.Value + Environment.NewLine + new string(' ', 4);
                }
            }
           // xAxis.Value = xAxis.Value + Environment.NewLine + new string(' ', 9); //Формируем конец строки
            table.Add(xAxis);

            //структура данных оси Y (нагрузка/Дроссель)
            XElement yAxis = new XElement("yAxis");
            yAxis.Add(new XAttribute("name", "LoadEngine"),
                      new XAttribute("rows", "16"));
            for (var i = 0; i < TPSLoad.Count(); i++)
            {
                yAxis.Value = yAxis.Value + Environment.NewLine + new string(' ', 9) + TPSLoad[i]; //Формирование строки новым способом
                if (i == TPSLoad.Count() - 1)
                {
                    yAxis.Value = yAxis.Value + Environment.NewLine + new string(' ', 4);
                }
            }
           // yAxis.Value = yAxis.Value + Environment.NewLine + new string(' ', 9); //Формируем конец строки
            table.Add(yAxis);
            //структура данных оси Z (Числовые значения)
            XElement zValues = new XElement("zValues");
            zValues.Add(new XAttribute("cols", "16"),
                      new XAttribute("rows", "16"));
            zValues.Value = xmlzValuesmass(mass,name) + Environment.NewLine + new string(' ', 4); 
            table.Add(zValues);

            doc.Root.Add(bibliography);
            doc.Root.Add(versionInfo);
            doc.Root.Add(table);

            doc.Save(fileName);
            richTextBox1.AppendText(name + " - Successfully" + Environment.NewLine);

        }

        //Проверка на валидность
        private bool check(string filename,string text)
        {
            StreamReader sr = new StreamReader(filename, Encoding.Default);
            string name = null;
            var count = 0;
            while(count < 2)
            {
                name = sr.ReadLine();
                count++;
            }
            if (name != null)
            {
                if (name.Contains(text)) { return true; } else { return false; }
            }
            else { return false; }

        }
        //Открытие файла и запись адреса
        private void openfile(string name,TextBox txt,int number) {
            OpenFileDialog pcn = new OpenFileDialog();
            string FileAddr = null;
            pcn.Filter = filtercte;
            pcn.FilterIndex = 1;
            if (pcn.ShowDialog() == DialogResult.OK)
            {
                FileAddr = pcn.FileName;
            if (check(FileAddr, name) == true)
            {
                cteFileAddr.Add(pcn.FileName);
                txt.Text = FileAddr;
               // txt.ReadOnly = false;
            }
            else
            {
                MessageBox.Show(baseermessage+name, errdownlmes, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }
            else
            {
                MessageBox.Show("Не выбран файл калибровки", errdownlmes, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            //richTextBox1.Clear();
                   // foreach (string l in cteFileAddr)
                   // richTextBox1.AppendText(l+" ");
//////////////////////////////////////////
              
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (transliter.Count == 0)
            {
                prepareTranslit();
            }
            var count = 0;
            foreach (TextBox textBox in Controls.OfType<TextBox>())
            {
                if (textBox.Text == "")
                {
                    if (count < 1)
                    {
                        MessageBox.Show("Отсутствующие калибровки не будут сконвертированы ", errdownlmes, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        count++;
                    }
                }
                else
                {

                    if (textBox.TabIndex < 5)
                    {
                        parceCTE(textBox);
                    }
                    
                }
            }
            richTextBox1.AppendText(Environment.NewLine);
        }

        public static string Tr(string sourceText)
        {
            StringBuilder ans = new StringBuilder();
            for (int i = 0; i < sourceText.Length; i++)
            {
                if (transliter.ContainsKey(sourceText[i].ToString()))
                {
                    ans.Append(transliter[sourceText[i].ToString()]);
                }
                else
                {
                    ans.Append(sourceText[i].ToString());
                }
            }
            return ans.ToString();
        }

        private static Dictionary<string,string> transliter = new Dictionary<string, string>();
        private static void prepareTranslit() //Трансляция в транслит названия
        {
            string[] Rus = new[] { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я", "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] Eng = new[] { "a","b","v","g","d","e","yo","zh","z","i","j","k","l","m","n","o","p","r","s","t","u","f","h","c","ch","sh","sch","j","i","j","e","yu","ya","A","B","V","G","D","E","Yo","Zh","Z","I","J","K","L","M","N","O","P","R","S","T","U","F","H","C","Ch","Sh","Sch","J","I","J","E","Yu","Ya"};
            for(var i = 0; i<Rus.Count();i++)
            {
                transliter.Add(Rus[i],Eng[i]);
            }
        }

        private void button5_Click(object sender, EventArgs e) //SaveFolder
        {
            folderBrowserDialog1.SelectedPath = Environment.CurrentDirectory;
            folderBrowserDialog1.ShowDialog();
            savefolder = folderBrowserDialog1.SelectedPath;
            foreach (Button butt in Controls.OfType<Button>())
            {
                if (butt.TabIndex != 10)
                {
                    butt.Enabled = true;
                }
            }
            label2.Text = savefolder;
        }
    }
}
