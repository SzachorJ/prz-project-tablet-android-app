using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using static System.Net.Mime.MediaTypeNames;
using ZedGraph;
using System.Windows.Forms.DataVisualization.Charting;
using System.Security.Authentication;
using System.Globalization;

namespace inzynierkaapka
{
    public partial class Form1 : Form
    {

        private SerialPort SerPort;
        private string RecievedData;
        private string[] SplitData;
        private float xval;
        private float minxval=0;
        private float maxxval=0;
        private float yval;
        private float minyval=0;
        private float maxyval=0;
        private int lpos;
        private int rpos;
        

        PointPairList gList = new PointPairList();
        PointPairList lList = new PointPairList();
        PointPairList rList = new PointPairList();
        public Form1()
        {
            InitializeComponent();
            InitGraph();
            FetchAvailablePorts();
        }
        System.Windows.Forms.Timer PlotTimer = new System.Windows.Forms.Timer();
        private void InitGraph()
        {
            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "g-force graph";
            myPane.XAxis.Title.Text = "x-axis";
            myPane.YAxis.Title.Text = "y-axis";
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;
            myPane.XAxis.Scale.Max = 4;
            myPane.XAxis.Scale.Min = -4;
            myPane.YAxis.Scale.Max = 4;
            myPane.YAxis.Scale.Min = -4;

            myPane.Chart.Fill = new Fill(Color.White,Color.Beige, 45.0f);
            LineItem myCurve = myPane.AddCurve("", gList, Color.Blue, SymbolType.Circle);

            myCurve.Line.IsVisible = true;
            myCurve.Symbol.IsVisible = true;
            myCurve.Symbol.Fill = new Fill(Color.Blue);
            myCurve.Symbol.Size = 5;

            chart1.ChartAreas[0].AxisY.Minimum= 0;
            chart1.ChartAreas[0].AxisY.Maximum = 180;
            chart1.ChartAreas[0].AxisY.Interval = 10;
            chart2.ChartAreas[0].AxisY.Minimum= 0;
            chart2.ChartAreas[0].AxisY.Maximum = 180;
            chart2.ChartAreas[0].AxisY.Interval = 10;

        }

        private void UpdateGraph()
        {
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Update();
            zedGraphControl1.Refresh();
   
        }

        private void ConvertAndFill()
        {


                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                xval = float.Parse(SplitData[1], CultureInfo.InvariantCulture.NumberFormat);
                yval = 0-float.Parse(SplitData[2], CultureInfo.InvariantCulture.NumberFormat);
                if(xval > maxxval) maxxval= xval;
                if(xval < minxval) minxval= xval;
                if(yval > maxyval) maxyval= yval;
                if(yval < minyval) minyval= yval;
                lpos = int.Parse(SplitData[3]);
                rpos = int.Parse(SplitData[4]);

            textBox2.Text = maxxval.ToString();
            textBox1.Text = minxval.ToString();
            textBox4.Text = maxyval.ToString();
            textBox3.Text = minyval.ToString();
                gList.Add(yval, xval);
                chart1.Series[0].Points.AddXY(0, rpos);
                chart2.Series[0].Points.AddXY(0, lpos);
                UpdateGraph();
                gList.Clear();
        }

        void FetchAvailablePorts()
        { 
            String[] ports = SerialPort.GetPortNames();
            AvailablePortsBox.Items.AddRange(ports);
        }

        private void ConnectPortButton_Click(object sender, EventArgs e)
        {

            SerPort= new SerialPort();
            SerPort.BaudRate = 115200;
            SerPort.PortName = AvailablePortsBox.Text;
            SerPort.Parity = Parity.None;
            SerPort.DataBits = 8;
            SerPort.StopBits = StopBits.One;
            SerPort.ReadBufferSize= 1024;
            SerPort.DataReceived += SerPort_DataReceived;

            try
            {
                SerPort.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }

            
        }

        private void SerPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            RecievedData = SerPort.ReadLine();

            this.Invoke(new Action(ProcessingData));
        }

        private void ProcessingData()
        {

           if(RecievedData.Contains('<')&&RecievedData.Contains('>'))
            {

            SplitData = RecievedData.Split(';');

                

            TextBoxValue1.Text= SplitData[1];
            TextBoxValue2.Text= SplitData[2];
            TextBoxValue3.Text= SplitData[3];
            TextBoxValue4.Text= SplitData[4];

            this.Invoke(new Action(ConvertAndFill));

            Array.Clear(SplitData,0,SplitData.Length);
           }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SerPort.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void DisconnectPortButton_Click(object sender, EventArgs e)
        {
            try
            {
                SerPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "0";
            textBox2.Text = "0";
            textBox3.Text = "0";
            textBox4.Text = "0";
        }
    }


}
