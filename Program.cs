using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using ScriptSDK;
using System.Text.RegularExpressions;
using System.IO;
using ScriptSDK;
using ScriptSDK.Data;
using ScriptSDK.Engines;
using ScriptSDK.Gumps;
using ScriptSDK.Items;
using ScriptSDK.Utils;
using ScriptSDK.Mobiles;
using System.Diagnostics;
using System.Threading;
using ScriptDotNet2;
using System.Reflection;

namespace RailRunner
{
    class Program
    {
        static Dictionary<int, Item> runebooks = new Dictionary<int, Item>();
        static int currentRail = 1;
        static int curBook = 1;
        static List<string> railNames = new List<String>();
        static int numRecallButtons = 1;
        private static Dictionary<byte, uint> equippedItems = new Dictionary<byte, uint>();


        static void Main(string[] args)
        {
            consoleStartText();
            try
            {
                Console.WriteLine("Player name is " + StealthAPI.Stealth.Client.Self.Name);
            }
            catch (System.IO.FileNotFoundException e)
            {
                consoleErrorText(e);
                Console.ReadKey(false);
                Environment.Exit(0);
            }

            bool finishRail = checkDistance();

            if (finishRail)
            {
                string file = ReadTxt(StealthAPI.Stealth.Client.GetShardName());
                string[] fileSplit = file.Split(',');
                int waypoint = (ushort)int.Parse(fileSplit[0]);
                railNames.Add(fileSplit[1]);
                Points locations = loadRail(0, railNames);
                Console.WriteLine("Finishing the rail I left off on!");
                Console.WriteLine("Loading Rail: " + railNames[0]);
                System.Threading.Thread.Sleep(3000);
                bool changedRail = move(locations, 3, waypoint);

                if (StealthAPI.Stealth.Client.Self.IsDead)
                {
                    Console.WriteLine("You are dead, attempting to load Shrine Rail");

                    locations = loadShrineRail(0, railNames);

                    Console.WriteLine("Shrine Rail Loaded");
                    System.Threading.Thread.Sleep(3000);
                    bool changeRail2 = move(locations, 0, 0);
                    System.Threading.Thread.Sleep(10000);
                    // checkRez(); not working yet !!!
                    checkMounted();
                    if (StealthAPI.Stealth.Client.Self.IsDead)
                    {
                        ErrorExit("Error- You should have got auto rezzed but I failed.. Go get rezzed");
                    }
                    railNames.Clear();




                }
            }

            setup(); // gets dress, runebooks, railnames, turns on autoreconnect, and rail pos for user
            if (finishRail)
            {
                updateBookInfo();
            }
            //number of recall buttons is currently not checked..... needs updated (as of now assumes there are 16... thats when it changes books)


            List<int> recallButtonPos = new List<int>()
            {
                32,35,46,49,60,63,74,77,88,91,102,105,116,119,129,132
            };

            while (true)
            {

                Gump RunebookGump = OpenRunebook(runebooks[curBook]);
                System.Threading.Thread.Sleep(1000);
                int ButtonID = recallButtonPos[currentRail - 1];

                List<ScriptSDK.Gumps.GumpButton> Buttons = RunebookGump.Buttons;

                bool ClickResult = Buttons[ButtonID].Click();

                if (!ClickResult)
                    ErrorExit("Clicking Failed, this is a serious error. Ask Zoo.");


                System.Threading.Thread.Sleep(1500);
                Console.WriteLine("Loading Rail: " + railNames[currentRail - 1]);
                Points locations = loadRail(currentRail - 1, railNames);

                Console.WriteLine("New Rail Loaded");
                System.Threading.Thread.Sleep(3000);
                bool changedRail = move(locations, 3, 0);

                if (StealthAPI.Stealth.Client.Self.IsDead)
                {
                    Console.WriteLine("You are dead, attempting to load Shrine Rail");
                    if (changedRail)
                    {
                        locations = loadShrineRail(currentRail - 2, railNames);

                    }
                    else
                    {
                        locations = loadShrineRail(currentRail - 1, railNames);
                    }
                    Console.WriteLine("Shrine Rail Loaded");
                    System.Threading.Thread.Sleep(3000);
                    bool changeRail2 = move(locations, 0, 0);
                    System.Threading.Thread.Sleep(10000);

                    checkRez();
                    checkMounted();
                    if (StealthAPI.Stealth.Client.Self.IsDead)
                    {
                        ErrorExit("Error- You should have got auto rezzed but I failed.. Go get rezzed");
                    }




                }

                updateBookInfo();
                Thread.Sleep(2000);


                clear();



            }
            //ErrorExit("You reached the end of your rails!");


        }

