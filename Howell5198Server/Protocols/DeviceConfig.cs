using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;

namespace Howell5198.Protocols
{
    /*
     char	devName[MAX_NAME_LEN];				//设备名称
	char	devSerialID[HW_MAX_SERIALID];		//设备序列号
	int		window_count;		//窗口数
	int		alarm_in_count;		//报警输入数
	int		alarm_out_count;	//报警输出数		
	int		disk_count;			//硬盘数
	int		dsp_count;			//dsp数
	int		rs232_count;		//串口数
	char	channelname[OLD_MAX_SLOT][32]; // 通道名称
     int		net_channel_count;  //网络通道数
	char	reserve[28];
     */
    public class DeviceConfig : IBytesSerialize
    {
        public DeviceConfig()
        {
            this.DevName = "";
            this.DevSerialID = "";
            this.WindowCount = 0;
            this.AlarmInCount = 0;
            this.AlarmOutCount = 0;
            this.DiskCount = 0;
            this.DspCount = 0;
            this.Rs232Count = 0;
            this.ChannelName = new String[16];
            for (int i = 0; i < 16;i++ )
            {
                this.ChannelName[i] = "";
            }
            this.NetChannelCount = 0;
            this.Reserve = new Byte[28];
        }

        public String DevName { get; set; }
        public String DevSerialID { get; set; }

        public Int32 WindowCount { get; set; }
        public Int32 AlarmInCount { get; set; }
        public Int32 AlarmOutCount { get; set; }
        public Int32 DiskCount { get; set; }
        public Int32 DspCount { get; set; }
        public Int32 Rs232Count { get; set; }
        public String[] ChannelName { get; set; }
        public Int32 NetChannelCount { get; set; }
        public Byte[] Reserve { get; set; }
        public int GetLength()
        {
            return (32 + 32 * 16 + 4 * 6 + 64);
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
            this.DevName = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.DevSerialID = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.WindowCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.AlarmInCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.AlarmOutCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.DiskCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.DspCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Rs232Count = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.ChannelName=new String[16];
            for (int i = 0; i < 16;i++ )
            {
                this.ChannelName[i] = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            }
           
            this.NetChannelCount = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Reserve = LittleEndian.ReadBytes(28, buffer, ref newOffset, length);
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
            Byte[] devname = System.Text.Encoding.ASCII.GetBytes(this.DevName);
            if (devname.Length < 32)
            {
                LittleEndian.WriteBytes(devname, 0, devname.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - devname.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(devname, 0, 32, buffer, ref offset, buffer.Length);
            }
            Byte[] devserialid = System.Text.Encoding.ASCII.GetBytes(this.DevSerialID);
            if (devserialid.Length < 32)
            {
                LittleEndian.WriteBytes(devserialid, 0, devserialid.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - devserialid.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(devserialid, 0, 32, buffer, ref offset, buffer.Length);
            }

            LittleEndian.WriteInt32(this.WindowCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.AlarmInCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.AlarmOutCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.DiskCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.DspCount, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Rs232Count, buffer, ref offset, buffer.Length);
            for (int i = 0; i < 16; i++)
            {
                if(i>=this.ChannelName.Length)
                {
                    LittleEndian.WriteBytes(Fill, 0, 32, buffer, ref offset, buffer.Length);
                }
                else
                {
                    Byte[] channelname = System.Text.Encoding.ASCII.GetBytes(this.ChannelName[i]);
                    if (channelname.Length < 32)
                    {
                        LittleEndian.WriteBytes(channelname, 0, channelname.Length, buffer, ref offset, buffer.Length);
                        LittleEndian.WriteBytes(Fill, 0, 32 - channelname.Length, buffer, ref offset, buffer.Length);
                    }
                    else
                    {
                        LittleEndian.WriteBytes(channelname, 0, 32, buffer, ref offset, buffer.Length);
                    }
                }
            }
            LittleEndian.WriteInt32(this.NetChannelCount, buffer, ref offset, buffer.Length);
            if (this.Reserve.Length < 28)
            {
                LittleEndian.WriteBytes(this.Reserve, 0, this.Reserve.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 28 - this.Reserve.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(this.Reserve, 0, 28, buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
}
