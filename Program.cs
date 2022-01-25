using System;
using System.Diagnostics;
using System.Threading;
using MemoryAccess;

class Program
{
    const int lvlAddress = 0x4AE300;
    const int timeAddr = 0x4AE33C;
    // 0: no game, 1: easy, 2: shirase, 3: master, 4: sakura, 5: qual, 6: doubles
    const int gameAddr = 0x4B3D5C;
    static readonly int[] staticCools = { 52, 52, 49, 45, 45, 42, 42, 38, 38 };
    static readonly int[] regrets = { 90, 75, 75, 68, 60, 60, 50, 50, 50, 50 };
    static int cools;
    static bool[] coolList;
    static bool[] regretList;
    static int prevCoolTime;
    // previous section completion time
    static int prevSectCompTime = 0;
    static Process p;
    static int state;
    static void Main(string[] args)
    {
        Console.CursorVisible = false;

        cools = 0;
        coolList = new bool[9];
        regretList = new bool[10];
        prevSectCompTime = 0;
        prevCoolTime = 999999;
        // Init
        while (true)
        {
            Process[] _p = Process.GetProcessesByName("game");
            if (_p.Length > 0)
            {
                p = _p[0];
                MemoryAccessAPI.OpenProcess(MemoryAccessAPI.ProcessAccessFlags.VirtualMemoryRead, false, _p[0].Id);
                break;
            }
            Console.WriteLine("TGM3 process (game.exe) not found.\nStart the game, then press any key to retry...");
            Console.ReadKey();
        }
        Console.Clear();
        while (true)
        {
            int prevLevel = GetLevel();
            int prevSection = (int)Math.Floor(prevLevel / 100d);
            Thread.Sleep(16);
            int level = GetLevel();
            int time = GetTimeInFrames();
            int section = (int)Math.Floor(level / 100d);


            if (time == 0)
            {
                if (state != 0)
                {
                    state = 0;
                    Console.Clear();
                    cools = 0;
                    coolList = new bool[9];
                    regretList = new bool[10];
                    prevCoolTime = 999999;
                    prevSectCompTime = 0;
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Waiting for a mode to be started...");
                continue;
            }

            if(GetGameType() != (int)GameType.Master)
            {
                if(state != 1)
                {
                    state = 1;
                    Console.Clear();
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine((GameType)GetGameType() + " mode!");
            }
            else if (level < 999)
            {
                if (state != 2)
                {
                    state = 2;
                    Console.Clear();
                }
                Console.SetCursorPosition(0, 0);
                if(level % 100 > 70 && prevLevel % 100 <= 70 && time - prevSectCompTime < staticCools[section] * 60 && time - prevSectCompTime < (prevCoolTime + 120) && section < 9)
                {
                    // cool!
                    prevCoolTime = time - prevSectCompTime;
                    coolList[section] = true;
                    cools += 1;
                }
                if(prevSection < section)
                {
                    if(time - prevSectCompTime > regrets[prevSection] * 60)
                    {
                        regretList[prevSection] = true;
                    }
                    prevSectCompTime = time;
                }
                Console.WriteLine(
                    $"000-100: {(coolList[0] ? "COOL" : "----")}  {(regretList[0] ? "REGRET" : "------")}\n" +
                    $"101-200: {(coolList[1] ? "COOL" : "----")}  {(regretList[1] ? "REGRET" : "------")}\n" +
                    $"201-300: {(coolList[2] ? "COOL" : "----")}  {(regretList[2] ? "REGRET" : "------")}\n" +
                    $"301-400: {(coolList[3] ? "COOL" : "----")}  {(regretList[3] ? "REGRET" : "------")}\n" +
                    $"401-500: {(coolList[4] ? "COOL" : "----")}  {(regretList[4] ? "REGRET" : "------")}\n" +
                    $"501-600: {(coolList[5] ? "COOL" : "----")}  {(regretList[5] ? "REGRET" : "------")}\n" +
                    $"601-700: {(coolList[6] ? "COOL" : "----")}  {(regretList[6] ? "REGRET" : "------")}\n" +
                    $"701-800: {(coolList[7] ? "COOL" : "----")}  {(regretList[7] ? "REGRET" : "------")}\n" +
                    $"801-900: {(coolList[8] ? "COOL" : "----")}  {(regretList[8] ? "REGRET" : "------")}\n" +
                    $"901-999: ----  {(regretList[9] ? "REGRET" : "------")}\n"
                    );
            }
            else
            {
                if (state != 3)
                {
                    state = 3;
                    Console.Clear();
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"{(cools>=9?"INVISIBLE ROLL!!":"Fading roll!")}\nYou can do it!!\n\nProgram developed by Not-A-Normal-Robot\nSpecial thanks to BttrDrgn for helping me out <3\nAlso special thanks to EderJCosta for his MemoryAccess repo");
            }
        }
    }

    static int GetLevel()
    {
        byte[] bytes = MemoryAccessAPI.GetBytes(p.Handle, (IntPtr)lvlAddress, 2);
        return bytes[0] + bytes[1] * 256;
    }

    static int GetTimeInFrames()
    {
        return MemoryAccessAPI.GetInteger(p.Handle, (IntPtr)timeAddr, 4);
    }

    static double GetTimeInSeconds()
    {
        return GetTimeInFrames() / 60d;
    }

    static int GetGameType()
    {
        return MemoryAccessAPI.GetInteger(p.Handle, (IntPtr)gameAddr, 4);
    }

    public enum GameType
    {
        None = 0,
        Easy = 1,
        Shirase = 2,
        Master = 3,
        Sakura = 4,
        Qualified_Master = 5,
        Doubles = 6
    }

    public enum States
    {
        Waiting = 0,
        NotMaster = 1,
        Master = 2,
        Mroll = 3
    }
}