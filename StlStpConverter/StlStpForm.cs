using System;
using System.Reflection; // For retrieving AssemblyVersion
using System.Windows.Forms;

namespace Bolsover
{
    public partial class StlStpForm : Form
    {
        
        public StlStpForm()
        {
            InitializeComponent();
            // Append the AssemblyVersion to the form title so users can see the app version
            // AssemblyVersion is defined in Properties\AssemblyInfo.cs as [assembly: AssemblyVersion("major.minor.build.revision")]
            // We read it via reflection from the executing assembly.
            var version = GetAssemblyVersionString();
            if (!string.IsNullOrWhiteSpace(version))
            {
                // If designer already set a title, append the version; otherwise set a default title with version
                this.Text = string.IsNullOrWhiteSpace(this.Text)
                    ? $"STL->STEP Converter v{version}"
                    : $"{this.Text} v{version}";
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show About dialog with version pulled from AssemblyVersion
            var version = GetAssemblyVersionString();
            var aboutText = string.IsNullOrWhiteSpace(version)
                ? "STL->STEP Converter"
                : $"STL->STEP Converter\nVersion: {version}";
            aboutText = aboutText + "\nCopyright © 2025, David Bolsover ";
            aboutText = aboutText + "\nInspired by original work";
            aboutText = aboutText + "\nCopyright © 2018, slugdev";
            aboutText = aboutText + "\nOriginal and modifications are licensed under the BSD-4-Clause License";
            MessageBox.Show(aboutText, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Retrieves the AssemblyVersion string from this application's assembly.
        /// This reads the value specified by [assembly: AssemblyVersion("...")] in Properties\AssemblyInfo.cs.
        /// </summary>
        private static string GetAssemblyVersionString()
        {
            try
            {
                // Option A (direct AssemblyVersion): Assembly.GetExecutingAssembly().GetName().Version
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                return v?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private void usageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm helpForm = new();
            helpForm.ShowDialog();
        }
    }
}