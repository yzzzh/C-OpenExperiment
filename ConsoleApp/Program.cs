using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer srv;

        private Stream inputStream;
        public StreamWriter outputStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();


        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.socket = s;
            this.srv = srv;
        }


        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        public void process()
        {
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            inputStream = new BufferedStream(socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
            try
            {
                parseRequest();
                readHeaders();
                if (http_method.Equals("GET"))
                {
                    handleGETRequest();
                }
                else if (http_method.Equals("POST"))
                {
                    handlePOSTRequest();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                writeFailure();
            }
            outputStream.Flush();
            // bs.Flush(); // flush any remaining output
            inputStream = null; outputStream = null; // bs = null;            
            socket.Close();
        }

        public void parseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_versionstring = tokens[2];

            Console.WriteLine("starting: " + request);
        }

        public void readHeaders()
        {
            Console.WriteLine("readHeaders()");
            String line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                string value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}", name, value);
                httpHeaders[name] = value;
            }
        }

        public void handleGETRequest()
        {
            srv.handleGETRequest(this);
        }

        private const int BUF_SIZE = 4096;
        public void handlePOSTRequest()
        {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            Console.WriteLine("get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.httpHeaders.ContainsKey("Content-Length"))
            {
                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));
                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {
                    Console.WriteLine("starting Read, to_read={0}", to_read);

                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                    Console.WriteLine("read finished, numread={0}", numread);
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            Console.WriteLine("get post data end");
            srv.handlePOSTRequest(this, new StreamReader(ms));

        }

        public void writeSuccess()
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: text/html");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void writeFailure()
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }

    public abstract class HttpServer
    {

        protected int port;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            //listener用于监听端口
            listener = new TcpListener(port);
            listener.Start();
            while (is_active)
            {
                //TcpClient用于建立服务器端和客户端的链接
                TcpClient s = listener.AcceptTcpClient();
                //processor用于处理客户端的请求
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }

    public class MyHttpServer : HttpServer
    {
        public MyHttpServer(int port)
            : base(port)
        {
        }

        //处理GET请求
        public override void handleGETRequest(HttpProcessor p)
        {
            Console.WriteLine("request: {0}", p.http_url);
            p.writeSuccess();
            p.outputStream.WriteLine("<html><body><h1>Hello World!</h1>");
            p.outputStream.WriteLine("Current Time: " + DateTime.Now.ToString());
        }
        //处理POST请求
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            Console.WriteLine("POST request: {0}", p.http_url);
            string data = inputData.ReadToEnd();
            p.writeSuccess();

            string[] datas = data.Split('?');
            string source_code = datas[0];
            string input = datas[1];
            string output = datas[2];

            string response = Test(source_code, input, output);

            p.outputStream.WriteLine(response);
        }

        static string Test(string source_code, string input, string output)
        {
            //将源代码保存为c文件
            FileStream fs = new FileStream("source_code.c", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(source_code);
            sw.Close();
            fs.Close();

            //若target文件存在，先删除
            if (File.Exists("target.exe"))
            {
                File.Delete("target.exe");
            }

            //调用gcc编译c代码，生成exe文件
            Process gcc_prc = new Process();
            gcc_prc.StartInfo.FileName = "gcc.exe";
            gcc_prc.StartInfo.UseShellExecute = false;
            gcc_prc.StartInfo.RedirectStandardInput = true;
            gcc_prc.StartInfo.RedirectStandardOutput = true;
            gcc_prc.StartInfo.RedirectStandardError = true;
            gcc_prc.StartInfo.CreateNoWindow = true;
            gcc_prc.StartInfo.Arguments = "-o target.exe source_code.c";

            try
            {
                gcc_prc.Start();

                gcc_prc.WaitForExit();
                gcc_prc.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (!File.Exists("target.exe"))
            {
                return "Compile Error";
            }

            //运行这个exe文件，获取输出值
            Process target_prc = new Process();
            target_prc.StartInfo.FileName = "target.exe";
            target_prc.StartInfo.UseShellExecute = false;
            target_prc.StartInfo.RedirectStandardInput = true;
            target_prc.StartInfo.RedirectStandardOutput = true;
            target_prc.StartInfo.RedirectStandardError = true;
            target_prc.StartInfo.CreateNoWindow = true;

            string target_output = "";

            try
            {

                target_prc.Start();

                target_prc.StandardInput.WriteLine(input);
                target_prc.StandardInput.AutoFlush = true;

                target_output = target_prc.StandardOutput.ReadToEnd();

                target_prc.WaitForExit();
                target_prc.Close();
                Console.WriteLine(target_output);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (target_output == output)
            {
                return "Accept";
            }
            else
            {
                return "Refuse";
            }

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HttpServer httpServer = new MyHttpServer(7777);
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
        }

        
    }


}
