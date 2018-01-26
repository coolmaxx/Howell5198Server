using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    public class RegisterAlarmRequest : IBytesSerialize
    {
        public RegisterAlarmRequest()
        {
            this.bRegister = 0;
            this.listenPort = 0;
            this.Reserve = new Int32[4];
        }
        public Int32 bRegister { get; set; }
        public Int32 listenPort { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 24;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.bRegister = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.listenPort = LittleEndian.ReadInt32(buffer, ref newOffset, length); 
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
        }
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.bRegister, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.listenPort, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    public class RegisterAlarmResponse : StreamResponse { }
    public class AlarmData : IBytesSerialize
    {
        public AlarmData()
        {
            this.Value = 0;
            this.Status = 0;
            this.Reserve = new Int32[32];
        }
        public Int32 Value { get; set; }
        public Int32 Status { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 136;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Value = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Status = LittleEndian.ReadInt32(buffer, ref newOffset, length); 
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
        }
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Value, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Status, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }

    }
}
