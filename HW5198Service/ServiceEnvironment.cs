using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Configuration;

namespace HW5198Service
{
    /// <summary>
    /// 服务运行环境信息
    /// </summary>
    public class ServiceEnvironment : IDisposable
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static ServiceEnvironment Instance { get; private set; }
        static ServiceEnvironment()
        {
            lock (typeof(ServiceEnvironment))
            {
                Instance = new ServiceEnvironment();
            }
        }
        private ServiceEnvironment()
        {
            InitializeEnvironmentVariables();
            InitializeLogger();
        }
        private void InitializeEnvironmentVariables()
        {
            foreach (String key in ConfigurationManager.AppSettings.Keys)
            {
                if (key == "CurrentDirectory")
                {
                    CurrentDirectory = ConfigurationManager.AppSettings[key];
                }
            }
            CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            System.Threading.ThreadPool.SetMinThreads(256, 256);

            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location);
            FileVersion = fvi.FileVersion;

        }
        private void InitializeLogger()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(String.Format("{0}Logger.config", CurrentDirectory)));
            Logger = log4net.LogManager.GetLogger("Log4netRemotingServer");
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }
        /// <summary>
        /// 设置日志写入对象
        /// </summary>
        /// <param name="logger"></param>
        public void SetLogger(log4net.ILog logger)
        {
            this.Logger = logger;
        }
        /// <summary>
        /// 当前路径
        /// </summary>
        public String CurrentDirectory { get; private set; }
        /// <summary>
        /// 服务文件版本号
        /// </summary>
        public String FileVersion { get; private set; }
        /// <summary>
        /// 日志
        /// </summary>
        public log4net.ILog Logger { get; private set; }
    }
}
