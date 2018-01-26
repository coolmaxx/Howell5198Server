using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    /// 移动侦测
    /// </summary>
    public class Motion : IBytesSerialize
    {
        public Motion()
        {
            this.Slot = 0;
            this.Lev = 0;
            this.RecDelay = 0;
            this.Data = new Int32[18];
        }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        /// 0最高-5最低，6关闭
        /// </summary>
        public Int32 Lev { get; set; }
        /// <summary>
        /// 0-6:10s,20s,30s,1m,2m,5m,10m
        /// </summary>
        public Int32 RecDelay { get; set; }
        /// <summary>
        /// 从低位往高位为0-21
        /// </summary>
        public Int32[] Data { get; set; }

        public int GetLength()
        {
            return 84;
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
            this.Lev = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.RecDelay = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Data.Length; ++i)
            {
                this.Data[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Lev, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.RecDelay, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Data.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Data[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class GetMotionRequest : StreamRequest { }
    public class GetMotionResponse : Motion { }
    public class SetMotionRequest : Motion { }
    public class SetMotionResponse : StreamResponse { }
    public class MotionEx : IBytesSerialize
    {
        public MotionEx()
        {
            this.Slot = 0;
            this.Lev = 0;
            this.RecDelay = 0;
            this.Data = new Byte[2048];
            this.Type = 0;
            this.byRelAlarmOut = new Byte[16];
            this.byRelRecord = new Byte[16];
            this.byRelSnap = new Byte[16];
            this.Reserve = new Int32[32];
        }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        /// 0最高-5最低，6关闭
        /// </summary>
        public Int32 Lev { get; set; }
        /// <summary>
        /// 0-6:10s,20s,30s,1m,2m,5m,10m
        /// </summary>
        public Int32 RecDelay { get; set; }
        /// <summary>
        /// 128 * 128 / 8=2048
        /// </summary>
        public Byte[] Data { get; set; }
        /// <summary>
        /// 处理方式,处理方式的"或"结果,参照AlarmHandleType
        /// </summary>
        public Int32 Type { get; set; }
        public Byte[] byRelAlarmOut { get; set; }
        public Byte[] byRelRecord { get; set; }
        public Byte[] byRelSnap { get; set; }
        public Int32[] Reserve { get; set; }

        public int GetLength()
        {
            return 2240;
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
            this.Lev = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.RecDelay = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Data = LittleEndian.ReadBytes(2048, buffer, ref newOffset, length);
            this.Type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.byRelAlarmOut = LittleEndian.ReadBytes(16, buffer, ref newOffset, length);
            this.byRelRecord = LittleEndian.ReadBytes(16, buffer, ref newOffset, length);
            this.byRelSnap = LittleEndian.ReadBytes(16, buffer, ref newOffset, length);
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
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Slot, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Lev, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.RecDelay, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.Data, 0, this.Data.Length, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Type, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.byRelAlarmOut, 0, this.byRelAlarmOut.Length, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.byRelRecord, 0, this.byRelRecord.Length, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.byRelSnap, 0, this.byRelSnap.Length, buffer, ref offset, buffer.Length);

            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    public class GetMotionExRequest : StreamRequest { }
    public class GetMotionExResponse : MotionEx { }
    public class SetMotionExRequest : MotionEx { }
    public class SetMotionExResponse : StreamResponse { }
}
