using System;
using System.Windows.Forms;

namespace WorkshopModViewer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Unhandled Exception");
            }
        }
    }
}
