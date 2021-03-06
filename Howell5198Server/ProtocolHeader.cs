﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Extensions.Protocol;
using Howell.IO.Serialization;

namespace Howell5198
{
    /// <summary>
    /// 5198协议头结构
    /// </summary>
    public class ProtocolHeader : FixedHeader
    {

        //报文头
        //域	                类型(字节数)	描述
        //协议类型(proType)	    UInt32(4)	    当前为1
        //主版本号(proVersion)  UInt32(4)	    
        //数据长度(dataLen)	    UInt32(4)	    
        //次版本号(proMinVersion)   UInt32(4)	当前为0
        //错误类型(errornum)	UInt32(4)        
        //保留字节(reserved)	 Byte(120)	       



        /// <summary>
        /// 
        /// </summary>
        public ProtocolHeader()
            : base()
        {
            this.proType = 0x1;
            this.proVersion = 1;
            this.dataLen = 0;
            this.proMinVersion = 0;
            this.errornum = 0;
            this.Reserved = new Byte[120];
        }
        /// <summary>
        /// 协议类型
        /// </summary>
        public UInt32 proType { get; set; }
        /// <summary>
        /// 主版本号
        /// </summary>
        public UInt32 proVersion { get; set; }
        /// <summary>
        /// 数据长度，不包含协议头的长度
        /// </summary>
        public UInt32 dataLen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public UInt32 proMinVersion { get; set; }
        /// <summary>
        /// 错误类型:服务器发给客户端的错误码
        /// </summary>
        public UInt32 errornum { get; set; }
        /// <summary>
        /// 保留 120字节
        /// </summary>
        public Byte[] Reserved { get; set; }

