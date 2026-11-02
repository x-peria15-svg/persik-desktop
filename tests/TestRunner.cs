// SPDX-License-Identifier: Apache-2.0
using System;
using System.Windows.Forms;
namespace Persik
{
    public static class TestRunner
    {
        [STAThread] public static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Tests.Run(args.Length > 0 ? args[0] : null);
                Console.WriteLine("All checks passed. See build/test-results.txt.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
                return 1;
            }
        }
    }
}
