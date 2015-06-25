using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            new Thread(()=>{
            using (var backgroundWorker = new BackgroundWorker())
            {
                backgroundWorker.DoWork += (s, e) =>
                {
                    if (File.Exists("text.txt"))
                        File.Delete("text.txt");

                    using (StreamWriter sr = new StreamWriter("text.txt"))
                        {
                            for (int i = 0; i < 20000; i++)
                                sr.WriteLine(i.ToString() + "kkkkk;" + i.ToString() + ";" + i.ToString() + ";" + i.ToString());
                            sr.Flush();
                        }                
                };
                backgroundWorker.RunWorkerCompleted += (s, e) =>
                {
                    ReadDT();
                };
                backgroundWorker.RunWorkerAsync();
            }
            }).Start();           
            
        }

        private void ReadDT()
        {            
            BlockingCollection<string> lines = new BlockingCollection<string>();           
            var stage1 = Task.Run(() =>
            {
                    using (StreamReader sr = new StreamReader("text.txt"))
                    {
                        string s;
                        while ((s = sr.ReadLine()) != null)                                                
                            lines.Add(s);                                             
                    }
                lines.CompleteAdding();            
            });

            var stage2 = Task.Run(() =>
            {
                int i = 0;
                dataGridView1.Invoke((Action)(() => dataGridView1.SuspendLayout()));                
                foreach (string line in lines.GetConsumingEnumerable())
                {                               
                    dataGridView1.Invoke((Action)(() => dataGridView1.Rows.Add(line.Split(';'))));
                    dataGridView1.Invoke((Action)(() => dataGridView1.Rows[i].HeaderCell.Value = i.ToString()));
                    i++;
                }                
                dataGridView1.Invoke((Action)(() => dataGridView1.ResumeLayout(false)));
            });
            Task.WaitAll(stage1, stage2);                              
        }

        private void Save()
        {
            if(File.Exists("text.txt"))
            File.Delete("text.txt");

            using (StreamWriter sr = new StreamWriter("text.txt"))
            {
                for (int i = 1; i < dataGridView1.RowCount; i++)
                    sr.WriteLine(dataGridView1.Rows[i-1].Cells[0].Value + ";" + dataGridView1.Rows[i - 1].Cells[1].Value + ";" 
                        + dataGridView1.Rows[i - 1].Cells[2].Value + ";" + dataGridView1.Rows[i - 1].Cells[3].Value);
                sr.Flush();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Save();
        }
    }
}
