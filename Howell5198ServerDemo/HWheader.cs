using Howell5198.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HW5198Service
{
    class HWheader
    {
        public HWheader()
        {
            Header = null;
            DisconnectTime = new DateTime(1970, 1, 1);
            IsOnLine = false;
        }
        /// <summary>
        /// HW头结构
        /// </summary>
       public HW_MediaInfo  Header { get; set; }

        /// <summary>
       /// 对应流的下线时间
        /// </summary>
       public DateTime DisconnectTime { get;  set; }
        /// <summary>
        /// 对应流是否在线
        /// </summary>
       public Boolean IsOnLine { get; set; }
    }
}
