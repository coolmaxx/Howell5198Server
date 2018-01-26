using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    /// 字码流设置
    /// </summary>
    public class SubChannelSet: IBytesSerialize
    {
        public SubChannelSet()
        {
            this.Slot = 0;
            this.Used = 0;
            this.EncodeType = 0;
            this.QualityLev = 0;
            this.MaxBps = 0;
            this.Vbr = 0;
            this.Framerate = 0;
            this.Reserve = new Int32[256];
        }
        public Int32 Slot { get; set; }
        /// <summary>
        /// 是否启用子码流，0-不启用 1-启用,当不启用，下面参数无效
        /// </summary>
        public Int32 Used { get; set; }
        /// <summary>
        /// 0-CIF,1-D1,2-720p 3-1080p,0xff-qcif
        /// </summary>
        public Int32 EncodeType { get; set; }
        /// <summary>
        /// 0-最好,1-次好，2-较好，3一般
        /// </summary>
        public Int32 QualityLev { get; set; }
        /// <summary>
        /// 0-不限,1-较小 2-大 3-较大 4-最大，CIF(2000,400,600,800,1000),D1(4000,800,1000,1400,1600),qcif(600,100,200,300,400)
        /// </summary>
        public Int32 MaxBps { get; set; }
        /// <summary>
        /// 0-cbr 1-vbr
        /// </summary>
        public Int32 Vbr { get; set; }
        /// <summary>
        /// 0-全帧率 1-24 实际帧率
        /// </summary>
        public Int32 Framerate { get; set; }
        /// <summary>
        /// 256个int
        /// </summary>
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 263*4;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Slot = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Used = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.EncodeType = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.QualityLev = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.MaxBps = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Vbr = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Framerate = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
        }
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Used, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.EncodeType, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.QualityLev, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.MaxBps, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Vbr, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Framerate, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class GetSubChannelSetRequest : StreamRequest { }
    public class GetSubChannelSetResponse : SubChannelSet { }
    public class SetSubChannelSetRequest : SubChannelSet { }
    public class SetSubChannelSetResponse : StreamResponse { }

    public class ForceIFrameRequest : StreamRequest { }
    public class ForceIFrameResponse : StreamResponse { }
}
