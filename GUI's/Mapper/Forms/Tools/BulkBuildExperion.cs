using Honeywell.TDC;
using Microsoft.Msagl.Drawing;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper.Forms.Tools
{
    public class BulkBuildExperion
    {
        private string ConvertAlarm(string alarm)
        {
            if (alarm == "EMERGNCY")
                return "URGENT";
            else if (alarm == "HIGH")
                return "HIGH";
            else if (alarm == "JNLPRINT")
                return "JOURNAL";
            else if (alarm == "JOURNAL")
                return "JOURNAL";
            else if (alarm == "LOW")
                return "LOW";
            else if (alarm == "NOACTION")
                return "NONE";
            else if (alarm == "PRINTER")
                return "JOURNAL";

            return "NONE";
        }

        private string ConvertMode(string mode)
        {
            if (mode == "NONE")
                return mode;
            else if (mode == "MAN")
                return mode;
            else if (mode == "CAS")
                return mode;
            else if (mode == "AUTO")
                return mode;
            else if (mode == "BCAS")
                return mode;
            else if (mode == "NORMAL")
                return mode;

            return "MAN";
        }

        private string ConvertModeAttr(string modeAttr)
        {
            if (modeAttr == "OPERATOR")
                return modeAttr;
            else if (modeAttr == "PROGRAM")
                return modeAttr;
            else if (modeAttr == "NORMAL")
                return modeAttr;
            else if (modeAttr == "NONE")
                return modeAttr;

            return "OPERATOR";
        }

        public string ConvertTdcToExperion(TdcTag tag, string paramName, string paramValue)
        {
            paramName = paramName.ToUpper();
            paramValue = paramValue.ToUpper();

            if (paramName == "BADCTLOP")
            {
                if (paramValue == "NO_SHED")
                    return paramValue;
                else if (paramValue == "SHEDHOLD")
                    return paramValue;
                else if (paramValue == "SHEDLOW")
                    return paramValue;
                else if (paramValue == "SHEDHIGH")
                    return paramValue;
                else if (paramValue == "SHEDSAFE")
                    return paramValue;
                else
                    return "NO_SHED";
            }
            else if (paramName == "BADCTLPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "BADPVPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "CMDDISPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "CMDFALTM")
            {
                return paramValue;
            }
            else if (paramName == "CTLACTN")
            {
                if (paramValue == "DIRECT")
                    return paramValue;
                else if (paramValue == "REVERSE")
                    return paramValue;
                else
                    return "REVERSE";
            }
            else if (paramName == "CTLEQN")
            {
                //AUTOMAN
                if (tag.CTLALGID == "AUTOMAN")
                {
                    if (paramValue == "EQA") //(CV = X1 + B +BI)
                        return null;
                    else if (paramValue == "EQB") //(CV = X1 + (K*X2) + BI)
                        return null;
                }

                //ORSEL
                if (tag.CTLALGID == "ORSEL")
                {
                    if (paramValue == "EQA") //high
                        return paramValue;
                    else if (paramValue == "EQB") //low
                        return paramValue;
                }

                //PID
                if (tag.CTLALGID == "PID" || tag.CTLALGID == "PIDFF" || tag.CTLALGID == "PIDERFB")
                {
                    if (paramValue == "EQA") //PID on error
                        return paramValue;
                    else if (paramValue == "EQB") //PI on error, D on PV
                        return paramValue;
                    else if (paramValue == "EQC") //I on error, PD on PV
                        return paramValue;
                    else if (paramValue == "EQD") // I only
                        return paramValue;
                }

                //SWITCH
                if (tag.CTLALGID == "SWITCH")
                {
                    if (paramValue == "EQA") //operator control
                        return paramValue;
                    else if (paramValue == "EQB") //program or logic control
                        return paramValue;
                }
            }
            else if (paramName == "EUDESC")
            {
                return paramValue;
            }
            else if (paramName == "K")
            {
                return paramValue;
            }
            else if (paramName == "NMODATTR")
            {
                return ConvertModeAttr(paramValue);
            }
            else if (paramName == "NMODE")
            {
                return ConvertMode(paramValue);
            }
            else if (paramName == "OFFNRMPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "OP")
            {
                return paramValue;
            }
            else if (paramName == "OPHILM")
            {
                return paramValue;
            }
            else if (paramName == "OPHIPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "OPHITP")
            {
                return paramValue;
            }
            else if (paramName == "OPLOLM")
            {
                return paramValue;
            }
            else if (paramName == "OPLOPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "OPLOTP")
            {
                return paramValue;
            }
            else if (paramName == "OPTDIR")
            {
                if (paramValue == "DIRECT")
                    return paramValue;
                else if (paramValue == "REVERSE")
                    return paramValue;
                else
                    return "DIRECT";
            }
            else if (paramName == "PVALDB")
            {
                if (paramValue == "HALF")
                    return "0.5";
                else if (paramValue == "ONE")
                    return "1";
                else if (paramValue == "TWO")
                    return "2";
                else if (paramValue == "THREE")
                    return "3";
                else if (paramValue == "FOUR")
                    return "4";
                else if (paramValue == "FIVE")
                    return "5";
                else if (paramValue == "EU")
                {
                    var deadband = tag.Params.FirstOrDefault(x => x.Name == "PVALDBEU")?.Value;
                    if (double.TryParse(deadband, out var value))
                        return value.ToString();
                    else
                        return "0";
                }
                else
                    return "0";
            }
            else if (paramName == "PVALDBEU")
            {
                if (double.TryParse(paramValue, out var value))
                    return "EU";
                else
                    return "PERCENT";
            }
            else if (paramName == "PVCHAR")
            {
                if (tag.PVALGID == "FLOWCOMP")
                {
                    if (paramValue == "LINEAR")
                        return "NONE";
                    else if (paramValue == "SQROOT")
                        return "SQUAREROOT";
                }
                else
                {
                    //if (paramValue == "JTHERM")
                    //    return 5;
                    //else if (paramValue == "KTHERM")
                    //    return 5;
                    //else if (paramValue == "ETHERM")
                    //    return 5;
                    //else if (paramValue == "TTHERM")
                    //    return 5;
                }
            }
            else if (paramName == "PVCLAMP")
            {

            }
            else if (paramName == "PVEUHI")
            {
                return paramValue;
            }
            else if (paramName == "PVEULO")
            {
                return paramValue;
            }
            else if (paramName == "PVHHPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "PVHHTP")
            {
                return paramValue;
            }
            else if (paramName == "PVHIPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "PVHITP")
            {
                return paramValue;
            }
            else if (paramName == "PVLLPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "PVLLTP")
            {
                return paramValue;
            }
            else if (paramName == "PVLOPR")
            {
                return ConvertAlarm(paramValue);
            }
            else if (paramName == "PVLOTP")
            {
                return paramValue;
            }
            else if (paramName == "SEALOPT")
            {

            }
            else if (paramName == "SP")
            {
                return paramValue;
            }
            else if (paramName == "SPHILM")
            {
                return paramValue;
            }
            else if (paramName == "SPLOLM")
            {
                return paramValue;
            }
            else if (paramName == "STATETXT(0)")
            {
                return paramValue;
            }
            else if (paramName == "STATETXT(1)")
            {
                return paramValue;
            }
            else if (paramName == "STATETXT(2)")
            {
                return paramValue;
            }
            else if (paramName == "T1")
            {
                return paramValue;
            }
            else if (paramName == "T2")
            {
                return paramValue;
            }
            else if (paramName == "TF")
            {
                return paramValue;
            }

            return paramValue;
        }
    }
}
