using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide(); // Form1'i gizle
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Gerekirse buraya bir şeyler yazılabilir.
        }
    }
}
