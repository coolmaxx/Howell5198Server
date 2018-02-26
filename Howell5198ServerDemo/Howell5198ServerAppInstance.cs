using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Howell5198;
using Howell5198.Protocols;
using System.Threading;
using System.IO;
using Howell.IO.Serialization;
using Howell.eCamera.Middlewares;
using Howell.Net.DataService.Authentication;
using Howell.Net.DataService.Management;
using Howell.Cryptography;
using Howell.Net.DataService.Video;
using Howell.Industry;
using Howell.Net.DeviceService.Network;
using Howell.Net.DeviceService;
using Howell.Net.DeviceService.DecodingUnits;
using Howell.Net.DeviceService.PanoCameras;

namespace HW5198Service
{
    public class Howell5198ServerAppInstance : IHowell5198ServerContract, IDisposable
    {
        private Howell5198Server m_Server = null;
        private Dictionary<String, double> targetfilelen = new Dictionary<String, double>();//指定beg时间的录像文件的长度
        private Dictionary<MediaStreamIdentifier, MTSession> m_mtsessions = new Dictionary<MediaStreamIdentifier, MTSession>();//所有的预览流
        private Dictionary<MTClient, MediaStreamSession> m_filesessions = new Dictionary<MTClient, MediaStreamSession>();//所有的回放流
        private Dictionary<MediaStreamIdentifier, HWheader> m_HWheaders = new Dictionary<MediaStreamIdentifier, HWheader>();//所有通道的HW头
        private Dictionary<String, Howell5198Session> m_AlarmSessions = new Dictionary<String, Howell5198Session>();//所有的注册报警的session
        private readonly Object mtsessions_lock = new Object();
        private readonly Object AddHWheaderLock = new Object();
        private DeviceSystemClient SystemClient = new DeviceSystemClient(String.Format("{0}/System",ServiceConfiguration.Instance.DeviceServiceAddress));
        private DeviceNetworkClient NetworkClient = new DeviceNetworkClient(String.Format("{0}/Components/Network", ServiceConfiguration.Instance.DeviceServiceAddress));
        private DecodingUnitsClient DecodingClient = new DecodingUnitsClient(String.Format("{0}/Components/DecodingUnits", ServiceConfiguration.Instance.DeviceServiceAddress));
        private PanoCamerasClient CamerasClient = new PanoCamerasClient(String.Format("{0}/Components/PanoCameras", ServiceConfiguration.Instance.DeviceServiceAddress));
        private System.Timers.Timer AlarmTimer = new System.Timers.Timer();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        public Howell5198ServerAppInstance(Int32 port)
        {
            m_Server = new Howell5198Server(port, this);

            m_Server.SessionClosed += new EventHandler<SessionClosedEventArgs>(m_Server_SessionClosed);
            m_Server.SessionRegistered += new EventHandler<SessionRegisteredEventArgs>(m_Server_SessionRegistered);
            m_Server.MediaStreamSessionRegistered += new EventHandler<MediaStreamSessionRegisteredEventArgs>(m_Server_MediaStreamSessionRegistered);
            m_Server.MediaStreamSessionClosed += new EventHandler<MediaStreamSessionClosedEventArgs>(m_Server_MediaStreamSessionClosed);
            m_Server.Error += new EventHandler<Howell5198.ErrorEventArgs>(m_Server_Error);
            AlarmTimer.Enabled = true;
            AlarmTimer.Interval =3000;
            //设置是否重复计时，如果该属性设为False,则只执行timer_Elapsed方法一次。
            AlarmTimer.AutoReset = true;
            AlarmTimer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);      
        }
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_AlarmSessions)
            {
                LinkedList<string> Useless = new LinkedList<string>();
                foreach (KeyValuePair<string, Howell5198Session> kvp in m_AlarmSessions)
                {
                    if (kvp.Value.IsConnected())
                    {
                        AlarmData alarmData = new AlarmData();
                        kvp.Value.TrySend(0, ProtocolType.AlarmHeartbeat, alarmData.GetBytes());
                    }
                    else
                    {
                        Useless.AddLast(kvp.Key);
                    }
                }
                foreach(string key in Useless)
                {
                    m_AlarmSessions.Remove(key);
                }
            }
            

        }

        public void Start()
        {
            m_Server.Start();
           AlarmTimer.Start();
            Console.WriteLine("服务端已启动");
            ServiceEnvironment.Instance.Logger.Info("服务端已启动");
        }

        public void Stop()
        {
            AlarmTimer.Stop();
            try
            {
                foreach (MTSession val in m_mtsessions.Values)
                {
                    val.Close();
                    Console.WriteLine(String.Format("通道{0}的类型{1}视频流已断开", val.StreamIdentifier.ChannelNo, val.StreamIdentifier.StreamNo));
                    ServiceEnvironment.Instance.Logger.Info(String.Format("通道{0}的类型{1}视频流已断开", val.StreamIdentifier.ChannelNo, val.StreamIdentifier.StreamNo));
                }
                Monitor.Enter(mtsessions_lock);
                m_mtsessions.Clear();
                Monitor.Exit(mtsessions_lock);
            }
          catch(Exception ex)
            {
                ServiceEnvironment.Instance.Logger.Error("VideoClient关闭异常",ex);
            }
            lock (m_AlarmSessions)
            {
                m_AlarmSessions.Clear();
            }
            lock (AddHWheaderLock)
            {
                m_HWheaders.Clear();
            }
            m_Server.Stop();
            Dispose();
            
            Console.WriteLine("服务端已停止");
            ServiceEnvironment.Instance.Logger.Info("服务端已停止");
        }

        public static DataManagementClient CreatNewDataManagementClient()
        {
            try
            {
                AuthenticationServiceClient authClient = new AuthenticationServiceClient(ServiceConfiguration.Instance.AuthenticationServiceAddress);
                String username = ServiceConfiguration.Instance.UserName;
                String password = new MD5().EncryptToString(ServiceConfiguration.Instance.Password);
                ServerNonce nonce = authClient.GetNonce(username);
                String clientNonce = Guid.NewGuid().ToString("N");
                String vertifySess = new MD5().EncryptToString(String.Format("{0}@{1}:{2}:{3}:{4}", username, nonce.Domain, nonce.Nonce, clientNonce, password.ToLower()));
                Howell.Net.DataService.Fault sess = authClient.Authenticate(new ClientCredential() { UserName = "howell", PhysicalAddress = "00-00-00-00-00-00", ClientNonce = clientNonce, Domain = nonce.Domain, Nonce = nonce.Nonce, VerifySession = vertifySess.ToLower() });
                DataManagementClient managementClient = new DataManagementClient(ServiceConfiguration.Instance.DataManagementServiceAddress);
                managementClient.SetClientCredential(username, sess.Id, nonce.Domain, vertifySess.ToLower());
                return managementClient;
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("CreatNewDataManagementClient时,{0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error(String.Format("CreatNewDataManagementClient时,{0}", ex.Message));
                throw ex;
            }
            
        }
        public void Dispose()
        {
            m_Server.Dispose();
        }

        private HW_MediaInfo CreatHWHeader(Howell.eCamera.Middlewares.Medium.GETCODECACK codec)
        {
            int videoCodec = 0;
            int audioCodec = 1;
            int audioBits = 8;
            if (codec != null && codec.Media.Meta != null && codec.Media.Meta.Video != null)
            {
                if (codec.Media.Meta.Video.Codec == "h264")
                {
                    videoCodec = 0;
                }
                else if (codec.Media.Meta.Video.Codec == "h265")
                {
                    videoCodec = 0x0f;
                }
                else if (codec.Media.Meta.Video.Codec == "h265_encrypt")
                {
                    videoCodec = 0x10;
                }
                else if (codec.Media.Meta.Video.Codec == "mjpeg")
                {
                    videoCodec = 0x06;
                }
            }
            if (codec != null && codec.Media.Meta != null && codec.Media.Meta.Audio != null)
            {
                if (codec.Media.Meta.Audio.Codec == "hisi_adpcm_div")
                {
                    audioBits = codec.Media.Meta.Audio.BitWidth;
                    audioCodec = 5;
                }
                else if(codec.Media.Meta.Audio.Codec=="g.711a")
                {
                    
                    audioBits = codec.Media.Meta.Audio.BitWidth;
                    audioCodec = 2;
                }
            }
            HW_MediaInfo header = new HW_MediaInfo()
            {
                Media_fourcc = 0x48574D49,
                Adec_code = audioCodec,//G.711U,
                Dvr_version = 0,
                Vdec_code = videoCodec,//H264
                Au_bits = (Byte)audioBits,
                Au_channel = 1,
                Au_sample = 8,
                Reserve = new Byte[3],
                Reserved = new Int32[3]
            };
            return header;
        }

        void m_Server_SessionRegistered(object sender, SessionRegisteredEventArgs e)
        {
            Console.WriteLine("客户:{0} 已连接,IP:{1}",e.Session.Context.UserName,e.Session.RemoteEndPoint.ToString());
            ServiceEnvironment.Instance.Logger.Info(String.Format("客户:{0} 已连接,IP:{1}", e.Session.Context.UserName, e.Session.RemoteEndPoint.ToString()));
        }

        void m_Server_MediaStreamSessionRegistered(object sender, MediaStreamSessionRegisteredEventArgs e)
        {    
            try
            {
                var solt = e.Session.Context.StreamIdentifier;
                Console.WriteLine(String.Format("请求通道{0}的类型{1}流会话{2} 已建立,来自客户端的流连接总数为{3}", solt.ChannelNo, solt.StreamNo, e.Session.Context.SessionID, m_Server.StreamCount));
                ServiceEnvironment.Instance.Logger.Info(String.Format("请求通道{0}的类型{1}流会话{2} 已建立,来自客户端的流连接总数为{3}", solt.ChannelNo, solt.StreamNo, e.Session.Context.SessionID, m_Server.StreamCount));

                Byte[] FrameData = null;
                lock (AddHWheaderLock)
                {
                    if (m_HWheaders.ContainsKey(solt) == false)
                    {
                        m_HWheaders.Add(solt, new HWheader());
                    }
                }
                lock (m_HWheaders[solt])
                {
                    if (m_HWheaders[solt].Header != null)
                    {
                        if (m_HWheaders[solt].IsOnLine==false)//判定是否需要重新取头
                        {
                            TimeSpan ts = DateTime.Now - m_HWheaders[solt].DisconnectTime;
                            TimeSpan disabledtime = new TimeSpan(0, 0, 60);
                            if (ts > disabledtime)//下线时间超过了60秒，重新取头
                            {
                                Howell.eCamera.Middlewares.Medium.GETCODECACK codec = m_mtsessions[solt].VideoClient.GetCodec(m_mtsessions[solt].DeviceID, m_mtsessions[solt].ChannelNo, solt.StreamNo);
                                Console.WriteLine("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]);
                                ServiceEnvironment.Instance.Logger.Info(String.Format("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]));
                                m_HWheaders[solt].Header = CreatHWHeader(codec);
                            }
                            m_HWheaders[solt].IsOnLine = true;
                        }
                    }
                    else
                    {
                        Howell.eCamera.Middlewares.Medium.GETCODECACK codec = m_mtsessions[solt].VideoClient.GetCodec(m_mtsessions[solt].DeviceID, m_mtsessions[solt].ChannelNo, solt.StreamNo);
                        Console.WriteLine("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]);
                        ServiceEnvironment.Instance.Logger.Info(String.Format("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]));
                        m_HWheaders[solt].Header = CreatHWHeader(codec);
                        m_HWheaders[solt].IsOnLine = true;
                    }
                    FrameData = m_HWheaders[solt].Header.GetBytes();
                }
                if (e.Session.IsConnected() == false)
                {
                    Console.WriteLine("m_Server_MediaStreamSessionRegistered时，Session断开连接");
                    ServiceEnvironment.Instance.Logger.Warn("m_Server_MediaStreamSessionRegistered时，Session断开连接");
                    return;
                }
                e.Session.TrySend(1, FrameData,100,10);
                m_mtsessions[solt].Subscribe(e.Session);
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("m_Server_MediaStreamSessionRegistered时,{0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Warn(String.Format("m_Server_MediaStreamSessionRegistered时,{0}", ex.Message));
            }
            
        }

        void m_Server_SessionClosed(object sender, SessionClosedEventArgs e)
        {
           Console.WriteLine(String.Format("客户：{0} 已断开", e.Session.Context.UserName));
           ServiceEnvironment.Instance.Logger.Info(String.Format("客户：{0} 已断开", e.Session.Context.UserName));     
        }
        void m_Server_MediaStreamSessionClosed(object sender, MediaStreamSessionClosedEventArgs e)
        {   
            try
            {
                var solt = e.MediaStreamSession.Context.StreamIdentifier;
                if (e.MediaStreamSession.Context.IsFileStream)
                {
                    lock (m_filesessions)
                    {
                        MTClient fileclient = m_filesessions.FirstOrDefault(q => q.Value == e.MediaStreamSession).Key;
                        fileclient.Dispose();
                        m_filesessions.Remove(fileclient);
                    }
                    Console.WriteLine(String.Format("回放流会话{0} 已断开,回放流连接总数为{1},关闭原因{2}", e.MediaStreamSession.Context.SessionID, m_filesessions.Count, e.CloseReason));
                    ServiceEnvironment.Instance.Logger.Info(String.Format("回放流会话{0} 已断开,回放流连接总数为{1},关闭原因{2}", e.MediaStreamSession.Context.SessionID, m_filesessions.Count, e.CloseReason));
                    return;
                }


                Console.WriteLine(String.Format("请求通道{0}的类型{1}流会话{2} 已断开,来自客户端的流连接总数为{3},关闭原因{4}", solt.ChannelNo, solt.StreamNo, e.MediaStreamSession.Context.SessionID, m_Server.StreamCount, e.CloseReason));
                ServiceEnvironment.Instance.Logger.Info(String.Format("请求通道{0}的类型{1}流会话{2} 已断开,来自客户端的流连接总数为{3},关闭原因{4}", solt.ChannelNo, solt.StreamNo, e.MediaStreamSession.Context.SessionID, m_Server.StreamCount, e.CloseReason));


                if (m_mtsessions.ContainsKey(solt) == false)
                    return;
                m_mtsessions[solt].RemoveSession(e.MediaStreamSession);

                if (m_mtsessions[solt].MediaStreamSessions != null && m_mtsessions[solt].MediaStreamSessions.Count > 0)//还存在指定通道号的流的话就跳过断开VideoClient;
                {
                }
                else
                {
                    lock (mtsessions_lock)
                    {
                        if (m_mtsessions.ContainsKey(solt))
                        {
                            MTSession target = m_mtsessions[solt];
                            m_mtsessions.Remove(solt); 
                            target.Close();
                            Console.WriteLine(String.Format("通道{0}的类型{1}预览流已断开,目前接收视频流的来源数量为{2}", solt.ChannelNo, solt.StreamNo, m_mtsessions.Count));
                            ServiceEnvironment.Instance.Logger.Info(String.Format("通道{0}的类型{1}预览流已断开,目前接收视频流的来源数量为{2}", solt.ChannelNo, solt.StreamNo, m_mtsessions.Count));
                            m_HWheaders[solt].IsOnLine = false;
                            m_HWheaders[solt].DisconnectTime = DateTime.Now;
                        }
                        else
                            return;
                    }
                }   
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("m_Server_MediaStreamSessionClosed时,{0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Warn(String.Format("m_Server_MediaStreamSessionClosed时,{0}", ex.Message));
            }
        }

        void m_Server_Error(object sender, Howell5198.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
            ServiceEnvironment.Instance.Logger.Error("5198Server_NewRequestReceived Error.", e.Exception);
            if (e.Exception.StackTrace!=null)
            {
                ServiceEnvironment.Instance.Logger.Error(e.Exception.StackTrace);
            }
            if(e.Exception.InnerException!=null)
            {
                ServiceEnvironment.Instance.Logger.Error("InnerException", e.Exception.InnerException);
            }
        }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="type">暂时未使用</param>
        /// <param name="logName">用户名</param>
        /// <param name="logPassword">密码</param>
        /// <param name="clientUserID">暂时未使用</param>
        /// <returns>服务器应答</returns>4
        public LoginResponse Login(Howell5198Session vcSession,LoginRequest loginrequest)
        {
            LoginResponse response = new LoginResponse();
            response.Success = 0;
            vcSession.ErrorNo = 0;
            return response;
        }
        /// <summary>
        /// 获取服务器信息
        /// </summary>
        /// <returns>服务器应答</returns>
        public ServerInfo GetServerInfo(Howell5198Session vcSession)
        {
            Console.WriteLine("客户端请求ServerInfo");
            ServiceEnvironment.Instance.Logger.Info("客户端请求ServerInfo");
            ServerInfo response = new ServerInfo();
            response.SlotCount = 9999;
            response.SverVersion = 65000;
            response.NetVersion = 1;
            vcSession.ErrorNo = 0;
            return response;
        }
        /// <summary>
        /// 获取实时编码数据
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="channelno">通道号</param>
        /// <param name="type">0:主码流 1:子码流</param>
        /// <returns>服务器应答</returns>
        public StreamResponse GetStream(MediaStreamSession session)
        {
            StreamResponse response = new StreamResponse();
            var solt = session.Context.StreamIdentifier;
            Monitor.Enter(mtsessions_lock);
            try
            {
                if (m_mtsessions.ContainsKey(solt) == false)
                {
                    try
                    {
                        m_mtsessions.Add(solt, new MTSession(solt));       
                        m_mtsessions[solt].Connect(session);
                    }
                    catch (Exception ex)
                    {
                        response.Success = -1;
                        m_mtsessions[solt].Close();
                        m_mtsessions.Remove(solt);
                        Console.WriteLine(String.Format("GetStream Error. {0} ", ex.Message));
                        ServiceEnvironment.Instance.Logger.Error("GetStream Error.", ex);
                    }
                }
                else
                {
                    if (m_mtsessions[solt].VideoClient == null)
                    {
                        response.Success = -1;
                        Console.WriteLine("GetStream Error. VideoClient is Disposed");
                        ServiceEnvironment.Instance.Logger.Error("GetStream Error. VideoClient is Disposed");
                        m_mtsessions.Remove(solt);
                    }
                    else if (m_mtsessions[solt].VideoClient.IsConnected == false)//VideoClient超时,断开相关的MediaStreamSession
                    {
                        response.Success = -1;
                        int streamcount = m_mtsessions[solt].MediaStreamSessions.Count;
                        m_mtsessions[solt].Close();
                        Console.WriteLine(String.Format("{0}个流会话 已断开,来自客户端的流连接总数为{1},关闭原因{2}", streamcount, m_Server.StreamCount, "VideoClient is unConnected"));
                        ServiceEnvironment.Instance.Logger.Info(String.Format("{0}个流会话 已断开,来自客户端的流连接总数为{1},关闭原因{2}", streamcount, m_Server.StreamCount, "VideoClient is unConnected"));
                       
                        Console.WriteLine("GetStream Error. VideoClient is unConnected");
                        ServiceEnvironment.Instance.Logger.Info("GetStream Error. VideoClient is unConnected");
                        m_mtsessions.Remove(solt);
                    }
                    else
                    {
                        TimeSpan ts = DateTime.Now - m_mtsessions[solt].VideoClient.LastMediaPacketTime;
                        TimeSpan timeout2 = new TimeSpan(0, 0, 40);
                        if (ts > timeout2)//超过40秒没收到过数据
                        {
                            response.Success = -1;
                            Console.WriteLine(String.Format("通道{0}类型{1}的MTClient超过40秒未转发数据,服务端主动断开", solt.ChannelNo, solt.StreamNo));
                            ServiceEnvironment.Instance.Logger.Warn(String.Format("通道{0}类型{1}的MTClient超过40秒未转发数据,服务端主动断开", solt.ChannelNo, solt.StreamNo));
                            m_mtsessions[solt].Close();
                            m_mtsessions.Remove(solt);
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(mtsessions_lock);
            }
            return response;       
        }

        /// <summary>
        /// 获取色彩
        /// </summary>
        /// <param name="channelno">通道号</param>
        /// <returns>帧数据</returns>
        public ColorInfo GetColor(Howell5198Session session, GetColorRequest getColorRequest)
        {
            Console.WriteLine("客户端请求ColorInfo");
            ColorInfo response = new ColorInfo();
            response.Slot = 0;
            response.Brightness = 110;
            return response;
        }
        /// <summary>
        /// 设置色彩
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setColorRequest"></param>
        /// <returns>GetColorResponse</returns>
        public SetColorResponse SetColor(Howell5198Session session, ColorInfo setColorRequest)
        {
            Console.WriteLine("客户端设置ColorInfo");
            SetColorResponse response = new SetColorResponse();
            return response;
        }
        /// <summary>
        /// 获取通道名称
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getOsdChannelRequest"></param>
        /// <returns>GetOsdChannelResponse</returns>
        public OsdChannelInfo GetOsdChannel(Howell5198Session session, GetOsdChannelRequest getOsdChannelRequest)
        {

            Console.WriteLine("客户端请求OsdChannelInfo");
            OsdChannelInfo response = new OsdChannelInfo();
            response.Left = 0;
            response.Top = 0;
            response.Name = "OsdChannel";
            return response;
        }
        /// <summary>
        /// 设置通道名称
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setOsdChannelRequest"></param>
        /// <returns>SetOsdChannelResponse</returns>
        public SetOsdChannelResponse SetOsdChannel(Howell5198Session session, OsdChannelInfo setOsdChannelRequest)
        {
            Console.WriteLine("客户端设置OsdChannelInfo");
            SetOsdChannelResponse response = new SetOsdChannelResponse();
            return response;
        }
        /// <summary>
        /// 获取通道日期
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getOsdDateRequest"></param>
        /// <returns>GetOsdDateResponse</returns>
        public OsdDateInfo GetOsdDate(Howell5198Session session, GetOsdDateRequest getOsdDateRequest)
        {
            Console.WriteLine("客户端请求OsdDateInfo");
            OsdDateInfo response = new OsdDateInfo();
            return response;
        }
        /// <summary>
        /// 设置通道日期
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getOsdDateRequest"></param>
        /// <returns>GetOsdDateResponse</returns>
        public SetOsdDateResponse SetOsdDate(Howell5198Session session, OsdDateInfo setOsdDateRequest)
        {
            Console.WriteLine("客户端设置OsdDateInfo");
            SetOsdDateResponse response = new SetOsdDateResponse();
            return response;
        }
        /// <summary>
        /// 获取图像质量
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getVideoQualityRequest"></param>
        /// <returns>GetVideoQualityResponse</returns>
        public VideoQualityInfo GetVideoQuality(Howell5198Session session, GetVideoQualityRequest getVideoQualityRequest)
        {
            Console.WriteLine("客户端请求VideoQualityInfo");
            VideoQualityInfo response = new VideoQualityInfo();
            response.EncodeType = 3;
            return response;
        }
        /// <summary>
        /// 设置图像质量
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setVideoQualityRequest"></param>
        /// <returns>SetVideoQualityResponse</returns>
        public SetVideoQualityResponse SetVideoQuality(Howell5198Session session, VideoQualityInfo setVideoQualityRequest)
        {
            Console.WriteLine("客户端设置VideoQualityInfo");
            SetVideoQualityResponse response = new SetVideoQualityResponse();
            return response;
        }
        /// <summary>
        /// 获取码流类型
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getStreamTypeRequest"></param>
        /// <returns>GetStreamTypeResponse</returns>
        public StreamTypeInfo GetStreamType(Howell5198Session session, GetStreamTypeRequest getStreamTypeRequest)
        {
            Console.WriteLine("客户端请求StreamTypeInfo");
            StreamTypeInfo response = new StreamTypeInfo();
            return response;
        }
        /// <summary>
        /// 设置码流类型
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setStreamTypeRequest"></param>
        /// <returns>SetStreamTypeResponse</returns>
        public SetStreamTypeResponse SetStreamType(Howell5198Session session, StreamTypeInfo setStreamTypeRequest)
        {
            Console.WriteLine("客户端设置StreamTypeInfo");
            SetStreamTypeResponse response = new SetStreamTypeResponse();
            return response;
        }
        /// <summary>
        /// 获取网络
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetNetInfoResponse</returns>
        public NetInfo GetNetInfo(Howell5198Session session)
        {
            Console.WriteLine("客户端请求NetInfo");
            NetInfo response = new NetInfo();
            response.SDvrIp = "127.0.0.1";
            response.SDvrMaskIp = "255.255.255.0";
            response.SPPPoEIP = "192.168.21.133";
            response.SPPPoEPassword = "12345";
            response.SPPPoEUser = "admin";
            response.Gateway = "192.168.18.1";
            response.Dns = "8.8.8.8";
            response.SMultiCastIP = "0.0.0.0";
            return response;
        }
        /// <summary>
        /// 设置网络
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setNetInfoRequest"></param>
        /// <returns>SetNetInfoResponse</returns>
        public SetNetInfoResponse SetNetInfo(Howell5198Session session, NetInfo setNetInfoRequest)
        {
            Console.WriteLine("客户端设置NetInfo");
            SetNetInfoResponse response = new SetNetInfoResponse();
            return response;
        }
        /// <summary>
        /// 获取时间
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        public SystemTimeInfo GetSystemTime(Howell5198Session session)
        {
            Console.WriteLine("客户端请求SystemTimeInfo");
            SystemTimeInfo response = LocalInstance.GetSystemTime();
            return response;
        }
        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setSystemTimeRequest"></param>
        /// <returns>SetSystemTimeResponse</returns>
        public SetSystemTimeResponse SetSystemTime(Howell5198Session session, SystemTimeInfo setSystemTimeRequest)
        {
            Console.WriteLine("客户端设置SystemTimeInfo");
            SetSystemTimeResponse response = new SetSystemTimeResponse();
            return response;
        }
        /// <summary>
        /// 重启设备
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        public RestartDeviceResponse RestartDevice(Howell5198Session session)
        {
            RestartDeviceResponse response = new RestartDeviceResponse();
            try
            {
                SystemClient.Reboot();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("RestartDevice Error. {0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error("RestartDevice Error", ex);
                response.Success = -1;
            }
            return response;
        }
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        public CloseDeviceResponse CloseDevice(Howell5198Session session)
        {
            CloseDeviceResponse response = new CloseDeviceResponse();
            try
            {
                SystemClient.Shutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("CloseDevice Error. {0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error("CloseDevice Error", ex);
                response.Success = -1;
            }
            return response;
        }
        /// <summary>
        /// 重置设备
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        public ResetDeviceResponse ResetDevice(Howell5198Session session)
        {
            ResetDeviceResponse response = new ResetDeviceResponse();
            return response;
        }
        /// <summary>
        /// 获取串口模式
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        public Rs232CfgInfo GetRs232Cfg(Howell5198Session session, GetRs232CfgRequest getRs232CfgRequest)
        {
            Console.WriteLine("客户端请求Rs232CfgInfo");
            Rs232CfgInfo response = new Rs232CfgInfo();
            return response;
        }
        /// <summary>
        /// 设置串口模式
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setSystemTimeRequest"></param>
        /// <returns>SetSystemTimeResponse</returns>
        public SetRs232CfgResponse SetRs232Cfg(Howell5198Session session, Rs232CfgInfo setRs232CfgRequest)
        {
            Console.WriteLine("客户端设置Rs232CfgInfo");
            SetRs232CfgResponse response = new SetRs232CfgResponse();
            return response;
        }
        /// <summary>
        /// 获取PTZ设置
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        public PtzRs232CfgInfo GetPtzRs232Cfg(Howell5198Session session, GetPtzRs232CfgRequest getRs232CfgRequest)
        {
            Console.WriteLine("客户端请求PtzRs232CfgInfo");
            PtzRs232CfgInfo response = new PtzRs232CfgInfo();
            return response;
        }
        /// <summary>
        /// 设置PTZ设置
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setSystemTimeRequest"></param>
        /// <returns>SetSystemTimeResponse</returns>
        public SetPtzRs232CfgResponse SetPtzRs232Cfg(Howell5198Session session, PtzRs232CfgInfo setRs232CfgRequest)
        {
            Console.WriteLine("客户端设置PtzRs232CfgInfo");
            SetPtzRs232CfgResponse response = new SetPtzRs232CfgResponse();
            return response;
        }
        /// <summary>
        /// PTZ命令控制
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="ptzControlRequest"></param>
        /// <returns>PtzControlResponse</returns>
        public PtzControlResponse PtzControl(Howell5198Session session, PtzControlRequest ptzControlRequest)
        {
            Console.WriteLine("客户端请求PtzControl");
            ServiceEnvironment.Instance.Logger.Info("客户端请求PtzControl");
            PtzControlResponse response = new PtzControlResponse();

            var managementClient = CreatNewDataManagementClient();
            var pseudodev = managementClient.GetVideoInputChannelByPseudoCode(Convert.ToString(ptzControlRequest.Slot + 1));
            Identity channelid = Identity.Parse(pseudodev.Id);
            Howell.Net.DataService.Fault error = null;
            if(ptzControlRequest.ControlType==0)//direct
            {
                Howell.Industry.PTZDirection direction=Howell.Industry.PTZDirection.Stop;
                switch(ptzControlRequest.Cmd)
                {
                    case 5:
                        direction=Howell.Industry.PTZDirection.Stop;
                        break;
                    case 8:
                        direction=Howell.Industry.PTZDirection.Up;
                        break;
                    case 2:
                        direction=Howell.Industry.PTZDirection.Down;
                        break;
                    case 4:
                        direction=Howell.Industry.PTZDirection.Left;
                        break;
                    case 6:
                        direction=Howell.Industry.PTZDirection.Right;
                        break;
                    default:
                        direction=Howell.Industry.PTZDirection.Stop;
                        break;
                }
                error=managementClient.PTZDirectionControl(channelid.GetDeviceIdentity().ToString(), pseudodev.Id, direction, ptzControlRequest.Presetno);
            }
            else if(ptzControlRequest.ControlType==1)//len
            {
                Howell.Industry.PTZLens lens=Howell.Industry.PTZLens.Stop;
                if (ptzControlRequest.Cmd > 0 && ptzControlRequest.Cmd<7)
                    lens = (Howell.Industry.PTZLens)ptzControlRequest.Cmd;
                error = managementClient.PTZLensControl(channelid.GetDeviceIdentity().ToString(), pseudodev.Id, lens, ptzControlRequest.Presetno);
            }
            else if(ptzControlRequest.ControlType==3)//preset
            {
                Howell.Industry.PTZPreset preset;
                 switch(ptzControlRequest.Cmd)
                {
                    case 1:
                        preset=Howell.Industry.PTZPreset.Set;
                        break;
                    case 2:
                        preset=Howell.Industry.PTZPreset.Clear;
                        break;
                    case 3:
                        preset=Howell.Industry.PTZPreset.Goto;
                        break;
                    default:
                        preset=Howell.Industry.PTZPreset.Goto;
                        break;
                }
                 error = managementClient.PTZPresetControl(channelid.GetDeviceIdentity().ToString(), pseudodev.Id, preset, ptzControlRequest.Presetno, 50);
            }
            else
            {
                response.Success = -1;
            }
            if (error.FaultCode != 0)
                response.Success = -1;
            return response;
        }

        public SearchFileResponse SearchFile(Howell5198Session session, SearchFileRequest searchFileRequest)
        {
            Console.WriteLine("客户端请求SearchFile");
            ServiceEnvironment.Instance.Logger.Info("客户端请求SearchFile");
            try
            {
                VideoInputChannel pseudodev = CreatNewDataManagementClient().GetVideoInputChannelByPseudoCode(Convert.ToString(searchFileRequest.ChannelNo + 1));
                Identity channelid = Identity.Parse(pseudodev.Id);
                //int solt=searchFileRequest.ChannelNo;
                DateTime beg = new DateTime(searchFileRequest.Beg.WYear, searchFileRequest.Beg.WMonth, searchFileRequest.Beg.WDay, searchFileRequest.Beg.WHour, searchFileRequest.Beg.WMinute, searchFileRequest.Beg.WSecond, searchFileRequest.Beg.WMilliseconds, DateTimeKind.Local);
                DateTime end = new DateTime(searchFileRequest.End.WYear, searchFileRequest.End.WMonth, searchFileRequest.End.WDay, searchFileRequest.End.WHour, searchFileRequest.End.WMinute, searchFileRequest.End.WSecond, searchFileRequest.End.WMilliseconds, DateTimeKind.Local);
                TimeSpan ts = end - beg;
                TimeSpan maxtime = new TimeSpan(30, 0, 0, 0, 0);
                if (ts > maxtime)
                    beg = end - maxtime;
                MTClient Client = null;
                IList<Howell.eCamera.Middlewares.Medium.RECORDEDFILE> recordedfiles = null;
                using (Client = new MTClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ServiceConfiguration.Instance.MTServerIP), ServiceConfiguration.Instance.MTServerPort)))
                {
                    Client.Credential = new MTClientCredential() { UserName = ServiceConfiguration.Instance.UserName, Password = ServiceConfiguration.Instance.Password, MobileTerminalId = Guid.NewGuid().ToString("N") };
                    Client.Connect();
                    recordedfiles = Client.GetRecordedFiles(channelid.GetDeviceIdentity().ToString(), channelid.ModuleNumber - 1, beg, end);
                }
                SearchFileResponse response = new SearchFileResponse();
                int count = 100;
                if (recordedfiles.Count < 100)
                    count = recordedfiles.Count;
                response.FileInfos = new SearchFileRequest[count];
                for (int i = 0; i < count; i++)
                {
                    response.FileInfos[i] = new SearchFileRequest() { Beg = new SystemTimeInfo(((DateTime)recordedfiles[i].BeginTime).ToLocalTime()), End = new SystemTimeInfo(((DateTime)recordedfiles[i].EndTime).ToLocalTime()), ChannelNo = searchFileRequest.ChannelNo, Type = searchFileRequest.Type };
                }
                return response;
            }
            catch(Exception ex)
            {
                Console.WriteLine("SearchFile Error." + ex.Message);
                ServiceEnvironment.Instance.Logger.Error("SearchFile Error.", ex);
                return null;
            }

            //SearchFileResponse response = new SearchFileResponse();
            //response.FileInfos = new SearchFileRequest[1];
            //response.FileInfos[0] = new SearchFileRequest() { Beg = searchFileRequest.Beg, End = searchFileRequest.End, ChannelNo = searchFileRequest.ChannelNo, Type = searchFileRequest.Type };
            //return response;
        }
        public void GetFile(MediaStreamSession session, GetFileRequest getFileRequest)
        {
            Console.WriteLine("客户端请求GetFile");
            ServiceEnvironment.Instance.Logger.Info("客户端请求GetFile");
            Byte[] Buffer = new byte[100];
            int offset2 = 0;
            LittleEndian.WriteInt32(0, Buffer, ref offset2, 100);//beg_tm
            LittleEndian.WriteInt32(0, Buffer, ref offset2, 100);//end_tm
            int curfilelen=0;
            String begtime=getFileRequest.Beg.ToString();
            if(targetfilelen.ContainsKey(begtime))
                curfilelen=Convert.ToInt32(targetfilelen[begtime]);
            LittleEndian.WriteInt32(curfilelen, Buffer, ref offset2, 100);//file_len
            LittleEndian.WriteInt32(0, Buffer, ref offset2, 100);//record_type
            session.Send(1, Buffer);

            int solt = getFileRequest.ChannelNo;
            MTClient fileclient=null;
            try
            {
                fileclient = new MTClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ServiceConfiguration.Instance.MTServerIP), ServiceConfiguration.Instance.MTServerPort));
                fileclient.Credential = new MTClientCredential() { UserName = ServiceConfiguration.Instance.UserName, Password = ServiceConfiguration.Instance.Password, MobileTerminalId = session.Context.SessionID };
                fileclient.Connect();  

                VideoInputChannel pseudodev = CreatNewDataManagementClient().GetVideoInputChannelByPseudoCode(Convert.ToString(session.Context.StreamIdentifier.ChannelNo + 1));
                Identity channelid = Identity.Parse(pseudodev.Id);
                fileclient.MediaDataReceived += new MTClientMediaDataReceivedHandler(FileDataReceived);


                DateTime beg = new DateTime(getFileRequest.Beg.WYear, getFileRequest.Beg.WMonth, getFileRequest.Beg.WDay, getFileRequest.Beg.WHour, getFileRequest.Beg.WMinute, getFileRequest.Beg.WSecond, getFileRequest.Beg.WMilliseconds, DateTimeKind.Local);
                DateTime end = new DateTime(getFileRequest.End.WYear, getFileRequest.End.WMonth, getFileRequest.End.WDay, getFileRequest.End.WHour, getFileRequest.End.WMinute, getFileRequest.End.WSecond, getFileRequest.End.WMilliseconds, DateTimeKind.Local);
                fileclient.Subscribe(20000, channelid.GetDeviceIdentity().ToString(), channelid.ModuleNumber - 1, 0, beg, end);
                lock (m_filesessions)
                {
                    m_filesessions.Add(fileclient, session);
                }
                Console.WriteLine(String.Format("回放流会话{0} 已建立,回放流连接总数为{1}", session.Context.SessionID, m_filesessions.Count));
                ServiceEnvironment.Instance.Logger.Info(String.Format("回放流会话{0} 已建立,回放流连接总数为{1}", session.Context.SessionID, m_filesessions.Count));

            }
            catch (Exception ex)
            {
                ServiceEnvironment.Instance.Logger.Error("回放流打开失败.", ex);
                if(fileclient!=null)
                {
                    fileclient.Close();
                    fileclient.Dispose();
                    fileclient = null;
                }
            }       
        }
        void FileDataReceived(MTClient sender, MediaData mediaData)
        {
            //int handle = (int)mediaData.DialogId;
            if (m_filesessions.ContainsKey(sender))//回放流
            {
                if (m_filesessions[sender].IsConnected())
                {
                    int offset = 0;
                    while (offset < mediaData.FrameData.Count)
                    {
                        Byte[] buffer = new Byte[2048];
                        if (mediaData.FrameData.Count > (offset + buffer.Length))
                        {
                            Buffer.BlockCopy(mediaData.FrameData.Array, mediaData.FrameData.Offset + offset, buffer, 0, buffer.Length);
                            offset += buffer.Length;
                        }
                        else
                        {
                            Buffer.BlockCopy(mediaData.FrameData.Array, mediaData.FrameData.Offset + offset, buffer, 0, mediaData.FrameData.Count - offset);
                            offset = mediaData.FrameData.Count;
                        }
                        bool sendout = m_filesessions[sender].TrySend(3, buffer,100,10);
                        if(sendout==false)
                        {
                            m_filesessions[sender].Close();
                            Console.WriteLine("回放流数据发送超时，主动关闭session");
                            ServiceEnvironment.Instance.Logger.Info("回放流数据发送超时，主动关闭session");
                            return;
                        }
                    }
                }
            }
            else
            {
                sender.Dispose();
            }
        }
        public GetFileInfoResponse GetFileInfo(Howell5198Session session, GetFileInfoRequest getFileInfoRequest)
        {
            Console.WriteLine("客户端请求GetFileInfo");
            ServiceEnvironment.Instance.Logger.Info("客户端请求GetFileInfo");
           // int solt = getFileInfoRequest.ChannelNo;
            VideoInputChannel pseudodev = CreatNewDataManagementClient().GetVideoInputChannelByPseudoCode(Convert.ToString(getFileInfoRequest.ChannelNo + 1));
            Identity channelid = Identity.Parse(pseudodev.Id);
            DateTime beg = new DateTime(getFileInfoRequest.Beg.WYear, getFileInfoRequest.Beg.WMonth, getFileInfoRequest.Beg.WDay, getFileInfoRequest.Beg.WHour, getFileInfoRequest.Beg.WMinute, getFileInfoRequest.Beg.WSecond, getFileInfoRequest.Beg.WMilliseconds, DateTimeKind.Local);
            DateTime end = new DateTime(getFileInfoRequest.End.WYear, getFileInfoRequest.End.WMonth, getFileInfoRequest.End.WDay, getFileInfoRequest.End.WHour, getFileInfoRequest.End.WMinute, getFileInfoRequest.End.WSecond, getFileInfoRequest.End.WMilliseconds, DateTimeKind.Local);
            MTClient Client = null;
            try
            {
                Client = new MTClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ServiceConfiguration.Instance.MTServerIP), ServiceConfiguration.Instance.MTServerPort));
                Client.Credential = new MTClientCredential() { UserName = ServiceConfiguration.Instance.UserName, Password = ServiceConfiguration.Instance.Password, MobileTerminalId = Guid.NewGuid().ToString("N") };
                Client.Connect();
                Howell.eCamera.Middlewares.Medium.GETCODECACK codec = Client.GetCodec(channelid.GetDeviceIdentity().ToString(), channelid.ModuleNumber - 1, 0, beg, end);
                Client.Close();
                Client.Dispose();
                Console.WriteLine("GetFileInfo,Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]);

                double videolen = 0;
                double audiolen = 0;
                TimeSpan ts = end.Subtract(beg);
                double totalsec = ts.TotalSeconds;
                int videoCodec = 0;
                int audioCodec = 1;
                int audioBits = 8;
                if (codec != null && codec.Media.Meta != null && codec.Media.Meta.Video != null)
                {
                    if (codec.Media.Meta.Video.Codec == "h264")
                    {
                        videoCodec = 0;
                    }
                    else if (codec.Media.Meta.Video.Codec == "h265")
                    {
                        videoCodec = 0x0f;
                    }
                    else if (codec.Media.Meta.Video.Codec == "h265_encrypt")
                    {
                        videoCodec = 0x10;
                    }
                    else if (codec.Media.Meta.Video.Codec == "mjpeg")
                    {
                        videoCodec = 0x06;
                    }
                    videolen = codec.Media.Meta.Video.Bitrate * totalsec * 1000 / 8;//视频数据的总byte数
                }
                if (codec != null && codec.Media.Meta != null && codec.Media.Meta.Audio != null)
                {
                    if (codec.Media.Meta.Audio.Codec == "hisi_adpcm_div")
                    {
                        audioBits = codec.Media.Meta.Audio.BitWidth;
                        audioCodec = 0x05;
                    }
                    audiolen = codec.Media.Meta.Audio.Samples * codec.Media.Meta.Audio.BitWidth * totalsec / 8;//数据量（字节/秒）= (采样频率（Hz）× 采样位数（bit） × 声道数)/ 8
                }

                GetFileInfoResponse response = new GetFileInfoResponse();
                response.FileFormatType = 65000;
                response.Video_dec = videoCodec;
                response.Audio_dec = audioCodec;
                response.ChannelNo = getFileInfoRequest.ChannelNo;
                response.Beg = getFileInfoRequest.Beg;
                response.End = getFileInfoRequest.End;

                double curfilelen = videolen + audiolen;
                if (curfilelen > Int32.MaxValue)
                    curfilelen = Int32.MaxValue;

                response.Reserved[0] = Convert.ToInt32(curfilelen);
                String begstr = getFileInfoRequest.Beg.ToString();
                lock (targetfilelen)
                {
                    if (targetfilelen.Count >= 50)
                        targetfilelen.Clear();
                    if (targetfilelen.ContainsKey(begstr) == false)
                    {
                        targetfilelen.Add(begstr, curfilelen);
                    }
                }
                return response;
            }
            catch(Exception ex)
            {
                Console.WriteLine("GetFileInfo Error." + ex.Message);
                ServiceEnvironment.Instance.Logger.Error("GetFileInfo Error.", ex);
                if (Client != null)
                {
                    Client.Close();
                    Client.Dispose();
                    Client = null;
                }
                GetFileInfoResponse response = new GetFileInfoResponse();
                return response;
            }
            
        }
        public GetNetHeadResponse GetNetHead(Howell5198Session session, GetNetHeadRequest getNetHeadRequest)
        {
            Console.WriteLine("客户端请求GetNetHead");
            ServiceEnvironment.Instance.Logger.Info("客户端请求GetNetHead");
            var solt = new MediaStreamIdentifier(getNetHeadRequest.ChannelNo, (getNetHeadRequest.IsSub > 0)?1:0);
            VideoInputChannel pseudodev = CreatNewDataManagementClient().GetVideoInputChannelByPseudoCode(Convert.ToString(getNetHeadRequest.ChannelNo + 1));
            Identity channelid = Identity.Parse(pseudodev.Id);

            Byte[] nethead_buf = new Byte[128];
            GetNetHeadResponse response = null;
            MTClient Client = null;
            lock (AddHWheaderLock)
            {
                if (m_HWheaders.ContainsKey(solt)==false)
                {
                    m_HWheaders.Add(solt, new HWheader());    
                }
            }
            lock (m_HWheaders[solt])
            {
                if (m_HWheaders[solt].Header!=null)
                {
                    if (m_HWheaders[solt].IsOnLine==false)
                    {
                        TimeSpan ts = DateTime.Now - m_HWheaders[solt].DisconnectTime;
                        TimeSpan disabledtime = new TimeSpan(0, 0, 60);
                        if(ts>disabledtime)//断线超过了60秒，重新取头
                        {
                            using (Client = new MTClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ServiceConfiguration.Instance.MTServerIP), ServiceConfiguration.Instance.MTServerPort)))
                            {
                                Client.Credential = new MTClientCredential() { UserName = ServiceConfiguration.Instance.UserName, Password = ServiceConfiguration.Instance.Password, MobileTerminalId = Guid.NewGuid().ToString("N") };
                                Client.Connect();
                                Howell.eCamera.Middlewares.Medium.GETCODECACK codec = Client.GetCodec(channelid.GetDeviceIdentity().ToString(), channelid.ModuleNumber - 1, getNetHeadRequest.IsSub);
                                Console.WriteLine("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]);
                                ServiceEnvironment.Instance.Logger.Info(String.Format("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]));
                                m_HWheaders[solt].Header = CreatHWHeader(codec);
                            }
                        }
                        m_HWheaders[solt].IsOnLine = true;
                    }
                    Buffer.BlockCopy(m_HWheaders[solt].Header.GetBytes(), 0, nethead_buf, 0, m_HWheaders[solt].Header.GetLength());
                    response = new GetNetHeadResponse() { ChannelNo = getNetHeadRequest.ChannelNo, IsSub = getNetHeadRequest.IsSub, Buf = nethead_buf, Len = m_HWheaders[solt].Header.GetLength() };
                    return response; 
                }
                else
                {
                    using (Client = new MTClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ServiceConfiguration.Instance.MTServerIP), ServiceConfiguration.Instance.MTServerPort)))
                    {
                        Client.Credential = new MTClientCredential() { UserName = ServiceConfiguration.Instance.UserName, Password = ServiceConfiguration.Instance.Password, MobileTerminalId = Guid.NewGuid().ToString("N") };
                        Client.Connect();
                        Howell.eCamera.Middlewares.Medium.GETCODECACK codec = Client.GetCodec(channelid.GetDeviceIdentity().ToString(), channelid.ModuleNumber - 1, getNetHeadRequest.IsSub);
                        Console.WriteLine("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]);
                        ServiceEnvironment.Instance.Logger.Info(String.Format("Video:{0} Resolution:{1}*{2}", codec.Media.Meta.Video.Codec, codec.Media.Meta.Video.Resolution[0], codec.Media.Meta.Video.Resolution[1]));
                        m_HWheaders[solt].Header = CreatHWHeader(codec);
                        m_HWheaders[solt].IsOnLine = true;
                    }
                    Buffer.BlockCopy(m_HWheaders[solt].Header.GetBytes(), 0, nethead_buf, 0, m_HWheaders[solt].Header.GetLength());
                    response = new GetNetHeadResponse() { ChannelNo = getNetHeadRequest.ChannelNo, IsSub = getNetHeadRequest.IsSub, Buf = nethead_buf, Len = m_HWheaders[solt].Header.GetLength() };
                    return response; 
                }
            }
        }
 

      

        public DeviceConfig GetDeviceConfig(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetDeviceConfig");
            DeviceConfig response = new DeviceConfig(); 
            return response;
        }

        public GetMotionResponse GetMotionSet(Howell5198Session session, GetMotionRequest getMotionRequest)
        {
            Console.WriteLine("客户端请求GetMotionSet");
            GetMotionResponse response = new GetMotionResponse();
            return response;
        }

        public DavinciUsers GetUsers(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetUsers");
            DavinciUsers response = new DavinciUsers();
            return response;
        }

        public SetMotionResponse SetMotionSet(Howell5198Session session, SetMotionRequest setMotionRequest)
        {
            Console.WriteLine("客户端请求SetMotionSet");
            SetMotionResponse response = new SetMotionResponse();
            return response;
        }

        public SyncTimeResponse SyncTime(Howell5198Session session, SyncTimeRequest syncTimeRequest)
        {
            Console.WriteLine("客户端请求SyncTime");
            SyncTimeResponse response = new SyncTimeResponse();
            return response;
        }

        public UpdateUserResponse UpdateUser(Howell5198Session session, UpdateUserRequest updateUserRequest)
        {
            Console.WriteLine("客户端请求UpdateUser");
            UpdateUserResponse response = new UpdateUserResponse();
            return response;
        }


        public CapturenResponse CaptureJpeg(Howell5198Session session, CaptureRequest captureRequest)
        {
            throw new NotImplementedException();
        }

        public NtpInfo GetNtpInfo(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetNtpInfo");
            NtpInfo response = new NtpInfo();
            try
            {
                NTPServer[] ntpservers = SystemClient.GetNTPServers();
                response.Cycletime = (uint)ntpservers[0].PollInterval;
                response.SvrIp = ntpservers[0].HostName;
                response.NtpServerId = ntpservers[0].Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("GetNtpInfo Error. {0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error("GetDeviceConfig Error", ex);
                return null;                                                                                                                                                                                                                                                                                                                                                                                                                                                     
            }

            return response;
        }

        public SetNtpInfoResponse SetNtpInfo(Howell5198Session session, NtpInfo ntpInfo)
        {
            Console.WriteLine("客户端请求SetNtpInfo");
            return new SetNtpInfoResponse();
        }


        public tDeviceInfo GetDeviceInfo(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetDeviceInfo");
            tDeviceInfo response = new tDeviceInfo();
            try
            {
                Howell.Net.DeviceService.DeviceInfo devinfo = SystemClient.GetDeviceInfo();
                response.Id = devinfo.Id;
                response.Name = devinfo.Name;
                response.SerialNumber = devinfo.SerialNumber;
                response.Model = devinfo.Model;
                response.FirmwareVersion = devinfo.FirmwareVersion;
                response.FirmwareReleasedDate = devinfo.FirmwareReleasedDate;
                response.HardwareVersion = devinfo.HardwareVersion;
                if (devinfo.Description!=null)
                {
                    response.Description_enabled = 1;
                    response.Description = devinfo.Description;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("GetDeviceInfo Error. {0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error("GetDeviceInfo Error", ex);
                return null;
            }
            return response;
        }

        public tDeviceStatus GetDeviceStatus(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetDeviceStatus");
            tDeviceStatus response = new tDeviceStatus();
            try
            {
                Howell.Net.DeviceService.DeviceStatus devstatus = SystemClient.GetDeviceStatus();
                response.CurrentTime = devstatus.CurrentTime;
                response.SystemUpTime = devstatus.SystemUpTime;
                if (devstatus.CPU != null)
                {
                    response.CPU_enabled = 1;
                    response.CPU.Name = devstatus.CPU[0].Name;
                    response.CPU.Utilization = devstatus.CPU[0].Utilization;
                }
                if (devstatus.Memory != null)
                {
                    response.Memory_enabled = 1;
                    response.Memory.Usage = devstatus.Memory[0].Usage;
                    response.Memory.TotalSize = devstatus.Memory[0].TotalSize;
                    response.Memory.Description = devstatus.Memory[0].Description;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("GetDeviceStatus Error. {0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error("GetDeviceStatus Error", ex);
                return null;
            }
            return response;
        }

        public tNetworkInterface GetNetworkInterface(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetNetworkInterface");
            tNetworkInterface response = new tNetworkInterface();
            try
            {
                Howell.Net.DeviceService.Network.NetworkInterface[] networkInterfaces = NetworkClient.GetNetworkInterfaces();
                for (int i = 0; i < networkInterfaces.Length;++i )
                {
                    if(networkInterfaces[i].Internal==false)
                    {
                        response.Id = networkInterfaces[i].Id;
                        response.Name = networkInterfaces[i].Name;
                        response.Address = networkInterfaces[i].IPAddress.IPv4Address.Address;
                        response.SubnetMask = networkInterfaces[i].IPAddress.IPv4Address.SubnetMask;
                        response.SpeedDuplex = networkInterfaces[i].SpeedDuplex;
                        response.MTU = networkInterfaces[i].MTU;
                        response.PhysicalAddress = networkInterfaces[i].PhysicalAddress;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("GetNetworkInterface Error. {0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error("GetNetworkInterface Error", ex);
                return null;
            }
            return response;
        }

        public tServiceVersion GetServiceVersion(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetServiceVersion");
            tServiceVersion response = new tServiceVersion();
            try
            {
                Howell.Net.DeviceService.ServiceVersion version = SystemClient.GetVersion();
                response.Version = version.Version;
                response.BuildDate = version.BuildDate;
                response.Company = version.Company;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("GetServiceVersion Error. {0}", ex.Message));
                ServiceEnvironment.Instance.Logger.Error("GetServiceVersion Error", ex);
                return null;
            }
            return response;
        }

        public tFault AddPanoCamera(Howell5198Session session, tPanoCamera panoCamera)
        {
            Console.WriteLine("客户端请求AddPanoCamera");
            return new tFault();
        }

        public tFault DeletePanoCamera(Howell5198Session session, tPanoCameraId panoCameraId)
        {
            Console.WriteLine("客户端请求DeletePanoCamera");
            return new tFault();
        }

        public tDecodingUnit GetDecodingUnit(Howell5198Session session, tDecodingUnitId decodingUnitId)
        {
           Console.WriteLine("客户端请求GetDecodingUnit");
           DecodingUnit decodingUnit = DecodingClient.GetDecodingUnit(decodingUnitId.DecodingUnitId);
           return ConvertClass.Convert(decodingUnit);
           //return new tDecodingUnit();
        }

        public tDecodingUnitList GetDecodingUnitList(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetDecodingUnitList");
            DecodingUnit[] decodingUnits = DecodingClient.GetDecodingUnits();
            tDecodingUnitList response = new tDecodingUnitList() { DecodingUnit_count = decodingUnits.Length };
            response.DecodingUnits = new tDecodingUnit[decodingUnits.Length];
            for (int i = 0; i < decodingUnits.Length; ++i)
            {
                response.DecodingUnits[i] = ConvertClass.Convert(decodingUnits[i]);
            }
            return response;
            //return new tDecodingUnitList();
        }

        public tPanoCamera GetPanoCamera(Howell5198Session session, tPanoCameraId panoCameraId)
        {
            Console.WriteLine("客户端请求GetPanoCamera");
            PanoCamera panoCamera = CamerasClient.GetPanoCamera(panoCameraId.PanoCameraId);
            return ConvertClass.Convert(panoCamera);
            //return new tPanoCamera() { Id="testid",Name="testname"};
        }

        public tPanoCameraList GetPanoCameraList(Howell5198Session session, tQueryString queryString)
        {
            Console.WriteLine("客户端请求GetPanoCameraList");
            int? pageindex = null;
            if (queryString.PageIndex_enabled == 1)
                pageindex = queryString.PageIndex;
            int? pagesize = null;
            if (queryString.PageSize_enabled == 1)
                pagesize = queryString.PageSize;
            PanoCameraList panoCameraList = CamerasClient.GetPanoCameras(pageindex, pagesize);
            return ConvertClass.Convert(panoCameraList);
            //return new tPanoCameraList();
        }

        public tPlayerStatus GetPlayerStatus(Howell5198Session session, tDecodingUnitId decodingUnitId)
        {
            Console.WriteLine("客户端请求GetPlayerStatus");
            var playerStatus = DecodingClient.GetStatus(decodingUnitId.DecodingUnitId);
            tPlayerStatus tplayerStatus = new tPlayerStatus() { Duration = playerStatus.Duration, Seekable = playerStatus.Seekable ? 1 : 0, State = (Howell5198.Protocols.PlayerState)playerStatus.State.Value };
            return tplayerStatus;
            //return new tPlayerStatus();
        }

        public tRotatingSpeed GetRotatingSpeed(Howell5198Session session, tDecodingUnitId decodingUnitId)
        {
            Console.WriteLine("客户端请求GetRotatingSpeed");
            int rotatingSpeed = DecodingClient.GetRotatingSpeed(decodingUnitId.DecodingUnitId);
            return new tRotatingSpeed() { RotatingSpeed = rotatingSpeed, DecodingUnitId = decodingUnitId.DecodingUnitId };
           // return new tRotatingSpeed();
        }

        public tViewPoint GetViewPoint(Howell5198Session session, tDecodingUnitId decodingUnitId)
        {
            Console.WriteLine("客户端请求GetViewPoint");
            ViewPoint viewPoint = DecodingClient.GetViewPoint(decodingUnitId.DecodingUnitId);
            return ConvertClass.Convert(viewPoint);
            //return new tViewPoint();
        }

        public tFault OneByOne(Howell5198Session session, OneByOneRequest oneByOneRequest)
        {
            Console.WriteLine("客户端请求OneByOne");
            return new tFault();
        }

        public tFault Pause(Howell5198Session session, PauseRequest pauseRequest)
        {
            Console.WriteLine("客户端请求Pause");
            return new tFault();
        }

        public tFault Resume(Howell5198Session session, ResumeRequest resumeRequest)
        {
            Console.WriteLine("客户端请求Resume");
            return new tFault();
        }

        public tFault Seek(Howell5198Session session, SeekRequest seekRequest)
        {
            Console.WriteLine("客户端请求Seek");
            return new tFault();
        }

        public tFault SetPanoCamera(Howell5198Session session, tPanoCamera panoCamera)
        {
            Console.WriteLine("客户端请求SetPanoCamera");
            return new tFault();
        }

        public tFault SetRotatingSpeed(Howell5198Session session, tRotatingSpeed rotatingSpeed)
        {
            Console.WriteLine("客户端请求SetRotatingSpeed");
            var fault = DecodingClient.SetRotatingSpeed(rotatingSpeed.DecodingUnitId, rotatingSpeed.RotatingSpeed);
            return new tFault() { FaultCode = fault.FaultCode, FaultReason = fault.FaultReason };
           // return new tFault();
        }

        public tFault SetViewPoint(Howell5198Session session, SetViewPointRequest setViewPointRequest)
        {
            Console.WriteLine("客户端请求SetViewPoint");
            var fault = DecodingClient.SetViewPoint(setViewPointRequest.DecodingUnitId, ConvertClass.Convert(setViewPointRequest.ViewPoint));
            return new tFault() { FaultCode = fault.FaultCode, FaultReason = fault.FaultReason };
            //return new tFault();
        }

        public tFault SetViewPointFixed(Howell5198Session session, SetViewPointFixedRequest setViewPointFixedRequest)
        {
            Console.WriteLine("客户端请求SetViewPointFixed");
            var fault = DecodingClient.SetViewPointFixed(setViewPointFixedRequest.DecodingUnitId, setViewPointFixedRequest.IsFixed == 0 ? false : true);
            return new tFault() { FaultCode = fault.FaultCode, FaultReason = fault.FaultReason };
           // return new tFault();
        }

        public tFault SetViewPointRows(Howell5198Session session, SetViewPointRowsRequest setViewPointRowsRequest)
        {
            Console.WriteLine("客户端请求SetViewPointRows");
            var fault = DecodingClient.SetViewPointRows(setViewPointRowsRequest.DecodingUnitId, setViewPointRowsRequest.Rows);
            return new tFault() { FaultCode = fault.FaultCode, FaultReason = fault.FaultReason };
            //return new tFault();
        }

        public tFault SwitchPanoCamera(Howell5198Session session, SwitchPanoCameraRequest switchPanoCameraRequest)
        {
            Console.WriteLine("客户端请求SwitchPanoCamera");
            var fault = DecodingClient.SetPanoCamera(switchPanoCameraRequest.DecodingUnitId, switchPanoCameraRequest.PanoCameraId);
            return new tFault() { FaultCode = fault.FaultCode, FaultReason = fault.FaultReason };
            //return new tFault();
        }


        public RegisterAlarmResponse SetRegisterAlarm(Howell5198Session session, RegisterAlarmRequest registerAlarmRequest)
        {
            Console.WriteLine("客户端请求SetRegisterAlarm");
            lock (m_AlarmSessions)
            {
                m_AlarmSessions.Add(session.Context.SessionID, session);
            }
            return new RegisterAlarmResponse();
        }


        public GetMotionExResponse GetMotionExSet(Howell5198Session session, GetMotionExRequest getMotionRequest)
        {
            Console.WriteLine("客户端请求GetMotionExSet");
            return new GetMotionExResponse() { Slot = getMotionRequest.ChannelNo };

        }

        public GetSubChannelSetResponse GetSubChannelSet(Howell5198Session session, GetSubChannelSetRequest getMotionRequest)
        {
            Console.WriteLine("客户端请求GetSubChannelSet");
            return new GetSubChannelSetResponse() { Slot = getMotionRequest.ChannelNo,Used=1 ,EncodeType=1};
        }

        public SetMotionExResponse SetMotionExSet(Howell5198Session session, SetMotionExRequest getMotionRequest)
        {
            Console.WriteLine("客户端请求SetMotionExSet");
            return new SetMotionExResponse();
        }

        public SetSubChannelSetResponse SetSubChannelSet(Howell5198Session session, SetSubChannelSetRequest getMotionRequest)
        {
            Console.WriteLine("客户端请求SetSubChannelSet");
            return new SetSubChannelSetResponse();
        }


        public GetNetSyncTimeResponse GetNetSyncTime(Howell5198Session session)
        {
            Console.WriteLine("客户端请求GetNetSyncTime");
            return new GetNetSyncTimeResponse();
        }

        public SetNetSyncTimeResponse SetNetSyncTime(Howell5198Session session, NetSyncTime netSyncTime)
        {
            Console.WriteLine("客户端请求SetNetSyncTime;Enable_force:{0},Force_interval:{1}", netSyncTime.Enable_force, netSyncTime.Force_interval);
            return new SetNetSyncTimeResponse();
        }


        public ForceIFrameResponse ForceIFrame(Howell5198Session Session, ForceIFrameRequest forceIFrameRequest)
        {
            Console.WriteLine("客户端请求ForceIFrame");
            return new ForceIFrameResponse();
        }
    }
}
