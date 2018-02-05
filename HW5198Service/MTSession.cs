using Howell.eCamera.Middlewares;
using Howell.Industry;
using Howell.Net.DataService.Video;
using Howell5198;
using Howell5198.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HW5198Service
{
    public class MTSession
    {
        /// <summary>
        /// 流转发会话
        /// </summary>
        public MTSession(MediaStreamIdentifier mediaStreamIdentifier)
        {
            VideoClient = null;
            MediaStreamSessions = new List<MediaStreamSession>();
            DeviceID = null;
            ChannelNo = 0;
            StreamIdentifier = mediaStreamIdentifier;
        }
        /// <summary>
        /// 视频源对象
        /// </summary>
        public MTClient VideoClient { get; private set; }
        public List<MediaStreamSession> MediaStreamSessions { get; private set; }
        /// <summary>
        /// 通过伪码解析出的设备ID
        /// </summary>
        public String DeviceID { get; private set; }
        /// <summary>
        /// 通过伪码解析出的通道号
        /// </summary>
        public int ChannelNo { get; private set; }
        /// <summary>
        /// 媒体流唯一标识符
        /// </summary>
        public MediaStreamIdentifier StreamIdentifier { get;private set; }
        private readonly Object VideoClient_lock = new Object();
        private readonly ReaderWriterLockSlim MTSession_lock = new ReaderWriterLockSlim();
        const int timeout = 30000;//connect时的超时时间
        public void Close()
        {
           
            if (VideoClient != null)
            {
                VideoClient.Dispose();
                VideoClient = null;
            }  

            MTSession_lock.EnterWriteLock();
            try
            {
                foreach (MediaStreamSession stream in MediaStreamSessions)
                {
                    if (stream.IsConnected())
                    {
                        stream.Close();
                    }

                }
                MediaStreamSessions.Clear();
            }
            finally
            {
                MTSession_lock.ExitWriteLock();
            }

        }
        public void AddSession(MediaStreamSession MediaStreamSession)
        {
            MTSession_lock.EnterWriteLock();
            try
            {
                MediaStreamSessions.Add(MediaStreamSession);
            }
            finally
            {
                MTSession_lock.ExitWriteLock();
            }

        }

        public void RemoveSession(MediaStreamSession MediaStreamSession)
        {
            MTSession_lock.EnterWriteLock();
            try
            {
                if (MediaStreamSessions.Contains(MediaStreamSession))
                {
                    MediaStreamSessions.Remove(MediaStreamSession);
                }
            }
            finally
            {
                MTSession_lock.ExitWriteLock();
            }
        }

        public void Connect(MediaStreamSession session)
        {
            lock (VideoClient_lock)
            {
                if (VideoClient != null)
                {
                    if (VideoClient.IsConnected)
                    {
                        return;
                    }
                    else
                    {
                        VideoClient.Dispose();
                        VideoClient = null;
                    }
                }
                try
                {
                    VideoInputChannel pseudodev = Howell5198ServerAppInstance.CreatNewDataManagementClient().GetVideoInputChannelByPseudoCode(Convert.ToString(session.Context.StreamIdentifier.ChannelNo + 1));
                    VideoClient = new MTClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ServiceConfiguration.Instance.MTServerIP), ServiceConfiguration.Instance.MTServerPort));
                    VideoClient.Credential = new MTClientCredential() { UserName = ServiceConfiguration.Instance.UserName, Password = ServiceConfiguration.Instance.Password, MobileTerminalId = session.Context.SessionID };
                    VideoClient.Timeout = timeout;
                    VideoClient.Connect();
                    Identity Channelid = Identity.Parse(pseudodev.Id);
                    DeviceID = Channelid.GetDeviceIdentity().ToString();
                    ChannelNo = Channelid.ModuleNumber - 1;
                }
                catch (Exception ex)
                {
                    if (VideoClient != null)
                    {
                        VideoClient.Dispose();
                        VideoClient = null;
                    }
                    Console.WriteLine("MTSession.Connect时." + ex.Message);
                    ServiceEnvironment.Instance.Logger.Warn("MTSession.Connect时.", ex);
                }
            }
        }
        public void Subscribe(MediaStreamSession Session)
        {
            var solt = Session.Context.StreamIdentifier;
            lock (VideoClient_lock)
            {
                try
                {
                    if (VideoClient == null)
                        return;
                    if (VideoClient.IsSubscribed)
                    {
                        AddSession(Session);
                    }
                    else
                    {
                        VideoClient.MediaDataReceived += new MTClientMediaDataReceivedHandler(MediaDataReceived);
                        uint dialogid=(uint)((solt.StreamNo == 0) ? solt.ChannelNo : (solt.ChannelNo+10000));
                        VideoClient.Subscribe(dialogid, DeviceID, ChannelNo, solt.StreamNo);
                        AddSession(Session);
                        Console.WriteLine(String.Format("通道{0}的类型{1}预览流已建立", solt.ChannelNo, solt.StreamNo));
                        ServiceEnvironment.Instance.Logger.Info(String.Format("通道{0}的类型{1}预览流已建立", solt.ChannelNo, solt.StreamNo));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("VideoClient Subscribe Error.ChannelNo:{0}", solt.ChannelNo));
                    ServiceEnvironment.Instance.Logger.Error(String.Format("VideoClient Subscribe Error.ChannelNO:{0}", solt.ChannelNo), ex);
                }
            }
        }
        private void MediaStreamSessionClose(object livesession)
        {
            ((MediaStreamSession)livesession).Close();
        }


        void MediaDataReceived(MTClient sender, MediaData mediaData)
        {
            //int handle = (int)mediaData.DialogId;
            Byte[] framedate = new Byte[mediaData.FrameData.Count];
            int frametype = 0;
            Buffer.BlockCopy(mediaData.FrameData.Array, mediaData.FrameData.Offset, framedate, 0, mediaData.FrameData.Count);

            if (mediaData.FrameType == FrameType.PFrame)
                frametype = 4;
            else if (mediaData.FrameType == FrameType.IFrame)
                frametype = 3;
            else if (mediaData.FrameType == FrameType.BFrame)
                frametype = 5;
            else if (mediaData.FrameType == FrameType.Audio)
                frametype = 2;
            else
            {
                Console.WriteLine("frametype error");
                ServiceEnvironment.Instance.Logger.Error("frametype error");
                return;
            }
            MTSession_lock.EnterReadLock();
            MediaStreamSession[] sessions = MediaStreamSessions.ToArray();
  
            MTSession_lock.ExitReadLock();
            try
            {
                foreach (MediaStreamSession livesession in sessions)
                {
                    if (livesession.IsConnected())
                    {
                        bool sendout = livesession.TrySend(frametype, framedate, 100, 10);
                        if(sendout == false)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(MediaStreamSessionClose), livesession);
                            Console.WriteLine("预览流数据发送超时，主动关闭session");
                            ServiceEnvironment.Instance.Logger.Info("预览流数据发送超时，主动关闭session");
                            return;
                        }
                    }
                    else
                        ThreadPool.QueueUserWorkItem(new WaitCallback(MediaStreamSessionClose), livesession);
                }
            }
            finally
            {
            }
        }

        
    }
}
