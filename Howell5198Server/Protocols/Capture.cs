using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;


namespace Howell5198.Protocols
{
    /*
     truct tCaptureJpeg{
	int slot; //通道
	int stream;//0:主码流  1:子码流，当前只支持子码流
	int type;//0:JPG 1:BMP,当前只支持JPEG
	int quality;//5-100
	char reserve[16];//保留，必须为0
}
     */
    public class CaptureRequest : IBytesSerialize
    {
        public CaptureRequest()
        {
            this.Slot = 0;
            this.Stream = 1;
            this.Type = 0;
            this.Quality = 50;
            this.Reserve = new Int32[16];
        }

        /// <summary>
        /// 通道
        /// </summary>
        public int Slot { get; set; }
        /// <summary>
        /// 0:主码流  1:子码流，当前只支持子码流
        /// </summary>
        public int Stream { get; set; }
        /// <summary>
        /// 0:JPG 1:BMP,当前只支持JPEG
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 5-100
        /// </summary>
        public int Quality { get; set; }
        /// <summary>
        /// 16个Int32的保留信息
        /// </summary>
        public Int32[] Reserve { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 16+4*16;
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
            this.Stream = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Quality = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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

            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Stream, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Type, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Quality, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    public class CapturenResponse: IBytesSerialize
    {
          public CapturenResponse()
        {
            this.PicData = null;
        }
        /// <summary>
        /// jpeg格式的图片buf
        /// </summary>
        public Byte[] PicData { get; set; }
        public int GetLength()
        {
            return  this.PicData.Length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void FromBytes(byte[] buffer, int offset, int length)//length为一个(帧类型 + 帧数据)数据包的长度
        {
            Int32 newOffset = offset;
            this.PicData = LittleEndian.ReadBytes(length, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteBytes(this.PicData, 0, this.PicData.Length, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }
}
