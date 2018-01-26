using Howell.Net.DeviceService;
using Howell.Net.DeviceService.DecodingUnits;
using Howell.Net.DeviceService.PanoCameras;
using Howell5198.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HW5198Service
{
    class ConvertClass
    {
        public static tDecodingUnit Convert(DecodingUnit decodingUnit)
        {
            tDecodingUnit tdecodingUnit = new tDecodingUnit() { Id = decodingUnit.Id };
            if (decodingUnit.Name != null)
            {
                tdecodingUnit.Name_enabled = 1;
                tdecodingUnit.Name = decodingUnit.Name;
            }
            if (decodingUnit.PanoCameraId != null)
            {
                tdecodingUnit.PanoCameraId_enabled = 1;
                tdecodingUnit.PanoCameraId = decodingUnit.PanoCameraId;
            }
            if (decodingUnit.Position != null)
            {
                tdecodingUnit.Position_enabled = 1;
                tdecodingUnit.Position = new tPosition() { X = decodingUnit.Position.X, Y = decodingUnit.Position.Y };
            }
            if (decodingUnit.Resolution != null)
            {
                tdecodingUnit.Resolution_enabled = 1;
                tdecodingUnit.Resolution = new tResolution() { Height = decodingUnit.Resolution.Height, Width = decodingUnit.Resolution.Width };
            }
            if (decodingUnit.DisplayDeviceId != null)
            {
                tdecodingUnit.DisplayDeviceId_enabled = 1;
                tdecodingUnit.DisplayDeviceId = decodingUnit.DisplayDeviceId;
            }
            return tdecodingUnit;
        }

        public static tPanoCamera Convert(PanoCamera panoCamera)
        {
            tPanoCamera tpanoCamera = new tPanoCamera()
            {
                GroupId = panoCamera.GroupId,
                Name = panoCamera.Name,
                Model = panoCamera.Model,
                MainCameraBuildInId = panoCamera.MainCameraBuildInId,
                WiperCameraBuildInId = panoCamera.WiperCameraBuildInId,
                BuildInCamera_count = panoCamera.BuildInCamera.Length
            };
            if (panoCamera.BuildInCamera.Length > 0)
            {
                tpanoCamera.BuildInCameras = new tBuildInCamera[panoCamera.BuildInCamera.Length];
                for (int i = 0; i < panoCamera.BuildInCamera.Length; ++i)
                {
                    tpanoCamera.BuildInCameras[i] = ConvertClass.Convert(panoCamera.BuildInCamera[i]);
                }
            }
            if (panoCamera.Id != null)
            {
                tpanoCamera.Id_enabled = 1;
                tpanoCamera.Id = panoCamera.Id;
                tpanoCamera.IsAllOnline = (bool)panoCamera.IsAllOnline ? 1 : 0;
                tpanoCamera.IsCompleted = (bool)panoCamera.IsCompleted ? 1 : 0;
            }
            if (panoCamera.DefaultViewPoint != null)
            {
                tpanoCamera.DefaultViewPoint_enabled = 1;
                tpanoCamera.DefaultViewPoint = ConvertClass.Convert(panoCamera.DefaultViewPoint);
            }
            if (panoCamera.ExistedInDatabase != null)
            {
                tpanoCamera.ExistedInDatabase_enabled = 1;
                tpanoCamera.ExistedInDatabase = (bool)panoCamera.ExistedInDatabase ? 1 : 0;
            }
            return tpanoCamera;
        }
        public static tBuildInCamera Convert(BuildInCamera buildInCamera)
        {
            return new tBuildInCamera();
        }
        public static tViewPoint Convert(ViewPoint viewPoint)
        {
            tViewPoint tviewPoint = new tViewPoint() { Angle = viewPoint.Angle, Distance = viewPoint.Distance, Height = viewPoint.Height };
            if (viewPoint.Rows != null)
            {
                tviewPoint.Rows_enabled = 1;
                tviewPoint.Rows = (int)viewPoint.Rows;
            }
            if (viewPoint.Fixed != null)
            {
                tviewPoint.Fixed_enabled = 1;
                tviewPoint.Fixed = (bool)viewPoint.Fixed ? 1 : 0;
            }
            return tviewPoint;
        }
        public static ViewPoint Convert(tViewPoint tviewPoint)
        {
            ViewPoint viewPoint = new ViewPoint() { Angle = tviewPoint.Angle, Distance = tviewPoint.Distance, Height = tviewPoint.Height };
            if (tviewPoint.Rows_enabled == 1)
            {
                viewPoint.Rows = tviewPoint.Rows;
            }
            if (tviewPoint.Fixed_enabled == 1)
            {
                viewPoint.Fixed = tviewPoint.Fixed == 0 ? false : true;
            }
            return viewPoint;
        }
        public static tPanoCameraList Convert(PanoCameraList panoCameraList)
        {
            tPanoCameraList tpanoCameraList = new tPanoCameraList() { PanoCamera_count = panoCameraList.PanoCamera.Length };
            if (panoCameraList.Page != null)
            {
                tpanoCameraList.Page_enabled = 1;
                tpanoCameraList.Page = new tPage()
                {
                    PageCount = panoCameraList.Page.PageCount,
                    PageIndex = panoCameraList.Page.PageIndex,
                    PageSize = panoCameraList.Page.PageSize,
                    RecordCount = panoCameraList.Page.RecordCount,
                    TotalRecordCount = panoCameraList.Page.TotalRecordCount
                };
            }
            if (panoCameraList.PanoCamera.Length > 0)
            {
                tpanoCameraList.PanoCameras = new tPanoCamera[panoCameraList.PanoCamera.Length];
                for (int i = 0; i < panoCameraList.PanoCamera.Length; ++i)
                {
                    tpanoCameraList.PanoCameras[i] = ConvertClass.Convert(panoCameraList.PanoCamera[i]);
                }
            }
            return tpanoCameraList;
        }
    }
}
