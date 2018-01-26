using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    public class RecFile : IBytesSerialize
    {
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public SystemTimeInfo Beg{ get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public SystemTimeInfo End{ get; set; }
        /// <summary>
        ///0=all 1=normal file 2=mot file 3=alarm 4=mot alarm 5=ipcam lost 6=analyze metadata
        /// </summary>
        public Int32 Type { get; set; }
        public RecFile()
        {
            this.ChannelNo = 0;
            this.Beg = new SystemTimeInfo();
            this.End = new SystemTimeInfo();
            this.Type = 0;
        }
        public int GetLength()
        {
            return 8 + this.Beg.GetLength() + this.End.GetLength();
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
            this.Beg.FromBytes(buffer, newOffset, length);
            newOffset += this.Beg.GetLength();
            this.End.FromBytes(buffer, newOffset, length);
            newOffset += this.End.GetLength();
            this.Type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.ChannelNo, buffer, ref offset, buffer.Length);
            Array.Copy(this.Beg.GetBytes(), 0, buffer, offset, this.Beg.GetLength());
            offset += this.Beg.GetLength();
            Array.Copy(this.End.GetBytes(), 0, buffer, offset, this.End.GetLength());
            offset += this.End.GetLength();
            LittleEndian.WriteInt32(this.Type, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    /// <summary>
    /// 搜索文件的请求
    /// </summary>
    public class SearchFileRequest :RecFile
    {

    }
    /// <summary>
    /// 搜索文件的应答
    /// </summary>
     public class SearchFileResponse : IBytesSerialize
     {
        // List<SearchFileRequest> FileInfos;
         public SearchFileRequest[] FileInfos;
         public SearchFileResponse()
         {
             FileInfos = null;
         }
         public int GetLength()
         {
             int length = 0;
             foreach(var fi in FileInfos)
             {
                 length += fi.GetLength();
             }
             return length;
         }
         public void FromBytes(byte[] buffer, int offset, int length)
         {
             Int32 newOffset = offset;
             SearchFileRequest fileinfo = new SearchFileRequest();
             Int32 count = length / fileinfo.GetLength();
             FileInfos = new SearchFileRequest[count];
             for (int i = 0; i < count; i++)
             {
                 SearchFileRequest fileinfo2 = new SearchFileRequest();
                 fileinfo2.FromBytes(buffer, newOffset, length);
                 newOffset += fileinfo2.GetLength();
                 FileInfos[i] = fileinfo2;
             }
         }

         public byte[] GetBytes()
         {
             Byte[] buffer = new byte[this.GetLength()];
             Int32 offset = 0;
             for (int i = 0; i < FileInfos.Length;i++ )
             {
                 Array.Copy(FileInfos[i].GetBytes(), 0, buffer, offset, FileInfos[i].GetLength());
                 offset += FileInfos[i].GetLength();
             }
             return buffer;
         }

     }
    /// <summary>
     /// 获取文件的请求
    /// </summary>
     public class GetFileRequest : RecFile
    {
    }
    /// <summary>
    /// 获取文件的应答
    /// </summary>
     public class GetFileResponse : IBytesSerialize
     {
         /// <summary>
         /// 通道号
         /// </summary>
         public Int32 ChannelNo { get; set; }
         /// <summary>
         /// 0=head  1=video data
         /// </summary>
         public Int32 Type { get; set; }
         /// <summary>
         /// 数据长度
         /// </summary>
         public Int32 Datalen { get; set; }
         /// <summary>
         /// 录像文件数据
         /// </summary>
         public Byte[] Buffer { get; set; }
         public GetFileResponse()
         {
             this.ChannelNo = 0;
             this.Type = 0;
             this.Datalen = 0;
             Buffer = new Byte[2048];
         }
         public int GetLength()
         {
             return 12+2048;
         }
         public void FromBytes(byte[] buffer, int offset, int length)
         {
             Int32 newOffset = offset;
             this.ChannelNo = LittleEndian.ReadInt32(buffer, ref newOffset, length);
             this.Type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
             this.Datalen = LittleEndian.ReadInt32(buffer, ref newOffset, length);
             this.Buffer = LittleEndian.ReadBytes(length - newOffset, buffer, ref newOffset, length);
         }
         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         public byte[] GetBytes()
         {
             Byte[] buffer = new byte[this.GetLength()];
             Int32 offset = 0;
             LittleEndian.WriteInt32(this.ChannelNo, buffer, ref offset, buffer.Length);
             LittleEndian.WriteInt32(this.Type, buffer, ref offset, buffer.Length);
             LittleEndian.WriteInt32(this.Datalen, buffer, ref offset, buffer.Length);
             LittleEndian.WriteBytes(this.Buffer, 0, this.Buffer.Length, buffer, ref offset, buffer.Length);
             return buffer;
         }
     }

     public class GetFileInfo : IBytesSerialize
     {
         /// <summary>
         /// 通道号
         /// </summary>
         public Int32 ChannelNo { get; set; }
         /// <summary>
         /// 开始时间
         /// </summary>
         public SystemTimeInfo Beg { get; set; }
         /// <summary>
         /// 结束时间
         /// </summary>
         public SystemTimeInfo End { get; set; }
         /// <summary>
         /// 文件编码格式类型，参见HW_DEVICE_TYPE
         /// </summary>
         public Int32 FileFormatType { get; set; }
         public Int32 Video_dec { get; set; }
         public Int32 Audio_dec { get; set; }
         /// <summary>
         /// 30个Int32的保留信息
         /// </summary>
         public Int32[] Reserved { get; set; }
         public GetFileInfo()
        {
            this.ChannelNo = 0;
            this.Beg = new SystemTimeInfo();
            this.End = new SystemTimeInfo();
            this.FileFormatType = 0;
            this.Video_dec = 0;
            this.Audio_dec = 0;
            this.Reserved = new Int32[30];
        }
        public int GetLength()
        {
            return 16+120 + this.Beg.GetLength() + this.End.GetLength();
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
            this.Beg.FromBytes(buffer, newOffset, length);
            newOffset += this.Beg.GetLength();
            this.End.FromBytes(buffer, newOffset, length);
            newOffset += this.End.GetLength();
            this.FileFormatType = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Video_dec = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Audio_dec = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserved.Length; ++i)
            {
                this.Reserved[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.ChannelNo, buffer, ref offset, buffer.Length);
            Array.Copy(this.Beg.GetBytes(), 0, buffer, offset, this.Beg.GetLength());
            offset += this.Beg.GetLength();
            Array.Copy(this.End.GetBytes(), 0, buffer, offset, this.End.GetLength());
            offset += this.End.GetLength();
            LittleEndian.WriteInt32(this.FileFormatType, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Video_dec, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Audio_dec, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserved.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserved[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
     }
    public class GetFileInfoRequest:GetFileInfo
    {

    }
    public class GetFileInfoResponse:GetFileInfo
    {
    }
    public class GetNetHead:IBytesSerialize
    {
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; }
        /// <summary>
        /// 是否为子码流
        /// </summary>
        public Int32 IsSub { get; set; }
        /// <summary>
        /// 保存返回HW头的BUFFER
        /// </summary>
        public Byte[] Buf { get; set; }
        /// <summary>
        /// Buf中有效数据的长度
        /// </summary>
        public Int32 Len { get; set; }
        public GetNetHead()
        {
            this.ChannelNo = 0;
            this.IsSub = 0;
            this.Buf = new Byte[128];
            this.Len = 0;
        }
        public int GetLength()
        {
            return 12 + 128;
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
            this.IsSub = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Buf = LittleEndian.ReadBytes(128, buffer, ref newOffset, length);
            this.Len = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.ChannelNo, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsSub, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.Buf, 0, 128, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Len, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }
      public class GetNetHeadRequest:GetNetHead
      {

      }
    public class GetNetHeadResponse:GetNetHead
    {

    }

    public class TimeLabel:IBytesSerialize
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public Int32 Beg { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public Int32 End { get; set; }
        /// <summary>
        /// 录像文件的长度
        /// </summary>
        public Int32 FileLen { get; set; }
        public Int32 RecordType { get; set; }
        public TimeLabel()
        {
            this.Beg = 0;
            this.End = 0;
            this.FileLen =0;
            this.RecordType = 0;
        }
        public int GetLength()
        {
            return 12;
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
            this.Beg = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.End = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.FileLen = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.RecordType = LittleEndian.ReadInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Beg, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.End, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.FileLen, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.RecordType, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

}
/*
 //下载文件信息
typedef struct  
{		
int beg_tm;
int end_tm;
unsigned int file_len;
int record_type;		
}time_label;

typedef struct  
{
int slot;
int is_sub;
char buf[128];
int len;
}net_head_t;
 */