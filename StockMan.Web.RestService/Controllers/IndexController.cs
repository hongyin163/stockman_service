using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Facade.Models;
using data = StockMan.EntityModel;
using Newtonsoft.Json;
using StockMan.Web.RestService.Filters;
using StockMan.Service.Cache;
namespace StockMan.Web.RestService.Controllers
{
    [IdentityBasicAuthentication]
    public class IndexController : ApiController
    {
        IIndexService service = new IndexService();
        IUserDataVersionService versionService = new UserDataVersionService();
        IRuleService ruleService = new RuleService();
        // GET api/index
        [HttpOptions]
        [HttpGet]
        public IList<IndexDefinition> Get()
        {
            var indexStr = CacheHelper.Get("index");
            if (string.IsNullOrEmpty(indexStr))
            {
                var list = service.FindAll();
                var result = list
                    .Where(p => p.state == 1)
                    .Select(p => new IndexDefinition
                {
                    code = p.code,
                    description = p.description,
                    fields = p.fields,
                    name = p.name,
                    parameter = p.parameter,
                    table_name = p.table_name,
                    algorithm_script = p.algorithm_script,
                    chart_config = p.chart_config,
                    state = p.state ?? 0,
                    version = p.version ?? 0,
                    group_code = p.indexdefinegroup.code,
                    group_name = p.indexdefinegroup.group_name

                }).ToList();
                CacheHelper.Set("index", result);
                return result;
            }
            else
            {
                return CacheHelper.Get<IList<IndexDefinition>>("index");
            }

        }

        // GET api/index/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/index
        public IHttpActionResult Post(IndexDefinition data)
        {

            var u = service.Find(data.code);
            if (u == null)
            {
                service.Add(new data.indexdefinition
                {
                    code = data.code,
                    name = data.name,
                    description = data.description,
                    parameter = data.parameter,
                    fields = data.fields,
                    table_name = data.table_name,
                    algorithm_script = data.algorithm_script,
                    chart_config = data.chart_config,
                    state = 0,
                    version = 1,
                    group_code = data.group_code
                });
                CacheHelper.Remove("index");
                return Ok<IndexDefinition>(data);
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, this);
            }
        }

        // PUT api/index/5
        public IHttpActionResult Put(int id, IndexDefinition data)
        {

            service.Update(new data.indexdefinition
            {
                code = data.code,
                name = data.name,
                description = data.description,
                parameter = data.parameter,
                fields = data.fields,
                table_name = data.table_name,
                algorithm_script = data.algorithm_script,
                chart_config = data.chart_config,
                state = data.state,
                version = data.version,
                group_code = data.group_code
            });
            return Ok<IndexDefinition>(data);
        }

        // DELETE api/index/5
        public void Delete(int id)
        {
        }
        //更新指数
        public IHttpActionResult Update(IndexDefinition data)
        {
            if (data == null || string.IsNullOrEmpty(data.code))
                return BadRequest("参数不合规");
            //try
            //{
            service.Update(new data.indexdefinition
            {
                code = data.code,
                name = data.name,
                description = data.description,
                parameter = data.parameter,
                fields = data.fields,
                table_name = data.table_name,
                algorithm_script = data.algorithm_script,
                chart_config = data.chart_config,
                state = data.state,
                group_code = data.group_code
            });
            CacheHelper.Remove("index");
            return Ok<IndexDefinition>(data);
            //}
            //catch (Exception ex)
            //{
            //    return this.InternalServerError(ex);
            //}

        }
        /// <summary>
        /// 发布指数
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public IHttpActionResult Release(string code)
        {
            service.Release((code));
            return Ok();
        }

        /// <summary>
        /// 获取指数数据
        /// </summary>
        /// <param name="p1">指数类型，股票、行业</param>
        /// <param name="p2">技术指标名称</param>
        /// <param name="p3">指数周期，日，周，月</param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public string[][] GetIndexData(string p1, string p2, string p3, string p4)
        {
            if (string.IsNullOrEmpty(p3))
                p3 = "Day";

            var cycle = (data.TechCycle)Enum.Parse(typeof(data.TechCycle), p3, true);

            return service.GetIndexData(p1, p2, cycle, p4);
        }



