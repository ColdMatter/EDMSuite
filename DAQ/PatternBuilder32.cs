using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using DAQ.HAL;
using DAQ.Environment;

namespace DAQ.Pattern
{
    /// <summary>
    /// A thin wrapper class around PatternBuilder32SingleBoard to allow for multiple pattern boards
    /// </summary>
    public class PatternBuilder32 : IPatternSource
    {
        private Dictionary<string, PatternBuilder32SingleBoard> boards = new Dictionary<string, PatternBuilder32SingleBoard>();
        public Dictionary<string, PatternBuilder32SingleBoard> Boards
        {
            get { return boards; }
            set { boards = value; }
        }

        public PatternBuilder32() 
		{
			
		}

        public void AddBoard(string address)
        {
            Boards.Add(address, new PatternBuilder32SingleBoard());
        }

        public PatternBuilder32SingleBoard GetBoard(DigitalOutputChannel channel)
        {
            string boardName = channel.Device;
            if (!Boards.ContainsKey(boardName))
            {
                AddBoard(boardName);
            }
            return Boards[boardName];
        }

        public void AddEdge(string channelName, int time, bool sense)
        {
            DigitalOutputChannel channel = (DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[channelName];
            GetBoard(channel).AddEdge(channel.BitNumber, time, sense);
        }

        public int Pulse(int startTime, int delay, int duration, string channelName)
        {
            DigitalOutputChannel channel = (DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[channelName];
            return GetBoard(channel).Pulse(startTime, delay, duration, channel.BitNumber);
        }

        public int DownPulse(int startTime, int delay, int duration, string channelName)
        {
            DigitalOutputChannel channel = (DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[channelName];
            return GetBoard(channel).DownPulse(startTime, delay, duration, channel.BitNumber);
        }

        /** Convenience method to determine the channel number from a NI port/line spec. */
        public static int ChannelFromNIPort(int port, int line)
        {
            return line + (8 * port);
        }

        /** Return the minimum length array of the longest pattern. */
        public int GetMinimumLength()
        {
            PatternBuilder32SingleBoard[] boardsList = Boards.Values.ToArray();
            int numBoards = boardsList.Count();
            int[] minLengths = new int[numBoards];
            for (int i = 0; i < numBoards; i++)
            {
                minLengths[i] = boardsList[i].GetMinimumLength();
            }
            return minLengths.Max();
        }

        /** Builds each pattern */
        public void BuildPattern(int length)
        {
            foreach (PatternBuilder32SingleBoard board in Boards.Values)
            {
                board.BuildPattern(length);
            }
        }

        public PatternBuilder32SingleBoard GetDefaultBoard()
        {
            if (Boards.Count == 0)
            {
                string name = (string)Environs.Hardware.GetInfo("PatternGeneratorBoard");
                AddBoard(name);
                return Boards[name];
            }
            else if (Boards.Count == 1)
            {
                return Boards.First().Value;
            }
            else
            {
                throw new System.InvalidOperationException(
                    "If you have more than a single pattern board you must address them separately"
                    );
            }
        }


        #region Legacy Methods
        /** Add an edge to a pattern. All pattern trees must have addEdges as their terminals
         * (either directly or through <code>pulse</code> which is just two <code>addEdge</code>s
         */
        public void AddEdge(int channel, int time, bool sense)
        {
            PatternBuilder32SingleBoard board = GetDefaultBoard();
            board.AddEdge(channel, time, sense);
        }
        
        /** Convenience method to add two edges. */
        public int Pulse(int startTime, int delay, int duration, int channel)
        {
            PatternBuilder32SingleBoard board = GetDefaultBoard();
            board.AddEdge(channel, startTime + delay, true);
            board.AddEdge(channel, startTime + delay + duration, false);

            return delay + duration;
        }

        /** Adds a downward going pulse **/
        public int DownPulse(int startTime, int delay, int duration, int channel)
        {
            PatternBuilder32SingleBoard board = GetDefaultBoard();
            board.AddEdge(channel, startTime + delay, false);
            board.AddEdge(channel, startTime + delay + duration, true);

            return delay + duration;
        }

        /** Clear the pattern. */
        public void Clear()
        {
            PatternBuilder32SingleBoard board = GetDefaultBoard();
            board.Clear();
        }

        /** Get the pattern - you must call <code>generatePattern()</code> first.
         */
        public UInt32[] Pattern
        {
            get 
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard(); 
                return board.Pattern;
            }
        }

        public Int16[] PatternAsInt16s
        {
            get
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard();
                return board.PatternAsInt16s;
            }
        }

        public byte[] PatternAsBytes
        {
            get
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard();
                return board.PatternAsBytes;
            }
        }

        // gets the low word of the pattern so that you can run the pattern generator in half-width mode
        public Int16[] LowHalfPatternAsInt16
        {
            get
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard();
                return board.LowHalfPatternAsInt16;
            }
        }

        // gets the high word of the pattern so that you can run the pattern generator in half-width mode
        public Int16[] HighHalfPatternAsInt16
        {
            get
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard();
                return board.HighHalfPatternAsInt16;
            }
        }

        public byte[] LowHalfPatternAsByte
        {
            get
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard();
                return board.LowHalfPatternAsByte;
            }
        }

        public byte[] HighHalfPatternAsByte
        {
            get
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard();
                return board.HighHalfPatternAsByte;
            }
        }

        public Layout Layout
        {
            get 
            {
                PatternBuilder32SingleBoard board = GetDefaultBoard();
                return board.Layout; 
            }
        }

        // Methods for displaying the pattern and layout

        /** Display the binary representation of the pattern. */
        public String ArrayToString()
        {
            PatternBuilder32SingleBoard[] boardsList = Boards.Values.ToArray();
            int numBoards = boardsList.Count();
            string[] patternStrings = new string[numBoards];
            for (int i = 0; i < numBoards; i++)
            {
                patternStrings[i] = boardsList[i].ArrayToString();
            }
            return string.Join("\n\n\n", patternStrings);
        }

        /** Display the pattern's layout as a list of edge events. */
        public String LayoutToString()
        {
            PatternBuilder32SingleBoard[] boardsList = Boards.Values.ToArray();
            int numBoards = boardsList.Count();
            string[] patternStrings = new string[numBoards];
            for (int i = 0; i < numBoards; i++)
            {
                patternStrings[i] = boardsList[i].LayoutToString();
            }
            return string.Join("\n\n\n", patternStrings);
        }
        #endregion
    }
}
