﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace IndexForumCrawler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 2)
            {
                Application.Run(new Form1(int.Parse(args[0]), args[1]));
            }
            else
            {
                Application.Run(new Form1(0, ""));
            }
        }
    }
}
