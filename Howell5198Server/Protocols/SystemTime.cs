using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{

    /// <summary>
    /// 获取时间的应答
    /// </summary>
    public class SystemTimeInfo : IBytesSerialize
    {
        public SystemTimeInfo()
        {
            this.WYear = 0;
            this.WMonth = 0;
            this.WDayOfWeek = 0;
            this.WDay = 0;
            this.WHour = 0;
            this.WMinute = 0;
            this.WSecond = 0;
            this.WMilliseconds = 0;
        }
        public SystemTimeInfo(UInt16 year, UInt16 month, UInt16 day, UInt16 hour, UInt16 minute,UInt16 second)
        {
            this.WYear = year;
            this.WMonth = month;
            this.WDay = day;
            this.WHour = hour;
            this.WMinute = minute;
            this.WSecond = second;

            this.WDayOfWeek = 0;
            this.WMilliseconds = 0;
        }
        public SystemTimeInfo(DateTime time)
        {
            this.WYear = (UInt16)time.Year;
            this.WMonth = (UInt16)time.Month;
            this.WDay = (UInt16)time.Day;
            this.WHour = (UInt16)time.Hour;
            this.WMinute = (UInt16)time.Minute;
            this.WSecond = (UInt16)time.Second;

            this.WDayOfWeek = 0;
            this.WMilliseconds = 0;
        }

        /// <summary>
        /// 年
        /// </summary>
        public UInt16 WYear { get; set; }
        /// <summary>
        /// 月
        /// </summary>
        public UInt16 WMonth { get; set; }
        /// <summary>
        /// 星期,0=星期日,1=星期一...
        /// </summary>
        public UInt16 WDayOfWeek { get; set; }
        /// <summary>
        /// 日
        /// </summary>
        public UInt16 WDay { get; set; }
        /// <summary>
        /// 时
        /// </summary>
        public UInt16 WHour { get; set; }
        /// <summary>
        /// 分
        /// </summary>
        public UInt16 WMinute { get; set; }
        /// <summary>
        /// 秒
        /// </summary>
        public UInt16 WSecond { get; set; }
        /// <summary>
        /// 毫秒
        /// </summary>
        public UInt16 WMilliseconds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 16;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.WYear = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
            this.WMonth = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
            this.WDayOfWeek = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
            this.WDay = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
            this.WHour = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
            this.WMinute = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
            this.WSecond = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
            this.WMilliseconds = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteUInt16(this.WYear, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt16(this.WMonth, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt16(this.WDayOfWeek, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt16(this.WDay, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt16(this.WHour, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt16(this.WMinute, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt16(this.WSecond, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt16(this.WMilliseconds, buffer, ref offset, buffer.Length);
            return buffer;
        }
        public override string ToString()
        {
            String timestr = String.Format("{0},{1},{2},{3},{4},{5}", WYear, WMonth, WDay, WHour, WMinute, WSecond);
            return timestr;
        }
    }


    /// <summary>
    /// 设置时间的应答
    /// </summary>
    public class SetSystemTimeResponse : StreamResponse { }

    public class SyncTimeRequest : SystemTimeInfo { }
    public class SyncTimeResponse : StreamResponse { }
    /*
     typedef struct
{
	char svrip[32];//NTP IP
	unsigned int cycletime;//校时间隔 
	unsigned int port;//本地端口，不是NTP服务器端口，设置为0最好
	char timezone[64]; //默认
	int reserved[32];
}ntp_info_t;
     */
    public class NtpInfo : IBytesSerialize
    {
        public NtpInfo()
        {
            this.SvrIp = "";
            this.Cycletime = 0;
            this.Port = 0;
            this.Timezone = new Byte[64];
            NtpServerId = "";
            this.Reserve = new Int32[24];
        }

        /// <summary>
        /// ntp ip地址
        /// </summary>
        public String SvrIp { get; set; }
        /// <summary>
        /// 校时间隔
        /// </summary>
        public UInt32 Cycletime { get; set; }
        /// <summary>
        /// 本地端口，不是NTP服务器端口，设置为0最好
        /// </summary>
        public UInt32 Port { get; set; }
        /// <summary>
        /// 64个字节
        /// </summary>
        public Byte[] Timezone { get; set; }
        /// <summary>
        /// 32个字节
        /// </summary>
        public String NtpServerId { get; set; }
        /// <summary>
        /// 24个Int32的保留信息
        /// </summary>
        public Int32[] Reserve { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 32*4+64+8+32;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.SvrIp = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Cycletime = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.Port = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.Timezone = LittleEndian.ReadBytes(64, buffer, ref newOffset, length);
            this.NtpServerId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Byte[] Fill = new byte[64];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.SvrIp, 32, ref buffer, ref offset);
            LittleEndian.WriteUInt32(this.Cycletime, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.Port, buffer, ref offset, buffer.Length);
            if (this.Timezone.Length <64)
            {
                LittleEndian.WriteBytes(this.Timezone, 0, this.Timezone.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 64 - this.Timezone.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(this.Timezone, 0,64, buffer, ref offset, buffer.Length);
            }
            toolclass.WirteStringToBuf(this.NtpServerId, 32, ref buffer, ref offset);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class SetNtpInfoResponse : StreamResponse { }

    public class GetNetSyncTimeResponse:IBytesSerialize
    {
        public GetNetSyncTimeResponse()
        {
            Remote_Time = 0;
        }
        public Int32 Remote_Time { get; set; }
        public int GetLength()
        {
            return 4;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Remote_Time = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Remote_Time, buffer, ref offset, buffer.Length);

            return buffer;
        }
    }

    public class NetSyncTime: IBytesSerialize
    {
        public NetSyncTime()
        {
            this.Enable_force =0;
            this.Force_interval = 0;
            this.Pre_t = 0;
            this.Reserve = new Byte[32];
        }

        public Int32 Enable_force { get; set; }
        public Int32 Force_interval { get; set; }
        public Int32 Pre_t { get; set; }
        /// <summary>
        /// 32个字节
        /// </summary>
        public Byte[] Reserve { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 44;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Enable_force = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Force_interval = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Pre_t = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Reserve = LittleEndian.ReadBytes(32, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Enable_force, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Force_interval, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Pre_t, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.Reserve, 0,32, buffer, ref offset, buffer.Length);
          
            return buffer;
        }
    }

    public class SetNetSyncTimeResponse : StreamResponse { }
}