        /// <summary>
        /// 同步上传
        /// </summary>
        /// <param name="myIndex"></param>
        /// <returns></returns>
        public IHttpActionResult PostMyIndexs(MyIndex myIndex)
        {
            IList<data.index_user_map> list = (from t in myIndex.indexs
                                               select new data.index_user_map
                                               {
                                                   user_id = myIndex.user_id,
                                                   index_code = t.code,
                                                   sort = t.sort,
                                                   paramaters = t.parameter
                                               }).ToList();

            service.SyncMyIndex(myIndex.user_id, list);
            versionService.UpdateUserDataVersion(myIndex.user_id, data.DataVersionCode.my_index.ToString(), myIndex.version);

            return Ok<Message>(new Message { success = true });
        }
        /// <summary>
        /// 同步下载
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IHttpActionResult GetMyIndexs(string id)
        {
            var list = service.GetMyIndexs(id);

            //IList<string> codeList = list.Select(p => p.).ToList();

            var query = from t in list
                        select new IndexDefinition
                        {
                            code = t.index_code,
                            name = t.indexdefinition.name,
                            sort = t.sort,
                            chart_config = t.indexdefinition.chart_config,
                            algorithm_script = t.indexdefinition.algorithm_script,
                            description = t.indexdefinition.description,
                            fields = t.indexdefinition.fields,
                            parameter = t.indexdefinition.parameter,
                            state = t.indexdefinition.state
                        };
            //decimal version = versionService.GetUserDataVersion(id, data.DataVersionCode.my_index.ToString());

            var my = new MyIndex()
            {
                user_id = id,
                indexs = query.ToList()
                //,
                //version = version
            };

            return Ok<MyIndex>(my);
        }

        /// <summary>
        /// 获取用户的规则
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<MyRule> GetMyRules(string id)
        {
            var list = ruleService.GetMyRule(id);
            IList<MyRule> ruleList = list.Select(rule => new MyRule
            {
                code = rule.code,
                user_id = rule.user_id,
                name = rule.name,
                state = rule.state ?? 0,
                description = "",//rule.description,
                conditions = rule.rulecondition.Select(p => new Condition()
                    {
                        code = p.code,
                        category_code = p.category_code,
                        object_code = p.object_code,
                        index_code = p.index_code,
                        category_name = p.category_name,
                        object_name = p.object_name,
                        index_name = p.index_name,
                        rule_code = p.rule_code,
                        sort = p.sort ?? 0
                    }).ToList()
            }).ToList();

            return ruleList;
        }

        public IHttpActionResult PostMyRules(IList<MyRule> list)
        {

            //var myRule1 = new data.Rule
            //{
            //    user_id = myRule.user_id,
            //    name = myRule.name,
            //    Condition = myRule.conditions.Select(p => new data.Condition()
            //    {
            //        code = p.code,
            //        category_code = p.category_code,
            //        object_code = p.object_code,
            //        index_code = p.index_code,
            //        category_name = p.category_name,
            //        object_name = p.object_name,
            //        index_name = p.index_name,
            //        sort = p.sort
            //    }).ToList()
            //};
            //ruleService.Add(myRule1);

            if (list.Count == 0)
                return BadRequest();

            string userId = list[0].user_id;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            ruleService.RemoveByUserId(userId);

            IList<data.rule> ruleList = list.Select(rule => new data.rule
            {
                user_id = rule.user_id,
                name = rule.name,
                code = rule.code,
                state = rule.state,
                rulecondition = rule.conditions.Select(p => new data.rulecondition()
                {
                    code = p.code,
                    category_code = p.category_code,
                    object_code = p.object_code,
                    index_code = p.index_code,
                    category_name = p.category_name,
                    object_name = p.object_name,
                    index_name = p.index_name,
                    rule_code = p.rule_code,
                    sort = p.sort
                }).ToList()
            }).ToList();

            ruleService.AddRange(ruleList);

            return Ok(new Message() { code = "200", content = "", success = true });
        }

        public IList<IndexDefineGroup> GetGroups()
        {
            var list = service.GetGroups();
            return list.Select(p => new IndexDefineGroup()
            {
                code = p.code,
                group_name = p.group_name
            }).ToList();

        }

        public IHttpActionResult PostGroup(IndexDefineGroup group)
        {
            if (string.IsNullOrEmpty(group.code) || string.IsNullOrEmpty(group.group_name))
            {
                return BadRequest("参数不完整");
            }
            service.AddGroup(new data.indexdefinegroup()
            {
                code = group.code,
                group_name = group.group_name
            });
            return Ok<IndexDefineGroup>(group);
        }

        public IHttpActionResult DeleteGroup(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("参数不完整");
            }
            service.DeleteGroup(id);
            return Ok(new Message() { success = true });
        }


        public IList<ObjectState> GetState(string id)
        {
            IList<string> codeList = id.Split(',').ToList();
            var list = service.GetObjectStates(codeList);
            return list.Select(p => new ObjectState
            {
                code = p.code,
                category_code = p.category_code,
                object_code = p.object_code,
                index_code = p.index_code,
                date = p.date,
                day = p.day,
                week = p.week,
                month = p.month,
                last_day = p.last_day,
                last_week = p.last_week,
                last_month = p.last_month
            }).ToList();
        }

        public IHttpActionResult GetIndexByCodes(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            string[] codes = id.Split(',');
            var list = service.GetIndexByCodes(codes);

            IEnumerable<IndexDefinition> result = list.Select(p => new IndexDefinition
            {
                code = p.code,
                name = p.name,
                algorithm_script = p.algorithm_script,
                chart_config = p.chart_config,
                fields = p.fields,
                parameter = p.parameter
            });
            return Ok<IEnumerable<IndexDefinition>>(result);
        }

    }
}
