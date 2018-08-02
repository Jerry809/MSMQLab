using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Xml;
using Newtonsoft.Json;

namespace MSMQWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            SendMessage();
            //using (var writer = new StreamWriter(File.Open("D:\\msmq.txt", FileMode.OpenOrCreate)))
            //{
            //    for (int i = 0; i < args.Length; i++)
            //    {
                    
            //            writer.WriteLine($"{i}--{args[i]}");
                   
            //    }
            //}
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
            customerList.Add(new Customer { Name = "worker", Age = 11 });
         


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
