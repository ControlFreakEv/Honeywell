using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.TDC
{
    public class TdcParameter
    {
        public int Id { get; set; }
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Parameter name with padding on arrays e.g T(1) => T(001)
        /// </summary>
        public string SortName { get;}
        /// <summary>
        /// Rawvalue after normalization
        /// </summary>
        public string? Value { get; set; }
        /// <summary>
        /// value in EB files
        /// </summary>
        public string RawValue { get; set; }
        /// <summary>
        /// Custom Data Segment that references another Tag
        /// </summary>
        public TdcTag? CdsTag { get; set; }
        public TdcTag ParentTag { get; set; }
        public TdcTag? ConnectedTag
        {
            get
            {
                if (connectedTag == null)
                    SetConnection();
                return connectedTag;
            }
        }
        public string? ConnectedParameter
        {
            get
            {
                if (connectedParameter == null)
                    SetConnection();
                return connectedParameter;
            }
        }
        public static Dictionary<string, TdcTag>? TdcTagDict { get; set; }

        private TdcTag? connectedTag = null;
        private string? connectedParameter = null;
        private static int count = 0;

        public TdcParameter(TdcTag tdcTag, string name, string? value, string rawValue)
        {
            ParentTag = tdcTag;
            Name = name;
            Value = value;
            RawValue = rawValue;
            Id = ++count;

            if (name.Contains('('))
            {
                var split = name.Split('(');
                SortName = $"{split[0]}({split[1].PadLeft(8,'0')}";
            }
            else
                SortName = name;
        }

        private void SetConnection()
        {
            if (Value != null && Value.Contains('.'))
            {
                var arr = Value.Split('.');
                connectedParameter = arr.Last();
                var tagname = Value[..^(connectedParameter.Length + 1)];
                TdcTagDict.TryGetValue(tagname, out connectedTag);
            }
        }
    }
}
