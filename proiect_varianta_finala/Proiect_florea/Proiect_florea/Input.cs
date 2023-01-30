using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_florea
{
    //clasa pentru verificare daca input este corect sau nu
    class Input
    {
        #region comboboxes
        public static bool FRcorect { get; set; } = true;
        public static bool IRmaxcorect { get; set; } = true;
        public static bool IBScorect { get; set; } = true;
        public static bool MissCachecorect { get; set; } = true;
        public static bool RegSetNumbercorect { get; set; } = true;
        public static bool BlockSizeInstrcorect { get; set; } = true;
        public static bool SizeICcorect { get; set; } = true;
        public static bool BlockSizeDatacorect { get; set; } = true;
        public static bool SizeDCcorect { get; set; } = true;

        #endregion


        #region additionalConditions

        //IBS >= FR + IR -- conditia1
        //IBS >= FR -- conditia2
        //IR <= FR -- conditia3

        public static bool ConditiaUnu { get; set; } = true;
        public static bool ConditiaDoi { get; set; } = true;
        public static bool ConditiaTrei { get; set; } = true;

        #endregion

        public static bool ValuesValid()
        {
            return FRcorect && IRmaxcorect && IBScorect && MissCachecorect && RegSetNumbercorect && ((BlockSizeInstrcorect
                && SizeICcorect) || (BlockSizeDatacorect && SizeDCcorect)) && ConditiaUnu && ConditiaDoi && ConditiaTrei;
        }
    }
}
