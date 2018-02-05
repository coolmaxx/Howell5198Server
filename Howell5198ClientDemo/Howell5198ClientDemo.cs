using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Howell5198;
using Howell5198.Protocols;
using System.Threading;
using System.IO;

namespace Howell5198ClientDemo
{
   public class Howell5198ClientTest
    {
       public Howell5198ClientTest(string ipAddress, int port)
        {
            m_howell5198client = new Howell5198Client(ipAddress,port);
            m_howell5198client.Identity = "Howell5198ClientTest";
        }
        private Howell5198Client m_howell5198client;

        public void Run()
        {
            LoginTest("admin", "12345");
           //StreamTest();
            //ColorTest();
            //OsdChannelTest();
            //OsdDateTest();
            //VideoQualityTest();
            //StreamTypeTest();
            //NetInfoTest();
            //SystemTimeTest();
            //Rs232CfgTest();
            //PtzRs232CfgTest();
            //PtzControlTest();
            //GetNetHeadTest();
            //GetFileTest();
            //DeviceControlTest();
            //GetDevCfgTest();
            //GetUserTest();

            NtpServerTest();
            DeviceSystemTest();
            DeviceNetWorkTest();
            PanoCameraTest();
            DecodingUnitTest();
            PlaybackTest();
            m_howell5198client.Close();
        }
        void LoginTest(string userName, string password)
        {
            ServerInfo serverinfo = m_howell5198client.Connect("admin", "12345");
            //ServerInfo serverinfo2 = m_howell5198client2.Connect("admin", "12345");
             //ServerInfo serverinfo3=m_howell5198client3.Connect("admin", "12345");
            //m_howell5198client4.Connect("admin", "12345");
            //m_howell5198client5.Connect("admin", "12345");
            if(serverinfo==null)
            {
                Console.WriteLine("Connect error");
                return;
            }
            Console.WriteLine("serverinfo:  SlotCount {0} sverVersion {1}", serverinfo.SlotCount, serverinfo.SverVersion);
        }

