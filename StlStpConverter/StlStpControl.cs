using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bolsover.Converter;

namespace Bolsover
{
    public partial class StlStpControl : UserControl
    {
        private CancellationTokenSource _cts;

        public StlStpControl()
        {
            InitializeComponent();
            comboBox.SelectedIndex = 7;
            // Wire up event handlers
            _converterParams.ParameterChanged += ConverterParamsOnParameterChanged;
            okButton.Click += OkButton_Click;
            inputButton.Click += InputButton_Click;
            outputButton.Click += OutputButton_Click;
            cancelButton.Click += CancelButton_Click;
            openChkBox.CheckedChanged += OpenChkBox_CheckedChanged;
        }


        private void ConverterParamsOnParameterChanged(object sender, ParameterChangeEventArgs args)
        {
            switch (args.Property)
            {
                case "InFile": this.infile.Text = args.Value.ToString(); break;
                case "OutFile": this.outfile.Text = args.Value.ToString(); break;
                case "Message": this.message.Text = args.Value.ToString(); break;
            }
        }

        private readonly ConverterParams _converterParams = new();

        private async void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                await ExecuteConversionAsync();
            }
            catch (Exception ex)
            {
                _converterParams.Message = ex.Message;
            }
        }

        private void InputButton_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog
                { Filter = @"Standard Tessellation Language|*.stl", Title = @"Select an STL file" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _converterParams.InFile = dlg.FileName;
                if (string.IsNullOrWhiteSpace(_converterParams.OutFile))
                    _converterParams.OutFile = Path.ChangeExtension(dlg.FileName, ".stp");
            }
        }

        private void OutputButton_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = @"STEP file|*.stp;*.step", Title = @"Save STEP file" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _converterParams.OutFile = dlg.FileName;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private async Task ExecuteConversionAsync()
        {
            if (string.IsNullOrWhiteSpace(_converterParams.InFile) ||
                string.IsNullOrWhiteSpace(_converterParams.OutFile))
            {
                _converterParams.Message = @"Please select both STL input and STEP output files.";
                return;
            }

            // UI: disable inputs, enable cancel, show progress
            okButton.Enabled = false;
            inputButton.Enabled = false;
            outputButton.Enabled = false;
            comboBox.Enabled = false;
            openChkBox.Enabled = false;
            cancelButton.Enabled = true;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var progress = new Progress<string>(msg => { _converterParams.Message = msg; });
            _converterParams.Message = @"Converting file... Please wait";

            try
            {
                // Optionally parse tolerance from comboBox if relevant
                var tol = _converterParams.Tol;
                if (comboBox.SelectedItem is string s && double.TryParse(s, out var tolUi))
                {
                    tol = tolUi;
                }

                var result = await StlReader.Convert(_converterParams.InFile, _converterParams.OutFile, tol, token,
                    progress);

                if (!token.IsCancellationRequested)
                {
                    _converterParams.Message =
                        result == 0 ? @"Conversion complete" : @"Conversion finished with issues";

                    if (result == 0 && _converterParams.OpenConverted)
                    {
                        try
                        {
                           
                            Process.Start(_converterParams.OutFile);
                        }
                        catch
                        {
                            /* ignore */
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _converterParams.Message = @"Conversion cancelled";
            }
            catch (Exception ex)
            {
                _converterParams.Message = $"Conversion failed: {ex.Message}";
            }
            finally
            {
                // Reset UI
                progressBar.Visible = false;
                cancelButton.Enabled = false;
                okButton.Enabled = true;
                inputButton.Enabled = true;
                outputButton.Enabled = true;
                comboBox.Enabled = true;
                openChkBox.Enabled = true;
                _converterParams.InFile = "";
                _converterParams.OutFile = "";
                _cts.Dispose();
                _cts = null;
            }
        }

        private void OpenChkBox_CheckedChanged(object sender, EventArgs e)
        {
            _converterParams.OpenConverted = openChkBox.Checked;
        }
    }
}