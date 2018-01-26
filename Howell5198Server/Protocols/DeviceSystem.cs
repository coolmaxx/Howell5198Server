using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    /// 服务程序版本信息
    /// </summary>
    public class tServiceVersion : IBytesSerialize
    {
        public tServiceVersion()
        {
            this.Version = "";
            this.BuildDate = new DateTime();
            this.Company = "";
            this.Reserve = new Int32[32];
        }
        /// <summary>
        /// 版本号 1.0.1
        /// </summary>
        public String Version { get; set; }
        /// <summary>
        /// 编译时间
        /// </summary>
        public DateTime BuildDate { get; set; }
        /// <summary>
        /// 公司名
        /// </summary>
        public String Company { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 32+8+32+4*32;
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
            this.Version = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            long builddate = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.BuildDate = DateTime.FromBinary(builddate);
            this.Company = LittleEndian.ReadString(32, buffer, ref newOffset, length);
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
            Byte[] Fill = new byte[64];
            Byte[] version = System.Text.Encoding.ASCII.GetBytes(this.Version);
            if (version.Length < 32)
            {
                LittleEndian.WriteBytes(version, 0, version.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - version.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(version, 0, 32, buffer, ref offset, buffer.Length);
            }
            LittleEndian.WriteInt64(this.BuildDate.ToBinary(), buffer, ref offset, buffer.Length);
            Byte[] company = System.Text.Encoding.ASCII.GetBytes(this.Company);
            if (company.Length < 32)
            {
                LittleEndian.WriteBytes(company, 0, company.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - company.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(company, 0, 32, buffer, ref offset, buffer.Length);
            }
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    public class tDeviceInfo : IBytesSerialize
    {
        public tDeviceInfo()
        {
            Id = "";
            Name = "";
            SerialNumber = "";
            Model = "";
            FirmwareVersion = "";
            FirmwareReleasedDate = new DateTime();
            HardwareVersion = "";
            Description_enabled=0;
            Description = "";
            Reserve = new int[32];
        }
        /// <summary>
        /// 设备唯一标识符
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 设备序列号
        /// </summary>
        public String SerialNumber { get; set; }
        /// <summary>
        /// 设备型号
        /// </summary>
        public String Model { get; set; }
        /// <summary>
        /// 固件版本号
        /// </summary>
        public String FirmwareVersion { get; set; }
        /// <summary>
        /// 固件发布日期
        /// </summary>
        public DateTime FirmwareReleasedDate { get; set; }
        /// <summary>
        /// 硬件版本号
        /// </summary>
        public String HardwareVersion { get; set; }
        /// <summary>
        /// 是否包含描述信息,0为false
        /// </summary>
        public int Description_enabled { get; set; }
        /// <summary>
        /// 描述信息
        /// </summary>
        public String Description { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 32*7 + 8 + 4 + 4 * 32;
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
            this.Name = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.SerialNumber = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Model = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.FirmwareVersion = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            long date = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.FirmwareReleasedDate = DateTime.FromBinary(date);
            this.HardwareVersion = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Description_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Description = LittleEndian.ReadString(32, buffer, ref newOffset, length);
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
            toolclass.WirteStringToBuf(this.Id,32,ref buffer,ref offset);
            toolclass.WirteStringToBuf(this.Name, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.SerialNumber, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.Model, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.FirmwareVersion, 32, ref buffer, ref offset);
            LittleEndian.WriteInt64(this.FirmwareReleasedDate.ToBinary(), buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.HardwareVersion, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.Description_enabled, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.Description, 32, ref buffer, ref offset);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class tDeviceStatus : IBytesSerialize
    {
        public tDeviceStatus()
        {
            CurrentTime = new DateTime();
            SystemUpTime = 0;
            CPU_enabled = 0;
            Memory_enabled = 0;
            CPU = new tCPUPerformance();
            Memory = new tMemoryPerformance();
            Reserve = new int[32];
        }   
        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime CurrentTime { get; set; }
        /// <summary>
        /// 系统启动到现在的秒数
        /// </summary>
        public long SystemUpTime { get; set; }
        /// <summary>
        /// 是否包含CPU状态,0为false
        /// </summary>
        public int CPU_enabled { get; set; }
        /// <summary>
        /// 是否包含内存状态,0为false
        /// </summary>
        public int Memory_enabled { get; set; }
        /// <summary>
        /// CPU状态
        /// </summary>
        public tCPUPerformance CPU { get; set; }
        /// <summary>
        /// 内存状态
        /// </summary>
        public tMemoryPerformance Memory { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 8 + 8 + 4 + 4 + CPU.GetLength() + Memory .GetLength()+ 4 * 32;
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
            long date = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.CurrentTime = DateTime.FromBinary(date);
            this.SystemUpTime = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.CPU_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Memory_enabled = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            CPU.FromBytes(buffer, newOffset, length);
            newOffset += CPU.GetLength();
            Memory.FromBytes(buffer, newOffset, length);
            newOffset += Memory.GetLength();
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
            LittleEndian.WriteInt64(this.CurrentTime.ToBinary(), buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt64(this.SystemUpTime, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.CPU_enabled, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Memory_enabled, buffer, ref offset, buffer.Length);
            Array.Copy(this.CPU.GetBytes(), 0, buffer, offset, this.CPU.GetLength());
            offset += this.CPU.GetLength();
            Array.Copy(this.Memory.GetBytes(), 0, buffer, offset, this.CPU.GetLength());
            offset += this.Memory.GetLength();
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class tCPUPerformance : IBytesSerialize
    {
        public tCPUPerformance()
        {
            this.Name = "";
            this.Utilization =0;
            this.Reserve = new Int32[32];
        }
        /// <summary>
        /// 名称&描述
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 使用百分比 [0,100]
        /// </summary>
        public Double Utilization { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 32 + 8  + 4 * 32;
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
            this.Name = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Utilization = LittleEndian.ReadInt64(buffer, ref newOffset, length);
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
            toolclass.WirteStringToBuf(this.Name, 32, ref buffer, ref offset);
            LittleEndian.WriteInt64((long)this.Utilization, buffer, ref offset, buffer.Length);         
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class tMemoryPerformance : IBytesSerialize
    {
        public tMemoryPerformance()
        {
            this.Usage =0;
            this.TotalSize =0;
            this.Description = "";
            this.Reserve = new Int32[32];
        }
        /// <summary>
        /// 使用百分比[0,100]
        /// </summary>
        public Double Usage { get; set; }
        /// <summary>
        /// 内存大小，单位：MB
        /// </summary>
        public Double TotalSize { get; set; }
        /// <summary>
        /// 描述信息
        /// </summary>
        public String Description { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 8 + 8 + 32 + 4 * 32;
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
            this.Usage = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.TotalSize = LittleEndian.ReadInt64(buffer, ref newOffset, length);
            this.Description = LittleEndian.ReadString(32, buffer, ref newOffset, length);
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

            LittleEndian.WriteInt64((long)this.Usage, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt64((long)this.TotalSize, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.Description, 32, ref buffer, ref offset);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    class toolclass
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strvalue">要写入buf的字符串</param>
        /// <param name="bytesize">协议中给字符串预留的空间长度</param>
        /// <param name="targetbuf">要写入的buf</param>
        /// <param name="offset">偏移量</param>
        public static void WirteStringToBuf(String strvalue, int bytesize, ref Byte[] targetbuf,ref int offset)
        {
            Byte[] Fill = new byte[bytesize];
            Byte[] bytevalue = System.Text.Encoding.ASCII.GetBytes(strvalue);
            if (bytevalue.Length < bytesize)
            {
                LittleEndian.WriteBytes(bytevalue, 0, bytevalue.Length, targetbuf, ref offset, targetbuf.Length);
                LittleEndian.WriteBytes(Fill, 0, bytesize - bytevalue.Length, targetbuf, ref offset, targetbuf.Length);
            }
            else
            {
                LittleEndian.WriteBytes(bytevalue, 0, bytesize, targetbuf, ref offset, targetbuf.Length);
            }
        }
    }
}
