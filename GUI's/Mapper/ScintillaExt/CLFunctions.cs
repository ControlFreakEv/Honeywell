using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper.ScintillaExt
{
    public class CLFunctions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Arguments { get; set; }

        public static readonly List<CLFunctions> Functions = new List<CLFunctions>();

        public static void LoadFunctions()
        {
            string fileName = "TDC Function Tooltips.csv";
            string path = Path.Combine(Environment.CurrentDirectory, fileName);

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");
                if (!parser.EndOfData)
                    parser.ReadLine(); //skip header

                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    Functions.Add(new CLFunctions() { Name = fields[0], Description = fields[1], Arguments = bool.Parse(fields[2]) });
                }
            }
        }
    }


}
