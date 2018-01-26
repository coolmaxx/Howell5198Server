using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    ///获取码流类型
    /// </summary>
    public class GetStreamTypeRequest : StreamRequest { }

    /// <summary>
    ///码流类型信息
    /// </summary>
    public class StreamTypeInfo : IBytesSerialize
    {
        public StreamTypeInfo()
        {
            this.Slot = 0;
            this.Type = 0;
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        ///1- video, 2-audio,3-both
        /// </summary>
        public Int32 Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 8;
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
            this.Type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Type, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    /// <summary>
    ///设置码流类型的应答
    /// </summary>
    public class SetStreamTypeResponse : StreamResponse { }
}
