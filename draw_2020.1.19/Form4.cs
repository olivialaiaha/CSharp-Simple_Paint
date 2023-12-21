using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace draw_2020_1_19 {
    public partial class Form4 : Form {

        private bool button = false;
        public Form4() {
            InitializeComponent();
        }

        private void Button_yes_Click(object sender, EventArgs e) {
            button = true;
            this.Close();
        }

        private void Button_no_Click(object sender, EventArgs e) {
            this.Close();
        }

        public bool getButton() {
            return button;
        }
    }
}
