using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Index;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Jobs.Biz;
using StockMan.Common;
using System.Threading.Tasks;
namespace StockMan.Jobs.Tech
{
    public class TechCalculate
    {
        IIndexService indexService = new IndexService();
        //技术分析统一模型，输入是价格数据，输出是，技术数据，遍历技术定义，根据输入数据，技术对应的 指标数据

        //输入1：对象标识字符串，用来区分存储，比如tech_stock,,tech_category,,_tech_object,,
        //输入2：数据，需要输入的数据，需要规定对象模型，自建
        /// <summary>
        /// 对象类型
        /// </summary>
        public ObjectType ObjectType { get; set; }
        /// <summary>
        /// 周期
        /// </summary>
        public TechCycle Cycle { get; set; }
        /// <summary>
        /// 需要计算的对象的id列表
        /// </summary>
        public IList<PriceInfo> PriceList { get; set; }
        /// <summary>
        /// 对象的主键
        /// </summary>
        public string ObjectCode { get; set; }
        public TechCalculate()
        {

        }

        public TechCalculate(ObjectType objectType, TechCycle cycle, string objectCode, IList<PriceInfo> priceList)
        {
            this.ObjectType = objectType;
            this.Cycle = cycle;
            this.PriceList = priceList;
            this.ObjectCode = objectCode;

        }
        public void Run()
        {
            //if (string.IsNullOrEmpty(ObjectType))
            //{
            //    throw new Exception("ObjectType属性为空");
            //}
            var defineList = index_definition_infos();


            #region OLD

            foreach (IndexDefinitionInfo define in defineList)
            {
                this.Log().Info("开始计算：" + define.name);
                //根据定义创建表结构
                BuidTable(define);

                //计算技术数据
                IList<IndexData> dataResult = null;
                try
                {
                    dataResult = CalculateData(define);
                    this.Log().Info("计算" + define.name + "结束,结果" + dataResult.Count + "条");
                }
                catch (Exception ex)
                {
                    this.Log().Error("计算技术数据异常：对象类型:{0},对象编码:{1},周期:{2},技术:{3},\r\n异常信息:{4}"
    .Format(this.ObjectType, this.ObjectCode, this.Cycle, define.name, ex.GetAllExptionMessage() + ex.StackTrace));

                }
                //存储计算接
                try
                {
                    this.Log().Info("保存技术数据：" + define.name);
                    SaveTechData(define, dataResult);
                }
                catch (Exception ex)
                {
                    this.Log().Error("保存技术数据异常：对象类型:{0},对象编码:{1},周期:{2},技术:{3},\r\n异常信息:{4}"
                        .Format(this.ObjectType, this.ObjectCode, this.Cycle, define.name, ex.GetAllExptionMessage() + ex.StackTrace));

                    //回滚
                    //ClearTechData(define);
                    continue;
                }
                //计算状态数据
                IndexState stateResult = IndexState.Warn;
                try
                {
                    this.Log().Info("计算技术状态：" + define.name);
                    stateResult = CalculateState(define);
                }
                catch (Exception ex)
                {
                    this.Log().Error("计算技术状态异常：对象类型:{0},对象编码:{1},周期:{2},技术:{3},\r\n异常信息:{4}"
    .Format(this.ObjectType, this.ObjectCode, this.Cycle, define.name, ex.GetAllExptionMessage() + ex.StackTrace));

                }
                //存储状态
                try
                {
                    this.Log().Info("保存技术状态：" + define.name);
                    SaveTechState(define, stateResult);
                }
                catch (Exception ex)
                {
                    this.Log().Error("保存状态数据异常：对象类型:{0},对象编码:{1},周期:{2},技术:{3},\r\n异常信息:{4}"
                       .Format(this.ObjectType, this.ObjectCode, this.Cycle, define.name, ex.GetAllExptionMessage() + ex.StackTrace));

                    //回滚
                    //ClearTechData(define);
                    continue;
                }

                //计算Tag

                try
                {
                    this.Log().Info("计算Tag：" + define.name);
                    string tag = CalculateTag(define);
                    this.Log().Info("保存Tag：" + tag);
                    SaveTag(define, tag);
                }
                catch (Exception ex)
                {
                    this.Log().Error("保存标签数据异常：对象类型:{0},对象编码:{1},周期:{2},技术:{3},\r\n异常信息:{4}"
                       .Format(this.ObjectType, this.ObjectCode, this.Cycle, define.name, ex.GetAllExptionMessage() + ex.StackTrace));

                    //回滚
                    //ClearTechData(define);
                    continue;
                }

            }

            #endregion
        }

