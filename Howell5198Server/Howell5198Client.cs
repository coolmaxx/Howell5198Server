using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Extensions.Protocol;
using Howell.IO.Serialization;
using Howell5198.Protocols;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace Howell5198
{
    /// <summary>
    /// 5198客户端程序
    /// </summary>
    public class Howell5198Client : IDisposable
    {
        private FixedHeaderProtocolClient<ProtocolHeader> m_Client = null;
        private String m_IPAddress = null;
        private Int32 m_Port = 0;
        private Boolean m_ssl=false;
        private String m_certificateName = null;
        private X509Certificate2 m_certificate = null;
        private Dictionary<UInt32, WaitingPackageInfo> m_WaitingPackages = new Dictionary<uint, WaitingPackageInfo>();
        //private FixedHeaderProtocolClient<ProtocolHeader>[] m_Client_Stream = new FixedHeaderProtocolClient<ProtocolHeader>[100];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public Howell5198Client(String ipAddress, Int32 port)
        {
            m_IPAddress = ipAddress;
            m_Port = port;
            this.Timeout = 10000;
        }
        public Howell5198Client(String ipAddress, Int32 port, Boolean ssl, String certificateName, X509Certificate2 certificate)
        {
            m_IPAddress = ipAddress;
            m_Port = port;
            this.Timeout = 10000;
            m_ssl = ssl;
            m_certificateName=certificateName;
            m_certificate = certificate;
        }
        /// <summary>
        /// 连接服务器并登陆
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public ServerInfo Connect(String userName, String password)
        {
            lock (this)
            {
                CheckDisposed();
                try
                {
                    if(m_ssl)
                    {
                        m_Client = new FixedHeaderProtocolClient<ProtocolHeader>(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(m_IPAddress), m_Port), 4 * 1024 * 1024, true, m_certificateName, m_certificate);
                    }
                    else
                    {
                        m_Client = new FixedHeaderProtocolClient<ProtocolHeader>(m_IPAddress, m_Port);
                    }
                   
                    m_Client.DataReceived += new EventHandler<ClientDataReceivedEventArgs<ProtocolHeader>>(m_Client_DataReceived);
                    m_Client.Error += new EventHandler<ClientErrorEventArgs>(m_Client_Error);
                    m_Client.Connect();
                    if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
                    if (Connected != null)
                    {
                        Connected(this, new EventArgs());
                    }
                    LoginResponse response = Send<LoginResponse>(COMMAND.Login, new LoginRequest()
                    {
                        Type = 0,
                        UserName = userName,
                        Password = password,
                        ClientUserID = 0
                    }.GetBytes());
                    if (response.Success != 0) throw new InvalidOperationException("Login failed");
                    ServerInfo serverinfo = Send<ServerInfo>(COMMAND.Serverinfo, null);
                    return serverinfo;
                }
                catch (Exception ex)
                {                                                                                                                                                                                            
                    try
                    {
                        if (m_Client != null)
                        {
                            m_Client.Dispose();
                            m_Client = null;
                        }
                    }
                    catch { }
                    throw ex;
                }
            }
        }
        /// <summary>
        /// 连接客户端
        /// </summary>
        public void Close()
        {
            lock (this)
            {
                CheckDisposed();
                //if (m_Timer != null)
                //{
                //    m_Timer.Dispose();
                //    m_Timer = null;
                //}
                if (m_Client != null)
                {
                    if (m_Client.IsConnected == true)
                    {
                        if (Closed != null)
                        {
                            Closed(this, new EventArgs());
                        }
                        m_Client.Close();
                        
                    }
                    m_Client.Dispose();
                    m_Client = null;
                }

            }
        }
        #region Properties
        /// <summary>
        /// 是否已连接到服务器
        /// </summary>
        /// <returns></returns>
        public Boolean IsConnected
        {
            get
            {
                lock (this)
                {
                    if (m_Client == null) return false;
                    return m_Client.IsConnected;
                }
            }
        }
        /// <summary>
        /// 超时的毫秒值
        /// </summary>
        public Int32 Timeout { get; set; }

        /// <summary>
        /// 目标IP地址
        /// </summary>
        public String IPAddress
        {
            get
            {
                return m_IPAddress;
            }
        }
        /// <summary>
        /// 目标端口
        /// </summary>
        public int Port
        {
            get
            {
                return m_Port;
            }
        }
        /// <summary>
        /// 用户自定义标示
        /// </summary>
        public String Identity { get; set; } 
        #endregion
        #region Events
        /// <summary>
        /// 客户端建立连接
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// 客户端关闭连接
        /// </summary>
        public event EventHandler Closed;
        #endregion
        #region Methods
 
        ///// <summary>
        ///// 获取ServerInfo
        ///// </summary>
        ///// <param name="userName">用户名</param>
        ///// <param name="password">密码</param>
        //public LoginResponse Login(String userName, String password)
        //{
        //    CheckDisposed();
        //    if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
        //    LoginResponse response = Send<LoginResponse>(COMMAND.Login, new LoginRequest()
        //    {
        //        Type = 0,
        //        UserName = userName,
        //        Password = password,
        //        ClientUserID = 0
        //    }.GetBytes());
        //    if (response.Success != 0) throw new InvalidOperationException("Login failed");
        //    return response;
        //}

        /// <summary>
        /// 获取色彩
        /// </summary>
        /// <param name="channelno">通道号</param>
        public ColorInfo GetColor(Int32 channelno)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            ColorInfo response = Send<ColorInfo>(COMMAND.Get_color, new GetColorRequest()
            {
                ChannelNo = channelno
            }.GetBytes());
            return response;
        }

        /// <summary>
        /// 设置色彩
        /// </summary>
        /// <param name="setColorRequest"></param>
        public SetColorResponse SetColor(ColorInfo setColorRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetColorResponse response = Send<SetColorResponse>(COMMAND.Set_color, setColorRequest.GetBytes());
            return response;
        }

        /// <summary>
        /// 获取通道名称
        /// </summary>
        /// <param name="channelno">通道号</param>
        public OsdChannelInfo GetOsdChannel(Int32 channelno)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            OsdChannelInfo response = Send<OsdChannelInfo>(COMMAND.Get_osdchannel, new GetOsdChannelRequest()
            {
                ChannelNo = channelno
            }.GetBytes()); 
            return response;
        }
        /// <summary>
        /// 设置通道名称
        /// </summary>
        /// <param name="setOsdChannelRequest"></param>
        public SetOsdChannelResponse SetOsdChannel(OsdChannelInfo setOsdChannelRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetOsdChannelResponse response = Send<SetOsdChannelResponse>(COMMAND.Set_osdchannel, setOsdChannelRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取通道日期
        /// </summary>
        /// <param name="channelno">通道号</param>
        public OsdDateInfo GetOsdDate(Int32 channelno)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            OsdDateInfo response = Send<OsdDateInfo>(COMMAND.Get_osddate, new GetOsdDateRequest()
            {
                ChannelNo = channelno
            }.GetBytes());
            return response;
        }
        /// <summary>
        /// 设置通道日期
        /// </summary>
        /// <param name="setOsdDateRequest">OsdDateInfo</param>
        public SetOsdDateResponse SetOsdDate(OsdDateInfo setOsdDateRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetOsdDateResponse response = Send<SetOsdDateResponse>(COMMAND.Set_osddate, setOsdDateRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取图像质量
        /// </summary>
        /// <param name="channelno">通道号</param>
        public VideoQualityInfo GetVideoQuality(Int32 channelno)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            VideoQualityInfo response = Send<VideoQualityInfo>(COMMAND.Get_videoquality, new GetVideoQualityRequest()
            {
                ChannelNo = channelno
            }.GetBytes());
            return response;
        }
        /// <summary>
        /// 设置图像质量
        /// </summary>
        /// <param name="setOsdChannelRequest"></param>
        public SetVideoQualityResponse SetVideoQuality(VideoQualityInfo setVideoQualityRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetVideoQualityResponse response = Send<SetVideoQualityResponse>(COMMAND.Set_videoquality, setVideoQualityRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取码流类型
        /// </summary>
        /// <param name="channelno">通道号</param>
        public StreamTypeInfo GetStreamType(Int32 channelno)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            StreamTypeInfo response = Send<StreamTypeInfo>(COMMAND.Get_streamtype, new GetStreamTypeRequest()
            {
                ChannelNo = channelno
            }.GetBytes());
            return response;
        }
        /// <summary>
        /// 设置码流类型
        /// </summary>
        /// <param name="setStreamTypeRequest"></param>
        public SetStreamTypeResponse SetStreamType(StreamTypeInfo setStreamTypeRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetStreamTypeResponse response = Send<SetStreamTypeResponse>(COMMAND.Set_streamtype, setStreamTypeRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取网络
        /// </summary>
        /// <returns>GetNetInfoResponse</returns>
        public NetInfo GetNetInfo()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            NetInfo response = Send<NetInfo>(COMMAND.Get_netinfo, null);
            return response;
        }
        /// <summary>
        /// 设置网络
        /// </summary>
        /// <param name="setNetInfoRequest"></param>
        /// <returns>SetNetInfoResponse</returns>
        public SetNetInfoResponse SetNetInfo(NetInfo setNetInfoRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetNetInfoResponse response = Send<SetNetInfoResponse>(COMMAND.Set_netinfo, setNetInfoRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取时间
        /// </summary>
        /// <returns>GetSystemTimeResponse</returns>
        public SystemTimeInfo GetSystemTime()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SystemTimeInfo response = Send<SystemTimeInfo>(COMMAND.Get_systemtime, null);
            return response;
        }
        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="setSystemTimeRequest"></param>
        /// <returns>SetSystemTimeResponse</returns>
        public SetSystemTimeResponse SetSystemTime(SystemTimeInfo setSystemTimeRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetSystemTimeResponse response = Send<SetSystemTimeResponse>(COMMAND.Set_systemtime, setSystemTimeRequest.GetBytes());
            return response;

        }
        /// <summary>
        /// 重启设备
        /// </summary>
        /// <returns>RestartDeviceResponse</returns>
        public RestartDeviceResponse RestartDevice()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            RestartDeviceResponse response = Send<RestartDeviceResponse>(COMMAND.Restart_device, null);
            return response;
        }
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns>CloseDeviceResponse</returns>
        public CloseDeviceResponse CloseDevice()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            CloseDeviceResponse response = Send<CloseDeviceResponse>(COMMAND.Close_device, null);
            return response;
        }
        /// <summary>
        /// 重置设备
        /// </summary>
        /// <returns>ResetDeviceResponse</returns>
        public ResetDeviceResponse ResetDevice()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            ResetDeviceResponse response = Send<ResetDeviceResponse>(COMMAND.Reset, null);
            return response;
        }
        /// <summary>
        /// 获取串口模式
        /// </summary>
        /// <returns>Rs232CfgInfo</returns>
        public Rs232CfgInfo GetRs232Cfg(GetRs232CfgRequest getRs232CfgRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            Rs232CfgInfo response = Send<Rs232CfgInfo>(COMMAND.Get_rs232cfg, getRs232CfgRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 设置串口模式
        /// </summary>
        /// <param name="setRs232CfgRequest"></param>
        /// <returns>SetRs232CfgResponse</returns>
        public SetRs232CfgResponse SetRs232Cfg(Rs232CfgInfo setRs232CfgRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetRs232CfgResponse response = Send<SetRs232CfgResponse>(COMMAND.Set_rs232cfg, setRs232CfgRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取PTZ设置
        /// </summary>
        /// <returns>GetPtzRs232CfgResponse</returns>
        public PtzRs232CfgInfo GetPtzRs232Cfg(Int32 channelno)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            PtzRs232CfgInfo response = Send<PtzRs232CfgInfo>(COMMAND.Get_ptzrs232cfg, new GetPtzRs232CfgRequest()
            {
                ChannelNo = channelno
            }.GetBytes()); 
            return response;
        }
        /// <summary>
        /// 设置PTZ设置
        /// </summary>
        /// <param name="setPtzRs232CfgRequest"></param>
        /// <returns>SetPtzRs232CfgResponse</returns>
        public SetPtzRs232CfgResponse SetPtzRs232Cfg(PtzRs232CfgInfo setPtzRs232CfgRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetPtzRs232CfgResponse response = Send<SetPtzRs232CfgResponse>(COMMAND.Set_ptzrs232cfg, setPtzRs232CfgRequest.GetBytes());
            return response;
        } 
        /// <summary>
        /// PTZ控制
        /// </summary>
        /// <param name="ptzControlRequest"></param>
        /// <returns>PtzControlResponse</returns>
        public PtzControlResponse PtzControl(PtzControlRequest ptzControlRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            PtzControlResponse response = Send<PtzControlResponse>(COMMAND.Ptzcontrol, ptzControlRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 创建流对象
        /// </summary>
       /// <param name="channelno">通道号</param>
       /// <param name="type">0:主码流 1:子码流</param>
        /// <returns></returns>
        public Howell5198Stream CreateStream(Int32 channelno, Int32 type)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
                
           Howell5198Stream howell5198stream = new Howell5198Stream(this,m_IPAddress, m_Port, channelno, type);
           return howell5198stream;     
        }

        /// <summary>
        /// 搜索获得回放文件列表
        /// </summary>
        /// <param name="searchFileRequest"></param>
        /// <returns></returns>
        public SearchFileResponse SearchFile(SearchFileRequest searchFileRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SearchFileResponse response = Send<SearchFileResponse>(COMMAND.SearchFile, searchFileRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取回放文件流
        /// </summary>
        /// <param name="getFileRequest"></param>
        /// <returns></returns>
        public Howell5198FileStream GetFile(GetFileRequest getFileRequest)
        {
            return new Howell5198FileStream(this, m_IPAddress, m_Port, getFileRequest);
        }
        /// <summary>
        /// 获取回放文件信息
        /// </summary>
        /// <param name="getFileInfoRequest"></param>
        /// <returns></returns>
        public GetFileInfoResponse GetFileInfo( GetFileInfoRequest getFileInfoRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            GetFileInfoResponse response = Send<GetFileInfoResponse>(COMMAND.GetFileInfo, getFileInfoRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取预览视频信息
        /// </summary>
        /// <param name="getNetHeadRequest"></param>
        /// <returns></returns>
        public GetNetHeadResponse GetNetHead(GetNetHeadRequest getNetHeadRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            GetNetHeadResponse response = Send<GetNetHeadResponse>(COMMAND.GetNetHead, getNetHeadRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <returns></returns>
        public DeviceConfig GetDevCfg()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            DeviceConfig response = Send<DeviceConfig>(COMMAND.GetDeviceCfg, null);
            return response;
        }
        /// <summary>
        /// 获取移动侦测
        /// </summary>
        /// <param name="channelno"></param>
        /// <returns></returns>
        public GetMotionResponse GetMotionSet(Int32 channelno)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            GetMotionResponse response = Send<GetMotionResponse>(COMMAND.GetMotionSet, new GetMotionRequest()
            {
                ChannelNo = channelno
            }.GetBytes());
            return response;
        }
        /// <summary>
        /// 设置移动侦测
        /// </summary>
        /// <param name="setMotionRequest"></param>
        /// <returns></returns>
        public SetMotionResponse SetMotionSet(SetMotionRequest setMotionRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetMotionResponse response = Send<SetMotionResponse>(COMMAND.SetMotionSet, setMotionRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 同步时间
        /// </summary>
        /// <param name="syncTimeRequest"></param>
        /// <returns></returns>
        public SyncTimeResponse SyncTime(SyncTimeRequest syncTimeRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SyncTimeResponse response = Send<SyncTimeResponse>(COMMAND.SyncTime, syncTimeRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public DavinciUsers GetUser()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            DavinciUsers response = Send<DavinciUsers>(COMMAND.GetUser, null);
            return response;
        }
        /// <summary>
        /// 更改用户列表
        /// </summary>
        /// <param name="updateUserRequest"></param>
        /// <returns></returns>
        public UpdateUserResponse UpdateUser(UpdateUserRequest updateUserRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            UpdateUserResponse response = Send<UpdateUserResponse>(COMMAND.UpdateUser, updateUserRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 抓图
        /// </summary>
        /// <param name="captureRequest"></param>
        /// <returns></returns>
        public CapturenResponse CaptureJpeg(CaptureRequest captureRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            CapturenResponse response = Send<CapturenResponse>(COMMAND.CaptureJpeg, captureRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取NTP服务器
        /// </summary>
        /// <returns></returns>
        public NtpInfo GetNtpInfo()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            NtpInfo response = Send<NtpInfo>(COMMAND.Get_ntpinfo, null);
            return response;
        }
        /// <summary>
        /// 设置NTP服务器
        /// </summary>
        /// <param name="ntpInfo"></param>
        /// <returns></returns>
        public SetNtpInfoResponse SetNtpInfo(NtpInfo ntpInfo)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            SetNtpInfoResponse response = Send<SetNtpInfoResponse>(COMMAND.Set_ntpinfo, ntpInfo.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取全景摄像机列表
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public tPanoCameraList GetPanoCameraList(tQueryString queryString)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tPanoCameraList response = Send<tPanoCameraList>(COMMAND.GetPanoCameraList, queryString.GetBytes());
            return response;
        }
        /// <summary>
        /// 创建全景摄像机
        /// </summary>
        /// <param name="panoCamera"></param>
        /// <returns></returns>
        public tFault AddPanoCamera(tPanoCamera panoCamera)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.AddPanoCamera, panoCamera.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取指定全景摄像机信息
        /// </summary>
        /// <param name="panoCameraId"></param>
        /// <returns></returns>
        public tPanoCamera GetPanoCamera(String panoCameraId)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tPanoCameraId panoId = new tPanoCameraId(panoCameraId);
            tPanoCamera response = Send<tPanoCamera>(COMMAND.GetPanoCamera, panoId.GetBytes());
            return response;
        }
        /// <summary>
        /// 修改或创建指定全景摄像机信息
        /// </summary>
        /// <param name="panoCamera"></param>
        /// <returns></returns>
        public tFault SetPanoCamera(tPanoCamera panoCamera)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.SetPanoCamera, panoCamera.GetBytes());
            return response;
        }
        /// <summary>
        /// 删除指定全景摄像机信息
        /// </summary>
        /// <param name="panoCameraId"></param>
        /// <returns></returns>
        public tFault DeletePanoCamera(String panoCameraId)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tPanoCameraId panoId = new tPanoCameraId(panoCameraId);
            tFault response = Send<tFault>(COMMAND.DeletePanoCamera, panoId.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取服务程序版本信息
        /// </summary>
        /// <returns></returns>
        public tServiceVersion GetServiceVersion()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tServiceVersion response = Send<tServiceVersion>(COMMAND.GetServiceVersion, null);
            return response;
        }
        /// <summary>
        /// 查询设备信息
        /// </summary>
        /// <returns></returns>
        public tDeviceInfo GetDeviceInfo()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tDeviceInfo response = Send<tDeviceInfo>(COMMAND.GetDeviceInfo, null);
            return response;
        }
        /// <summary>
        /// 查询设备状态
        /// </summary>
        /// <returns></returns>
        public tDeviceStatus GetDeviceStatus()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tDeviceStatus response = Send<tDeviceStatus>(COMMAND.GetDeviceStatus, null);
            return response;
        }
        /// <summary>
        /// 查询网口信息
        /// </summary>
        /// <returns></returns>
        public tNetworkInterface GetNetworkInterface()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tNetworkInterface response = Send<tNetworkInterface>(COMMAND.GetNetworkInterface, null);
            return response;
        }
        /// <summary>
        /// 查询解码单元列表
        /// </summary>
        /// <returns></returns>
        public tDecodingUnitList GetDecodingUnitList()
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tDecodingUnitList response = Send<tDecodingUnitList>(COMMAND.GetDecodingUnitList, null);
            return response;
        }
        /// <summary>
        /// 查询解码单元
        /// </summary>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        public tDecodingUnit GetDecodingUnit(String decodingUnitId)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tDecodingUnitId decodingId = new tDecodingUnitId() { DecodingUnitId = decodingUnitId };
            tDecodingUnit response = Send<tDecodingUnit>(COMMAND.GetDecodingUnit, decodingId.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取解码单元播放器视频水平旋转速度
        /// </summary>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        public tRotatingSpeed GetRotatingSpeed(String decodingUnitId)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tDecodingUnitId decodingId = new tDecodingUnitId() { DecodingUnitId = decodingUnitId };
            tRotatingSpeed response = Send<tRotatingSpeed>(COMMAND.GetRotatingSpeed, decodingId.GetBytes());
            return response;
        }
        /// <summary>
        /// 设置解码单元播放器视频水平旋转速度
        /// </summary>
        /// <param name="rotatingSpeed"></param>
        /// <returns></returns>
        public tFault SetRotatingSpeed(tRotatingSpeed rotatingSpeed)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.SetRotatingSpeed, rotatingSpeed.GetBytes());
            return response;
        }
        /// <summary>
        /// 切换解码单元显示的全景摄像机
        /// </summary>
        /// <param name="switchPanoCameraRequest"></param>
        /// <returns></returns>
        public tFault SwitchPanoCamera(SwitchPanoCameraRequest switchPanoCameraRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.SwitchPanoCamera, switchPanoCameraRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取解码单元显示的全景摄像机视角
        /// </summary>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        public tViewPoint GetViewPoint(String decodingUnitId)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tDecodingUnitId decodingId = new tDecodingUnitId() { DecodingUnitId = decodingUnitId };
            tViewPoint response = Send<tViewPoint>(COMMAND.GetViewPoint, decodingId.GetBytes());
            return response;
        }
        /// <summary>
        /// 修改解码单元显示的全景摄像机视角
        /// </summary>
        /// <param name="setViewPointRequest"></param>
        /// <returns></returns>
        public tFault SetViewPoint(SetViewPointRequest setViewPointRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.SetViewPoint, setViewPointRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 锁定或解锁解码单元显示的全景摄像机视角
        /// </summary>
        /// <param name="setViewPointRequest"></param>
        /// <returns></returns>
        public tFault SetViewPointFixed(SetViewPointFixedRequest setViewPointFixedRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.SetViewPointFixed, setViewPointFixedRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 修改解码单元显示的全景摄像机的显示列数量
        /// </summary>
        /// <param name="setViewPointRowsRequest"></param>
        /// <returns></returns>
        public tFault SetViewPointRows(SetViewPointRowsRequest setViewPointRowsRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.SetViewPointRows, setViewPointRowsRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 获取解码单元的播放器状态
        /// </summary>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        public tPlayerStatus GetPlayerStatus(String decodingUnitId)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tDecodingUnitId decodingId = new tDecodingUnitId() { DecodingUnitId = decodingUnitId };
            tPlayerStatus response = Send<tPlayerStatus>(COMMAND.GetPlayerStatus, decodingId.GetBytes());
            return response;
        }
        /// <summary>
        /// 单帧进
        /// </summary>
        /// <param name="oneByOneRequest"></param>
        /// <returns></returns>
        public tFault OneByOne(OneByOneRequest oneByOneRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.OneByOne, oneByOneRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 暂停播放
        /// </summary>
        /// <param name="pauseRequest"></param>
        /// <returns></returns>
        public tFault Pause(PauseRequest pauseRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.Pause, pauseRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 恢复播放
        /// </summary>
        /// <param name="resumeRequest"></param>
        /// <returns></returns>
        public tFault Resume(ResumeRequest resumeRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.Resume, resumeRequest.GetBytes());
            return response;
        }
        /// <summary>
        /// 定位播放器进度
        /// </summary>
        /// <param name="seekRequest"></param>
        /// <returns></returns>
        public tFault Seek(SeekRequest seekRequest)
        {
            CheckDisposed();
            if (IsConnected == false) throw new InvalidOperationException("5198Client has not connected yet.");
            tFault response = Send<tFault>(COMMAND.Seek, seekRequest.GetBytes());
            return response;
        }

        #endregion
        void m_Client_DataReceived(object sender, ClientDataReceivedEventArgs<ProtocolHeader> e)
        {
            //应答信息
            if (e.PackageInfo.Header.proType >= COMMAND.Login)
            {
                try
                {
                    if (e.PackageInfo.Header.errornum != 0)
                    {
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, null);
                        throw new Exception(String.Format("Response Error,EroorNum:{0}", e.PackageInfo.Header.errornum));
                    }
                    if (e.PackageInfo.Header.proType == COMMAND.Login)
                    {
                        LoginResponse result = new LoginResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Serverinfo)
                    {
                        ServerInfo result = new ServerInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_color)
                    {
                        ColorInfo result = new ColorInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_color)
                    {
                        SetColorResponse result = new SetColorResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_osdchannel)
                    {
                        OsdChannelInfo result = new OsdChannelInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_osdchannel)
                    {
                        SetOsdChannelResponse result = new SetOsdChannelResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_osddate)
                    {
                        OsdDateInfo result = new OsdDateInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_osddate)
                    {
                        SetOsdDateResponse result = new SetOsdDateResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_videoquality)
                    {
                        VideoQualityInfo result = new VideoQualityInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_videoquality)
                    {
                        SetVideoQualityResponse result = new SetVideoQualityResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_streamtype)
                    {
                        StreamTypeInfo result = new StreamTypeInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_streamtype)
                    {
                        SetStreamTypeResponse result = new SetStreamTypeResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_netinfo)
                    {
                        NetInfo result = new NetInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_netinfo)
                    {
                        SetNetInfoResponse result = new SetNetInfoResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_systemtime)
                    {
                        SystemTimeInfo result = new SystemTimeInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_systemtime)
                    {
                        SetSystemTimeResponse result = new SetSystemTimeResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Restart_device)
                    {
                        RestartDeviceResponse result = new RestartDeviceResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Close_device)
                    {
                        CloseDeviceResponse result = new CloseDeviceResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Reset)
                    {
                        ResetDeviceResponse result = new ResetDeviceResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_rs232cfg)
                    {
                        Rs232CfgInfo result = new Rs232CfgInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_rs232cfg)
                    {
                        SetRs232CfgResponse result = new SetRs232CfgResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_ptzrs232cfg)
                    {
                        PtzRs232CfgInfo result = new PtzRs232CfgInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_ptzrs232cfg)
                    {
                        SetPtzRs232CfgResponse result = new SetPtzRs232CfgResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Ptzcontrol)
                    {
                        PtzControlResponse result = new PtzControlResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetNetHead)
                    {
                        GetNetHeadResponse result = new GetNetHeadResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SearchFile)
                    {
                        SearchFileResponse result = new SearchFileResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetFileInfo)
                    {
                        GetFileInfoResponse result = new GetFileInfoResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetFile)
                    {
                        GetFileResponse result = new GetFileResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetDeviceCfg)
                    {
                        DeviceConfig result = new DeviceConfig();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetMotionSet)
                    {
                        GetMotionResponse result = new GetMotionResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SetMotionSet)
                    {
                        SetMotionResponse result = new SetMotionResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SyncTime)
                    {
                        SyncTimeResponse result = new SyncTimeResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetUser)
                    {
                        DavinciUsers result = new DavinciUsers();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.UpdateUser)
                    {
                        UpdateUserResponse result = new UpdateUserResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.CaptureJpeg)
                    {
                        CapturenResponse result = new CapturenResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Get_ntpinfo)
                    {
                        NtpInfo result = new NtpInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Set_ntpinfo)
                    {
                        SetNtpInfoResponse result = new SetNtpInfoResponse();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetPanoCameraList)
                    {
                        tPanoCameraList result = new tPanoCameraList();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.AddPanoCamera)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetPanoCamera)
                    {
                        tPanoCamera result = new tPanoCamera();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SetPanoCamera)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.DeletePanoCamera)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetServiceVersion)
                    {
                        tServiceVersion result = new tServiceVersion();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetDeviceInfo)
                    {
                        tDeviceInfo result = new tDeviceInfo();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetDeviceStatus)
                    {
                        tDeviceStatus result = new tDeviceStatus();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetNetworkInterface)
                    {
                        tNetworkInterface result = new tNetworkInterface();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }

                    else if (e.PackageInfo.Header.proType == COMMAND.GetDecodingUnitList)
                    {
                        tDecodingUnitList result = new tDecodingUnitList();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetDecodingUnit)
                    {
                        tDecodingUnit result = new tDecodingUnit();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetRotatingSpeed)
                    {
                        tRotatingSpeed result = new tRotatingSpeed();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SetRotatingSpeed)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SwitchPanoCamera)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetViewPoint)
                    {
                        tViewPoint result = new tViewPoint();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SetViewPoint)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SetViewPointFixed)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.SetViewPointRows)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.GetPlayerStatus)
                    {
                        tPlayerStatus result = new tPlayerStatus();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.OneByOne)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Pause)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Resume)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }
                    else if (e.PackageInfo.Header.proType == COMMAND.Seek)
                    {
                        tFault result = new tFault();
                        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                        return;
                    }

                    //else if ((e.PackageInfo.Header.proType == COMMAND.Main_stream) || (e.PackageInfo.Header.proType == COMMAND.Sub_stream))
                    //{
                    //    if (e.PackageInfo.Header.errornum != 0)
                    //    {
                    //        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, null);
                    //        return;
                    //    }
                    //    if(e.PackageInfo.Header.dataLen==4)
                    //    {
                     //        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                    //        UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                    //    }
                    //    else if (e.PackageInfo.Header.dataLen>4)
                    //    {
                    //        FramePayload result = new FramePayload();
                    //        result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                    //        //UploadWaitingPackageInfo(e.PackageInfo.Header.proType+1, result);       
                         
                    //            //视频数据递交
                    //        if (StreamDataReceived != null)
                    //        {
                    //            Byte[] reserved = new Byte[4];
                    //            reserved[0] = e.PackageInfo.Header.Reserved[3];
                    //            reserved[1] = e.PackageInfo.Header.Reserved[2];
                    //            reserved[2] = e.PackageInfo.Header.Reserved[1];
                    //            reserved[3] = e.PackageInfo.Header.Reserved[0];
                    //            StreamDataReceived(sender, new StreamDataReceivedEventArgs(System.BitConverter.ToInt32(reserved, 0), result));
                    //        }
                            
                    //    }        
                    //    return;
                    //}
                   
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
               
            }
        }
        void m_Client_Error(object sender, ClientErrorEventArgs e)
        {

        }
        #region Send
        private TResult Send<TResult>(UInt32 command, byte[] payload,int handle=0)
            where TResult : class, new()
        {
            UInt32 cSeq = command;

            Send(command, payload, handle);

            using (WaitingPackageInfo package = new WaitingPackageInfo(cSeq))
            {
                AddPackage(package);
                try
                {
                    if (package.WaitHandle.WaitOne(this.Timeout) == false)
                    {
                        throw new TimeoutException();
                    }
                    if (package.Payload == null) return null;
                    return (package.Payload as TResult);
                }
                finally
                {
                    RemovePackage(cSeq);
                }
            }
        }
        private void Send(UInt32 command, byte[] payload, int handle)
        {
           if(command == COMMAND.Serverinfo)
           {
              // m_Client_Stream[handle].Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, dataLen = (uint)payload.Length, Reserved = System.BitConverter.GetBytes(handle) }, payload));
           }
           else
           {
               m_Client.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, dataLen =(payload == null) ?0:(uint)payload.Length }, payload));
           }
        }

        private TResult Get<TResult>(UInt32 command)
            where TResult : class, new()
        {
            UInt32 cSeq = command;
            using (WaitingPackageInfo package = new WaitingPackageInfo(cSeq))
            {
                AddPackage(package);
                try
                {
                    if (package.WaitHandle.WaitOne(this.Timeout) == false)
                    {
                        throw new TimeoutException();
                    }
                    return (package.Payload as TResult);
                }
                finally
                {
                    RemovePackage(cSeq);
                }
            }
        }
        //private void Send<TPayload>(JsonPackageInfo<TPayload> packageInfo)
        //    where TPayload : class, new()
        //{
        //    m_Client.Send(packageInfo);
        //}
        #endregion
        #region Packages
        private void AddPackage(WaitingPackageInfo value)
        {
            lock (m_WaitingPackages)
            {
                m_WaitingPackages.Add(value.Sequence, value);
            }
        }
        private void RemovePackage(UInt32 sequence)
        {
            lock (m_WaitingPackages)
            {
                m_WaitingPackages.Remove(sequence);
            }
        }
        private WaitingPackageInfo GetPackage(UInt32 sequence)
        {
            lock (m_WaitingPackages)
            {
                if (m_WaitingPackages.ContainsKey(sequence) == true)
                    return m_WaitingPackages[sequence];
                return null;
            }
        }
        private void UploadWaitingPackageInfo(UInt32 sequence, Object payload)
        {
            lock (m_WaitingPackages)
            {
                if (m_WaitingPackages.ContainsKey(sequence) == true)
                {
                    m_WaitingPackages[sequence].Payload = payload;
                    m_WaitingPackages[sequence].WaitHandle.Set();
                }
            }
        }
        #endregion
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
                Console.WriteLine("VoiceCommClient Disposed.");
                Close();
                if (m_Client != null)
                {
                    m_Client = null;
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
    /// 接收单个用户发送的音频数据的事件参数
    /// </summary>
    public class StreamDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="framepayload"></param>
        public StreamDataReceivedEventArgs(FramePayload framepayload)
            : base()
        {
            this.FrameType = framepayload.FrameType;
            this.Data = framepayload.FrameData;
        }

        /// <summary>
        /// <summary>
        /// 流类型
        /// </summary>
        public FramePayload.frametype FrameType { get; private set; }
        /// <summary>
        /// 流数据
        /// </summary>
        public Byte[] Data { get; private set; }
    }

    /// <summary>
    /// 接收文件数据的事件参数
    /// </summary>
    public class FileDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="sender"></param>
        /// <param name="groupId"></param>
        /// <param name="data"></param>
        public FileDataReceivedEventArgs(GetFileResponse fileData)
            : base()
        {
            this.FileData = fileData;
        }

        /// <summary>
        /// 文件数据
        /// </summary>
        public GetFileResponse FileData { get; private set; }
    }
    /// <summary>
    /// 等待数据包
    /// </summary>
    internal class WaitingPackageInfo : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequence"></param>
        public WaitingPackageInfo(UInt32 sequence)
        {
            this.WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.Sequence = sequence;
        }
        /// <summary>
        /// 等待句柄
        /// </summary>
        public EventWaitHandle WaitHandle { get; private set; }
        /// <summary>
        /// 序号
        /// </summary>
        public UInt32 Sequence { get; private set; }
        /// <summary>
        /// 载荷内容
        /// </summary>
        public Object Payload { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this.WaitHandle != null)
            {
                this.WaitHandle.Dispose();
                this.WaitHandle = null;
            }
        }
    }

     public class Howell5198Stream:IDisposable
     {
         public Howell5198Stream(Howell5198Client client,String ipAddress, Int32 port, Int32 channelNo, Int32 type)
         {
             m_Client = client;
             m_IPAddress = ipAddress;
             m_Port = port;
             ChannelNo = channelNo;
             Type = type;
             this.Timeout = 10000;
         }
         private Howell5198Client m_Client;
         private FixedHeaderProtocolClient<ProtocolHeader> m_Client_Stream = null;
         private String m_IPAddress = null;
         private Int32 m_Port = 0;
         private Dictionary<UInt32, WaitingPackageInfo> m_WaitingPackages = new Dictionary<uint, WaitingPackageInfo>();
      
         /// <summary>
         /// 通道号
         /// </summary>
         public Int32 ChannelNo { get; set; }
         /// <summary>
         /// 流类型，0:主码流,1:子码流
         /// </summary>
         public Int32 Type { get; set; }

         /// <summary>
         /// 该Stream所属的Client对象
         /// </summary>
         public Howell5198Client Client
         {
             get
             {
                 return m_Client;
             }
         }
         /// <summary>
         /// 流会话是否在连接中
         /// </summary>
         public bool Connected() { return m_Client_Stream.IsConnected; }
         /// <summary>
         /// 超时的毫秒值
         /// </summary>
         public Int32 Timeout { get; set; }
         /// <summary>
         /// 开始获取流数据
         /// <returns>请求流是否成功</returns>
         /// </summary>
         public Boolean StartReceive()
         {
             m_Client.Closed += new EventHandler(m_Client_Closed);
             m_Client_Stream = new FixedHeaderProtocolClient<ProtocolHeader>(m_IPAddress, m_Port);
             m_Client_Stream.DataReceived += new EventHandler<ClientDataReceivedEventArgs<ProtocolHeader>>(m_Client_DataReceived);
             m_Client_Stream.Error += new EventHandler<ClientErrorEventArgs>(m_Client_Error);
             m_Client_Stream.Connect();
             StreamRequest request = new StreamRequest() { ChannelNo = ChannelNo };
             StreamResponse response=Send<StreamResponse>(Type == 0 ? COMMAND.Main_stream : COMMAND.Sub_stream, request.GetBytes());
             if (response.Success == 0)
                 return true;
             else
                 return false;
         }
         /// <summary>
         /// 停止获取流数据
         /// </summary>
         public void StopReceive()
         {
             if (m_Client_Stream != null)
             {
                 if (m_Client_Stream.IsConnected == true)
                     m_Client_Stream.Close();
                 m_Client_Stream.Dispose();
                 m_Client_Stream = null;
             }
         }

         /// <summary>
         /// 接收到单个用户发送的流数据
         /// </summary>
         public event EventHandler<StreamDataReceivedEventArgs> StreamDataReceived;

         void m_Client_Closed(object sender, EventArgs e)
         {
             Dispose();
         }
         void m_Client_DataReceived(object sender, ClientDataReceivedEventArgs<ProtocolHeader> e)
         {
             //应答信息
                 try
                 {
                    if ((e.PackageInfo.Header.proType == COMMAND.Main_stream) || (e.PackageInfo.Header.proType == COMMAND.Sub_stream))
                     {
                         if (e.PackageInfo.Header.errornum != 0)
                         {
                            
                             return;
                         }
                         if (e.PackageInfo.Header.dataLen == 4)
                         {
                             StreamResponse result = new StreamResponse();
                             result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                             UploadWaitingPackageInfo(e.PackageInfo.Header.proType, result);
                         }
                         else if (e.PackageInfo.Header.dataLen > 4)
                         {
                             FramePayload result = new FramePayload();
                             result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);
                             //UploadWaitingPackageInfo(e.PackageInfo.Header.proType+1, result);       

                             //视频数据递交
                             if (StreamDataReceived != null)
                             {
                                 StreamDataReceived(this, new StreamDataReceivedEventArgs(result));
                             }

                         }
                         return;
                     }

                 }
                 catch { }

         }
         void m_Client_Error(object sender, ClientErrorEventArgs e)
         {
            
         }

         private TResult Send<TResult>(UInt32 command, byte[] payload)
            where TResult : class, new()
         {
             UInt32 cSeq = command;

             Send(command, payload);

             using (WaitingPackageInfo package = new WaitingPackageInfo(cSeq))
             {
                 AddPackage(package);
                 try
                 {
                     if (package.WaitHandle.WaitOne(this.Timeout) == false)
                     {
                         throw new TimeoutException();
                     }
                     if (package.Payload == null) return null;
                     return (package.Payload as TResult);
                 }
                 finally
                 {
                     RemovePackage(cSeq);
                 }
             }
         }
         private void Send(UInt32 command, byte[] payload)
         {      
             m_Client_Stream.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, dataLen = (uint)payload.Length }, payload));

         }

         private TResult Get<TResult>(UInt32 command)
             where TResult : class, new()
         {
             UInt32 cSeq = command;
             using (WaitingPackageInfo package = new WaitingPackageInfo(cSeq))
             {
                 AddPackage(package);
                 try
                 {
                     if (package.WaitHandle.WaitOne(this.Timeout) == false)
                     {
                         throw new TimeoutException();
                     }
                     return (package.Payload as TResult);
                 }
                 finally
                 {
                     RemovePackage(cSeq);
                 }
             }
         }
         #region Packages
         private void AddPackage(WaitingPackageInfo value)
         {
             lock (m_WaitingPackages)
             {
                 m_WaitingPackages.Add(value.Sequence, value);
             }
         }
         private void RemovePackage(UInt32 sequence)
         {
             lock (m_WaitingPackages)
             {
                 m_WaitingPackages.Remove(sequence);
             }
         }
         private WaitingPackageInfo GetPackage(UInt32 sequence)
         {
             lock (m_WaitingPackages)
             {
                 if (m_WaitingPackages.ContainsKey(sequence) == true)
                     return m_WaitingPackages[sequence];
                 return null;
             }
         }
         private void UploadWaitingPackageInfo(UInt32 sequence, Object payload)
         {
             lock (m_WaitingPackages)
             {
                 if (m_WaitingPackages.ContainsKey(sequence) == true)
                 {
                     m_WaitingPackages[sequence].Payload = payload;
                     m_WaitingPackages[sequence].WaitHandle.Set();
                 }
             }
         }
         #endregion
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
                 Console.WriteLine("Stream Disposed.");
                 StopReceive();
                 if (m_Client_Stream != null)
                 {
                     m_Client_Stream = null;
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
     public class Howell5198FileStream : IDisposable
     {
         public Howell5198FileStream(Howell5198Client client, String ipAddress, Int32 port, GetFileRequest getFileRequest)
         {
             m_Client = client;
             m_IPAddress = ipAddress;
             m_Port = port;
             m_GetFileRequest = getFileRequest;
             ChannelNo = getFileRequest.ChannelNo;
             Type = getFileRequest.Type;
             Beg = getFileRequest.Beg;
             End = getFileRequest.End;
             this.Timeout = 10000;
         }
         private Howell5198Client m_Client;
         private FixedHeaderProtocolClient<ProtocolHeader> m_Client_Stream = null;
         private String m_IPAddress = null;
         private Int32 m_Port = 0;
         private GetFileRequest m_GetFileRequest = null;
         private Dictionary<UInt32, WaitingPackageInfo> m_WaitingPackages = new Dictionary<uint, WaitingPackageInfo>();

         /// <summary>
         /// 通道号
         /// </summary>
         public Int32 ChannelNo { get; set; }

         /// <summary>
         /// 开始时间
         /// </summary>
         public SystemTimeInfo Beg { get; set; }
         /// <summary>
         /// 结束时间
         /// </summary>
         public SystemTimeInfo End { get; set; }
         /// <summary>
         /// 0=all 1=normal file 2=mot file 3=alarm 4=mot alarm 5=ipcam lost 6=analyze metadata
         /// </summary>
         public Int32 Type { get; set; }

         /// <summary>
         /// 该Stream所属的Client对象
         /// </summary>
         public Howell5198Client Client
         {
             get
             {
                 return m_Client;
             }
         }
         /// <summary>
         /// 流会话是否在连接中
         /// </summary>
         public bool Connected() { return m_Client_Stream.IsConnected; }
         /// <summary>
         /// 超时的毫秒值
         /// </summary>
         public Int32 Timeout { get; set; }
         /// <summary>
         /// 开始获取流数据
         /// <returns>请求流是否成功</returns>
         /// </summary>
         public void StartReceive()
         {
             m_Client.Closed += new EventHandler(m_Client_Closed);
             m_Client_Stream = new FixedHeaderProtocolClient<ProtocolHeader>(m_IPAddress, m_Port);
             m_Client_Stream.DataReceived += new EventHandler<ClientDataReceivedEventArgs<ProtocolHeader>>(m_Client_DataReceived);
             m_Client_Stream.Error += new EventHandler<ClientErrorEventArgs>(m_Client_Error);
             m_Client_Stream.Connect();
             Send(COMMAND.GetFile, m_GetFileRequest.GetBytes());
         }
         /// <summary>
         /// 停止获取流数据
         /// </summary>
         public void StopReceive()
         {
             if (m_Client_Stream != null)
             {
                 if (m_Client_Stream.IsConnected == true)
                     m_Client_Stream.Close();
                 m_Client_Stream.Dispose();
                 m_Client_Stream = null;
             }
         }

         /// <summary>
         /// 接收到单个用户发送的流数据
         /// </summary>
         public event EventHandler<FileDataReceivedEventArgs> FileDataReceived;

         void m_Client_Closed(object sender, EventArgs e)
         {
             Dispose();
         }
         void m_Client_DataReceived(object sender, ClientDataReceivedEventArgs<ProtocolHeader> e)
         {
             //应答信息
             try
             {
                 if (e.PackageInfo.Header.proType == COMMAND.GetFile)
                 {
                     if (e.PackageInfo.Header.errornum != 0)
                     {

                         return;
                     }
                    GetFileResponse result = new GetFileResponse();
                    result.FromBytes(e.PackageInfo.Payload, 0, e.PackageInfo.Payload.Length);

                    //视频数据递交
                    if (FileDataReceived != null)
                    {
                        FileDataReceived(this, new FileDataReceivedEventArgs(result));
                    }
                     return;
                 }

             }
             catch { }

         }
         void m_Client_Error(object sender, ClientErrorEventArgs e)
         {

         }

         private TResult Send<TResult>(UInt32 command, byte[] payload)
            where TResult : class, new()
         {
             UInt32 cSeq = command;

             Send(command, payload);

             using (WaitingPackageInfo package = new WaitingPackageInfo(cSeq))
             {
                 AddPackage(package);
                 try
                 {
                     if (package.WaitHandle.WaitOne(this.Timeout) == false)
                     {
                         throw new TimeoutException();
                     }
                     if (package.Payload == null) return null;
                     return (package.Payload as TResult);
                 }
                 finally
                 {
                     RemovePackage(cSeq);
                 }
             }
         }
         private void Send(UInt32 command, byte[] payload)
         {
             m_Client_Stream.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, dataLen = (uint)payload.Length }, payload));

         }

         private TResult Get<TResult>(UInt32 command)
             where TResult : class, new()
         {
             UInt32 cSeq = command;
             using (WaitingPackageInfo package = new WaitingPackageInfo(cSeq))
             {
                 AddPackage(package);
                 try
                 {
                     if (package.WaitHandle.WaitOne(this.Timeout) == false)
                     {
                         throw new TimeoutException();
                     }
                     return (package.Payload as TResult);
                 }
                 finally
                 {
                     RemovePackage(cSeq);
                 }
             }
         }
         #region Packages
         private void AddPackage(WaitingPackageInfo value)
         {
             lock (m_WaitingPackages)
             {
                 m_WaitingPackages.Add(value.Sequence, value);
             }
         }
         private void RemovePackage(UInt32 sequence)
         {
             lock (m_WaitingPackages)
             {
                 m_WaitingPackages.Remove(sequence);
             }
         }
         private WaitingPackageInfo GetPackage(UInt32 sequence)
         {
             lock (m_WaitingPackages)
             {
                 if (m_WaitingPackages.ContainsKey(sequence) == true)
                     return m_WaitingPackages[sequence];
                 return null;
             }
         }
         private void UploadWaitingPackageInfo(UInt32 sequence, Object payload)
         {
             lock (m_WaitingPackages)
             {
                 if (m_WaitingPackages.ContainsKey(sequence) == true)
                 {
                     m_WaitingPackages[sequence].Payload = payload;
                     m_WaitingPackages[sequence].WaitHandle.Set();
                 }
             }
         }
         #endregion
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
                 Console.WriteLine("Stream Disposed.");
                 StopReceive();
                 if (m_Client_Stream != null)
                 {
                     m_Client_Stream = null;
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
}
