using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace draw_2020_1_19
{
    public partial class Form3 : Form
    {
        private float R = 1, G = 1, B = 1, A = 1;

        public Form3()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            float tem = (float)trackBar1.Value / 10;
            R = tem;
            label5.Text = R.ToString();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            float tem = (float)trackBar4.Value / 10;
            A = tem;
            label8.Text = A.ToString();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            float tem = (float)trackBar3.Value / 10;
            B = tem;
            label7.Text = B.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            float tem = (float)trackBar2.Value / 10;
            G = tem;
            label6.Text = G.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public float getR()
        {
            return R;
        }

        public float getG()
        {
            return G;
        }

        public float getB()
        {
            return B;
        }

        public float getA()
        {
            return A;
        }
    }
}
