using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /// <summary>
    /// 获取串口模式
    /// </summary>
    public class GetRs232CfgRequest : IBytesSerialize
    {
        public GetRs232CfgRequest()
        {
            this.Rs232_no = 0;
        }

        /// <summary>
        /// 串口号
        /// </summary>
        public Int32 Rs232_no { get; set; }

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
            this.Rs232_no = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.Rs232_no, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    /// <summary>
    ///串口模式信息
    /// </summary>
    public class Rs232CfgInfo : IBytesSerialize
    {
        public Rs232CfgInfo()
        {
            this.Rs232_no = 0;
            this.Rate = 0;
            this.Data_bit = 0;
            this.Stop_bit = 0;
            this.Parity = 0;
            this.Flow_control = 0;
            this.Work_mode = 0;
            this.Annunciator_type = 0;
            this.Reserved = new Int32[32];
        }

        /// <summary>
        /// 串口号
        /// </summary>
        public Int32 Rs232_no { get; set; }
        /// <summary>
        ///波特率(bps)，0－50，1－75，2－110，3－150，4－300，5－600，6－1200，7－2400，8－4800，9－9600，10－19200， 11－38400，12－57600，13－76800，14－115.2k
        /// </summary>
        public Int32 Rate { get; set; }
        /// <summary>
        /// 数据位  0-5位 1-6位 2-7位 3-8位
        /// </summary>
        public Byte Data_bit { get; set; }
        /// <summary>
        /// 停止位0-1位 1-2位
        /// </summary>
        public Byte Stop_bit { get; set; }
        /// <summary>
        /// 0-无校验 1-奇校验 2-偶校验
        /// </summary>
        public Byte Parity { get; set; }
        /// <summary>
        /// 0-无 1-软流控 2-硬流控
        /// </summary>
        public Byte Flow_control { get; set; }
        /// <summary>
        /// 工作模式 0-云台(PTZ控制) 1-报警接收 2-透明通道
        /// </summary>
        public Int32 Work_mode { get; set; }
        /// <summary>
        ///0-Howell 1-VISTA 120 ...
        /// </summary>
        public Int32 Annunciator_type { get; set; }
        /// <summary>
        /// 32个Int32的保留信息
        /// </summary>
        public Int32[] Reserved { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 148;
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
            this.Rs232_no = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Rate = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Data_bit = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Stop_bit = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Parity = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Flow_control = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Work_mode = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Annunciator_type = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserved.Length; ++i)
            {
                this.Reserved[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.Rs232_no, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Rate, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Data_bit, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Stop_bit, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Parity, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Flow_control, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Work_mode, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Annunciator_type, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserved.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserved[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    /// <summary>
    ///设置串口模式的应答
    /// </summary>
    public class SetRs232CfgResponse : StreamResponse { }
    /// <summary>
    ///获取PTZ设置
    /// </summary>
    public class GetPtzRs232CfgRequest : StreamRequest { }

    /// <summary>
    ///PTZ设置信息
    /// </summary>
    public class PtzRs232CfgInfo : IBytesSerialize
    {
        enum ProtocolType
        {
            PELCO_D = 0,
            PELCO_P,
            ALEC,
            YANAN
        };
        public PtzRs232CfgInfo()
        {
            this.Slot = 0;
            this.Rs232_no = 0;
            this.Rate = 0;
            this.Data_bit = 0;
            this.Stop_bit = 0;
            this.Parity = 0;
            this.Flow_control = 0;
            this.Protocol = 0;
            this.Address = 0;
            this.Reserved = new Int32[32];
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        /// 串口号
        /// </summary>
        public Int32 Rs232_no { get; set; }
        /// <summary>
        ///波特率(bps)，0－50，1－75，2－110，3－150，4－300，5－600，6－1200，7－2400，8－4800，9－9600，10－19200， 11－38400，12－57600，13－76800，14－115.2k
        /// </summary>
        public Int32 Rate { get; set; }
        /// <summary>
        /// 数据位  0-5位 1-6位 2-7位 3-8位
        /// </summary>
        public Byte Data_bit { get; set; }
        /// <summary>
        /// 停止位0-1位 1-2位
        /// </summary>
        public Byte Stop_bit { get; set; }
        /// <summary>
        /// 0-无校验 1-奇校验 2-偶校验
        /// </summary>
        public Byte Parity { get; set; }
        /// <summary>
        /// 0-无 1-软流控 2-硬流控
        /// </summary>
        public Byte Flow_control { get; set; }
        /// <summary>
        /// 协议类型，参照ProtocolType
        /// </summary>
        public Int32 Protocol { get; set; }
        /// <summary>
        ///地址 0-255
        /// </summary>
        public Int32 Address { get; set; }
        /// <summary>
        /// 32个Int32的保留信息
        /// </summary>
        public Int32[] Reserved { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return 152;
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
            this.Rs232_no = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Rate = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Data_bit = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Stop_bit = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Parity = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Flow_control = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Protocol = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Address = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserved.Length; ++i)
            {
                this.Reserved[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.Rs232_no, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Rate, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Data_bit, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Stop_bit, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Parity, buffer, ref offset, buffer.Length);
            LittleEndian.WriteByte(this.Flow_control, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Protocol, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Address, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserved.Length; ++i)
            {
                LittleEndian.WriteInt32(this.Reserved[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }

    /// <summary>
    ///设置PTZ设置的应答
    /// </summary>
    public class SetPtzRs232CfgResponse : StreamResponse { }
    /// <summary>
    ///PTZ命令控制
    /// </summary>
    public class PtzControlRequest : IBytesSerialize
    {
        public PtzControlRequest()
        {
            this.Slot = 0;
            this.ControlType = 0;
            this.Cmd = 0;
            this.Presetno = 0;
            this.Left = 0;
            this.Top = 0;
            this.Right = 0;
            this.Bottom = 0;
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Int32 Slot { get; set; }
        /// <summary>
        ///0-direct 1- len 2-AUTO zoom in,3-preset
        /// </summary>
        public Int32 ControlType { get; set; }
        /// <summary>
        ///
        /// </summary> 
        /*direct: 7-left up
		             8-up
					 9-right up
					 4-left
					 5-stop
					 6-right
					 1-left down
					 2-down
					 3-right down

			len:     1-iris open
			         2-iris close
					 3-len tele
					 4-len wide
					 5-focus far
					 6-focus near
					 7-stop
			AUTO zoom in:
					 1-auto zoom in
			preset:
					 1-set
					 2-clear
					 3-goto  */
        public Int32 Cmd { get; set; }
        /// <summary>
        /// 预置点号或方向移动的速度(0-64)
        /// </summary>
        public Int32 Presetno { get; set; }
        /// <summary>
        /// RECT.left
        /// </summary>
        public Int32 Left { get; set; }
        /// <summary>
        ///RECT.top
        /// </summary>
        public Int32 Top { get; set; }
        /// <summary>
        /// RECT.right
        /// </summary>
        public Int32 Right { get; set; }
        /// <summary>
        ///RECT.bottom
        /// </summary>
        public Int32 Bottom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
            this.Slot = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.ControlType = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Cmd = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Presetno = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Left = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Top = LittleEndian.ReadByte(buffer, ref newOffset, length);
            this.Right = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Bottom = LittleEndian.ReadInt32(buffer, ref newOffset, length);
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
            LittleEndian.WriteInt32(this.ControlType, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Cmd, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Presetno, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Left, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Top, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Right, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Bottom, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }
    /// <summary>
    ///PTZ命令控制的应答
    /// </summary>
    public class PtzControlResponse : StreamResponse{ }
}