        public void CalculateState()
        {
            var defineList = index_definition_infos();

            //根据定义生成指数信息
            foreach (IndexDefinitionInfo define in defineList)
            {
                //计算状态数据
                IndexState stateResult = IndexState.Warn;
                try
                {
                    stateResult = CalculateState(define);
                }
                catch (Exception ex)
                {
                    this.Log().Error("计算技术状态异常：对象类型:{0},对象编码:{1},周期:{2},技术:{3},\r\n异常信息:{4}"
    .Format(this.ObjectType, this.ObjectCode, this.Cycle, define.name, ex.GetAllExptionMessage() + ex.StackTrace));

                }
                //存储状态
                try
                {
                    SaveTechState(define, stateResult);
                }
                catch (Exception ex)
                {
                    this.Log().Error("保存状态数据异常：对象类型:{0},对象编码:{1},周期:{2},技术:{3},\r\n异常信息:{4}"
                       .Format(this.ObjectType, this.ObjectCode, this.Cycle, define.name, ex.GetAllExptionMessage() + ex.StackTrace));

                    //回滚
                    //ClearTechData(define);
                    continue;
                }



            }
        }
        private void ClearTechData(IndexDefinitionInfo define)
        {
            throw new NotImplementedException();
        }

        private void SaveTechState(IndexDefinitionInfo define, IndexState result)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var cate = (int)this.ObjectType + "";
                var code = cate + "_" + this.ObjectCode + "_" + define.code;
                var objectState = entity.objectstate.FirstOrDefault(p => p.code == code);
                if (objectState == null)
                {
                    var temp = new objectstate()
                    {
                        code = code,
                        category_code = cate,
                        object_code = this.ObjectCode,
                        index_code = define.code,
                        date = DateTime.Now
                    };

                    switch (this.Cycle)
                    {
                        case TechCycle.day:
                            temp.day = (int)result;
                            break;
                        case TechCycle.week:
                            temp.week = (int)result;
                            break;
                        default:
                            temp.month = (int)result;
                            break;
                    }
                    entity.objectstate.Add(temp);

                }
                else
                {
                    switch (this.Cycle)
                    {
                        case TechCycle.day:
                            objectState.last_day = objectState.day;
                            objectState.day = (int)result;
                            break;
                        case TechCycle.week:
                            objectState.last_week = objectState.week;
                            objectState.week = (int)result;
                            break;
                        default:
                            objectState.last_month = objectState.month;
                            objectState.month = (int)result;
                            break;
                    }
                    objectState.date = DateTime.Now;
                }

