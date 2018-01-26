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
        public MTSession()
        {
            VideoClient = null;
            StreamSessions = new List<StreamSession>();
            DeviceID = null;
            ChannelNo = 0;
        }
        public MTClient VideoClient { get; set; }
        public List<StreamSession> StreamSessions { get; set; }
        public String DeviceID { get; set; }
        public int ChannelNo { get; set; }
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
                foreach (StreamSession stream in StreamSessions)
                {
                    if (stream.IsConnected())
                    {
                        stream.Close();
                    }

                }
                StreamSessions.Clear();
            }
            finally
            {
                MTSession_lock.ExitWriteLock();
            }

        }
        public void AddSession(StreamSession streamsession)
        {
            MTSession_lock.EnterWriteLock();
            try
            {
                StreamSessions.Add(streamsession);
            }
            finally
            {
                MTSession_lock.ExitWriteLock();
            }

        }

        public void RemoveSession(StreamSession streamsession)
        {
            MTSession_lock.EnterWriteLock();
            try
            {
                if (StreamSessions.Contains(streamsession))
                {
                    StreamSessions.Remove(streamsession);
                }
            }
            finally
            {
                MTSession_lock.ExitWriteLock();
            }
        }

        public void Connect(StreamSession session)
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
                    VideoInputChannel pseudodev = Howell5198ServerAppInstance.CreatNewDataManagementClient().GetVideoInputChannelByPseudoCode(Convert.ToString(session.Context.ChannelNo + 1));
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
        public void Subscribe(StreamSession Session)
        {
            int solt = Session.Context.ChannelNo;
            if (Session.Context.Type == 1)
            {
                solt += 10000;
            }
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
                        VideoClient.Subscribe((uint)solt, DeviceID, ChannelNo, Session.Context.Type);
                        AddSession(Session);
                        Console.WriteLine(String.Format("通道{0}的类型{1}预览流已建立", Session.Context.ChannelNo, Session.Context.Type));
                        ServiceEnvironment.Instance.Logger.Info(String.Format("通道{0}的类型{1}预览流已建立", Session.Context.ChannelNo, Session.Context.Type));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("VideoClient Subscribe Error.ChannelNo:{0}", Session.Context.ChannelNo));
                    ServiceEnvironment.Instance.Logger.Error(String.Format("VideoClient Subscribe Error.ChannelNO:{0}", Session.Context.ChannelNo), ex);
                }
            }
        }
        void MediaDataReceived(MTClient sender, MediaData mediaData)
        {
            //int handle = (int)mediaData.DialogId;
            Byte[] framedate = new Byte[mediaData.FrameData.Count];
            Buffer.BlockCopy(mediaData.FrameData.Array, mediaData.FrameData.Offset, framedate, 0, mediaData.FrameData.Count);

            FramePayload framepayload = new FramePayload();
            if (mediaData.FrameType == FrameType.PFrame)
                framepayload.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_P;
            else if (mediaData.FrameType == FrameType.IFrame)
                framepayload.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_I;
            else if (mediaData.FrameType == FrameType.BFrame)
                framepayload.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_B;
            else if (mediaData.FrameType == FrameType.Audio)
                framepayload.FrameType = FramePayload.frametype.HW_FRAME_AUDIO;
            else if (mediaData.FrameType == FrameType.MJpeg)
                framepayload.FrameType = FramePayload.frametype.HW_FRAME_MOTION_FRAME;
            else
            {
                Console.WriteLine("frametype error");
                ServiceEnvironment.Instance.Logger.Error("frametype error");
                return;
            }
            framepayload.FrameData = framedate;
            MTSession_lock.EnterReadLock();
            try
            {
                foreach (StreamSession livesession in StreamSessions)
                {
                    if (livesession.IsConnected())
                    {
                        bool sendout = livesession.TrySend(framepayload);
                        int count = 0;
                        while (sendout == false)
                        {
                            Thread.Sleep(10);
                            sendout = livesession.TrySend(framepayload);
                            if (livesession.IsConnected() == false)
                                break;
                            count++;
                            if (count > 200)
                            {
                                livesession.Close();
                                Console.WriteLine("预览流数据发送超时，主动关闭session");
                                ServiceEnvironment.Instance.Logger.Info("预览流数据发送超时，主动关闭session");
                                return;
                            }
                        }
                    }
                    else
                        livesession.Close();
                }
            }
            finally
            {
                MTSession_lock.ExitReadLock();
            }
        }
    }
}
