using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task
{
    public class Loader
    {
        private static Loader instance = null;
        public static Loader Instance
        {
            get
            {
                if (instance == null)
                    instance = new Loader();
                return instance;

            }
        }

        public Dictionary<string, AppDomain> appDomainDic = new Dictionary<string, AppDomain>();
        public AppDomain CreateAppDomain(string name, string path)
        {
            if (appDomainDic.ContainsKey(name))
            {
                AppDomain.Unload(appDomainDic[name]);
                appDomainDic.Remove(name);
                return appDomainDic[name];
            }

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = path;
            setup.ApplicationName = name;
            setup.ConfigurationFile = Path.Combine(path, "App.config");
            var appDomain = AppDomain.CreateDomain(name, null, setup);
            appDomainDic.Add(name, appDomain);

            return appDomain;
        }

        public void UnLoad(string taskCode)
        {
            if (appDomainDic[taskCode] != null)
            {
                AppDomain.Unload(appDomainDic[taskCode]);
                appDomainDic.Remove(taskCode);
            }
        }

        public RemoteLoader GetRemoteLoader(string assemblyName, string taskCode)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks", assemblyName);
            AppDomain appDomain = this.CreateAppDomain(taskCode, path);
            return (RemoteLoader)appDomain.CreateInstanceAndUnwrap("StockMan.Message.Task", "StockMan.Message.Task.RemoteLoader");
        }

        public void CreateTaskAssembly(string assemblyName, Dictionary<string, byte[]> files)
        {
            this.Log().Info("接收任务程序集");
            var path = AppDomain.CurrentDomain.BaseDirectory + "tasks";
            var binPath = Path.Combine(path, assemblyName);

            if (!Directory.Exists(binPath))
            {
                Directory.CreateDirectory(binPath);
            }
            foreach (var name in files.Keys)
            {
                if (files[name] == null)
                    continue;
                var dir = Path.GetDirectoryName(name);
                dir = Path.Combine(binPath, dir);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                using (FileStream fs = new FileStream(Path.Combine(binPath, name), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 1024))
                {
                    fs.Write(files[name], 0, files[name].Length);
                    fs.Close();
                }
                this.Log().Info("保存" + name);
            }
        }
    }

    public class RemoteLoader : MarshalByRefObject
    {
        public void LoadAssemblys()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                AppDomain.CurrentDomain.Load(File.ReadAllBytes(file));
            }
        }

        public T GetObject<T>(string assemblyName, string typeName) where T : MarshalByRefObject
        {
            return (T)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        public TaskExcuter GetTaskExcuteer()
        {
            return (TaskExcuter)AppDomain.CurrentDomain.CreateInstanceAndUnwrap("StockMan.Message.Task", "StockMan.Message.Task.TaskExcuter");
        }
        public TaskSender GetTaskSender()
        {
            return (TaskSender)AppDomain.CurrentDomain.CreateInstanceAndUnwrap("StockMan.Message.Task", "StockMan.Message.Task.TaskSender");
        }

    }
}