                entity.SaveChanges();
            }
        }

        private IndexState CalculateState(IndexDefinitionInfo define)
        {
            var result = IndexCalculate.GetState(define.algorithm_script, GetLastIndexData(define));

            return result;
        }
        private string CalculateTag(IndexDefinitionInfo define)
        {
            var result = IndexCalculate.GetTag(define.algorithm_script, GetLastIndexData(define));

            return result;
        }
        private void SaveTag(IndexDefinitionInfo define, string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                return;
            }
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                //cate_object_code+tag_code+date
                var tag = entity.tag.FirstOrDefault(p => p.name == result);
                if (tag == null)
                    tag = entity.tag.Add(new EntityModel.tag { name = result });

                var cate = (int)this.ObjectType + "";
                var code = cate + "_" + this.ObjectCode + "_" + tag.code;
                var objectTag = entity.object_tag_map.FirstOrDefault(p => p.code == code);
                if (objectTag == null)
                {
                    entity.object_tag_map.Add(new object_tag_map
                    {
                        code = code,
                        category_code = (int)this.ObjectType + "",
                        createdate = DateTime.Now,
                        object_code = this.ObjectCode,
                        tag_code = tag.code + "",
                        tag_name = tag.name,
                        index_code = define.code,
                        cycle = this.Cycle.ToString().ToLower()
                    });
                }

                entity.SaveChanges();
            }
        }
        private IList<IndexData> CalculateData(IndexDefinitionInfo define)
        {

            //获取算法

            var contextOld = GetTechContext(define);

            var contextNew = "";
            IList<IndexData> result = IndexCalculate.GetIndexData(define.algorithm_script, define.parameter, contextOld, out contextNew, PriceList);

            SaveTechContext(contextNew, define);

            return result;
        }

        private string GetTechContext(IndexDefinitionInfo define)
        {
            string code = (int)this.ObjectType + "_" + this.ObjectCode + "_" + define.code + "_" + this.Cycle;
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var context = entity.tech_context.FirstOrDefault(p => p.code == code);
                if (context == null)
                    return string.Empty;
                return context.context;
            }
        }

        private void SaveTechContext(string context, IndexDefinitionInfo define)
        {
            string objCode = (int)this.ObjectType + "";
            string code = objCode + "_" + this.ObjectCode + "_" + define.code + "_" + this.Cycle;

            using (var entity = new StockManDBEntities())
            {
                var item = entity.tech_context.FirstOrDefault(p => p.code == code);
                if (item == null)
                {
                    entity.tech_context.Add(new tech_context()
                    {
                        code = code,
                        category_code = objCode,
                        object_code = this.ObjectCode,
                        index_code = define.code,
                        context = context,
                        createtime = DateTime.Now
                    });
                }
                else
                {
                    item.context = context;
                    item.createtime = DateTime.Now;
                }
                try
                {
                    entity.SaveChanges();
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex.Message);
                }
            }
        }
        protected IList<IndexData> GetLastIndexData(IndexDefinitionInfo define)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = @"select  * from " + define.table_name + " where f_code='" + this.ObjectCode + "' order by code desc limit 50";

                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    IDataReader reader = commond.ExecuteReader();

                    IList<IndexData> list = new List<IndexData>();
                    while (reader.Read())
                    {

                        IndexData id = new IndexData();
                        id.date = DateTime.Parse(reader["date"] + "");
                        foreach (var filed in define.fields)
                        {
                            id.Add(double.Parse(reader[filed.name] + ""));
                        }

                        list.Add(id);
                    }
                    entity.Database.Connection.Close();
                    return list.OrderBy(p => p.date).ToList();
                }
            }
        }

        private void SaveTechData(IndexDefinitionInfo define, IList<IndexData> result)
        {
            IList<string> fields = define.fields.Select(p => p.name).ToList();
            switch (this.Cycle)
            {
                case TechCycle.day:
                    indexService.AddTechByDay(define.table_name, fields, this.ObjectCode, result);
                    break;
                case TechCycle.week:
                    indexService.AddTechByWeek(define.table_name, fields, this.ObjectCode, result);
                    break;
                case TechCycle.month:
                    indexService.AddTechByMonth(define.table_name, fields, this.ObjectCode, result);
                    break;

            }


            //string str = "";
            //string table = define.table_name;
            //foreach (Field f in define.fields)
            //{
            //    if (str.Length == 0)
            //    {
            //        str = f.name;
            //    }
            //    else
            //    {
            //        str += "," + f.name;
            //    }
            //}
            //string sqlTemplate = string.Format(@"insert into {0} (code,f_code,date,{1})values", table, str) + "({0})";

            //foreach (IndexData ds in result)
            //{
            //    //入库
            //    //字段映射，0->k,1->d,2->j
            //    string v = string.Format("'{0}','{1}','{2}'", ObjectCode + ds.date.ToString("yyyyMMdd"), ObjectCode,
            //        ds.date.ToString("yyyy-MM-dd"));

            //    foreach (double d in ds)
            //    {
            //        v += ",'" + d + "'";
            //    }
            //    string sql = string.Format(sqlTemplate, v);

            //    using (StockManDBEntities entity = new StockManDBEntities())
            //    {
            //        entity.Database.ExecuteSqlCommand(sql);
            //    }
            //}
        }

        protected void BuidTable(IndexDefinitionInfo define)
        {
            string sql = @"create table if not exists `{0}` ({1}, PRIMARY KEY (`code`),KEY `f_code` (`f_code`));";

            //string sql = @"if not exists (select * from sysobjects where id = object_id(N'{0}') and OBJECTPROPERTY(id, N'IsUserTable') = 1)"
            //           + @"create table {0} ({1})";
            string sql1 = @"code varchar(50) NOT NULL,f_code varchar(50) NULL, date datetime NULL";
            foreach (var field in define.fields)
            {
                sql1 += "," + field.name + " float NULL";
            }

            string result = string.Format(sql, define.table_name, sql1);

            using (var entity = new StockManDBEntities())
            {
                //this.Log().Info("构建表：" + define.table_name);
                entity.Database.ExecuteSqlCommand(result);
            }
        }

        protected IList<IndexDefinitionInfo> index_definition_infos()
        {
            var list = indexService.FindAll().Where(p => p.state == 1).ToList();

            IList<IndexDefinitionInfo> defineList = list.Select(define => new IndexDefinitionInfo
            {
                code = define.code,
                name = define.name,
                fields = JsonConvert.DeserializeObject<IList<Field>>(define.fields),
                parameter = define.parameter,
                table_name = string.Format("Tech_{0}_{1}_{2}", this.ObjectType.ToString(), define.table_name, this.Cycle.ToString()),
                algorithm_script = define.algorithm_script
            }).ToList();
            return defineList;
        }


    }
}
