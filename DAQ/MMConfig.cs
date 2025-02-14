﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ
{
    public class MMConfig
    {
        public MMConfig()
        {

        }

        public MMConfig(bool camera, bool translation, bool reporter, bool dbg)
        {
            CameraUsed = camera;
            TranslationStageUsed = translation;
            ReporterUsed = reporter;
            Debug = dbg;
            DigitalPatternClockFrequency = 100000; //default value
            AnalogPatternClockFrequency = 100000; //default value
            ExternalFilePattern = null;
        }

        public MMConfig(bool camera, bool translation, bool reporter, bool dbg, bool dds)
        {
            CameraUsed = camera;
            TranslationStageUsed = translation;
            ReporterUsed = reporter;
            Debug = dbg;
            useDDS = dds;
            DigitalPatternClockFrequency = 100000; //default value
            AnalogPatternClockFrequency = 100000; //default value
            ExternalFilePattern = null;
        }

        private bool debug;
        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }

        private bool cameraUsed;
        public bool CameraUsed
        {
            get { return cameraUsed; }
            set { cameraUsed = value; }
        }

        private bool translationStageUsed;
        public bool TranslationStageUsed
        {
            get { return translationStageUsed; }
            set { translationStageUsed = value; }
        }

        private bool useDDS;
        public bool UseDDS
        {
            get { return useDDS; }
            set { useDDS = value; }
        }

        private bool reporterUsed;
        public bool ReporterUsed
        {
            get { return reporterUsed; }
            set { reporterUsed = value; }
        }

        private bool ddsUsed = false;
        public bool DdsUsed
        {
            get { return ddsUsed; }
            set { ddsUsed = value; }
        }

        private int digitalClockFreq;
        public int DigitalPatternClockFrequency
        {
            get { return digitalClockFreq; }
            set { digitalClockFreq = value; }
        }

        private int analogClockFreq;
        public int AnalogPatternClockFrequency
        {
            get { return analogClockFreq; }
            set { analogClockFreq = value; }
        }

        private string externalFilePattern;
        // Filename pattern for files generated by an external program to be zipped up along with the MOTMaster files, e.g. "*.tif" for image files generated by an external camera control program
        public string ExternalFilePattern
        {
            get { return externalFilePattern; }
            set { externalFilePattern = value; }
        }
    }
}
