using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;

namespace laba3
{
    public partial class Form1 : Form
    {
        #region Parameters

        public Parameters parameter = new Parameters();

        public static bool FlagOfMenu;
        public static bool FlagOfStop = false;   //задача снята, ответ из формы
        public static bool FlagOfContinue = false; //задача продолжена, ответ из формы
        public static bool FlagOfForm = false; //форма выведена?
        public static bool FlagSystemIsBusy = false;    //есть задача на выполнение?

        bool Mod = false;   //идет процесс моделирования?
        bool ModPaused = false; //моделирование приостановлено?
        int ModTime = 0;    //время моделирования

        int kolF = 0;
        int p = -1;

        public int[] Tplan = new int[4] { -1, -1, -1, -1 }; //Тплановое задачи
        int[] WaitingTime = { 0, 0, 0, 0 }; //время ожидания задачи
        int[] CurrSessionTime = { 0, 0, 0, 0 };//текущее время работы задачи
        int[] State = new int[4];   //состояние задач
        int jj = -1;

        public int startSession;    //время сеанса задачи
        int StartTime1; //время начала работы задачи
        int StartTime2;
        int StartTime3;
        int StartTime4;

        public float xK = 0.0F;
        public float yK = 0.0F;
        public float HWin = 0.0F;

        public enum Status : int
        {
            INACTIVE,   //за пределами интервала активности
            WAIT_TPLAN, //ждет Тплановое
            WAIT_WORKING,   //ожидает управления до Тз
            WORKING,    //выполняется
            SUSPENDED,  //приостановлена
        };

        //Редактирование параметров
        public Parameters buf = new Parameters();
        public static bool fl_edit = false;

        #endregion //regionParameters

        #region Form

        //форма
        public Form1()
        {
            InitializeComponent();
        }

        //всплывающие подсказки
        void tooltip_on()
        {
            ToolTip t1 = new ToolTip();
            t1.SetToolTip(pictureBox5, "Start of task activity interval");
            ToolTip t2 = new ToolTip();
            t2.SetToolTip(pictureBox6, "End of task activity interval");
            ToolTip t4 = new ToolTip();
            t4.SetToolTip(pictureBox7, "Scheduled time for the next start of the task execution");
            ToolTip t5 = new ToolTip();
            t5.SetToolTip(pictureBox9, "A task call timed out. \nThe request was issued to the user interface.\nThe task is suspended");
            ToolTip t6 = new ToolTip();
            t6.SetToolTip(pictureBox10, "The user has responded to a previously issued request:\n'Continue execution'");
            ToolTip t3 = new ToolTip();
            t3.SetToolTip(pictureBox8, "The user has responded to a previously issued request:\n'Cancel the task'");
            ToolTip t7 = new ToolTip();
            t7.SetToolTip(pictureBox11, "A task session timed out. \nТkr increased value by N=5");
            ToolTip t8 = new ToolTip();
            t8.SetToolTip(TnAll, "Start of task activity interval");
            ToolTip t9 = new ToolTip();
            t9.SetToolTip(TpAll, "Period of timing task calling");
            ToolTip t10 = new ToolTip();
            t10.SetToolTip(TzAll, "Acceptable delay (waiting time for the task to get control)");
            ToolTip t11 = new ToolTip();
            t11.SetToolTip(TkrAll, "Maximum allowed run time for a task");
            ToolTip t12 = new ToolTip();
            t12.SetToolTip(PrAll, "Priority (lower value gives higher priority), range of values: 0...3");
            ToolTip t13 = new ToolTip();
            t13.SetToolTip(TcAll, "Session time (time for which the task performs it functions");
            ToolTip t14 = new ToolTip();
            t14.SetToolTip(TkAll, "End of task activity interval");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tooltip_on();

            Form2 TZonstart = new Form2();
            TZonstart.ShowDialog();
            if (!FlagOfMenu) this.Dispose();

            timer1.Enabled = true;
            DateTime date1 = DateTime.Now;
            label2.Invoke(new Action(() => { Tastr.Text = date1.ToString("HH:mm:ss t"); }));

            HWin = FunctionBox.Height;
            yK = HWin * 0.82F;

            buttonModify.Enabled = false;
            buttonPause.Enabled = false;
            buttonStop.Enabled = false;
            buttonStart.Enabled = true;
            Tmod.Text = "0";

            bwtask1.WorkerReportsProgress = true;
            bwtask1.WorkerSupportsCancellation = true;
            bwtask2.WorkerReportsProgress = true;
            bwtask2.WorkerSupportsCancellation = true;
            bwtask3.WorkerReportsProgress = true;
            bwtask3.WorkerSupportsCancellation = true;
            bwtask4.WorkerReportsProgress = true;
            bwtask4.WorkerSupportsCancellation = true;
        }

        #endregion //region Form

        #region Buttons

        Parameters parameters;
        //Start
        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (!error_param_mesenger())
            {
                //доступность кнопок
                buttonPause.Text = "Pause";
                buttonModify.Enabled = true;
                buttonPause.Enabled = true;
                buttonStop.Enabled = true;
                buttonStart.Enabled = false;

                parameters = new Parameters();
                if (ReadParameters(parameters))
                {
                    //блокировка доступных для редактирования полей
                    List<GroupBox> gb = new List<GroupBox>();
                    gb.Add(TnAll);
                    gb.Add(TcAll);
                    gb.Add(TkrAll);
                    foreach (GroupBox g in gb)
                    {
                        foreach (TextBox t in g.Controls)
                        {
                            t.ReadOnly = true;
                        }
                    }
                    Tp2.ReadOnly = true;
                    Tp3.ReadOnly = true;
                    Tp1.ReadOnly = true;
                    Tz3.ReadOnly = true;
                    Tz2.ReadOnly = true;
                    Tz1.ReadOnly = true;
                    Pr2.ReadOnly = true;
                    Pr3.ReadOnly = true;
                    Pr1.ReadOnly = true;

                    //моделирование
                    ModTime = 0;    //начало моделирования
                    Mod = true;
                    ModPaused = false;

                    for (int i = 0; i < 4; i++)
                        Tplan[i] = -1;

                    parameter.SetUpInBegin(parameters); //получение начальных параметров  
                    startSession = parameters.Tc[0];

                    WritePicture(parameter.Tn[0], "start.png", pictureBox1, 0.45F);
                    WritePicture(parameter.Tn[1], "start.png", pictureBox2, 0.45F);
                    WritePicture(parameter.Tn[2], "start.png", pictureBox3, 0.45F);
                    WritePicture(parameter.Tn[3], "start.png", pictureBox4, 0.45F);

                    FlagSystemIsBusy = false;
                }
            }
        }

