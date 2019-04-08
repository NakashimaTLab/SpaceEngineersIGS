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

        public class Destination
        {
            public IMyTerminalBlock block { get; }
            public int priority { get; }

            public Destination(IMyTerminalBlock block, int priority)
            {
                this.block = block;
                this.priority = priority;
            }
        }

        public class InvalidSpecifier
        {
            public IMyTerminalBlock block { get; }
            public List<string> specifiers { get; }

            public InvalidSpecifier(IMyTerminalBlock block)
            {
                this.block = block;
                this.specifiers = new List<string>();
            }

            public bool IsSameBlock(IMyTerminalBlock block)
            {
                return object.ReferenceEquals(this.block, block);
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

        //Integrate messages written in each language.
        static Dictionary<string, Dictionary<string, string>> messages = new Dictionary<string, Dictionary<string, string>>()
        {
            { "english", EnglishMessage },
            { "japanese", JapaneseMessage }
        };

        static Dictionary<string, string> EnglishAmmoMagazineName = new Dictionary<string, string>()
        {
            {"Missile200mm", ""},
            {"NATO_25x184mm", ""},
            {"NATO_5p56x45mm", ""}
        };

        static Dictionary<string, string> EnglishComponentName = new Dictionary<string, string>()
        {
            {"BulletproofGlass", ""},
            {"Canvas", ""},
            {"Computer", ""},
            {"Construction", ""},
            {"Detector", ""},
            {"Display", ""},
            {"Explosives", ""},
            {"Girder", ""},
            {"GravityGenerator", ""},
            {"InteriorPlate", ""},
            {"LargeTube", ""},
            {"Medical", ""},
            {"MetalGrid", ""},
            {"Motor", ""},
            {"PowerCell", ""},
            {"RadioCommunication", ""},
            {"Reactor", ""},
            {"SmallTube", ""},
            {"SolarCell", ""},
            {"SteelPlate", ""},
            {"Superconductor", ""},
            {"Thrust", ""}
        };

        static Dictionary<string, string> EnglishGasContainerObjectName = new Dictionary<string, string>()
        {
            {"HydrogenBottle", ""}
        };

        static Dictionary<string, string> EnglishIngotName = new Dictionary<string, string>()
        {
            {"Cobalt", ""},
            {"Gold", ""},
            {"Iron", ""},
            {"Magnesium", ""},
            {"Nickel", ""},
            {"Platinum", ""},
            {"Scrap", ""},
            {"Silicon", ""},
            {"Silver", ""},
            {"Stone", ""},
            {"Uranium", ""}
        };

        static Dictionary<string, string> EnglishOreName = new Dictionary<string, string>()
        {
            {"Cobalt", ""},
            {"Gold", ""},
            {"Ice", ""},
            {"Iron", ""},
            {"Magnesium", ""},
            {"Nickel", ""},
            {"Organic", ""},
            {"Platinum", ""},
            {"Scrap", ""},
            {"Silicon", ""},
            {"Silver", ""},
            {"Stone", ""},
            {"Uranium", ""}
        };

        static Dictionary<string, string> EnglishOxygenContainerObjectName = new Dictionary<string, string>()
        {
            {"OxygenBottle", ""}
        };

        static Dictionary<string, string> EnglishPhysicalGunObjectName = new Dictionary<string, string>()
        {
            {"AngleGrinder2Item", ""},
            {"AngleGrinder3Item", ""},
            {"AngleGrinder4Item", ""},
            {"AngleGrinderItem", ""},
            {"AutomaticRifleItem", ""},
            {"HandDrill2Item", ""},
            {"HandDrill3Item", ""},
            {"HandDrill4Item", ""},
            {"HandDrillItem", ""},
            {"PreciseAutomaticRifleItem", ""},
            {"RapidFireAutomaticRifleItem", ""},
            {"RapidFireAutomaticRifleItem", ""},
            {"WelderItem", ""},
            {"WelderItem", ""},
            {"WelderItem", ""},
            {"WelderItem", ""}
        };

        //dictionary of TypeIDs. TypeID is required to search for items in the block's inventory.
        static Dictionary<string, string[]> TypeIDs = new Dictionary<string, string[]>()
        {
            {"AmmoMagazine", new string[] { "Missile200mm", "NATO_25x184mm", "NATO_5p56x45mm" } },
            {"Component", new string[] { "BulletproofGlass", "Canvas", "Computer", "Construction", "Detector", "Display", "Explosives", "Girder", "GravityGenerator", "InteriorPlate", "LargeTube", "Medical", "MetalGrid", "Motor", "PowerCell", "RadioCommunication", "Reactor", "SmallTube", "SolarCell", "SteelPlate", "Superconductor", "Thrust"}},
            {"GasContainerObject", new string[] { "HydrogenBottle" } },
            {"Ingot", new string[] { "Cobalt", "Gold", "Iron", "Magnesium", "Nickel", "Platinum", "Scrap", "Silicon", "Silver", "Stone", "Uranium" } },
            {"Ore", new string[] { "Cobalt", "Gold", "Ice", "Iron", "Magnesium", "Nickel", "Organic", "Platinum", "Scrap", "Silicon", "Silver", "Stone", "Uranium" } },
            {"OxygenContainerObject", new string[] { "OxygenBottle" } },
            {"PhysicalGunObject", new string[] { "AngleGrinder2Item", "AngleGrinder3Item", "AngleGrinder4Item", "AngleGrinderItem", "AutomaticRifleItem", "HandDrill2Item", "HandDrill3Item", "HandDrill4Item", "HandDrillItem", "PreciseAutomaticRifleItem", "RapidFireAutomaticRifleItem", "UltimateAutomaticRifleItem", "Welder2Item", "Welder3Item", "Welder4Item", "WelderItem" } }
        };

        //The following dictionaries are regular expression patterns for finding the container to which items should be distributed.
        static Dictionary<string, string> AmmoMagazinePatterns = new Dictionary<string, string>()
        {
            {"Missile200mm", @"^(?i)(200mm)?\s*missile\s*(container)?$|^(200mm)?[\s　]*ミサイル[\s　]*(コンテナ)?$"},
            {"NATO_25x184mm", @"^(?i)(25x184(mm)?)?\s*(NATO)?\s*ammo\s*(container)?$|^25x184(mm)?\s*(NATO)?\s*(ammo)?\s*(container)?$|^(25x184(mm)?)?[\s　]*(NATO)?[\s　]*弾\s*コンテナ$|^(25x184(mm)?)?[\s　]*NATO[\s　]*弾?[\s　]*コンテナ$|^25x184(mm)?[\s　]*(NATO)?[\s　]*弾?[\s　]*(コンテナ)?$"},
            {"NATO_5p56x45mm", @"^(?i)(5.56(x45(mm)?)?)?\s*(NATO)?\s*magazine$|^5.56(x45(mm)?)?\s*(NATO)?\s*(magazine)?$|^(5.56(x45(mm)?)?)?[\s　]*(NATO)?[\s　]*弾?[\s　]*マガジン$|^5.56(x45(mm)?)?[\s　]*(NATO)?[\s　]*弾?[\s　]*(マガジン)?$"}
        };

        static Dictionary<string, string> ComponentPatterns = new Dictionary<string, string>()
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

        static Dictionary<string, string> GasContainerObjectPatterns = new Dictionary<string, string>()
        {
            {"HydrogenBottle", @"^(?i)Hydrogen\s*(Bottle)?$|^水素(ボトル)?$"}
        };

        static Dictionary<string, string> IngotPatterns = new Dictionary<string, string>()
        {
            {"Cobalt", @"^(?i)(コバ(ルト)?|鈷|Co(balt)?)\s*(イン(ゴ(ット)?)?|塊|Ingot)$"},
            {"Gold", @"^(?i)(金|ゴールド|Gold|Au)\s*(イン(ゴ(ット)?)?|塊|Ingot)$"},
            {"Iron", @"^(?i)(鉄|アイアン|Iron|Fe)\s*(イン(ゴ(ット)?)?|塊|Ingot)$"},
            {"Magnesium", @"^(?i)(マグネ(シウム)?|鎂|Magnesium|Mg)\s*(粉末?|イン(ゴ(ット)?)?|塊|Powder|Ingot)$"},
            {"Nickel", @"^(?i)(ニッケル|鎳|Ni(ckel)?)\s*(イン(ゴ(ット)?)?|塊|Ingot)$"},
            {"Platinum", @"^(?i)(プラチナ|白金|Platinum|Pt)\s*(イン(ゴ(ット)?)?|塊|Ingot)$"},
            {"Scrap", @"^(?i)(古(びた)?|オールド|Old)\s*((金属)?くず|(スクラップ|Scrap)\s*(メタル|Metal)?)$"},
            {"Silicon", @"^(?i)(シリコン|Si(licon)?)\s*(ウ[ェエ](ハー|ファー)|イン(ゴ(ット)?)?|塊|Wafer|Ingot)$"},
            {"Silver", @"^(?i)(銀|シルバー|Silver|Ag)\s*(イン(ゴ(ット)?)?|塊|Ingot)$"},
            {"Stone", @"^(?i)Gravel$|^(砂利|グラベル)$"},
            {"Uranium", @"^(?i)(ウラ(ン|ニウム)|鈾|U(ranium))\s*(イン(ゴ(ット)?)?|塊|Ingot)$"}
        };

        static Dictionary<string, string> OrePatterns = new Dictionary<string, string>()
        {
            {"Cobalt", @"^(?i)(コバ(ルト)?|鈷|Co(balt)?)\s*(鉱石?|オア|Ore)$"},
            {"Gold", @"^(?i)(金|ゴールド|Gold|Au)\s*(鉱石?|オア|Ore)$"},
            {"Ice", @"^(?i)Ice$|^(氷|アイス)$"},
            {"Iron", @"^(?i)(鉄|アイアン|Iron|Fe)\s*(鉱石?|オア|Ore)$"},
            {"Magnesium", @"^(?i)(マグネ(シウム)?|鎂|Magnesium|Mg)\s*(鉱石?|オア|Ore)$"},
            {"Nickel", @"^(?i)(ニッケル|鎳|Ni(ckel)?)\s*(鉱石?|オア|Ore)$"},
            {"Organic", @"^(?i)Organic$|^(有機物|オーガニック)$"},
            {"Platinum", @"^(?i)(プラチナ|白金|Platinum|Pt)\s*(鉱石?|オア|Ore)$"},
            {"Scrap", @"^(?i)((金属)?くず|(スクラップ|Scrap)\s*(メタル|Metal)?)$"},
            {"Silicon", @"^(?i)(シリコン|ケイ素|珪素?|Si(licon)?)\s*(鉱石?|オア|Ore)$"},
            {"Silver", @"^(?i)(銀|シルバー|Silver|Ag)\s*(鉱石?|オア|Ore)$"},
            {"Stone", @"^(?i)Stone$|^(石|ストーン)$"},
            {"Uranium", @"^(?i)(ウラ(ン|ニウム)|鈾|U(ranium)?)\s*(鉱石?|オア|Ore)$"}
        };

        static Dictionary<string, string> OxygenContainerObjectPatterns = new Dictionary<string, string>()
        {
            {"OxygenBottle", @"^(?i)Oxygen\s*(Bottle)?$|^酸素(ボトル)?$"}
        };

        static Dictionary<string, string> PhysicalGunObjectPatterns = new Dictionary<string, string>()
        {
            {"AngleGrinder2Item", @"^(?i)Enhanced\s*Grinder$|^(強化?|エン(ハンスド)?)(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AngleGrinder3Item", @"^(?i)Proficient\s*Grinder$|^(熟練?|プロ(フィシェント)?)(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AngleGrinder4Item", @"^(?i)Elite\s*Grinder$|^エリ(ート)?(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AngleGrinderItem", @"^(?i)Grinder$|^(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$"},
            {"AutomaticRifleItem", @"^(?i)(Auto(matic)?)?\s*Rifle$|^(自動|オート(マ(チック|ティック)?)?)?(ライ(フル)?|小?銃)$"},
            {"HandDrill2Item", @"^(?i)Enhanced\s*(Hand)?\s*Drill$|^(強化?|エン(ハンスド)?)((ハンド)?ドリル?|掘(削機)?)$"},
            {"HandDrill3Item", @"^(?i)Proficient\s*(Hand)?\s*Drill$|^(熟練?|プロ(フィシェント)?)((ハンド)?ドリル?|掘(削機)?)$"},
            {"HandDrill4Item", @"^(?i)Elite\s*(Hand)?\s*Drill$|^エリ(ート)?((ハンド)?ドリル?|掘(削機)?)$"},
            {"HandDrillItem", @"^(?i)(Hand)?\s*Drill$|^((ハンド)?ドリル?|掘(削機)?)$"},
            {"PreciseAutomaticRifleItem", @"^(?i)Precise\s*(Auto(matic)?)?\s*Rifle$|^(精密?|プリ(サイス)?)(自動|オート(マ(チック|ティック)?)?)?(ライ(フル)?|小?銃)$"},
            {"RapidFireAutomaticRifleItem", @"^(?i)Rapid(-?Fire)?\s*(Auto(matic)?)?\s*Rifle$|^(速射?|ラピ(ッド(ファイア)?)?)(自動|オート(マ(チック|ティック)?)?)?(ライ(フル)?|小?銃)$"},
            {"UltimateAutomaticRifleItem", @"^(?i)Elite\s*(Auto(matic)?)?\s*Rifle$|^エリ(ート)?(自動|オート(マ(チック|ティック)?)?)?(ライ(フル)?|小?銃)$"},
            {"Welder2Item", @"^(?i)Enhanced\s*Welder$|^(強化?|エン(ハンスド)?)(ウェル(ダー?)?|溶(接機)?)$"},
            {"Welder3Item", @"^(?i)Proficient\s*Welder$|^(熟練?|プロ(フィシェント)?)(ウェル(ダー?)?|溶(接機)?)$"},
            {"Welder4Item", @"^(?i)Elite\s*Welder$|^エリ(ート)?(ウェル(ダー?)?|溶(接機)?)$"},
            {"WelderItem", @"^(?i)Welder$|^(ウェル(ダー?)?|溶(接機)?)$"}
        };

        //Combine regular expression patterns of each TypeID.
        static Dictionary<string, Dictionary<string, string>> Patterns = new Dictionary<string, Dictionary<string, string>>()
        {
            {"AmmoMagazine", AmmoMagazinePatterns},
            {"Component", ComponentPatterns},
            {"GasContainerObject", GasContainerObjectPatterns},
            {"Ingot", IngotPatterns},
            {"Ore", OrePatterns},
            {"OxygenContainerObject", OxygenContainerObjectPatterns},
            {"PhysicalGunObject", PhysicalGunObjectPatterns}
        };

        public void Main(string argument, UpdateType updateSource)
        {
            Dictionary<string, Dictionary<string, List<Destination>>> Destinations = new Dictionary<string, Dictionary<string, List<Destination>>>();
            List<IMyTerminalBlock> ListDestinationCandidate = new List<IMyTerminalBlock>();
            IMyTerminalBlock[] DestinationCandidate;
            List<IMyTerminalBlock> Containers = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> BlockWeapons = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> LargeGatlingTurrets = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> LargeInteriorTurrets = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> LargeMissileTurrets = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> SmallGatlingGuns = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> SmallMissileLaunchers = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> SmallMissileLauncherReloads = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> BlockTools = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> ShipDrills = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> ShipGrinders = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> Machines = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> Refineries = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> Assemblers = new List<IMyTerminalBlock>();
            IMyTextPanel OutputLCD;
            string LCDName = LCDNameDefault;
            string Language = LanguageDefault;
            string output = "";
            string outputOnlyCustom = "";
            bool detectedInvalidSpecifiers = false;
            List<InvalidSpecifier> invalidPriorities = new List<InvalidSpecifier>();
            List<InvalidSpecifier> invalidIDs = new List<InvalidSpecifier>();

            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Containers);

            GridTerminalSystem.GetBlocksOfType<IMyLargeGatlingTurret>(LargeGatlingTurrets);
            GridTerminalSystem.GetBlocksOfType<IMyLargeInteriorTurret>(LargeInteriorTurrets);
            GridTerminalSystem.GetBlocksOfType<IMyLargeMissileTurret>(LargeMissileTurrets);
            GridTerminalSystem.GetBlocksOfType<IMySmallGatlingGun>(SmallGatlingGuns);
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(SmallMissileLaunchers);
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncherReload>(SmallMissileLauncherReloads);
            BlockWeapons.AddRange(LargeGatlingTurrets);
            BlockWeapons.AddRange(LargeInteriorTurrets);
            BlockWeapons.AddRange(LargeMissileTurrets);
            BlockWeapons.AddRange(SmallGatlingGuns);
            BlockWeapons.AddRange(SmallMissileLaunchers);
            BlockWeapons.AddRange(SmallMissileLauncherReloads);

            ListDestinationCandidate.AddRange(Containers);
            ListDestinationCandidate.AddRange(BlockWeapons);
            DestinationCandidate = ListDestinationCandidate.ToArray();

            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(ShipDrills);
            GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(ShipGrinders);
            BlockTools.AddRange(ShipDrills);
            BlockTools.AddRange(ShipGrinders);

            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Refineries);
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Assemblers);
            Machines.AddRange(Refineries);
            Machines.AddRange(Assemblers);

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

            string[] TypeIDsKeys;
            for (int i = 0; i < DestinationCandidate.Length; i++)
            {
                //Cut out the bracketed part of the custom data using positive look-ahead and positive look-behind.
                string SpecifierOfCustomData = System.Text.RegularExpressions.Regex.Match(DestinationCandidate[i].CustomData, "(?<=<excont>).*?(?=</excont>)").ToString();

                //If the specifier is not described, the next processing is performed.
                if (SpecifierOfCustomData.Trim() == "") continue;

                List<string> Specifiers = SpecifierOfCustomData.Split(',').ToList();
                List<string> spIDs = new List<string>();
                List<int> spPriorities = new List<int>();
                for(int j = 0; j < Specifiers.Count; j++)
                {
                    //If nothing is written between commas, the next processing is performed.
                    if (Specifiers[j].Trim() == "") continue;
                    string[] IDandPri = Specifiers[j].Split(':');
                    spIDs.Add(IDandPri[0].Trim());
                    spPriorities.Add(0);
                    if (IDandPri.Length >= 2)
                    {
                        int priTemp;
                        if (int.TryParse(IDandPri[1].Trim(), out priTemp))
                        {
                            spPriorities[j] = priTemp;
                        }
                        else
                        {
                            //If an invalid priority is detected, make a note of the block and specifier.
                            AddInvalidSpecifier(invalidPriorities, DestinationCandidate[i], IDandPri[0]);
                            detectedInvalidSpecifiers = true;
                        }
                    }
                }

                TypeIDsKeys = new string[TypeIDs.Count];
                TypeIDs.Keys.CopyTo(TypeIDsKeys, 0);
                for(int j = 0; j < spIDs.Count; j++)
                {
                    bool spIDMatched = false;
                    for(int k = 0; k < TypeIDs.Count; k++)
                    {
                        for(int l = 0; l < TypeIDs[TypeIDsKeys[k]].Length; l++)
                        {
                            if(System.Text.RegularExpressions.Regex.IsMatch(spIDs[j], Patterns[TypeIDsKeys[k]][TypeIDs[TypeIDsKeys[k]][l]]))
                            {
                                if (!Destinations.ContainsKey(TypeIDsKeys[k]))
                                {
                                    Destinations.Add(TypeIDsKeys[k], new Dictionary<string, List<Destination>>() { });
                                }
                                if (!Destinations[TypeIDsKeys[k]].ContainsKey(TypeIDs[TypeIDsKeys[k]][l]))
                                {
                                    Destinations[TypeIDsKeys[k]].Add(TypeIDs[TypeIDsKeys[k]][l], new List<Destination>() { });
                                }
                                Destinations[TypeIDsKeys[k]][TypeIDs[TypeIDsKeys[k]][l]].Add(new Destination(DestinationCandidate[i], spPriorities[j]));
                                spIDMatched = true;
                            }
                        }
                    }
                    if (!spIDMatched)
                    {
                        AddInvalidSpecifier(invalidIDs, DestinationCandidate[i], spIDs[j]);
                        detectedInvalidSpecifiers = true;
                    }
                }
            }

            //If an invalid priority is detected, stop the actual processing and try to find out invalid priorities.
            if (detectedInvalidSpecifiers)
            {
                outputResult(OutputLCD, output, outputOnlyCustom);
                return;
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

        public void AddInvalidSpecifier(List<InvalidSpecifier> list, IMyTerminalBlock block, string specifier)
        {
            if (list.Count > 0)
            {
                if (list[list.Count - 1].IsSameBlock(block))
                {
                    list[list.Count - 1].specifiers.Add(specifier);
                }
                else
                {
                    list.Add(new InvalidSpecifier(block));
                    list[list.Count - 1].specifiers.Add(specifier);
                }
            }
            else
            {
                list.Add(new InvalidSpecifier(block));
                list[list.Count - 1].specifiers.Add(specifier);
            }
        }
        //Please copy and paste from the mark to here when running this program in SpaceEngineers.
    }
}