        /// <summary>
        /// 协议头是否有效
        /// </summary>
        /// <returns></returns>
        public override bool Validated()
        {
            return true;
        }
        /// <summary>
        /// 获取载荷数据字节长度
        /// </summary>
        /// <returns></returns>
        public override int GetPayloadLength()
        {
            return (int)this.dataLen;
        }
        /// <summary>
        /// 设置载荷数据字节长度
        /// </summary>
        /// <param name="payloadLength"></param>
        public override void SetPayloadLength(int payloadLength)
        {
            this.dataLen = (uint)payloadLength;
        }
        /// <summary>
        /// 获取协议头字节长度
        /// </summary>
        /// <returns></returns>
        public override int GetFixedHeaderLength()
        {
            return 140;
        }
        /// <summary>
        /// 从数组中读取头结构
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.proType = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.proVersion = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.dataLen = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.proMinVersion = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.errornum = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.Reserved = LittleEndian.ReadBytes(120, buffer, ref newOffset, length);
           
        }
        /// <summary>
        /// 将头结构转换为数组
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetFixedHeaderLength()];
            Int32 offset = 0;
            LittleEndian.WriteUInt32(this.proType, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.proVersion, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.dataLen, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.proMinVersion, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.errornum, buffer, ref offset, buffer.Length);
            LittleEndian.WriteBytes(this.Reserved, 0, this.Reserved.Length, buffer, ref offset, buffer.Length);
            return buffer;
        }

    }
    /// <summary>
    /// 协议命令
    /// </summary>
    public static class ProtocolType
    {
        /// <summary>
        /// 登录
        /// </summary>
        public const UInt32 Login = 0xff000001;
        /// <summary>
        ///获取实时编码数据
        /// </summary>
        public const UInt32 Main_stream = 0xff00001E;
        public const UInt32 Sub_stream = 0xff000067;
        /// <summary>
        ///获取&设置色彩
        /// </summary>
        public const UInt32 Get_color = 0xff000003;
        public const UInt32 Set_color = 0xff000004;
        /// <summary>
        ///获取&设置通道日期
        /// </summary>
        public const UInt32 Get_osddate = 0xff000005;
        public const UInt32 Set_osddate = 0xff000006;
        /// <summary>
        ///获取&设置通道名称
        /// </summary>
        public const UInt32 Get_osdchannel = 0xff000007;
        public const UInt32 Set_osdchannel = 0xff000008;
        /// <summary>
        ///重启&关闭设备
        /// </summary>
        public const UInt32 Restart_device = 0xff000011;
        public const UInt32 Close_device = 0xff000012;
        /// <summary>
        /// 获取文件列表
        /// </summary>
        public const UInt32 SearchFile = 0xff000020;
        /// <summary>
        /// 请求下载文件
        /// </summary>
        public const UInt32 GetFile = 0xff000021;
        /// <summary>
        ///获取&设置图像质量
        /// </summary>
        public const UInt32 Get_videoquality = 0xff000027;
        public const UInt32 Set_videoquality = 0xff000028;
        /// <summary>
        ///恢复默认值
        /// </summary>
        public const UInt32 Reset = 0xff00002e;
        /// <summary>
        /// 强制I帧
        /// </summary>
        public const UInt32 ForceIFrame = 0xff000037;
        /// <summary>
        ///PTZ命令控制
        /// </summary>
        public const UInt32 Ptzcontrol = 0xff000041;
        /// <summary>
        ///获取&设置码流类型
        /// </summary>
        public const UInt32 Get_streamtype = 0xff000042;
        public const UInt32 Set_streamtype = 0xff000043;
        /// <summary>
        ///tServerInfo信息前的协议类型
        /// </summary>
        public const UInt32 Serverinfo = 0xff000046;
        /// <summary>
        ///获取&设置时间
        /// </summary>
        public const UInt32 Get_systemtime = 0xff000047;
        public const UInt32 Set_systemtime = 0xff000048;
        /// <summary>
        ///获取&设置网络
        /// </summary>
        public const UInt32 Get_netinfo = 0xff000049;
        public const UInt32 Set_netinfo = 0xff00004a;
        /// <summary>
        ///获取&设置PTZ设置
        /// </summary>
        public const UInt32 Get_ptzrs232cfg = 0xff000056;
        public const UInt32 Set_ptzrs232cfg = 0xff000057;
        /// <summary>
        /// 回放获取文件信息
        /// </summary>
        public const UInt32 GetFileInfo = 0xff00005e;
        /// <summary>
        ///获取&设置串口模式
        /// </summary>
        public const UInt32 Get_rs232cfg = 0xff000063;
        public const UInt32 Set_rs232cfg = 0xff000064;
        /// <summary>
        /// 字码流设置
        /// </summary>
         public const UInt32 Get_SubChannelSet = 0xff00006C;
         public const UInt32 Set_SubChannelSet = 0xff00006D;
        /// <summary>
        /// 扩展移动侦测信息
        /// </summary>
        public const UInt32 Get_MotionRowcols = 0xff000070;
        public const UInt32 Get_MotionExSet = 0xff000071;
        public const UInt32 Set_MotionExSet = 0xff000072;
        /// <summary>
        /// 预览获取视频信息
        /// </summary>
        public const UInt32 GetNetHead = 0xff000080;

        /// <summary>
        /// 发送需要接收报警信息协议
        /// </summary>
        public const UInt32 RegisterAlarm = 0xff000044;
        /// <summary>
        /// 获取设备配置信息
        /// </summary>
        public const UInt32 GetDeviceCfg = 0xff00004F;
        /// <summary>
        /// 获取远程服务器窗口数目，可调用GetDevCfg来达到类似功能
        /// </summary>
        public const UInt32 GetWindowNumber=0xff000009;
        public const UInt32 GetDspNumber=0xff00000B;

        /// <summary>
        /// 移动侦测
        /// </summary>
        public const UInt32 GetMotionRecord=0xff00000C;
        public const UInt32 GetMotionSet=0xff00000D;
        public const UInt32 SetMotionSet=0xff00000E;

        /*system*/
        public const UInt32 SyncTime=0xff000013;

        /*user*/
        public const UInt32 GetUser=0xff000014;
        public const UInt32 AddUser=0xff000015;
        public const UInt32 DelUser=0xff000016;
        public const UInt32 UpdateUser=0xff000017;

        /*work sheet info*/
        public const UInt32 GetWorkSheet=0xff000018;
        public const UInt32 SetWorkSheet=0xff000019;
        /*rec*/
        public const UInt32 StartRecord=0xff00001A;
        public const UInt32 StopRecord=0xff00001B;
        /// <summary>
        /// 获取网络状况，若网络异常的话返回失败
        /// </summary>
        public const UInt32 NetStat = 0xff00001D;
        public const UInt32 CaptureJpeg = 0xff000091;
        //可靠校时
        public const UInt32 GetTimeEx = 0xff001040;
        public const UInt32 SetTimeEx = 0xff001041;
        /// <summary>
        /// NTP
        /// </summary>
        public const UInt32 Get_ntpinfo=0xff002049;
        public const UInt32 Set_ntpinfo=0xff002050;

        /// <summary>
        /// 获取全景摄像机列表
        /// </summary>
        public const UInt32 GetPanoCameraList = 0xff003000;
        /// <summary>
        /// 创建全景摄像机
        /// </summary>
        public const UInt32 AddPanoCamera = 0xff003001;
        /// <summary>
        /// 获取指定全景摄像机信息
        /// </summary>
        public const UInt32 GetPanoCamera = 0xff003002;
        /// <summary>
        /// 修改或创建指定全景摄像机信息
        /// </summary>
        public const UInt32 SetPanoCamera = 0xff003003;
        /// <summary>
        /// 删除指定全景摄像机信息
        /// </summary>
        public const UInt32 DeletePanoCamera = 0xff003004;
        /// <summary>
        /// 获取服务程序版本信息
        /// </summary>
        public const UInt32 GetServiceVersion = 0xff003005;
        /// <summary>
        /// 查询设备信息
        /// </summary>
        public const UInt32 GetDeviceInfo= 0xff003006;
        /// <summary>
        /// 查询设备状态
        /// </summary>
        public const UInt32 GetDeviceStatus = 0xff003007;
        /// <summary>
        /// 查询网口信息
        /// </summary>
        public const UInt32 GetNetworkInterface = 0xff003008;
        /// <summary>
        /// 查询解码单元列表
        /// </summary>
        public const UInt32 GetDecodingUnitList = 0xff003009;
        /// <summary>
        /// 查询解码单元
        /// </summary>
        public const UInt32 GetDecodingUnit = 0xff00300a;
        /// <summary>
        /// 获取解码单元播放器视频水平旋转速度
        /// </summary>
        public const UInt32 GetRotatingSpeed = 0xff00300b;
        /// <summary>
        /// 设置解码单元播放器视频水平旋转速度
        /// </summary>
        public const UInt32 SetRotatingSpeed = 0xff00300c;
        /// <summary>
        /// 切换解码单元显示的全景摄像机
        /// </summary>
        public const UInt32 SwitchPanoCamera = 0xff00300d;
        /// <summary>
        /// 获取解码单元显示的全景摄像机视角
        /// </summary>
        public const UInt32 GetViewPoint = 0xff00300e;
        /// <summary>
        /// 修改解码单元显示的全景摄像机视角
        /// </summary>
        public const UInt32 SetViewPoint = 0xff00300f;
        /// <summary>
        /// 锁定或解锁解码单元显示的全景摄像机视角
        /// </summary>
        public const UInt32 SetViewPointFixed = 0xff003010;
        /// <summary>
        /// 修改解码单元显示的全景摄像机的显示列数量
        /// </summary>
        public const UInt32 SetViewPointRows = 0xff003011;
        /// <summary>
        /// 获取解码单元的播放器状态
        /// </summary>
        public const UInt32 GetPlayerStatus = 0xff003012;
        /// <summary>
        /// 单帧进
        /// </summary>
        public const UInt32 OneByOne = 0xff003013;
        /// <summary>
        /// 暂停播放
        /// </summary>
        public const UInt32 Pause = 0xff003014;
        /// <summary>
        /// 恢复播放
        /// </summary>
        public const UInt32 Resume = 0xff003015;
        /// <summary>
        /// 定位播放器进度
        /// </summary>
        public const UInt32 Seek = 0xff003016;
        public const UInt32 AlarmHeartbeat = 8;
        public const UInt32 Unknow = 0xff001024;
        public const UInt32 GetCapability= 0xff00008c;
    }
    /// <summary>
    /// 错误号
    /// </summary>
    public static class ErrorNo
    {
        /// <summary>
        /// 没有错误
        /// </summary>
        public const Int32 HW_NET_NOERROR = 0;
        /// <summary>
        /// 用户名密码错误
        /// </summary>
        public const Int32 HW_NET_PASSWORD_ERROR = 1;
        /// <summary>
        /// 权限不足
        /// </summary>
        public const Int32 HW_NET_NOENOUGHPRI = 2;
        /// <summary>
        /// 没有初始化
        /// </summary>
        public const Int32 HW_NET_NOINIT = 3;
        /// <summary>
        /// 通道号错误
        /// </summary>
        public const Int32 HW_NET_CHANNEL_ERROR = 4;
        /// <summary>
        /// 连接到DVR的客户端个数超过最大
        /// </summary>
        public const Int32 HW_NET_OVER_MAXLINK = 5;
        /// <summary>
        /// 版本不匹配
        /// </summary>
        public const Int32 HW_NET_VERSIONNOMATCH = 6;
        /// <summary>
        /// 连接服务器失败
        /// </summary>
        public const Int32 HW_NET_NETWORK_FAIL_CONNECT = 7;
        /// <summary>
        /// 向服务器发送失败
        /// </summary>
        public const Int32 HW_NET_NETWORK_SEND_ERROR = 8;
        /// <summary>
        /// 从服务器接收数据失败
        /// </summary>
        public const Int32 HW_NET_NETWORK_RECV_ERROR = 9;
        /// <summary>
        /// 从服务器接收数据超时
        /// </summary>
        public const Int32 HW_NET_NETWORK_RECV_TIMEOUT = 10;
        /// <summary>
        /// 传送的数据有误
        /// </summary>
        public const Int32 HW_NET_NETWORK_ERRORDATA = 11;
        /// <summary>
        /// 调用次序错误
        /// </summary>
        public const Int32 HW_NET_ORDER_ERROR = 12;
        /// <summary>
        /// 无此权限
        /// </summary>
        public const Int32 HW_NET_OPERNOPERMIT = 13;
        /// <summary>
        /// DVR命令执行超时
        /// </summary>
        public const Int32 HW_NET_COMMANDTIMEOUT = 14;
        /// <summary>
        /// 串口号错误
        /// </summary>
        public const Int32 HW_NET_ERRORSERIALPORT = 15;
        /// <summary>
        /// 报警端口错误
        /// </summary>
        public const Int32 HW_NET_ERRORALARMPORT = 16;
        /// <summary>
        /// 参数错误
        /// </summary>
        public const Int32 HW_NET_PARAMETER_ERROR = 17;
        /// <summary>
        /// 服务器通道处于错误状态
        /// </summary>
        public const Int32 HW_NET_CHAN_EXCEPTION = 18;
        /// <summary>
        /// 没有硬盘
        /// </summary>
        public const Int32 HW_NET_NODISK = 19;
        /// <summary>
        /// 硬盘号错误
        /// </summary>
        public const Int32 HW_NET_ERRORDISKNUM = 20;
        /// <summary>
        /// 服务器硬盘满
        /// </summary>
        public const Int32 HW_NET_DISK_FULL = 21;
        /// <summary>
        /// 服务器硬盘出错
        /// </summary>
        public const Int32 HW_NET_DISK_ERROR = 22;
        /// <summary>
        /// 服务器不支持
        /// </summary>
        public const Int32 HW_NET_NOSUPPORT = 23;
        /// <summary>
        /// 服务器忙
        /// </summary>
        public const Int32 HW_NET_BUSY = 24;
        /// <summary>
        /// 服务器修改不成功
        /// </summary>
        public const Int32 HW_NET_MODIFY_FAIL = 25;
        /// <summary>
        /// 密码输入格式不正确
        /// </summary>
        public const Int32 HW_NET_PASSWORD_FORMAT_ERROR = 26;
        /// <summary>
        /// 硬盘正在格式化，不能启动操作
        /// </summary>
        public const Int32 HW_NET_DISK_FORMATING = 27;
        /// <summary>
        /// DVR资源不足
        /// </summary>
        public const Int32 HW_NET_DVRNORESOURCE = 28;
        /// <summary>
        /// DVR操作失败
        /// </summary>
        public const Int32 HW_NET_DVROPRATEFAILED = 29;
        /// <summary>
        /// 打开PC声音失败
        /// </summary>
        public const Int32 HW_NET_OPENHOSTSOUND_FAIL = 30;
        /// <summary>
        /// 服务器语音对讲被占用
        /// </summary>
        public const Int32 HW_NET_DVRVOICEOPENED = 31;
        /// <summary>
        /// 时间输入不正确
        /// </summary>
        public const Int32 HW_NET_TIMEINPUTERROR = 32;
        /// <summary>
        /// 回放时服务器没有指定的文件
        /// </summary>
        public const Int32 HW_NET_NOSPECFILE = 33;
        /// <summary>
        /// 创建文件出错
        /// </summary>
        public const Int32 HW_NET_CREATEFILE_ERROR = 34;
        /// <summary>
        /// 打开文件出错
        /// </summary>
        public const Int32 HW_NET_FILEOPENFAIL = 35;
        /// <summary>
        /// 上次的操作还没有完成
        /// </summary>
        public const Int32 HW_NET_OPERNOTFINISH = 36;
        /// <summary>
        /// 获取当前播放的时间出错
        /// </summary>
        public const Int32 HW_NET_GETPLAYTIMEFAIL = 37;
        /// <summary>
        /// 播放出错
        /// </summary>
        public const Int32 HW_NET_PLAYFAIL = 38;
        /// <summary>
        /// 文件格式不正确
        /// </summary>
        public const Int32 HW_NET_FILEFORMAT_ERROR = 39;
        /// <summary>
        /// 路径错误
        /// </summary>
        public const Int32 HW_NET_DIR_ERROR = 40;
        /// <summary>
        /// 资源分配错误
        /// </summary>
        public const Int32 HW_NET_ALLOC_RESOUCE_ERROR = 41;
        /// <summary>
        /// 声卡模式错误
        /// </summary>
        public const Int32 HW_NET_AUDIO_MODE_ERROR = 42;
        /// <summary>
        /// 缓冲区太小
        /// </summary>
        public const Int32 HW_NET_NOENOUGH_BUF = 43;
        /// <summary>
        /// 创建SOCKET出错
        /// </summary>
        public const Int32 HW_NET_CREATESOCKET_ERROR = 44;
        /// <summary>
        /// 设置SOCKET出错
        /// </summary>
        public const Int32 HW_NET_SETSOCKET_ERROR = 45;
        /// <summary>
        /// 个数达到最大
        /// </summary>
        public const Int32 HW_NET_MAX_NUM = 46;
        /// <summary>
        /// 用户不存在
        /// </summary>
        public const Int32 HW_NET_USERNOTEXIST = 47;
        /// <summary>
        /// 写FLASH出错
        /// </summary>
        public const Int32 HW_NET_WRITEFLASHERROR = 48;
        /// <summary>
        /// DVR升级失败
        /// </summary>
        public const Int32 HW_NET_UPGRADEFAIL = 49;
        /// <summary>
        /// 解码卡已经初始化过
        /// </summary>
        public const Int32 HW_NET_CARDHAVEINIT = 50;
        /// <summary>
        /// 播放器中错误
        /// </summary>
        public const Int32 HW_NET_PLAYERFAILED = 51;
        /// <summary>
        /// 用户数达到最大
        /// </summary>
        public const Int32 HW_NET_MAX_USERNUM = 52;
        /// <summary>
        /// 获得客户端的IP地址或物理地址失败
        /// </summary>
        public const Int32 HW_NET_GETLOCALIPANDMACFAIL = 53;
        /// <summary>
        /// 该通道没有编码
        /// </summary>
        public const Int32 HW_NET_NOENCODEING = 54;
        /// <summary>
        /// IP地址不匹配
        /// </summary>
        public const Int32 HW_NET_IPMISMATCH = 55;
        /// <summary>
        /// MAC地址不匹配
        /// </summary>
        public const Int32 HW_NET_MACMISMATCH = 56;
        /// <summary>
        /// 升级文件语言不匹配
        /// </summary>
        public const Int32 HW_NET_UPGRADELANGMISMATCH = 57;
        /// <summary>
        /// 本地显卡不支持
        /// </summary>
        public const Int32 HW_NET_DDRAWDEVICENOSUPPORT = 58;
        /// <summary>
        /// 服务器升级资源被占用
        /// </summary>
        public const Int32 HW_NET_UPGRADEOCCUPIED = 59;
        /// <summary>
        /// 语音对讲已经存在
        /// </summary>
        public const Int32 HW_NET_VOICE_EXIST = 60;
        /// <summary>
        /// 预览个数达到最大
        /// </summary>
        public const Int32 HW_NET_MAX_LIVE_NUM = 61;
    }
    /// <summary>
    /// 应答结果常量
    /// </summary>
    public class ResultCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        public const Int32 OK = 0;
        /// <summary>
        /// 发送成功，被屏蔽
        /// </summary>
        public const Int32 Silent = 201;
        /// <summary>
        /// 权限不足
        /// </summary>
        public const Int32 Priority = 202;
        /// <summary>
        /// 未找到资源
        /// </summary>
        public const Int32 NotFound = 404;
        /// <summary>
        /// 服务器内部错误
        /// </summary>
        public const Int32 ServerInternalError = 500;
    }
}