        //Modify
        private void buttonModify_Click(object sender, EventArgs e)
        {
            fl_edit = true;
            //приостановка таймера
            Mod = false;
            ModPaused = true;

            //разблокировка доступных для редактирования полей
            List<GroupBox> gb = new List<GroupBox>();
            gb.Add(TcAll);
            gb.Add(TkrAll);
            foreach (GroupBox g in gb)
            {
                foreach (TextBox t in g.Controls)
                {
                    t.ReadOnly = false;
                }
            }

            foreach (TextBox t in TnAll.Controls)
            {
                if (Convert.ToInt32(t.Text) > ModTime)
                {
                    t.ReadOnly = false;
                }
            }

            Tp2.ReadOnly = false;
            Tp3.ReadOnly = false;
            Tp1.ReadOnly = false;
            Tz3.ReadOnly = false;
            Tz2.ReadOnly = false;
            Tz1.ReadOnly = false;
            Pr2.ReadOnly = false;
            Pr3.ReadOnly = false;
            Pr1.ReadOnly = false;

            //доступность кнопок
            buttonPause.Text = "Resume";
            buttonModify.Enabled = false;

            buf.CopyIn(parameter);

            WritePicture(ModTime, "edit.png", pictureBox1, 0.25F);
            WritePicture(ModTime, "edit.png", pictureBox2, 0.25F);
            WritePicture(ModTime, "edit.png", pictureBox3, 0.25F);
            WritePicture(ModTime, "edit.png", pictureBox4, 0.25F);
        }

        //Pause
        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (buttonPause.Text == "Pause")
            {
                //приостанов работы таймера
                Mod = false;
                ModPaused = true;

                //доступность кнопок
                buttonPause.Text = "Resume";
            }
            else
            {
                if (buttonPause.Text == "Resume")
                {
                    if (!fl_edit)
                    {
                        if (!error_param_mesenger())
                        {
                            //возобновление работы таймера
                            ModPaused = false;
                            Mod = true;

                            //блокировка доступных для редактирования полей
                            List<GroupBox> gb = new List<GroupBox>();
                            gb.Add(TnAll);
                            gb.Add(TcAll);
                            gb.Add(TkrAll);
                            foreach (GroupBox g in gb)
                            {
                                foreach (TextBox t in g.Controls)
                                {
                                    t.ReadOnly = true;
                                }
                            }
                            Tp2.ReadOnly = true;
                            Tp3.ReadOnly = true;
                            Tp1.ReadOnly = true;
                            Tz3.ReadOnly = true;
                            Tz2.ReadOnly = true;
                            Tz1.ReadOnly = true;
                            Pr2.ReadOnly = true;
                            Pr3.ReadOnly = true;
                            Pr1.ReadOnly = true;

                            //доступность кнопок
                            buttonPause.Text = "Pause";
                            buttonModify.Enabled = true;
                        }
                    }
                    else
                        if (fl_edit)
                    {
                        if (Tplan[0] == -1 || Tplan[0] - (parameter.Tp[0] - Convert.ToInt32(Tp1.Text)) > ModTime)
                            if (Tplan[1] == -1 || Tplan[1] - (parameter.Tp[1] - Convert.ToInt32(Tp2.Text)) > ModTime)
                                if (Tplan[2] == -1 || Tplan[2] - (parameter.Tp[2] - Convert.ToInt32(Tp3.Text)) > ModTime)
                                {
                                    Tplan[0] = Tplan[0] - (parameter.Tp[0] - Convert.ToInt32(Tp1.Text));
                                    WritePicture(Tplan[0], "vypolnenie.png", pictureBox1, 0.45F);
                                    Tplan[1] = Tplan[1] - (parameter.Tp[1] - Convert.ToInt32(Tp2.Text));
                                    WritePicture(Tplan[1], "vypolnenie.png", pictureBox2, 0.45F);
                                    Tplan[2] = Tplan[2] - (parameter.Tp[2] - Convert.ToInt32(Tp3.Text));
                                    WritePicture(Tplan[2], "vypolnenie.png", pictureBox3, 0.45F);
                                    if (!error_param_mesenger_after_edit())
                                    {
                                        if (!error_param_mesenger())
                                        {
                                            int i = 0;
                                            foreach (TextBox t in TnAll.Controls)
                                            {
                                                PictureBox pb = new PictureBox();
                                                switch (i)
                                                {
                                                    case 0: pb = pictureBox1; break;
                                                    case 1: pb = pictureBox2; break;
                                                    case 2: pb = pictureBox3; break;
                                                    case 3: pb = pictureBox4; break;
                                                    default: break;
                                                }
                                                if (Convert.ToInt32(t.Text) != parameter.Tn[i])
                                                    WritePicture(Convert.ToInt32(t.Text), "start.png", pb, 0.45F);
                                                i++;
                                            }

                                            ReadParameters(parameter);

                                            //возобновление работы таймера
                                            ModPaused = false;
                                            Mod = true;

                                            //блокировка доступных для редактирования полей
                                            List<GroupBox> gb = new List<GroupBox>();
                                            gb.Add(TnAll);
                                            gb.Add(TcAll);
                                            gb.Add(TkrAll);
                                            foreach (GroupBox g in gb)
                                            {
                                                foreach (TextBox t in g.Controls)
                                                {
                                                    t.ReadOnly = true;
                                                }
                                            }
                                            Tp2.ReadOnly = true;
                                            Tp3.ReadOnly = true;
                                            Tp1.ReadOnly = true;
                                            Tz3.ReadOnly = true;
                                            Tz2.ReadOnly = true;
                                            Tz1.ReadOnly = true;
                                            Pr2.ReadOnly = true;
                                            Pr3.ReadOnly = true;
                                            Pr1.ReadOnly = true;

                                            //доступность кнопок
                                            buttonPause.Text = "Pause";
                                            buttonModify.Enabled = true;
                                            fl_edit = false;
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("New T_scheduled cannot be set less than the current simulation time!");
                                }
                    }
                }
            }
        }

        //Stop
        private void buttonStop_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (State[i] == (int)Status.WORKING)
                    switch (i)
                    {
                        case 0:
                            bwtask1.CancelAsync();
                            break;
                        case 1:
                            bwtask2.CancelAsync();
                            break;
                        case 2:
                            bwtask3.CancelAsync();
                            break;
                        case 3:
                            bwtask4.CancelAsync();
                            break;
                        default: break;
                    }
                State[i] = (int)Status.INACTIVE;
            }

