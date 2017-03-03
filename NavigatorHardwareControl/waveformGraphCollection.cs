using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.Vision;
using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

namespace NavigatorHardwareControl
{
    class waveformGraphCollection
    {
        public waveformGraphCollection()
        {
            graphs = new Dictionary<string, waveformGraphConfiguration>();
        }

        private Dictionary<string, waveformGraphConfiguration> graphs;

        public void addGraphToCollection(string key, WaveformGraph graph)
        {
            waveformGraphConfiguration graphConfiguration = new waveformGraphConfiguration(graph);
            graphs.Add(key, graphConfiguration);
        }

        public void updateGraphs()
        {
            foreach(string key in graphs.Keys)
            {
                graphs[key].applySettingsToGraph();
            }
        }

        public void turnOffAllAutoScale()
        {
            foreach (string key in graphs.Keys)
            {
                graphs[key].turnAutoScaleOff();
            }
        }

        public void turnOnAllAutoScale()
        {
            foreach (string key in graphs.Keys)
            {
                graphs[key].turnAutoScaleOn();
            }
        }

        class waveformGraphConfiguration
        {
            public waveformGraphConfiguration(WaveformGraph thisObjectsGraph)
            {
                this.graph = thisObjectsGraph;
                this.graph.XAxes[0].Mode = AxisMode.AutoScaleExact;
               // this.graph.XAxes[0].Range = new NationalInstruments.UI.Range();
            }

            public WaveformGraph graph;

            private double ymin = 0.0;
            private double ymax = 255.0;
            

            public void applySettingsToGraph()
            {

            }

            public void turnAutoScaleOn()
            {
                this.graph.YAxes[0].Mode = AxisMode.AutoScaleLoose;
            }

            public void turnAutoScaleOff()
            {
                this.graph.YAxes[0].Mode = AxisMode.Fixed;
                this.graph.YAxes[0].Range = new NationalInstruments.UI.Range(this.ymin, this.ymax);
            }



            // public setYAxisRange(double ymin, double ymax);

        }

    }
}
