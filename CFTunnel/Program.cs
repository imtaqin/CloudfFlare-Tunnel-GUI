namespace CFTunnel
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new CFTunnel.Forms.MainWizard());
        }
    }
}