        static void setup()
        {


            runebooks.Clear();
            equippedItems.Clear();

            saveCurrentDress();
            getRunebooks();

            if (runebooks.Count != 4)
            {
                runebooks.Clear();
                getRunebooks();
            }

            while (!runebooks.ContainsKey(0))
            {
                StealthAPI.Stealth.Client.Disconnect();
                Thread.Sleep(300000);
                StealthAPI.Stealth.Client.Connect();
                Thread.Sleep(10000);
                setup();
            }


            if (runebooks.Count != 4)
            {
                ErrorExit("Did not find all runebooks, check runebook names and try again.");
            }


            getInfo();
            railNames = getRailNames(runebooks[curBook]);
        }



        static void ErrorExit(string ErrorText)
        {
            Console.WriteLine(ErrorText);
            Console.Write("Press any key to End Program.");
            Console.ReadKey(false);
            Environment.Exit(0);

        }




        static void getRunebooks()
        {
            var player = ScriptSDK.Mobiles.PlayerMobile.GetPlayer();
            player.UpdateLocalizedProperties();
            player.UpdateTextProperties();

            Container backpack = player.Backpack;
            backpack.UpdateLocalizedProperties();
            backpack.UpdateTextProperties();
            Console.WriteLine(backpack.Tooltip);
            backpack.DoubleClick();

            List<Item> books = ScriptSDK.Items.Item.Find(typeof(Runebook), backpack.Serial.Value, false);
            System.Threading.Thread.Sleep(1000);
            char[] separators = { ' ', '|' };
            Console.WriteLine("Number of runebooks " + books.Count);
            for (int i = 0; i < books.Count; i++)
            {
                System.Threading.Thread.Sleep(500);
                bool update = books[i].UpdateTextProperties();
                System.Threading.Thread.Sleep(500);
                String bookInfo = books[i].Tooltip;

                String[] parseBook = bookInfo.Split(separators);
                int last = parseBook.Length - 1;

                String bookName = parseBook[last];
                switch (bookName)
                {
                    case "info":
                        runebooks.Add(0, books[i]); //Information Book
                        Console.WriteLine("Information runebook loaded");
                        break;
                    case "1":
                        runebooks.Add(1, books[i]); // Malas
                        Console.WriteLine("Malas runebook loaded");
                        break;
                    case "2":
                        runebooks.Add(2, books[i]); //Tram
                        Console.WriteLine("Tram runebook loaded");
                        break;
                    case "3":
                        runebooks.Add(3, books[i]); // Fel
                        Console.WriteLine("Fel runebook loaded");
                        break;
                    default:
                        //Runebook is not needed, so nothing happens
                        break;
                }


            }



        }

        static void getInfo()
        {

            char[] separators = { ' ', '|' };
            String[] parsedInfo = runebooks[0].Tooltip.Split(separators);

            String curFacet = (parsedInfo[parsedInfo.Length - 3]);
            Console.WriteLine("Current facet is " + curFacet);
            switch (curFacet.ToLower())
            {
                case "tram":
                    curBook = 2;
                    break;
                case "fel":
                    curBook = 3;
                    break;
                case "malas":
                    curBook = 1;
                    break;
                default:
                    ErrorExit("Error- Should not make it here in getInfo() ");
                    break;
            }
            currentRail = Int32.Parse(parsedInfo[parsedInfo.Length - 2]);

            Console.WriteLine("Starting rail position is " + currentRail);
        }

