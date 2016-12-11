﻿namespace SmartPrinterHealthCheckerService
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
            this.SmartPrinterHealthCheckerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SmartPrinterHealthCheckerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // SmartPrinterHealthCheckerServiceProcessInstaller
            // 
            this.SmartPrinterHealthCheckerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.SmartPrinterHealthCheckerServiceProcessInstaller.Password = null;
            this.SmartPrinterHealthCheckerServiceProcessInstaller.Username = null;
            // 
            // SmartPrinterHealthCheckerServiceInstaller
            // 
            this.SmartPrinterHealthCheckerServiceInstaller.ServiceName = "SmartPrinterHealthCheckerService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SmartPrinterHealthCheckerServiceProcessInstaller,
            this.SmartPrinterHealthCheckerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller SmartPrinterHealthCheckerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller SmartPrinterHealthCheckerServiceInstaller;
    }
}