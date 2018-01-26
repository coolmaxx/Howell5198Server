using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Howell.Net.DataService;

namespace HW5198Service
{

    /// <summary>
    /// 服务运行配置信息
    /// </summary>
    public  class ServiceConfiguration
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static ServiceConfiguration Instance { get; private set; }
        static ServiceConfiguration()
        {
            lock (typeof(ServiceConfiguration))
            {
                Instance = new ServiceConfiguration();
            }
        }
        private String m_AuthenticationServiceAddress = null;
        /// <summary>
        /// 认证服务地址
        /// </summary>
        public String AuthenticationServiceAddress
        {
            get
            {
                if (m_AuthenticationServiceAddress == null)
                {
                    lock (this)
                    {
                        if (m_AuthenticationServiceAddress == null)
                        {
                            m_AuthenticationServiceAddress = System.Configuration.ConfigurationManager.AppSettings["Authentication"];
                        }
                    }
                }
                return m_AuthenticationServiceAddress;
            }
        }

        private String m_DataManagementServiceAddress = null;
        /// <summary>
        /// 数据服务地址
        /// </summary>
        public String DataManagementServiceAddress
        {
            get
            {
                if (m_DataManagementServiceAddress == null)
                {
                    lock (this)
                    {
                        if (m_DataManagementServiceAddress == null)
                        {
                            m_DataManagementServiceAddress = System.Configuration.ConfigurationManager.AppSettings["DataManagement"];
                        }
                    }
                }
                return m_DataManagementServiceAddress;
            }
        }

        private String m_MTServerIP = null;
        /// <summary>
        /// 流转服务器IP
        /// </summary>
        public String MTServerIP
        {
            get
            {
                if (m_MTServerIP == null)
                {
                    lock (this)
                    {
                        if (m_MTServerIP == null)
                        {
                            m_MTServerIP = System.Configuration.ConfigurationManager.AppSettings["MTServerIP"];
                        }
                    }
                }
                return m_MTServerIP;
            }
        }
        private int? m_MTServerPort = null;
        /// <summary>
        /// 流转服务器端口
        /// </summary>
        public int MTServerPort
        {
            get
            {
                if (m_MTServerPort == null)
                {
                    lock (this)
                    {
                        if (m_MTServerPort == null)
                        {
                            m_MTServerPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MTServerPort"]);
                        }
                    }
                }
                return (int)m_MTServerPort;
            }
        }


        private String m_UserName = null;
        /// <summary>
        /// 登陆流转服务器用的账户
        /// </summary>
        public String UserName
        {
            get
            {
                if (m_UserName == null)
                {
                    lock (this)
                    {
                        if (m_UserName == null)
                        {
                            m_UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
                        }
                    }
                }
                return m_UserName;
            }
        }

        private String m_Password = null;
        /// <summary>
        /// 登陆流转服务器用的账户
        /// </summary>
        public String Password
        {
            get
            {
                if (m_Password == null)
                {
                    lock (this)
                    {
                        if (m_Password == null)
                        {
                            m_Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
                        }
                    }
                }
                return m_Password;
            }
        }

        private int? m_HW5198ServerPort = null;
        /// <summary>
        /// HW5198Server侦听端口
        /// </summary>
        public int HW5198ServerPort
        {
            get
            {
                if (m_HW5198ServerPort == null)
                {
                    lock (this)
                    {
                        if (m_HW5198ServerPort == null)
                        {
                            m_HW5198ServerPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["HW5198ServerPort"]);
                        }
                    }
                }
                return (int)m_HW5198ServerPort;
            }
        }

        private int? m_MaxConnectionNumber = null;
        /// <summary>
        /// 最大连接的客户端数量
        /// </summary>
        public int MaxConnectionNumber
        {
            get
            {
                if (m_MaxConnectionNumber == null)
                {
                    lock (this)
                    {
                        if (m_MaxConnectionNumber == null)
                        {
                            m_MaxConnectionNumber = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxConnectionNumber"]);
                        }
                    }
                }
                return (int)m_MaxConnectionNumber;
            }
        }
        private String m_DeviceServiceAddress = null;
        /// <summary>
        /// 数据服务地址
        /// </summary>
        public String DeviceServiceAddress
        {
            get
            {
                if (m_DeviceServiceAddress == null)
                {
                    lock (this)
                    {
                        if (m_DeviceServiceAddress == null)
                        {
                            m_DeviceServiceAddress = System.Configuration.ConfigurationManager.AppSettings["DeviceServiceAddress"];
                        }
                    }
                }
                return m_DeviceServiceAddress;
            }
        }
    }
}
