using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CleanerService
{
    public partial class CleanerService : ServiceBase
    {
        System.Timers.Timer timer;
        string baseDirectory, connectionString;
        int startupInterval, lifeSpan;
        public CleanerService()
        {
            InitializeComponent();
            startupInterval = int.Parse(ConfigurationManager.AppSettings["StartupInterval"]);
            lifeSpan = int.Parse(ConfigurationManager.AppSettings["LifeSpan"]);
            baseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
            connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnString"].ConnectionString;
        }

        protected override void OnStart(string[] args)
        {
            Deletefile();
        }

        protected void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Deletefile();
        }
        public void Deletefile()
        {
            try
            {
                timer = new System.Timers.Timer();
                timer.Interval = (10000) * startupInterval;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                List<string> fileNames = new List<string>();
                foreach (string file in Directory.GetFiles(baseDirectory, "*", SearchOption.AllDirectories))
                {
                    if (DateTime.Now.Subtract(File.GetCreationTime(file)).TotalDays > lifeSpan)
                    {
                        File.Delete(file);
                        fileNames.Add(file);
                    }
                }
                if (fileNames.Count > 0)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string combinedFileNames = string.Join(",", fileNames);
                        string deleteQuery = "Delete From PrintJob Where FileName IN (@files)";
                        SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                        deleteCommand.Parameters.AddWithValue("@files", combinedFileNames);
                        deleteCommand.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Cleaner Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);                
            }

        }

        protected override void OnStop()
        {
        }
    }
}
