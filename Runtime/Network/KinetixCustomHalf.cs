using UnityEngine;

namespace Kinetix.Internal
{
    public class KinetixCustomHalf
    {
        //=======================================================================//
        // Custom Half                                                           //
        //-----------------------------------------------------------------------//
        // Converter from float to short with a precision of 3 digits            //
        //                                                                       //
        //=======================================================================//
        #region CUSTOM_HALF
        private const int CUSTOM_HALF = 1000;
        public static short FloatToCustomHalf(float @float)
        {
            @float = Mathf.Clamp(@float, short.MinValue / CUSTOM_HALF, short.MaxValue / CUSTOM_HALF);

            return (short)(@float * CUSTOM_HALF);
        }

        public static float CustomHalfToFloat(short customHalf)
        {
            float toReturn = customHalf;
            return toReturn / CUSTOM_HALF;
        }
        #endregion
    }
}
