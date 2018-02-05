using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Howell.Security;
using SuperSocket.Extensions.Protocol;

namespace Howell5198
{
    /// <summary>
    /// 5198协议会话对象
    /// </summary>
    public class Howell5198Session : Session<Howell5198SessionContext>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolSession"></param>
        /// <param name="timeout"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="context"></param>
        public Howell5198Session(FixedHeaderProtocolSession<ProtocolHeader> protocolSession, Int32 timeout, System.Net.IPEndPoint remoteEndPoint, Howell5198SessionContext context)
            : base(protocolSession.SessionID, timeout, remoteEndPoint, context)
        {
            this.ProtocolSession = protocolSession;
        }
        /// <summary>
        /// 协议会话信息
        /// </summary>
        public FixedHeaderProtocolSession<ProtocolHeader> ProtocolSession { get; private set; }
        /// <summary>
        /// 会话连接状态
        /// </summary>
        /// <returns></returns>
        public Boolean IsConnected()
        {
            return ProtocolSession.Connected;
        }
        /// <summary>
        /// 错误号 见ErrorNo静态类
        /// </summary>
        public Int32 ErrorNo { get; set; }
        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="errorNo"></param>
        /// <param name="ProtocolType"></param>
        /// <param name="payload"></param>
        public void Send(Int32 errorNo, UInt32 ProtocolType, byte[] payload)
        {
            if (payload == null)
            {
                ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = ProtocolType, errornum = (UInt32)errorNo, dataLen = 0 }, null));
            }
            else
            {
                ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = ProtocolType, errornum = (UInt32)errorNo, dataLen = (uint)payload.Length }, payload));
            }

        }
        ///// <summary>
        // /// 尝试发送数据包
        ///// </summary>
        ///// <param name="errorNo"></param>
        ///// <param name="ProtocolType"></param>
        ///// <param name="payload"></param>
        ///// <returns></returns>
        // public Boolean TrySend(Int32 errorNo, UInt32 ProtocolType, byte[] payload)
        // {
        //     if (payload == null)
        //     {
        //         return ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = ProtocolType, errornum = (UInt32)errorNo, dataLen = 0 }, null));
        //     }
        //     else
        //     {
        //         return ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = ProtocolType, errornum = (UInt32)errorNo, dataLen = (uint)payload.Length }, payload));
        //     }
        // }
        /// <summary>
        /// 尝试发送数据包
        /// </summary>
        /// <param name="errorNo"></param>
        /// <param name="protocolType"></param>
        /// <param name="payload"></param>
        /// <param name="tryTimes">尝试重新发送的次数</param>
        /// <param name="tryInterval">尝试重新发送的间隔，单位毫秒</param>
        /// <returns></returns>
        public Boolean TrySend(Int32 errorNo, UInt32 protocolType, byte[] payload, Int32 tryTimes = 0, Int32 tryInterval = 0)
        {
            int i = 0;
            do
            {
                if (payload == null)
                {
                    if (ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = protocolType, errornum = (UInt32)errorNo, dataLen = 0 }, null)) == true)
                        return true;
                }
                else
                {
                    if (ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = protocolType, errornum = (UInt32)errorNo, dataLen = (uint)payload.Length }, payload)) == true)
                        return true;
                }
                ++i;
                System.Threading.Thread.Sleep(tryInterval);
            } while (i < tryTimes);
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                if (ProtocolSession != null && ProtocolSession.Connected)
                {
                    ProtocolSession.Close();
                    ProtocolSession = null;
                }
            }
        }
    }

    /// <summary>
    /// 会话上下文对象
    /// </summary>
    public class Howell5198SessionContext
    { /// <summary>
        /// 会话上下文对象
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="isMediaStreamSession">是否为流的会话</param>
        /// <param name="isMediaStreamSession">会话ID</param>
        public Howell5198SessionContext(String userName, String password, String sessionID)
            : this(userName, password, sessionID, false)
        {

        }
        /// <summary>
        /// 会话上下文对象
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="sessionID">会话ID</param>
        /// <param name="loggedIn">是否已登录</param>
        public Howell5198SessionContext(String userName, String password, String sessionID, Boolean loggedIn)
        {
            this.m_UserName = userName;
            this.m_Password = password;
            this.SessionID = sessionID;
            this.m_LoggedIn = loggedIn;
        }
        /// <summary>
        /// 会话ID
        /// </summary>
        public String SessionID { get; private set; }
        private String m_UserName;
        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName
        {
            get
            {
                lock (this)
                {
                    return m_UserName;
                }
            }
        }
        private String m_Password;
        /// <summary>
        /// 密码
        /// </summary>
        public String Password
        {
            get
            {
                lock (this)
                {
                    return m_Password;
                }
            }

        }
        private Boolean m_LoggedIn = false;
        /// <summary>
        /// 是否已登录系统
        /// </summary>
        public Boolean LoggedIn
        {
            get
            {
                lock (this)
                {
                    return m_LoggedIn;
                }
            }
        }
        /// <summary>
        /// 设置是否已登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="loggedIn"></param>
        internal void SetLoggedIn(String userName, String password, Boolean loggedIn)
        {
            lock (this)
            {
                m_UserName = userName;
                m_Password = password;
                m_LoggedIn = loggedIn;
            }
        }
    }
}
