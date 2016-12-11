namespace RasPiHealthCheckerService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RasPiHealthCheckerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.RasPiHealthCheckerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // RasPiHealthCheckerServiceProcessInstaller
            // 
            this.RasPiHealthCheckerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.RasPiHealthCheckerServiceProcessInstaller.Password = null;
            this.RasPiHealthCheckerServiceProcessInstaller.Username = null;
            // 
            // RasPiHealthCheckerServiceInstaller
            // 
            this.RasPiHealthCheckerServiceInstaller.ServiceName = "RasPiHealthCheckerService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.RasPiHealthCheckerServiceProcessInstaller,
            this.RasPiHealthCheckerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller RasPiHealthCheckerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller RasPiHealthCheckerServiceInstaller;
    }
}