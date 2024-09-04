
namespace FLORENCE_Client.FrameworkSpace.ClientSpace
{
    public class Data
    {
        private FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.Input input;
        private FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.Output output;

        public Data()
        {
            System.Console.WriteLine("FLORENCE: Data");
            this.input = new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.Input();
            while (this.input == null) { /* wait untill created */ }
            this.output = new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.Output();
            while (this.output == null) { /* wait untill created */ }
        }

        public FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.Input Get_Input()
        {
            return this.input;
        }

        public FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.Output Get_Output()
        {
            return this.output;
        }
    }
}
