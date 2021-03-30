using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace laba3
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        //моделирование
        private void button1_Click(object sender, EventArgs e)
        {
            Form1.FlagOfMenu = true;
            Close();
        }
        //выход
        private void button2_Click(object sender, EventArgs e)
        {
            Form1.FlagOfMenu = false;
            Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = @"    
                According to the course project's assignment there are 3 timing tasks and 1 interrupt task supported. 

                Tasks have the following parameters:
                •	Тn  – start of task activity interval.
                •	Тk  – end of task activity interval.
                •	Tp  – period of timing task calling.
                •	Тkr – max allowed run time for a task.
                •	Тс  – session time.
                •	Тz  – acceptable delay.
                •	Pr   – task priority.

                Methods for specifying simulation parameters:
                •	Тn of all the tasks is defined by the number.
                •	Tk of all the tasks is set by an event from the keyboard.

                Editing of the task settings is possible during the simulation.
                
                Interrupt handling: all active tasks are terminated, and only then the interrupt is processed.
                
                If the waiting time for the task call has been exceeded relative to the moment that triggers  
                the event, issuing a request to the user interface: 'Continue execution' or 'Cancel the task'.
                
                If the session time of the task has exceeded, the value Тex.max (Тkr) increases by N, where N = 5.
                
                Task №2: in case of a call a value is calculated, that is equivalent to the current value 
                                 of the object function.
                Task №3: changes performed modeling session time duration.
     
                Describing function: f(t)=sin(2t-2).
                Display type: analog.
                Total simulation time 90 секунд. Number of periods for a function: 6.
                ";
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

    }
}

