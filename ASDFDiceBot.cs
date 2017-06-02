using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ASDFDiceBot
{
    class Program
    {
        private static string _cookie = "";
        private static List<KeyValuePair<string, string>> leftalone = new List<KeyValuePair<string, string>>();
        private static List<string> disapproved = new List<string>();
        private static List<KeyValuePair<string, string>> allleftalone = new List<KeyValuePair<string, string>>();
        private static List<string> alldisapproved = new List<string>();


        static void Main(string[] args)
        {

            string choice = "go";
            login();
            while (choice != "stop")
            {
                choice = menu();
            }
        }

        public static string menu()
        {
            string forum = "";
            string topic = "";
            string message = "";
            string choice = "";
            int numDice = 0;
            int numSides = 0;
            int modifier = 0;
            int total = 0;
            Random rand = new Random();

            Console.WriteLine("\n\npost - Posts a message on a topic on a forum");
            Console.WriteLine("roll - Rolls a die with modifiers onto a topic on a forum");
            Console.WriteLine("stop - Stops the program");
            Console.WriteLine("What is choose:");
            choice = Encode(Console.ReadLine());

            switch (choice)
            {
                case "post":
                    Console.WriteLine("Ready to post");
                    Console.WriteLine("Forum ID:");
                    forum = Encode(Console.ReadLine());
                    Console.WriteLine("Topic ID:");
                    topic = Encode(Console.ReadLine());
                    Console.WriteLine("Message:");
                    message = Encode(Console.ReadLine());
                    post(forum, topic, message);
                    break;
                case "roll":
                    Console.WriteLine("Ready to post");
                    Console.WriteLine("Forum ID:");
                    forum = Encode(Console.ReadLine());
                    Console.WriteLine("Topic ID:");
                    topic = Encode(Console.ReadLine());
                    Console.WriteLine("Number of Dice:");
                    numDice = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Number of Sides on the Die:");
                    numSides = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Total Modifier:");
                    modifier = Int32.Parse(Console.ReadLine());
                    message += "Rolling " + numDice + "d" + numSides + " + (" + modifier + ")\n";
                    message += "( ";
                    for (int d = numDice; d > 0; d--)
                    {
                        int dieRoll = rand.Next(numSides) + 1;
                        total += dieRoll;
                        message += dieRoll.ToString();
                        if (d > 1) { message += " + "; }
                        Console.WriteLine("dice:" + dieRoll);
                    }
                    message += ") + ( ";
                    message += modifier + " ) = ";
                    total += modifier;
                    Console.WriteLine("mod:" + modifier);

                    message += total;
                    Console.WriteLine("total:" + total);

                    post(forum, topic, Encode(message));
                    break;
                case "stop":
                    break;
            }
            return choice;
        }

        public static void post(string forum, string topic, string message)
        {
            HttpWebResponse response = null;
            HttpWebRequest webRequest = null;
            string source = string.Empty;
            string lastClick = string.Empty;
            string creationTime = string.Empty;
            string formToken = string.Empty;
            string Url = "http://forums.asdf.com/";
            CookieContainer cookieJar = new CookieContainer();

            // GET
            while (true)
            {
                webRequest =
                    (HttpWebRequest)HttpWebRequest.Create(Url + "posting.php?mode=reply&f=" + forum + "&t=" + topic);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Accept = "text/html,application/xhtml+xml,application/xml;";
                webRequest.Headers["Accept-Language"] = "en-US,en;q=0.5";
                webRequest.Method = "GET";
                webRequest.Headers["Cookie"] = _cookie;

                ServicePointManager.Expect100Continue = false;

                try
                {
                    response = (HttpWebResponse)webRequest.GetResponse();
                }
                catch (Exception ex)
                {
                    continue;
                }
                break;
            }
            Console.WriteLine("Page Loaded!");

            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            source = streamReader.ReadToEnd();
            streamReader.Close();

            response.Close();

            // Get stuff
            // last click
            Match lastClickMatch = Regex.Match(source, "name=\"lastclick\" value=\"([0-9]{10})\" />");
            if (lastClickMatch.Success) lastClick = lastClickMatch.Groups[1].Value;

            // creation time
            Match creationTimeMatch = Regex.Match(source, "name=\"creation_time\" value=\"([0-9]{10})\" />");
            if (creationTimeMatch.Success) creationTime = creationTimeMatch.Groups[1].Value;

            // form token
            Match formTokenMatch = Regex.Match(source, "name=\"form_token\" value=\"(.{40})\" />");
            if (formTokenMatch.Success) formToken = formTokenMatch.Groups[1].Value;

            Console.WriteLine("waiting 8 seconds");
            for (int j = 8; j > 0; j--)
            {
                Console.Write(j + "...");
                System.Threading.Thread.Sleep(1000);
            }
            // POST
            webRequest = (HttpWebRequest)WebRequest.Create(Url + "posting.php?mode=reply&f=" + forum + "&t=" + topic);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;";
            webRequest.Headers["Accept-Language"] = "en-US,en;q=0.5";
            webRequest.Headers["Accept-Encoding"] = "gzip, deflate";
            webRequest.Method = "POST";
            webRequest.Headers["Cookie"] = _cookie;
            string data = "icon=&subject=" + Encode("Re: FROM LUNARBOT") + "&addbbcode20=100&message=" + message + "&attach_sig=on&post=Submit&lastclick=" + lastClick + "&creation_time=" + creationTime + "&form_token=" + formToken;

            byte[] byte1 = Encoding.UTF8.GetBytes(data);
            webRequest.ContentLength = byte1.Length;

            ServicePointManager.Expect100Continue = false;

            Stream stream = webRequest.GetRequestStream();
            stream.Write(byte1, 0, byte1.Length);
            stream.Close();

            response = (HttpWebResponse)webRequest.GetResponse();

            response.Close();
        }

        public static void login()
        {
            Console.WriteLine("Username:");
            string username = Encode(Console.ReadLine());
            Console.WriteLine("Password:");
            string pw = Encode(GetPassword());
            Console.Clear();
            Console.WriteLine("Logging in...");
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://forums.asdf.com/ucp.php?mode=login");
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;";
            req.Headers["Accept-Language"] = "en-US,en;q=0.5";
            req.Headers["Accept-Encoding"] = "gzip, deflate";
            req.Method = "POST";
            string sentdata = string.Format("username={0}&password={1}&autologin=on&login=Login&redirect=.%2Findex.php%3F", username, pw);
            byte[] sentdatabytes = Encoding.UTF8.GetBytes(sentdata);
            using (Stream stream = req.GetRequestStream())
                stream.Write(sentdatabytes, 0, sentdatabytes.Length);
            int i;
            string uid = "";
            string sid = "";
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse()) //sends the webrequest, gets the response
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Fail("login: " + response.StatusDescription);
                string c = response.Headers["Set-Cookie"];
                int ui = c.LastIndexOf("phpbb3_fwjdi_u=") + 15;
                int ki = c.LastIndexOf("phpbb3_fwjdi_k=") + 15;
                int sidi = c.LastIndexOf("phpbb3_fwjdi_sid=") + 17;
                if (ui == -1 || ki == -1 || sidi == -1) //couldn't find the cookie info in response, login failed
                    Fail("Error, couldn't log in :(");
                i = ui;
                while (c[i] != ';')
                    uid += c[i++];
                string k = "";
                i = ki;
                while (c[i] != ';')
                    k += c[i++];
                i = sidi;
                while (c[i] != ';')
                    sid += c[i++];
                if (uid == "" || k == "" || sid == "") //some of the cookie info was empty, login failed
                    Fail("couldn't log in :(");
                _cookie = string.Format("style_cookie=printonly; phpbb3_fwjdi_u={0}; phpbb3_fwjdi_k={1}; phpbb3_fwjdi_sid={2}", uid, k, sid); //storing received cookie info
            }
            Console.Clear();
            Console.WriteLine("Successfully logged in");
        }

        private static string Encode(string s)
        {
            s = System.Uri.EscapeDataString(s);//HttpUtility.UrlEncode(s);
            char[] invalid = { '_', '.', '!', '*', '\'', '(', ')' }; //chars that are not encoded by HttpUtility.UrlEncode: http://msdn.microsoft.com/en-us/library/system.net.webutility.urlencode%28v=vs.110%29.aspx
            List<char> toReplace = new List<char>();
            foreach (char c in s)
            {
                if (invalid.Contains(c) && !toReplace.Contains(c))
                    toReplace.Add(c);
            }
            foreach (char c in toReplace)
                s = s.Replace(c.ToString(), Uri.HexEscape(c));
            return s;
        }

        private static void Fail(string reason)
        {
            Console.WriteLine(reason);
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            Environment.Exit(0);
        }

        /// <summary>Gets the next (currently first) page of unapproved posts as a string</summary>
        private static string GetNextPage()
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://forums.asdf.com/mcp.php?i=queue&mode=unapproved_posts");
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;";
            req.Headers["Accept-Language"] = "en-US,en;q=0.5";
            req.Method = "GET";
            req.Headers["Cookie"] = _cookie;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Fail("while getting a page of unapproved posts: " + response.StatusDescription);
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>Asks for the mod's password</summary>
        public static string GetPassword()
        {
            string pwd = string.Empty;
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.Remove(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd += i.KeyChar;
                    Console.Write("*");
                }
            }
            return pwd;
        }
    }
}
