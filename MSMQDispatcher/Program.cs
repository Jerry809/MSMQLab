using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MSMQDispatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            SendAndReceive();
            //for (int i = 0; i < 10; i++)
            //{
            //    SendMessage();
            //}
        }

        public static void SendAndReceive()
        {
            string msmqPath = @".\Private$\json";

            MessageQueue mq ;
            if (MessageQueue.Exists(msmqPath))
                mq = new MessageQueue(msmqPath);
            else
                mq = MessageQueue.Create(msmqPath);
            
            Message msg = new Message();
            msg.Body = "{\"empoyees\":[{\"firstName\":\"Bill\", \"lastName\":\"Gates\"}, {\"firstName\":\"Indiana\", \"lastName\":\"Jones\"}]}";
            //mq.Send(msg);
            //SendMessage();
            // 接收消息msgReceive
            // 设置消息的格式
            // 把消息转换为字符串格式
            // ...
            Message msgReceive = mq.Receive();
            msgReceive.Formatter = new StringMessageFormatter();// new XmlMessageFormatter(new Type[] { typeof(string) });
            string receivedMsmq = msgReceive.Body.ToString();

            Console.Write(receivedMsmq);
            //mq.Purge();// 删除队列中所有消息
            Console.ReadLine();
        }

        public static void SendMessage()
        {
            MessageQueue myQueue;
            string path = "";
            //local private
            path = ".\\private$\\json"; //本機
            //path = @"FormatName:Direct=OS:fij-nb\private$\MyQueue"; //遠端主機名稱
            //path = @"FormatName:Direct=TCP:192.168.0.103\private$\MyQueue";//遠端主機IP
            if (path.StartsWith(@".\"))// path=@".\Private$\MyQueue"
            {
                if (MessageQueue.Exists(path))
                {
                    myQueue = new MessageQueue(path);
                }
                else
                {
                    myQueue = MessageQueue.Create(path);
                }
            }
            else
            {
                myQueue = new MessageQueue(path);
            }


            List<Customer> customerList = new List<Customer>();
            customerList.Add(new Customer { Name = "A", Age = 11 });
            customerList.Add(new Customer { Name = "B", Age = 21 });
            customerList.Add(new Customer { Name = "C", Age = 13 });


            Message MyMessage = new Message();
            //set formatter for Message
            MyMessage.Formatter = new StringMessageFormatter();

            MyMessage.Body = JsonConvert.SerializeObject(customerList);
            MyMessage.Label = JsonConvert.SerializeObject(customerList);
            MyMessage.Priority = MessagePriority.High;
            myQueue.Send(MyMessage);
            Console.WriteLine("path:{0}", path);
        }
    }

    public class StringMessageFormatter : IMessageFormatter, ICloneable
    {
        public bool CanRead(Message message)
        {
            return message.BodyStream != null;
        }

        public object Clone()
        {
            return new StringMessageFormatter();
        }

        public object Read(Message message)
        {
            if (message.BodyStream == null)
            {
                return null;
            }

            var bytes = new byte[message.BodyStream.Length];
            message.BodyStream.Read(bytes, 0, bytes.Length);
            message.Body = System.Text.Encoding.Unicode.GetString(bytes); ;
            return message.Body;
        }

        public void Write(Message message, object obj)
        {
            var str = obj as string;
            if (str != null)
            {
                //MSMQ Triggers是將Body以Unicode的方式傳給Console
                //如果不用Unicode就要在Console處理轉碼
                //還有因為UTF-8是1-4 Byte，但Unicode 是 2 Byte
                //被MSMQ Triggers硬轉成Unicode時，奇數Byte會掉一個Byte
                var bytes = Encoding.Unicode.GetBytes(str);
                message.BodyStream = new System.IO.MemoryStream(bytes);
            }
        }
    }

    public class Customer
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }


}
