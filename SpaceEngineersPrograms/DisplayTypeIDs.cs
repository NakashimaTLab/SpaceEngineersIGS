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
    class DisplayTypeIDs : MyGridProgram
    {
        //Please copy and paste from here to the mark when running this program in SpaceEngineers.
        //This program outputs TypeIDs of items contained in the specified container. 
        const string LCDNameDefault = "LCD Panel DT Output";
        const string LanguageDefault = "english";
        const string ContainerNameDefault = "Cargo Container DT";

        //English message data
        static Dictionary<string, string> EnglishMessage = new Dictionary<string, string>()
        {
            {"languageInvalid", "Argument \"Lang\" is invalid value.\n Language of message is set to " + LanguageDefault + ".\n"},
            {"outputLCDName", "outputLCDName" },
            {"containerName", "searchContainerName"},
            {"Language", "Language"},
            {"containerNotFound1", "A container named "},
            {"containerNotFound2", " was not found on the grid.\n"},
            {"containerEmpty", " is empty.\n"},
            {"Type", "\nTypes:\n"}
        };

        //Japanese message data
        static Dictionary<string, string> JapaneseMessage = new Dictionary<string, string>()
        {
            {"languageInvalid", "引数 \"Lang\" が無効な値です。メッセージの言語は " + LanguageDefault + " に設定されました。\n"},
            {"outputLCDName", "出力先LCDパネル" },
            {"containerName", "検索先コンテナ"},
            {"Language", "言語"},
            {"containerNotFound1", ""},
            {"containerNotFound2", " という名前のコンテナはグリッド上に見つかりませんでした。\n"},
            {"containerEmpty", " が空です。\n"},
            {"Type", "\nタイプ:\n"}
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
            string ContainerName = ContainerNameDefault;
            string output = "";
            string outputOnlyCustom = "";
            IMyTextPanel OutputLCD;
            List<IMyTerminalBlock> Containers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Containers);

            //parse arguments. You can set the name of the output LCD panel, the name of the container to search, and the language of the message.
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

                        case "ContainerName":
                            ContainerName = arg[1].Trim();
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
            outputOnlyCustom += messages[Language]["outputLCDName"] + ": " + LCDName + "\n" + messages[Language]["containerName"] + ": " + ContainerName + "\n" + messages[Language]["Language"] + ": " + Language + "\n";

            OutputLCD = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;

            IMyCargoContainer container = null;
            for(int i = 0; i < Containers.Count; i++)
            {
                if(Containers[i].CustomName == ContainerName)
                {
                    container = Containers[i] as IMyCargoContainer;
                    break;
                }
            }

            if(container == null)
            {
                output += messages[Language]["containerNotFound1"] + ContainerName + messages[Language]["containerNotFound2"];
                outputResult(OutputLCD, output, outputOnlyCustom);
                return;
            }

            IMyInventory containerInventory = container.GetInventory(0);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            containerInventory.GetItems(items);

            if(items.Count == 0)
            {
                output += container.CustomName + messages[Language]["containerEmpty"];
                outputResult(OutputLCD, output, outputOnlyCustom);
                return;
            }

            output += messages[Language]["Type"];
            items.Sort((a, b) => a.Type.ToString().CompareTo(b.Type.ToString()));
            for (int i = 0; i < items.Count; i++)
            {
                output += items[i].Type.ToString() + "\n";
            }

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
