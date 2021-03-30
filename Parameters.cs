using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laba3
{
    public class Parameters
    {
        public int[] Tn { get; set; }
        public int[] Tk { get; set; }
        public int[] Tp { get; set; }
        public int[] Tc { get; set; }
        public int[] Tkr { get; set; }
        public int[] Tz { get; set; }
        public int[] Priority { get; set; }

         public Parameters()
        {
            Tn = new int[4];
            Tk = new int[4];
            Tp = new int[3];
            Tc = new int[4];
            Tkr = new int[4];
            Tz = new int[3];
            Priority = new int[3];
        }

        public void CopyIn(Parameters cl)
         {
             Tn = cl.Tn;
             Tk = cl.Tk;
             for (int i = 0; i < 3; i++)
             {
                 Tp[i] = cl.Tp[i];
                 Tz[i] = cl.Tz[i];
                 Priority[i] = cl.Priority[i];
             }
             for (int i = 0; i < 4; i++)
             {
                 Tc[i] = cl.Tc[i];
                 Tkr[i] = cl.Tkr[i];
             }
         }

         public void SetUpInBegin(Parameters cl)
         {
             for (int i = 0; i < 4; i++)
             {
                 Tn[i] = cl.Tn[i];
                 Tk[i] = -1;
             }   
         }
    }
}