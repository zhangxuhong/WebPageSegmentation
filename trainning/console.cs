using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace bid
{
    public class ConsoleWindow2
    {

        public static IntPtr CreateConsole()
        {

            var console = new ConsoleWindow2();

            return console.Hwnd;

        }



        public IntPtr Hwnd { get; private set; }



        public ConsoleWindow2()
        {

            Initialize();

        }



        public void Initialize()
        {

            Hwnd = GetConsoleWindow();



            // Console app

            if (Hwnd != IntPtr.Zero)
            {

                return;

            }



            // Windows app

            AllocConsole();

            Hwnd = GetConsoleWindow();

        }



        #region Win32



        [DllImport("kernel32")]

        static extern IntPtr GetConsoleWindow();



        [System.Runtime.InteropServices.DllImport("kernel32")]

        static extern bool AllocConsole();





        #endregion

    }

}

