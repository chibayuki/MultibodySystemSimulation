using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Multibody
{
    static class Program
    {
        static Program()
        {
            Dictionary<string, Assembly> dlls = new Dictionary<string, Assembly>();

            Assembly ass = new StackTrace(0).GetFrame(1).GetMethod().Module.Assembly;

            foreach (string name in ass.GetManifestResourceNames())
            {
                if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Stream s = ass.GetManifestResourceStream(name);
                        byte[] bytes = new byte[s.Length];
                        s.Read(bytes, 0, (int)s.Length);
                        Assembly dll = Assembly.Load(bytes);

                        if (!dlls.ContainsKey(dll.FullName))
                        {
                            dlls.Add(dll.FullName, dll);
                        }
                    }
                    catch { }
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                string name = new AssemblyName(e.Name).FullName;

                if (dlls.TryGetValue(name, out Assembly dll) && dll != null)
                {
                    dlls[name] = null;

                    return dll;
                }
                else
                {
                    throw new DllNotFoundException(name);
                }
            };
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
