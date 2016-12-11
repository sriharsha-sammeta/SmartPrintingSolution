namespace CleanerService
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
            this.CleanerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.CleanerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // CleanerServiceProcessInstaller
            // 
            this.CleanerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.CleanerServiceProcessInstaller.Password = null;
            this.CleanerServiceProcessInstaller.Username = null;
            // 
            // CleanerServiceInstaller
            // 
            this.CleanerServiceInstaller.ServiceName = "CleanerService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.CleanerServiceProcessInstaller,
            this.CleanerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller CleanerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller CleanerServiceInstaller;
    }
}