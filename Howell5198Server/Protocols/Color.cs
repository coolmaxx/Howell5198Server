using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    ///获取色彩
    /// </summary>
    public class GetColorRequest :  StreamRequest
    { }

    /// <summary>
    ///色彩信息
    /// </summary>
    public class ColorInfo : IBytesSerialize
    {
        public ColorInfo()
        {
            this.Slot = 0;
            this.Brightness = 0;
            this.Contrast = 0;
            this.Saturation = 0;
            this.Hue = 0;
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        /// 亮度0-255
        /// </summary>
        public Int32 Brightness { get; set; }
        /// <summary>
        /// 对比度0-127
        /// </summary>
        public Int32 Contrast { get; set; }
        /// <summary>
        /// 饱和度0-127
        /// </summary>
        public Int32 Saturation { get; set; }
        /// <summary>
        /// 色相0-255
        /// </summary>
        public Int32 Hue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 20;
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
            this.Slot = LittleEndian.ReadInt32(buffer, ref newOffset, length );
            this.Brightness = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Contrast = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Saturation = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Hue = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Brightness, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Contrast, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Saturation, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Hue, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    ///// <summary>
    /////设置色彩
    ///// </summary>
    //public class SetColorRequest : GetColorResponse
    //{
    //    public SetColorRequest(GetColorResponse videoColor)
    //    {
    //        Brightness=videoColor.Brightness;
    //        Contrast=videoColor.Contrast;
    //        Hue=videoColor.Hue;
    //        Saturation=videoColor.Saturation;
    //       Slot=videoColor.Slot;
    //    }
    //}

    /// <summary>
    ///设置色彩的应答
    /// </summary>
    public class SetColorResponse : IBytesSerialize
    {
        public SetColorResponse()
        {
            this.Success = 0;
        }
         /// <summary>
        /// 该值为0表明请求成功，否则请求失败
        /// </summary>
        public Int32 Success { get; set; }
       
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
            this.Success = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.Success, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

}
