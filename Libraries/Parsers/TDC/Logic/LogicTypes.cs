using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Parsers.TDC.Logic
{
    internal class LogicTypes
    {
        public enum LogicBlockTypes { 
            NULL, 
            AND, 
            OR, 
            NOT, 
            NAND, 
            NOR, 
            XOR, 
            QOR2, 
            QOR3, 
            DISCREP3, 
            EQ, 
            NE, 
            GT, 
            GE, 
            LT, 
            LE, 
            CHECKBAD, 
            PULSE, 
            MAXPULSE, 
            MINPULSE, 
            DELAY, 
            ONDLY, 
            OFFDLY, 
            FLIPFLOP, 
            CHDETECT,
            WATCHDOG
        }

        public static LogicBlockTypes GetLogicType(string? logicAlgId)
        {
            if (logicAlgId == null)
                return LogicBlockTypes.NULL;

            if (Enum.TryParse(logicAlgId.ToUpper(), out LogicBlockTypes logicBlockType))
                return logicBlockType;
            throw new Exception($"Unknown LOGALID {logicAlgId}");
        }
    }
}
