using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        //定义的成员变量，用于保存波形数据
        private List<double> data_display1 = new List<double>();
        private List<double> data_display2 = new List<double>();
        private List<double> data_display3 = new List<double>();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitChart();
        }

        private void InitChart()
        {
            //定义图表区域
            this.chart1.ChartAreas.Clear();
            ChartArea chartArea1 = new ChartArea("C1");
            this.chart1.ChartAreas.Add(chartArea1);
            //定义存储和显示点的容器
            this.chart1.Series.Clear();
            Series series1 = new Series("S1");
            Series series2 = new Series("S2");
            Series series3 = new Series("S3");
            series1.ChartArea = "C1";
            series2.ChartArea = "C1";
            series3.ChartArea = "C1";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            //设置图表显示样式
            this.chart1.ChartAreas[0].AxisX.ArrowStyle = AxisArrowStyle.Triangle;//设置坐标x轴箭头为三角
            this.chart1.ChartAreas[0].AxisY.ArrowStyle = AxisArrowStyle.Triangle;//设置坐标y轴箭头为三角

            //设置x轴的最大最小值
            //this.chart1.ChartAreas[0].AxisX.Minimum = 0;
            //this.chart1.ChartAreas[0].AxisX.Maximum = 100;
            this.chart1.ChartAreas[0].AxisX.Interval = 100;//x轴刻度间隔大小

            //设置y轴的最大最小值
            this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            this.chart1.ChartAreas[0].AxisY.Maximum = 100;
            this.chart1.ChartAreas[0].AxisY.Interval = 10;//x轴刻度间隔大小

            //设置xy轴的颜色
            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;

            //不设置标题
            this.chart1.Titles.Clear();

            //设置图表显示样式
            this.chart1.Series[0].Color = Color.Red;
            this.chart1.Series[1].Color = Color.Blue;
            this.chart1.Series[2].Color = Color.Green;

            //设置线形：
            //SeriesChartType.Spline
            //SeriesChartType.Line
            this.chart1.Series[0].ChartType = SeriesChartType.Line;
            this.chart1.Series[1].ChartType = SeriesChartType.Line;
            this.chart1.Series[2].ChartType = SeriesChartType.Line;

            //开始清空数据
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            this.chart1.Series[2].Points.Clear();

        }
        /// <summary>
        /// 生成假的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //接收数据到list中，假的数据
            Random r = new Random();
            data_display1.Add(r.NextDouble() * 10);
            data_display2.Add(r.NextDouble() * 20 + 10);
            data_display3.Add(r.NextDouble() * 20 + 40);

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //更新显示波形，先删除之前的数据，然后将目前的数据显示出来
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            this.chart1.Series[2].Points.Clear();
            for (int i = 0; i < data_display1.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY((i + 1), data_display1.ElementAt(i));
                this.chart1.Series[1].Points.AddXY((i + 1), data_display2.ElementAt(i));
                this.chart1.Series[2].Points.AddXY((i + 1), data_display3.ElementAt(i));
            }

        }
    }
}