        static int getButtons(Gump RunebookGump)
        {
            int numRecallButtons = 66;
            Console.WriteLine(numRecallButtons);
            int numEmptyRunes = 18 - RunebookGump.HTMLTexts.Count;
            numRecallButtons -= numEmptyRunes;

            return numRecallButtons;

        }

        static List<string> getRailNames(Item railsRunebook)
        {

            List<string> railNames = new List<string>();

            PlayerMobile player = playerInfo();


            ScriptSDK.Items.Container backpack = player.Backpack;
            backpack.DoubleClick();

            System.Threading.Thread.Sleep(500);

            ScriptSDK.Gumps.Gump RunebookGump = OpenRunebook(railsRunebook);


            if (RunebookGump == null)
            {
                RunebookGump = OpenRunebook(railsRunebook);
            }
            if (RunebookGump == null)
            {
                ErrorExit("Couldn't open rails runebook.  Halting.");
            }

            List<GumpLabel> otherTexts = RunebookGump.Labels;
            foreach (GumpLabel line in otherTexts)
            {
                var cur = line.Text.ToLower();

                if (cur.Contains("stealth"))
                {
                    railNames.Add(cur + ".txt");
                }



            }
            return railNames;
        }


        static byte[] getRailHTTP(string rail)
        {
            string url = "http://www.lnrguild.com/rails/" + rail;

            // string fn = rail + ".txt";
            // var ret; //= "Download Failed!";
            // create a new instance of WebClient
            using (WebClient client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                client.Credentials = new NetworkCredential("railRunner2", "trump2019!");
                return client.DownloadData(url);
            }
        }








        static Points loadRail(int railNum, List<string> railNames)
        {
            Points location = new Points();
            string file = railNames[railNum];


            var result = getRailHTTP(file);
            string str = Encoding.UTF8.GetString(result);

            string[] lines = str.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);


            try
            {

                var i = 0;
                foreach (string line in lines)
                {
                    string[] words;
                    if (!char.IsDigit(line[0]))
                    {
                        String newLine = line.Substring(1);
                        words = newLine.Split(' ');
                    }
                    else
                    {
                        words = line.Split(' ');
                    }
                    location.xPoints.Add((ushort)int.Parse(words[0]));

                    location.yPoints.Add((ushort)int.Parse(words[1]));

                    //    Console.WriteLine("X is " + location.xPoints[i] + "Y is " + location.yPoints[i]);
                    i++;
                }

                ushort xSize = (ushort)location.xPoints.Count;
                ushort ySize = (ushort)location.yPoints.Count;
                Console.WriteLine("Rail is " + location.xPoints.Count + " waypoints long");
            }
            catch (IOException)
            {
                Console.WriteLine("Error Opening Rail..Exiting...");
                ErrorExit("Can't Open Rail!");
            }

            return location;





        }

        static Points loadShrineRail(int railNum, List<string> railNames)
        {
            Points location = new Points();
            string file = railNames[railNum];

            string[] temp = file.Split('.');
            string newFileName = temp[0] + " healing.txt";
            Console.WriteLine("Loading Rail: " + newFileName);

            var result = getRailHTTP(newFileName);
            string str = Encoding.UTF8.GetString(result);

            string[] lines = str.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine(lines[0]);

            try
            {

                var i = 0;
                foreach (string line in lines)
                {
                    string[] words;
                    if (!char.IsDigit(line[0]))
                    {
                        String newLine = line.Substring(1);
                        words = newLine.Split(' ');
                    }
                    else
                    {
                        words = line.Split(' ');
                    }
                    location.xPoints.Add((ushort)int.Parse(words[0]));

                    location.yPoints.Add((ushort)int.Parse(words[1]));

                    //    Console.WriteLine("X is " + location.xPoints[i] + "Y is " + location.yPoints[i]);
                    i++;
                }

                ushort xSize = (ushort)location.xPoints.Count;
                ushort ySize = (ushort)location.yPoints.Count;
                Console.WriteLine("Rail is " + location.xPoints.Count + " waypoints long");
            }
            catch (IOException)
            {
                Console.WriteLine("Error Opening Rail..Exiting...");
                ErrorExit("Can't Open Rail!");
            }

            return location;





        }

