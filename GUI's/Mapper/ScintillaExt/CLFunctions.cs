using Microsoft.VisualBasic.FileIO;

namespace Honeywell.GUI.Mapper.ScintillaExt
{
    public class CLFunctions
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
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
                    string[]? fields = parser.ReadFields();
                    if (fields == null)
                        continue;

                    Functions.Add(new CLFunctions() { Name = fields[0], Description = fields[1], Arguments = bool.Parse(fields[2]) });
                }
            }
        }
    }


}
