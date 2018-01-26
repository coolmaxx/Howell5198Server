using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howell5198.Protocols
{
    public interface IBytesSerialize
    {
        /// <summary>
        /// 获取结构长度
        /// </summary>
        /// <returns></returns>
        int GetLength();

       /// <summary>
        /// 读取数据
       /// </summary>
       /// <param name="buffer"></param>
       /// <param name="offset"></param>
       /// <param name="length"></param>
        void FromBytes(byte[] buffer,int offset, int length);
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <returns></returns>
        byte[] GetBytes();
    }
}
