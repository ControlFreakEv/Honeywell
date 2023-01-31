using Honeywell.GUI.Mapper.Properties;

namespace Honeywell.GUI.Mapper
{
    internal static class Program
    {
        public static MainForm MainForm {  get; set; }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //todo: try/catch that auto installs framework
            Database.TdcContext.Server = Settings.Default.Server;
            Database.TdcContext.ConnectionString = Settings.Default.ConnectionString;
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            #region DPI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            #endregion
            ApplicationConfiguration.Initialize();

            try
            {
                MainForm = new();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("SQLITE"))
                {
                    Settings.Default.Server = false;
                    Settings.Default.ConnectionString = null;
                    Settings.Default.Save();

                    Database.TdcContext.Server = Settings.Default.Server;
                    Database.TdcContext.ConnectionString = Settings.Default.ConnectionString;

                    MainForm = new();
                }
                else
                    throw ex;
            }

            Application.Run(MainForm);
        }
    }
}