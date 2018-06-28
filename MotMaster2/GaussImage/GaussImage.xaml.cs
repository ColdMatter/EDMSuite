using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Data;
using NationalInstruments.Analysis.Math;
using ErrorManager;
using UtilsNS;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MOTMaster2
{
    #region Internal image - it can work without show or call procSource()
    public enum ColorScheme
    { BnW, Color1 }

    public class GImage  // the mechanics behind the scene 
    {
        public GImage(string imagePth = "") 
        {            
            if(!imagePth.Equals("")) LoadImage(imagePth);
            else ChangeImageEvent(true);
        }
        public string imagePath // full filename of current image file
        {
            get; private set;  
        }
        public void Clear() 
        {
            _source = null;
            pix1 = null;
            pix2 = null;
            imagePath = "";
            horzDbuffer = null; vertDbuffer = null;
            ChangeImageEvent(true);
        }
        public bool isEmpty()
        {
            return Utils.isNull(_source);
        }

        private BitmapSource _source;
        public BitmapSource source 
        {
            get {return _source;}
            set 
            {
                _source = value;
                if (!Utils.isNull(value))
                {
                    int stride = _source.PixelWidth * depth;
                    int size = _source.PixelHeight * stride;
                    pix1 = new byte[size];
                    _source.CopyPixels(pix1, stride, 0);

                    pix2 = new byte[_source.PixelWidth, _source.PixelHeight];
                    for (int x = 0; x < _source.PixelWidth; x++) 
                        for (int y = 0; y < _source.PixelHeight; y++)
                        {
                            pix2[x, y] = pix1[y * stride + depth * x];
                        }
                    if (autoUpperLimit) recalcUpperLimit();
                    horzDbuffer = null;  vertDbuffer = null;
                }
                ChangeImageEvent(true);
            }
        }
        private int _upperLimit = 70;                        
        public int upperLimit 
        { 
            get {return _upperLimit;}
            set { _upperLimit = value; ChangeImageEvent(false); } 
        }
        private void recalcUpperLimit()
        {
            if (Utils.isNull(pix1)) return;
            double mx = pix1.Max();
            _upperLimit = (int)(100 * mx / 255);
            upperLimit = Utils.EnsureRange(_upperLimit, 1, 100);
        }
        private bool _autoUpperLimit = false;
        public bool autoUpperLimit
        { 
            get { return _autoUpperLimit; } 
            set { 
                    bool bb = value && !_autoUpperLimit;
                    _autoUpperLimit = value;
                    if (bb) recalcUpperLimit();
                }           
        }
        private ColorScheme _colorScheme = ColorScheme.BnW;
        public ColorScheme colorScheme
        { 
            get { return _colorScheme; } 
            set {
                    bool bb = _colorScheme.Equals(value); // different 
                    _colorScheme = value;
                    if(!bb) ChangeImageEvent(false);                   
                } 
        }

        /*private Color getColor(double i, out double r, out double g, out double b) // i= 0..255
        {
            double ratio = i / 255.0;
            int col = (int)(i / iterations * 255);
            int alpha = 255;

            if (ratio >= 0 && ratio < 0.25)
            {
                r = col
            }

                return Color.FromArgb(alpha, col, col / 5, 0);

            if (ratio >= 0.25 && ratio < 0.50)
                return Color.FromArgb(alpha, col, col / 4, 0);

            if (ratio >= 0.50 && ratio < 0.75)
                return Color.FromArgb(alpha, col, col / 3, 0);

            if (ratio >= 0.75 && ratio < 1)
                return Color.FromArgb(alpha, col, col / 2, 0);

            return Color.Black; //color of the set itself
        }*/

        private double LinearStep(double x, double start, double width = 0.2)
        {
            if (x < start)
                return 0.0;
            else if (x > start + width)
                return 1.0;
            else
                return (x - start) / width;
        }

        private double GetRedValue(double intensity)
        {
            return LinearStep(intensity, 0.2)*255;
        }

        private double GetGreenValue(double intensity)
        {
            return LinearStep(intensity, 0.6)*255;
        }

        private double GetBlueValue(double intensity)
        {
            return (LinearStep(intensity, 0)
            - LinearStep(intensity, 0.4)
            + LinearStep(intensity, 0.8))*255;
        }

        public BitmapSource procSource()
        {   
            if (isEmpty()) return null;
            BitmapSource res = null;
            
            int i = 0; double m = 0;
            byte[] pix1t = new byte[sizeX*sizeY];
            double[,] pix2t = new double[sizeX, sizeY];
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                {
                    m = Utils.EnsureRange((100 * pix2[x, y] / upperLimit),0,255);                   
                    pix2t[x, y] = m;
                    pix1t[i] = (byte)m;
                    i++;
                }
            
            int stride = source.PixelWidth * 4;
            byte[] pix1c = new byte[stride * source.PixelHeight];
            for (int y = 0; y < sizeY; y++)
                for (int x = 0; x < sizeX; x++)
                {
                    int xIndex = x * 4;
                    int yIndex = y * stride;
                    switch (colorScheme)
                    {
                        case ColorScheme.BnW:
                            {
                                pix1c[xIndex + yIndex] = (byte)pix2t[x, y]; // red
                                pix1c[xIndex + yIndex + 1] = (byte)pix2t[x,y]; // green
                                pix1c[xIndex + yIndex + 2] = (byte)pix2t[x, y]; // blue
                                pix1c[xIndex + yIndex + 3] = 255; 
                            break;
                            }
                        case ColorScheme.Color1:
                            {
                                m = pix2t[x, y] / 255;
                                pix1c[xIndex + yIndex] = (byte)GetBlueValue(m); // red
                                pix1c[xIndex + yIndex + 1] = (byte)GetGreenValue(m); // green
                                pix1c[xIndex + yIndex + 2] = (byte)GetRedValue(m); // blue
                                pix1c[xIndex + yIndex + 3] = 255; 
                            break;
                            }
                    }
                }
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Red);            
            colors.Add(System.Windows.Media.Colors.Green);
            colors.Add(System.Windows.Media.Colors.Blue);

            BitmapPalette myPalette = new BitmapPalette(colors);

            res = BitmapSource.Create(
                                    sizeX, sizeY,
                                    96, 96,
                                    PixelFormats.Bgra32,
                                    myPalette,
                                    pix1c,
                                    stride);                                           
            
            if (Utils.isNull(res)) res = _source.Clone();
            return res; 
        }
        public delegate void ChangeImageHandler(bool newData);
        public event ChangeImageHandler OnChangeImage;

        protected void ChangeImageEvent(bool newData)
        {
            if (OnChangeImage != null) OnChangeImage(newData);
        }

        private int depth = 1;

        public byte[] pix1 {get; private set;}
        public byte[,] pix2 {get; private set;}
        public int sizeX { get { return pix2.GetUpperBound(0); } }
        public int sizeY { get { return pix2.GetUpperBound(1); } } 

        private double sumByIdx(bool horiz, int idx)
        {
            double sum = 0;            
            if (horiz)
            {
                for (int i = 0; i < sizeY; i++)
                {
                    sum += pix2[idx, i];
                }
            }
            else
            {
                for (int i = 0; i < sizeX; i++)
                {
                    sum += pix2[i, idx];
                }
            }
            return sum;
        }
        private List<Point> horzDbuffer;
        public List<Point> horzD()
        {
            if(isEmpty()) return null;
            if (Utils.isNull(horzDbuffer))
            {
                horzDbuffer = new List<Point>();
                for (int i = 0; i < sizeX; i++)
                {
                    horzDbuffer.Add(new Point(i, sumByIdx(true, i)));
                }

            }
            return horzDbuffer; 
        }
        public double[] horzGaussFit(out double ampl, out double center, out double stdDev, out double res)
        {
            List<Point> hd = horzD();
            double[] wg = new double[hd.Count];
            double[] xs = new double[hd.Count];
            double[] ys = new double[hd.Count];
            for (int i = 0; i < hd.Count; i++)
            {
                xs[i] = hd[i].X; ys[i] = hd[i].Y; 
                wg[i] = 1;
            }                
            return CurveFit.GaussianFit(xs, ys, FitMethod.Bisquare, wg, 0, out ampl, out center, out stdDev, out res);
        }
        public List<Point> rotateDist(List<Point> vert)
        {
            List<Point> vert1 = new List<Point>();
            for (int i = 0; i < vert.Count; i++)
            {
                vert1.Add(new Point(vert[vert.Count-i-1].Y,vert[i].X));
            }
            return vert1;
        }

        private List<Point> vertDbuffer;
        public List<Point> vertD() 
        {
            if(isEmpty()) return null;
            if (Utils.isNull(vertDbuffer)) 
            {
                vertDbuffer = new List<Point>();
                for (int i = 0; i < sizeY; i++) // starting point is top/left !!!
                {
                    vertDbuffer.Add(new Point(i, sumByIdx(false, i))); 
                }
            }
            return vertDbuffer;
        }
        public double[] vertGaussFit(out double ampl, out double center, out double stdDev, out double res)
        {
            List<Point> vd = vertD();
            double[] wg = new double[vd.Count];
            double[] xs = new double[vd.Count];
            double[] ys = new double[vd.Count];
            for (int i = 0; i < vd.Count; i++)
            {
                xs[i] = vd[i].X; ys[i] = vd[i].Y;
                wg[i] = 1;
            }
            return CurveFit.GaussianFit(xs, ys, FitMethod.Bisquare, wg, 0, out ampl, out center, out stdDev, out res);
        }

        public bool LoadImage(string imagePth)
        {           
            if (!File.Exists(imagePth)) 
            {
                ErrorMng.errorMsg("No file " + imagePth, 10001); return false;
            }
            imagePath = imagePth;
            FileStream imageStream = File.OpenRead(imagePath);
            PngBitmapDecoder pngDecoder = new PngBitmapDecoder(imageStream,
                                            BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            source = pngDecoder.Frames[0];
            return true;
        }

        public void ExtractXY(List<Point> pnts, out double[] xs, out double[] ys)
        {
            xs = new double[pnts.Count];
            ys = new double[pnts.Count];
            for (int i = 0; i < pnts.Count; i++)
            {
                xs[i] = pnts[i].X; ys[i] = pnts[i].Y;
            }
        }
    #endregion
    }
     
    #region internal table of DataGrid data
    public struct iRecord
    {
        public bool Use { set; get; }
        public string Param { set; get; }       
        public string Filename { set; get; }

        public bool isProcessed()
        {
            return !amplX.Equals("");
        }        
        public string amplX { set; get; }
        public string centerX { set; get; } 
        public string stdDevX { set; get; }
        public string resX { set; get; }

        public string amplY { set; get; }
        public string centerY { set; get; }
        public string stdDevY { set; get; }
        public string resY { set; get; }           
    }
    public class DataBank
    {
        public ObservableCollection<iRecord> iRecords;
        private string tablePrec = "G6";
        System.Windows.Controls.DataGrid dg;
        public DataBank(System.Windows.Controls.DataGrid DG)
        {
            dg = DG;
            iRecords = new ObservableCollection<iRecord>();
            dg.ItemsSource = iRecords;
        }
        
        public void Clear()
        {
            iRecords.Clear();
        }

        public void UpdateRecord(int idx, iRecord rc)
        {
            if (Utils.isNull(rc) || !Utils.InRange(idx, 0, iRecords.Count - 1)) return;
            iRecords.Insert(idx, rc);
            iRecords.RemoveAt(idx + 1);   
        }

        public List<bool> checks() // actual check values from the table
        {
            List<bool> ls = new List<bool>();

            IEnumerable lst = dg.ItemsSource as IEnumerable;
            DataGridColumn dgc = dg.Columns[0];
            foreach (var row in lst)
            {
                System.Windows.Controls.CheckBox chk = ((System.Windows.Controls.CheckBox)dg.Columns[0].GetCellContent(row));
                ls.Add(chk.IsChecked.Value);            
            }
            return ls;
        } 
        public int AddFile(string param, string filename)
        {
            if (iRecords.Count == 1)
            {
                for (int i = 1; i < 11; i++)
                {
                    dg.Columns[i].IsReadOnly = true;
                }
            }
            iRecords.Add(new iRecord { Use = true, Param = param, Filename = filename });
            return iRecords.Count;
        }

        private int idxFromFilename(string filename)
        {
            int j = -1;
            for (int i = 0; i < iRecords.Count; i++)
            {
                if (iRecords[i].Filename.Equals(filename))
                {
                    j = i; break;
                }
            }
            return j;
        }

        public List<string> Filenames()
        {
            List<string> ls = new List<string>();
            for (int i = 0; i < iRecords.Count; i++) ls.Add(iRecords[i].Filename);
            return ls;
        }

        public bool AddStat(string filename, Dictionary<string, double> stat)
        {
            int j = idxFromFilename(filename);
            if (j < 0) return false;
            string sp = "   ";
            iRecord rec = new iRecord
            {
                Use = iRecords[j].Use,
                Param = iRecords[j].Param,
                Filename = iRecords[j].Filename+sp,

                amplX = sp+stat["ampl.X"].ToString(tablePrec) + sp,
                centerX = stat["center.X"].ToString(tablePrec) + sp,
                stdDevX = stat["stdDev.X"].ToString(tablePrec) + sp,
                resX = stat["res.X"].ToString(tablePrec) + sp,

                amplY = sp+stat["ampl.Y"].ToString(tablePrec) + sp,
                centerY = stat["center.Y"].ToString(tablePrec) + sp,
                stdDevY = stat["stdDev.Y"].ToString(tablePrec) + sp,
                resY = stat["res.Y"].ToString(tablePrec) + sp
            };
            iRecords.Insert(j, rec); iRecords.RemoveAt(j + 1);
            return true;
        }
    }
    #endregion

    public enum ScanMode
    { none, scan, repeat}
    /// <summary>
    /// Interaction logic for GaussImage.xaml =======================================================================================================
    /// </summary>
    public partial class GaussImage : System.Windows.Controls.UserControl
    {
        public void DoEvents()
        {
            if (Utils.isNull(System.Windows.Application.Current)) return;
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        public bool LineMode // Online / Offline
        {
            get { return (bool)rbOnline.IsChecked.Value; }
            set
            {
                rbOnline.IsChecked = value; rbOffline.IsChecked = !value; 
            }
        }
        private bool _ProcessingMode;
        public bool ProcessingMode // if processing is on foot
        {
            get { return _ProcessingMode; }
            private set
            {
                if (value)
                {
                    btnProcess.Content = "Processing..."; btnProcess.Background = Brushes.Coral; btnProcess.IsEnabled = false;
                }
                else
                {
                    btnProcess.Content = "Process"; btnProcess.Background = Utils.ToSolidColorBrush("#FFD7EFE8"); btnProcess.IsEnabled = true;  
                }
                _ProcessingMode = value;
            }
        }

        public string ParentDir
        {
            get
            {
                string sd = Convert.ToString(lbSelectedDir.Content);
                if (sd.Equals("Directory")) return "";
                return sd.Split('=')[1].TrimStart();
            }
            set { lbSelectedDir.Content = "Directory = " + value; }
        }
        public string SelectedDir
        {
            get 
            { 
                string sd = Convert.ToString(lbSelectedDir.Content);  
                if(sd.Equals("Directory")) return "";
                sd = sd.Split('=')[1].TrimStart();
                if (LineMode) return sd + "Images";
                else
                {
                    ComboBoxItem cbi = (cbExtraDirectory.SelectedItem as ComboBoxItem);
                    if (Utils.isNull(cbi))
                    {
                        ErrorMng.warningMsg("No sub-directory is selected");
                        return "";
                    }
                    return sd + cbi.Content.ToString();
                }
            }
        }

        List<Point> devX = new List<Point>();
        List<Point> devY = new List<Point>();

        FileSystemWatcher watcher;

        private void watchImageDir(string dir)
        {
            if (!LineMode) return;
            if (Utils.isNull(watcher))
            {
                watcher = new FileSystemWatcher(dir);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.Created += new FileSystemEventHandler(OnChangedWatcher);
            }
            else { watcher.Path = dir; }
            watcher.EnableRaisingEvents = true;
        }
        public bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnChangedWatcher(object source, FileSystemEventArgs e)
        {
            if (!System.IO.Path.GetExtension(e.Name).Equals(".sis")) return;
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                new Action( 
                    delegate()
                    {
                        if (!LineMode) return;
                        procStage = 2;
                        while (!IsFileReady(e.FullPath)) { DoEvents(); }
                        UpdateFileList(e.Name);                         
                    }
                )
            );
        }
        ScanMode scanMode = ScanMode.none; baseMMscan mmscan; 
        int procStage = 0; // 0 -> before anything; 1 -> new value; 2 -> file in; 3 -> start proc; 4 = 0 -> end proc

        public void StartScanEvent(bool _start, bool _scanMode, MMscan _mmscan)
        {
            if(!_start) 
            {
                procStage = 0;
                if (chkLinearFit.IsChecked.Value && tcModes.SelectedIndex == 0 && scanMode == ScanMode.scan) 
                    LinearFit(devX, devY);
                scanMode = ScanMode.none;
                lbInfo.Content = "Info:";
                return;
            }
            if (!Directory.Exists(ParentDir))
            {
                scanMode = ScanMode.none;
                Utils.TimedMessageBox("No valid container directory is selected.","Error",2500);
                return;
            }
            mmscan = new baseMMscan();
            mmscan.Value = double.NaN;
            if(_scanMode) 
            {
                mmscan.sParam = _mmscan.sParam;
                mmscan.sFrom = _mmscan.sFrom;
                mmscan.sTo = _mmscan.sTo;
                mmscan.sBy = _mmscan.sBy;
                scanMode = ScanMode.scan;
            }
            else
            {
                mmscan.sFrom = 0;
                mmscan.sTo = -1;
                mmscan.sBy = 1;                
                scanMode = ScanMode.repeat;
            }
            lbInfo.Content = scanMode.ToString() + ":\t" + mmscan.getAsString();
            devX.Clear(); devY.Clear();
        }

        public void NextScanValEvent(double val)
        {
            if (scanMode == ScanMode.none) return;
            if ((scanMode == ScanMode.scan) && (val > (mmscan.sFrom+mmscan.sBy)) ) // on the second val
            {
                System.IO.File.WriteAllText(SelectedDir+"\\scan.json", JsonConvert.SerializeObject(mmscan));
            }
            lbInfo.Content = scanMode.ToString() + ":\t" + mmscan.getAsString() + "\tval= " + val.ToString("G6");
            while (procStage != 0) DoEvents(); // wait for the previous file to be get and processed
            mmscan.Value = val; procStage = 1;
        }
        /// <summary>
        /// Visual control and show
        /// </summary>
        #region visuals

        public GImage gImage;
        public DataBank dBank;

        public GaussImage()
        {
            InitializeComponent();

            gImage = new GImage(""); // something funny ?
            gImage.autoUpperLimit = chkAutoUpperLimit.IsChecked.Value;
            gImage.upperLimit = (int)kdUpperLimit.Value;
            gImage.colorScheme = ColorScheme.BnW;
            gImage.OnChangeImage += new GImage.ChangeImageHandler(DoChangeImage);
          
            dBank = new DataBank(dgTable);
        }

        private void DoChangeImage(bool newData)
        {
            if (Utils.isNull(gImage)) return;
            btnProcess.IsEnabled = !gImage.isEmpty();
            if (gImage.isEmpty())
            {
                image1.Source = null;
                graphX.Data[0] = null; graphY.Data[0] = null;
                graphX.Data[1] = null; graphY.Data[1] = null;
                return;
            }           
            if (chkImageUpdate.IsChecked.Value) 
            {
                image1.Source = gImage.procSource();
                if (gImage.autoUpperLimit)
                {
                    kdUpperLimit.Value = gImage.upperLimit;
                    lbUpperLimit.Content = ((int)kdUpperLimit.Value).ToString() + "%";
                } 
            }                   
            else image1.Source = null;
            if (newData && chkDistUpdate.IsChecked.Value)
            {
                graphX.Data[0] = gImage.horzD(); 
                List<Point> vl = gImage.vertD();
                graphY.Data[0] = gImage.rotateDist(vl);
                graphX.Data[1] = null; graphY.Data[1] = null;
            }
            if (!chkDistUpdate.IsChecked.Value)
            {
                graphX.Data[0] = null; graphY.Data[0] = null;
                graphX.Data[1] = null; graphY.Data[1] = null;
            }
        } 

        private List<string> UpdateDirectories(string imagePath) 
        {
            List<string> rslt = new List<string>();
            if (!Directory.Exists(imagePath))
            {
                ErrorMng.warningMsg("Not such directory: " + imagePath);
                return rslt;
            }
            string[] entries = Directory.GetDirectories(imagePath, "*", SearchOption.TopDirectoryOnly);
            cbExtraDirectory.Items.Clear(); string ss = "";
            foreach (string entry in entries)
            {
                ComboBoxItem cbi = new ComboBoxItem();                              
                string[] sa = entry.Split('\\');
                ss = sa[sa.Length - 1];
                cbi.Content = ss; rslt.Add(ss);
                cbExtraDirectory.Items.Add(cbi);
            }
            cbExtraDirectory.SelectedIndex = 0;
            return rslt;
        }
        private void UpdateFileList(string newFile = "")  
        {
            string ip = SelectedDir; int j = -1;            
            if (!Directory.Exists(SelectedDir))
            {
                ErrorMng.warningMsg("Not such directory: " + ip);
                return;
            }
            string[] entries = Directory.GetFiles(ip, "*.sis", SearchOption.TopDirectoryOnly);
            if (newFile.Equals("")) 
            {           
                dBank.Clear();
                if (File.Exists(ip + "\\scan.json")) // scan 
                {
                    string txt = System.IO.File.ReadAllText(ip + "\\scan.json");
                    mmscan = JsonConvert.DeserializeObject<baseMMscan>(txt);
                    lbInfo.Content = "Folder's  scan.json:   " + mmscan.getAsString();
                    j = 0;
                    for (double d = mmscan.sFrom; d < mmscan.sTo; d += mmscan.sBy)
                    {
                        if (j == entries.Length) break;
                        dBank.AddFile(d.ToString("G6"), System.IO.Path.GetFileName(entries[j]));
                        j++;
                    }
                }
                else // repeat
                {
                    lbInfo.Content = "Info:  mode -> repeat;  " + entries.Length.ToString() + " files";
                    mmscan = null;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        dBank.AddFile(i.ToString(), System.IO.Path.GetFileName(entries[i]));
                    }
                }
            }
            else
            {
                switch (scanMode)
                {
                    case ScanMode.none: return;
                        break;
                    case ScanMode.repeat:
                        j = dBank.AddFile(dBank.iRecords.Count.ToString(), newFile);
                        break;
                    case ScanMode.scan:
                        j = dBank.AddFile(mmscan.Value.ToString("G6"), newFile);
                        break;
                }
                procStage = 3;
                getStats(j-1, mmscan.Value);
                procStage = 0; // re-init
           }           
        }

        private void gridLeft_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            gridLeft.RowDefinitions[0].Height = new GridLength(gridLeft.ActualWidth);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "Sellect parent directory for the current experiment";
            dialog.SelectedPath = @"e:\VSprojects\Camera\";
            dialog.ShowDialog();
            string sp = dialog.SelectedPath+"\\"; // @"e:\VSprojects\Camera\";
            ParentDir = sp; 
            UpdateDirectories(sp);
            UpdateFileList();
            watchImageDir(SelectedDir);
            btnSelect.FontWeight = FontWeights.Normal;
        }

        private void rbOnline_Checked(object sender, RoutedEventArgs e)
        {
            if (LineMode)
            {
                lbImages.Visibility = System.Windows.Visibility.Visible;
                cbExtraDirectory.Visibility = System.Windows.Visibility.Collapsed;
                if(!Utils.isNull(btnArchive))
                    btnArchive.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lbImages.Visibility = System.Windows.Visibility.Collapsed;
                cbExtraDirectory.Visibility = System.Windows.Visibility.Visible;
                if (!Utils.isNull(btnArchive))
                    btnArchive.Visibility = System.Windows.Visibility.Collapsed;
            }
            if(!SelectedDir.Equals(""))
            {
                UpdateFileList();
            }       
        }

        private void cbExtraDirectory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFileList();
        }
        private void dgTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.DataGrid dataGrid = sender as System.Windows.Controls.DataGrid;
            int j = dataGrid.SelectedIndex; 
            if (j == -1) return;
            List<string> ls = dBank.Filenames();
            gImage.LoadImage(SelectedDir+"\\"+ls[j]);
            if (chkFitAsShown.IsChecked.Value && !ProcessingMode) distStats();
            else lbCurrentResults.Items.Clear();
        }
        private void dgTable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dgTable_SelectionChanged(sender, null); 
        }

        private void kdUpperLimit_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (Utils.isNull(lbUpperLimit)) return;
            lbUpperLimit.Content = ((int)kdUpperLimit.Value).ToString()+"%";
            gImage.upperLimit = (int)kdUpperLimit.Value;
        }

        private void chkAutoUpperLimit_Checked(object sender, RoutedEventArgs e)
        {
            if (Utils.isNull(gImage)) return;
            kdUpperLimit.IsEnabled = !chkAutoUpperLimit.IsChecked.Value;
            gImage.autoUpperLimit = chkAutoUpperLimit.IsChecked.Value;
        }
        #endregion 

        private void cbColorScheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Utils.isNull(gImage)) return;
            switch (cbColorScheme.SelectedIndex)
            {
                case 0: gImage.colorScheme = ColorScheme.BnW;
                    break;
                case 1: gImage.colorScheme = ColorScheme.Color1;
                    break;
            }
        }

        private void chkImageUpdate_Checked(object sender, RoutedEventArgs e)
        {
            DoChangeImage(true);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            gImage.Clear();
            lbCurrentResults.Items.Clear();
        }

        public Dictionary<string, double> distStats()
        {
            if (gImage.isEmpty()) return null;
            var dict = new Dictionary<string, double>();
            double ampl, center, stdDev, res;
            
            // horizontal
            double[] hf = gImage.horzGaussFit(out ampl, out center, out stdDev, out res); res = res / gImage.sizeX;
            dict.Add("ampl.X", ampl); dict.Add("center.X", center); dict.Add("stdDev.X", stdDev); dict.Add("res.X", res);
            if (chkDistUpdate.IsChecked.Value)
            {
                List<Point> hp = gImage.horzD(); 
                List<Point> hl = new List<Point>();
                for(int i = 0; i < hp.Count; i++)
                {
                    hl.Add(new Point(hp[i].X, hf[i]));
                }
                graphX.Data[1] = hl;
            }
            // vertical
            double[] vf = gImage.vertGaussFit(out ampl, out center, out stdDev, out res); res = res / gImage.sizeY;
            dict.Add("ampl.Y", ampl); dict.Add("center.Y", center); dict.Add("stdDev.Y", stdDev); dict.Add("res.Y", res);
            if (chkDistUpdate.IsChecked.Value)
            {
                List<Point> vp = gImage.vertD(); 
                List<Point> vl = new List<Point>();
                for (int i = 0; i < vp.Count; i++)
                {
                    vl.Add(new Point(vp[i].X, vf[i])); 
                }
                graphY.Data[1] = gImage.rotateDist(vl);
            }
            // result list
            lbCurrentResults.Items.Clear(); int j = -1;
            ListBoxItem lba = new ListBoxItem(); lba.Content = System.IO.Path.GetFileNameWithoutExtension(gImage.imagePath); lbCurrentResults.Items.Add(lba);
            lba = new ListBoxItem(); lba.Content = "==============="; lbCurrentResults.Items.Add(lba);
            foreach (var item in dict)
            {
                j++;
                ListBoxItem lbi = new ListBoxItem(); lbi.Content = item.Key+" = " + item.Value.ToString("G7");
                if (j == 4)
                {
                    lba = new ListBoxItem(); lba.Content = "--------------------"; lbCurrentResults.Items.Add(lba);
                }
                if(j<4) lbi.Foreground = Brushes.DarkGreen; 
                else lbi.Foreground = Brushes.Navy;
                lbCurrentResults.Items.Add(lbi);                
            }       
            return dict;
        }

        private Dictionary<string, double> LinearFit(List<Point> dvX, List<Point> dvY)
        {
            Dictionary<string, double> sts = new Dictionary<string, double>();
            double[] xs; double[] ys; double a,b,res;

            gImage.ExtractXY(dvX,out xs,out ys);
            CurveFit.LinearFit(xs, ys, FitMethod.Bisquare,out a, out b, out res);
            List<Point> lx = new List<Point>();
            lx.Add(new Point(xs[0], a * xs[0] + b)); lx.Add(new Point(xs[xs.Length - 1], a * xs[xs.Length - 1] + b));
            graphRslt.Data[2] = lx;
            sts["slope.X"] = a; sts["intercept.X"] = b; sts["residue.X"] = res;

            gImage.ExtractXY(dvY, out xs, out ys);
            CurveFit.LinearFit(xs, ys, FitMethod.Bisquare, out a, out b, out res);
            List<Point> ly = new List<Point>();
            ly.Add(new Point(xs[0], a * xs[0] + b)); ly.Add(new Point(xs[xs.Length - 1], a * xs[xs.Length - 1] + b));
            graphRslt.Data[3] = ly;
            sts["slope.Y"] = a; sts["intercept.Y"] = b; sts["residue.Y"] = res;

            ListBoxItem lba; int j = 0;
            foreach (var item in sts) 
            {
                j++;
                ListBoxItem lbi = new ListBoxItem(); lbi.Content = item.Key + " = " + item.Value.ToString("G7");
                if (j == 4)
                {
                    lba = new ListBoxItem(); lba.Content = "--------------------"; lbDevResults.Items.Add(lba);
                }
                if (j < 4) lbi.Foreground = Brushes.DarkGreen;
                else lbi.Foreground = Brushes.Navy;
                lbDevResults.Items.Add(lbi);
            }
            return sts;
        }

        private void getStats(int i, double val = double.NaN)
        {
            
            Dictionary<string, double> sts = null;
            
            Dispatcher.Invoke(new Action(() =>
            {
                dgTable.SelectedIndex = i; 
                sts = distStats();
                dBank.AddStat(dBank.iRecords[i].Filename, sts);
               
                Thread.Sleep(100);
            }
            ), DispatcherPriority.Background); 
            if (val == double.NaN) // repeat
            {
                devX.Add(new Point(i, sts["stdDev.X"])); devY.Add(new Point(i, sts["stdDev.Y"]));
            }
            else 
            {
                devX.Add(new Point(val, sts["stdDev.X"])); devY.Add(new Point(val, sts["stdDev.Y"]));
            }
            
            graphRslt.Data[0] = devX; graphRslt.Data[1] = devY;
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            ProcessingMode = true;
            btnClearGraph_Click(null, null);
            List<string> ls = dBank.Filenames();
            
            for (int i = 0; i < dBank.iRecords.Count; i++)
            {
                double d = Convert.ToDouble(dBank.iRecords[i].Param);
                getStats(i, d);
            }
            if (chkLinearFit.IsChecked.Value && tcModes.SelectedIndex == 0) LinearFit(devX, devY);
            ProcessingMode = false;      
        }

        private void btnClearGraph_Click(object sender, RoutedEventArgs e)
        {
            devX.Clear(); devY.Clear();
            for (int i = 0; i < 4; i++)
                graphRslt.Data[i] = null;
            lbDevResults.Items.Clear();            
        }

        public string NextAvailFolder(string mask)
        {
            string ss = "";
            List<string> dirs = UpdateDirectories(ParentDir);
            for (int i = 1; i < 100; i++)
            {
                if (dirs.IndexOf(mask + i.ToString()) > -1) continue;
                if (dirs.IndexOf(mask + i.ToString("D2")) > -1) continue;
                ss = mask + i.ToString("D2");
                break;
            }
            return ss;
        }

        private void btnArchive_Click(object sender, RoutedEventArgs e)
        {
            if (!LineMode)
            {
                ErrorMng.warningMsg("No archive in offline mode");
                return;
            }
            string ss = NextAvailFolder("Run ");
            Directory.Move(ParentDir + "Images", ParentDir + ss);
            Directory.CreateDirectory(ParentDir + "Images");
            Thread.Sleep(200);
            UpdateDirectories(ParentDir);
            UpdateFileList();
        }
    }
}
