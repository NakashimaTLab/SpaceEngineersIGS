using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Game.GameSystems.Conveyors;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SpaceEngineersPrograms
{
    class Template : MyGridProgram
    {
        //Please copy and paste from here to the mark when running this program in SpaceEngineers.
        const string LCDNameDefault = "LCD Panel Template Output";
        const string LanguageDefault = "english";

        //English message data
        static Dictionary<string, string> EnglishMessage = new Dictionary<string, string>()
        {
            {"languageInvalid", "Argument \"Lang\" is invalid value.\n Language of message is set to " + LanguageDefault + ".\n"},
            {"outputLCDName", "outputLCDName" },
            {"Language", "Language"}
        };

        //Japanese message data
        static Dictionary<string, string> JapaneseMessage = new Dictionary<string, string>()
        {
            {"languageInvalid", "引数 \"Lang\" が無効な値です。メッセージの言語は " + LanguageDefault + " に設定されました。\n"},
            {"outputLCDName", "出力先LCDパネル" },
            {"Language", "言語"}
        };

        //Integrate messages written in each language.
        static Dictionary<string, Dictionary<string, string>> messages = new Dictionary<string, Dictionary<string, string>>()
        {
            { "english", EnglishMessage },
            { "japanese", JapaneseMessage }
        };

        public void Main(string argument, UpdateType updateSource)
        {
            string LCDName = LCDNameDefault;
            string Language = LanguageDefault;
            string output = "";
            string outputOnlyCustom = "";
            IMyTextPanel OutputLCD;

            //parse arguments. You can set the name of the output LCD panel and the language of the message.
            //\s is whitespace characters. @ is a character that prevents the exception from occurring even if the string contains an escape sequence that is not usually recognized such as "\s".
            string[] arguments = System.Text.RegularExpressions.Regex.Split(argument, @"\s*,\s*");
            for (int i = 0; i < arguments.Length; i++)
            {
                string[] arg = System.Text.RegularExpressions.Regex.Split(arguments[i], @"\s*:\s*");
                if (arg.Length == 2)
                {
                    switch (arg[0].Trim())
                    {
                        case "LCDName":
                            LCDName = arg[1].Trim();
                            break;

                        case "Lang":
                            //Recognize upper case and lower case. and allow leading and trailing whitespace characters.
                            if (System.Text.RegularExpressions.Regex.IsMatch(arg[1].Trim(), "^(?i)english$|^japanese$"))
                            {
                                Language = arg[1].Trim().ToLower(); //Trim():remove whitespace. ToLower:make all characters lowercase.
                            }
                            else
                            {
                                output += messages[Language]["languageInvalid"];
                            }
                            break;
                    }
                }
            }

            //write information of output LCD Panel and language on customdata of Progammable Block that executes this program.
            outputOnlyCustom += messages[Language]["outputLCDName"] + ": " + LCDName + "\n" + messages[Language]["Language"] + ": " + Language + "\n";

            OutputLCD = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;

            //If there is a LCD Panel to output, this program will output the result there. Similar contents are written to custom data of programmable block.
            outputResult(OutputLCD, output, outputOnlyCustom);
        }

        //Prints the contents of the argument. The content of outputOnlyCustom is written only to custom data of programmable block that executes this program.
        public void outputResult(IMyTextPanel OutputLCD, string output, string outputOnlyCustom)
        {
            if (OutputLCD != null)
            {
                OutputLCD.WritePublicText(output);
            }
            Me.CustomData = outputOnlyCustom + output;
        }
        //Please copy and paste from the mark to here when running this program in SpaceEngineers.
    }
}
