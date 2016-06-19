using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using StockMan.Index;

namespace StockMan.Jobs
{
    public class CodeHelper
    {
        public static IIndex GetIEvaluator(string expression)
        {
            var coding = @"
					using System;
                    using System.Collections.Generic;
                    using System.Linq;
                    using System.Text;          
                    using StockMan.EntityModel;
                    using StockMan.Index;
                    using System.Collections.Specialized;
					{0}".Replace("{0}", expression);

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v3.5" } };
            var cp = new CSharpCodeProvider(providerOptions);
            CompilerParameters cps = new CompilerParameters();
            cps.GenerateExecutable = false;
            cps.GenerateInMemory = true;
            cps.IncludeDebugInformation = false;
            cps.ReferencedAssemblies.Add("system.dll");
            cps.ReferencedAssemblies.Add("system.core.dll");
            cps.ReferencedAssemblies.Add("StockMan.EntityModel.dll");
            cps.ReferencedAssemblies.Add("StockMan.Index.dll");
            //cps.ReferencedAssemblies.Add(@"D:\ERMS1.5\ERMS\Coding-201306\CNPC.ERMS\CNPC.ERMS.Service.ComplianceDetectWeb\bin\CNPC.ERMS.Service.ComplianceDetectEntity.dll");
            var dllPath = string.Empty;
            var dllName = "928d91e7-0dcb-4e69-b1c9-faf7aeb4b576.dll";
            //throw new Exception(System.Environment.CurrentDirectory + "-----" + AppDomain.CurrentDomain.BaseDirectory);

            //DirectoryInfo info = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "bin");


            //dllPath = AppDomain.CurrentDomain.BaseDirectory + "Bin\\CNPC.ERMS.Service.ComplianceDetectEntity.dll";
            dllName = AppDomain.CurrentDomain.BaseDirectory + dllName;
            //cps.ReferencedAssemblies.Add(dllPath);
            cps.OutputAssembly = dllName;

            var a = cp.CompileAssemblyFromSource(cps, coding);

            if (a.Errors.Count > 0)
            {
                string errors = a.Errors.Cast<object>().Aggregate("", (current, er) => current + (er.ToString() + "\r\n"));
                throw new Exception(errors);
            }

            var b = a.CompiledAssembly;

            //class\s+\w+\s+:
            var reg = new Regex(@"class\s+\w+\s+:");
            var match = reg.Match(expression);
            var className = match.Value.Replace("class", "").Replace(":", "").Trim();

            var obj = (IIndex)b.CreateInstance(className);
            //obj.FullName = cps.OutputAssembly;

            return obj;
        }



    }
}