            //остановка таймера
            Mod = false;
            ModTime = 0;
            ModPaused = false;
            FlagOfForm = false;
            xK = 0.0F;
            yK = 0.0F;
            HWin = FunctionBox.Height;
            yK = HWin * 0.82F;

            //разблокировка доступных для редактирования полей
            List<GroupBox> gb = new List<GroupBox>();
            gb.Add(TnAll);
            gb.Add(TcAll);
            gb.Add(TkrAll);
            foreach (GroupBox g in gb)
            {
                foreach (TextBox t in g.Controls)
                {
                    t.ReadOnly = false;
                }
            }
            Tp2.ReadOnly = false;
            Tp3.ReadOnly = false;
            Tp1.ReadOnly = false;
            Tz3.ReadOnly = false;
            Tz2.ReadOnly = false;
            Tz1.ReadOnly = false;
            Pr2.ReadOnly = false;
            Pr3.ReadOnly = false;
            Pr1.ReadOnly = false;

            //доступность кнопок
            buttonModify.Enabled = false;
            buttonPause.Enabled = false;
            buttonStop.Enabled = false;
            buttonStart.Enabled = true;

            //очищение полей
            pictureBox1.Refresh();
            pictureBox2.Refresh();
            pictureBox3.Refresh();
            pictureBox4.Refresh();
            FunctionBox.Refresh();
            Tmod.Text = ModTime.ToString();

