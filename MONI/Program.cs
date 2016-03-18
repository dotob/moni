﻿using System;
using System.Reflection;

namespace MONI {
    public class Program {
        [STAThread]
        public static void Main() {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                var resourceName = "MONI.DllsAsResource." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                    if (stream != null) {
                        var assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            };
            App.Main();
        }
    }
}