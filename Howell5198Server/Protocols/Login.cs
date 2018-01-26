using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    [DataContract(Name = "LoginRequest")]
    public class LoginRequest : IBytesSerialize
    {
        public LoginRequest()
        {
            this.Type = 0;
            this.UserName = null;
            this.Password = null;
            this.ClientUserID = 0;
            this.Reserved = new Int32[32];
        }
        /// <summary>
        /// 暂时未使用
        /// </summary>
          [DataMember(Name = "Type", IsRequired = true, EmitDefaultValue = true, Order = 1)]
        public Int32 Type { get; set; }
        /// <summary>
        /// 用户名（最大长度32个单字节）
        /// </summary>
      [DataMember(Name = "UserName", IsRequired = true, EmitDefaultValue = true, Order = 2)]
        public String UserName { get; set; }
        /// <summary>
        /// 密码（最大长度32个单字节）
        /// </summary>
       [DataMember(Name = "Password", IsRequired = true, EmitDefaultValue = true, Order = 3)]
        public String Password { get; set; }
        /// <summary>
        /// 客户端用户ID(暂时未使用)
        /// </summary>
       [DataMember(Name = "ClientUserID", IsRequired = true, EmitDefaultValue = true, Order = 4)]
        public Int32 ClientUserID { get; set; }
        /// <summary>
        /// 32个Int32的保留信息
        /// </summary>
        [DataMember(Name = "Reserved", IsRequired = true, EmitDefaultValue = true, Order =5)]
        public Int32[] Reserved { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 200;
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
            this.Type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.UserName = LittleEndian.ReadString(32, buffer, ref newOffset, length); 
            //Byte[] password = LittleEndian.ReadBytes(32, buffer, ref newOffset, length);
            this.Password = LittleEndian.ReadString(32, buffer, ref newOffset, length); 
            this.ClientUserID = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            Byte[] Fill = new byte[32];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Type, buffer, ref offset, buffer.Length);
            /*以下代码为新加*/
            Byte[] userName = System.Text.Encoding.ASCII.GetBytes(this.UserName);
            if(userName.Length<32)
            {
                LittleEndian.WriteBytes(userName, 0, userName.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - userName.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(userName, 0, 32, buffer, ref offset, buffer.Length);
            }
            Byte[] password = System.Text.Encoding.ASCII.GetBytes(this.Password);
             if(password.Length<32)
             {
                 LittleEndian.WriteBytes(password, 0, password.Length, buffer, ref offset, buffer.Length);
                 LittleEndian.WriteBytes(Fill, 0, 32-password.Length, buffer, ref offset, buffer.Length);
             }
             else
             {
                 LittleEndian.WriteBytes(password, 0, 32, buffer, ref offset, buffer.Length);
             }                 
            LittleEndian.WriteInt32(this.ClientUserID, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserved.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserved[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    /// <summary>
    /// 登录应答
    /// </summary>
    [DataContract(Name = "LoginResponse")]
    public class LoginResponse : IBytesSerialize
    {
        public LoginResponse()
        {
            this.Success = 0;
            //this.SlotCount = 0;
            //this.SverVersion = 0;
            //this.NetVersion = 0;
            //this.Reserver = new Int32[32];
        }
        /// <summary>
        /// 该值为0表明登录成功，并继续发送tServerInfo信息，如果为-1表明登录失败，不发任何信息
        /// </summary>
        [DataMember(Name = "Success", IsRequired = true, EmitDefaultValue = true, Order = 1)]
        public Int32 Success { get; set; }
        ///// <summary>
        ///// 通道总数
        ///// </summary>
        //[DataMember(Name = "SlotCount", IsRequired = false, EmitDefaultValue = false, Order = 2)]
        //public Int32 SlotCount { get; set; }
        ///// <summary>
        ///// 服务器版本
        ///// </summary>
        //[DataMember(Name = "SverVersion", IsRequired = false, EmitDefaultValue = false, Order = 3)]
        //public Int32 SverVersion { get; set; }
        ///// <summary>
        ///// 网络版本
        ///// </summary>
        //[DataMember(Name = "NetVersion", IsRequired = false, EmitDefaultValue = false, Order = 4)]
        //public Int32 NetVersion { get; set; }
        ///// <summary>
        ///// 32个Int32的保留信息
        ///// </summary>
        // [DataMember(Name = "Reserver", IsRequired = false, EmitDefaultValue = false, Order = 5)]
        //public Int32[] Reserver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            //return (this.Success == 0) ? 144 : 4;
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
            this.Success = LittleEndian.ReadInt32(buffer, ref newOffset, length );
            //if (this.Success == 0)
            //{
            //    this.SlotCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            //    this.SverVersion = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            //    this.NetVersion = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            //    for (int i = 0; i < this.Reserver.Length; ++i)
            //    {
            //        this.Reserver[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            //    }
            //}

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Int32 offset = 0;
            LittleEndian.WriteInt32(this.Success, buffer, ref offset, buffer.Length);
            //if (this.Success == 0)
            //{
            //    LittleEndian.WriteInt32(this.SlotCount, buffer, ref offset, buffer.Length);
            //    LittleEndian.WriteInt32(this.SverVersion, buffer, ref offset,  buffer.Length);
            //    LittleEndian.WriteInt32(this.NetVersion, buffer, ref offset, buffer.Length);
            //    for (int i = 0; i < this.Reserver.Length; ++i)
            //    {
            //        LittleEndian.WriteInt32(this.Reserver[i], buffer, ref offset, buffer.Length);
            //    }
            //}
            return buffer;
        }
    }
    public class ServerInfo : IBytesSerialize
    {
        public ServerInfo()
        {
            this.SlotCount = 0;
            this.SverVersion = 0;
            this.NetVersion = 0;
            this.Reserver = new Int32[32];
        }
        /// <summary>
        /// 通道总数
        /// </summary>
        [DataMember(Name = "SlotCount", IsRequired = false, EmitDefaultValue = false, Order = 2)]
        public Int32 SlotCount { get; set; }
        /// <summary>
        /// 服务器版本
        /// </summary>
        [DataMember(Name = "SverVersion", IsRequired = false, EmitDefaultValue = false, Order = 3)]
        public Int32 SverVersion { get; set; }
        /// <summary>
        /// 网络版本
        /// </summary>
        [DataMember(Name = "NetVersion", IsRequired = false, EmitDefaultValue = false, Order = 4)]
        public Int32 NetVersion { get; set; }
        /// <summary>
        /// 32个Int32的保留信息
        /// </summary>
        [DataMember(Name = "Reserver", IsRequired = false, EmitDefaultValue = false, Order = 5)]
        public Int32[] Reserver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 140;
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
            
            this.SlotCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.SverVersion = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.NetVersion = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserver.Length; ++i)
            {
                this.Reserver[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
           
            LittleEndian.WriteInt32(this.SlotCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.SverVersion, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.NetVersion, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserver.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserver[i], buffer, ref offset, buffer.Length);
            }
           
            return buffer;
        }
    }
}