        void StreamTest()
        {
            Howell5198Stream mainstream = m_howell5198client.CreateStream(0, 0);
            Howell5198Stream mainstream2 = m_howell5198client.CreateStream(1, 0);
            mainstream.StreamDataReceived += new EventHandler<StreamDataReceivedEventArgs>(howell5198client_StreamDataReceived);
            mainstream2.StreamDataReceived += new EventHandler<StreamDataReceivedEventArgs>(howell5198client_StreamDataReceived);
            mainstream.StartReceive();
            mainstream2.StartReceive();
            Thread.Sleep(3000);
            mainstream.StopReceive();
            mainstream2.StopReceive();
        }
        void ColorTest()
        {
           ColorInfo colorInfo = m_howell5198client.GetColor(0);
           if (colorInfo == null)
           {
               Console.WriteLine("GetColor error");
               return;
           }
           Console.WriteLine("colorInfo: Slot {0} Brightness {1}", colorInfo.Slot, colorInfo.Brightness);
           SetColorResponse setColorResponse = m_howell5198client.SetColor(colorInfo);
           if (setColorResponse == null)
          {
              Console.WriteLine("SetColor error");
              return;
          }
           Console.WriteLine("setColorResponse: Success {0} ", setColorResponse.Success);
        }
       void OsdChannelTest()
        {
            OsdChannelInfo osdChannelInfo = m_howell5198client.GetOsdChannel(0);
            if (osdChannelInfo == null)
            {
                Console.WriteLine("GetOsdChannel error");
                return;
            }
            Console.WriteLine("osdChannelInfo: Left {0} Top {1} Name {2}", osdChannelInfo.Left, osdChannelInfo.Top, osdChannelInfo.Name);
            SetOsdChannelResponse setOsdChannelResponse = m_howell5198client.SetOsdChannel(osdChannelInfo);
            if (setOsdChannelResponse == null)
            {
                Console.WriteLine("setOsdChannelResponse error");
                return;
            }
            Console.WriteLine("setOsdChannelResponse: Success {0} ", setOsdChannelResponse.Success);
        }
       void OsdDateTest()
       {
           OsdDateInfo osdDateInfo = m_howell5198client.GetOsdDate(0);
           if (osdDateInfo == null)
           {
               Console.WriteLine("GetOsdDate error");
               return;
           }
           Console.WriteLine("osdDateInfo: Left {0} Top {1}", osdDateInfo.Left, osdDateInfo.Top);
           SetOsdDateResponse setOsdDateResponse = m_howell5198client.SetOsdDate(osdDateInfo);
           if (setOsdDateResponse == null)
           {
               Console.WriteLine("setOsdDateResponse error");
               return;
           }
           Console.WriteLine("setOsdDateResponse: Success {0} ", setOsdDateResponse.Success);
       }
       void VideoQualityTest()
       {
           VideoQualityInfo videoQualityInfo = m_howell5198client.GetVideoQuality(0);
           if (videoQualityInfo == null)
           {
               Console.WriteLine("GetVideoQuality error");
               return;
           }
           Console.WriteLine("videoQualityInfo:EncodeType {0} MaxBps {1} Framerate {2}", videoQualityInfo.EncodeType, videoQualityInfo.MaxBps, videoQualityInfo.Framerate);
           SetVideoQualityResponse setVideoQualityResponse = m_howell5198client.SetVideoQuality(videoQualityInfo);
           if (setVideoQualityResponse == null)
           {
               Console.WriteLine("SetVideoQuality error");
               return;
           }
           Console.WriteLine("setVideoQualityResponse: Success {0} ", setVideoQualityResponse.Success);
       }
       void StreamTypeTest()
       {
           StreamTypeInfo streamTypeInfo = m_howell5198client.GetStreamType(0);
           if (streamTypeInfo == null)
           {
               Console.WriteLine("GetStreamType error");
               return;
           }
           Console.WriteLine("streamTypeInfo:Slot {0} Type {1}", streamTypeInfo.Slot, streamTypeInfo.Type);
           SetStreamTypeResponse setStreamTypeResponse = m_howell5198client.SetStreamType(streamTypeInfo);
           if (setStreamTypeResponse == null)
           {
               Console.WriteLine("SetStreamType error");
               return;
           }
           Console.WriteLine("setStreamTypeResponse: Success {0} ", setStreamTypeResponse.Success);
       }
       void NetInfoTest()
       {
           NetInfo netInfo = m_howell5198client.GetNetInfo();
           if (netInfo == null)
           {
               Console.WriteLine("GetNetInfo error");
               return;
           }
           Console.WriteLine("netInfo:Gateway {0} SDvrIp {1} Port {2}", netInfo.Gateway, netInfo.SDvrIp, netInfo.Port);
           SetNetInfoResponse setNetInfoResponse = m_howell5198client.SetNetInfo(netInfo);
           if (setNetInfoResponse == null)
           {
               Console.WriteLine("SetNetInfo error");
               return;
           }
           Console.WriteLine("setNetInfoResponse: Success {0} ", setNetInfoResponse.Success);
       }
       void SystemTimeTest()
       {
           SystemTimeInfo systemTimeInfo = m_howell5198client.GetSystemTime();
           if (systemTimeInfo == null)
           {
               Console.WriteLine("GetSystemTime error");
               return;
           }
           String str = string.Format("{0:G}", new System.DateTime(systemTimeInfo.WYear, systemTimeInfo.WMonth, systemTimeInfo.WDay, systemTimeInfo.WHour, systemTimeInfo.WMinute, systemTimeInfo.WSecond,systemTimeInfo.WMilliseconds));
            Console.WriteLine("systemTimeInfo: "+str);
           SetSystemTimeResponse setSystemTimeResponse = m_howell5198client.SetSystemTime(systemTimeInfo);
           if (setSystemTimeResponse == null)
           {
               Console.WriteLine("SetSystemTime error");
               return;
           }
           Console.WriteLine("setSystemTimeResponse: Success {0} ", setSystemTimeResponse.Success);
       }
       void DeviceControlTest()
       {
           RestartDeviceResponse restartDeviceResponse = m_howell5198client.RestartDevice();
           if (restartDeviceResponse == null)
           {
               Console.WriteLine("RestartDevice error");
               return;
           }
           Console.WriteLine("restartDeviceResponse: Success {0} ", restartDeviceResponse.Success);

           //CloseDevice()和ResetDevice()与此只有协议号的区别，未测
       }
       void Rs232CfgTest()
       {
           Rs232CfgInfo rs232CfgInfo = m_howell5198client.GetRs232Cfg(new GetRs232CfgRequest() { Rs232_no = 0 });
           if (rs232CfgInfo == null)
           {
               Console.WriteLine("GetRs232Cfg error");
               return;
           }
           Console.WriteLine("rs232CfgInfo: Rs232_no {0}  Rate {1}  Data_bit {2}", rs232CfgInfo.Rs232_no, rs232CfgInfo.Rate, rs232CfgInfo.Data_bit);
           SetRs232CfgResponse setRs232CfgResponse = m_howell5198client.SetRs232Cfg(rs232CfgInfo);
           if (setRs232CfgResponse == null)
           {
               Console.WriteLine("SetRs232Cfg error");
               return;
           }
           Console.WriteLine("setRs232CfgResponse: Success {0} ", setRs232CfgResponse.Success);
       }
       void PtzRs232CfgTest()
       {
           PtzRs232CfgInfo ptzRs232CfgInfo = m_howell5198client.GetPtzRs232Cfg(0);
           if (ptzRs232CfgInfo == null)
           {
               Console.WriteLine("GetPtzRs232Cfg error");
               return;
           }
           Console.WriteLine("ptzRs232CfgInfo: Rate {0}  Data_bit {1}  address {2}", ptzRs232CfgInfo.Rate, ptzRs232CfgInfo.Data_bit, ptzRs232CfgInfo.Address);
           SetPtzRs232CfgResponse setPtzRs232CfgResponse = m_howell5198client.SetPtzRs232Cfg(ptzRs232CfgInfo);
           if (setPtzRs232CfgResponse == null)
           {
               Console.WriteLine("SetPtzRs232Cfg error");
               return;
           }
           Console.WriteLine("setPtzRs232CfgResponse: Success {0} ", setPtzRs232CfgResponse.Success);
       }
       void PtzControlTest()
       {
           PtzControlRequest ptzControlRequest = new PtzControlRequest();
           ptzControlRequest.Cmd = 8;
           PtzControlResponse ptzControlResponse = m_howell5198client.PtzControl(ptzControlRequest);
           if (ptzControlResponse == null)
           {
               Console.WriteLine("PtzControl error");
               return;
           }
           Console.WriteLine("ptzControlResponse: Success {0} ", ptzControlResponse.Success);
       }

