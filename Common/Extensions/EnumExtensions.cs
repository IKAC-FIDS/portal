using System;

namespace TES.Common.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Convert enum to int32
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static int ToInt32(this Enum @enum)
        {
            return Convert.ToInt32(@enum);
        }

        /// <summary>
        /// Convert enum to int16
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static short ToInt16(this Enum @enum)
        {
            return Convert.ToInt16(@enum);
        }

        /// <summary>
        /// Convert enum to int64
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static long ToInt64(this Enum @enum)
        {
            return Convert.ToInt64(@enum);
        }

        /// <summary>
        /// Convert enum to byte
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static byte ToByte(this Enum @enum)
        {
            return Convert.ToByte(@enum);
        }
    }
}
