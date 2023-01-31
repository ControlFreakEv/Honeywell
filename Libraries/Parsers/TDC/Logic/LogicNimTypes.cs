using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Parsers.TDC.Logic
{
    internal class LogicNimTypes
    {
        public enum LogicNimBlockTypes { 
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

        public static LogicNimBlockTypes GetLogicType(string? logicAlgId)
        {
            if (logicAlgId == null)
                return LogicNimBlockTypes.NULL;

            if (Enum.TryParse(logicAlgId.ToUpper(), out LogicNimBlockTypes logicBlockType))
                return logicBlockType;
            throw new Exception($"Unknown LOGALID {logicAlgId}");
        }
    }
}
