using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    public class tQueryString : IBytesSerialize
    {
        public tQueryString()
        {
            PageIndex_enabled = 0;
            PageSize_enabled = 0;
            PageIndex = 0;
            PageSize = 0;
            Reserve = new int[32];
        }
        /// <summary>
        /// 是否包含页码
        /// </summary>
        public int PageIndex_enabled { get; set; }
        /// <summary>
        /// 是否包含分页大小
        /// </summary>
        public int PageSize_enabled { get; set; }
        /// <summary>
        /// 页码[1-n]
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 分页大小[1-100]
        /// </summary>
        public int PageSize { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 16+32*4;
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
            this.PageIndex_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PageSize_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PageIndex = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PageSize = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.PageIndex_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.PageSize_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.PageIndex, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.PageSize, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class tPanoCameraList:IBytesSerialize
    {
        public tPanoCameraList()
        {
            Page_enabled = 0;
            Page = new tPage();
            PanoCamera_count = 1;
            PanoCameras = new tPanoCamera[1] { new tPanoCamera() };
            Reserve = new int[32];
        }
        /// <summary>
        /// 是否包含分页信息，如果没有分页信息，则表示一次性获取所有的结果
        /// </summary>
        public int Page_enabled { get; set; }
        /// <summary>
        /// 分页信息
        /// </summary>
        public tPage Page { get; set; }
        /// <summary>
        /// 全景摄像机的数量
        /// </summary>
        public int PanoCamera_count { get; set; }
        public tPanoCamera[] PanoCameras { get; set; }
        public Int32[] Reserve { get; set; }

        public int GetLength()
        {
            int len=8+148+32*4;
            for(int i=0;i<PanoCamera_count;++i)
            {
                len+=PanoCameras[i].GetLength();
            }
            return len;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Page_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Page.FromBytes(buffer, newOffset, length);
            newOffset += this.Page.GetLength();
            this.PanoCamera_count = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PanoCameras = new tPanoCamera[this.PanoCamera_count];
            for (int i = 0; i < this.PanoCamera_count; ++i)
            {
                this.PanoCameras[i] = new tPanoCamera();
                this.PanoCameras[i].FromBytes(buffer, newOffset, length);
                newOffset += this.PanoCameras[i].GetLength();
            }
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
        }
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Page_enabled, buffer, ref offset, buffer.Length);
            Array.Copy(this.Page.GetBytes(), 0, buffer, offset, this.Page.GetLength());
            offset += this.Page.GetLength();
            LittleEndian.WriteInt32(this.PanoCamera_count, buffer, ref offset, buffer.Length);
            if (this.PanoCamera_count>0)
            {
                for (int i = 0; i < this.PanoCameras.Length; ++i)
                {
                    Array.Copy(this.PanoCameras[i].GetBytes(), 0, buffer, offset, this.PanoCameras[i].GetLength());
                    offset += this.PanoCameras[i].GetLength();
                }
            }
            
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class tPage:IBytesSerialize
    {
        public tPage()
        {
            PageIndex = 0;
            PageSize = 0;
            PageCount = 0;
            RecordCount = 0;
            TotalRecordCount = 0;
            Reserve = new int[32];
        }
        /// <summary>
        /// 页码 1.2.3 …..
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 当前页的记录数目
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// 总记录数目
        /// </summary>
        public int TotalRecordCount { get; set; }
        public Int32[] Reserve { get; set; }

        public int GetLength()
        {
            return 20+4*32;//148
        }

        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.PageIndex = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PageSize = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PageCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.RecordCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.TotalRecordCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
        }

        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.PageIndex, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.PageSize, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.PageCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.RecordCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.TotalRecordCount, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    
    public class tPanoCamera : IBytesSerialize
    {
        public tPanoCamera()
        {
            Id_enabled = 0;
            Id = "";
            Name = "";
            Model = "";
            ExistedInDatabase_enabled = 0;
            ExistedInDatabase = 0;
            GroupId = "";
            WiperCameraBuildInId = 0;
            MainCameraBuildInId = 0;
            BuildInCamera_count = 1;
            BuildInCameras = new tBuildInCamera[1]{new tBuildInCamera()};
            DefaultViewPoint_enabled = 0;
            DefaultViewPoint = new tViewPoint();
            IsAllOnline=0;
            IsCompleted = 0;
            Reserve = new int[32];
        }
        /// <summary>
        /// 是否包含全景摄像机唯一标识符
        /// </summary>
        public int Id_enabled { get; set; }
        /// <summary>
        /// 全景摄像机唯一标识符
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 型号
        /// </summary>
        public String Model { get; set; }
        /// <summary>
        /// 是否包含ExistedInDatabase
        /// </summary>
        public int ExistedInDatabase_enabled { get; set; }
        /// <summary>
        /// 是否已存在与数据库中
        /// </summary>
        public int ExistedInDatabase { get; set; }
        /// <summary>
        /// 全景摄像机分组ID
        /// </summary>
        public String GroupId { get; set; }
        /// <summary>
        /// 雨刷对应的内置摄像机ID，如果不支持雨刷，该值为0
        /// </summary>
        public int WiperCameraBuildInId { get; set; }
        /// <summary>
        /// 主摄像机对应的内置ID，如果没有主摄像机，该值为0
        /// </summary>
        public int MainCameraBuildInId { get; set; }
        /// <summary>
        /// 内置摄像机信息个数
        /// </summary>
        public int BuildInCamera_count { get; set; }
        /// <summary>
        /// BuildInCamera_count个连续tBuildInCamera
        /// </summary>
        public tBuildInCamera[] BuildInCameras { get; set; }
        public int DefaultViewPoint_enabled { get; set; }
        /// <summary>
        /// 默认视点
        /// </summary>
        public tViewPoint DefaultViewPoint { get; set; }
        /// <summary>
        /// 是否所有内建摄像机都在线,只在Id_enabled为true时有效
        /// </summary>
        public int IsAllOnline { get; set; }
        /// <summary>
        /// 内置信息是否完整,只在Id_enabled为true时有效
        /// </summary>
        public int IsCompleted { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 4*9+4*32+BuildInCamera_count*436+168 + 4 * 32;//436
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
            this.Id_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Id =  LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Name = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Model = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.ExistedInDatabase_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.ExistedInDatabase = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.GroupId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.WiperCameraBuildInId = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.MainCameraBuildInId = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.BuildInCamera_count = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.BuildInCameras = new tBuildInCamera[this.BuildInCamera_count];
            for (int i = 0; i < this.BuildInCamera_count; ++i)
            {
                this.BuildInCameras[i]=new tBuildInCamera();
                this.BuildInCameras[i].FromBytes(buffer, newOffset, length);
                newOffset += this.BuildInCameras[i].GetLength();
            }
            this.DefaultViewPoint_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.DefaultViewPoint.FromBytes(buffer, newOffset, length);
            newOffset += this.DefaultViewPoint.GetLength();
            this.IsAllOnline = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.IsCompleted = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.Id_enabled, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.Id, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.Name, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.Model, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.ExistedInDatabase_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.ExistedInDatabase, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.GroupId, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.WiperCameraBuildInId, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.MainCameraBuildInId, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.BuildInCamera_count, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.BuildInCameras.Length;++i )
            {
                Array.Copy(this.BuildInCameras[i].GetBytes(), 0, buffer, offset, this.BuildInCameras[i].GetLength());
                offset += this.BuildInCameras[i].GetLength();
            }
            LittleEndian.WriteInt32(this.DefaultViewPoint_enabled, buffer, ref offset, buffer.Length);
            Array.Copy(this.DefaultViewPoint.GetBytes(), 0, buffer, offset, this.DefaultViewPoint.GetLength());
            offset += this.DefaultViewPoint.GetLength();
            LittleEndian.WriteInt32(this.IsAllOnline, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsCompleted, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    /*
     struct tBuildInCamera{
int Id_enabled;//是否包含唯一标识符
char Id[32];//唯一标识符
char Name[32];//设备名称
char ProtocolType[32];//协议类型，Howell5198
char Uri[64];//设备访问地址
int BuildInId;//内置ID [1,n]
int IsMainCamera;//是否为主摄像机,只在Id_enabled为true时有效
int IsFishEyeCamera;//是否为鱼眼摄像机,只在Id_enabled为true时有效
int IsOnline;//是否在线,只在Id_enabled为true时有效
int reserver[64];
      }
     */
    public class tBuildInCamera: IBytesSerialize
    {
        public tBuildInCamera()
        {
            Id_enabled = 0;
            Id = "";
            Name = "";
            ProtocolType = "";
            Uri = "";
            BuildInId = 0;
            IsMainCamera = 0;
            IsFishEyeCamera = 0;
            IsOnline = 0;
            Reserve = new int[64];
        }
        /// <summary>
        /// 是否包含唯一标识符
        /// </summary>
        public int Id_enabled { get; set; }
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 协议类型，Howell5198
        /// </summary>
        public String ProtocolType { get; set; }
        /// <summary>
        /// 设备访问地址
        /// </summary>
        public String Uri { get; set; }
        /// <summary>
        /// 内置ID [1,n]
        /// </summary>
        public int BuildInId { get; set; }
        /// <summary>
        /// 是否为主摄像机,只在Id_enabled为true时有效
        /// </summary>
        public int IsMainCamera { get; set; }
        /// <summary>
        /// 是否为鱼眼摄像机,只在Id_enabled为true时有效
        /// </summary>
        public int IsFishEyeCamera { get; set; }
        /// <summary>
        /// 是否在线,只在Id_enabled为true时有效
        /// </summary>
        public int IsOnline { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 32 * 5 + 4 * 5 + 4 * 64;//436
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
            this.Id_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Id = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Name = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.ProtocolType = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Uri = LittleEndian.ReadString(64, buffer, ref newOffset, length);
            this.BuildInId = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.IsMainCamera = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.IsFishEyeCamera = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.IsOnline = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.Id_enabled, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.Id, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.Name, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.ProtocolType, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.Uri, 64, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.BuildInId, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsMainCamera, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsFishEyeCamera, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.IsOnline, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    /*
     struct tViewPoint {
double angle;//水平方面的旋转角度，0°位于摄像机组的中间位置
double distance;//距离画面的距离，与放大率成反比，1表示单视频全屏显示
double height;//垂直方向的偏移，+1表示图像的上边缘，-1表示图像的下边缘
int Fixed_enabled;
int Rows_enabled;
int Fixed;//是否默认固定视角，默认：false
int Rows;//视频分行显示，视频行数[1,2],默认：1
int reserver[32];
}
     */
    public class tViewPoint : IBytesSerialize
    {
        public tViewPoint()
        {
            Angle = 0;
            Distance = 0;
            Height = 0;
            Fixed_enabled = 0;
            Rows_enabled = 0;
            Fixed = 0;
            Rows = 0;
            Reserve = new int[32];
        }
        /// <summary>
        /// 水平方面的旋转角度，0°位于摄像机组的中间位置，比如:3摄像机，0°位于摄像机2正中;6摄像机，0°位于摄像机3与4相接处，摄像机1正中是-150°，摄像机4正中是30°
        /// </summary>
        public Double Angle { get; set; }
        /// <summary>
        /// 距离画面的距离，与放大率成反比，1表示单视频全屏显示
        /// </summary>
        public Double Distance { get; set; }
        /// <summary>
        /// 垂直方向的偏移，+1表示图像的上边缘，-1表示图像的下边缘
        /// </summary>
        public Double Height { get; set; }
        public int Fixed_enabled { get; set; }
        public int Rows_enabled { get; set; }
        /// <summary>
        /// 是否默认固定视角，默认：false
        /// </summary>
        public int Fixed { get; set; }
        /// <summary>
        /// 视频分行显示，视频行数[1,2],默认：1
        /// </summary>
        public int Rows { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 24+16+32*4;//168
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
            this.Angle = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.Distance = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.Height = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.Fixed_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Rows_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Fixed = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Rows = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt64(Convert.ToInt64(this.Angle), buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt64(Convert.ToInt64(this.Distance), buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt64(Convert.ToInt64(this.Height), buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Fixed_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Rows_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Fixed, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Rows, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    public class tFault: IBytesSerialize
    {
        public tFault()
        {
            this.FaultCode = 0;
            this.FaultReason = "";
            Reserve = new int[32];
        }
        /// <summary>
        /// 错误码
        /// </summary>
        public int FaultCode { get; set; }
        /// <summary>
        /// 错误原因
        /// </summary>
        public String FaultReason { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 4+32+32*4;
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
            this.FaultCode = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.FaultReason = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.FaultCode, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.FaultReason, 32, ref buffer, ref offset);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    public class tPanoCameraId: IBytesSerialize
    {
        public tPanoCameraId()
        {
            PanoCameraId="";
        }
        public tPanoCameraId(String panoCameraId)
        {
            PanoCameraId = panoCameraId;
        }
        /// <summary>
        /// 全景摄像机唯一标识符
        /// </summary>
        public String PanoCameraId { get; set; }
        public int GetLength()
        {
            return 32;
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
            this.PanoCameraId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.PanoCameraId, 32, ref buffer, ref offset);
            return buffer;
        }
    }
}
