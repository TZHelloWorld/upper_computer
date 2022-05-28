using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        #region 全局变量
        /// <summary>
        /// 全局变量
        /// </summary>
        private SerialPort sp = null;       //声明一个串口类
        private bool isOpen = false;        //打开串口标志
        private bool isSetProperty = false; //属性设置标志
        private bool isHex = false;         //十六进制显示标志接收
        private bool isHexTx = false;       //十六进制显示标志发送标志
 
        /// <summary>
        /// 波形显示数据定义
        /// </summary>
        /// 
        //如果是全部显示使用list数组
        //private List<double> data_display1 = new List<double>();
        //private List<double> data_display2 = new List<double>();
        //private List<double> data_display3 = new List<double>();

        //如果显示的数据每次都只显示100个（固定个数的）数据，然后动起来的，使用队列
        private static int show_len = 100;//定义每次显示数据的长度
        private Queue<double> data_display1 = new Queue<double>(show_len);
        private Queue<double> data_display2 = new Queue<double>(show_len);
        private Queue<double> data_display3 = new Queue<double>(show_len);


        #endregion



        public Form1()
        {
            InitializeComponent();  //窗口初始化，net自动生成
        }

        #region 打开软件载入数据函数        
        /// <summary>
        /// 打开软件载入数据函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            checkCOM();

            //cbxCOMPort.SelectedIndex = 0;
            //列出常用的波特率
            cbxBaudBate.Items.Add("1200");
            cbxBaudBate.Items.Add("2400");
            cbxBaudBate.Items.Add("4800");
            cbxBaudBate.Items.Add("9600");
            cbxBaudBate.Items.Add("19200");
            cbxBaudBate.Items.Add("38400");
            cbxBaudBate.Items.Add("43000");
            cbxBaudBate.Items.Add("56000");
            cbxBaudBate.Items.Add("57600");
            cbxBaudBate.Items.Add("115200");
            cbxBaudBate.SelectedIndex = 3;//设置默认选中
            //列出停止位
            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;//设置默认选中
            //列出数据位
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;//设置默认选中
            //列出奇偶校验
            cbxparity.Items.Add("无");
            cbxparity.Items.Add("奇校验");
            cbxparity.Items.Add("偶校验");
            cbxparity.SelectedIndex = 0;//设置默认选中

            //默认为char显示，标志位，单选框
            rbnChar.Checked = true;

            //默认为char发送
            tbnChar.Checked = true;

            //初始化图形
            InitChart();
        }
        #endregion


        #region 串口检测
        /// <summary>
        /// 串口检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkCOM() {
            bool comExistence = false;      //有可用串口标志
            cbxCOMPort.Items.Clear();       //清除当前串口号中的所有串口名称


            string[] sPorts = SerialPort.GetPortNames();//获取所有的能用的串口的名字
            //cbxCOMPort.Items.Add("");//显示空字符的串口。
            foreach (string port in sPorts)
            {
                cbxCOMPort.Items.Add(port);
                comExistence = true;
            }
            

            if (comExistence)
            {
                cbxCOMPort.SelectedIndex = 0;   //使ListBox显示第1个添加的索引
            }
            else
            {
                MessageBox.Show("没有找到可用的串口！", "错误提示");
            }
        }

        private void btnCheckCOM_Click(object sender, EventArgs e)
        {
            checkCOM();
        }
        #endregion


        #region 私有函数，检测串口是否设置         
        /// <summary>
        /// 私有函数，检测串口是否设置 
        /// </summary>
        /// <returns></returns>
        private bool CheckPortSerring()
        {
            if (cbxCOMPort.Text.Trim() == "") return false;
            if (cbxBaudBate.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            if (cbxparity.Text.Trim() == "") return false;
            if (cbxStopBits.Text.Trim() == "") return false;
            return true;
        }
        #endregion


        #region 私有函数，检测发送数据
        /// <summary>
        /// 私有函数，检测发送数据
        /// </summary>
        /// <returns></returns>
        private bool CheckSendData()
        {
            if (tbxSendData.Text.Trim() == "") return false;
            return true;
        }
        #endregion


        #region 私有函数,软件启动设置串口的属性
        /// <summary>
        /// 私有函数,软件启动设置串口的属性
        /// </summary>
        private void SetPortProperty()  //
        {
            sp = new SerialPort();

            sp.PortName = cbxCOMPort.Text.Trim();  //设置串口名

            sp.BaudRate = Convert.ToInt32(cbxBaudBate.Text.Trim()); //设置串口的波特率

            float f = Convert.ToSingle(cbxStopBits.Text.Trim());    //设置停止位

            if (f == 0)
            {
                sp.StopBits = StopBits.None;
            }
            else if (f == 1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (f == 1)
            {
                sp.StopBits = StopBits.One;
            }
            else if (f == 2)
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim()); //设置数据位

            string s = cbxparity.Text.Trim();   //设置奇偶检验为
            if (s.CompareTo("无") == 0)
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }

            sp.ReadTimeout = -1;    //设置超时读取时间
            sp.RtsEnable = true;

            //定义DataReceived 事件，当串口接收到数据后触发事件
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived); //该方法加载就运行
            sp.Encoding = System.Text.Encoding.GetEncoding("UTF-8");               //根据实际情况选择UTF-8还是GB2312(串口显示中文)
            if (rbnHex.Checked)  //接收字符或者hex选择
            {
                isHex = true;
            }
            else
            {
                isHex = false;
            }

            if (tbnHex.Checked) //发送字符或者hex选择
            {
                isHexTx = true;
            }
            else
            {
                isHexTx = false;
            }
        }
        #endregion


        #region 数据发送函数       
        /// <summary>
        /// 数据发送函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (isOpen)  //写串口数据
            {
                try
                {
                    if (isHexTx == false)//如果发送模式为字符模式
                    {
                        //sp.Write(tbxSendData.Text.ToString());                  //发送一串字符                     
                        //sp.WriteLine(tbxSendData.Text);                         //发送一串字符

                        Encoding gb = System.Text.Encoding.GetEncoding("UTF-8");   //发送字符
                        Byte[] writeBytes = gb.GetBytes(tbxSendData.Text);
                        sp.Write(writeBytes, 0, writeBytes.Length);
                    }
                    else//如果发送模式为HEX模式
                    {
                        string dataTxStr = tbxSendData.Text.ToString(); ;     //将textbox1 转化为 字符串
                        byte[] byte_hexTx = new byte[] { };
                        byte_hexTx = strToToHexByte(dataTxStr.ToString());    //字符串转化为16进制数组
                        sp.Write(byte_hexTx, 0, byte_hexTx.Length);           //发送数组
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("1、软件故障！\r\n" +
                        "2、发送十六进制高低位需填满！", "错误提示");

                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开!", "提示错误");
                return;
            }

            if (!CheckSendData())    //检测要发送的数据
            {
                MessageBox.Show("请输入要发送的数据！", "错误提示");
                return;
            }
        }
        #endregion


        #region 打开串口按键
        /// <summary>
        /// 打开串口按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //打开串口
        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSerring()) //检测串口设置
                {
                    MessageBox.Show("串口未设置", "错误提示");
                    return;
                }

                if (!isSetProperty)  //串口未设置则设置串口
                {
                    SetPortProperty();
                    isSetProperty = true;
                }

                try //打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "关闭串口";
                    //串口打开后则相关的串口设置按钮便不可再用
                    cbxCOMPort.Enabled = false;
                    cbxBaudBate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxparity.Enabled = false;
                    cbxStopBits.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHex.Enabled = false;
                    tbnChar.Enabled = false;
                    tbnHex.Enabled = false;

                }
                catch (Exception)
                {
                    //串口打开失败后，相应标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或已被占用!", "错误提示");
                }
            }
            else  //关闭串口
            {
                try
                {
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpenCom.Text = "打开串口";
                    cbxCOMPort.Enabled = true;
                    cbxBaudBate.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxparity.Enabled = true;
                    cbxStopBits.Enabled = true;
                    rbnChar.Enabled = true;
                    rbnHex.Enabled = true;
                    tbnChar.Enabled = true;
                    tbnHex.Enabled = true;

                }
                catch (Exception)
                {
                    MessageBox.Show("关闭时发生错误！", "错误提示");

                }
            }
        }
        #endregion


        #region 串口接收处理函数       
        /// <summary>
        /// 串口接收处理函数
        /// </summary>
        /// <param name="sendr"></param>
        /// <param name="e"></param>
        private void sp_DataReceived(object sendr, SerialDataReceivedEventArgs e)
        {
            try
            {
                Control.CheckForIllegalCrossThreadCalls = false;//线程间操作无效报错，需要添加这一句
                Byte[] ReceivedData = new Byte[sp.BytesToRead]; //创建接收字节数组
                String RecvDataText = null;                     //显示字符存储
                
                sp.Read(ReceivedData, 0, ReceivedData.Length);  //读取所有接收到的数据

                if (isHex == false)//如果接收模式为字符模式
                {
                    String decodedString = Encoding.Default.GetString(ReceivedData); //设置编码格式，显示汉字                                
                    tbxRecvData.Text += decodedString + " "; //接收框显示，追加数据  
                }
                else//如果接收模式为HEX数值模式
                {
                    for (int i = 0; i < ReceivedData.Length; i++)
                    {
                        RecvDataText += ("0x" + ReceivedData[i].ToString("X2") + " "); //显示前缀0x
                        //RecvDataText += (ReceivedData[i].ToString("x2") + " "); //不显示前缀0x
                    }
                    tbxRecvData.Text += RecvDataText;
                }

                //对数据进行保存处理，这里得看具体传过来的数据是什么。否则不好处理啊。
                // 1. 为了显示波形，所以存储到list中或者queue中


                // 2. 将保存的数据持久化到文件中（追加），这里暂时写成txt文件
                File.AppendAllText("./log.txt", "数据。。。"+ "\r\n");//追加到txt文件中

                //如果要追加到csv文件中，可以参考:https://www.delftstack.com/zh/howto/csharp/how-to-write-data-into-a-csv-file-in-csharp/


                tbxRecvData.Select(tbxRecvData.TextLength, 0);                   //文本显示最后一行
                tbxRecvData.ScrollToCaret();                                     //滚动到最后一行                
                
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion


        #region 清除发送数据
        /// <summary>
        /// 清除发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void btnCleanData_Click(object sender, EventArgs e)
        {
            tbxSendData.Text = "";
        }
        #endregion


        #region 字符串转16进制数组
        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");//去除空格，将接受到的hexStringde 
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        #endregion


        #region 清空接收数据
        /// <summary>
        /// 清空接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCleanRxData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";      //清空接收数据
        }
        #endregion


        #region 初始化波形显示区域
        /// <summary>
        /// 初始化图表
        /// </summary>
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



        #endregion
        /// <summary>
        /// 定时器函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        //更新数据，怎么说呢，传输过来的数据保存到list当中，或者queue中，函数都一样
        private void timer_display_Tick(object sender, EventArgs e)
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
        /// <summary>
        /// 生成假的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //接收数据到queue中，假的数据
            Random r = new Random();
            int len = r.Next(0, 5);

            for (int i = 0; i < len; i++)
            {
                if (data_display1.Count > show_len)
                {
                    data_display1.Dequeue();
                    data_display2.Dequeue();
                    data_display3.Dequeue();
                }
                data_display1.Enqueue(r.NextDouble() * 10);          
                data_display2.Enqueue(r.NextDouble() * 20 + 10);
                data_display3.Enqueue(r.NextDouble() * 20 + 40);
            }

            

        }
    }




}
