using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    public enum PlayerState
    {
        UNKNOWN = 0,//未知 
        OPENING = 1,//打开中 
        BUFFERING = 2,//缓存中 
        PLAYING = 3,//播放 
        PAUSED = 4,//暂停 
        STOPED = 5,//停止 
        ENDED = 6,//结束 
        ERROR = 7,//出错
    }
    public class tDecodingUnitList : IBytesSerialize
    {
        public tDecodingUnitList()
        {
            DecodingUnit_count = 1;
            DecodingUnits = new tDecodingUnit[1] { new tDecodingUnit() };
        }
        /// <summary>
        /// 解码单元的数量
        /// </summary>
        public int DecodingUnit_count { get; set; }
        /// <summary>
        /// DecodingUnit_count个连续的tDecodingUnit数据
        /// </summary>
        public tDecodingUnit[] DecodingUnits { get; set; }

        public int GetLength()
        {
            return 4 + DecodingUnit_count * 308;
        }

        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.DecodingUnit_count = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.DecodingUnits = new tDecodingUnit[this.DecodingUnit_count];
            for (int i = 0; i < this.DecodingUnit_count; ++i)
            {
                this.DecodingUnits[i] = new tDecodingUnit();
                this.DecodingUnits[i].FromBytes(buffer, newOffset, length);
                newOffset += this.DecodingUnits[i].GetLength();
            }
        }

        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.DecodingUnit_count, buffer, ref offset, buffer.Length);
            if (DecodingUnit_count>0)
            {
                for (int i = 0; i < this.DecodingUnits.Length; ++i)
                {
                    Array.Copy(this.DecodingUnits[i].GetBytes(), 0, buffer, offset, this.DecodingUnits[i].GetLength());
                    offset += this.DecodingUnits[i].GetLength();
                }
            }
            
            return buffer;
        }
    }
    public class tDecodingUnit : IBytesSerialize
    {
        public tDecodingUnit()
        {
            Id = "";
            Name_enabled = 0;
            Position_enabled = 0;
            Resolution_enabled = 0;
            PanoCameraId_enabled = 0;
            DisplayDeviceId_enabled = 0;
            Name = "";
            Position = new tPosition();
            Resolution = new tResolution();
            PanoCameraId = "";
            DisplayDeviceId = "";
            Reserve = new int[32];
        }
        /// <summary>
        /// 解码单元唯一标识符
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// 是否包含名称,0为false，非0为true
        /// </summary>
        public int Name_enabled { get; set; }
        /// <summary>
        /// 是否包含左上角位置
        /// </summary>
        public int Position_enabled { get; set; }
        /// <summary>
        /// 是否包含分辨率
        /// </summary>
        public int Resolution_enabled { get; set; }
        /// <summary>
        /// 是否包含解码的全景相机ID
        /// </summary>
        public int PanoCameraId_enabled { get; set; }
        /// <summary>
        /// 是否包含解对应的显示设备唯一标识符
        /// </summary>
        public int DisplayDeviceId_enabled { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 左上角位置
        /// </summary>
        public tPosition Position { get; set; }
        /// <summary>
        /// 分辨率
        /// </summary>
        public tResolution Resolution { get; set; }
        /// <summary>
        /// 解码的全景相机ID
        /// </summary>
        public String PanoCameraId { get; set; }
        /// <summary>
        /// 对应的显示设备唯一标识符
        /// </summary>
        public String DisplayDeviceId { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 4 * 5 + 4 * 32 + 16*2  + 4 * 32;//308
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
            this.Id = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Name_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Position_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Resolution_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PanoCameraId_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.DisplayDeviceId_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Name = LittleEndian.ReadString(32, buffer, ref newOffset, length);

            this.Position.FromBytes(buffer, newOffset, length);
            newOffset += this.Position.GetLength();

            this.Resolution.FromBytes(buffer, newOffset, length);
            newOffset += this.Resolution.GetLength();

            this.PanoCameraId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.DisplayDeviceId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
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
            toolclass.WirteStringToBuf(this.Id, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.Name_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Position_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Resolution_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.PanoCameraId_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.DisplayDeviceId_enabled, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.Name, 32, ref buffer, ref offset);
            Array.Copy(this.Position.GetBytes(), 0, buffer, offset, this.Position.GetLength());
            offset += this.Position.GetLength();
            Array.Copy(this.Resolution.GetBytes(), 0, buffer, offset, this.Resolution.GetLength());
            offset += this.Resolution.GetLength();
            toolclass.WirteStringToBuf(this.PanoCameraId, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.DisplayDeviceId, 32, ref buffer, ref offset);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    public class tPosition  : IBytesSerialize
    {
        public tPosition()
        {
            this.X = 0;
            this.Y = 0;
        }
        /// <summary>
        /// X轴坐标，单位：Pixel
        /// </summary>
        public Double X { get; set; }
        /// <summary>
        /// Y轴坐标，单位：Pixel
        /// </summary>
        public Double Y { get; set; }
        public int GetLength()
        {
            return 16;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.X = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.Y = LittleEndian.ReadInt64(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt64(Convert.ToInt64(this.X), buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt64(Convert.ToInt64(this.Y), buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    public class tResolution : IBytesSerialize
    {
        public tResolution ()
        {
            this.Height = 0;
            this.Width = 0;
        }
        /// <summary>
        ///高度，单位：Pixel
        /// </summary>
        public Double Height { get; set; }
        /// <summary>
        /// 宽度，单位：Pixel
        /// </summary>
        public Double Width { get; set; }
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
            this.Height = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.Width = LittleEndian.ReadInt64(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt64(Convert.ToInt64(this.Height), buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt64(Convert.ToInt64(this.Width), buffer, ref offset, buffer.Length);
            return buffer;
        }
    }
    public class tDecodingUnitId : IBytesSerialize
    {
        public tDecodingUnitId()
        {
            DecodingUnitId = "";
        }
        /// <summary>
        /// 解码单元唯一标识符
        /// </summary>
        public String DecodingUnitId { get; set; }
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            return buffer;
        }
    }

    public class tRotatingSpeed:IBytesSerialize
    {
        public tRotatingSpeed()
        {
            DecodingUnitId = "";
            RotatingSpeed = 0;
        }
        /// <summary>
        /// 解码单元唯一标识符
        /// </summary>
        public String DecodingUnitId { get; set; }
        /// <summary>
        /// 速度单位： 度/秒 0：表示不转动， 负值表示逆时针转动
        /// </summary>
        public int RotatingSpeed { get; set; }
        public int GetLength()
        {
            return 36;
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.RotatingSpeed = LittleEndian.ReadInt32( buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.RotatingSpeed,buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    public class SwitchPanoCameraRequest:IBytesSerialize
    {
        public SwitchPanoCameraRequest()
        {
            DecodingUnitId = "";
            PanoCameraId = "";
        }
        /// <summary>
        /// 解码单元ID
        /// </summary>
        public String DecodingUnitId { get; set; }
        /// <summary>
        /// 全景摄像机ID
        /// </summary>
        public String PanoCameraId { get; set; }
        public int GetLength()
        {
            return 64;
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);

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
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.PanoCameraId, 32, ref buffer, ref offset);
            return buffer;
        }
    }

    public class SetViewPointRequest:IBytesSerialize
    {
        public SetViewPointRequest()
        {
            DecodingUnitId = "";
            ViewPoint=new tViewPoint();
        }
        /// <summary>
        /// 解码单元ID
        /// </summary>
        public String DecodingUnitId { get; set; }
        /// <summary>
        /// 全景摄像机视角
        /// </summary>
        public tViewPoint ViewPoint { get; set; }
        public int GetLength()
        {
            return 32+168;
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);

            this.ViewPoint.FromBytes(buffer, newOffset, length);
            newOffset += this.ViewPoint.GetLength();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
             Array.Copy(this.ViewPoint.GetBytes(), 0, buffer, offset, this.ViewPoint.GetLength());
            offset += this.ViewPoint.GetLength();
            return buffer;
        }
    }
    public class SetViewPointFixedRequest:IBytesSerialize
    {
        public SetViewPointFixedRequest()
        {
            DecodingUnitId = "";
            IsFixed = 0;
        }
        /// <summary>
        /// 解码单元ID
        /// </summary>
        public String DecodingUnitId { get; set; }
        /// <summary>
        /// 非零为锁定，零为解锁
        /// </summary>
        public int IsFixed { get; set; }
        public int GetLength()
        {
            return 36;
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.IsFixed = LittleEndian.ReadInt32( buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.IsFixed,buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    public class SetViewPointRowsRequest:IBytesSerialize
    {
        public SetViewPointRowsRequest()
        {
            DecodingUnitId = "";
            Rows = 1;
        }
        /// <summary>
        /// 解码单元ID
        /// </summary>
        public String DecodingUnitId { get; set; }
        /// <summary>
        /// 1：单行显示，2：多行显示 注意：只有视角是360度的才支持Rows=2的模式
        /// </summary>
        public int Rows { get; set; }
        public int GetLength()
        {
            return 36;
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Rows = LittleEndian.ReadInt32( buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.Rows,buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    public class tPlayerStatus:IBytesSerialize
    {
         /// 是否可定位，0为假，非0为真
        /// </summary>
        public int Seekable { get; set; }
        /// <summary>
        /// 播放器状态
        /// </summary>
        public PlayerState State { get; set; }
        /// <summary>
        /// 媒体总时长
        /// </summary>
        public Double Duration { get; set; }
        public int GetLength()
        {
            return 16;
        }

        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
           
            Seekable = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            State = (PlayerState)LittleEndian.ReadInt32(buffer, ref newOffset, length);
            Duration = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            
        }

        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(Seekable, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32((int)State, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt64(Convert.ToInt64(Duration), buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    public class OneByOneRequest : IBytesSerialize
    {
        public OneByOneRequest()
        {
            DecodingUnitId = "";
        }
        /// <summary>
        /// 解码单元唯一标识符
        /// </summary>
        public String DecodingUnitId { get; set; }
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            return buffer;
        }
    }
    public class PauseRequest : IBytesSerialize
    {
        public PauseRequest()
        {
            DecodingUnitId = "";
        }
        /// <summary>
        /// 解码单元唯一标识符
        /// </summary>
        public String DecodingUnitId { get; set; }
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            return buffer;
        }
    }
    public class ResumeRequest : IBytesSerialize
    {
        public ResumeRequest()
        {
            DecodingUnitId = "";
        }
        /// <summary>
        /// 解码单元唯一标识符
        /// </summary>
        public String DecodingUnitId { get; set; }
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            return buffer;
        }
    }

    public class SeekRequest:IBytesSerialize
    {
        public SeekRequest()
        {
            DecodingUnitId = "";
            Position = 0;
        }
        /// <summary>
        /// 解码单元唯一标识符
        /// </summary>
        public String DecodingUnitId { get; set; }
        /// <summary>
        /// 归一化的进度[0,1] 0: 表示文件开始位置 1: 表示文件结束位置
        /// </summary>
        public Double Position { get; set; }
        public int GetLength()
        {
            return 40;
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
            this.DecodingUnitId = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Position = LittleEndian.ReadInt64( buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            toolclass.WirteStringToBuf(this.DecodingUnitId, 32, ref buffer, ref offset);
            LittleEndian.WriteInt64(Convert.ToInt64(this.Position), buffer, ref offset, buffer.Length);
            return buffer;
        }
    }
}
