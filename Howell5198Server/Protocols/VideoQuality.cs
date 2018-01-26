using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    ///获取图像质量
    /// </summary>
    public class GetVideoQualityRequest : StreamRequest { }
    /// <summary>
    ///图像质量信息
    /// </summary>
    public class VideoQualityInfo : IBytesSerialize
    {
        public VideoQualityInfo()
        {
            this.Slot = 0;
            this.EncodeType = 0;
            this.QualityLev = 0;
            this.MaxBps = 0;
            this.Vbr = 0;
            this.Framerate = 0;
            this.Reserved = 0;
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        /// 0-CIF, 1-D1 2-720p
        /// </summary>
        public Int32 EncodeType { get; set; }
        /// <summary>
        /// 0-最好,1-次好，2-较好，3一般
        /// </summary>
        public Int32 QualityLev { get; set; }
        /// <summary>
        /// 0-不限,1-较小 2-大 3-较大 4-最大，CIF(2000,400,600,800,1000),D1(4000,800,1000,1400,1600),720p(8000,2000,2500,3000,4000)
        /// </summary>
        public Int32 MaxBps { get; set; }
        /// <summary>
        /// /0-cbr 1-vbr
        /// </summary>
        public Int32 Vbr { get; set; }
        /// <summary>
        /// 0-全帧率 1-24 实际帧率
        /// </summary>
        public Int32 Framerate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 Reserved { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 28;
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
            this.EncodeType = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.QualityLev = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.MaxBps = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Vbr = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Framerate = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Reserved = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.EncodeType, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.QualityLev, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.MaxBps, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Vbr, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Framerate, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Reserved, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }


    /// <summary>
    ///设置图像质量的应答
    /// </summary>
    public class SetVideoQualityResponse :  StreamResponse { }

}
