using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileCartographer.Library
{
    public static class Data
    {
        /// <summary>
        /// "Reference Data" is a matrix containing quartile indices referring to quartiles in a multitile image.
        /// This matrix is used to efficently generate a reference sheet from any 3x4 autotile.
        /// </summary>
        /// <returns>The quartile index matrix.</returns>
        public static byte[,] GetReferenceData3x4()
        {
            return new byte[,]
            {
            {12,17,36,41,12,13,36,05,12,17,24,29,12,13,24,05},
            {42,47,42,47,42,43,42,43,18,23,30,35,18,11,30,11},
            {16,17,04,41,14,15,04,05,16,17,04,29,14,15,04,05},
            {46,47,46,47,44,45,44,45,10,23,10,35,10,11,10,11},
            {36,37,12,13,24,25,24,05,24,25,40,41,04,39,38,05},
            {42,43,18,19,30,11,30,31,30,31,46,47,44,45,44,45},
            {38,39,16,17,04,29,28,29,28,29,14,15,14,15,14,15},
            {44,45,22,23,34,35,10,35,34,35,10,21,20,11,20,21},
            {04,27,04,05,04,27,04,05,04,27,04,05,04,27,26,05},
            {10,11,10,33,10,33,32,11,32,11,32,33,32,33,10,11},
            {26,27,26,05,26,27,26,05,26,27,26,05,26,27,00,01},
            {10,11,10,33,10,33,32,11,32,11,32,33,32,33,06,07},
            };
        }

        /// <summary>
        /// These masks are used to reduce the diagonal sum such that it only contains significant bits.
        /// The mask is chosen based on the hSum and applied to the dSum.
        /// If no mask is linked to a hSum, the dSum is ignored.
        /// </summary>
        /// <returns>The mask dictionary.</returns>
        public static Dictionary<byte, byte> GetMaskDict()
        {
            //<hSum, dMask>
            var dic = new Dictionary<byte, byte>();

            dic.Add(03, 01);
            dic.Add(06, 02);
            dic.Add(07, 03);
            dic.Add(09, 08);
            dic.Add(11, 09);
            dic.Add(12, 04);
            dic.Add(13, 12);
            dic.Add(14, 06);
            dic.Add(15, 15);

            return dic;
        }

        /// <summary>
        /// This dictionary allows us to find the reference index needed for a multitile with a certain neighbour sum.
        /// This dictionary only contains indices for combined sums (hSum + bitshifted masked dSum).
        /// </summary>
        /// <returns></returns>
        public static Dictionary<byte, byte> GetReferenceDict()
        {
            //<sum, index>
            var dic = new Dictionary<byte, byte>();

            dic.Add(019, 16);
            dic.Add(038, 17);
            dic.Add(023, 18);
            dic.Add(039, 19);
            dic.Add(055, 20);
            dic.Add(137, 21);
            dic.Add(027, 22);
            dic.Add(139, 23);
            dic.Add(155, 24);
            dic.Add(076, 25);
            dic.Add(077, 26);
            dic.Add(141, 27);
            dic.Add(205, 28);
            dic.Add(046, 29);
            dic.Add(078, 30);
            dic.Add(110, 31);
            dic.Add(031, 32);
            dic.Add(047, 33);
            dic.Add(063, 34);
            dic.Add(079, 35);
            dic.Add(095, 36);
            dic.Add(111, 37);
            dic.Add(127, 38);
            dic.Add(143, 39);
            dic.Add(159, 40);
            dic.Add(175, 41);
            dic.Add(191, 42);
            dic.Add(207, 43);
            dic.Add(223, 44);
            dic.Add(239, 45);
            dic.Add(255, 46);

            return dic;
        }
    }
}