       void GetNetHeadTest()
       {
           GetNetHeadRequest getNetHeadRequest = new GetNetHeadRequest();
           GetNetHeadResponse getNetHeadResponse = m_howell5198client.GetNetHead(getNetHeadRequest);
           if (getNetHeadResponse == null)
           {
               Console.WriteLine("GetNetHead error");
               return;
           }
           Console.WriteLine("getNetHeadResponse: len {0}", getNetHeadResponse.Len);
       }
       void GetFileTest()
       {
           SearchFileRequest searchFileRequest=new SearchFileRequest(){ChannelNo=0,Beg=new SystemTimeInfo(2017,7,4,8,0,0),End=new SystemTimeInfo(2017,7,4,13,0,0),Type=0};
           SearchFileResponse searchFileResponse=m_howell5198client.SearchFile(searchFileRequest);
            if (searchFileResponse == null)
           {
               Console.WriteLine("SearchFile error");
               return;
           }
           Console.WriteLine("searchFileResponse: filecount:{0}", searchFileResponse.FileInfos.Length);
           if(searchFileResponse.FileInfos.Length>0)//搜索到录像文件的话
           {
               GetFileInfoRequest getFileInfoRequest = new GetFileInfoRequest();
               getFileInfoRequest.ChannelNo = searchFileResponse.FileInfos[0].ChannelNo;
               getFileInfoRequest.Beg = searchFileResponse.FileInfos[0].Beg;
               getFileInfoRequest.End = searchFileResponse.FileInfos[0].End;
               GetFileInfoResponse getFileInfoResponse = m_howell5198client.GetFileInfo(getFileInfoRequest);
               if (getFileInfoResponse == null)
               {
                   Console.WriteLine("GetFileInfo error");
                   return;
               }
               Console.WriteLine("getFileInfoResponse: FileFormatType:{0}", getFileInfoResponse.FileFormatType);
               //构建HW头
               HW_MediaInfo media = new HW_MediaInfo();
               media.Dvr_version = getFileInfoResponse.FileFormatType;
               media.Adec_code = getFileInfoResponse.Audio_dec;
               media.Vdec_code = getFileInfoResponse.Video_dec;
               media.Reserved[0] = (8 << 24) + (8 << 16) + (1 << 8) + 0;//au_bits+au_sampleau_sample+au_channel+reserve
             
               GetFileRequest getFileRequest=new GetFileRequest();
               getFileRequest.ChannelNo = searchFileResponse.FileInfos[0].ChannelNo;
               getFileRequest.Beg = searchFileResponse.FileInfos[0].Beg;
               getFileRequest.End = searchFileResponse.FileInfos[0].End;
               getFileRequest.Type = searchFileResponse.FileInfos[0].Type;
               Howell5198FileStream filestream = m_howell5198client.GetFile(getFileRequest);
               filestream.FileDataReceived += howell5198client_FileDataReceived;
               filestream.StartReceive();
               Thread.Sleep(1000);
               filestream.StopReceive();
           }
       }
       void GetDevCfgTest()
       {
           DeviceConfig devcfg = m_howell5198client.GetDevCfg();
           if (devcfg == null)
           {
               Console.WriteLine("GetDevCfg error");
               return;
           }
           Console.WriteLine("DeviceConfig: DevName:{0} DevSerialID:{1} Channel1:{2} Channel2:{3}", devcfg.DevName, devcfg.DevSerialID, devcfg.ChannelName[0], devcfg.ChannelName[1]);
       }
       void GetUserTest()
       {
           DavinciUsers users = m_howell5198client.GetUser();
           if (users == null)
           {
               Console.WriteLine("GetUser error");
               return;
           }
           Console.WriteLine("TempestUsers: Count:{0} Name:{1} Password:{2}", users.Count, users.Users[0].Name, users.Users[0].Password);
       }
       
