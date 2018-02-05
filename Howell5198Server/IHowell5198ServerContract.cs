using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Howell5198.Protocols;

namespace Howell5198
{
    public interface IHowell5198ServerContract
    {
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="type">暂时未使用</param>
        /// <param name="logName">用户名</param>
        /// <param name="logPassword">密码</param>
        /// <param name="clientUserID">暂时未使用</param>
        /// <returns>服务器应答</returns>
        LoginResponse Login(Howell5198Session session, LoginRequest loginRequest);
        /// <summary>
        /// 获取服务器信息
        /// </summary>
        /// <returns>服务器应答</returns>
        ServerInfo GetServerInfo(Howell5198Session session);
        /// <summary>
        /// 获取实时编码数据
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="channelno">通道号</param>
        /// <param name="type">0:主码流 1:子码流</param>
        /// <returns>服务器应答</returns>
        StreamResponse GetStream(MediaStreamSession session);
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="channelno">通道号</param>
        /// <param name="type">0:主码流 1：子码流</param>
        /// <returns>帧数据</returns>
        //  FramePayload GetPayload(Howell5198Session session, Int32 channelno,Int32 type);
        /// <summary>
        /// 获取色彩
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="channelno">通道号</param>
        /// <returns>帧数据</returns>
        ColorInfo GetColor(Howell5198Session session, GetColorRequest getColorRequest);
        /// <summary>
        /// 设置色彩
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setColorRequest"></param>
        /// <returns>GetColorResponse</returns>
        SetColorResponse SetColor(Howell5198Session session, ColorInfo setColorRequest);
        /// <summary>
        /// 获取通道名称
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getOsdChannelRequest"></param>
        /// <returns>GetOsdChannelResponse</returns>
        OsdChannelInfo GetOsdChannel(Howell5198Session session, GetOsdChannelRequest getOsdChannelRequest);
        /// <summary>
        /// 设置通道名称
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setOsdChannelRequest"></param>
        /// <returns>SetOsdChannelResponse</returns>
        SetOsdChannelResponse SetOsdChannel(Howell5198Session session, OsdChannelInfo setOsdChannelRequest);
        /// <summary>
        /// 获取通道日期
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getOsdDateRequest"></param>
        /// <returns>GetOsdDateResponse</returns>
        OsdDateInfo GetOsdDate(Howell5198Session session, GetOsdDateRequest getOsdDateRequest);
        /// <summary>
        /// 设置通道日期
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setOsdDateRequest"></param>
        /// <returns>SetOsdDateResponse</returns>
        SetOsdDateResponse SetOsdDate(Howell5198Session session, OsdDateInfo setOsdDateRequest);
        /// <summary>
        /// 获取图像质量
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getVideoQualityRequest"></param>
        /// <returns>GetVideoQualityResponse</returns>
        VideoQualityInfo GetVideoQuality(Howell5198Session session, GetVideoQualityRequest getVideoQualityRequest);
        /// <summary>
        /// 设置图像质量
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setVideoQualityRequest"></param>
        /// <returns>SetVideoQualityResponse</returns>
        SetVideoQualityResponse SetVideoQuality(Howell5198Session session, VideoQualityInfo setVideoQualityRequest);
        /// <summary>
        /// 获取码流类型
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="getStreamTypeRequest"></param>
        /// <returns>GetStreamTypeResponse</returns>
        StreamTypeInfo GetStreamType(Howell5198Session session, GetStreamTypeRequest getStreamTypeRequest);
        /// <summary>
        /// 设置码流类型
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setStreamTypeRequest"></param>
        /// <returns>SetStreamTypeResponse</returns>
        SetStreamTypeResponse SetStreamType(Howell5198Session session, StreamTypeInfo setStreamTypeRequest);
        /// <summary>
        /// 获取网络
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetNetInfoResponse</returns>
        NetInfo GetNetInfo(Howell5198Session session);
        /// <summary>
        /// 设置网络
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setNetInfoRequest"></param>
        /// <returns>SetNetInfoResponse</returns>
        SetNetInfoResponse SetNetInfo(Howell5198Session session, NetInfo setNetInfoRequest);
        /// <summary>
        /// 获取时间
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        SystemTimeInfo GetSystemTime(Howell5198Session session);
        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setSystemTimeRequest"></param>
        /// <returns>SetSystemTimeResponse</returns>
        SetSystemTimeResponse SetSystemTime(Howell5198Session session, SystemTimeInfo setSystemTimeRequest);
        /// <summary>
        /// 重启设备
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        RestartDeviceResponse RestartDevice(Howell5198Session session);
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        CloseDeviceResponse CloseDevice(Howell5198Session session);
        /// <summary>
        /// 重置设备
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        ResetDeviceResponse ResetDevice(Howell5198Session session);
        /// <summary>
        /// 获取串口模式
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        Rs232CfgInfo GetRs232Cfg(Howell5198Session session, GetRs232CfgRequest getRs232CfgRequest);
        /// <summary>
        /// 设置串口模式
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setSystemTimeRequest"></param>
        /// <returns>SetSystemTimeResponse</returns>
        SetRs232CfgResponse SetRs232Cfg(Howell5198Session session, Rs232CfgInfo setRs232CfgRequest);
        /// <summary>
        /// 获取PTZ设置
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <returns>GetSystemTimeResponse</returns>
        PtzRs232CfgInfo GetPtzRs232Cfg(Howell5198Session session, GetPtzRs232CfgRequest getRs232CfgRequest);
        /// <summary>
        /// 设置PTZ设置
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="setSystemTimeRequest"></param>
        /// <returns>SetSystemTimeResponse</returns>
        SetPtzRs232CfgResponse SetPtzRs232Cfg(Howell5198Session session, PtzRs232CfgInfo setPtzRs232CfgRequest);
        /// <summary>
        /// PTZ命令控制
        /// </summary>
        /// <param name="session">请求对象的会话信息</param>
        /// <param name="ptzControlRequest"></param>
        /// <returns>PtzControlResponse</returns>
        PtzControlResponse PtzControl(Howell5198Session session, PtzControlRequest ptzControlRequest);
        /// <summary>
        /// 搜索获得回放文件列表
        /// </summary>
        /// <param name="session"></param>
        /// <param name="searchFileRequest"></param>
        /// <returns></returns>
        SearchFileResponse SearchFile(Howell5198Session session, SearchFileRequest searchFileRequest);
        /// <summary>
        /// 获取回放文件
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getFileRequest"></param>
        /// <returns></returns>
        void GetFile(MediaStreamSession session, GetFileRequest getFileRequest);
        /// <summary>
        /// 回放获取文件信息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getFileInfoRequest"></param>
        /// <returns></returns>
        GetFileInfoResponse GetFileInfo(Howell5198Session session, GetFileInfoRequest getFileInfoRequest);
        /// <summary>
        /// 预览获取视频信息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getNetHeadRequest"></param>
        /// <returns></returns>
        GetNetHeadResponse GetNetHead(Howell5198Session session, GetNetHeadRequest getNetHeadRequest);
        /// <summary>
        /// 获取设备配置信息
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        DeviceConfig GetDeviceConfig(Howell5198Session session);
        /// <summary>
        /// 获取移动侦测配置
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getMotionRequest"></param>
        /// <returns></returns>
        GetMotionResponse GetMotionSet(Howell5198Session session, GetMotionRequest getMotionRequest);
        /// <summary>
        /// 设置移动侦测配置
        /// </summary>
        /// <param name="session"></param>
        /// <param name="setMotionRequest"></param>
        /// <returns></returns>
        SetMotionResponse SetMotionSet(Howell5198Session session, SetMotionRequest setMotionRequest);
        /// <summary>
        /// 获取移动侦测配置EX
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getMotionRequest"></param>
        /// <returns></returns>
        GetMotionExResponse GetMotionExSet(Howell5198Session session, GetMotionExRequest getMotionRequest);
        /// <summary>
        /// 设置移动侦测配置EX
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getMotionRequest"></param>
        /// <returns></returns>
        SetMotionExResponse SetMotionExSet(Howell5198Session session, SetMotionExRequest getMotionRequest);
        /// <summary>
        /// 获取字码流设置
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getMotionRequest"></param>
        /// <returns></returns>
        GetSubChannelSetResponse GetSubChannelSet(Howell5198Session session, GetSubChannelSetRequest getMotionRequest);
        /// <summary>
        /// 设置字码流设置
        /// </summary>
        /// <param name="session"></param>
        /// <param name="getMotionRequest"></param>
        /// <returns></returns>
        SetSubChannelSetResponse SetSubChannelSet(Howell5198Session session, SetSubChannelSetRequest getMotionRequest);
        /// <summary>
        /// 获取可靠校时
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        GetNetSyncTimeResponse GetNetSyncTime(Howell5198Session session);
        /// <summary>
        /// 设置可靠校时
        /// </summary>
        /// <param name="session"></param>
        /// <param name="netSyncTime"></param>
        /// <returns></returns>
        SetNetSyncTimeResponse SetNetSyncTime(Howell5198Session session, NetSyncTime netSyncTime);
        /// <summary>
        ///  强制I帧
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="forceIFrameRequest"></param>
        /// <returns></returns>
        ForceIFrameResponse ForceIFrame(Howell5198Session Session, ForceIFrameRequest forceIFrameRequest);
        /// <summary>
        /// 同步时间
        /// </summary>
        /// <param name="session"></param>
        /// <param name="syncTimeRequest"></param>
        /// <returns></returns>
        SyncTimeResponse SyncTime(Howell5198Session session, SyncTimeRequest syncTimeRequest);
        /// <summary>
        /// 获取用户信息列表
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        DavinciUsers GetUsers(Howell5198Session session);
        /// <summary>
        /// 更新用户信息列表
        /// </summary>
        /// <param name="session"></param>
        /// <param name="updateUserRequest"></param>
        /// <returns></returns>
        UpdateUserResponse UpdateUser(Howell5198Session session, UpdateUserRequest updateUserRequest);
        /// <summary>
        /// 抓图
        /// </summary>
        /// <param name="session"></param>
        /// <param name="captureRequest"></param>
        /// <returns></returns>
        CapturenResponse CaptureJpeg(Howell5198Session session, CaptureRequest captureRequest);
        /// <summary>
        /// 获取NTP配置信息
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        NtpInfo GetNtpInfo(Howell5198Session session);
        /// <summary>
        /// 设置NTP配置信息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ntpInfo"></param>
        /// <returns></returns>
        SetNtpInfoResponse SetNtpInfo(Howell5198Session session, NtpInfo ntpInfo);
        /// <summary>
        /// 注册报警
        /// </summary>
        /// <param name="session"></param>
        /// <param name="registerAlarmRequest"></param>
        /// <returns></returns>
        RegisterAlarmResponse SetRegisterAlarm(Howell5198Session session, RegisterAlarmRequest registerAlarmRequest);
        /// <summary>
        /// 获取全景摄像机列表
        /// </summary>
        /// <param name="session"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        tPanoCameraList GetPanoCameraList(Howell5198Session session, tQueryString queryString);
        /// <summary>
        /// 创建全景摄像机
        /// </summary>
        /// <param name="session"></param>
        /// <param name="panoCamera"></param>
        /// <returns></returns>
        tFault AddPanoCamera(Howell5198Session session, tPanoCamera panoCamera);
        /// <summary>
        /// 获取指定全景摄像机信息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="panoCameraId"></param>
        /// <returns></returns>
        tPanoCamera GetPanoCamera(Howell5198Session session, tPanoCameraId panoCameraId);
        /// <summary>
        /// 修改或创建指定全景摄像机信息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="panoCamera"></param>
        /// <returns></returns>
        tFault SetPanoCamera(Howell5198Session session, tPanoCamera panoCamera);
        /// <summary>
        /// 删除指定全景摄像机信息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="panoCameraId"></param>
        /// <returns></returns>
        tFault DeletePanoCamera(Howell5198Session session, tPanoCameraId panoCameraId);
        /// <summary>
        /// 获取服务程序版本信息
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        tServiceVersion GetServiceVersion(Howell5198Session session);
        /// <summary>
        /// 查询设备信息
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        tDeviceInfo GetDeviceInfo(Howell5198Session session);
        /// <summary>
        /// 查询设备状态
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        tDeviceStatus GetDeviceStatus(Howell5198Session session);
        /// <summary>
        /// 查询网口信息
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        tNetworkInterface GetNetworkInterface(Howell5198Session session);
        /// <summary>
        /// 查询解码单元列表
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        tDecodingUnitList GetDecodingUnitList(Howell5198Session session);
        /// <summary>
        /// 查询解码单元
        /// </summary>
        /// <param name="session"></param>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        tDecodingUnit GetDecodingUnit(Howell5198Session session, tDecodingUnitId decodingUnitId);
        /// <summary>
        /// 获取解码单元播放器视频水平旋转速度
        /// </summary>
        /// <param name="session"></param>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        tRotatingSpeed GetRotatingSpeed(Howell5198Session session, tDecodingUnitId decodingUnitId);
        /// <summary>
        /// 设置解码单元播放器视频水平旋转速度
        /// </summary>
        /// <param name="session"></param>
        /// <param name="rotatingSpeed"></param>
        /// <returns></returns>
        tFault SetRotatingSpeed(Howell5198Session session, tRotatingSpeed rotatingSpeed);
        /// <summary>
        /// 切换解码单元显示的全景摄像机
        /// </summary>
        /// <param name="session"></param>
        /// <param name="switchPanoCameraRequest"></param>
        /// <returns></returns>
        tFault SwitchPanoCamera(Howell5198Session session, SwitchPanoCameraRequest switchPanoCameraRequest);
        /// <summary>
        /// 获取解码单元显示的全景摄像机视角
        /// </summary>
        /// <param name="session"></param>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        tViewPoint GetViewPoint(Howell5198Session session, tDecodingUnitId decodingUnitId);
        /// <summary>
        /// 修改解码单元显示的全景摄像机视角
        /// </summary>
        /// <param name="session"></param>
        /// <param name="setViewPointRequest"></param>
        /// <returns></returns>
        tFault SetViewPoint(Howell5198Session session, SetViewPointRequest setViewPointRequest);
        /// <summary>
        /// 锁定或解锁解码单元显示的全景摄像机视角
        /// </summary>
        /// <param name="session"></param>
        /// <param name="setViewPointFixedRequest"></param>
        /// <returns></returns>
        tFault SetViewPointFixed(Howell5198Session session, SetViewPointFixedRequest setViewPointFixedRequest);
        /// <summary>
        /// 修改解码单元显示的全景摄像机的显示列数量
        /// </summary>
        /// <param name="session"></param>
        /// <param name="setViewPointRowsRequest"></param>
        /// <returns></returns>
        tFault SetViewPointRows(Howell5198Session session, SetViewPointRowsRequest setViewPointRowsRequest);
        /// <summary>
        /// 获取解码单元的播放器状态
        /// </summary>
        /// <param name="session"></param>
        /// <param name="decodingUnitId"></param>
        /// <returns></returns>
        tPlayerStatus GetPlayerStatus(Howell5198Session session, tDecodingUnitId decodingUnitId);
        /// <summary>
        /// 单帧进
        /// </summary>
        /// <param name="session"></param>
        /// <param name="oneByOneRequest"></param>
        /// <returns></returns>
        tFault OneByOne(Howell5198Session session, OneByOneRequest oneByOneRequest);
        /// <summary>
        /// 暂停播放
        /// </summary>
        /// <param name="session"></param>
        /// <param name="pauseRequest"></param>
        /// <returns></returns>
        tFault Pause(Howell5198Session session, PauseRequest pauseRequest);
        /// <summary>
        /// 恢复播放
        /// </summary>
        /// <param name="session"></param>
        /// <param name="resumeRequest"></param>
        /// <returns></returns>
        tFault Resume(Howell5198Session session, ResumeRequest resumeRequest);
        /// <summary>
        /// 定位播放器进度
        /// </summary>
        /// <param name="session"></param>
        /// <param name="seekRequest"></param>
        /// <returns></returns>
        tFault Seek(Howell5198Session session, SeekRequest seekRequest);
    }
}
