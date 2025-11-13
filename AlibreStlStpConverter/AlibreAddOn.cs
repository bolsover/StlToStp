using System;
using System.Windows.Forms;
using AlibreAddOn;
using AlibreX;
using Bolsover.AlibreStlStpConverter;

namespace AlibreAddOnAssembly
{
    public static class AlibreAddOn
    {
        private static IADRoot alibreRoot { get; set; }
        private static IntPtr parentWinHandle;
        private static Bolsover.AlibreStlStpConverter.AlibreStlStpConverter _alibreStlStpConverter;


        public static void AddOnLoad(IntPtr hwnd, IAutomationHook pAutomationHook, IntPtr unused)
        {
            alibreRoot = (IADRoot)pAutomationHook.Root;
            parentWinHandle = hwnd;
            string version = alibreRoot.Version.Replace("PRODUCTVERSION ", "");
            string[] versionarr = version.Split(',');
            int majorVersion = int.Parse(versionarr[0]);
            if (majorVersion < 25)
                MessageBox.Show(Globals.AppName + "requires a newer version of Alibre Design", "Error");

            _alibreStlStpConverter =
                new Bolsover.AlibreStlStpConverter.AlibreStlStpConverter(alibreRoot, parentWinHandle);
        }

        public static IADRoot GetRoot()
        {
            return alibreRoot;
        }

        public static void AddOnInvoke(
            IntPtr hwnd,
            IntPtr pAutomationHook,
            string sessionName,
            bool isLicensed,
            int reserved1,
            int reserved2)
        {
        }


        public static void AddOnUnload(
            IntPtr hwnd,
            bool forceUnload,
            ref bool cancel,
            int reserved1,
            int reserved2)
        {
        }


        public static IAlibreAddOn GetAddOnInterface()
        {
            return (IAlibreAddOn)_alibreStlStpConverter;
        }
    }
}