       void NtpServerTest()
       {
           try
           {
              NtpInfo ntpinfo = m_howell5198client.GetNtpInfo();
              Console.WriteLine("ntpinfo svrip:{0} Cycletime:{1} Timezone:{2}", ntpinfo.SvrIp, ntpinfo.Cycletime, System.Text.Encoding.ASCII.GetString(ntpinfo.Timezone));
           }
           catch (Exception ex)
           {
               Console.WriteLine(String.Format("GetNtpInfo error.{0}", ex.Message));
           }
          
       }
       void DeviceSystemTest()
       {
           try
           {
               tServiceVersion serviceVersion = m_howell5198client.GetServiceVersion();
               Console.WriteLine("serviceVersion Version:{0} BuildDate:{1} Company:{2}", serviceVersion.Version, serviceVersion.BuildDate, serviceVersion.Company);
               tDeviceInfo deviceInfo = m_howell5198client.GetDeviceInfo();
               Console.WriteLine("deviceInfo Id:{0} Name:{1} SerialNumber:{2} Model:{3}", deviceInfo.Id, deviceInfo.Name, deviceInfo.SerialNumber, deviceInfo.Model);
               tDeviceStatus deviceStatus = m_howell5198client.GetDeviceStatus();
               Console.WriteLine("deviceStatus CurrentTime:{0} SystemUpTime:{1}", deviceStatus.CurrentTime, deviceStatus.SystemUpTime);
           }
           catch (Exception ex)
           {
               Console.WriteLine(String.Format("DeviceSystemTest error.{0}", ex.Message));
           }
       }
       void DeviceNetWorkTest()
       {
           try 
           {
               tNetworkInterface networkInterface = m_howell5198client.GetNetworkInterface();
               Console.WriteLine("networkInterface Address:{0} MTU:{1} PhysicalAddress:{2}", networkInterface.Address, networkInterface.MTU, networkInterface.PhysicalAddress);
           }
             catch(Exception ex)
           {
               Console.WriteLine(String.Format("DeviceNetWorkTest error.{0}", ex.Message));
           }
       }

