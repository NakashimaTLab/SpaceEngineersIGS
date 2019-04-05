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
    class CodeEditorEmulator : MyGridProgram
    {
        #region CodeEditor
        //Please copy and paste from here to the mark when running this program in SpaceEngineers.
        const string LCDNameDefault = "LCD Panel RSP Output";
        const float rThresholdDefault = 1000.0f;
        const string LanguageDefault = "english";

        //English message data
        static Dictionary<string, string> EnglishMessage = new Dictionary<string, string>()
        {
            {"rThresholdInvalid", "Argument \"rThreshold\" is invalid value.\nThis program is not executed to avoid incorrect operation.\n"},
            {"languageInvalid", "Argument \"Lang\" is invalid value.\n Language of message is set to " + LanguageDefault + ".\n"},
            {"outputLCDName", "outputLCDName" },
            {"rThreshold", "reserving threshold"},
            {"Language", "Language"},
            {"noContainer", "There is no container on this grid.\n"},
            {"noRefinery", "There is no refinery on this grid.\n"},
            {"noWork", "This grid has no work to do with this program.\nPlace the container and refinery on the grid in a connected state.\n"},
            {"stoneLocated1", "Stone is located in "},
            {"stoneLocated2", ".\n"},
            {"transferred1", "Stone was transferred successfully from "},
            {"transferred2", ".\n"},
            {"notConnected1", "Stone was not transferred from "},
            {"notConnected2", "\nbecause that container is not connected to any refinery.\n"},
            {"notTransferred1", "Stone was not transferred from "},
            {"notTransferred2", " for some reason.\n"},
            {"noStoneOnGrid", "There are no stones to refine in containers of this grid.\n"},
            {"noTransportableStoneOnGrid", "There are no transportable stones on this grid.\n"}
        };

        //Japanese message data
        static Dictionary<string, string> JapaneseMessage = new Dictionary<string, string>()
        {
            {"rThresholdInvalid", "引数 \"rThreshold\" が無効な値です。不正な動作を避けるため、プログラムは実行されません。\n"},
            {"languageInvalid", "引数 \"Lang\" が無効な値です。メッセージの言語は " + LanguageDefault + " に設定されました。\n"},
            {"outputLCDName", "出力先LCDパネル" },
            {"rThreshold", "退避閾値"},
            {"Language", "言語"},
            {"noContainer", "このグリッドにコンテナがありません。\n"},
            {"noRefinery", "このグリッドにリファイナリーがありません。\n"},
            {"noWork", "このプログラムがすべきことがグリッド上にありません。\n相互に接続されたコンテナとリファイナリーをグリッド上に設置してください。\n"},
            {"stoneLocated1", ""},
            {"stoneLocated2", " に石があります。\n"},
            {"transferred1", "石が "},
            {"transferred2", " から搬出されました。\n"},
            {"notConnected1", "どのリファイナリーとも繋がっていなかったため、"},
            {"notConnected2", " から石は搬出されませんでした。\n"},
            {"notTransferred1", "何らかの理由によって、"},
            {"notTransferred2", " から石は搬出されませんでした。\n"},
            {"noStoneOnGrid", "グリッド上のコンテナに石がありません。\n"},
            {"noTransportableStoneOnGrid", "グリッド上に搬出可能な石がありません。\n"}
        };

        static Dictionary<string, Dictionary<string, string>> messages = new Dictionary<string, Dictionary<string, string>>()
        {
            { "english", EnglishMessage },
            { "japanese", JapaneseMessage }
        };

        public void Save()
        {
            // プログラムが状態を保存する必要がある時に呼び出されます。このメソッドを使用して、ストレージ・フィールドまたはその他の手段に状態を保存します。
            // 
            // このメソッドは省略可能であり、不要な場合は削除することが可能です。
        }

        public void Main(string argument, UpdateType updateSource)
        {
            List<IMyTerminalBlock> Containers = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> ShipDrills = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> Refineries = new List<IMyTerminalBlock>();
            IMyTextPanel OutputLCD;
            string LCDName = LCDNameDefault;
            float rThreshold = rThresholdDefault;
            string Language = LanguageDefault;
            string output = "";
            string outputOnlyCustom = "";
            bool stoneExistonGrid = false;
            bool Transportable = false;

            //Get lists of cargocontainers(and drills) and refineries. This program is terminated if there is not one of them.
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Containers);
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(ShipDrills);
            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Refineries);

            Containers.AddRange(ShipDrills);

            //parse arguments. You can set the name of the output LCD panel, the threshold for evacuating ores other than stone, and the language of the message.
            string[] arguments = argument.Split(',');
            for (int i = 0; i < arguments.Length; i++)
            {
                string[] arg = arguments[i].Split(':');
                if (arg.Length == 2)
                {
                    switch (arg[0])
                    {
                        case "LCDName":
                            LCDName = arg[1];
                            break;

                        case "rThreshold":
                            if(!float.TryParse(arg[1], out rThreshold))
                            {
                                output += messages[Language]["rThresholdInvalid"];
                                OutputLCD = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;
                                outputResult(OutputLCD, output, outputOnlyCustom);
                                return;
                            }
                            break;

                        case "Lang":
                            //Recognize upper case and lower case.
                            if (System.Text.RegularExpressions.Regex.IsMatch(arg[1], "^(?i)english$|^(?i)japanese$"))
                            {
                                Language = arg[1].ToLower();
                            }
                            else
                            {
                                output += messages[Language]["languageInvalid"];
                            }
                            break;
                    }
                }
            }

            //write information of output LCD Panel, rThreshold, and language on customdata of Progammable Block that executes this program.
            outputOnlyCustom += messages[Language]["outputLCDName"] + ": " + LCDName + "\n" + messages[Language]["rThreshold"] + ": " + rThreshold.ToString() + "\n" + messages[Language]["Language"] + ": " + Language + "\n";

            OutputLCD = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;

            if (Containers.Count == 0 || Refineries.Count == 0)
            {
                if (Containers.Count == 0)
                {
                    output += messages[Language]["noContainer"];
                }
                if (Refineries.Count == 0)
                {
                    output += messages[Language]["noRefinery"];
                }

                output += messages[Language]["noWork"];
                
                outputResult(OutputLCD, output, outputOnlyCustom);
                return;
            }

            //Each container try to move stones to refinery.
            for (int i = 0; i < Containers.Count; i++)
            {
                bool connectedToAnyRefinery = false;
                bool transferred = false;
                bool stoneContain = false;
                for(int j = 0; j < Refineries.Count; j++)
                {
                    //Get the contents of the container inventory, and if there is a stone, grasp its location
                    //If there is no stone, move on to the next container processing.
                    IMyInventory containerInventory = Containers[i].GetInventory(0);
                    int? itemIndex = FindItem(containerInventory, "Ore", "Stone");
                    if (!itemIndex.HasValue)
                    {
                        break;
                    }
                    else
                    {
                        output += messages[Language]["stoneLocated1"] + Containers[i].CustomName + messages[Language]["stoneLocated2"];
                        stoneExistonGrid = true;
                        stoneContain = true;
                    }

                    IMyInventory refineryInventory = Refineries[j].GetInventory(0);

                    //In order to prevent other ores from being evacuated unnecessarily, ores are not evacuated if the container and the destination refinery are not connected by a conveyor.
                    if (containerInventory.IsConnectedTo(refineryInventory))
                    {
                        reserveSpace(refineryInventory, Containers, rThreshold);
                        connectedToAnyRefinery = true;
                    }

                    if(containerInventory.TransferItemTo(refineryInventory, (int)itemIndex, targetItemIndex: 0))
                    {
                        transferred = true;
                        Transportable = true;
                    }
                }

                //display whether it succeeded to transfer stones with its cause.
                if (stoneContain)
                {
                    if (transferred)
                    {
                        output += messages[Language]["transferred1"] + Containers[i].CustomName + messages[Language]["transferred2"];
                    }
                    else if (!connectedToAnyRefinery)
                    {
                        output += messages[Language]["notConnected1"] + Containers[i].CustomName + messages[Language]["notConnected2"];
                    }
                    else
                    {
                        output += messages[Language]["notTransferred1"] + Containers[i].CustomName + messages[Language]["notTransferred2"];
                    }
                }
            }

            //If there is no stone in the container in the grid or no stone that can be transferred, this is indicated.
            if (!stoneExistonGrid)
            {
                output += messages[Language]["noStoneOnGrid"];
            }
            else if (!Transportable)
            {
                output += messages[Language]["noTransportableStoneOnGrid"];
            }

            //If there is a LCD Panel to output, this program will output the result there. Similar contents are written to custom data of programmable block.
            outputResult(OutputLCD, output, outputOnlyCustom);
        }

        //Identifies the location of items of the specified type and subtype from the inventory.
        public int? FindItem(IMyInventory inventory, string type, string subtype)
        {
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inventory.GetItems(items);
            for(int i = 0; i < items.Count; i++)
            {
                if(items[i].Type.ToString().Contains(type) && items[i].Type.ToString().Contains(subtype))
                {
                    return i;
                }
            }
            return null;
        }

        //When ores other than stones are in the refinery above the threshold, they are transferred to other containers to make room for the stones.
        public void reserveSpace(IMyInventory inventory, List<IMyTerminalBlock> dstContainers, float rThreshold)
        {
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inventory.GetItems(items);
            for(int i = 0; i < items.Count; i++)
            {
                if(items[i].Type.ToString().Contains("Ore") && System.Text.RegularExpressions.Regex.IsMatch(items[i].Type.ToString(), "Iron|Nickel|Cobalt|Magnesium|Silicon|Silver|Gold|Platinum|Uranium"))
                {
                    for(int j = 0; j < dstContainers.Count; j++)
                    {
                        if (items[i].Amount <= (VRage.MyFixedPoint)rThreshold) break;

                        IMyInventory containerInventory = dstContainers[j].GetInventory(0);
                        inventory.TransferItemTo(containerInventory, i, amount: (items[i].Amount - (VRage.MyFixedPoint)rThreshold));
                    }
                }
            }
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
        #endregion
    }
}
