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
        private Dictionary<String, MediaStreamSession> m_StreamSessions = new Dictionary<String, MediaStreamSession>();
        private IHowell5198ServerContract m_AppInstance = null;
        /// <summary>
        /// 创建 Howell5198服务器对象
        /// </summary>
        /// <param name="port">本地端口号</param>
        /// <param name="appInstance">协议逻辑实现</param>
        public Howell5198Server(Int32 port, IHowell5198ServerContract appInstance)
        {
            AliveInterval = 30;
            m_AppInstance = appInstance;
            m_Server = new FixedHeaderProtocolServer<ProtocolHeader>(port);
            m_Server.NewRequestReceived += new SuperSocket.SocketBase.RequestHandler<FixedHeaderProtocolSession<ProtocolHeader>, FixedHeaderPackageInfo<ProtocolHeader>>(m_Server_NewRequestReceived);
            m_Server.NewSessionConnected += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>>(m_Server_NewSessionConnected);
            m_Server.SessionClosed += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>, SuperSocket.SocketBase.CloseReason>(m_Server_SessionClosed);
        }
        /// <summary>
        /// 创建 Howell5198服务器对象
        /// </summary>
        /// <param name="port">本地端口号</param>
        /// <param name="appInstance">协议逻辑实现</param>
        /// <param name="maxConnectionNumber">最大连接客户端上限</param>
        /// <param name="ssl">是否启用SSL安全连接</param>
        /// <param name="certFilePath">证书路径</param>
        /// <param name="certPassword">证书密码</param>
        public Howell5198Server(Int32 port, IHowell5198ServerContract appInstance, int maxRequestLength, int maxConnectionNumber, bool ssl, string certFilePath, string certPassword, bool clientCertificateRequired)
        {
            AliveInterval = 30;
            m_AppInstance = appInstance;
            m_Server = new FixedHeaderProtocolServer<ProtocolHeader>(port, maxRequestLength, maxConnectionNumber, ssl, certFilePath, certPassword, clientCertificateRequired);
            m_Server.NewRequestReceived += new SuperSocket.SocketBase.RequestHandler<FixedHeaderProtocolSession<ProtocolHeader>, FixedHeaderPackageInfo<ProtocolHeader>>(m_Server_NewRequestReceived);
            m_Server.NewSessionConnected += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>>(m_Server_NewSessionConnected);
            m_Server.SessionClosed += new SuperSocket.SocketBase.SessionHandler<FixedHeaderProtocolSession<ProtocolHeader>, SuperSocket.SocketBase.CloseReason>(m_Server_SessionClosed);
            m_Server.ValidateSessionCertificate += new ValidateSessionCertificate<ProtocolHeader>(Server_ValidateSessionCertificate);
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
            MediaStreamSession msSession = null;
            if (m_StreamSessions.SyncRemoveGet(session.SessionID, out msSession))
            {
                try
                {
                    try
                    {
                        RaiseStreamSessionClosed(msSession, value.ToString());
                    }
                    finally
                    {
                        msSession.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    RaisingError(ex);
                }
            }
            Howell5198Session vcSession = null;
            if (m_Sessions.SyncRemoveGet(session.SessionID, out vcSession))
            {
                try
                {
                    try
                    {
                        if (vcSession.Context.LoggedIn)
                        {
                            RaiseSessionClosed(vcSession, value.ToString());
                        }
                    }
                    finally
                    {
                        vcSession.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    RaisingError(ex);
                }
            }
        }
        //客户端连接
        void m_Server_NewSessionConnected(FixedHeaderProtocolSession<ProtocolHeader> session)
        {
            //不做任何处理
        }
        private void Send<TRequest, TResponse>(Howell5198Session session, FixedHeaderPackageInfo<ProtocolHeader> requestInfo, Func<Howell5198Session, TRequest, TResponse> instanceFunction)
            where TRequest : class, IBytesSerialize, new()
            where TResponse : class, IBytesSerialize, new()
        {
            TRequest request = new TRequest();
            request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
            TResponse response = null;
            session.ErrorNo = 0;
            try
            {
                response = instanceFunction(session, request);
            }
            catch (Exception ex)
            {
                session.ErrorNo = ErrorNo.HW_NET_NETWORK_RECV_ERROR;
                Console.WriteLine("Protocol Process Error,{0}.", ex.Message);
            }
            if (response == null)
            {
                session.Send(session.ErrorNo, requestInfo.Header.proType, null);
            }
            else
            {
                session.Send(session.ErrorNo, requestInfo.Header.proType, response.GetBytes());
            }
        }
        //数据接收
        void m_Server_NewRequestReceived(FixedHeaderProtocolSession<ProtocolHeader> session, FixedHeaderPackageInfo<ProtocolHeader> requestInfo)
        {

            try
            {
                Howell5198Session vcSession = null;
                lock (m_Sessions)
                {
                    if (m_Sessions.SyncTryGet(session.SessionID, out vcSession) == false)
                    {
                        vcSession = new Howell5198Session(session, AliveInterval, session.RemoteEndPoint, new Howell5198SessionContext("", "", session.SessionID));
                        m_Sessions.Add(session.SessionID, vcSession);
                    }
                }
                if (requestInfo.Header.proType == ProtocolType.Login)
                {
                    Send<LoginRequest, LoginResponse>(vcSession, requestInfo, m_AppInstance.Login);
                    if (vcSession.ErrorNo == ErrorNo.HW_NET_NOERROR)
                    {
                        LoginRequest request = new LoginRequest();
                        request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                        vcSession.Context.SetLoggedIn(request.UserName, request.Password, true);
                        //登陆成功后跟着发送ServerInfo，并没有额外的request
                        ServerInfo serverinfo = m_AppInstance.GetServerInfo(vcSession);
                        session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                        new ProtocolHeader() { proType = ProtocolType.Serverinfo, errornum = (uint)vcSession.ErrorNo, dataLen = (uint)serverinfo.GetLength() },
                       serverinfo.GetBytes()));

                        //提交设备登录成功事件
                        RaiseSessionRegistered(vcSession);

                    }
                }
                else if (requestInfo.Header.proType == ProtocolType.Main_stream || requestInfo.Header.proType == ProtocolType.Sub_stream || requestInfo.Header.proType == ProtocolType.Unknow)
                {
                    if (m_StreamSessions.SyncContainsKey(session.SessionID) == true) return;
                    StreamRequest request = new StreamRequest();
                    request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);

                    int type = requestInfo.Header.proType == ProtocolType.Sub_stream ? 1 : 0;
                    MediaStreamSession newStreamSession = new MediaStreamSession(session, AliveInterval, session.RemoteEndPoint,
                          new MediaStreamSessionContext(session.SessionID, new MediaStreamIdentifier(request.ChannelNo, type)));

                    StreamResponse response = m_AppInstance.GetStream(newStreamSession);
                    if (response.Success == -1)
                    {
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                        newStreamSession.Dispose();
                    }
                    else
                    {
                        lock (m_StreamSessions)
                        {
                            m_StreamSessions.Add(session.SessionID, newStreamSession);
                        }
                        vcSession.Send(0, requestInfo.Header.proType, response.GetBytes());
                        RaiseStreamSessionRegistered(newStreamSession);
                    }
                }
                else if (requestInfo.Header.proType == ProtocolType.GetFile)
                {
                    if (m_StreamSessions.SyncContainsKey(session.SessionID) == true)
                        return;
                    GetFileRequest request = new GetFileRequest();
                   request.FromBytes(requestInfo.Payload, 0, requestInfo.Payload.Length);
                    MediaStreamSession newSession = new MediaStreamSession(session, AliveInterval, session.RemoteEndPoint,
                         new MediaStreamSessionContext(session.SessionID, new MediaStreamIdentifier(request.ChannelNo, 0), true,
                             new DateTime(request.Beg.WYear, request.Beg.WMonth, request.Beg.WDay, request.Beg.WHour, request.Beg.WMinute, request.Beg.WSecond, DateTimeKind.Local),
                             new DateTime(request.End.WYear, request.End.WMonth, request.End.WDay, request.End.WHour, request.End.WMinute, request.End.WSecond, DateTimeKind.Local)));
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
                else if (requestInfo.Header.proType == ProtocolType.GetNetHead)
                {
                    Send<GetNetHeadRequest, GetNetHeadResponse>(vcSession, requestInfo, m_AppInstance.GetNetHead);
                }
                else if (requestInfo.Header.proType == ProtocolType.ForceIFrame)
                {
                    Send<ForceIFrameRequest, ForceIFrameResponse>(vcSession, requestInfo, m_AppInstance.ForceIFrame);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_color)
                {
                    Send<GetColorRequest, ColorInfo>(vcSession, requestInfo, m_AppInstance.GetColor);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_color)
                {
                    Send<ColorInfo, SetColorResponse>(vcSession, requestInfo, m_AppInstance.SetColor);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_osdchannel)
                {
                    Send<GetOsdChannelRequest, OsdChannelInfo>(vcSession, requestInfo, m_AppInstance.GetOsdChannel);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_osdchannel)
                {
                    Send<OsdChannelInfo, SetOsdChannelResponse>(vcSession, requestInfo, m_AppInstance.SetOsdChannel);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_osddate)
                {
                    Send<GetOsdDateRequest, OsdDateInfo>(vcSession, requestInfo, m_AppInstance.GetOsdDate);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_osddate)
                {
                    Send<OsdDateInfo, SetOsdDateResponse>(vcSession, requestInfo, m_AppInstance.SetOsdDate);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_videoquality)
                {
                    Send<GetVideoQualityRequest, VideoQualityInfo>(vcSession, requestInfo, m_AppInstance.GetVideoQuality);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_videoquality)
                {
                    Send<VideoQualityInfo, SetVideoQualityResponse>(vcSession, requestInfo, m_AppInstance.SetVideoQuality);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_streamtype)
                {
                    Send<GetStreamTypeRequest, StreamTypeInfo>(vcSession, requestInfo, m_AppInstance.GetStreamType);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_streamtype)
                {
                    Send<StreamTypeInfo, SetStreamTypeResponse>(vcSession, requestInfo, m_AppInstance.SetStreamType);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_netinfo)
                {
                    NetInfo response = m_AppInstance.GetNetInfo(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_netinfo)
                {
                    Send<NetInfo, SetNetInfoResponse>(vcSession, requestInfo, m_AppInstance.SetNetInfo);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_systemtime)
                {
                    SystemTimeInfo response = m_AppInstance.GetSystemTime(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_systemtime)
                {
                    Send<SystemTimeInfo, SetSystemTimeResponse>(vcSession, requestInfo, m_AppInstance.SetSystemTime);
                }
                else if (requestInfo.Header.proType == ProtocolType.Restart_device)
                {
                    RestartDeviceResponse response = m_AppInstance.RestartDevice(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }

                else if (requestInfo.Header.proType == ProtocolType.Close_device)
                {
                    CloseDeviceResponse response = m_AppInstance.CloseDevice(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }

                else if (requestInfo.Header.proType == ProtocolType.Reset)
                {
                    ResetDeviceResponse response = m_AppInstance.ResetDevice(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_rs232cfg)
                {
                    Send<GetRs232CfgRequest, Rs232CfgInfo>(vcSession, requestInfo, m_AppInstance.GetRs232Cfg);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_rs232cfg)
                {
                    Send<Rs232CfgInfo, SetRs232CfgResponse>(vcSession, requestInfo, m_AppInstance.SetRs232Cfg);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_ptzrs232cfg)
                {
                    Send<GetPtzRs232CfgRequest, PtzRs232CfgInfo>(vcSession, requestInfo, m_AppInstance.GetPtzRs232Cfg);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_ptzrs232cfg)
                {
                    Send<PtzRs232CfgInfo, SetPtzRs232CfgResponse>(vcSession, requestInfo, m_AppInstance.SetPtzRs232Cfg);
                }
                else if (requestInfo.Header.proType == ProtocolType.Ptzcontrol)
                {
                    Send<PtzControlRequest, PtzControlResponse>(vcSession, requestInfo, m_AppInstance.PtzControl);
                }
                else if (requestInfo.Header.proType == ProtocolType.SearchFile)
                {
                    Send<SearchFileRequest, SearchFileResponse>(vcSession, requestInfo, m_AppInstance.SearchFile);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetFileInfo)
                {
                    Send<GetFileInfoRequest, GetFileInfoResponse>(vcSession, requestInfo, m_AppInstance.GetFileInfo);
                }
                else if (requestInfo.Header.proType == ProtocolType.RegisterAlarm)
                {
                    Send<RegisterAlarmRequest, RegisterAlarmResponse>(vcSession, requestInfo, m_AppInstance.SetRegisterAlarm);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_MotionExSet)
                {
                    Send<GetMotionExRequest, GetMotionExResponse>(vcSession, requestInfo, m_AppInstance.GetMotionExSet);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_MotionExSet)
                {
                    Send<SetMotionExRequest, SetMotionExResponse>(vcSession, requestInfo, m_AppInstance.SetMotionExSet);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_SubChannelSet)
                {
                    Send<GetSubChannelSetRequest, GetSubChannelSetResponse>(vcSession, requestInfo, m_AppInstance.GetSubChannelSet);
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_SubChannelSet)
                {
                    Send<SetSubChannelSetRequest, SetSubChannelSetResponse>(vcSession, requestInfo, m_AppInstance.SetSubChannelSet);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetTimeEx)
                {
                    GetNetSyncTimeResponse response = m_AppInstance.GetNetSyncTime(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.SetTimeEx)
                {
                    Send<NetSyncTime, SetNetSyncTimeResponse>(vcSession, requestInfo, m_AppInstance.SetNetSyncTime);
                }
                else if (requestInfo.Header.proType == ProtocolType.ForceIFrame)
                {
                    Send<ForceIFrameRequest, ForceIFrameResponse>(vcSession, requestInfo, m_AppInstance.ForceIFrame);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetDeviceCfg)
                {
                    DeviceConfig response = m_AppInstance.GetDeviceConfig(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.GetMotionSet)
                {
                    Send<GetMotionRequest, GetMotionResponse>(vcSession, requestInfo, m_AppInstance.GetMotionSet);
                }
                else if (requestInfo.Header.proType == ProtocolType.SetMotionSet)
                {
                    Send<SetMotionRequest, SetMotionResponse>(vcSession, requestInfo, m_AppInstance.SetMotionSet);
                }
                else if (requestInfo.Header.proType == ProtocolType.SyncTime)
                {
                    Send<SyncTimeRequest, SyncTimeResponse>(vcSession, requestInfo, m_AppInstance.SyncTime);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetUser)
                {
                    DavinciUsers response = m_AppInstance.GetUsers(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.UpdateUser)
                {
                    Send<UpdateUserRequest, UpdateUserResponse>(vcSession, requestInfo, m_AppInstance.UpdateUser);
                }
                else if (requestInfo.Header.proType == ProtocolType.CaptureJpeg)
                {
                    Send<CaptureRequest, CapturenResponse>(vcSession, requestInfo, m_AppInstance.CaptureJpeg);
                }
                else if (requestInfo.Header.proType == ProtocolType.Get_ntpinfo)
                {
                    NtpInfo response = m_AppInstance.GetNtpInfo(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.Set_ntpinfo)
                {
                    Send<NtpInfo, SetNtpInfoResponse>(vcSession, requestInfo, m_AppInstance.SetNtpInfo);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetPanoCameraList)
                {
                    Send<tQueryString, tPanoCameraList>(vcSession, requestInfo, m_AppInstance.GetPanoCameraList);
                }
                else if (requestInfo.Header.proType == ProtocolType.AddPanoCamera)
                {
                    Send<tPanoCamera, tFault>(vcSession, requestInfo, m_AppInstance.AddPanoCamera);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetPanoCamera)
                {
                    Send<tPanoCameraId, tPanoCamera>(vcSession, requestInfo, m_AppInstance.GetPanoCamera);
                }
                else if (requestInfo.Header.proType == ProtocolType.SetPanoCamera)
                {
                    Send<tPanoCamera, tFault>(vcSession, requestInfo, m_AppInstance.SetPanoCamera);
                }
                else if (requestInfo.Header.proType == ProtocolType.DeletePanoCamera)
                {
                    Send<tPanoCameraId, tFault>(vcSession, requestInfo, m_AppInstance.DeletePanoCamera);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetServiceVersion)
                {
                    tServiceVersion response = m_AppInstance.GetServiceVersion(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.GetDeviceInfo)
                {
                    tDeviceInfo response = m_AppInstance.GetDeviceInfo(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.GetDeviceStatus)
                {
                    tDeviceStatus response = m_AppInstance.GetDeviceStatus(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.GetNetworkInterface)
                {
                    tNetworkInterface response = m_AppInstance.GetNetworkInterface(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.GetDecodingUnitList)
                {
                    var response = m_AppInstance.GetDecodingUnitList(vcSession);
                    vcSession.Send(vcSession.ErrorNo, requestInfo.Header.proType, response.GetBytes());
                }
                else if (requestInfo.Header.proType == ProtocolType.GetDecodingUnit)
                {
                    Send<tDecodingUnitId, tDecodingUnit>(vcSession, requestInfo, m_AppInstance.GetDecodingUnit);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetRotatingSpeed)
                {
                    Send<tDecodingUnitId, tRotatingSpeed>(vcSession, requestInfo, m_AppInstance.GetRotatingSpeed);
                }
                else if (requestInfo.Header.proType == ProtocolType.SetRotatingSpeed)
                {
                    Send<tRotatingSpeed, tFault>(vcSession, requestInfo, m_AppInstance.SetRotatingSpeed);
                }
                else if (requestInfo.Header.proType == ProtocolType.SwitchPanoCamera)
                {
                    Send<SwitchPanoCameraRequest, tFault>(vcSession, requestInfo, m_AppInstance.SwitchPanoCamera);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetViewPoint)
                {
                    Send<tDecodingUnitId, tViewPoint>(vcSession, requestInfo, m_AppInstance.GetViewPoint);
                }
                else if (requestInfo.Header.proType == ProtocolType.SetViewPoint)
                {
                    Send<SetViewPointRequest, tFault>(vcSession, requestInfo, m_AppInstance.SetViewPoint);
                }
                else if (requestInfo.Header.proType == ProtocolType.SetViewPointFixed)
                {
                    Send<SetViewPointFixedRequest, tFault>(vcSession, requestInfo, m_AppInstance.SetViewPointFixed);
                }
                else if (requestInfo.Header.proType == ProtocolType.SetViewPointRows)
                {
                    Send<SetViewPointRowsRequest, tFault>(vcSession, requestInfo, m_AppInstance.SetViewPointRows);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetPlayerStatus)
                {
                    Send<tDecodingUnitId, tPlayerStatus>(vcSession, requestInfo, m_AppInstance.GetPlayerStatus);
                }
                else if (requestInfo.Header.proType == ProtocolType.OneByOne)
                {
                    Send<OneByOneRequest, tFault>(vcSession, requestInfo, m_AppInstance.OneByOne);
                }
                else if (requestInfo.Header.proType == ProtocolType.Pause)
                {
                    Send<PauseRequest, tFault>(vcSession, requestInfo, m_AppInstance.Pause);
                }
                else if (requestInfo.Header.proType == ProtocolType.Resume)
                {
                    Send<ResumeRequest, tFault>(vcSession, requestInfo, m_AppInstance.Resume);
                }
                else if (requestInfo.Header.proType == ProtocolType.Seek)
                {
                    Send<SeekRequest, tFault>(vcSession, requestInfo, m_AppInstance.Seek);
                }
                else if (requestInfo.Header.proType == ProtocolType.GetCapability)
                {
                    session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                       new ProtocolHeader() { proType = requestInfo.Header.proType, errornum = ErrorNo.HW_NET_NOSUPPORT },
                       null));
                }
                else
                {
                    //未注册异常应答
                    session.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                        new ProtocolHeader() { proType = requestInfo.Header.proType, errornum = ErrorNo.HW_NET_NOSUPPORT },
                        null));
                    RaisingError(new Exception(String.Format("不支持的协议类型:{0:X00000000}", requestInfo.Header.proType)));
                }
            }
            catch (Exception ex)
            {
                RaisingError(ex);
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
        public event EventHandler<MediaStreamSessionRegisteredEventArgs> MediaStreamSessionRegistered;
        /// <summary>
        /// 流会话关闭事件
        /// </summary>
        public event EventHandler<MediaStreamSessionClosedEventArgs> MediaStreamSessionClosed;
        private void RaisingError(Exception ex)
        {
            if (Error != null && ex != null)
            {
                try
                {
                    Error(this, new ErrorEventArgs(ex));
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2.Message);
                }
            }
        }
        private void RaiseSessionClosed(Howell5198Session session, String closeReason)
        {
            try
            {
                if (SessionClosed != null)
                {
                    SessionClosed(this, new SessionClosedEventArgs(session, closeReason));
                }
            }
            catch { }
        }
        private void RaiseSessionRegistered(Howell5198Session session)
        {
            try
            {
                if (SessionRegistered != null)
                {
                    SessionRegistered(this, new SessionRegisteredEventArgs(session));
                }
            }
            catch { }
        }
        private void RaiseStreamSessionClosed(MediaStreamSession session, String closeReason)
        {
            try
            {
                if (MediaStreamSessionClosed != null)
                {
                    MediaStreamSessionClosed(this, new MediaStreamSessionClosedEventArgs(session, closeReason));
                }
            }
            catch { }
        }
        private void RaiseStreamSessionRegistered(MediaStreamSession session)
        {
            try
            {
                if (MediaStreamSessionRegistered != null)
                {
                    MediaStreamSessionRegistered(this, new MediaStreamSessionRegisteredEventArgs(session));
                }
            }
            catch { }
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
        public SessionRegisteredEventArgs(MediaStreamSession session)
            : base()
        {
            this.MediaStreamSession = session;
        }
        /// <summary>
        /// 协议会话对象
        /// </summary>
        public Howell5198Session Session { get; private set; }
        /// <summary>
        /// 流会话对象
        /// </summary>
        public MediaStreamSession MediaStreamSession { get; private set; }

    }
    /// <summary>
    /// 流会话注册事件参数
    /// </summary>
    public class MediaStreamSessionRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// 发送流事件参数
        /// </summary>
        /// <param name="session">会话信息</param>
        /// <param name="channelno">通道号</param>
        /// <param name="type">0:主码流 1:子码流</param>
        public MediaStreamSessionRegisteredEventArgs(MediaStreamSession session)
            : base()
        {
            this.Session = session;
            //this.ChannelNo = channelno;
            //this.Type = type;
        }
        /// <summary>
        /// 会话对象
        /// </summary>
        public MediaStreamSession Session { get; private set; }
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
    public class MediaStreamSessionClosedEventArgs : EventArgs
    {
        /// <summary>
        /// 对讲会话注册事件参数
        /// </summary>
        /// <param name="session">会话信息</param>
        /// <param name="closeReason">关闭原因</param>
        public MediaStreamSessionClosedEventArgs(MediaStreamSession session, String closeReason)
            : base()
        {
            this.MediaStreamSession = session;
            this.CloseReason = closeReason;
        }
        /// <summary>
        /// 流会话对象
        /// </summary>
        public MediaStreamSession MediaStreamSession { get; private set; }
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
}