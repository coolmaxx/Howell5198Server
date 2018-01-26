using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howell5198.Protocols
{
    /// <summary>
    /// 重启设备的应答
    /// </summary>
    public class RestartDeviceResponse : StreamResponse { }
    /// <summary>
    /// 关闭设备的应答
    /// </summary>
    public class CloseDeviceResponse : StreamResponse { }
    /// <summary>
    /// 重置设备的应答
    /// </summary>
    public class ResetDeviceResponse : StreamResponse { }
}
