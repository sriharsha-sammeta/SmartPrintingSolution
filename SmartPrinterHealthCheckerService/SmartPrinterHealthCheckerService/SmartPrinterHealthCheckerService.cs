using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartPrinterHealthCheckerService
{
    public partial class SmartPrinterHealthCheckerService : ServiceBase
    {
        System.Timers.Timer timer;
        int startupInterval;
        string connectionString, recepientQuery, senderQuery, templatePath;

        public SmartPrinterHealthCheckerService()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnString"].ConnectionString;
            startupInterval = int.Parse(ConfigurationManager.AppSettings["StartupInterval"]);
            recepientQuery = ConfigurationManager.AppSettings["RecepientQuery"];
            senderQuery = ConfigurationManager.AppSettings["SenderQuery"];
            templatePath = ConfigurationManager.AppSettings["TemplatePath"];
        }

        protected void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckSmartPrinterStatus();
        }

        protected override void OnStart(string[] args)
        {
            CheckSmartPrinterStatus();
        }

        public void CheckSmartPrinterStatus()
        {
            try
            {
                timer = new System.Timers.Timer();
                timer.Interval = (60000) * startupInterval;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                List<string> collectedProviders = new List<string>();
                string query = String.Format("*[System/Provider/@Name=\"Smart Printer Service\" or System/Provider/@Name=\"Virtual Printer\" or System/Provider/@Name=\"SmartPrinterWebsite\" or System/Provider/@Name=\"Cleaner Service\" or System/Provider/@Name=\"RasPiHealthChecker Service\"] and *[System[TimeCreated[@SystemTime >= \"{0}\"]]]", DateTime.Now.AddMinutes(-startupInterval).ToUniversalTime().ToString("o")); //*[System[TimeCreated[@SystemTime >= '{1}']]]    DateTime.Now.AddMinutes(-startupInterval)
                EventLogQuery eventsQuery = new EventLogQuery("Application", PathType.LogName, query);
                EventLogReader logReader = new EventLogReader(eventsQuery);
                string recepients = "", senderAddress = "", senderPassword = "";                
                for (EventRecord eventdetail = logReader.ReadEvent(); eventdetail != null; eventdetail = logReader.ReadEvent())
                {                    
                    if (!collectedProviders.Contains(eventdetail.ProviderName))
                    {
                        collectedProviders.Add(eventdetail.ProviderName);                        
                    }
                }                
                if (collectedProviders.Count == 0)
                    return;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand recepientCommand = new SqlCommand(recepientQuery, conn);
                    conn.Open();
                    SqlDataReader reader = recepientCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (String.IsNullOrEmpty(recepients))
                                recepients += reader.GetString(0);
                            else
                                recepients += "," + reader.GetString(0);
                        }
                    }
                    reader.Close();
                    recepientCommand.Dispose();

                    SqlCommand senderCommand = new SqlCommand(senderQuery, conn);
                    reader = senderCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            senderAddress = reader.GetString(0);
                            senderPassword = reader.GetString(1);
                        }
                    }
                    reader.Close();
                    senderCommand.Dispose();                    
                }
                string content = File.ReadAllText(templatePath);
                List<string> servicesList = new List<string>() { "Smart Printer Service", "Virtual Printer", "Cleaner Service", "RasPiHealthChecker Service" };
                foreach (string service in servicesList)
                {
                    string statusTag = "Active", statusColor = "Green";
                    if (collectedProviders.Contains(service))
                    {
                        statusTag = "Error";
                        statusColor = "Red";
                    }
                    content = content.Replace("#" + service + "Status#", statusTag);
                    content = content.Replace("#" + service + "Color#", statusColor);
                }
                SmtpClient client = new SmtpClient("smtp-mail.outlook.com");
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(senderAddress, senderPassword);
                client.EnableSsl = true;
                client.Credentials = credentials;

                using (MailMessage message = new MailMessage(senderAddress, recepients))
                {
                    message.Subject = "Smart Printer Health Report";
                    message.IsBodyHtml = true;
                    message.Body = content;
                    client.Send(message);
                }
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "SmartPrinterHealthChecker Service";
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
