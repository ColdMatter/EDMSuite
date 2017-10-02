using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace TriggeredShapedPulses
{
    static class Runner
    {

        static void Main(string[] args)
        {
            Controller prog = new Controller();

            TcpChannel channel = new TcpChannel(1191);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(prog, "controller.rem");

            Runner.Example(prog);
        }

        static void Example(Controller prog)
        {
            double[][] pulseParams = new double[2][] 
            { 
            new double[5]{ 1, 0, 0, 170e6, 0 },
            new double[5]{ 0, 0.5, 0.5, 170e6, 0 }
            };

            int[][] pulseList = new int[5][] 
            { 
            new int[2]{ 1, 2 }, 
            new int[2]{ 2, 1 }, 
            new int[2]{ 2, 2 }, 
            new int[2]{ 1, 2 }, 
            new int[2]{ 1, 1 } 
            };

            prog.LoadPulses(pulseParams);

            prog.Initialise();

            foreach (int[] pulseSeq in pulseList)
            {
                prog.BotRfChoice = pulseSeq[0];
                prog.TopRfChoice = pulseSeq[1];

                prog.StartGeneration();

                Console.WriteLine("Waiting for trigger... Press enter to move on to next sequence.");
                Console.ReadLine();

                prog.PauseGeneration();
            }

            Console.WriteLine("Finished generation. Press enter to stop program.");
            Console.ReadLine();

            prog.StopGeneration();

            
        }
    }
}
