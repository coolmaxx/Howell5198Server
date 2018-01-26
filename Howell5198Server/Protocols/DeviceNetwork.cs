using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    public class tNetworkInterface : IBytesSerialize
    {
        public enum NetworkInterfaceWorkMode
        {
            Disable=0,//禁用
            Enable=1,//启用
            Bridge=2,//桥接或对等
            Balancing=3//负载均衡
        };
        public tNetworkInterface()
        {
            Id = "";
            Name = "";
            Address = "";
            SubnetMask = "";
            SpeedDuplex = 0;
            WorkMode = 0;
            MTU = 0;
            PhysicalAddress = "";
            this.Reserve = new Int32[32];
        }
        /// <summary>
        /// 网口ID
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// 网口名称
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// IPV4地址
        /// </summary>
        public String Address { get; set; }
        /// <summary>
        /// 子网掩码
        /// </summary>
        public String SubnetMask { get; set; }
        /// <summary>
        /// 网口速率 M/bps
        /// </summary>
        public int SpeedDuplex { get; set; }
        //网口工作模式
        public NetworkInterfaceWorkMode WorkMode { get; set; }
        /// <summary>
        /// 最大传输单元长度
        /// </summary>
        public int MTU { get; set; }
        /// <summary>
        /// MAC地址，00-00-00-00-00-00
        /// </summary>
        public String PhysicalAddress { get; set; }
        public Int32[] Reserve { get; set; }
        public int GetLength()
        {
            return 32 * 5 + 4*3 + 4 * 32;
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
            this.Address = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.SubnetMask = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.SpeedDuplex=LittleEndian.ReadInt32(buffer,ref newOffset,length);
            this.WorkMode = (NetworkInterfaceWorkMode)LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.MTU = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PhysicalAddress = LittleEndian.ReadString(32, buffer, ref newOffset, length);
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
            toolclass.WirteStringToBuf(this.Name, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.Address, 32, ref buffer, ref offset);
            toolclass.WirteStringToBuf(this.SubnetMask, 32, ref buffer, ref offset);
            LittleEndian.WriteInt32(this.SpeedDuplex, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32((int)this.WorkMode, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.MTU, buffer, ref offset, buffer.Length);
            toolclass.WirteStringToBuf(this.PhysicalAddress, 32, ref buffer, ref offset);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
}
