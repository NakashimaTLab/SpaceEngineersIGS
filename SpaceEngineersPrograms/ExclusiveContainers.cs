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
    class ExclusiveContainers : MyGridProgram
    {
        //Please copy and paste from here to the mark when running this program in SpaceEngineers.
        const string LCDNameDefault = "LCD Panel EC Output";
        const string LanguageDefault = "english";

        class Destination
        {
            private IMyTerminalBlock block { get; }
            private int priority { get; }

            Destination(IMyTerminalBlock block, int priority)
            {
                this.block = block;
                this.priority = priority;
            }
        }

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

        static Dictionary<string, Dictionary<string, string>> messages = new Dictionary<string, Dictionary<string, string>>()
        {
            { "english", EnglishMessage },
            { "japanese", JapaneseMessage }
        };

        static Dictionary<string, List<string>> ItemKeywords = new Dictionary<string, List<string>>()
        {
            {"HandDrill", new List<string>(){@"(?i)hand\s*drill"} }
        };

        //dictionary of TypeIDs. TypeID is required to search for items in the block's inventory.
        static Dictionary<string, List<string>> TypeIDs = new Dictionary<string, List<string>>()
        {
            {"AmmoMagazine", new List<string>(){ "Missile200mm", "NATO_25x184mm", "NATO_5p56x45mm" } },
            {"Component", new List<string>(){ "BulletproofGlass", "Canvas", "Computer", "Construction", "Detector", "Display", "Explosives", "Girder", "GravityGenerator", "InteriorPlate", "LargeTube", "Medical", "MetalGrid", "Motor", "PowerCell", "RadioCommunication", "Reactor", "SmallTube", "SolarCell", "SteelPlate", "Superconductor", "Thrust"}},
            {"GasContainerObject", new List<string>(){ "HydrogenBottle" } },
            {"Ingot", new List<string>(){ "Cobalt", "Gold", "Iron", "Magnesium", "Nickel", "Platinum", "Scrap", "Silicon", "Silver", "Stone", "Uranium" } },
            {"Ore", new List<string>(){ "Cobalt", "Gold", "Iron", "Magnesium", "Nickel", "Platinum", "Scrap", "Silicon", "Silver", "Stone", "Uranium" } },
            {"OxygenContainerObject", new List<string>(){ "OxygenBottle" } },
            {"PhysicalGunObject", new List<string>(){ "AngleGrinder2Item", "AngleGrinder3Item", "AngleGrinder4Item", "AngleGrinderItem", "AutomaticRifleItem", "HandDrill2Item", "HandDrill3Item", "HandDrill4Item", "HandDrillItem", "PreciseAutomaticRifleItem", "RapidFireAutomaticRifleItem", "UltimateAutomaticRifleItem", "Welder2Item", "Welder3Item", "Welder4Item", "WelderItem" } }
        };

        public void Main(string argument, UpdateType updateSource)
        {
            Dictionary<string, List<Destination>> Destinations = new Dictionary<string, List<Destination>>();
            List<IMyTerminalBlock> Containers = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> BlockWeapons = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> BlockTools = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> Machines = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> Temp = new List<IMyTerminalBlock>();
            IMyTextPanel OutputLCD;
            string LCDName = LCDNameDefault;
            string Language = LanguageDefault;
            string output = "";
            string outputOnlyCustom = "";


            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Containers);

            GridTerminalSystem.GetBlocksOfType<IMyLargeGatlingTurret>(BlockWeapons);
            GridTerminalSystem.GetBlocksOfType<IMyLargeInteriorTurret>(Temp);
            BlockWeapons.AddRange(Temp);
            GridTerminalSystem.GetBlocksOfType<IMyLargeMissileTurret>(Temp);
            BlockWeapons.AddRange(Temp);
            GridTerminalSystem.GetBlocksOfType<IMySmallGatlingGun>(Temp);
            BlockWeapons.AddRange(Temp);
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(Temp);
            BlockWeapons.AddRange(Temp);
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncherReload>(Temp);
            BlockWeapons.AddRange(Temp);

            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(BlockTools);
            GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(Temp);
            BlockTools.AddRange(Temp);

            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Machines);
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Temp);
            Machines.AddRange(Temp);

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
                            if (System.Text.RegularExpressions.Regex.IsMatch(arg[1].Trim(), "^(?i)english$|^(?i)japanese$"))
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
