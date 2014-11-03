using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TimegripExtended.Business.Domain;
using TimegripExtended.Business.Domain.Plurals;
using TimegripExtended.Converters;
using TimegripExtended.Store;

namespace TimegripExtended
{
    public partial class Form1 : Form
    {
        private JiraTasks jiraTasks;

        public Form1()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "c:\\temp\\timegripextended";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.InitialDirectory = "c:\\temp\\timegripextended";
            openFileDialog2.RestoreDirectory = true;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog2.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = GetCombinedTasksSourceIfAvailable().ToList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = GetCombinedTasksSourceIfAvailable().Where(t => t.Timespent != t.TimeUsed).ToList();
        }

        private IEnumerable<JiraAndTimegrip> GetCombinedTasksSourceIfAvailable()
        {
            IEnumerable<JiraAndTimegrip> dataSource = Enumerable.Empty<JiraAndTimegrip>();
            if (!FilesAreSelected())
                MessageBox.Show(@"Select files");
            else
                dataSource = GetCombinedTaskSource();
            return dataSource.OrderByDescending(GetIssueNumber);
        }

        private static int GetIssueNumber(JiraAndTimegrip t)
        {
            var numberString = t.TaskName.Split(new[] { '-' })[1];
            return int.Parse(numberString);
        }


        private bool FilesAreSelected()
        {
            return openFileDialog1.FileName != "openFileDialog1" &&
                            openFileDialog2.FileName != "openFileDialog2";
        }

        private IEnumerable<JiraAndTimegrip> GetCombinedTaskSource()
        {
            var tgfileStream = openFileDialog1.OpenFile();
            var jtfileStream = openFileDialog2.OpenFile();

            var timegripActivities = GetTimegripActivities(tgfileStream);
            jiraTasks = GetJiraTasks(jtfileStream);

            var jiraAndTimegrips = new List<JiraAndTimegrip>(jiraTasks.Count());
            foreach (var jiraTask in jiraTasks)
            {
                foreach (var timegripActivity in timegripActivities)
                {
                    if (jiraTask.Task == timegripActivity.Activity)
                    {
                        if (jiraTask.IsClosed)
                        {
                            jiraAndTimegrips.Add(new JiraAndTimegrip()
                            {
                                Activity = timegripActivity,
                                Task = jiraTask
                            });
                        }
                        else
                        {
                            jiraAndTimegrips.Add(new JiraAndTimegrip()
                            {
                                Activity = timegripActivity,
                                Task = jiraTask
                            });
                        }
                    }
                }
            }
            return jiraAndTimegrips;
        }

        private TimegripActivities GetTimegripActivities(Stream stream)
        {
            var reader = new StreamReader(stream);
            var tglines = new List<string>();
            while (!reader.EndOfStream)
            {
                tglines.Add(reader.ReadLine());
            }

            return TimegripActivityConverter.ConvertToActivities(tglines);
        }

        private JiraTasks GetJiraTasks(Stream stream)
        {
            return JiraTaskConverter.ConvertToList(stream);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var db = new TaskStoreContext())
            {
                var firstOrDefault = jiraTasks.FirstOrDefault(jt => jt.Task == txtExclude.Text);
                db.ExcludedTasks.Add(new ExcludedTask { JiraNumber = txtExclude.Text, Name = firstOrDefault != null ? firstOrDefault.Title : string.Empty, Excluded = DateTime.Now });
                db.SaveChanges();
            }
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                using (var db = new TaskStoreContext())
                {
                    var query = from c in db.ExcludedTasks select c;

                    var users = query.ToList();

                    
                }
            }
        }


        //Data binding directly to a store query (DbSet, DbQuery, DbSqlQuery) is not supported. 
        //Instead populate a DbSet with data, for example by calling Load on the DbSet, 
        //and then bind to local data. For WPF bind to DbSet.Local. For WinForms bind to DbSet.Local.ToBindingList().
    }
}