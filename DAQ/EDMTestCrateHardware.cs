namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware that the edm test crate has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class EDMTestCrateHardware : DAQ.HAL.Hardware
    {
        public override void ConnectApplications()
        {

        }


        public EDMTestCrateHardware()
        {

            // add the boards
            Boards.Add("rfAWG", "Dev2");

        }

    }
}