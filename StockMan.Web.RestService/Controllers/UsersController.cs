using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using StockMan.Facade.Models;
using StockMan.Service.Interface;
using StockMan.Service.Rds;
using StockMan.MySqlAccess;
using data = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.EntityModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Web.Http.OData;
using StockMan.Web.RestService.Filters;
namespace StockMan.WebAPI.Controllers
{
    [IdentityBasicAuthentication]
    public class UsersController : ApiController
    {
        IUserService service = new UserService();
        IUserDataVersionService versionService = new UserDataVersionService();

        private StockManDBEntities db = new StockManDBEntities();

        public IHttpActionResult GetUser(string Id)
        {

            var user = service.Find(Id);
            if (user == null)
                return NotFound();
            User u = new User
            {
                id = user.id,
                name = user.name,
                email = user.email,
                phone = user.phone,
                points = user.points ?? 0,
                exp = user.exp ?? 0
            };
            return Json<User>(u);


            //using (Service.Dynamo.UserService service = new Service.Dynamo.UserService())
            //{
            //    var user = service.Find(Id);
            //    if (user == null)
            //        return NotFound();
            //    User u = new User
            //    {
            //        Id = user.Id,
            //        Name = user.Name,
            //        Email = user.Email,
            //        Phone = user.Phone,
            //        Points = user.Points,
            //        Exp = user.Exp
            //    };
            //    return Json<User>(u);
            //}
        }

        [ActionName("add")]
        public IHttpActionResult PostUser(User user)
        {

            var u = service.Find(user.id);
            if (u == null)
            {
                service.Add(new data.users
                {
                    id = user.id,
                    name = user.name,
                    email = user.email,
                    exp = user.exp,
                    phone = user.phone,
                    points = user.points,
                    password = user.password
                });
                return Ok<User>(user);
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, this);
            }


        }
        [ActionName("update")]
        public IHttpActionResult PutUser(User user)
        {

            service.Add(new data.users
            {
                id = user.id,
                name = user.name,
                email = user.email,
                exp = user.exp,
                phone = user.phone,
                points = user.points,
                password = user.password
            });

            return Ok();

        }
        [ActionName("Login")]
        public IHttpActionResult Login(User data)
        {
            if (string.IsNullOrEmpty(data.id.Trim())
              || string.IsNullOrEmpty(data.password.Trim()))
                return BadRequest("账号或密码为空");

            var user = service.Find(data.id);
            if (user != null && user.password == data.password)
                return Ok(new User
                {
                    id = user.id,
                    name = user.name,
                    email = user.email,
                    exp = user.exp ?? 0,
                    phone = user.phone,
                    points = user.points ?? 0,
                    password = user.password,
                    sso = user.sso
                });
            else
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

        }
        [ActionName("regist")]
        public IHttpActionResult Regist(User user)
        {
            if (string.IsNullOrEmpty(user.id.Trim())
                || string.IsNullOrEmpty(user.password.Trim()))
                return BadRequest("账号或密码为空");
            Regex emailRx = new Regex(@"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$");
            Regex phoneRx = new Regex(@"^0?(13[0-9]|15[012356789]|18[0236789]|14[57])[0-9]{8}$");

            if (string.IsNullOrEmpty(user.sso))
            {
                if (!emailRx.IsMatch(user.id) && !phoneRx.IsMatch(user.id))
                {
                    return BadRequest("账号应为邮箱或者手机号");
                }
            }
            user.id = user.id.Trim();
            user.password = user.password.Trim();
            var u = service.Find(user.id);
            if (u == null)
            {
                //新增用户
                var newUser = new data.users
                {
                    id = user.id,
                    name = user.name,
                    email = user.email,
                    exp = 1,
                    phone = user.phone,
                    points = 500,
                    password = user.password,
                    sso = user.sso
                };
                service.Add(newUser);
                //新增用户版本数据
                //if(user.id
                // /^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$/ 
                //^0?(13[0-9]|15[012356789]|18[0236789]|14[57])[0-9]{8}$

                UserConfigItem ci = new UserConfigItem();
                if (emailRx.IsMatch(user.id))
                {
                    ci.email = user.id;
                }

                if (phoneRx.IsMatch(user.id))
                {
                    ci.phone = user.id;
                }

                service.SaveUserConfig(new sys_userconfig
                {
                    code = user.id,
                    config = JsonConvert.SerializeObject(ci)
                });

                IList<data.userdataversion> dvlist = new List<data.userdataversion>();

                dvlist.Add(new data.userdataversion
                    {
                        code = data.DataVersionCode.my_stock.ToString(),
                        user_id = user.id,
                        version = 1,
                        update_time = DateTime.Now
                    });
                //dvlist.Add(new data.userdataversion
                //  {
                //      code = data.DataVersionCode.my_category.ToString(),
                //      user_id = user.id,
                //      version = 1,
                //      update_time = DateTime.Now
                //  });

                versionService.AddRange(dvlist);

                return Ok<User>(new User
                {
                    id = newUser.id,
                    name = newUser.name,
                    email = newUser.email,
                    exp = newUser.exp ?? 0,
                    phone = user.phone,
                    points = newUser.points ?? 0,
                    password = newUser.password,
                    sso = newUser.sso

                });
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, this);
            }


        }


        public IHttpActionResult SaveSSOAcount(User user)
        {
            if (string.IsNullOrEmpty(user.id.Trim()) 
                || string.IsNullOrEmpty(user.password.Trim()))
                return BadRequest("账号或密码为空");


            user.id = user.id.Trim();
            user.password = user.password.Trim();
         

            var u = service.Find(user.id);

            if (u == null)
            {
                //新增用户
                var newUser = new data.users
                {
                    id = user.id,
                    name = user.name,
                    email = user.email,
                    exp = 1,
                    phone = user.phone,
                    points = 500,
                    password = user.password,
                    sso = user.sso
                };
                service.Add(newUser);
                return Ok<User>(new User
                {
                    id = newUser.id,
                    name = newUser.name,
                    email = newUser.email,
                    exp = newUser.exp ?? 0,
                    phone = newUser.phone,
                    points = newUser.points ?? 0,
                    password = newUser.password,
                    sso = newUser.sso
                });
            }
            else
            {
                return Ok<User>(new User
                {
                    id = u.id,
                    name = u.name,
                    email = u.email,
                    exp = u.exp ?? 0,
                    phone = u.phone,
                    points = u.points ?? 0,
                    password = u.password,
                    sso = u.sso
                });
            }
  

        }
        protected override void Dispose(bool disposing)
        {
            service.Dispose();
            base.Dispose(disposing);
        }


        public IHttpActionResult GetUserConfig()
        {
            var id = this.User.Identity.Name;
            if (string.IsNullOrEmpty(id))
                return BadRequest();
            var config = service.GetUserConfig(id);
            var userConfig = new UserConfig
                {
                    code = id,
                    config = ""
                };
            if (config != null)
            {
                userConfig.code = config.code;
                userConfig.config = config.config;
            }
            return Ok<UserConfig>(userConfig);
        }
        [HttpOptions]
        [HttpPost]
        public IHttpActionResult PostUserConfig(UserConfig config)
        {
            if (this.Request.Method.Method == "OPTIONS")
            {
                return Ok();
            }
            if (config != null)
            {
                service.SaveUserConfig(new sys_userconfig
                {
                    code = config.code,
                    config = config.config
                });


            }
            return Ok();
        }

        public IHttpActionResult OPTIONSValue()
        {
            return Ok();
        }



    }
}
