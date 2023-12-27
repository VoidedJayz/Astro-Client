using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static AstroClient.Objects;

namespace AstroClient.Systems
{
    internal class ConsoleSystem
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        public static void AnimatedText(string input)
        {
            var chars = input.ToCharArray();
            foreach (var letter in chars)
            {
                Colorful.Console.Write(letter);
                Thread.Sleep(10);
            }
            Colorful.Console.WriteLine();
        }
        public static void SetColor(Color color)
        {
            Colorful.Console.ForegroundColor = color;
        }
        public static void CenterText(string text)
        {
            Console.WriteLine(text.PadLeft((Console.WindowWidth / 2) + (text.Length / 2)).PadRight(Console.WindowWidth));
        }

        public static async Task KeepWindowSize()
        {
            while (true)
            {
                Console.SetWindowSize(100, 30);
                Console.SetBufferSize(100, 30);
                await Task.Delay(50);
            }
        }

        public static void GenerateOption(MenuOption options)
        {
            // lol
            var Option = options.option ?? "";
            var identity = options.identity ?? "";
            var color = Color.BlueViolet;
            var matchMenu = options.matchMenu ?? true;
            var newLine = options.newLine ?? true;
            var warning = options.warning ?? "";
            var warningColor = Color.Gray;

            // set cursor to console center
            var originalConsoleColor = Colorful.Console.ForegroundColor;
            if (matchMenu == true)
            {
                Console.Write("║");
            }
            SetColor(color);
            Colorful.Console.Write(" [ ");
            SetColor(Color.LightGray);
            Colorful.Console.Write(identity);
            SetColor(color);
            Colorful.Console.Write(" ] ");
            SetColor(Color.LightGray);
            if (newLine == true)
            {
                if (matchMenu == true)
                {
                    var old = Console.CursorLeft;
                    Colorful.Console.SetCursorPosition(45, Console.CursorTop);
                    SetColor(Color.HotPink);
                    Colorful.Console.Write("║");
                    SetColor(Color.LightGray);
                    Colorful.Console.SetCursorPosition(old, Console.CursorTop);
                }
                Colorful.Console.Write(Option);
                SetColor(warningColor);
                Colorful.Console.Write(warning + "\n");
            }
            else
            {
                if (matchMenu == true)
                {
                    var old = Console.CursorLeft;
                    Colorful.Console.SetCursorPosition(45, Console.CursorTop);
                    SetColor(Color.HotPink);
                    Colorful.Console.Write("║");
                    SetColor(Color.LightGray);
                    Colorful.Console.SetCursorPosition(old, Console.CursorTop);
                }
                Colorful.Console.Write(Option);
                SetColor(warningColor);
                Colorful.Console.Write(warning);
            }
            SetColor(originalConsoleColor);
        }
        public static void AppArt()
        {
            Console.Clear();
            Colorful.Console.ReplaceAllColorsWithDefaults();
            Colorful.Console.WriteWithGradient(@"
               **      ******** ********** *******     *******         **      **  **** 
              ****    **////// /////**/// /**////**   **/////**       /**     /** */// *
             **//**  /**           /**    /**   /**  **     //**      /**     /**/    /*
            **  //** /*********    /**    /*******  /**      /**      //**    **    *** 
           **********////////**    /**    /**///**  /**      /**       //**  **    /// *
          /**//////**       /**    /**    /**  //** //**     **         //****    *   /*
          /**     /** ********     /**    /**   //** //*******           //**    / **** 
          //      // ////////      //     //     //   ///////             //      ////  
", Color.BlueViolet, Color.HotPink, 5);
        }
        public static void UpdateArt()
        {
            Console.Clear();
            Colorful.Console.ReplaceAllColorsWithDefaults();
            Colorful.Console.WriteWithGradient(@"
 **     ** *******  *******       **     ********** ******** *******  
/**    /**/**////**/**////**     ****   /////**/// /**///// /**////** 
/**    /**/**   /**/**    /**   **//**      /**    /**      /**   /** 
/**    /**/******* /**    /**  **  //**     /**    /******* /*******  
/**    /**/**////  /**    /** **********    /**    /**////  /**///**  
/**    /**/**      /**    ** /**//////**    /**    /**      /**  //** 
//******* /**      /*******  /**     /**    /**    /********/**   //**
 ///////  //       ///////   //      //     //     //////// //     // 
", Color.BlueViolet, Color.HotPink, 5);
        }
    }
}