        static Boolean move(Points location, int tolerance, int waypoint)
        {
            StealthAPI.Stealth.Client.ClearBadLocationList();
            Console.WriteLine("Moving!");
            Boolean changedRailPos = false;
            if (location.xPoints.Count < 10 && !StealthAPI.Stealth.Client.Self.IsDead)
            {
                updateBookInfo();
                changedRailPos = true;
            }
            for (int i = waypoint; i < location.xPoints.Count; i++)
            {
                ushort x = location.xPoints[i];
                ushort y = location.yPoints[i];
                Console.WriteLine("Waypoint " + waypoint++ + " = " + x + " " + y);
                StealthAPI.Stealth.Client.newMoveXY(x, y, true, tolerance, true);
                WriteTxt(i.ToString(), railNames[currentRail - 1], StealthAPI.Stealth.Client.GetShardName());
                checkRez();
                checkMounted();
                if (StealthAPI.Stealth.Client.Self.IsDead && !StealthAPI.Stealth.Client.Self.IsInWarMode)
                {
                    StealthAPI.Stealth.Client.SetWarMode(true);
                }




            }





            return changedRailPos;
        }

        static void sendLocation(int waypoint)
        {
            string railName = "";
            if (railNames.Count > 0)
            {
                railName = railNames[currentRail - 1];
            }

            string sendStr = waypoint + "," + railName;
            string url = "http://www.lnrguild.com/idoc/files/" + StealthAPI.Stealth.Client.GetShardName() + ".txt";
            try
            {
                sendPost(url, sendStr);
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception has occurred while trying to download a rail!");
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine(e);
            }




        }

        static void sendPost(string url, string sendStr)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Get network credentials.
            request.Credentials =
                new NetworkCredential("railrunner", "railrunner123");

