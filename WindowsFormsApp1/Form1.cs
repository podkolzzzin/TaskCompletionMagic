using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            
            InitializeComponent();
            PerformClick();
        }

        private async void PerformClick()
        {
            var builder = new EventStreamBuilder<KeyEventArgs>();
            var stream1 = builder.Build(
                () => textBox1.KeyUp += builder.Handle, 
                () => textBox1.KeyUp -= builder.Handle,
                CancellationToken.None);
            button1.Enabled = false;
            await stream1.FirstAsync(x => textBox1.Text == "782");
            textBox1.Enabled = false;
            button1.Enabled = true;
            var builder2 = new EventStreamBuilder<EventArgs>();

            var stream2 = builder2.Build(() => button1.Click += builder2.Handle, () => button1.Click -= builder2.Handle, CancellationToken.None);

            await stream2.Take(5).ToArrayAsync();
            textBox1.Text = "Finished";

        }
    }
}
