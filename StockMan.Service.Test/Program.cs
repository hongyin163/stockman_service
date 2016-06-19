using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;
using StockMan.MySqlAccess;
using Newtonsoft.Json;
using StockMan.Common;
namespace StockMan.Service.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            RelatedDataService service = new RelatedDataService();

            //related_object_define difne = new related_object_define
            //{
            //    code = "R0001",
            //    name = "PMI",
            //    description = "测试",
            //    related_object_fields = new List<related_object_fields>()

            //};
            //difne.related_object_fields.Add(new related_object_fields
            //{
            //    code = "R0001_01",
            //    define_code = "R0001",
            //    name = "制造业",
            //    description = "制造业",
            //    sort = 0
            //});
            //difne.related_object_fields.Add(new related_object_fields
            //{
            //    code = "R0001_02",
            //    define_code = "R0001",
            //    name = "非制造业",
            //    description = "非制造业",
            //    sort = 1
            //});

            //service.Add(difne);

            //var define = service.Find("R0001");

            //Console.WriteLine(define.code);

            //foreach (var d in define.related_object_fields)
            //{
            //    Console.WriteLine(d.name);

            //}

            //StockManDBEntities entity = new StockManDBEntities();

            //var query = from p in entity.related_object_define.Include("related_object_fields")
            //            where p.code == "R0001"
            //            select p;
            //define = query.First();

            //foreach (var d in define.related_object_fields)
            //{
            //    Console.WriteLine(d.name);

            //}

            //string data = "{{date:'{0}',m:{1},n:{2}}}";

            //for (int i = 0; i < 100; i++)
            //{
            //    string str = string.Format(data, DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"), i, i + 2);
            //    Console.WriteLine(str);
            //    service.InsertData("R0001",str );
            //}

            //var datas = service.GetData("R0001");
            //foreach (string[] data in datas)
            //{

            //    Console.WriteLine(string.Join(",", data) + "\r\n");

            //}

            Console.ReadLine();
        }
    }
}
