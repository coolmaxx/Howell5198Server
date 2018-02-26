using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    public class StreamRequest : IBytesSerialize
    {
        public StreamRequest()
        {
            this.ChannelNo = 0;
        }
        public StreamRequest(int channelNo)
        {
            this.ChannelNo = channelNo;
        }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
            this.ChannelNo = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte [] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.ChannelNo, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    public class StreamResponse : IBytesSerialize
    {
        public StreamResponse()
        {
            this.Success = 0;
        }
        public StreamResponse(int success)
        {
            this.Success = success;
        }
        /// <summary>
        /// 该值为0表明请求成功，如果为-1表明请求失败。如果请求成功，服务器会不停的发送协议头+帧类型+帧数据
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
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Success, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    /// <summary>
    ///帧类型 + 帧数据
    /// </summary>
    public class FramePayload : IBytesSerialize
    {
        public enum frametype
        {
            HW_FRAME_VIDEO_HEAD = 1,
            HW_FRAME_VIDEO_I = 3,
            HW_FRAME_VIDEO_P = 4,
            HW_FRAME_VIDEO_BP = 5,
            HW_FRAME_AUDIO = 2,
            HW_FRAME_VIDEO_SUB_HEAD = 7,
            HW_FRAME_VIDEO_SUB_I = 8,
            HW_FRAME_VIDEO_SUB_P = 9,
            HW_FRAME_VIDEO_SUB_BP = 10,
            HW_FRAME_MOTION_FRAME = 13,
            HW_FRAME_VIDEO_B = 14,
            HW_FRAME_VIDEO_SUB_B = 15,
            HW_FRAME_VIDEO_MJPEG=16,
            HW_FRAME_VIDEO_SUB_MJPEG=17
        };
        public FramePayload()
        {
            this.FrameType = 0;
            this.FrameData = null;

        }
        //public FramePayload(int framelen)
        //{
        //    this.FrameType = 0;
        //    this.FrameData = new Byte[framelen];

        //}

        /// <summary>
        /// 帧类型
        /// </summary>
        public frametype FrameType { get; set; }
        /// <summary>
        /// 帧数据
        /// </summary>
        public Byte[] FrameData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return (4 + this.FrameData.Length);
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
            this.FrameType = (frametype)LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.FrameData = LittleEndian.ReadBytes(length - newOffset, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte [] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32((int)this.FrameType, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.FrameData, 0, this.FrameData.Length, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }
    /// <summary>
    /// HW头
    /// </summary>
      public class HW_MediaInfo : IBytesSerialize
      {
          /// <summary>
          /// "HKMI": 0x484B4D49 Hikvision Media Information,"HWMI":0x48574D49
          /// </summary>
          public UInt32 Media_fourcc { get; set; }
          public Int32 Dvr_version { get; set; }
          public Int32 Vdec_code { get; set; }
          public Int32 Adec_code { get; set; }
          /// <summary>
          /// 8,16...
          /// </summary>
          public Byte Au_bits { get; set; }
          /// <summary>
          /// Kbps 8,16,64
          /// </summary>
          public Byte Au_sample { get; set; }
          /// <summary>
          /// 1,2
          /// </summary>
          public Byte Au_channel { get; set; }
          /// <summary>
          /// 版本，如果为1,必须正确填写w,h,fr
          /// </summary>
          public Byte Hd_ver { get; set; }
          /// <summary>
          /// 实际帧率
          /// </summary>
          public Byte Fr { get; set; }
          public Byte[] Reserve { get; set; }

          public UInt16 Height { get; set; }
          public UInt16 Width { get; set; }
          public Int32[] Reserved { get; set; }
          public HW_MediaInfo()
          {
              Media_fourcc = 0x48574D49;
              Dvr_version = 0;
              Vdec_code = 0;
              Adec_code = 1;
              Au_bits = 0;
              Au_sample =8;
              Au_channel = 1;
              Hd_ver = 0;
              Fr = 0;
              Reserve = new Byte[3];
              Height = 0;
              Width = 0;
              Reserved = new Int32[3];
          }
          public int GetLength()
          {
              return 40;
          }

          public void FromBytes(byte[] buffer, int offset, int length)//length为一个(帧类型 + 帧数据)数据包的长度
          {
              Int32 newOffset = offset;
              this.Media_fourcc = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
              this.Dvr_version = LittleEndian.ReadInt32(buffer, ref newOffset, length);
              this.Vdec_code = LittleEndian.ReadInt32(buffer, ref newOffset, length);
              this.Adec_code = LittleEndian.ReadInt32(buffer, ref newOffset, length);
              this.Au_bits = LittleEndian.ReadByte(buffer, ref newOffset, length);
              this.Au_sample = LittleEndian.ReadByte(buffer, ref newOffset, length);
              this.Au_channel = LittleEndian.ReadByte(buffer, ref newOffset, length);
              this.Hd_ver = LittleEndian.ReadByte(buffer, ref newOffset, length);
              this.Fr = LittleEndian.ReadByte(buffer, ref newOffset, length);
              this.Reserve = LittleEndian.ReadBytes(3, buffer, ref newOffset, length);
              this.Height = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
              this.Width = LittleEndian.ReadUInt16(buffer, ref newOffset, length);
              for (int i = 0; i < this.Reserved.Length; ++i)
              {
                  this.Reserved[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
              }
          }
          public byte[] GetBytes()
          {
              Byte[] buffer = new byte[this.GetLength()];
              Int32 offset = 0;
              LittleEndian.WriteUInt32(this.Media_fourcc, buffer, ref offset, buffer.Length);
              LittleEndian.WriteInt32(this.Dvr_version, buffer, ref offset, buffer.Length);
              LittleEndian.WriteInt32(this.Vdec_code, buffer, ref offset, buffer.Length);
              LittleEndian.WriteInt32(this.Adec_code, buffer, ref offset, buffer.Length);
              LittleEndian.WriteByte(this.Au_bits, buffer, ref offset, buffer.Length);
              LittleEndian.WriteByte(this.Au_sample, buffer, ref offset, buffer.Length);
              LittleEndian.WriteByte(this.Au_channel, buffer, ref offset, buffer.Length);
              LittleEndian.WriteByte(this.Hd_ver, buffer, ref offset, buffer.Length);
              LittleEndian.WriteByte(this.Fr, buffer, ref offset, buffer.Length);
              LittleEndian.WriteBytes(this.Reserve,0,3, buffer, ref offset, buffer.Length);
              LittleEndian.WriteUInt16(this.Height, buffer, ref offset, buffer.Length);
              LittleEndian.WriteUInt16(this.Width, buffer, ref offset, buffer.Length);
              for (int i = 0; i < this.Reserved.Length; ++i)
              {
                  LittleEndian.WriteInt32(this.Reserved[i], buffer, ref offset, buffer.Length);
              }
              return buffer;
          }
      }
}
