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

        public class BlockAndAdditionalInformation
        {
            public IMyTerminalBlock block { get; }

            public BlockAndAdditionalInformation(IMyTerminalBlock block)
            {
                this.block = block;
            }

            public bool IsSameBlock(IMyTerminalBlock block)
            {
                return object.ReferenceEquals(this.block, block);
            }
        }

        public class Destination : BlockAndAdditionalInformation
        {
            private int priority;

            public Destination(IMyTerminalBlock block, int priority) : base(block)
            {
                this.priority = priority;
            }

            public int Priority
            {
                set
                {
                    this.priority = value;
                }
                get
                {
                    return this.priority;
                }
            }
        }

        public class InvalidSpecifier : BlockAndAdditionalInformation
        {
            public List<string> specifiers { get; } = new List<string>();

            public InvalidSpecifier(IMyTerminalBlock block) : base(block)
            {
                
            }
        }

        //English message data
        static Dictionary<string, string> EnglishMessage = new Dictionary<string, string>()
        {
            {"languageInvalid", "Argument \"Lang\" is invalid value.\n Language of message is set to " + LanguageDefault + ".\n"},
            {"outputLCDName", "outputLCDName" },
            {"Language", "Language"},
            {"deliveredItems", "The following items are delivered to this container:\n"},
            {"priority", "Priority"},
            {"invalidID1", ":"},
            {"invalidID2", " is an invalid specifier.\n"},
            {"invalidPriority1", ": The priority of "},
            {"invalidPriority2", " is an invalid value.\n"},
            {"detectedInvalidSpecifier", "An invalid specifier or invalid priority was detected.\nThis program is not executed to avoid incorrect operation.\n"}
        };

        //Japanese message data
        static Dictionary<string, string> JapaneseMessage = new Dictionary<string, string>()
        {
            {"languageInvalid", "引数 \"Lang\" が無効な値です。メッセージの言語は " + LanguageDefault + " に設定されました。\n"},
            {"outputLCDName", "出力先LCDパネル" },
            {"Language", "言語"},
            {"deliveredItems", "このコンテナには以下のアイテムが配送されます。\n"},
            {"priority", "優先度"},
            {"invalidID1", " の "},
            {"invalidID2", " は無効な指定子です。\n"},
            {"invalidPriority1", " の "},
            {"invalidPriority2", " の優先度は無効な値です。\n"},
            {"detectedInvalidSpecifier", "無効な指定子あるいは優先度の値が検出されました。\n不正な動作を避けるため、プログラムは実行されません。\n"}
        };

        //Integrate messages written in each language.
        static Dictionary<string, Dictionary<string, string>> messages = new Dictionary<string, Dictionary<string, string>>()
        {
            { "english", EnglishMessage },
            { "japanese", JapaneseMessage }
        };

        static Dictionary<string, string> EnglishAmmoMagazineNames = new Dictionary<string, string>()
        {
            {"Missile200mm", "200mm missile container"},
            {"NATO_25x184mm", "25x184mm NATO ammo container"},
            {"NATO_5p56x45mm", "5.56x45mm NATO magazine"}
        };

        static Dictionary<string, string> EnglishComponentNames = new Dictionary<string, string>()
        {
            {"BulletproofGlass", "Bulletproof Glass"},
            {"Canvas", "Canvas"},
            {"Computer", "Computer"},
            {"Construction", "Construction Component"},
            {"Detector", "Detector Components"},
            {"Display", "Display"},
            {"Explosives", "Explosives"},
            {"Girder", "Girder"},
            {"GravityGenerator", "Gravity Generator Components"},
            {"InteriorPlate", "Interior Plate"},
            {"LargeTube", "Large Steel Tube"},
            {"Medical", "Medical Components"},
            {"MetalGrid", "Metal Grid"},
            {"Motor", "Motor"},
            {"PowerCell", "Power Cell"},
            {"RadioCommunication", "Radio-communication Components"},
            {"Reactor", "Reactor Components"},
            {"SmallTube", "Small Steel Tube"},
            {"SolarCell", "Solar Cell"},
            {"SteelPlate", "Steel Plate"},
            {"Superconductor", "Superconductor"},
            {"Thrust", "Thruster Components"}
        };

        static Dictionary<string, string> EnglishGasContainerObjectNames = new Dictionary<string, string>()
        {
            {"HydrogenBottle", "Hydrogen Bottle"}
        };

        static Dictionary<string, string> EnglishIngotNames = new Dictionary<string, string>()
        {
            {"Cobalt", "Cobalt Ingot"},
            {"Gold", "Gold Ingot"},
            {"Iron", "Iron Ingot"},
            {"Magnesium", "Magnesium Powder"},
            {"Nickel", "Nickel Ingot"},
            {"Platinum", "Platinum Ingot"},
            {"Scrap", "Old Scrap Metal"},
            {"Silicon", "Silicon Wafer"},
            {"Silver", "Silver Ingot"},
            {"Stone", "Gravel"},
            {"Uranium", "Uranium Ingot"}
        };

        static Dictionary<string, string> EnglishOreNames = new Dictionary<string, string>()
        {
            {"Cobalt", "Cobalt Ore"},
            {"Gold", "Gold Ore"},
            {"Ice", "Ice"},
            {"Iron", "Iron Ore"},
            {"Magnesium", "Magnesium Ore"},
            {"Nickel", "Nickel Ore"},
            {"Organic", "Organic"},
            {"Platinum", "Platinum Ore"},
            {"Scrap", "Scrap Metal"},
            {"Silicon", "Silicon Ore"},
            {"Silver", "Silver Ore"},
            {"Stone", "Stone"},
            {"Uranium", "Uranium Ore"}
        };

        static Dictionary<string, string> EnglishOxygenContainerObjectNames = new Dictionary<string, string>()
        {
            {"OxygenBottle", "Oxygen Bottle"}
        };

        static Dictionary<string, string> EnglishPhysicalGunObjectNames = new Dictionary<string, string>()
        {
            {"AngleGrinder2Item", "Enhanced Grinder"},
            {"AngleGrinder3Item", "Proficient Grinder"},
            {"AngleGrinder4Item", "Elite Grinder"},
            {"AngleGrinderItem", "Grinder"},
            {"AutomaticRifleItem", "Automatic Rifle"},
            {"HandDrill2Item", "Enhanced Hand Drill"},
            {"HandDrill3Item", "Proficient Hand Drill"},
            {"HandDrill4Item", "Elite Hand Drill"},
            {"HandDrillItem", "Hand Drill"},
            {"PreciseAutomaticRifleItem", "Precise Automatic Rifle"},
            {"RapidFireAutomaticRifleItem", "Rapid-Fire Automatic Rifle"},
            {"UltimateAutomaticRifleItem", "Elite Automatic Rifle"},
            {"WelderItem", "Enhanced Welder"},
            {"Welder2Item", "Proficient Welder"},
            {"Welder3Item", "Elite Welder"},
            {"Welder4Item", "Welder"}
        };

        static Dictionary<string, Dictionary<string, string>> EnglishItemNames = new Dictionary<string, Dictionary<string, string>>()
        {
            {"AmmoMagazine", EnglishAmmoMagazineNames},
            {"Component", EnglishComponentNames},
            {"GasContainerObject", EnglishGasContainerObjectNames},
            {"Ingot", EnglishIngotNames},
            {"Ore", EnglishOreNames},
            {"OxygenContainerObject", EnglishOxygenContainerObjectNames},
            {"PhysicalGunObject", EnglishPhysicalGunObjectNames}
        };

        static Dictionary<string, string> JapaneseAmmoMagazineNames = new Dictionary<string, string>()
        {
            {"Missile200mm", "200mmミサイル コンテナ"},
            {"NATO_25x184mm", "25x184mm NATO弾 コンテナ"},
            {"NATO_5p56x45mm", "5.56x45mm NATO弾 マガジン"}
        };

        static Dictionary<string, string> JapaneseComponentNames = new Dictionary<string, string>()
        {
            {"BulletproofGlass", "防弾ガラス"},
            {"Canvas", "キャンバス"},
            {"Computer", "コンピュータ"},
            {"Construction", "建築部品"},
            {"Detector", "検出器用部品"},
            {"Display", "ディスプレイ"},
            {"Explosives", "爆薬"},
            {"Girder", "鉄骨"},
            {"GravityGenerator", "重力発生装置用部品"},
            {"InteriorPlate", "内装用板"},
            {"LargeTube", "スチール管（大）"},
            {"Medical", "医療用部品"},
            {"MetalGrid", "メタルグリッド"},
            {"Motor", "モーター"},
            {"PowerCell", "電池"},
            {"RadioCommunication", "無線通信用部品"},
            {"Reactor", "リアクター用部品"},
            {"SmallTube", "スチール管（小）"},
            {"SolarCell", "太陽電池"},
            {"SteelPlate", "鋼板"},
            {"Superconductor", "超伝導体"},
            {"Thrust", "スラスター用部品"}
        };

        static Dictionary<string, string> JapaneseGasContainerObjectNames = new Dictionary<string, string>()
        {
            {"HydrogenBottle", "水素ボトル"}
        };

        static Dictionary<string, string> JapaneseIngotNames = new Dictionary<string, string>()
        {
            {"Cobalt", "コバルトインゴット"},
            {"Gold", "ゴールドインゴット"},
            {"Iron", "鉄インゴット"},
            {"Magnesium", "マグネシウム粉末"},
            {"Nickel", "ニッケルインゴット"},
            {"Platinum", "プラチナインゴット"},
            {"Scrap", "古びた金属くず"},
            {"Silicon", "シリコンウェハー"},
            {"Silver", "シルバーインゴット"},
            {"Stone", "砂利"},
            {"Uranium", "ウランインゴット"}
        };

        static Dictionary<string, string> JapaneseOreNames = new Dictionary<string, string>()
        {
            {"Cobalt", "コバルト鉱石"},
            {"Gold", "金鉱石"},
            {"Ice", "氷"},
            {"Iron", "鉄鉱石"},
            {"Magnesium", "マグネシウム鉱石"},
            {"Nickel", "ニッケル鉱石"},
            {"Organic", "有機物"},
            {"Platinum", "プラチナ鉱石"},
            {"Scrap", "金属くず"},
            {"Silicon", "シリコン鉱石"},
            {"Silver", "銀鉱石"},
            {"Stone", "石"},
            {"Uranium", "ウラン鉱石"}
        };

        static Dictionary<string, string> JapaneseOxygenContainerObjectNames = new Dictionary<string, string>()
        {
            {"OxygenBottle", "酸素ボトル"}
        };

        static Dictionary<string, string> JapanesePhysicalGunObjectNames = new Dictionary<string, string>()
        {
            {"AngleGrinder2Item", "強化グラインダー"},
            {"AngleGrinder3Item", "熟練グラインダー"},
            {"AngleGrinder4Item", "エリートグラインダー"},
            {"AngleGrinderItem", "グラインダー"},
            {"AutomaticRifleItem", "自動ライフル"},
            {"HandDrill2Item", "強化ハンドドリル"},
            {"HandDrill3Item", "熟練ハンドドリル"},
            {"HandDrill4Item", "エリートハンドドリル"},
            {"HandDrillItem", "ハンドドリル"},
            {"PreciseAutomaticRifleItem", "精密自動ライフル"},
            {"RapidFireAutomaticRifleItem", "速射自動ライフル"},
            {"UltimateAutomaticRifleItem", "エリート自動ライフル"},
            {"WelderItem", "強化ウェルダー"},
            {"Welder2Item", "熟練ウェルダー"},
            {"Welder3Item", "エリートウェルダー"},
            {"Welder4Item", "ウェルダー"}
        };

        static Dictionary<string, Dictionary<string, string>> JapaneseItemNames = new Dictionary<string, Dictionary<string, string>>()
        {
            {"AmmoMagazine", JapaneseAmmoMagazineNames},
            {"Component", JapaneseComponentNames},
            {"GasContainerObject", JapaneseGasContainerObjectNames},
            {"Ingot", JapaneseIngotNames},
            {"Ore", JapaneseOreNames},
            {"OxygenContainerObject", JapaneseOxygenContainerObjectNames},
            {"PhysicalGunObject", JapanesePhysicalGunObjectNames}
        };

        static Dictionary<string, Dictionary<string, Dictionary<string, string>>> ItemNames = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>()
        {
            {"english", EnglishItemNames},
            {"japanese", JapaneseItemNames}
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
            {"Missile200mm", @"^(?i)(200mm)?\s*missile\s*(container)?$|^(200mm)?[\s　]*ミサイル[\s　]*(コンテナ)?$|^(弾薬?|アモ|アミュニション|Ammo|Ammunition)$"},
            {"NATO_25x184mm", @"^(?i)(25x184(mm)?)?\s*(NATO)?\s*ammo\s*(container)?$|^25x184(mm)?\s*(NATO)?\s*(ammo)?\s*(container)?$|^(25x184(mm)?)?[\s　]*(NATO)?[\s　]*弾\s*コンテナ$|^(25x184(mm)?)?[\s　]*NATO[\s　]*弾?[\s　]*コンテナ$|^25x184(mm)?[\s　]*(NATO)?[\s　]*弾?[\s　]*(コンテナ)?$|^(弾薬?|アモ|アミュニション|Ammo|Ammunition)$"},
            {"NATO_5p56x45mm", @"^(?i)(5.56(x45(mm)?)?)?\s*(NATO)?\s*magazine$|^5.56(x45(mm)?)?\s*(NATO)?\s*(magazine)?$|^(5.56(x45(mm)?)?)?[\s　]*(NATO)?[\s　]*弾?[\s　]*マガジン$|^5.56(x45(mm)?)?[\s　]*(NATO)?[\s　]*弾?[\s　]*(マガジン)?$|^(弾薬?|アモ|アミュニション|Ammo|Ammunition)$"}
        };

        static Dictionary<string, string> ComponentPatterns = new Dictionary<string, string>()
        {
            {"BulletproofGlass", @"^(?i)(Bullet(proof)?)?\s*Glass$|^(防弾)?ガラス$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Canvas", @"^(?i)Canvas$|^キャンバス$"},
            {"Computer", @"^(?i)Computer$|^コンピュータ$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Construction", @"^(?i)Construction\s*(Comp(\.|onent)?)?$|^建築(部品)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Detector", @"^(?i)Detector\s*(Comp(\.|onents)?)?$|^検出(器用部品)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Display", @"^(?i)Display$|^ディスプレイ$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Explosives", @"^(?i)Explosives$|^爆薬$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Girder", @"^(?i)Girder$|^鉄骨$|^((建築)?資材|コンポーネント|Components?)$"},
            {"GravityGenerator", @"^(?i)Gravity\s*(Generator)?\s*(Comp(\.|onents)?)?$|^重力(発生装置用部品)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"InteriorPlate", @"^(?i)Interior\s*Plate$|^内装(用板)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"LargeTube", @"^(?i)Large\s*(Steel)?\s*Tube$|^(スチ(ール)?)?管[\s　]*[（(]?大[）)]?$|^スチ(ール)?管?[\s　]*[（(]?大[）)]?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Medical", @"^(?i)Medical\s*(Comp(\.|onents)?)?$|^医療(用部品)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"MetalGrid", @"^(?i)Metal\s*Grid$|^メタルグリッド$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Motor", @"^(?i)Motor$|^モーター$|^((建築)?資材|コンポーネント|Components?)$"},
            {"PowerCell", @"^(?i)Power\s*Cell$|^電池$|^((建築)?資材|コンポーネント|Components?)$"},
            {"RadioCommunication", @"^(?i)Radio(-comm(unication)?)?\s*(Comp(\.|onents)?)?$|^無線((通信用)?部品)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Reactor", @"^(?i)Reactor\s*(Comp(\.|onents)?)?$|^リアクター(用部品)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"SmallTube", @"^(?i)Small\s*(Steel)?\s*Tube$|^(スチ(ール)?)?管[\s　]*[（(]?小[）)]?$|^スチ(ール)?管?[\s　]*[（(]?小[）)]?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"SolarCell", @"^(?i)Solar\s*Cell$|^太陽(電池)?$|^((建築)?資材|コンポーネント|Components?)$"},
            {"SteelPlate", @"^(?i)Steel\s*Plate$|^鋼板$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Superconductor", @"^(?i)Superconductor$|^超伝導体$|^((建築)?資材|コンポーネント|Components?)$"},
            {"Thrust", @"^(?i)Thruster\s*(Comp(\.|onents)?)?$|^スラスター(用部品)?$|^((建築)?資材|コンポーネント|Components?)$"}
        };

        static Dictionary<string, string> GasContainerObjectPatterns = new Dictionary<string, string>()
        {
            {"HydrogenBottle", @"^(?i)Hydrogen\s*(Bottle)?$|^水素(ボトル)?$|^(ボトル|Bottles?)$"}
        };

        static Dictionary<string, string> IngotPatterns = new Dictionary<string, string>()
        {
            {"Cobalt", @"^(?i)(コバ(ルト)?|鈷|Co(balt)?)\s*(イン(ゴ(ット)?)?|塊|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Gold", @"^(?i)(金|ゴールド|Gold|Au)\s*(イン(ゴ(ット)?)?|塊|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Iron", @"^(?i)(鉄|アイアン|Iron|Fe)\s*(イン(ゴ(ット)?)?|塊|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Magnesium", @"^(?i)(マグネ(シウム)?|鎂|Magnesium|Mg)\s*(粉末?|イン(ゴ(ット)?)?|塊|Powder|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Nickel", @"^(?i)(ニッケル|鎳|Ni(ckel)?)\s*(イン(ゴ(ット)?)?|塊|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Platinum", @"^(?i)(プラチナ|白金|Platinum|Pt)\s*(イン(ゴ(ット)?)?|塊|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Scrap", @"^(?i)(古(びた)?|オールド|Old)\s*((金属)?くず|(スクラップ|Scrap)\s*(メタル|Metal)?)$"},
            {"Silicon", @"^(?i)(シリコン|Si(licon)?)\s*(ウ[ェエ](ハー|ファー)|イン(ゴ(ット)?)?|塊|Wafer|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Silver", @"^(?i)(銀|シルバー|Silver|Ag)\s*(イン(ゴ(ット)?)?|塊|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"},
            {"Stone", @"^(?i)Gravel$|^(砂利|グラベル)$"},
            {"Uranium", @"^(?i)(ウラ(ン|ニウム)|鈾|U(ranium))\s*(イン(ゴ(ット)?)?|塊|Ingot)$|^(イン(ゴ(ット)?)?|塊|Ingots?)$"}
        };

        static Dictionary<string, string> OrePatterns = new Dictionary<string, string>()
        {
            {"Cobalt", @"^(?i)(コバ(ルト)?|鈷|Co(balt)?)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Gold", @"^(?i)(金|ゴールド|Gold|Au)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Ice", @"^(?i)Ice$|^(氷|アイス)$"},
            {"Iron", @"^(?i)(鉄|アイアン|Iron|Fe)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Magnesium", @"^(?i)(マグネ(シウム)?|鎂|Magnesium|Mg)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Nickel", @"^(?i)(ニッケル|鎳|Ni(ckel)?)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Organic", @"^(?i)Organic$|^(有機物|オーガニック)$"},
            {"Platinum", @"^(?i)(プラチナ|白金|Platinum|Pt)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Scrap", @"^(?i)((金属)?くず|(スクラップ|Scrap)\s*(メタル|Metal)?)$"},
            {"Silicon", @"^(?i)(シリコン|ケイ素|珪素?|Si(licon)?)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Silver", @"^(?i)(銀|シルバー|Silver|Ag)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"},
            {"Stone", @"^(?i)Stone$|^(石|ストーン)$"},
            {"Uranium", @"^(?i)(ウラ(ン|ニウム)|鈾|U(ranium)?)\s*(鉱石?|オア|Ore)$|^(鉱石?|オア|Ores?)$"}
        };

        static Dictionary<string, string> OxygenContainerObjectPatterns = new Dictionary<string, string>()
        {
            {"OxygenBottle", @"^(?i)Oxygen\s*(Bottle)?$|^酸素(ボトル)?$|^(ボトル|Bottles?)$"}
        };

        static Dictionary<string, string> PhysicalGunObjectPatterns = new Dictionary<string, string>()
        {
            {"AngleGrinder2Item", @"^(?i)Enhanced\s*Grinder$|^(強化?|エン(ハンスド)?)(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$|^(道具|ツール|Tools?)$"},
            {"AngleGrinder3Item", @"^(?i)Proficient\s*Grinder$|^(熟練?|プロ(フィシ[ェエ]ント)?)(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))|^(道具|ツール|Tools?)$"},
            {"AngleGrinder4Item", @"^(?i)Elite\s*Grinder$|^エリ(ート)?(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$|^(道具|ツール|Tools?)$"},
            {"AngleGrinderItem", @"^(?i)Grinder$|^(グラ(インダー?)?|(電気?)?(ノコ(ギリ)?|鋸))$|^(道具|ツール|Tools?)$"},
            {"AutomaticRifleItem", @"^(?i)(Auto(matic)?)?\s*Rifle$|^(自動|オート(マ(チック|ティック)?)?)(ライ(フル)?|小?銃)$|^(ライ(フル)?|小?銃|Rifle|Guns?)$|^(武器|アームズ?|ウ[ェエ]ポン|Arms?|Weapons?)$"},
            {"HandDrill2Item", @"^(?i)Enhanced\s*(Hand)?\s*Drill$|^(強化?|エン(ハンスド)?)((ハンド)?ドリル?|掘(削機)?)$|^(道具|ツール|Tools?)$"},
            {"HandDrill3Item", @"^(?i)Proficient\s*(Hand)?\s*Drill$|^(熟練?|プロ(フィシ[ェエ]ント)?)((ハンド)?ドリル?|掘(削機)?)$|^(道具|ツール|Tools?)$"},
            {"HandDrill4Item", @"^(?i)Elite\s*(Hand)?\s*Drill$|^エリ(ート)?((ハンド)?ドリル?|掘(削機)?)$|^(道具|ツール|Tools?)$"},
            {"HandDrillItem", @"^(?i)(Hand)?\s*Drill$|^((ハンド)?ドリル?|掘(削機)?)$|^(道具|ツール|Tools?)$"},
            {"PreciseAutomaticRifleItem", @"^(?i)Precise\s*(Auto(matic)?)?\s*Rifle$|^(精密?|プリ(サイス)?)(自動|オート(マ(チック|ティック)?)?)?(ライ(フル)?|小?銃)$|^(ライ(フル)?|小?銃|Rifle|Guns?)$|^(武器|アームズ?|ウ[ェエ]ポン|Arms?|Weapons?)$"},
            {"RapidFireAutomaticRifleItem", @"^(?i)Rapid(-?Fire)?\s*(Auto(matic)?)?\s*Rifle$|^(速射?|ラピ(ッド(ファイア)?)?)(自動|オート(マ(チック|ティック)?)?)?(ライ(フル)?|小?銃)$|^(ライ(フル)?|小?銃|Rifle|Guns?)$|^(武器|アームズ?|ウ[ェエ]ポン|Arms?|Weapons?)$"},
            {"UltimateAutomaticRifleItem", @"^(?i)Elite\s*(Auto(matic)?)?\s*Rifle$|^エリ(ート)?(自動|オート(マ(チック|ティック)?)?)?(ライ(フル)?|小?銃)$|^(ライ(フル)?|小?銃|Rifle|Guns?)$|^(武器|アームズ?|ウ[ェエ]ポン|Arms?|Weapons?)$"},
            {"Welder2Item", @"^(?i)Enhanced\s*Welder$|^(強化?|エン(ハンスド)?)(ウェル(ダー?)?|溶(接機)?)$|^(道具|ツール|Tools?)$"},
            {"Welder3Item", @"^(?i)Proficient\s*Welder$|^(熟練?|プロ(フィシ[ェエ]ント)?)(ウェル(ダー?)?|溶(接機)?)$|^(道具|ツール|Tools?)$"},
            {"Welder4Item", @"^(?i)Elite\s*Welder$|^エリ(ート)?(ウェル(ダー?)?|溶(接機)?)$|^(道具|ツール|Tools?)$"},
            {"WelderItem", @"^(?i)Welder$|^(ウェル(ダー?)?|溶(接機)?)$|^(道具|ツール|Tools?)$"}
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
            TypeIDsKeys = new string[TypeIDs.Count];
            TypeIDs.Keys.CopyTo(TypeIDsKeys, 0);
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

                List<string> validSpecifiers = new List<string>();
                List<int> validPriorities = new List<int>();
                for(int j = 0; j < spIDs.Count; j++)
                {
                    bool spIDMatched = false;
                    for(int k = 0; k < TypeIDs.Count; k++)
                    {
                        for(int l = 0; l < TypeIDs[TypeIDsKeys[k]].Length; l++)
                        {
                            //use pattern matching to find out which item a specifier in custom data points to.
                            if (System.Text.RegularExpressions.Regex.IsMatch(spIDs[j], Patterns[TypeIDsKeys[k]][TypeIDs[TypeIDsKeys[k]][l]]))
                            {
                                //If settings for the item do not yet exist in the dictionary, add the item to the dictionary.
                                if (!Destinations.ContainsKey(TypeIDsKeys[k]))
                                {
                                    Destinations.Add(TypeIDsKeys[k], new Dictionary<string, List<Destination>>() { });
                                }
                                if (!Destinations[TypeIDsKeys[k]].ContainsKey(TypeIDs[TypeIDsKeys[k]][l]))
                                {
                                    Destinations[TypeIDsKeys[k]].Add(TypeIDs[TypeIDsKeys[k]][l], new List<Destination>() { });
                                }

                                //If you previously set the distribution priority for the same item in the same block, it will be overwritten. If not, set new priority.
                                Destination sameDestination;
                                if ((sameDestination = Destinations[TypeIDsKeys[k]][TypeIDs[TypeIDsKeys[k]][l]].Find(d => d.IsSameBlock(DestinationCandidate[i]))) == null)
                                {
                                    Destinations[TypeIDsKeys[k]][TypeIDs[TypeIDsKeys[k]][l]].Add(new Destination(DestinationCandidate[i], spPriorities[j]));
                                    validSpecifiers.Add(ItemNames[Language][TypeIDsKeys[k]][TypeIDs[TypeIDsKeys[k]][l]]);
                                    validPriorities.Add(spPriorities[j]);
                                }
                                else
                                {
                                    sameDestination.Priority = spPriorities[j];
                                    validPriorities[validSpecifiers.IndexOf(ItemNames[Language][TypeIDsKeys[k]][TypeIDs[TypeIDsKeys[k]][l]])] = spPriorities[j];
                                }
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

                if(validSpecifiers.Count > 0)
                {
                    string specifierOutput = "<excontmessage>\n\t" + messages[Language]["deliveredItems"];
                    for (int j = 0; j < validSpecifiers.Count; j++)
                    {
                        specifierOutput += "\t" + validSpecifiers[j] + "(" + messages[Language]["priority"] + ":" + validPriorities[j].ToString() + ")\n";
                    }
                    specifierOutput += "</excontmessage>";

                    System.Text.RegularExpressions.MatchCollection CustomDataPreserve = System.Text.RegularExpressions.Regex.Matches(DestinationCandidate[i].CustomData, "^.*(?=<excontmessage>)|(?<=</excontmessage>).*$");
                    DestinationCandidate[i].CustomData = "";
                    for(int j = 0; j < CustomDataPreserve.Count; j++)
                    {
                        DestinationCandidate[i].CustomData += CustomDataPreserve[j].ToString();
                    }
                    DestinationCandidate[i].CustomData += specifierOutput;
                }
            }

            if(invalidIDs.Count > 0)
            {
                for(int i = 0; i < invalidIDs.Count; i++)
                {
                    for(int j = 0; j < invalidIDs[i].specifiers.Count; j++)
                    {
                        output += invalidIDs[i].block.CustomName + messages[Language]["invalidID1"] + "\"" + invalidIDs[i].specifiers[j] + "\"" + messages[Language]["invalidID2"];
                    }
                }
            }
            if(invalidPriorities.Count > 0)
            {
                for(int i = 0; i < invalidPriorities.Count; i++)
                {
                    for(int j = 0; j < invalidPriorities[i].specifiers.Count; j++)
                    {
                        output += invalidIDs[i].block.CustomName + messages[Language]["invalidPriority1"] + "\"" + invalidIDs[i].specifiers[j] + "\"" + messages[Language]["invalidPriority2"];
                    }
                }
            }

            //If an invalid priority is detected, stop the actual processing and try to find out invalid priorities.
            if (detectedInvalidSpecifiers)
            {
                output += messages[Language]["detectedInvalidSpecifier"];
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
