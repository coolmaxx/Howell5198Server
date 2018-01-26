using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    ///获取通道名称
    /// </summary>
    public class GetOsdChannelRequest : StreamRequest
    { }
    /// <summary>
    ///通道名称信息
    /// </summary>
    public class OsdChannelInfo : IBytesSerialize
    {
        public OsdChannelInfo()
        {
            this.Slot = 0;
            this.IsEnable = 0;
            this.Left = 0;
            this.Top = 0;
            this.Name = null;
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public Int32 IsEnable { get; set; }
        /// <summary>
        /// 0-703
        /// </summary>
        public Int32 Left { get; set; }
        /// <summary>
        /// 0-575
        /// </summary>
        public Int32 Top { get; set; }
        /// <summary>
        /// 通道名称（最大长度32个单字节）
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 48;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Slot = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.IsEnable = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Left = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Top = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Name = LittleEndian.ReadString(32,buffer, ref newOffset, length);
        }

        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Byte[] Fill = new byte[32];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsEnable, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Left, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Top, buffer, ref offset, buffer.Length);
            Byte[] Name = System.Text.Encoding.ASCII.GetBytes(this.Name);
            if (Name.Length < 32)
            {
                LittleEndian.WriteBytes(Name, 0, Name.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - Name.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(Name, 0, 32, buffer, ref offset, buffer.Length);
            }
            return buffer;
        } 
    }

    /// <summary>
    ///设置通道名称的应答
    /// </summary>
    public class SetOsdChannelResponse : StreamResponse { }

    /// <summary>
    ///获取通道日期
    /// </summary>
    public class GetOsdDateRequest : StreamRequest { }

    /// <summary>
    ///通道日期信息
    /// </summary>
    public class OsdDateInfo : IBytesSerialize
    {
        public OsdDateInfo()
        {
            this.Slot = 0;
            this.IsEnable = 0;
            this.Left = 0;
            this.Top = 0;
            this.Type = 0;
            this.IsShowWeek = 0;
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public Int32 IsEnable { get; set; }
        /// <summary>
        /// 0-703
        /// </summary>
        public Int32 Left { get; set; }
        /// <summary>
        /// 0-575
        /// </summary>
        public Int32 Top { get; set; }
        /// <summary>
        /// 无效
        /// </summary>
        public Int32 Type { get; set; }
        /// <summary>
        /// 是否显示日期
        /// </summary>
        public Int32 IsShowWeek { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 24;
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
            this.Slot = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.IsEnable = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Left = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Top = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.IsShowWeek = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsEnable, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Left, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Top, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Type, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsShowWeek, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    /// <summary>
    ///设置通道日期的应答
    /// </summary>
    public class SetOsdDateResponse : StreamResponse { }

}
