using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    /// 网络信息
    /// </summary>
    public class NetInfo : IBytesSerialize
    {
        public NetInfo()
        {
            this.Port = 0;
            this.SDvrIp = null;
            this.SDvrMaskIp = null;
            this.Gateway = null;
            this.ByMACAddr = new Int32[6];
            this.Dns = null;
            this.SMultiCastIP = null;
            this.DwPPPOE = 0;
            this.SPPPoEUser = null;
            this.SPPPoEPassword = null;
            this.SPPPoEIP = null;
            this.Reserve = new Int32[32];
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 Port { get; set; }
        /// <summary>
        /// dvr ip地址
        /// </summary>
        public String SDvrIp { get; set; }
        /// <summary>
        /// dvr 子网掩码
        /// </summary>
        public String SDvrMaskIp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String Gateway { get; set; }
        /// <summary>
        /// 服务器的物理地址,6个Int32
        /// </summary>
        public Int32[] ByMACAddr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String Dns { get; set; }
        /// <summary>
        /// 多播组地址
        /// </summary>
        public String SMultiCastIP { get; set; }
        /// <summary>
        /// 0-不启用,1-启用
        /// </summary>
        public Int32 DwPPPOE { get; set; }
        /// <summary>
        /// /PPPoE用户名
        /// </summary>
        public String SPPPoEUser { get; set; }
        /// <summary>
        /// /PPPoE密码,16个char
        /// </summary>
        public String SPPPoEPassword { get; set; }
        /// <summary>
        /// PPPoE IP地址(只读)
        /// </summary>
        public String SPPPoEIP { get; set; }
        /// <summary>
        /// 32个Int32的保留信息
        /// </summary>
        public Int32[] Reserve { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 400;
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
            this.Port = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.SDvrIp = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.SDvrMaskIp = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Gateway = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            for (int i = 0; i < this.ByMACAddr.Length; ++i)
            {
                this.ByMACAddr[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
            this.Dns =  LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.SMultiCastIP = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.DwPPPOE = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.SPPPoEUser = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.SPPPoEPassword = LittleEndian.ReadString(16, buffer, ref newOffset, length);
            this.SPPPoEIP = LittleEndian.ReadString(32, buffer, ref newOffset, length);
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
            Byte[] Fill = new byte[32];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Port, buffer, ref offset, buffer.Length);
            Byte[] sDvrIp = System.Text.Encoding.ASCII.GetBytes(this.SDvrIp);
            if (sDvrIp.Length < 32)
            {
                LittleEndian.WriteBytes(sDvrIp, 0, sDvrIp.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - sDvrIp.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(sDvrIp, 0, 32, buffer, ref offset, buffer.Length);
            }
            Byte[] sDvrMaskIp = System.Text.Encoding.ASCII.GetBytes(this.SDvrMaskIp);
            if (sDvrMaskIp.Length < 32)
            {
                LittleEndian.WriteBytes(sDvrMaskIp, 0, sDvrMaskIp.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - sDvrMaskIp.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(sDvrMaskIp, 0, 32, buffer, ref offset, buffer.Length);
            }
            Byte[] gateway = System.Text.Encoding.ASCII.GetBytes(this.Gateway);
            if (gateway.Length < 32)
            {
                LittleEndian.WriteBytes(gateway, 0, gateway.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - gateway.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(gateway, 0, 32, buffer, ref offset, buffer.Length);
            }
            for (int i = 0; i < this.ByMACAddr.Length; ++i)
            {
                LittleEndian.WriteInt32(this.ByMACAddr[i], buffer, ref offset, buffer.Length);
            }
            Byte[] dns = System.Text.Encoding.ASCII.GetBytes(this.Dns);
            if (dns.Length < 32)
            {
                LittleEndian.WriteBytes(dns, 0, dns.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - dns.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(dns, 0, 32, buffer, ref offset, buffer.Length);
            }
            Byte[] sMultiCastIP = System.Text.Encoding.ASCII.GetBytes(this.SMultiCastIP);
            if (sMultiCastIP.Length < 32)
            {
                LittleEndian.WriteBytes(sMultiCastIP, 0, sMultiCastIP.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - sMultiCastIP.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(sMultiCastIP, 0, 32, buffer, ref offset, buffer.Length);
            }
            LittleEndian.WriteInt32(this.DwPPPOE, buffer, ref offset, buffer.Length);
            Byte[] sPPPoEUser = System.Text.Encoding.ASCII.GetBytes(this.SPPPoEUser);
            if (sPPPoEUser.Length < 32)
            {
                LittleEndian.WriteBytes(sPPPoEUser, 0, sPPPoEUser.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - sPPPoEUser.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(sPPPoEUser, 0, 32, buffer, ref offset, buffer.Length);
            }
            Byte[] sPPPoEPassword = System.Text.Encoding.ASCII.GetBytes(this.SPPPoEPassword);
            if (sPPPoEPassword.Length < 16)
            {
                LittleEndian.WriteBytes(sPPPoEPassword, 0, sPPPoEPassword.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 16 - sPPPoEPassword.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(sPPPoEPassword, 0, 16, buffer, ref offset, buffer.Length);
            }
            Byte[] sPPPoEIP = System.Text.Encoding.ASCII.GetBytes(this.SPPPoEIP);
            if (sPPPoEIP.Length < 32)
            {
                LittleEndian.WriteBytes(sPPPoEIP, 0, sPPPoEIP.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - sPPPoEIP.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(sPPPoEIP, 0, 32, buffer, ref offset, buffer.Length);
            }
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    /// <summary>
    /// 设置网络的应答
    /// </summary>
    public class SetNetInfoResponse : StreamResponse { }
}
