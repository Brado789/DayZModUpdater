using System;
using System.Windows.Forms;

namespace WorkshopModViewer
{
    public partial class ChangeNotesDialog : Form
    {
        public string ChangeNotes => txtChangeNotes.Text;

        public ChangeNotesDialog(string title)
        {
            InitializeComponent();
            lblPrompt.Text = $"Please confirm your {title.ToLower()} and enter change notes:";
            this.Text = $"{title} Confirmation";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtChangeNotes.Text))
            {
                MessageBox.Show("Change notes cannot be empty.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
