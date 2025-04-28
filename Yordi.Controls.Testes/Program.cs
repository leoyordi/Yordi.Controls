namespace Yordi.Controls.Testes
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            CurrentTooltipTheme.BackColor = Color.DarkBlue;
            CurrentTooltipTheme.ForeColor = Color.White;

            Application.Run(new Form1());
        }
    }
}