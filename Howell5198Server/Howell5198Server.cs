using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Extensions.Protocol;
using Howell5198.Protocols;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Howell5198
{
    public class Howell5198Server : IDisposable
    {
        private FixedHeaderProtocolServer<ProtocolHeader> m_Server = null;
        private Dictionary<String, Howell5198Session> m_Sessions = new Dictionary<String, Howell5198Session>();
        private Dictionary<String, Howell5198Session> m_UnLoginSessions = new Dictionary<String, Howell5198Session>();
        private Dictionary<String, StreamSession> m_StreamSessions = new Dictionary<String, StreamSession>();
        private IHowell5198ServerAppInstance m_AppInstance = null;
        //public const int CLIENT_MAX_COUNT = 100;
        //StreamThread[] streamthread = new StreamThread[CLIENT_MAX_COUNT];
        //Thread[] thread = new Thread[CLIENT_MAX_COUNT];
        /// <summary>
        /// 创建语言对讲服务器对象
        /// </summary>
        /// <param name="port">本地端口号</param>
        /// <param name="appInstance">协议逻辑实现</param>
        public Howell5198Server(Int32 port, IHowell5198ServerAppInstance appInstance)
        {
            AliveInterval = 30;
            m_AppInstance = appInstance;
            m_Server = new FixedHeaderProtocolServer<ProtocolHeader>(port);
            m_Server.NewRequestReceived += new SuperSocket.SocketBase.RequestHandler<FixedHeaderProtocolSession<ProtocolHeader>, FixedHeaderPackageInfo<ProtocolHeader>>(m_Server_NewRequestReceived);
            m_Server.NewSessionConnected += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>>(m_Server_NewSessionConnected);
            m_Server.SessionClosed += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>, SuperSocket.SocketBase.CloseReason>(m_Server_SessionClosed);
        }
          /// <summary>
        /// 创建语言对讲服务器对象
        /// </summary>
        /// <param name="port">本地端口号</param>
        /// <param name="appInstance">协议逻辑实现</param>
        /// <param name="maxConnectionNumber">最大连接客户端上限</param>
        /// <param name="ssl">是否启用SSL安全连接</param>
        /// <param name="certFilePath">证书路径</param>
        /// <param name="certPassword">证书密码</param>
        public Howell5198Server(Int32 port, IHowell5198ServerAppInstance appInstance, int maxRequestLength, int maxConnectionNumber, bool ssl, string certFilePath, string certPassword, bool clientCertificateRequired)
        {
            AliveInterval = 30;
            m_AppInstance = appInstance;
            m_Server = new FixedHeaderProtocolServer<ProtocolHeader>(port, maxRequestLength, maxConnectionNumber, ssl, certFilePath, certPassword, clientCertificateRequired);
            m_Server.NewRequestReceived += new SuperSocket.SocketBase.RequestHandler<FixedHeaderProtocolSession<ProtocolHeader>, FixedHeaderPackageInfo<ProtocolHeader>>(m_Server_NewRequestReceived);
            m_Server.NewSessionConnected += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>>(m_Server_NewSessionConnected);
            m_Server.SessionClosed += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>, SuperSocket.SocketBase.CloseReason>(m_Server_SessionClosed);
            m_Server.ValidateSessionCertificate += new ValidateSessionCertificate<ProtocolHeader>(Server_ValidateSessionCertificate);
        }
        public static void Calculate(object arg)
        {
            Random ra = new Random();//随机数对象
            Thread.Sleep(ra.Next(10, 100));//随机休眠一段时间
            Console.WriteLine(arg);
        }

        private Boolean m_IsStarted = false;
        /// <summary>
        /// 服务是否已启动
        /// </summary>
        public Boolean IsStarted
        {
            get
            {
                lock (this)
                {
                    return m_IsStarted;
                }
            }
            private set
            {
                lock (this)
                {
                    m_IsStarted = value;
                }
            }
        }
        /// <summary>
        /// 来自客户端的流连接总数
        /// </summary>
        public int StreamCount
        {
            get
            {
                return m_StreamSessions.Count;
            }
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                CheckDisposed();
                if (this.IsStarted == true) throw new InvalidOperationException("5198Server has already started.");
                m_Server.Start();
                this.IsStarted = true;
            }
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            Exception exc = null;
            lock (this)
            {
                CheckDisposed();
                try
                {
                    if (m_Server.State == SuperSocket.SocketBase.ServerState.Running)
                    {
                        m_Server.Stop();
                    }
                }
                catch (Exception ex)
                {
                    exc = ex;
                }
                finally
                {
                    this.IsStarted = false;
                }
            }
            RaisingError(exc);
        }
        //客户端断开
        void m_Server_SessionClosed(FixedHeaderProtocolSession<ProtocolHeader> session, SuperSocket.SocketBase.CloseReason value)
        {
            Howell5198Session vcSession = null;
            StreamSession vcSession2 = null;
            if (m_Sessions.SyncRemoveGet(session.SessionID, out vcSession))
            {
                try
                {
                    if (SessionClosed != null)
                    {
                        SessionClosed(this, new SessionClosedEventArgs(vcSession, value.ToString()));
                    }
                }
                catch(Exception ex)
                {
                    RaisingError(ex);             
                }
                finally
                {
                    vcSession.Dispose();
                } 
            }
            else if (m_UnLoginSessions.SyncRemoveGet(session.SessionID, out vcSession))
            {
               try
               {
                   if (m_StreamSessions.SyncRemoveGet(session.SessionID, out vcSession2))
                   {
                       try
                       {
                           if (StreamSessionClosed != null)
                           {
                               StreamSessionClosed(this, new StreamSessionClosedEventArgs(vcSession2, value.ToString()));
                           }
                       }
                       catch (Exception ex)
                       {
                           RaisingError(ex);
                       }
                       finally
                       {
                           vcSession2.Dispose();
                       }
                   }
               }
                finally
               {
                   vcSession.Dispose();
               }
                
            }
        }
        //客户端连接
        void m_Server_NewSessionConnected(FixedHeaderProtocolSession<ProtocolHeader> session)
        {
            //不做任何处理
        }
        //数据接收
        void m_Server_NewRequestReceived(FixedHeaderProtocolSession<ProtocolHeader> session, FixedHeaderPackageInfo<ProtocolHeader> requestInfo)
        {

            try
            {
                Howell5198Session vcSession = null;
                //未注册
                if (m_Sessions.SyncTryGet(session.SessionID, out vcSession) == false)
                {
                    Howell5198Session UnLoginSession = null;
                    if (requestInfo.Header.proType != COMMAND.Login )
                    {
                        if (m_UnLoginSessions.SyncContainsKey(session.SessionID) == false)
                        {
                            UnLoginSession = new Howell5198Session(session, AliveInterval, session.RemoteEndPoint,
                                new SessionContext("", "", session.SessionID));
                            m_UnLoginSessions.SyncAdd(session.SessionID, UnLoginSession);
                        }
                        else
                        {
                            UnLoginSession = m_UnLoginSessions[session.SessionID];
                        }
                    }
                    
                    if (requestInfo.Header.proType == COMMAND.Login)
                    {
                        LoginRequest request = new LoginRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        LoginResponse response = m_AppInstance.Login(request);
                        Boolean logined = (response.Success == 0 )? true : false; 
                        //发送认证应答结果
                        session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                            new ProtocolHeader() { proType = COMMAND.Login, errornum = 0, dataLen = (uint)response.GetLength() },
                           response.GetBytes()));
                        if (logined == true)
                        {
                            ServerInfo serverinfo = m_AppInstance.GetServerInfo();
                            session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                            new ProtocolHeader() { proType = COMMAND.Serverinfo, errornum = 0, dataLen = (uint)serverinfo.GetLength() },
                           serverinfo.GetBytes()));
                            
                            Howell5198Session newSession = new Howell5198Session(session, AliveInterval, session.RemoteEndPoint,
                                new SessionContext(request.UserName, request.Password, session.SessionID));
                              
                            m_Sessions.SyncAdd(session.SessionID, newSession);
                                
                            if (SessionRegistered != null)
                            {
                                SessionRegistered(this, new SessionRegisteredEventArgs(newSession));
                            }
                           
                            
                        }
                    }
                    else if (requestInfo.Header.proType == COMMAND.Main_stream || requestInfo.Header.proType == COMMAND.Sub_stream || requestInfo.Header.proType == COMMAND.Unknow)
                    {
                        if (m_StreamSessions.SyncContainsKey(session.SessionID) == true)
                            return;
                        StreamRequest request = new StreamRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);

                        int type = requestInfo.Header.proType == COMMAND.Sub_stream ? 1: 0;
                        StreamSession newSession = new StreamSession(session, AliveInterval, session.RemoteEndPoint,
                              new StreamSessionContext(session.SessionID, request.ChannelNo, type));

                        StreamResponse response = m_AppInstance.GetStream(newSession);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                        if (response.Success == -1)
                        {
                            newSession.Dispose();
                            return;
                        }
                     
                        if (session.Connected)
                        {
                            m_StreamSessions.SyncAdd(session.SessionID, newSession);
                        }
                        else
                        {
                            newSession.Dispose();
                            return;
                        }
                        if (session.Connected)
                        {
                            if (StreamSessionRegistered != null)
                            {
                                StreamSessionRegistered(this, new StreamSessionRegisteredEventArgs(newSession));
                            }
                        }
                    }
                    else if (requestInfo.Header.proType == COMMAND.GetFile)
                    {
                        if (m_StreamSessions.SyncContainsKey(session.SessionID) == true)
                            return;
                        GetFileRequest request = new GetFileRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);

                        StreamSession newSession = new StreamSession(session, AliveInterval, session.RemoteEndPoint,
                             new StreamSessionContext(session.SessionID, request.ChannelNo, 2));                
                        if (session.Connected)
                        {
                            m_StreamSessions.SyncAdd(session.SessionID, newSession);
                        }
                        else
                        {
                            newSession.Dispose();
                            return;
                        }
                       m_AppInstance.GetFile(newSession, request);//获得GetFileResponse.Type=0的包
                    }
                    else if (requestInfo.Header.proType == COMMAND.GetNetHead)
                    {
                        GetNetHeadRequest request = new GetNetHeadRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        GetNetHeadResponse response = m_AppInstance.GetNetHead(UnLoginSession, request);
                        if (response != null)
                        {
                            UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                        }
                        else
                        {
                            UnLoginSession.Send(1, requestInfo.Header.proType, null);
                        }
                    }
                    else if (requestInfo.Header.proType == COMMAND.ForceIFrame)
                    {
                        StreamRequest request = new StreamRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        StreamResponse response = new StreamResponse();
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_color)
                    {
                        GetColorRequest request = new GetColorRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        ColorInfo response = m_AppInstance.GetColor(UnLoginSession, request.ChannelNo);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_color)
                    {
                        ColorInfo request = new ColorInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetColorResponse response = m_AppInstance.SetColor(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_osdchannel)
                    {
                        GetOsdChannelRequest request = new GetOsdChannelRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        OsdChannelInfo response = m_AppInstance.GetOsdChannel(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_osdchannel)
                    {
                        OsdChannelInfo request = new OsdChannelInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetOsdChannelResponse response = m_AppInstance.SetOsdChannel(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_osddate)
                    {
                        GetOsdDateRequest request = new GetOsdDateRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        OsdDateInfo response = m_AppInstance.GetOsdDate(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_osddate)
                    {
                        OsdDateInfo request = new OsdDateInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetOsdDateResponse response = m_AppInstance.SetOsdDate(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_videoquality)
                    {
                        GetVideoQualityRequest request = new GetVideoQualityRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        VideoQualityInfo response = m_AppInstance.GetVideoQuality(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_videoquality)
                    {
                        VideoQualityInfo request = new VideoQualityInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetVideoQualityResponse response = m_AppInstance.SetVideoQuality(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_streamtype)
                    {
                        GetStreamTypeRequest request = new GetStreamTypeRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        StreamTypeInfo response = m_AppInstance.GetStreamType(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_streamtype)
                    {
                        StreamTypeInfo request = new StreamTypeInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetStreamTypeResponse response = m_AppInstance.SetStreamType(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_netinfo)
                    {
                        NetInfo response = m_AppInstance.GetNetInfo(UnLoginSession);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_netinfo)
                    {
                        NetInfo request = new NetInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetNetInfoResponse response = m_AppInstance.SetNetInfo(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_systemtime)
                    {
                        SystemTimeInfo response = m_AppInstance.GetSystemTime(UnLoginSession);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_systemtime)
                    {
                        SystemTimeInfo request = new SystemTimeInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetSystemTimeResponse response = m_AppInstance.SetSystemTime(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }

                    else if (requestInfo.Header.proType == COMMAND.Restart_device)
                    {
                        RestartDeviceResponse response = m_AppInstance.RestartDevice(UnLoginSession);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }

                    else if (requestInfo.Header.proType == COMMAND.Close_device)
                    {
                        CloseDeviceResponse response = m_AppInstance.CloseDevice(UnLoginSession);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }

                    else if (requestInfo.Header.proType == COMMAND.Reset)
                    {
                        ResetDeviceResponse response = m_AppInstance.ResetDevice(UnLoginSession);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_rs232cfg)
                    {
                        GetRs232CfgRequest request = new GetRs232CfgRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        Rs232CfgInfo response = m_AppInstance.GetRs232Cfg(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_rs232cfg)
                    {
                        Rs232CfgInfo request = new Rs232CfgInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetRs232CfgResponse response = m_AppInstance.SetRs232Cfg(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_ptzrs232cfg)
                    {
                        GetPtzRs232CfgRequest request = new GetPtzRs232CfgRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        PtzRs232CfgInfo response = m_AppInstance.GetPtzRs232Cfg(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_ptzrs232cfg)
                    {
                        PtzRs232CfgInfo request = new PtzRs232CfgInfo();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetPtzRs232CfgResponse response = m_AppInstance.SetPtzRs232Cfg(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Ptzcontrol)
                    {
                        PtzControlRequest request = new PtzControlRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        PtzControlResponse response = m_AppInstance.PtzControl(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.SearchFile)
                    {
                        SearchFileRequest request = new SearchFileRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SearchFileResponse response = m_AppInstance.SearchFile(UnLoginSession, request);
                        if (response!=null)
                        {
                            UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                        }
                        else
                       {
                           session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                           new ProtocolHeader() { proType = requestInfo.Header.proType, errornum = 32 },
                           null));
                       }
                    }
                    else if (requestInfo.Header.proType == COMMAND.GetFileInfo)
                    {
                        GetFileInfoRequest request = new GetFileInfoRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        GetFileInfoResponse response = m_AppInstance.GetFileInfo(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if(requestInfo.Header.proType == COMMAND.RegisterAlarm)
                    {
                        RegisterAlarmRequest request = new RegisterAlarmRequest(); 
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        RegisterAlarmResponse response = m_AppInstance.SetRegisterAlarm(UnLoginSession, request);
                        UnLoginSession.Send(0, requestInfo.Header.proType, response.GetBytes());    
                     }
                    else
                    {
                        //未注册异常应答
                        session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                            new ProtocolHeader() { proType = requestInfo.Header.proType, errornum = 401 },
                            null)); 
                         if (Error != null)
                        {
                            Error(this, new ErrorEventArgs(new Exception(String.Format("不支持的协议类型:{0:X00000000}", requestInfo.Header.proType))));
                        }  
                    }
                }
                //已注册
                else
                {
                    if (requestInfo.Header.proType == COMMAND.Main_stream || requestInfo.Header.proType == COMMAND.Sub_stream || requestInfo.Header.proType == COMMAND.Unknow)
                    {
                        if (Error != null)
                        {
                            Error(this, new ErrorEventArgs(new Exception(String.Format("在协议session中请求流:{0:X00000000}", requestInfo.Header.proType))));
                        }  

                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_color)
                    {
                        GetColorRequest request = new GetColorRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        ColorInfo response = m_AppInstance.GetColor(vcSession,request.ChannelNo);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                   else if (requestInfo.Header.proType == COMMAND.Set_color)
                   {
                       ColorInfo request = new ColorInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetColorResponse response = m_AppInstance.SetColor(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if(requestInfo.Header.proType ==COMMAND.Get_osdchannel)
                   {
                       GetOsdChannelRequest request = new GetOsdChannelRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       OsdChannelInfo response = m_AppInstance.GetOsdChannel(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_osdchannel)
                   {
                       OsdChannelInfo request = new OsdChannelInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetOsdChannelResponse response = m_AppInstance.SetOsdChannel(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_osddate)
                   {
                       GetOsdDateRequest request = new GetOsdDateRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       OsdDateInfo response = m_AppInstance.GetOsdDate(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_osddate)
                   {
                       OsdDateInfo request = new OsdDateInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetOsdDateResponse response = m_AppInstance.SetOsdDate(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_videoquality)
                   {
                       GetVideoQualityRequest request = new GetVideoQualityRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       VideoQualityInfo response = m_AppInstance.GetVideoQuality(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_videoquality)
                   {
                       VideoQualityInfo request = new VideoQualityInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetVideoQualityResponse response = m_AppInstance.SetVideoQuality(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_streamtype)
                   {
                       GetStreamTypeRequest request = new GetStreamTypeRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       StreamTypeInfo response = m_AppInstance.GetStreamType(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_streamtype)
                   {
                       StreamTypeInfo request = new StreamTypeInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetStreamTypeResponse response = m_AppInstance.SetStreamType(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_netinfo)
                   {
                       NetInfo response = m_AppInstance.GetNetInfo(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_netinfo)
                   {
                       NetInfo request = new NetInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetNetInfoResponse response = m_AppInstance.SetNetInfo(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_systemtime)
                   {
                       SystemTimeInfo response = m_AppInstance.GetSystemTime(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_systemtime)
                   {
                       SystemTimeInfo request = new SystemTimeInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetSystemTimeResponse response = m_AppInstance.SetSystemTime(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }

                   else if (requestInfo.Header.proType == COMMAND.Restart_device)
                   {
                       RestartDeviceResponse response = m_AppInstance.RestartDevice(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }

                   else if (requestInfo.Header.proType == COMMAND.Close_device)
                   {
                       CloseDeviceResponse response = m_AppInstance.CloseDevice(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }

                   else if (requestInfo.Header.proType == COMMAND.Reset)
                   {
                       ResetDeviceResponse response = m_AppInstance.ResetDevice(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_rs232cfg)
                   {
                       GetRs232CfgRequest request = new GetRs232CfgRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       Rs232CfgInfo response = m_AppInstance.GetRs232Cfg(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_rs232cfg)
                   {
                       Rs232CfgInfo request = new Rs232CfgInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetRs232CfgResponse response = m_AppInstance.SetRs232Cfg(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_ptzrs232cfg)
                   {
                       GetPtzRs232CfgRequest request = new GetPtzRs232CfgRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       PtzRs232CfgInfo response = m_AppInstance.GetPtzRs232Cfg(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_ptzrs232cfg)
                   {
                       PtzRs232CfgInfo request = new PtzRs232CfgInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetPtzRs232CfgResponse response = m_AppInstance.SetPtzRs232Cfg(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Ptzcontrol)
                   {
                       PtzControlRequest request = new PtzControlRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       PtzControlResponse response = m_AppInstance.PtzControl(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if(requestInfo.Header.proType==COMMAND.SearchFile)
                   {
                       SearchFileRequest request = new SearchFileRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SearchFileResponse response = m_AppInstance.SearchFile(vcSession, request);
                       if(response!=null)
                       {
                           vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                       }
                       else
                       {
                           session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                           new ProtocolHeader() { proType = requestInfo.Header.proType, errornum = 32 },
                           null));
                       }
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetFileInfo)
                   {
                       GetFileInfoRequest request = new GetFileInfoRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       GetFileInfoResponse response = m_AppInstance.GetFileInfo(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.RegisterAlarm)
                   {
                       RegisterAlarmRequest request = new RegisterAlarmRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       RegisterAlarmResponse response = m_AppInstance.SetRegisterAlarm(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                    else if (requestInfo.Header.proType == COMMAND.Get_MotionExSet)
                    {
                        GetMotionExRequest request = new GetMotionExRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        GetMotionExResponse response = m_AppInstance.GetMotionExSet(vcSession, request);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_MotionExSet)
                    {
                        SetMotionExRequest request = new SetMotionExRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetMotionExResponse response = m_AppInstance.SetMotionExSet(vcSession, request);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Get_SubChannelSet)
                    {
                        GetSubChannelSetRequest request = new GetSubChannelSetRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        GetSubChannelSetResponse response = m_AppInstance.GetSubChannelSet(vcSession, request);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.Set_SubChannelSet)
                    {
                        SetSubChannelSetRequest request = new SetSubChannelSetRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetSubChannelSetResponse response = m_AppInstance.SetSubChannelSet(vcSession, request);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.GetTimeEx)
                    {
                        GetNetSyncTimeResponse response = m_AppInstance.GetNetSyncTime(vcSession);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.SetTimeEx)
                    {
                        NetSyncTime request = new NetSyncTime();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        SetNetSyncTimeResponse response = m_AppInstance.SetNetSyncTime(vcSession, request);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                    else if (requestInfo.Header.proType == COMMAND.ForceIFrame)
                    {
                        ForceIFrameRequest request = new ForceIFrameRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        ForceIFrameResponse response = m_AppInstance.ForceIFrame(vcSession, request);
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                    }
                   else if (requestInfo.Header.proType == COMMAND.GetNetHead)
                   {
                       GetNetHeadRequest request = new GetNetHeadRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       GetNetHeadResponse response = m_AppInstance.GetNetHead(vcSession, request);
                       if(response!=null)
                       {
                           vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                       }
                       else
                       {
                           vcSession.Send(1, requestInfo.Header.proType,null);
                       }
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetDeviceCfg)
                   {
                       DeviceConfig response = m_AppInstance.GetDeviceConfig(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetMotionSet)
                   {
                       GetMotionRequest request = new GetMotionRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       GetMotionResponse response = m_AppInstance.GetMotionSet(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SetMotionSet)
                   {
                       SetMotionRequest request = new SetMotionRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetMotionResponse response = m_AppInstance.SetMotionSet(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SyncTime)
                   {
                       SyncTimeRequest request = new SyncTimeRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SyncTimeResponse response = m_AppInstance.SyncTime(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetUser)
                   {
                       DavinciUsers response = m_AppInstance.GetUsers(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.UpdateUser)
                   {
                       UpdateUserRequest request = new UpdateUserRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       UpdateUserResponse response = m_AppInstance.UpdateUser(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.CaptureJpeg)
                   {
                       CaptureRequest request = new CaptureRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       CapturenResponse response = m_AppInstance.CaptureJpeg(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Get_ntpinfo)
                   {
                       NtpInfo response = m_AppInstance.GetNtpInfo(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Set_ntpinfo)
                   {
                       NtpInfo request = new NtpInfo();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       SetNtpInfoResponse response = m_AppInstance.SetNtpInfo(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetPanoCameraList)
                   {
                       tQueryString request = new tQueryString();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.GetPanoCameraList(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.AddPanoCamera)
                   {
                       tPanoCamera request = new tPanoCamera();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.AddPanoCamera(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetPanoCamera)
                   {
                       tPanoCameraId request = new tPanoCameraId();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.GetPanoCamera(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SetPanoCamera)
                   {
                       tPanoCamera request = new tPanoCamera();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.SetPanoCamera(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.DeletePanoCamera)
                   {
                       tPanoCameraId request = new tPanoCameraId();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.DeletePanoCamera(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetServiceVersion)
                   {
                       tServiceVersion response = m_AppInstance.GetServiceVersion(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetDeviceInfo)
                   {
                       tDeviceInfo response = m_AppInstance.GetDeviceInfo(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetDeviceStatus)
                   {
                       tDeviceStatus response = m_AppInstance.GetDeviceStatus(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetNetworkInterface)
                   {
                       tNetworkInterface response = m_AppInstance.GetNetworkInterface(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetDecodingUnitList)
                   {
                       var response = m_AppInstance.GetDecodingUnitList(vcSession);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetDecodingUnit)
                   {
                       tDecodingUnitId request = new tDecodingUnitId();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.GetDecodingUnit(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetRotatingSpeed)
                   {
                       tDecodingUnitId request = new tDecodingUnitId();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.GetRotatingSpeed(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SetRotatingSpeed)
                   {
                       tRotatingSpeed request = new tRotatingSpeed();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.SetRotatingSpeed(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SwitchPanoCamera)
                   {
                       SwitchPanoCameraRequest request = new SwitchPanoCameraRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.SwitchPanoCamera(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetViewPoint)
                   {
                       tDecodingUnitId request = new tDecodingUnitId();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.GetViewPoint(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SetViewPoint)
                   {
                       SetViewPointRequest request = new SetViewPointRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.SetViewPoint(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SetViewPointFixed)
                   {
                       SetViewPointFixedRequest request = new SetViewPointFixedRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.SetViewPointFixed(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.SetViewPointRows)
                   {
                       SetViewPointRowsRequest request = new SetViewPointRowsRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.SetViewPointRows(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.GetPlayerStatus)
                   {
                       tDecodingUnitId request = new tDecodingUnitId();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.GetPlayerStatus(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.OneByOne)
                   {
                       OneByOneRequest request = new OneByOneRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.OneByOne(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Pause)
                   {
                       PauseRequest request = new PauseRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.Pause(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Resume)
                   {
                       ResumeRequest request = new ResumeRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.Resume(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                   else if (requestInfo.Header.proType == COMMAND.Seek)
                   {
                       SeekRequest request = new SeekRequest();
                       request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                       var response = m_AppInstance.Seek(vcSession, request);
                       vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                   }
                    else if (requestInfo.Header.proType == COMMAND.GetCapability)
                    {
                        session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                           new ProtocolHeader() { proType = requestInfo.Header.proType, errornum = 140 },
                           null));
                    }
                   else
                   {
                       //未注册异常应答
                       session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                           new ProtocolHeader() { proType = requestInfo.Header.proType, errornum = 401 },
                           null));
                       if (Error != null)
                       {
                           Error(this, new ErrorEventArgs(new Exception(String.Format("不支持的协议类型:{0:X00000000}", requestInfo.Header.proType))));
                       }  
                   }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (Error != null)
                    {
                        Error(this, new ErrorEventArgs(ex));
                    }
                }
                catch { }
            }  
        }

        static bool Server_ValidateSessionCertificate(FixedHeaderProtocolSession<ProtocolHeader> session, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #region Properties
        /// <summary>
        /// 会话心跳间隔时间，单位：秒
        /// </summary>
        public Int32 AliveInterval { get; set; }
        #endregion
        /// <summary>
        /// 会话注册成功
        /// </summary>
        public event EventHandler<SessionRegisteredEventArgs> SessionRegistered;
        /// <summary>
        /// 协议会话关闭事件
        /// </summary>
        public event EventHandler<SessionClosedEventArgs> SessionClosed;
        /// <summary>
        /// 错误信息事件
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;
        /// <summary>
        /// 发送流事件
        /// </summary>
        public event EventHandler<StreamSessionRegisteredEventArgs> StreamSessionRegistered;
        /// <summary>
        /// 流会话关闭事件
        /// </summary>
        public event EventHandler<StreamSessionClosedEventArgs> StreamSessionClosed;
        private void RaisingError(Exception ex)
        {
            if (Error != null && ex!=null)
            {
                try
                {
                    Error(this, new ErrorEventArgs(ex));
                }
                catch (Exception exx)
                {
                    Console.WriteLine(exx.Message);
                }
            }    
        }
        #region IDisposable 成员
        private Boolean m_IsDisposed = false;
        /// <summary>
        /// whether disposed or not.
        /// </summary>
        protected Boolean IsDisposed
        {
            get
            {
                lock (this)
                {
                    return m_IsDisposed;
                }
            }
            set
            {
                lock (this)
                {
                    m_IsDisposed = value;
                }
            }
        }
        /// <summary>
        /// dispose MTServer.
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                CheckDisposed();
                this.Dispose(true);
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                if (m_Server != null)
                {
                    m_Server.Dispose();
                    m_Server = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void CheckDisposed()
        {
            if (IsDisposed == true)
                throw new ObjectDisposedException(this.GetType().FullName);
        }
        #endregion
    }
    /// <summary>
    /// 协议会话注册事件参数
    /// </summary>
    public class SessionRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// 对讲会话注册事件参数
        /// </summary>
        /// <param name="session">会话信息</param>
        public SessionRegisteredEventArgs(Howell5198Session session)
            : base()
        {
            this.Session = session;
        }
        public SessionRegisteredEventArgs(StreamSession session)
            : base()
        {
            this.StreamSession = session;
        }
        /// <summary>
        /// 协议会话对象
        /// </summary>
        public Howell5198Session Session { get; private set; }
        /// <summary>
        /// 流会话对象
        /// </summary>
        public StreamSession StreamSession { get; private set; }

    }
    /// <summary>
    /// 流会话注册事件参数
    /// </summary>
    public class StreamSessionRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// 发送流事件参数
        /// </summary>
        /// <param name="session">会话信息</param>
        /// <param name="channelno">通道号</param>
        /// <param name="type">0:主码流 1:子码流</param>
        public StreamSessionRegisteredEventArgs(StreamSession session)
            : base()
        {
            this.Session = session;
            //this.ChannelNo = channelno;
            //this.Type = type;
        }
        /// <summary>
        /// 会话对象
        /// </summary>
        public StreamSession Session { get; private set; }
        ///// <summary>
        ///// 通道号
        ///// </summary>
        //public Int32 ChannelNo { get; set; }
        ///// <summary>
        ///// 0:主码流 1:子码流
        ///// </summary>
        //public Int32 Type { get; set; }

    }

    /// <summary>
    /// 协议会话关闭事件参数
    /// </summary>
    public class SessionClosedEventArgs : EventArgs
    {
        /// <summary>
        /// 对讲会话注册事件参数
        /// </summary>
        /// <param name="session">会话信息</param>
        /// <param name="closeReason">关闭原因</param>
        public SessionClosedEventArgs(Howell5198Session session, String closeReason)
            : base()
        {
            this.Session = session;
            this.CloseReason = closeReason;
        }
        /// <summary>
        /// 会话对象
        /// </summary>
        public Howell5198Session Session { get; private set; }
        /// <summary>
        /// 关闭原因
        /// </summary>
        public String CloseReason { get; private set; }
    }

    /// <summary>
    /// 流会话关闭事件参数
    /// </summary>
    public class StreamSessionClosedEventArgs : EventArgs
    {
        /// <summary>
        /// 对讲会话注册事件参数
        /// </summary>
        /// <param name="session">会话信息</param>
        /// <param name="closeReason">关闭原因</param>
        public StreamSessionClosedEventArgs(StreamSession session, String closeReason)
            : base()
        {
            this.StreamSession = session;
            this.CloseReason = closeReason;
        }
        /// <summary>
        /// 流会话对象
        /// </summary>
        public StreamSession StreamSession { get; private set; }
        /// <summary>
        /// 关闭原因
        /// </summary>
        public String CloseReason { get; private set; }
    }
    /// <summary>
    /// 错误信息事件参数
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 错误信息事件参数
        /// </summary>
        /// <param name="exception">异常对象</param>
        public ErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception Exception { get; private set; }
    }
      public interface IHowell5198ServerAppInstance
    {
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="type">暂时未使用</param>
        /// <param name="logName">用户名</param>
        /// <param name="logPassword">密码</param>
        /// <param name="clientUserID">暂时未使用</param>
        /// <returns>服务器应答</returns>
          LoginResponse Login(LoginRequest loginRequest);
          /// <summary>
          /// 获取服务器信息
          /// </summary>
          /// <returns>服务器应答</returns>
          ServerInfo GetServerInfo();
          /// <summary>
          /// 获取实时编码数据
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="channelno">通道号</param>
          /// <param name="type">0:主码流 1:子码流</param>
          /// <returns>服务器应答</returns>
          StreamResponse GetStream(StreamSession session);
          /// <summary>
          /// 获取数据
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="channelno">通道号</param>
          /// <param name="type">0:主码流 1：子码流</param>
          /// <returns>帧数据</returns>
        //  FramePayload GetPayload(Howell5198Session session, Int32 channelno,Int32 type);
          /// <summary>
          /// 获取色彩
          /// </summary>
         /// <param name="session">请求对象的会话信息</param>
          /// <param name="channelno">通道号</param>
          /// <returns>帧数据</returns>
          ColorInfo GetColor(Howell5198Session session, Int32 channelNo);
          /// <summary>
          /// 设置色彩
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setColorRequest"></param>
          /// <returns>GetColorResponse</returns>
          SetColorResponse SetColor(Howell5198Session session, ColorInfo setColorRequest);
          /// <summary>
          /// 获取通道名称
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="getOsdChannelRequest"></param>
          /// <returns>GetOsdChannelResponse</returns>
          OsdChannelInfo GetOsdChannel(Howell5198Session session, GetOsdChannelRequest getOsdChannelRequest);
          /// <summary>
          /// 设置通道名称
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setOsdChannelRequest"></param>
          /// <returns>SetOsdChannelResponse</returns>
          SetOsdChannelResponse SetOsdChannel(Howell5198Session session, OsdChannelInfo setOsdChannelRequest);
          /// <summary>
          /// 获取通道日期
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="getOsdDateRequest"></param>
          /// <returns>GetOsdDateResponse</returns>
          OsdDateInfo GetOsdDate(Howell5198Session session, GetOsdDateRequest getOsdDateRequest);
          /// <summary>
          /// 设置通道日期
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setOsdDateRequest"></param>
          /// <returns>SetOsdDateResponse</returns>
          SetOsdDateResponse SetOsdDate(Howell5198Session session, OsdDateInfo setOsdDateRequest);
          /// <summary>
          /// 获取图像质量
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="getVideoQualityRequest"></param>
          /// <returns>GetVideoQualityResponse</returns>
          VideoQualityInfo GetVideoQuality(Howell5198Session session, GetVideoQualityRequest getVideoQualityRequest);
          /// <summary>
          /// 设置图像质量
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setVideoQualityRequest"></param>
          /// <returns>SetVideoQualityResponse</returns>
          SetVideoQualityResponse SetVideoQuality(Howell5198Session session, VideoQualityInfo setVideoQualityRequest);
          /// <summary>
          /// 获取码流类型
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="getStreamTypeRequest"></param>
          /// <returns>GetStreamTypeResponse</returns>
          StreamTypeInfo GetStreamType(Howell5198Session session, GetStreamTypeRequest getStreamTypeRequest);
          /// <summary>
          /// 设置码流类型
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setStreamTypeRequest"></param>
          /// <returns>SetStreamTypeResponse</returns>
          SetStreamTypeResponse SetStreamType(Howell5198Session session, StreamTypeInfo setStreamTypeRequest);
          /// <summary>
          /// 获取网络
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <returns>GetNetInfoResponse</returns>
          NetInfo GetNetInfo(Howell5198Session session);
          /// <summary>
          /// 设置网络
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setNetInfoRequest"></param>
          /// <returns>SetNetInfoResponse</returns>
          SetNetInfoResponse SetNetInfo(Howell5198Session session, NetInfo setNetInfoRequest);
          /// <summary>
          /// 获取时间
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <returns>GetSystemTimeResponse</returns>
          SystemTimeInfo GetSystemTime(Howell5198Session session);
          /// <summary>
          /// 设置时间
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setSystemTimeRequest"></param>
          /// <returns>SetSystemTimeResponse</returns>
          SetSystemTimeResponse SetSystemTime(Howell5198Session session, SystemTimeInfo setSystemTimeRequest);
          /// <summary>
          /// 重启设备
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <returns>GetSystemTimeResponse</returns>
          RestartDeviceResponse RestartDevice(Howell5198Session session);
          /// <summary>
          /// 关闭设备
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <returns>GetSystemTimeResponse</returns>
          CloseDeviceResponse CloseDevice(Howell5198Session session);
          /// <summary>
          /// 重置设备
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <returns>GetSystemTimeResponse</returns>
          ResetDeviceResponse ResetDevice(Howell5198Session session);
          /// <summary>
          /// 获取串口模式
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <returns>GetSystemTimeResponse</returns>
          Rs232CfgInfo GetRs232Cfg(Howell5198Session session, GetRs232CfgRequest getRs232CfgRequest);
          /// <summary>
          /// 设置串口模式
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setSystemTimeRequest"></param>
          /// <returns>SetSystemTimeResponse</returns>
          SetRs232CfgResponse SetRs232Cfg(Howell5198Session session, Rs232CfgInfo setRs232CfgRequest);
          /// <summary>
          /// 获取PTZ设置
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <returns>GetSystemTimeResponse</returns>
          PtzRs232CfgInfo GetPtzRs232Cfg(Howell5198Session session, GetPtzRs232CfgRequest getRs232CfgRequest);
          /// <summary>
          /// 设置PTZ设置
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="setSystemTimeRequest"></param>
          /// <returns>SetSystemTimeResponse</returns>
          SetPtzRs232CfgResponse SetPtzRs232Cfg(Howell5198Session session, PtzRs232CfgInfo setPtzRs232CfgRequest);
          /// <summary>
          /// PTZ命令控制
          /// </summary>
          /// <param name="session">请求对象的会话信息</param>
          /// <param name="ptzControlRequest"></param>
          /// <returns>PtzControlResponse</returns>
          PtzControlResponse PtzControl(Howell5198Session session, PtzControlRequest ptzControlRequest);
          /// <summary>
          /// 搜索获得回放文件列表
          /// </summary>
          /// <param name="session"></param>
          /// <param name="searchFileRequest"></param>
          /// <returns></returns>
          SearchFileResponse SearchFile(Howell5198Session session, SearchFileRequest searchFileRequest);
          /// <summary>
          /// 获取回放文件
          /// </summary>
          /// <param name="session"></param>
          /// <param name="getFileRequest"></param>
          /// <returns></returns>
          void GetFile(StreamSession session, GetFileRequest getFileRequest);
          /// <summary>
          /// 回放获取文件信息
          /// </summary>
          /// <param name="session"></param>
          /// <param name="getFileInfoRequest"></param>
          /// <returns></returns>
          GetFileInfoResponse GetFileInfo(Howell5198Session session, GetFileInfoRequest getFileInfoRequest);
          /// <summary>
          /// 预览获取视频信息
          /// </summary>
          /// <param name="session"></param>
          /// <param name="getNetHeadRequest"></param>
          /// <returns></returns>
          GetNetHeadResponse GetNetHead(Howell5198Session session, GetNetHeadRequest getNetHeadRequest);
          /// <summary>
          /// 获取设备配置信息
          /// </summary>
          /// <param name="?"></param>
          /// <returns></returns>
          DeviceConfig GetDeviceConfig(Howell5198Session session);
          /// <summary>
          /// 获取移动侦测配置
          /// </summary>
          /// <param name="session"></param>
          /// <param name="getMotionRequest"></param>
          /// <returns></returns>
          GetMotionResponse GetMotionSet(Howell5198Session session, GetMotionRequest getMotionRequest);
          /// <summary>
          /// 设置移动侦测配置
          /// </summary>
          /// <param name="session"></param>
          /// <param name="setMotionRequest"></param>
          /// <returns></returns>
          SetMotionResponse SetMotionSet(Howell5198Session session, SetMotionRequest setMotionRequest);

          GetMotionExResponse GetMotionExSet(Howell5198Session session, GetMotionExRequest getMotionRequest);
          SetMotionExResponse SetMotionExSet(Howell5198Session session, SetMotionExRequest getMotionRequest);
          GetSubChannelSetResponse GetSubChannelSet(Howell5198Session session, GetSubChannelSetRequest getMotionRequest);
          SetSubChannelSetResponse SetSubChannelSet(Howell5198Session session, SetSubChannelSetRequest getMotionRequest);
          GetNetSyncTimeResponse GetNetSyncTime(Howell5198Session session);
          SetNetSyncTimeResponse SetNetSyncTime(Howell5198Session session, NetSyncTime netSyncTime);
          ForceIFrameResponse ForceIFrame(Howell5198Session Session, ForceIFrameRequest forceIFrameRequest);
          /// <summary>
          /// 同步时间
          /// </summary>
          /// <param name="session"></param>
          /// <param name="syncTimeRequest"></param>
          /// <returns></returns>
          SyncTimeResponse SyncTime(Howell5198Session session, SyncTimeRequest syncTimeRequest);
          DavinciUsers GetUsers(Howell5198Session session);
          UpdateUserResponse UpdateUser(Howell5198Session session, UpdateUserRequest updateUserRequest);
          CapturenResponse CaptureJpeg(Howell5198Session session, CaptureRequest captureRequest);
          NtpInfo GetNtpInfo(Howell5198Session session);
          SetNtpInfoResponse SetNtpInfo(Howell5198Session session, NtpInfo ntpInfo);
          RegisterAlarmResponse SetRegisterAlarm(Howell5198Session session, RegisterAlarmRequest registerAlarmRequest);
          tPanoCameraList GetPanoCameraList(Howell5198Session session, tQueryString queryString);
          tFault AddPanoCamera(Howell5198Session session, tPanoCamera panoCamera);
          tPanoCamera GetPanoCamera(Howell5198Session session, tPanoCameraId panoCameraId);
          tFault SetPanoCamera(Howell5198Session session, tPanoCamera panoCamera);
          tFault DeletePanoCamera(Howell5198Session session, tPanoCameraId panoCameraId);
          tServiceVersion GetServiceVersion(Howell5198Session session);
          tDeviceInfo GetDeviceInfo(Howell5198Session session);
          tDeviceStatus GetDeviceStatus(Howell5198Session session);
          tNetworkInterface GetNetworkInterface(Howell5198Session session);
          tDecodingUnitList GetDecodingUnitList(Howell5198Session session);
        tDecodingUnit GetDecodingUnit(Howell5198Session session,tDecodingUnitId decodingUnitId);
        tRotatingSpeed GetRotatingSpeed(Howell5198Session session,tDecodingUnitId decodingUnitId);
        tFault SetRotatingSpeed(Howell5198Session session, tRotatingSpeed rotatingSpeed);
        tFault SwitchPanoCamera(Howell5198Session session, SwitchPanoCameraRequest switchPanoCameraRequest);
        tViewPoint GetViewPoint(Howell5198Session session, tDecodingUnitId decodingUnitId);
        tFault SetViewPoint(Howell5198Session session, SetViewPointRequest setViewPointRequest);
        tFault SetViewPointFixed(Howell5198Session session, SetViewPointFixedRequest setViewPointFixedRequest);
        tFault SetViewPointRows(Howell5198Session session, SetViewPointRowsRequest setViewPointRowsRequest);
        tPlayerStatus GetPlayerStatus(Howell5198Session session, tDecodingUnitId decodingUnitId);
        tFault OneByOne(Howell5198Session session, OneByOneRequest oneByOneRequest);
        tFault Pause(Howell5198Session session, PauseRequest pauseRequest);
        tFault Resume(Howell5198Session session, ResumeRequest resumeRequest);
        tFault Seek(Howell5198Session session, SeekRequest seekRequest);
    }


      //public class  StreamThread//线程类
      //{
      //    public Howell5198Session Session { set; get; }
      //    public int ChannelNo { set; get; }
      //    public int Type { set; get; }//0主码流，1子码流
      //    private IServerAppInstance m_appInstance { set; get; }

      //    //构造函数
      //    public StreamThread(Howell5198Session session,int channelno,int type,IServerAppInstance appInstance)
      //    {
      //        this.Session = session;
      //        this.ChannelNo = channelno;
      //        this.Type = type;
      //        this.m_appInstance = appInstance;
      //    }
      //    //线程执行方法
      //    public void senddate()
      //    {
      //        while(Session.ProtocolSession.Connected)
      //        {
      //            FramePayload framepayload = m_appInstance.GetPayload(Session, ChannelNo, Type);
      //            if(Type==0)
      //            {
      //                Session.Send(0, COMMAND.Main_stream, framepayload.GetBytes());
      //            }
      //            else if(Type==1)
      //            {
      //                Session.Send(0, COMMAND.Sub_stream, framepayload.GetBytes());
      //            }
                 
      //        }
           
      //    }
      //}

}

