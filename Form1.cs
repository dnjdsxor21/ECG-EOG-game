using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Mitov.PlotLab;

namespace BME_system
{
    public partial class Form1 : Form
    {
        SerialPort sPort;
        int[] data_buff = new int[60];
        static int buffsize = 2000;   //8 sec  
        static double fs = 250;
        double[] input_Data_1 = new double[buffsize];
        double[] input_Data_2 = new double[buffsize];
        double[] input_Data_3 = new double[buffsize];
        public double[] input_Draw_1 = new double[buffsize];
        public double[] input_Draw_2 = new double[buffsize];

        int start_byte = 0;
        int start_flag = 0;
        int data_count = 0;
        int eog_count = 0;
       
        int Data_1, Data_2, Data_3, Data_4, Data_5, Data_6;
        string thisdate = DateTime.Now.ToString("yyMMdd");
        
        int bpm = 60;
        double[] peak_ecg = new double[buffsize];
        int[] peak_idx = new int[20];
        double peak_max = 50;
        
        int x = 0;
        bool up_ing = false;
        bool down_ing = false;
        int posNow = 1;
        int[] top = new int[3];
        int moveSpeed = 0;

        int score = 0;
        int obstacleSpeed = 50;
        Random rand = new Random();
        int position;
        bool isGameOver = false;



        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnOpen.Enabled = true;
            btnClose.Enabled = false;

            cboPortName.BeginUpdate();
            foreach (string comport in SerialPort.GetPortNames())
            {
                cboPortName.Items.Add(comport);
            }
            cboPortName.EndUpdate();

            cboPortName.SelectedItem = "COM11";
            txtBaudRate.Text = "115200";

            CheckForIllegalCrossThreadCalls = false;
            txtDate.Text = thisdate;
         
            //Game Location Setting
            gameTimer.Stop();
            obstacle1.Location = new Point(obstacle1.Location.X, Line1.Location.Y + 8);
            Line2.Location = new Point(0, obstacle1.Location.Y + 43);
            obstacle2.Location = new Point(obstacle2.Location.X, Line2.Location.Y + 8);
            Line3.Location = new Point(0, obstacle2.Location.Y + 43);
            obstacle3.Location = new Point(obstacle3.Location.X, Line3.Location.Y + 8);
            Line4.Location = new Point(0, obstacle3.Location.Y + 43);
            trex.Location = new Point(trex.Location.X, obstacle2.Location.Y);
            border.Location = new Point(Line1.Width, Line1.Location.Y);
            top[0] = obstacle1.Location.Y; top[1] = obstacle2.Location.Y; top[2] = obstacle3.Location.Y;
        }
        private void eog_filter(double eog_in)
        {
            double threshold = 250;
                
                if (eog_in > threshold && eog_count > 39)
                {
                    txtEye.Text = "UP";
                    up_ing = true;
                    down_ing = false;
                }
                else if (eog_in < -threshold && eog_count > 39)
                {
                    txtEye.Text = "DOWN";
                    down_ing = true;
                    up_ing = false;
                }
                else
                {
                    up_ing = false;
                    down_ing = false;
                }
            
        }
        public void ecg_filter(double[] ecg_in )
        {
            peak_max = 50;
            for (int i = 1000; i < buffsize-1; i++)
            {
                if (ecg_in[i] > peak_max) {  peak_max = ecg_in[i]; }
            }
            double peak_threshold = peak_max * 0.5;
            int j = 0;
            
            
            for (int i = 0; i < buffsize-1; i++)
            {
                if (ecg_in[i] > peak_threshold)
                {
                    peak_idx[j] = i;
                    j++;
                    i += 125;
                }
            }
            int k = 0;
            double peak_sum = 0;
            int n = 8;
            for( int i = 19; i>0;i--)
            {
                if(peak_idx[i] > 0)
                {
                    k++;
                    peak_sum += peak_idx[i]- peak_idx[i-1];
                    if (k > n-1) { break; }
                }
            }
            if (peak_sum > 0)  { bpm = 60 * (int) fs * n / (int) peak_sum;
            }
            txtBPM.Text = bpm.ToString();
           
        }

        private void btnPeak_Click(object sender, EventArgs e)
        {
           
           
        }
          

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (null == sPort)
                {
                    sPort = new SerialPort();
                    sPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);