            //все задачи неактивны!
            for (int i = 0; i < 4; i++)
            {
                State[i] = (int)Status.INACTIVE;
            }
        }

        //ТЗ
        private void buttonTZ_Click(object sender, EventArgs e)
        {
            Form2 TZ = new Form2();
            TZ.ShowDialog();
        }

        #endregion //region Buttons

        #region Timer

        //таймер
        private void timer1_Tick(object sender, EventArgs e)
        {
            Tastr.Text = DateTime.Now.ToString("HH:mm:ss");

            if (Mod && ModTime <= 90)
            {
                Tc3.ReadOnly = false;
                Tc3.Text = parameter.Tc[2].ToString();
                Tc3.ReadOnly = true;
                Tkr3.ReadOnly = false;
                Tkr3.Text = parameter.Tkr[2].ToString();
                Tkr3.ReadOnly = true;

                Tmod.Text = (ModTime + 1).ToString();

                for (int i = 0; i < 4; i++)
                {
                    if (ModTime >= parameter.Tk[i] && State[i] != (int)Status.INACTIVE && parameter.Tk[i] != -1)
                    {
                        State[i] = (int)Status.INACTIVE;
                    }
                }

                if (FlagSystemIsBusy)
                {
                    for (int i = 0; i < 4; i++)
                    {   //если достигнут конец интервала активности, задача неактивна
                        if (ModTime == parameter.Tk[i] && State[i] != (int)Status.INACTIVE)
                        {
                            State[i] = (int)Status.INACTIVE;
                        }

                        //проверка начала выполнения задачи
                        if (i != 3)
                        {
                            if (ModTime == parameter.Tn[i])
                            {
                                State[i] = (int)Status.WAIT_WORKING;
                            }
                        }
                        if (i == 3)
                        {
                            if (ModTime == parameter.Tn[i])
                            {
                                State[i] = (int)Status.WAIT_TPLAN;
                            }
                        }
                        //проверка планового вызова временной задачи
                        if (i != 3)
                        {
                            if (Tplan[i] == ModTime && State[i] == (int)Status.WAIT_TPLAN)
                            {
                                State[i] = (int)Status.WAIT_WORKING;
                            }
                        }

                        if (State[i] == (int)Status.WAIT_WORKING)
                        {
                            WaitingTime[i]++;
                        }

                        //проверка на превышение времени ожидания
                        if (i != 3)
                        {
                            if (!FlagOfForm)
                            {
                                if (WaitingTime[i] > parameter.Tz[i])
                                {
                                    FlagOfForm = true;
                                    State[i] = (int)Status.SUSPENDED;
                                    SetWritePicture(i, ModTime, "zapros.png", 0.25F);
                                    Form3 Susp = new Form3(i);
                                    Susp.ShowDialog();
                                }
                            }
                        }

                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)///
                    {
                        //если задача была на выполнении, то следует сменить ее состояние на ожидание Тплан
                        if (State[i] == (int)Status.WORKING)
                        {
                            State[i] = (int)Status.WAIT_TPLAN;

                            if (kolF != 0) //для обеспечения буферизации прерываний
                            {
                                kolF--; //уменьшается количество необходимых вызовов функции, так как +1 произошел
                                State[3] = (int)Status.WAIT_WORKING;//поставить задачу по прерыванию на выполнение
                            }
                        }

                        //если наступило время начала интервала активности
                        if (i != 3)
                        {
                            if (ModTime == parameter.Tn[i])
                            {
                                State[i] = (int)Status.WAIT_WORKING;
                            }
                        }
                        if (i == 3)
                        {
                            if (ModTime == parameter.Tn[i])
                            {
                                State[i] = (int)Status.WAIT_TPLAN;
                            }
                        }

                        if (i != 3)
                        {
                            //проверка планового вызова временной задачи
                            if (Tplan[i] == ModTime && State[i] == (int)Status.WAIT_TPLAN)
                            {
                                State[i] = (int)Status.WAIT_WORKING;
                            }
                        }

                        if (CurrSessionTime[2] > parameter.Tkr[2])
                        {
                            parameter.Tkr[2] += 5;
                            WritePicture(ModTime, "prevyshenie.png", pictureBox3, 0.25F);
                        }
                    }

                    int candInd = -1;//кандидат на выполнение

                    //обработка для задачи по прерыванию
                    if (State[3] == (int)Status.WAIT_WORKING)
                    {
                        candInd = 3;
                        FlagSystemIsBusy = true;
                        State[candInd] = (int)Status.WORKING;
                        //WaitingTime[candInd] = 0;
                        StartTime4 = ModTime;
                        bwtask4.RunWorkerAsync();
                    }

                    //если нет задачи по прерыванию на выполнение, то идет выбор среди временных
                    if (candInd == -1)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (State[i] == (int)Status.WAIT_WORKING)
                            {
                                //найден первый кандидат
                                candInd = i;
                                break;
                            }
                        }
                        //если есть кандидат, перебираются все возможные
                        if (candInd != -1)
                        {
                            FlagSystemIsBusy = true;
                            for (int i = 0; i < 3; i++)
                            {
                                if (State[i] == (int)Status.WAIT_WORKING)
                                {
                                    //среди задач с состоянием на выполнение выбор по приоритету
                                    if (parameter.Priority[i] < parameter.Priority[candInd])
                                        candInd = i;
                                }
                            }
                            State[candInd] = (int)Status.WORKING;
                            WaitingTime[candInd] = 0;
                            switch (candInd)
                            {
                                case 0:
                                    StartTime1 = ModTime;
                                    bwtask1.RunWorkerAsync();
                                    SetTplan(candInd);
                                    break;
                                case 1:
                                    StartTime2 = ModTime;
                                    bwtask2.RunWorkerAsync();
                                    SetTplan(candInd);
                                    break;
                                case 2:
                                    StartTime3 = ModTime;
                                    bwtask3.RunWorkerAsync();
                                    SetTplan(candInd);
                                    break;
                                default: break;
                            }
                        }
                    }
                    //оставшиеся задачи с состоянием "ожидание выполнения"
                    for (int i = 0; i < 4; i++)
                    {
                        if (State[i] == (int)Status.WAIT_WORKING)
                        {
                            WaitingTime[i]++;
                        }

                        //проверка на превышение времени ожидания
                        if (i != 3)
                        {
                            if (!FlagOfForm)
                            {
                                if (WaitingTime[i] > parameter.Tz[i])
                                {
                                    FlagOfForm = true;
                                    State[i] = (int)Status.SUSPENDED;
                                    SetWritePicture(i, ModTime, "zapros.png", 0.25F);
                                    Form3 Susp = new Form3(i);
                                    Susp.ShowDialog();
                                }
                            }
                        }
                    }
                }

                //проверка на превышение времени ожидания
                for (int j = 0; j < 3; j++)
                {
                    if (State[j] == (int)Status.SUSPENDED)
                    {
                        if (FlagOfStop)
                        {
                            parameter.Tk[j] = ModTime;
                            State[j] = (int)Status.INACTIVE;
                            SetWritePicture(j, ModTime, "user_stop.png", 0.45F);
                            FlagOfForm = false;
                            WaitingTime[j] = 0;
                            FlagOfStop = false;
                        }
                        if (FlagOfContinue)
                        {
                            SetTplan(j);
                            State[j] = (int)Status.WAIT_TPLAN;
                            SetWritePicture(j, ModTime, "user_continue.png", 0.45F);
                            FlagOfForm = false;
                            WaitingTime[j] = 0;
                            FlagOfContinue = false;
                        }
                    }
                }

                //отрисовка состояния неактивных задач               
                for (int i = 0; i < 4; i++)
                {
                    DrawStatus(i);
                }

                if (!ModPaused) ModTime++;

            }
            if (ModTime >= 90)
            {
                buttonStart.Enabled = false;
                buttonModify.Enabled = false;
                buttonPause.Enabled = false;
                buttonStop.Enabled = true;

                Mod = false;
                ModTime = 0;
                FlagOfForm = false;
            }
        }

        #endregion //region Timer

        #region Drow

        //поля для вывода задач
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Pen myPen = new Pen(Color.Gray, (float)1);  //оси Х
            Pen myPen2 = new Pen(Color.Gray);   //оси У
            Font drawFont = new Font("Arial", 6);   //размер подписи
            SolidBrush br = new SolidBrush(Color.Black);    //цвет шрифта
            float H = e.ClipRectangle.Height;
            float W = (float)e.ClipRectangle.Width / 90;

            //рисование осей для задач с заданными шагами StepX и StepY
            e.Graphics.DrawLine(myPen, W * 90, H * 0.85F, 0, H * 0.85F);
            for (int j = 0; j < 90; j++)
            {
                e.Graphics.DrawLine(myPen2, W * j, H * 5, W * j, 0);
                e.Graphics.DrawString(j.ToString(), drawFont, br, W * j, H * 0.87F);
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Pen myPen = new Pen(Color.Gray, (float)1);  //оси Х
            Pen myPen2 = new Pen(Color.Gray);   //оси У
            Font drawFont = new Font("Arial", 6);   //размер подписи
            SolidBrush br = new SolidBrush(Color.Black);    //цвет шрифта
            float H = e.ClipRectangle.Height;
            float W = (float)e.ClipRectangle.Width / 90;

            //рисование осей для задач с заданными шагами StepX и StepY
            e.Graphics.DrawLine(myPen, W * 90, H * 0.85F, 0, H * 0.85F);
            for (int j = 0; j < 90; j++)
            {
                e.Graphics.DrawLine(myPen2, W * j, H * 5, W * j, 0);
                e.Graphics.DrawString(j.ToString(), drawFont, br, W * j, H * 0.87F);
            }
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            Pen myPen = new Pen(Color.Gray, (float)1);  //оси Х
            Pen myPen2 = new Pen(Color.Gray);   //оси У
            Font drawFont = new Font("Arial", 6);   //размер подписи
            SolidBrush br = new SolidBrush(Color.Black);    //цвет шрифта
            float H = e.ClipRectangle.Height;
            float W = (float)e.ClipRectangle.Width / 90;

            //рисование осей для задач с заданными шагами StepX и StepY
            e.Graphics.DrawLine(myPen, W * 90, H * 0.85F, 0, H * 0.85F);
            for (int j = 0; j < 90; j++)
            {
                e.Graphics.DrawLine(myPen2, W * j, H * 5, W * j, 0);
                e.Graphics.DrawString(j.ToString(), drawFont, br, W * j, H * 0.87F);
            }
        }

        private void pictureBox4_Paint(object sender, PaintEventArgs e)
        {
            Pen myPen = new Pen(Color.Gray, (float)1);  //оси Х
            Pen myPen2 = new Pen(Color.Gray);   //оси У
            Font drawFont = new Font("Arial", 6);   //размер подписи
            SolidBrush br = new SolidBrush(Color.Black);    //цвет шрифта
            float H = e.ClipRectangle.Height;
            float W = (float)e.ClipRectangle.Width / 90;

            //рисование осей для задач с заданными шагами StepX и StepY
            e.Graphics.DrawLine(myPen, W * 90, H * 0.85F, 0, H * 0.85F);
            for (int j = 0; j < 90; j++)
            {
                e.Graphics.DrawLine(myPen2, W * j, H * 5, W * j, 0);
                e.Graphics.DrawString(j.ToString(), drawFont, br, W * j, H * 0.87F);
            }
        }

        //график
        private void FunctionBox_Paint(object sender, PaintEventArgs e)
        {
            Pen myPen = new Pen(Color.Black, (float)1);//оси Х
            Pen myPen2 = new Pen(Color.Gray);//оси У
            Pen myPen3 = new Pen(Color.Blue);//график
            Font drawFont = new Font("Arial", 6);//размер подписи
            SolidBrush br = new SolidBrush(Color.Black);//цвет шрифта
            float H = e.ClipRectangle.Height;
            float W = (float)e.ClipRectangle.Width / 90;
            //рисуем оси для задач с заданными шагами StepX и StepY
            e.Graphics.DrawLine(myPen, W * 90, H * 0.87F, 0, H * 0.87F);
            for (int j = 0; j < 90; j++)
            {
                e.Graphics.DrawLine(myPen2, W * j, H, W * j, 0);
                e.Graphics.DrawString(j.ToString(), drawFont, br, W * j, H * 0.87F);
            }

            List<PointF> list = new List<PointF>();

            float step;
            float xF;
            float yF;

            // График
            for (step = 0; step <= 6 * Convert.ToSingle(Math.PI); step += Convert.ToSingle(Math.PI / 15))
            {
                xF = (step * 55.7F);
                yF = Convert.ToSingle(Math.Sin(2.0 * step - 2));
                yF *= 30.0F;
                yF = H * 0.45F - yF;

                list.Add(new PointF(xF, yF));
            }
            PointF[] points = list.ToArray();
            e.Graphics.DrawCurve(myPen3, points);
        }

        #endregion //region Drow

        #region ReadParameters

        //Чтение парметров в экземпляр класса Parameters
        bool ReadParameters(Parameters parameters)
        {
            // Функция проверки валидации введенных данных
            try
            {
                int count = 0;
                foreach (TextBox c in TnAll.Controls)
                {
                    parameters.Tn[count] = int.Parse(c.Text);
                    count++;
                }
                count = 0;
                foreach (TextBox c in TpAll.Controls)
                {
                    if (c.ReadOnly != true) { parameters.Tp[count] = int.Parse(c.Text); count++; }
                }
                count = 0;
                foreach (TextBox c in TcAll.Controls)
                {
                    parameters.Tc[count] = int.Parse(c.Text);
                    count++;
                }
                count = 0;
                foreach (TextBox c in TkrAll.Controls)
                {
                    parameters.Tkr[count] = int.Parse(c.Text);
                    count++;
                }
                count = 0;
                foreach (TextBox c in TzAll.Controls)
                {
                    if (c.ReadOnly != true) { parameters.Tz[count] = int.Parse(c.Text); count++; }
                }
                count = 0;
                foreach (TextBox c in PrAll.Controls)
                {
                    if (c.ReadOnly != true) { parameters.Priority[count] = int.Parse(c.Text); count++; }
                }

                parameter.CopyIn(parameters);
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Incorrect data entered:\n" + ex.Message);
                return false;
            }
        }

        #endregion //regionReadParameters

        #region CheckParameters

        //проверка на ошибки
        bool error_param_mesenger()
        {
            try
            {
                //Tн должно быть < времени моделирования (90) и > 0
                if (System.Convert.ToInt32(Tn1.Text) >= 90)
                {
                    MessageBox.Show("Timing task 1: parameter Тn cannot be equal to or greater than the simulation time");
                    return true;
                }
                if (System.Convert.ToInt32(Tn2.Text) >= 90)
                {
                    MessageBox.Show("Timing task 2: parameter Тn cannot be equal to or greater than the simulation time");
                    return true;
                }
                if (System.Convert.ToInt32(Tn3.Text) >= 90)
                {
                    MessageBox.Show("Timing task 3: parameter Тn cannot be equal to or greater than the simulation time");
                    return true;
                }
                if (System.Convert.ToInt32(Tn4.Text) >= 90)
                {
                    MessageBox.Show("Interrupt task 4: parameter Тn cannot be equal to or greater than the simulation time");
                    return true;
                }


                if (System.Convert.ToInt32(Tn1.Text) <= 0)
                {
                    MessageBox.Show("Timing task 1: parameter Тn must be greater than 0");
                    return true;
                }
                if (System.Convert.ToInt32(Tn2.Text) <= 0)
                {
                    MessageBox.Show("Timing task 2: parameter Тn must be greater than 0");
                    return true;
                }
                if (System.Convert.ToInt32(Tn3.Text) <= 0)
                {
                    MessageBox.Show("Timing task 3: parameter Тn must be greater than 0");
                    return true;
                }
                if (System.Convert.ToInt32(Tn4.Text) <= 0)
                {
                    MessageBox.Show("Interrupt task 4: parameter Тn must be greater than 0");
                    return true;
                }
                //------------------
                //Tп должно быть в диапазоне от 1 до 90
                if (System.Convert.ToInt32(Tp1.Text) < 1 || System.Convert.ToInt32(Tp1.Text) > 90)
                {
                    MessageBox.Show("Timing task 1: parameter Тp should be in the range from 1 to 90");
                    return true;
                }
                if (System.Convert.ToInt32(Tp2.Text) < 1 || System.Convert.ToInt32(Tp2.Text) > 90)
                {
                    MessageBox.Show("Timing task 2: parameter Тp should be in the range from 1 to 90");
                    return true;
                }
                if (System.Convert.ToInt32(Tp3.Text) < 1 || System.Convert.ToInt32(Tp3.Text) > 90)
                {
                    MessageBox.Show("Timing task 3: parameter Тp should be in the range from 1 to 90");
                    return true;
                }
                //------------------
                //Tс должно быть меньше Tkr
                if (System.Convert.ToInt32(Tc1.Text) > System.Convert.ToInt32(Tkr1.Text))
                {
                    MessageBox.Show("Timing task 1: parameter Тс cannot be more than parameter Тkr");
                    return true;
                }
                if (System.Convert.ToInt32(Tc2.Text) > System.Convert.ToInt32(Tkr2.Text))
                {
                    MessageBox.Show("Timing task 2: parameter Тс cannot be more than parameter Тkr");
                    return true;
                }
                if (System.Convert.ToInt32(Tc3.Text) > System.Convert.ToInt32(Tkr3.Text))
                {
                    MessageBox.Show("Timing task 3: parameter Тс cannot be more than parameter Тkr");
                    return true;
                }
                if (System.Convert.ToInt32(Tc4.Text) > System.Convert.ToInt32(Tkr4.Text))
                {
                    MessageBox.Show("Interrupt task 4: parameter Тс cannot be more than parameter Тkr");
                    return true;
                }
                //------------------
                //Tс должно быть больше 0
                if (System.Convert.ToInt32(Tc1.Text) <= 0)
                {
                    MessageBox.Show("Timing task 1: parameter Тc must be greater than 0");
                    return true;
                }
                if (System.Convert.ToInt32(Tc2.Text) <= 0)
                {
                    MessageBox.Show("Timing task 2: parameter Тc must be greater than 0");
                    return true;
                }
                if (System.Convert.ToInt32(Tc3.Text) <= 0)
                {
                    MessageBox.Show("Timing task 3: parameter Тc must be greater than 0");
                    return true;
                }
                if (System.Convert.ToInt32(Tc4.Text) <= 0)
                {
                    MessageBox.Show("Interrupt task 4: parameter Тc must be greater than 0");
                    return true;
                }
                //------------------
                //Tз должно быть в диапазоне от 1 до 90
                if (System.Convert.ToInt32(Tz1.Text) < 1 || System.Convert.ToInt32(Tz1.Text) > 90)
                {
                    MessageBox.Show("Timing task 1: parameter Тz should be in the range from 1 to 90");
                    return true;
                }
                if (System.Convert.ToInt32(Tz2.Text) < 1 || System.Convert.ToInt32(Tz2.Text) > 90)
                {
                    MessageBox.Show("Timing task 2: parameter Тz should be in the range from 1 to 90");
                    return true;
                }
                if (System.Convert.ToInt32(Tz3.Text) < 1 || System.Convert.ToInt32(Tz3.Text) > 90)
                {
                    MessageBox.Show("Timing task 3: parameter Тz should be in the range from 1 to 90");
                    return true;
                }
                //------------------
                //Tz+Tkr должно быть меньше, чем Tp
                if ((System.Convert.ToInt32(Tz1.Text) + System.Convert.ToInt32(Tkr1.Text)) >= System.Convert.ToInt32(Tp1.Text))
                {
                    MessageBox.Show("Timing task 1: parameter Тp cannot be less than or equal to Тkr + Тz");
                    return true;
                }

                if ((System.Convert.ToInt32(Tz2.Text) + System.Convert.ToInt32(Tkr2.Text)) >= System.Convert.ToInt32(Tp2.Text))
                {
                    MessageBox.Show("Timing task 2: parameter Тp cannot be less than or equal to Тkr + Тz");
                    return true;
                }
                if ((System.Convert.ToInt32(Tz3.Text) + System.Convert.ToInt32(Tkr3.Text)) >= System.Convert.ToInt32(Tp3.Text))
                {
                    MessageBox.Show("Timing task 3: parameter Тp cannot be less than or equal to Тkr + Тz");
                    return true;
                }
                //------------------
                //Tkr должно быть меньше Tп
                if (System.Convert.ToInt32(Tkr1.Text) >= System.Convert.ToInt32(Tp1.Text))
                {
                    MessageBox.Show("Timing task 1: parameter Tkr cannot be equal to or greater than parameter Tp");
                    return true;
                }
                if (System.Convert.ToInt32(Tkr2.Text) >= System.Convert.ToInt32(Tp2.Text))
                {
                    MessageBox.Show("Timing task 2: parameter Tkr cannot be equal to or greater than parameter Tp");
                    return true;
                }
                if (System.Convert.ToInt32(Tkr3.Text) >= System.Convert.ToInt32(Tp3.Text))
                {
                    MessageBox.Show("Timing task 3: parameter Tkr cannot be equal to or greater than parameter Tp");
                    return true;
                }
                //------------------
                //Tкр должно быть в диапазоне от 1 до 90
                if (System.Convert.ToInt32(Tkr1.Text) < 1 || System.Convert.ToInt32(Tkr1.Text) > 90)
                {
                    MessageBox.Show("Timing task 1: parameter Tkr must be in the range from 1 to 90");
                    return true;
                }
                if (System.Convert.ToInt32(Tkr2.Text) < 1 || System.Convert.ToInt32(Tkr2.Text) > 90)
                {
                    MessageBox.Show("Timing task 2: parameter Tkr must be in the range from 1 to 90");
                    return true;
                }
                if (System.Convert.ToInt32(Tkr3.Text) < 1 || System.Convert.ToInt32(Tkr3.Text) > 90)
                {
                    MessageBox.Show("Timing task 3: parameter Tkr must be in the range from 1 to 90");
                    return true;
                }
                //
                if (System.Convert.ToInt32(Tkr4.Text) < 1 || System.Convert.ToInt32(Tkr4.Text) > 90)
                {
                    MessageBox.Show("Interrupt task 4: parameter Tkr must be in the range from 1 to 90");
                    return true;
                }
                //------------------
                //Tс должно быть меньше Tп
                if (System.Convert.ToInt32(Tc1.Text) > System.Convert.ToInt32(Tp1.Text))
                {
                    MessageBox.Show("Timing task 1: parameter Тp cannot be less than parameter Тс");
                    return true;
                }
                if (System.Convert.ToInt32(Tc2.Text) > System.Convert.ToInt32(Tp2.Text))
                {
                    MessageBox.Show("Timing task 2: parameter Тp cannot be less than parameter Тс");
                    return true;
                }
                if (System.Convert.ToInt32(Tc3.Text) > System.Convert.ToInt32(Tp3.Text))
                {
                    MessageBox.Show("Timing task 3: parameter Тp cannot be less than parameter Тс");
                    return true;
                }
                //------------------
                //Приоритеты не должны совпадать
                if (System.Convert.ToInt32(Pr1.Text) == System.Convert.ToInt32(Pr2.Text)
                    || System.Convert.ToInt32(Pr1.Text) == System.Convert.ToInt32(Pr3.Text)
                    || System.Convert.ToInt32(Pr2.Text) == System.Convert.ToInt32(Pr3.Text))
                {
                    MessageBox.Show("The values of the task priorities (parameter Pr) must not match!");
                    return true;
                }
                //------------------
                //Значения приоритетов должны быть в диапазоне от 0 до 3
                if (System.Convert.ToInt32(Pr1.Text) < 1 || System.Convert.ToInt32(Pr1.Text) > 3)
                {
                    MessageBox.Show("Timing task 1: parameter Pr must be in the range from 1 to 3");
                    return true;
                }
                if (System.Convert.ToInt32(Pr2.Text) < 1 || System.Convert.ToInt32(Pr2.Text) > 3)
                {
                    MessageBox.Show("Timing task 2: parameter Pr must be in the range from 1 to 3");
                    return true;
                }
                if (System.Convert.ToInt32(Pr3.Text) < 1 || System.Convert.ToInt32(Pr3.Text) > 3)
                {
                    MessageBox.Show("Timing task 3: parameter Pr must be in the range from 1 to 3");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Incorrect data entered:\n" + ex.Message);
            }

            return false;
        }

        bool error_param_mesenger_after_edit()
        {

            try
            {
                if ((System.Convert.ToInt32(Tn1.Text) < ModTime) && (System.Convert.ToInt32(Tn1.Text) != parameter.Tn[0]))
                {
                    MessageBox.Show("Timing task 1: the value of the Тn parameter cannot be assigned less than the simulation time");
                    return true;
                }

                if ((System.Convert.ToInt32(Tn2.Text) < ModTime) && (System.Convert.ToInt32(Tn2.Text) != parameter.Tn[1]))
                {
                    MessageBox.Show("Timing task 2: the value of the Тn parameter cannot be assigned less than the simulation time");
                    return true;
                }

                if ((System.Convert.ToInt32(Tn3.Text) < ModTime) && (System.Convert.ToInt32(Tn3.Text) != parameter.Tn[2]))
                {
                    MessageBox.Show("Timing task 3: the value of the Тn parameter cannot be assigned less than the simulation time");
                    return true;
                }

                if ((System.Convert.ToInt32(Tn4.Text) < ModTime) && (System.Convert.ToInt32(Tn4.Text) != parameter.Tn[3]))
                {
                    MessageBox.Show("Interrupt task 4: the value of the Тn parameter cannot be assigned less than the simulation time");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Incorrect data entered " + ex.Message);
            }

            return false;
        }

        #endregion //regionCheckParameters

        #region Functions

        private void bwtask1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            float H = pictureBox1.Height;
            float W = (float)pictureBox1.Width / 90;
            Graphics g = pictureBox1.CreateGraphics();
            Pen myPen = new Pen(Color.LightGreen, W);

            CurrSessionTime[0] = 0;
            while (CurrSessionTime[0] < parameter.Tc[0])
            {
                //если произошел вызов метода закрытия потока => свойство CancellationPending == true
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                if (StartTime1 < ModTime)
                {
                    g.DrawLine(myPen, (StartTime1) * W + W / 2, H * 0.82F, (StartTime1) * W + W / 2, H * 0.82F - H / 7);

                    StartTime1++;
                    CurrSessionTime[0]++;
                }

            }
            FlagSystemIsBusy = false;
        }

        private void bwtask2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            float H = pictureBox2.Height;
            float W = (float)pictureBox2.Width / 90;
            Graphics g = pictureBox2.CreateGraphics();
            Pen myPen = new Pen(Color.LightGreen, W);

            CurrSessionTime[1] = 0;

            while (CurrSessionTime[1] < parameter.Tc[1])
            {

                //если произошел вызов метода закрытия потока => свойство CancellationPending == true
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                if (StartTime2 < ModTime)
                {
                    g.DrawLine(myPen, (StartTime2) * W + W / 2, H * 0.82F, (StartTime2) * W + W / 2, H * 0.82F - H / 7);

                    //строим взаимодействие с объектом
                    //график
                    Graphics gGraph;
                    gGraph = FunctionBox.CreateGraphics();
                    Pen myPen3 = new Pen(Color.LightGreen, 3);

                    float step;
                    float xF, xF1;
                    float yF, yF1;
                    float H1 = FunctionBox.Height;

                    // График

                    step = Convert.ToSingle(ModTime - 1) * Convert.ToSingle(Math.PI / 15);
                    xF = (step * 55.7F);
                    yF = Convert.ToSingle(Math.Sin(2.0 * step - 2));
                    yF *= 30.0F;
                    yF = H1 * 0.45F - yF;
                    gGraph.DrawLine(myPen3, new PointF(xK, yK), new PointF(xF, yK));
                    gGraph.DrawLine(myPen3, new PointF(xF, yK), new PointF(xF, yF));
                    step += Convert.ToSingle(Math.PI / 15);
                    xF1 = (step * 55.7F);
                    yF1 = Convert.ToSingle(Math.Sin(2.0 * step - 2));
                    yF1 *= 30.0F;
                    yF1 = H1 * 0.45F - yF1;
                    xK = xF1;
                    yK = yF1;

                    gGraph.DrawLine(myPen3, new PointF(xF, yF), new PointF(xF1, yF));
                    gGraph.DrawLine(myPen3, new PointF(xF1, yF), new PointF(xF1, yF1));
                    /////
                    StartTime2++;
                    CurrSessionTime[1]++;

                }
            }
            FlagSystemIsBusy = false;
        }

        private void bwtask3_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            float H = pictureBox3.Height;
            float W = (float)pictureBox3.Width / 90;
            Graphics g = pictureBox3.CreateGraphics();
            Pen myPen = new Pen(Color.LightGreen, W);

            CurrSessionTime[2] = 0;

            while (CurrSessionTime[2] < parameter.Tc[2])
            {
                //если произошел вызов метода закрытия потока => свойство CancellationPending == true
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                if (StartTime3 < ModTime)
                {
                    g.DrawLine(myPen, (StartTime3) * W + W / 2, H * 0.82F, (StartTime3) * W + W / 2, H * 0.82F - H / 7);

                    StartTime3++;
                    CurrSessionTime[2]++;
                }
            }
            parameter.Tc[2]++;
            FlagSystemIsBusy = false;
        }

        private void bwtask4_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            float H = pictureBox4.Height;
            float W = (float)pictureBox4.Width / 90;
            Graphics g = pictureBox4.CreateGraphics();
            Pen myPen = new Pen(Color.LightGreen, W);

            CurrSessionTime[3] = 0;
            while (CurrSessionTime[3] < parameter.Tc[3])
            {
                //если произошел вызов метода закрытия потока => свойство CancellationPending == true
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                if (StartTime4 < ModTime)
                {
                    g.DrawLine(myPen, (StartTime4) * W + W / 2, H * 0.82F, (StartTime4) * W + W / 2, H * 0.82F - H / 7);

                    StartTime4++;
                    CurrSessionTime[3]++;
                }
            }
            FlagSystemIsBusy = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Mod && ModTime <= 90)
            {
                switch (e.KeyCode)
                {
                    case Keys.H:
                        if (State[0] != (int)Status.INACTIVE)
                        {
                            parameter.Tk[0] = ModTime;
                            bwtask1.CancelAsync();
                            WritePicture(ModTime, "Stop.png", pictureBox1, 0.45F);
                        }
                        break;
                    case Keys.J:
                        if (State[1] != (int)Status.INACTIVE)
                        {
                            parameter.Tk[1] = ModTime;
                            bwtask2.CancelAsync();
                            WritePicture(ModTime, "Stop.png", pictureBox2, 0.45F);
                        }
                        break;
                    case Keys.K:
                        if (State[2] != (int)Status.INACTIVE)
                        {
                            parameter.Tk[2] = ModTime;
                            bwtask3.CancelAsync();
                            WritePicture(ModTime, "Stop.png", pictureBox3, 0.45F);
                        }
                        break;
                    case Keys.L:
                        if (State[3] != (int)Status.INACTIVE)
                        {
                            parameter.Tk[3] = ModTime;
                            bwtask4.CancelAsync();
                            WritePicture(ModTime, "Stop.png", pictureBox4, 0.45F);
                        }
                        break;
                    case Keys.F:
                        if (State[3] == (int)Status.WAIT_TPLAN)
                        {
                            State[3] = (int)Status.WAIT_WORKING;//поставить задачу по прерыванию на выполнение
                            WritePicture(ModTime, "vypolnenie.png", pictureBox4, 0.45F);
                        }
                        else if (State[3] == (int)Status.WORKING)
                        {
                            SetTplan(3); //установить новое Тплан (на основании клика клавиши F)
                            kolF++; //количество вызовов прерывания
                        }
                        break;
                    default: break;
                }
            }
        }

        //символы состояния
        void WritePicture(int time, string pictureName, PictureBox pb, float f)
        {
            try
            {
                Image img = Image.FromFile(pictureName);
                Graphics gfx = pb.CreateGraphics();
                float H = pb.Height;
                float W = (float)pb.Width / 90;
                gfx.DrawImage(img, time * W - 3, H * f);
            }
            catch (Exception)
            { MessageBox.Show("Error accessing image"); }

        }

        //установка Тплан (отталкиваясь от Тфакт)
        void SetTplan(int taskNumber)
        {
            PictureBox pb = new PictureBox();

            switch (taskNumber)
            {
                case 0: pb = pictureBox1; p = parameter.Tp[0]; break;
                case 1: pb = pictureBox2; p = parameter.Tp[1]; break;
                case 2: pb = pictureBox3; p = parameter.Tp[2]; break;
                case 3: pb = pictureBox4; p = parameter.Tc[3] - CurrSessionTime[3]; break;
                default: break;
            }
            Tplan[taskNumber] = ModTime + p; //Тфакт
            WritePicture(Tplan[taskNumber], "vypolnenie.png", pb, 0.45F);
        }

        //отрисовка символов (для формы, в первую очередь)
        void SetWritePicture(int taskNumber, int time, String pictureName, float f)
        {
            PictureBox pb = new PictureBox();
            switch (taskNumber)
            {
                case 0: pb = pictureBox1; break;
                case 1: pb = pictureBox2; break;
                case 2: pb = pictureBox3; break;
                case 3: pb = pictureBox4; break;
                default: break;
            }

            try
            {
                Image img = Image.FromFile(pictureName);
                Graphics gfx = pb.CreateGraphics();
                float H = pb.Height;
                float W = (float)pb.Width / 90;
                gfx.DrawImage(img, time * W - 3, H * f);
            }
            catch (Exception)
            { MessageBox.Show("Error accessing image"); }
        }

        //отрисовка линий
        void DrawStatus(int i)
        {
            PictureBox pb = new PictureBox();
            switch (i)
            {
                case 0: pb = pictureBox1; break;
                case 1: pb = pictureBox2; break;
                case 2: pb = pictureBox3; break;
                case 3: pb = pictureBox4; break;
                default: break;
            }

            float H = pb.Height;
            float W = (float)pb.Width / 90;
            Graphics g = pb.CreateGraphics();
            Pen myPen = new Pen(Color.LightGreen, W);
            ///
            switch (State[i])
            {
                case (int)Status.INACTIVE:  
                    myPen = new Pen(Color.LightSkyBlue, W);
                    break;
                ///
                case (int)Status.WAIT_TPLAN:    
                    myPen = new Pen(Color.Gold, W);
                    break;
                case (int)Status.WAIT_WORKING:  
                    myPen = new Pen(Color.Gold, W);
                    break;
                ///
                case (int)Status.SUSPENDED:
                    myPen = new Pen(Color.OrangeRed, W);
                    break;
                default: break;
            }
            if (State[i] != (int)Status.WORKING)
                g.DrawLine(myPen, (ModTime) * W + W / 2, H * 0.82F, (ModTime) * W + W / 2, H * 0.82F - H / 7);
        }

        #endregion  // regionFunctions


    }
}

