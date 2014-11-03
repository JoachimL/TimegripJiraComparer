using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.IsolatedStorage;
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
        private const string DefaultFolder = "c:\\temp\\timegripextended";
        private const string TimeGripDirectoryFile = "TimegripDirectory.txt";
        private const string JiraDirectoryFile = "JiraDirectory.txt";
        private JiraTasks _jiraTasks;
        private TimegripActivities _timegripActivities;

        public Form1()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
        }

        private void browseTimegripFileButton_Click(object sender, EventArgs e)
        {
            openTimegripFileDialog.InitialDirectory = GetTimegripDirectory();
            if (openTimegripFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = openTimegripFileDialog.FileName;
                SetTimegripDirectory(fileName);
                textBox1.Text = fileName;
            }
        }

        private static string GetTimegripDirectory()
        {
            return GetDirectoryFromFile(TimeGripDirectoryFile);
        }

        private static void SetTimegripDirectory(string fileName)
        {
            WriteDirectoryNameToFile(TimeGripDirectoryFile, new FileInfo(fileName).DirectoryName);
        }

        private void browseJiraFileButton_Click(object sender, EventArgs e)
        {
            openJiraFileDialog.InitialDirectory = GetJiraDirectory();
            if (openJiraFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = openJiraFileDialog.FileName;
                SetJiraDirectory(fileName);
                textBox2.Text = fileName;
            }
        }

        private static string GetJiraDirectory()
        {
            return GetDirectoryFromFile(JiraDirectoryFile);
        }

        private static void SetJiraDirectory(string fileName)
        {
            WriteDirectoryNameToFile(JiraDirectoryFile, fileName);
        }

        private static string GetDirectoryFromFile(string directory)
        {
            var isoStore = GetIsolatedStorageFile(directory);
            using (var reader = new StreamReader(isoStore.OpenFile(directory, FileMode.Open)))
            {
                return reader.ReadToEnd();
            }
        }

        private static IsolatedStorageFile GetIsolatedStorageFile(string fileName)
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(fileName))
                return isoStore;
            WriteDefault(fileName, isoStore);
            return isoStore;
        }

        private static void WriteDefault(string fileName, IsolatedStorageFile isoStore)
        {
            WriteToIsolatedStorageFile(fileName, isoStore, DefaultFolder);
        }

        private static void WriteToIsolatedStorageFile(string fileName, IsolatedStorageFile isoStore, string textToWrite)
        {
            using (var stream = new StreamWriter(isoStore.CreateFile(fileName)))
            {
                stream.Write(textToWrite);
            }
        }

        private static void WriteDirectoryNameToFile(string fileName, string directoryFileName)
        {
            WriteToIsolatedStorageFile(fileName, GetIsolatedStorageFile(fileName), directoryFileName);
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
            return openTimegripFileDialog.FileName != "openFileDialog1" &&
                            openJiraFileDialog.FileName != "openFileDialog2";
        }

        private IEnumerable<JiraAndTimegrip> GetCombinedTaskSource()
        {
            using (var tgfileStream = openTimegripFileDialog.OpenFile())
            {
                _timegripActivities = GetTimegripActivities(tgfileStream);
            }
            using (var jtfileStream = openJiraFileDialog.OpenFile())
            {
                _jiraTasks = GetJiraTasks(jtfileStream);
            }

            var jiraAndTimegrips = new List<JiraAndTimegrip>(_jiraTasks.Count());
            foreach (var jiraTask in _jiraTasks.GroupBy(t => t.Task.Trim().ToUpper()).Select(g => new JiraTask()
            {
                Task = g.Key,
                Status = g.Select(t => t.Status).FirstOrDefault(),
                Estimate = TimeSpan.FromMinutes(g.Sum(t => t.Estimate.TotalMinutes)),
                Timespent = TimeSpan.FromMinutes(g.Sum(t => t.Timespent.TotalMinutes)),
                Title = g.Select(t => t.Title).FirstOrDefault()
            }))
            {
                foreach (var timegripActivity in _timegripActivities.GroupBy(a => a.Activity).Select(a => new TimegripActivity()
                {
                    Activity = a.Key,
                    Amount = a.Sum(t => t.Amount),
                    TimeUsed = TimeSpan.FromMinutes(a.Sum(t => t.TimeUsed.TotalMinutes)),
                    Customer = a.Select(t => t.Customer).FirstOrDefault(),
                    Project = a.Select(t => t.Project).FirstOrDefault(),
                }))
                {
                    if (jiraTask.Task.Equals(timegripActivity.Activity, StringComparison.OrdinalIgnoreCase))
                    {
                        jiraAndTimegrips.Add(new JiraAndTimegrip()
                        {
                            Activity = timegripActivity,
                            Task = jiraTask
                        });
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
                var firstOrDefault = _jiraTasks.FirstOrDefault(jt => jt.Task == txtExclude.Text);
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