                    sPort.PortName = cboPortName.SelectedItem.ToString();
                    sPort.BaudRate = Convert.ToInt32(txtBaudRate.Text);
                    sPort.DataBits = (int)8;
                    sPort.Parity = Parity.None;
                    sPort.StopBits = StopBits.One;
                    sPort.Open();
                }

                if (sPort.IsOpen)
                {
                    btnOpen.Enabled = false;
                    btnClose.Enabled = true;
                }
                else
                {
                    btnOpen.Enabled = true;
                    btnClose.Enabled = false;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (null != sPort)
            {
                if (sPort.IsOpen)
                {
                    sPort.Close();
                    sPort.Dispose();
                    sPort = null;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (null != sPort)
            {
                if (sPort.IsOpen)
                {
                    sPort.Close();
                    sPort.Dispose();
                    sPort = null;
                }
            }
            
            btnOpen.Enabled = true;
            btnClose.Enabled = false;
        }

        
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while (sPort.BytesToRead > 0)
            {

                if (sPort.IsOpen)
                {
                    if (start_flag == 0)
                    {
                        start_byte = sPort.ReadByte();
                    }
                }

                if (start_byte == 0x81)
                {
                    start_flag = 1;
                    data_buff[data_count] = sPort.ReadByte();

                    data_count++;

                    if (data_count == 11)
                    {
                                              
                        Data_1 = ((data_buff[8] & 0x7f) << 7) + (data_buff[9] & 0x7f) - 7000;
                        Data_2 = ((data_buff[0] & 0x7f) << 7) + (data_buff[1] & 0x7f) - 7000 -4100- x;
                        Data_3 = ((data_buff[2] & 0x7f) << 7) + (data_buff[3] & 0x7f) - 7000;

                        Data_4 = ((data_buff[4] & 0x7f) << 7) + (data_buff[5] & 0x7f) - 7000;
                        Data_5 = ((data_buff[6] & 0x7f) << 7) + (data_buff[7] & 0x7f) - 7000;
                        Data_6 = ((data_buff[10] & 0x7f) << 7) + (data_buff[11] & 0x7f) - 7000;
                        
                        start_flag = 2;
                        data_count = 0;
                    }

                    if (start_flag == 2)
                    {
                        for (int i = 0; i < buffsize - 1; i++)
                        {
                            input_Data_1[i] = input_Data_1[i + 1];
                            input_Data_2[i] = input_Data_2[i + 1];
                            input_Data_3[i] = input_Data_3[i + 1];
                        }
                        // ECG - 30Hz LPF
                        input_Data_1[buffsize - 1] = Data_1;
                        input_Draw_1 = input_Data_1;

                        // EOG - 10Hz LPF               
                        input_Data_2[buffsize - 1] = Data_2;
                        input_Draw_2 = input_Data_2;
                        eog_filter(Data_2);

                        // ECG - 5-30Hz BandPass
                        input_Data_3[buffsize - 1] = Data_3;
                        ecg_filter(input_Data_3);

                        start_flag = 0;
       
                    }

                }

            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            scope1.Channels[0].Data.SetYData(input_Draw_1);
            scope1.Channels[1].Data.SetYData(input_Data_3);
            scope2.Channels[0].Data.SetYData(input_Draw_2);

        }

        private void btnBaseline_Click(object sender, EventArgs e)
        {
            x = Convert.ToInt32(input_Data_2[buffsize - 1]);                   }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            txt.Text = e.KeyCode.ToString();
            if (e.KeyCode == Keys.W && up_ing == false && down_ing == false)
            {
                up_ing = true;
            }
            else if (e.KeyCode == Keys.S && up_ing == false && down_ing == false)
            {
                down_ing = true;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            txt.Text = e.KeyCode.ToString();
            if (e.KeyCode == Keys.R && isGameOver == true)
            {
                GameReset();
            }
        }
        private void btnGame_Click(object sender, EventArgs e)
        {
            GameReset();
        }
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            trex.Top += moveSpeed;
            txtScore.Text = "Score:  " + score;

            // move and stop
            if (moveSpeed != 0)
            {
                if (trex.Top == top[0])
                {
                    posNow = 0;
                    moveSpeed = 0;
                    up_ing = false;
                    down_ing = false;
                }
                else if (trex.Top == top[1])
                {
                    posNow = 1;
                    moveSpeed = 0;
                    up_ing = false;
                    down_ing = false;
                }
                else if (trex.Top == top[2])
                {
                    posNow = 2;
                    moveSpeed = 0;
                    up_ing = false;
                    down_ing = false;
                }
            }

                // move up
                if (posNow != 0 && up_ing == true && down_ing == false) { moveSpeed = -17; eog_count = 0; }
                else if (posNow == 0 && up_ing == true && down_ing == false) { up_ing = false; eog_count = 0; }

                // move down
                if (posNow != 2 && down_ing == true && up_ing == false) { moveSpeed = 17; eog_count = 0; }
                else if (posNow == 2 && down_ing == true && up_ing == false) { down_ing = false; eog_count = 0; }

                if (eog_count < 40) { eog_count++; }
           
            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "obstacle")
                {
                    
                    x.Left -= obstacleSpeed;

                    if (x.Left < -100)
                    {
                        x.Left = this.ClientSize.Width + (x.Width * 15) + rand.Next(200,1200) ;
                        score++;
                    }

                    if (trex.Bounds.IntersectsWith(x.Bounds))
                    {
                        gameTimer.Stop();
                        trex.Image = Properties.Resources.dead;
                        txtScore.Text = " Press R to restart the game!";
                        isGameOver = true;
                    }
                }
            }

         

        }
        private void GameReset()
        {
            moveSpeed = 0;
            up_ing = false; down_ing = false;
            score = 0;
            obstacleSpeed = (int)bpm/12;
            txtScore.Text = "Score:  " + score;
            trex.Image = Properties.Resources.running;
            isGameOver = false;
            trex.Top = top[1];
            
            foreach (Control x in this.Controls)
            {

                if (x is PictureBox && (string)x.Tag == "obstacle")
                {
                    position = this.ClientSize.Width + (x.Width * 10) + rand.Next(200, 1200);

                    x.Left = position;
                   
                }
            }

            gameTimer.Start();

        }
    }
    

}
