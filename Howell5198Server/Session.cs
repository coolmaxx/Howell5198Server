using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Extensions.Protocol;
using Howell.Security;
using Howell5198.Protocols;

namespace Howell5198
{
    /// <summary>
    /// 媒体流会话
    /// </summary>
    public class MediaStreamSession : Session<MediaStreamSessionContext>
    {
        public MediaStreamSession(FixedHeaderProtocolSession<ProtocolHeader> protocolSession, Int32 timeout, System.Net.IPEndPoint remoteEndPoint, MediaStreamSessionContext context)
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
        /// 发送媒体数据
        /// </summary>
        /// <param name="frameType">媒体帧类型：1-流头结构，2-音频帧，3-I帧，4-P帧，5-BP帧</param>
        /// <param name="payload">载荷数据</param>
        public void Send(Int32 frameType, Byte[] payload)
        {
            if (payload == null || payload.Length == 0) return;
            //录像视频
            if (this.Context.IsFileStream == true)
            {
                GetFileResponse p = new GetFileResponse();
                if (frameType == 1) p.Type = 0;
                else p.Type = 1;

                p.Buffer = payload;
                p.ChannelNo = this.Context.StreamIdentifier.ChannelNo;
                p.Datalen = payload.Length;
                p.Buffer = payload;
                Byte[] data = p.GetBytes();
                ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                    new ProtocolHeader() { proType = ProtocolType.GetFile, errornum = 0, dataLen = (UInt32)data.Length }, data));
            }
            //实时视频
            else
            {
                FramePayload p = new FramePayload();
                p.FrameData = payload;
                //实时主码流
                if (this.Context.StreamIdentifier.StreamNo == 0)
                {
                    if (frameType == 1) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_HEAD;
                    else if (frameType == 2) p.FrameType = FramePayload.frametype.HW_FRAME_AUDIO;
                    else if (frameType == 3) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_I;
                    else if (frameType == 4) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_P;
                    else if (frameType == 5) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_BP;
                    else return;
                    Byte[] data = p.GetBytes();
                    ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                        new ProtocolHeader() { proType = ProtocolType.Main_stream, errornum = 0, dataLen = (UInt32)data.Length }, data));
                }
                else
                {
                    //实时子码流
                    if (frameType == 1) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_HEAD;
                    else if (frameType == 2) p.FrameType = FramePayload.frametype.HW_FRAME_AUDIO;
                    else if (frameType == 3) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_I;
                    else if (frameType == 4) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_P;
                    else if (frameType == 5) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_BP;
                    else return;
                    Byte[] data = p.GetBytes();
                    ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                      new ProtocolHeader() { proType = ProtocolType.Sub_stream, errornum = 0, dataLen = (UInt32)data.Length }, data));
                }
            }

        }

        /// <summary>
        /// 发送媒体数据
        /// </summary>
        /// <param name="frameType">媒体帧类型：1-流头结构，2-音频帧，3-I帧，4-P帧，5-BP帧</param>
        /// <param name="payload">载荷数据</param>
        /// <param name="tryTimes">尝试重新发送的次数</param>
        /// <param name="tryInterval">尝试重新发送的间隔，单位毫秒</param>
        /// <returns></returns>
        public Boolean TrySend(Int32 frameType, Byte[] payload, Int32 tryTimes = 0, Int32 tryInterval = 0)
        {
            if (payload == null || payload.Length == 0) return false;
           
            //录像视频
            if (this.Context.IsFileStream == true)
            {
                GetFileResponse p = new GetFileResponse();
                if (frameType == 1) p.Type = 0;
                else p.Type = 1;
                p.Buffer = payload;
                p.ChannelNo = this.Context.StreamIdentifier.ChannelNo;
                p.Datalen = payload.Length;
                p.Buffer = payload;
                Byte[] data = p.GetBytes();
                int i = 0;
                do
                {
                    if(ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                    new ProtocolHeader() { proType = ProtocolType.GetFile, errornum = 0, dataLen = (UInt32)data.Length }, data)))
                    {
                        return true;
                    }
                    ++i;
                    System.Threading.Thread.Sleep(tryInterval);
                } while (i < tryTimes);
                return false;
            }
            //实时视频
            else
            {
                FramePayload p = new FramePayload();
                p.FrameData = payload;
                //实时主码流
                if (this.Context.StreamIdentifier.StreamNo == 0)
                {
                    if (frameType == 1) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_HEAD;
                    else if (frameType == 2) p.FrameType = FramePayload.frametype.HW_FRAME_AUDIO;
                    else if (frameType == 3) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_I;
                    else if (frameType == 4) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_P;
                    else if (frameType == 5) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_BP;
                    else return false;
                    Byte[] data = p.GetBytes();
                    int i = 0;
                    do
                    {
                        if (ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                        new ProtocolHeader() { proType = ProtocolType.Main_stream, errornum = 0, dataLen = (UInt32)data.Length }, data)))
                        {
                            return true;
                        }
                        ++i;
                        System.Threading.Thread.Sleep(tryInterval);
                    } while (i < tryTimes);
                    return false;
                }
                //实时子码流
                else
                {
                    if (frameType == 1) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_HEAD;
                    else if (frameType == 2) p.FrameType = FramePayload.frametype.HW_FRAME_AUDIO;
                    else if (frameType == 3) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_I;
                    else if (frameType == 4) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_P;
                    else if (frameType == 5) p.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_BP;
                    else return false;
                    Byte[] data = p.GetBytes();
                    int i = 0;
                    do
                    {
                        if (ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"),
                      new ProtocolHeader() { proType = ProtocolType.Sub_stream, errornum = 0, dataLen = (UInt32)data.Length }, data)))
                        {
                            return true;
                        }
                        ++i;
                        System.Threading.Thread.Sleep(tryInterval);
                    } while (i < tryTimes);
                    return false;
                }
            }
        }

        /// <summary>
        /// 主动关闭session
        /// </summary>
        public void Close()
        {
            ProtocolSession.Close();
        }
    }

    /// <summary>
    /// 媒体流会话上下文
    /// </summary>
    public class MediaStreamSessionContext
    {
        /// <summary>
        /// 媒体流会话上下文对象
        /// </summary>
        /// <param name="sessionID">会话ID</param>
        public MediaStreamSessionContext(String sessionID, MediaStreamIdentifier streamIdentifier)
            : this(sessionID, streamIdentifier, false, null, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID">会话ID</param>
        /// <param name="streamIdentifier">码流唯一标识符</param>
        /// <param name="isFileStream">是否为文件流</param>
        /// <param name="beginTime">文件开始时间</param>
        /// <param name="endTime">文件结束时间</param>
        public MediaStreamSessionContext(String sessionID, MediaStreamIdentifier streamIdentifier, Boolean isFileStream, Nullable<DateTime> beginTime, Nullable<DateTime> endTime)
        {
            this.SessionID = sessionID;
            this.StreamIdentifier = streamIdentifier;
            this.IsFileStream = isFileStream;
            this.BeginTime = beginTime;
            this.EndTime = endTime;
        }
        /// <summary>
        /// 会话ID
        /// </summary>
        public String SessionID { get; private set; }
        /// <summary>
        /// 码流唯一标识符
        /// </summary>
        public MediaStreamIdentifier StreamIdentifier { get; private set; }
        /// <summary>
        /// 是否为文件流
        /// </summary>
        public Boolean IsFileStream { get; private set; }
        /// <summary>
        /// 文件开始时间
        /// </summary>
        public Nullable<DateTime> BeginTime { get; private set; }
        /// <summary>
        /// 文件结束时间
        /// </summary>
        public Nullable<DateTime> EndTime { get; private set; }
    }
    /// <summary>
    /// 媒体流唯一标识符
    /// </summary>
    public class MediaStreamIdentifier
    {
        /// <summary>
        /// 媒体流唯一标识符
        /// </summary>
        /// <param name="channelNo">通道编号0-n</param>
        /// <param name="streamNo">码流编号 0-主码流，1-子码流...</param>
        public MediaStreamIdentifier(Int32 channelNo, Int32 streamNo)
        {
            this.ChannelNo = channelNo;
            this.StreamNo = streamNo;
        }
        /// <summary>
        /// 通道编号0-n
        /// </summary>
        public Int32 ChannelNo { get; private set; }
        /// <summary>
        /// 码流编号 0-主码流，1-子码流...
        /// </summary>
        public Int32 StreamNo { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}_{1}", this.ChannelNo, this.StreamNo);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj.GetHashCode() == this.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        /// <summary>
        /// ==
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(MediaStreamIdentifier left, MediaStreamIdentifier right)
        {
            if (left as MediaStreamIdentifier == null && right as MediaStreamIdentifier == null) return true;
            if (left as MediaStreamIdentifier == null || right as MediaStreamIdentifier == null) return false;
            return left.GetHashCode() == right.GetHashCode();
        }
        /// <summary>
        /// !=
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(MediaStreamIdentifier left, MediaStreamIdentifier right)
        {
            return !(left == right);
        }

    }
}
