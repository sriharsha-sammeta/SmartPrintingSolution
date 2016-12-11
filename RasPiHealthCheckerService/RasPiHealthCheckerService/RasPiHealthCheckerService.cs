using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RasPiHealthCheckerService
{
    public partial class RasPiHealthCheckerService : ServiceBase
    {
        System.Timers.Timer timer;
        string connectionString, rasPiSelectQuery, rasPiUpdateQuery;
        int startupInterval;
        public RasPiHealthCheckerService()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnString"].ConnectionString;
            rasPiSelectQuery = ConfigurationManager.AppSettings["RasPiQuery"];
            rasPiUpdateQuery = ConfigurationManager.AppSettings["RasPiUpdateQuery"];
            startupInterval = int.Parse(ConfigurationManager.AppSettings["StartupInterval"]);
        }

        protected void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckRaspiStatus();
        }

        protected override void OnStart(string[] args)
        {
            CheckRaspiStatus();
        }

        public void CheckRaspiStatus()
        {
            try
            {
                timer = new System.Timers.Timer();
                timer.Interval = (60000) * startupInterval;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                bool pingable = false;
                Ping pinger = new Ping();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(rasPiSelectQuery, conn);
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            string ipAddress = reader.GetString(0);
                            PingReply reply = pinger.Send(ipAddress);
                            pingable = reply.Status == IPStatus.Success;
                            SqlCommand updateCommand = new SqlCommand(rasPiUpdateQuery, conn);
                            updateCommand.Parameters.AddWithValue("@Status", pingable ? 1 : 0);
                            updateCommand.Parameters.AddWithValue("@IpAddress", ipAddress);
                            updateCommand.ExecuteNonQuery();
                            updateCommand.Dispose();

                        }
                    }
                    reader.Close();
                    command.Dispose();                    
                }
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "RasPiHealthChecker Service";
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
