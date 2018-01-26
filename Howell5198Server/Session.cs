using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Extensions.Protocol;
using Howell.Security;
using Howell5198.Protocols;

namespace Howell5198
{
    public class Howell5198Session : Session<SessionContext>
    {
        public Howell5198Session(FixedHeaderProtocolSession<ProtocolHeader> protocolSession, Int32 timeout, System.Net.IPEndPoint remoteEndPoint, SessionContext context)
            : base(protocolSession.SessionID, timeout, remoteEndPoint, context)
        {
            this.ProtocolSession = protocolSession;
        }
        /// <summary>
        /// 协议会话信息
        /// </summary>
        public FixedHeaderProtocolSession<ProtocolHeader> ProtocolSession { get; private set; }

        public Boolean IsConnected()
        {
            return ProtocolSession.Connected;
        }
        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="command"></param>
        /// <param name="payload"></param>
        public void Send(UInt32 errornum, UInt32 command, byte[] payload)
        {
            if (payload==null)
            {
                ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = errornum, dataLen = 0 }, null));
            }
            else
            {
                ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = errornum, dataLen = (uint)payload.Length }, payload));
            }
            
        }
        /// <summary>
        /// 尝试发送数据包
        /// </summary>
        /// <param name="command"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public Boolean TrySend(UInt32 errornum, UInt32 command, byte[] payload)
        {
            if (payload == null)
            {
                return ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = errornum, dataLen = 0 }, null));
            }
            else
            {
                return ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = errornum, dataLen = (uint)payload.Length }, payload));
            }
        }
        //private UInt32 m_CSeq = 0;
        ///// <summary>
        ///// 获取序号
        ///// </summary>
        ///// <returns>应答返回序号</returns>
        //public UInt32 GetCSeq()
        //{
        //    lock (this)
        //    {
        //        return ++m_CSeq;
        //    }
        //}
    }

    /// <summary>
    /// 会话上下文对象
    /// </summary>
    public class SessionContext
    {
        /// <summary>
        /// 会话上下文对象
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="isStreamSession">是否为流的会话</param>
        /// <param name="isStreamSession">会话ID</param>
        public SessionContext(String userName, String password,String sessionID)
        {
            this.UserName = userName;
            this.Password = password;
            this.SessionID = sessionID;
        }
        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName { get; private set; }
        /// <summary>
        /// 密码
        /// </summary>
        public String Password { get; private set; }

        /// <summary>
        /// 会话ID
        /// </summary>
        public String SessionID { get; private set; }
    }

    public class StreamSession : Session<StreamSessionContext>
    {
        public StreamSession(FixedHeaderProtocolSession<ProtocolHeader> protocolSession, Int32 timeout, System.Net.IPEndPoint remoteEndPoint, StreamSessionContext context)
            : base(protocolSession.SessionID, timeout, remoteEndPoint, context)
        {
            this.ProtocolSession = protocolSession;
        }
        /// <summary>
        /// 协议会话信息
        /// </summary>
        public FixedHeaderProtocolSession<ProtocolHeader> ProtocolSession { get; private set; }

        public Boolean IsConnected()
        {
            return ProtocolSession.Connected;
        }
        /// <summary>
        /// 发送数据包
        /// </summary>
        public void Send(FramePayload framePayload)
        {          
            byte[] payload = framePayload.GetBytes();
            uint command = COMMAND.Main_stream;
            if (Context.Type==1)
                command = COMMAND.Sub_stream;
            ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = 0, dataLen = (uint)payload.Length }, payload));
        }
        /// <summary>
        /// 尝试发送数据包
        /// </summary>
        /// <returns></returns>
        public Boolean TrySend(FramePayload framePayload)
        {
            uint command = COMMAND.Main_stream;
            if (Context.Type == 1)
            {
                command = COMMAND.Sub_stream;
                if (framePayload.FrameType == FramePayload.frametype.HW_FRAME_VIDEO_P)
                    framePayload.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_P;
                else if (framePayload.FrameType == FramePayload.frametype.HW_FRAME_VIDEO_I)
                    framePayload.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_I;
                else if (framePayload.FrameType == FramePayload.frametype.HW_FRAME_VIDEO_B)
                    framePayload.FrameType = FramePayload.frametype.HW_FRAME_VIDEO_SUB_BP;
            }
                
            if (framePayload.FrameData == null)
            {
                return ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = 1, dataLen = 0 }, null));
            }
            else
            {
                byte[] payload = framePayload.GetBytes();
                return ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = 0, dataLen = (uint)payload.Length }, payload));
            }
           
        }

        /// <summary>
        /// 发送回放数据包
        /// </summary>
        public void SendFile(GetFileResponse file)
        {
            try
            {
                byte[] payload = file.GetBytes();
                uint command = COMMAND.GetFile;
                ProtocolSession.Send(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = 0, dataLen = (uint)payload.Length }, payload));
            }
            catch
            { }
           
        }

        /// <summary>
        /// 尝试发送回放数据包
        /// </summary>
        public bool TrySendFile(GetFileResponse file)
        {
            byte[] payload = file.GetBytes();
            uint command = COMMAND.GetFile;
            return ProtocolSession.TrySend(new FixedHeaderPackageInfo<ProtocolHeader>(Guid.NewGuid().ToString("N"), new ProtocolHeader() { proType = command, errornum = 0, dataLen = (uint)payload.Length }, payload));
        }
        /// <summary>
        /// 主动关闭session
        /// </summary>
        public void Close()
        {
            ProtocolSession.Close();
        }
    }

    public class StreamSessionContext
    {
        /// <summary>
        /// 会话上下文对象
        /// </summary>
        /// <param name="sessionID">唯一标识</param>
        /// <param name="channelNo">通道号</param>
        /// <param name="type">流类型，0:主码流 1:子码流 2:回放流</param>
        public StreamSessionContext(String sessionID, Int32 channelNo, Int32 type)
        {
            this.SessionID = sessionID;
            this.ChannelNo = channelNo;
            this.Type = type;
        }
        /// <summary>
        /// 会话ID
        /// </summary>
        public String SessionID { get; private set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; }
        /// <summary>
        /// 0:主码流 1:子码流  2:回放文件流
        /// </summary>
        public Int32 Type { get; set; }
    }
}