       void PanoCameraTest()
         {
             try
             {
                 tPanoCameraList panoCameraList = m_howell5198client.GetPanoCameraList(new tQueryString() { });
                 Console.WriteLine("panoCameraList PanoCamera_count:{0}", panoCameraList.PanoCamera_count);
                 if(panoCameraList.PanoCamera_count>0)
                 {
                     tPanoCamera panoCamera = m_howell5198client.GetPanoCamera(panoCameraList.PanoCameras[0].Id);
                     Console.WriteLine("panoCamera1 Id:{0} Name:{1} Model:{2}", panoCamera.Id, panoCamera.Name, panoCamera.Model);
                     tFault fault=m_howell5198client.SetPanoCamera(panoCamera);
                     Console.WriteLine("SetPanoCamera FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
                     panoCamera.Id="addtest";
                     panoCamera.Name="addtest";
                     fault = m_howell5198client.AddPanoCamera(panoCamera);
                     Console.WriteLine("AddPanoCamera FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
                     fault = m_howell5198client.DeletePanoCamera(panoCamera.Id);
                     Console.WriteLine("DeletePanoCamera FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine(String.Format("PanoCameraTest error.{0}", ex.Message));
             }
         }
       void DecodingUnitTest()
       {
           try
           {
               tDecodingUnitList decodingUnitList = m_howell5198client.GetDecodingUnitList();
               Console.WriteLine("decodingUnitList DecodingUnit_count:{0}", decodingUnitList.DecodingUnit_count);
               if (decodingUnitList.DecodingUnit_count > 0)
               {
                   tDecodingUnit decodingUnit = m_howell5198client.GetDecodingUnit(decodingUnitList.DecodingUnits[0].Id);
                   Console.WriteLine("decodingUnit1 Id:{0} Name:{1} PanoCameraId:{2}", decodingUnit.Id, decodingUnit.Name, decodingUnit.PanoCameraId);
                   tRotatingSpeed rotatingSpeed = m_howell5198client.GetRotatingSpeed(decodingUnitList.DecodingUnits[0].Id);
                   Console.WriteLine("GetRotatingSpeed RotatingSpeed:{0} ", rotatingSpeed.RotatingSpeed);
                   tFault fault= m_howell5198client.SetRotatingSpeed(rotatingSpeed);
                   Console.WriteLine("SetRotatingSpeed FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
                   SwitchPanoCameraRequest switchPanoCameraRequest = new SwitchPanoCameraRequest() { DecodingUnitId = decodingUnit.Id, PanoCameraId = decodingUnit.PanoCameraId };
                   fault = m_howell5198client.SwitchPanoCamera(switchPanoCameraRequest);
                   Console.WriteLine("SwitchPanoCamera FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);

                   tViewPoint viewPoint = m_howell5198client.GetViewPoint(decodingUnit.Id);
                   Console.WriteLine("GetViewPoint viewPoint Angle:{0} Distance:{1}", viewPoint.Angle, viewPoint.Distance);
                   SetViewPointRequest setViewPointRequest=new SetViewPointRequest(){DecodingUnitId=decodingUnit.Id,ViewPoint=viewPoint};
                   fault = m_howell5198client.SetViewPoint(setViewPointRequest);
                   Console.WriteLine("SetViewPoint FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
               }
           }
           catch (Exception ex)
           {
               Console.WriteLine(String.Format("DecodingUnitTest error.{0}", ex.Message));
           }
       }
       void PlaybackTest()
       {
           try
           {
               tPlayerStatus playerStatus = m_howell5198client.GetPlayerStatus("testid");
               Console.WriteLine("GetPlayerStatus playerStatus Duration:{0} Seekable:{1}", playerStatus.Duration, playerStatus.Seekable);
               tFault fault = m_howell5198client.OneByOne(new OneByOneRequest());
               Console.WriteLine("OneByOne FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
               fault = m_howell5198client.Pause(new PauseRequest());
               Console.WriteLine("Pause FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
               fault = m_howell5198client.Resume(new ResumeRequest());
               Console.WriteLine("Resume FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
               fault = m_howell5198client.Seek(new SeekRequest());
               Console.WriteLine("Seek FaultCode:{0} FaultReason:{1}", fault.FaultCode, fault.FaultReason);
             
           }
           catch (Exception ex)
           {
               Console.WriteLine(String.Format("DecodingUnitTest error.{0}", ex.Message));
           }
       }
        static void howell5198client_StreamDataReceived(object sender, StreamDataReceivedEventArgs e)
        {
            Console.WriteLine("DataReceived: ChannelNo:{0} Group_Name:{1} FrameDataLen:{2}", ((Howell5198Stream)sender).ChannelNo, e.FrameType, e.Data.Length);
        }

        static void howell5198client_FileDataReceived(object sender, FileDataReceivedEventArgs e)
        {
            if(e.FileData.Type==0)
            {
                if (e.FileData.Datalen != 100)
                    Console.WriteLine("data error.");
                TimeLabel timelabel = new TimeLabel();
                timelabel.FromBytes(e.FileData.Buffer, 0, 100);
                Console.WriteLine("FileLen:{0}",timelabel.FileLen);
            }
            Console.WriteLine("FileDataReceived: ChannelNo:{0} DataLen:{1}", ((Howell5198FileStream)sender).ChannelNo,e.FileData.Datalen);
        }
    }
}
