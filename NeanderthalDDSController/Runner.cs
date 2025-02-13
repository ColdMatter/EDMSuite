namespace NeanderthalDDSController
{
    internal static class Runner
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Controller controller = new Controller();
            controller.readCard();
            controller.start();
            
        }
    }
}