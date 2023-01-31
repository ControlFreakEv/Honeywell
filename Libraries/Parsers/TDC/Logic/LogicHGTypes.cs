using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Parsers.TDC.Logic
{
    public class LogicHgTypes
    {
        public enum LogicHGBlockTypes { 
            NOP, 
            AND, 
            OR, 
            XOR, 
            AND_OUT, 
            OR_OUT, 
            XOR_OUT,  
            FLIP_FLOP, 
            LINK,
            ON_DELAY,
            OFF_DELAY,
            PULSE,
            NOT, //special case for when input has "-"
            MOMENTARY, //special case for when input has "*"
        }

        public static LogicHGBlockTypes GetLogicType(string? logicAlgId)
        {
            if (logicAlgId == null)
                return LogicHGBlockTypes.NOP;

            if (Enum.TryParse(logicAlgId.ToUpper().Replace("-","_"), out LogicHGBlockTypes logicBlockType))
                return logicBlockType;
            throw new Exception($"Unknown LOGALID {logicAlgId}");
        }
    }
}
