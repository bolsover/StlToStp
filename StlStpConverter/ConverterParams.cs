using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bolsover
{
    public class ConverterParams
    {
        private string _inFile;
        private string _outFile;
        private double _tol;
        private bool _openConverted;
        private string _message;

        public string InFile
        {
            get => _inFile;
            set => SetField(ref _inFile, value);
        }

        public string OutFile
        {
            get => _outFile;
            set => SetField(ref _outFile, value);
        }

        public double Tol
        {
            get => _tol;
            set => SetField(ref _tol, value);
        }

        public bool OpenConverted
        {
            get => _openConverted;
            set => SetField(ref _openConverted, value);
        }

        public string Message
        {
            get => _message;
            set => SetField(ref _message, value);
        }

        public ConverterParams()
        {
            InFile = "";
            OutFile = "";
            Tol = 0.0000001;
            OpenConverted = false;
        }

        public event ParameterChangedEventHandler ParameterChanged;

        private void OnParameterChanged([CallerMemberName] string propertyName = null, object value = null)
        {
            ParameterChanged?.Invoke(this, new ParameterChangeEventArgs(propertyName, value));
        }


        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnParameterChanged(propertyName, value);
            return true;
        }
    }

    public delegate void ParameterChangedEventHandler(object sender, ParameterChangeEventArgs args);
}