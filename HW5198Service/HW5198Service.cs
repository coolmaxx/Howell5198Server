using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace HW5198Service
{
    partial class HW5198Service : ServiceBase
    {
        public HW5198Service()
        {
            InitializeComponent();
        }
        private Howell5198ServerAppInstance m_server = null;
        protected override void OnStart(string[] args)
        {
            // TODO:  在此处添加代码以启动服务。
            ServiceEnvironment.Instance.SetLogger(new Howell.ServiceModel.Log.ServiceEventLog(this.EventLogger));
            m_server = new Howell5198ServerAppInstance(ServiceConfiguration.Instance.HW5198ServerPort);
            m_server.Start();
            Program.PrintVersion();
        }

        protected override void OnStop()
        {
            // TODO:  在此处添加代码以执行停止服务所需的关闭操作。
            m_server.Stop();
            ServiceEnvironment.Instance.Logger.Info("Service Stopped.");
        }
    }
}
