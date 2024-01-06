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
            if (ConfigSystem.loadedConfig.animatedText)
            {
                var chars = input.ToCharArray();
                foreach (var letter in chars)
                {
                    Colorful.Console.Write(letter);
                    Task.Delay(1).Wait();
                }
                Colorful.Console.WriteLine();
            }
            else
            {
                Console.Write(input + "\n");
            }
        }
        public static void SetColor(Color color)
        {
            Colorful.Console.ForegroundColor = color;
        }
        public static void CenterText(string text)
        {
            Console.SetCursorPosition(27, Console.CursorTop);
            Console.WriteLine(text);
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

        public static void GenerateOption(MenuOption options, int pos = 27)
        {
            // variables
            var Option = options.option ?? "";
            var identity = options.identity ?? "";
            var color = Color.BlueViolet;
            var matchMenu = options.matchMenu ?? true;
            var newLine = options.newLine ?? true;
            var warning = options.warning ?? "";
            var warningColor = Color.Gray;


            var originalConsoleColor = Colorful.Console.ForegroundColor;
            if (matchMenu == true)
            {
                Console.SetCursorPosition(pos, Console.CursorTop);
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
                    Colorful.Console.SetCursorPosition(72, Console.CursorTop);
                    SetColor(Color.DeepPink);
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
                    Colorful.Console.SetCursorPosition(70, Console.CursorTop);
                    SetColor(Color.DeepPink);
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
", Color.BlueViolet, Color.DeepPink, 5);
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
", Color.BlueViolet, Color.DeepPink, 5);
        }
    }
}