            // Write the text's bytes into the request stream.
            request.ContentLength = sendStr.Length;
            using (Stream request_stream = request.GetRequestStream())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(sendStr);
                request_stream.Write(bytes, 0, sendStr.Length);
                request_stream.Close();
            }
            Console.WriteLine(request.GetResponse());
        }



        static void updateBookInfo()
        {
            List<string> facets = new List<String>() { "codehelper", "malas", "tram", "fel" }; //codehelper makes so book numbers 1,2,3 line up
            int RENAME_BUTTON = 8;
            if (currentRail == 16) // switch books, reset rail position to 1
            {
                switch (curBook)
                {
                    case 1:
                        curBook = 2;
                        break;
                    case 2:
                        curBook = 3;
                        break;
                    case 3:
                        curBook = 1;
                        break;
                    default:
                        Console.WriteLine("Error-- Should not be here in updateBookInfo()");
                        break;
                }

                currentRail = 1;
                Gump InfoBookGump = OpenRunebook(runebooks[0]);

                if (InfoBookGump == null)
                {
                    InfoBookGump = OpenRunebook(runebooks[0]);
                }
                if (InfoBookGump == null)
                {
                    ErrorExit("Couldn't open information runebook.  Halting.");
                }

                List<ScriptSDK.Gumps.GumpButton> Buttons = InfoBookGump.Buttons;

                bool ClickResult = Buttons[RENAME_BUTTON].Click(); // CLICK THE RENAME BOOK BUTTON

                if (!ClickResult)
                {
                    ErrorExit("Clicking Failed, this is a serious error. Ask Zoo. In update book.");
                }

                System.Threading.Thread.Sleep(1000);
                String rename1 = facets[curBook] + " " + currentRail + " " + "info";
                StealthAPI.Stealth.Client.ConsoleEntryUnicodeReply(rename1);

                //update railNames and buttons for new book
                railNames = getRailNames(runebooks[curBook]);
                numRecallButtons = getButtons(InfoBookGump);

            }
            else
            {
                currentRail++;
                System.Threading.Thread.Sleep(1000);
                Gump InfoBookGump = OpenRunebook(runebooks[0]);

                System.Threading.Thread.Sleep(1000);

                if (InfoBookGump == null)
                {
                    InfoBookGump = OpenRunebook(runebooks[0]);
                }
                if (InfoBookGump == null)
                {
                    ErrorExit("Couldn't open information runebook.  Halting.");
                }

                List<ScriptSDK.Gumps.GumpButton> Buttons = InfoBookGump.Buttons;

                bool ClickResult = Buttons[RENAME_BUTTON].Click(); // CLICK THE RENAME BOOK BUTTON 

                if (!ClickResult)
                    ErrorExit("Clicking Failed, this is a serious error. Ask Zoo. In update book.");
            }

            String rename = facets[curBook] + " " + currentRail + " " + "info";
            StealthAPI.Stealth.Client.ConsoleEntryUnicodeReply(rename);

        }



        static ScriptSDK.Gumps.Gump OpenRunebook(ScriptSDK.Items.Item Runebook)
        {
            System.Threading.Thread.Sleep(1000);
            Runebook.DoubleClick();

            ScriptSDK.Gumps.Gump.WaitForGump(Runebook.Serial.Value, 2000);


            return ScriptSDK.Gumps.Gump.GetGump(89);
        }



        static PlayerMobile playerInfo()
        {
            PlayerMobile player = ScriptSDK.Mobiles.PlayerMobile.GetPlayer();
            player.UpdateLocalizedProperties();
            player.UpdateTextProperties();
            return player;

        }






        static void clear()
        {

            StealthAPI.Stealth.Client.ClearBadLocationList();
            StealthAPI.Stealth.Client.ClearBadObjectList();
            StealthAPI.Stealth.Client.ClearChatUserIgnore();
            StealthAPI.Stealth.Client.ClearContextMenu();
            StealthAPI.Stealth.Client.ClearJournal();
            StealthAPI.Stealth.Client.ClearJournalIgnore();
            StealthAPI.Stealth.Client.ClearShopList();
            StealthAPI.Stealth.Client.ClearSystemJournal();
            StealthAPI.Stealth.Client.IgnoreReset();

        }






        public static void saveCurrentDress()
        {
            equippedItems.Add(StealthAPI.Stealth.GetArmsLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetArmsLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetBraceLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetBraceLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetGlovesLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetGlovesLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetHatLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetHatLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetLegsLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetLegsLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetLhandLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetLhandLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetNeckLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetNeckLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetPantsLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetPantsLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetRingLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetRingLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetTalismanLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetTalismanLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetTorsoLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetTorsoLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetWaistLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetWaistLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetShoesLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetShoesLayer()));
            equippedItems.Add(StealthAPI.Stealth.GetRobeLayer(), StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetRobeLayer()));


        }

        public static void dress()
        {
            Stealth s = Stealth.Default;
            s.Unequip(StealthAPI.Stealth.GetRobeLayer());
            Thread.Sleep(1000);
            foreach (KeyValuePair<byte, uint> entry in equippedItems)
            {
                StealthAPI.Stealth.Client.Equip(entry.Key, entry.Value);
                Thread.Sleep(1000);
            }
        }

        public static void mount()
        {

            List<Item> mounts = ScriptSDK.Items.Item.Find(typeof(Mounts), StealthAPI.Stealth.Client.Self.BackpackId, true);
            if (mounts.Count == 0)
            {
                Console.WriteLine("Error! Mount not recognized-- tell Aaron");
            }
            else
            {
                mounts[0].DoubleClick();
            }
        }


        static void checkRez()
        {
            uint robe = StealthAPI.Stealth.Client.ObjAtLayer(StealthAPI.Stealth.GetRobeLayer());
            ushort color = StealthAPI.Stealth.Client.GetColor(robe);
            if (color == 2301 && !StealthAPI.Stealth.Client.Self.IsDead)
            {
                Thread.Sleep(1000);
                dress();
                mount();
                Thread.Sleep(2000);
            }

        }

        static void checkMounted()
        {
            if (StealthAPI.Stealth.Client.Self.PetsCurrent == 0 && !StealthAPI.Stealth.Client.Self.IsDead)
            {
                mount();
                Thread.Sleep(2000);
            }
        }

        static bool checkDistance()
        {
            Console.WriteLine("Checking if you can continue on your last rail");
            bool result = false;
            string file = ReadTxt(StealthAPI.Stealth.Client.GetShardName());
            string[] fileSplit = file.Split(',');
            int waypoint = (ushort)int.Parse(fileSplit[0]);
            railNames.Add(fileSplit[1]);



            Points locations = loadRail(0, railNames);
            int x = locations.xPoints[waypoint];
            int y = locations.yPoints[waypoint];
            ushort largest = getDistance(x, y);
            if (largest < 250 && fileSplit[2].Equals(StealthAPI.Stealth.Client.GetWorldNum().ToString()))
            {
                result = true;
                Console.WriteLine("You are close enough to continue on the last rail");
            }
            else
            {
                Console.WriteLine("To far away to continue last rail.");
                Console.WriteLine("Starting on a new rail.");
            }


            railNames.Clear();
            return result;
        }

        private static ushort getDistance(int xPos, int yPos)
        {

            var player = ScriptSDK.Mobiles.PlayerMobile.GetPlayer();

            player.UpdateLocalizedProperties();
            player.UpdateTextProperties();

            ushort dx = (ushort)Math.Abs(player.Location.X - xPos);

            ushort dy = (ushort)Math.Abs(player.Location.Y - yPos);



            return Math.Max(dx, dy);

        }



        static void consoleStartText()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("*****************************************************");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("***********");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" ZOO's STEALTH RAIL RUNNER ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("***************");
            Console.WriteLine("*                                                   *");
            Console.WriteLine("*                                                   *");
            Console.WriteLine("*  Make sure to run your house sign reader at the   *");
            Console.WriteLine("*                same time!                         *");
            //Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("*****************************************************");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("*****************************************************\n\n");
        }

        static void consoleErrorText(System.IO.FileNotFoundException e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("*****************************************************");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("I can't seem to connect to ScriptDotNet.DLL.");
            Console.WriteLine("is it in the same directory?");
            Console.WriteLine("Try copying ScriptDontNet.DLL to the same directory");
            Console.WriteLine("as the .exe and try again...");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("*****************************************************\n\n");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("The Exact Error was: {0} .", e);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Press any key to End Program.");
        }


        private static void WriteTxt(string waypoint, string railname, string shard)
        {
            string text = waypoint + "," + railname + "," + StealthAPI.Stealth.Client.GetWorldNum();
            // WriteAllText creates a file, writes the specified string to the file,
            // and then closes the file.    You do NOT need to call Flush() or Close().
            System.IO.File.WriteAllText(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + shard + ".txt", text);
            //Console.WriteLine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + shard + ".txt");
            //System.Threading.Thread.Sleep(3000);
        }

        private static string ReadTxt(string shard)
        {
            // Read the file as one string.
            string text = System.IO.File.ReadAllText(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + shard + ".txt");

            // Debug
            //System.Console.WriteLine("Contents of WriteText.txt = {0}", text);
            //System.Threading.Thread.Sleep(3000);
            return text;
        }




    }
}


