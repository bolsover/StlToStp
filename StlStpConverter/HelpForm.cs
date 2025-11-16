using System;
using System.IO;
using System.Windows.Forms;

namespace Bolsover
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            LoadInformation();
            infoTextBox.Select(0, 0);
        }

        private void LoadInformation()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var infoPath = Path.Combine(baseDir, "information.me");
                if (File.Exists(infoPath))
                {
                    infoTextBox.Text = File.ReadAllText(infoPath);
                }
                else
                {
                    infoTextBox.Text = "Information file 'information.me' not found in application directory.";
                }
            }
            catch (Exception ex)
            {
                infoTextBox.Text = $"Error loading information: {ex.Message}";
            }

           
        }
    }
}