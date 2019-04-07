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

        static Dictionary<string, string> AmmoMagazineKeywords = new Dictionary<string, string>()
        {
            {"Missile200mm", @"^(?i)(200mm)?\s*missile\s*(container)?$|^(200mm)?[\s　]*ミサイル[\s　]*(コンテナ)?$"},
            {"NATO_25x184mm", @"^(?i)(25x184(mm)?)?\s*(NATO)?\s*ammo\s*(container)?$|^25x184(mm)?\s*(NATO)?\s*(ammo)?\s*(container)?$|^(25x184(mm)?)?[\s　]*(NATO)?[\s　]*弾\s*コンテナ$|^(25x184(mm)?)?[\s　]*NATO[\s　]*弾?[\s　]*コンテナ$|^25x184(mm)?[\s　]*(NATO)?[\s　]*弾?[\s　]*(コンテナ)?$"},
            {"NATO_5p56x45mm", @"^(?i)(5.56(x45(mm)?)?)?\s*(NATO)?\s*magazine$|^5.56(x45(mm)?)?\s*(NATO)?\s*(magazine)?$|^(5.56(x45(mm)?)?)?[\s　]*(NATO)?[\s　]*弾?[\s　]*マガジン$|^5.56(x45(mm)?)?[\s　]*(NATO)?[\s　]*弾?[\s　]*(マガジン)?$"}
        };

        static Dictionary<string, string> ComponentKeywords = new Dictionary<string, string>()
        {
            {"BulletproofGlass", @"^(?i)(Bullet(proof)?)?\s*Glass$|^(防弾)?ガラス$"},
            {"Canvas", @"^(?i)Canvas$|^キャンバス$"},
            {"Computer", @"^(?i)Computer$|^コンピュータ$"},
            {"Construction", @"^(?i)Construction\s*(Comp(\.|onent)?)?$|^建築(部品)?$"},
            {"Detector", @"^(?i)Detector\s*(Comp(\.|onents)?)?$|^検出(器用部品)?$"},
            {"Display", @"^(?i)Display$|^ディスプレイ$"},
            {"Explosives", @"^(?i)Explosives$|^爆薬$"},
            {"Girder", @"^(?i)Girder$|^鉄骨$"},
            {"GravityGenerator", @"^(?i)Gravity\s*(Generator)?\s*(Comp(\.|onents)?)?$|^重力(発生装置用部品)?$"},
            {"InteriorPlate", @"^(?i)Interior\s*Plate$|^内装(用板)?$"},
            {"LargeTube", @"^(?i)Large\s*(Steel)?\s*Tube$|^(スチ(ール)?)?管[\s　]*[（(]?大[）)]?$|^スチ(ール)?管?[\s　]*[（(]?大[）)]?$"},
            {"Medical", @"^(?i)Medical\s*(Comp(\.|onents)?)?$|^医療(用部品)?$"},
            {"MetalGrid", @"^(?i)Metal\s*Grid$|^メタルグリッド$"},
            {"Motor", @"^(?i)Motor$|^モーター$"},
            {"PowerCell", @"^(?i)Power\s*Cell$|^電池$"},
            {"RadioCommunication", @"^(?i)Radio(-comm(unication)?)?\s*(Comp(\.|onents)?)?$|^無線((通信用)?部品)?$"},
            {"Reactor", @"^(?i)Reactor\s*(Comp(\.|onents)?)?$|^リアクター(用部品)?$"},
            {"SmallTube", @"^(?i)Small\s*(Steel)?\s*Tube$|^(スチ(ール)?)?管[\s　]*[（(]?小[）)]?$|^スチ(ール)?管?[\s　]*[（(]?小[）)]?$"},
            {"SolarCell", @"^(?i)Solar\s*Cell$|^太陽(電池)?$"},
            {"SteelPlate", @"^(?i)Steel\s*Plate$|^鋼板$"},
            {"Superconductor", @"^(?i)Superconductor$|^超伝導体$"},
            {"Thrust", @"^(?i)Thruster\s*(Comp(\.|onents)?)?$|^スラスター(用部品)?$"}
        };

        static Dictionary<string, string> GasContainerObjectKeywords = new Dictionary<string, string>()
        {
            {"HydrogenBottle", @"^(?i)Hydrogen\s*(Bottle)?$|^水素(ボトル)?$"}
        };

        static Dictionary<string, string> IngotKeywords = new Dictionary<string, string>()
        {
            {"Cobalt", @"^(?i)Cobalt\s*Ingot$|^(コバ(ルト)?|鈷)(イン(ゴ(ット)?)?|塊)$"},
            {"Gold", @"^(?i)Gold\s*Ingot$|^(金|ゴールド)(イン(ゴ(ット)?)?|塊)$"},
            {"Iron", @"^(?i)Iron\s*Ingot$|^(鉄|アイアン)(イン(ゴ(ット)?)?|塊)$"},
            {"Magnesium", @"^(?i)Magnesium\s*(Powder|Ingot)$|^(マグネ(シウム)?|鎂)(粉末?|イン(ゴ(ット)?)?|塊)$"},
            {"Nickel", @"^(?i)Nickel\s*Ingot$|^(ニッケル|鎳)(イン(ゴ(ット)?)?|塊)$"},
            {"Platinum", @"^(?i)Platinum\s*Ingot$|^(プラチナ|白金)(イン(ゴ(ット)?)?|塊)$"},
            {"Scrap", @"^(?i)Old\s*Scrap\s*(Metal)?$|^(古(びた)?|オールド)(金属くず|スクラップ(メタル)?)$"},
            {"Silicon", @"^(?i)Silicon\s*(Wafer|Ingot)$|^シリコン(ウ[ェエ](ハー|ファー)|イン(ゴ(ット)?)?|塊)$"},
            {"Silver", @"^(?i)Silver\s*Ingot$|^(銀|シルバー)(イン(ゴ(ット)?)?|塊)$"},
            {"Stone", @"^(?i)Gravel$|^(砂利|グラベル)$"},
            {"Uranium", @"^(?i)Uranium\s*Ingot$|^(ウラ(ン|ニウム)|鈾)(イン(ゴ(ット)?)?|塊)$"}
        };

        static Dictionary<string, string> OreKeywords = new Dictionary<string, string>()
        {
            {"Cobalt", @"^(?i)Cobalt\s*Ore$|^(コバ(ルト)?|鈷)(鉱石?|オア)$"},
            {"Gold", @"^(?i)Gold\s*Ore$|^(金|ゴールド)(鉱石?|オア)$"},
            {"Ice", @"^(?i)Ice$|^(氷|アイス)$"},
            {"Iron", @"^(?i)Iron\s*Ore$|^(鉄|アイアン)(鉱石?|オア)$"},
            {"Magnesium", @"^(?i)Magnesium\s*Ore$|^(マグネ(シウム)?|鎂)(鉱石?|オア)$"},
            {"Nickel", @"^(?i)Nickel\s*Ore$|^(ニッケル|鎳)(鉱石?|オア)$"},
            {"Organic", @"^(?i)Organic$|^(有機物|オーガニック)$"},
            {"Platinum", @"^(?i)Platinum\s*Ore$|^(プラチナ|白金)(鉱石?|オア)$"},
            {"Scrap", @"^(?i)Scrap\s*(Metal)?$|^(金属くず|スクラップ(メタル)?)$"},
            {"Silicon", @"^(?i)Silicon\s*Ore$|^(シリコン|ケイ素|珪素?)(鉱石?|オア)$"},
            {"Silver", @"^(?i)Silver\s*Ore$|^(銀|シルバー)(鉱石?|オア)$"},
            {"Stone", @"^(?i)Stone$|^(石|ストーン)$"},
            {"Uranium", @"^(?i)Uranium Ore$|^(ウラ(ン|ニウム)|鈾)(鉱石?|オア)$"}
        };

        static Dictionary<string, string> OxygenContainerObjectKeywords = new Dictionary<string, string>()
        {
            {"OxygenBottle", @"^(?i)Oxygen\s*(Bottle)?$|^酸素(ボトル)?$"}
        };

        static Dictionary<string, string> PhysicalGunObjectKeywords = new Dictionary<string, string>()
        {
            {"AngleGrinder2Item", @"^(?i)Enhanced\s*Grinder$|^強化?(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AngleGrinder3Item", @"^(?i)Proficient\s*Grinder$|^熟練?(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AngleGrinder4Item", @"^(?i)Elite\s*Grinder$|^エリ(ート)?(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AngleGrinderItem", @"^(?i)Grinder$|^(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AutomaticRifleItem", @"^(?i)(Automatic)?\s*Rifle$|^(自動)?(ライフル|小?銃)$"},
            {"HandDrill2Item", @"^(?i)Enhanced\s*(Hand)?\s*Drill$|^強化?((ハンド)?ドリル?|掘(削機)?)$"},
            {"HandDrill3Item", @"^(?i)Proficient\s*(Hand)?\s*Drill$|^熟練?((ハンド)?ドリル?|掘(削機)?)$"},
            {"HandDrill4Item", @"^(?i)Elite\s*(Hand)?\s*Drill$|^エリ(ート)?((ハンド)?ドリル?|掘(削機)?)$"},
            {"HandDrillItem", @"^(?i)(Hand)?\s*Drill$|^((ハンド)?ドリル?|掘(削機)?)$"},
            {"PreciseAutomaticRifleItem", @"^(?i)Precise\s*(Automatic)?\s*Rifle$|^精密?(自動)?(ライフル|小?銃)$"},
            {"RapidFireAutomaticRifleItem", @"^(?i)Rapid(-?Fire)?\s*(Automatic)?\s*Rifle$|^速射?(自動)?(ライフル|小?銃)$"},
            {"UltimateAutomaticRifleItem", @"^(?i)Elite\s*(Automatic)?\s*Rifle$|^エリ(ート)?(自動)?(ライフル|小?銃)$"},
            {"Welder2Item", @"^(?i)Enhanced\s*Welder$|^強化?(ウェル(ダー?)?|溶接機)$"},
            {"Welder3Item", @"^(?i)Proficient\s*Welder$|^熟練?(ウェル(ダー?)?|溶接機)$"},
            {"Welder4Item", @"^(?i)Elite\s*Welder$|^エリ(ート)?(ウェル(ダー?)?|溶接機)$"},
            {"WelderItem", @"^(?i)Welder$|^(ウェル(ダー?)?|溶接機)$"}
        };

        //dictionary of TypeIDs. TypeID is required to search for items in the block's inventory.
        static Dictionary<string, List<string>> TypeIDs = new Dictionary<string, List<string>>()
        {
            {"AmmoMagazine", new List<string>(){ "Missile200mm", "NATO_25x184mm", "NATO_5p56x45mm" } },
            {"Component", new List<string>(){ "BulletproofGlass", "Canvas", "Computer", "Construction", "Detector", "Display", "Explosives", "Girder", "GravityGenerator", "InteriorPlate", "LargeTube", "Medical", "MetalGrid", "Motor", "PowerCell", "RadioCommunication", "Reactor", "SmallTube", "SolarCell", "SteelPlate", "Superconductor", "Thrust"}},
            {"GasContainerObject", new List<string>(){ "HydrogenBottle" } },
            {"Ingot", new List<string>(){ "Cobalt", "Gold", "Iron", "Magnesium", "Nickel", "Platinum", "Scrap", "Silicon", "Silver", "Stone", "Uranium" } },
            {"Ore", new List<string>(){ "Cobalt", "Gold", "Iron", "Magnesium", "Nickel", "Organic", "Platinum", "Scrap", "Silicon", "Silver", "Stone", "Uranium" } },
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
