using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_florea
{
    public class Instructiune
    {
        public InstructionType? instructionType { get; set; }
        public int currentPC { get; set; }
        public int targetAddress { get; set; }
    }
    public enum InstructionType
    {
        Branch,
        Load,
        Store,
        Arithmetic
    }
}
