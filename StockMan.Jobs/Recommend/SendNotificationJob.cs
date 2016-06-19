using Newtonsoft.Json;
using Quartz;
using StockMan.EntityModel.dto;
using StockMan.MySqlAccess;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;

namespace StockMan.Jobs.Recommend
{

    public class SendNotificationJob : IJob
    {
        IUserService userService = new UserService();
        public void Execute(IJobExecutionContext context)
        {

            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var msgList = entity.user_message.Where(p => p.notice_state == 0).ToList();

                foreach (var u in msgList)
                {
                    string email = "";
                    var config = entity.sys_userconfig.Where(p => p.code == u.user_id).FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.config))
                    {
                        var configDetail = JsonConvert.DeserializeObject<UserConfigItem>(config.config);
                        email = configDetail.email;
                    }

                    if (string.IsNullOrEmpty(email))
                    {
                        if (u.user_id.IndexOf("@") > 0)
                        {
                            email = u.user_id;
                        }
                    }

                    if (!string.IsNullOrEmpty(email))
                    {

                        sendmail("慢牛提醒", u.content, "manniudata@163.com", email, "manniudata@163.com", "18669040658");

                        u.notice_state = 1;
                    }
                }
                entity.SaveChanges();
            }
        }

        private bool sendmail(string title, string content, string from, string to, string account, string pwd)
        {
            SmtpClient _smtpClient = new SmtpClient();
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式

            _smtpClient.Host = "smtp.163.com"; ;//指定SMTP服务器

            _smtpClient.Credentials = new System.Net.NetworkCredential(account, pwd);//用户名和密码

            MailMessage _mailMessage = new MailMessage(from, to);
            _mailMessage.Subject = title;//主题
            _mailMessage.Body = content;//内容
            _mailMessage.BodyEncoding = System.Text.Encoding.UTF8;//正文编码
            _mailMessage.IsBodyHtml = true;//设置为HTML格式
            _mailMessage.Priority = MailPriority.High;//优先级


            try
            {
                _smtpClient.Send(_mailMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
