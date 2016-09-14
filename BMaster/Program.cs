using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace BMaster
{
    internal class Program
    {
        private static AIHeroClient user;

        private static Menu bmMenu, masteryEmoteMenu, generalMenu, laughMenu , spellsMenu, otherMenu;

        private static readonly Random delayRandom = new Random();

        private static int badgeAssists,laughAssists;

        private static List<string> spellNames = new List<string>();

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += LoadingOnOnLoadingComplete;
        }

        private static void LoadingOnOnLoadingComplete(EventArgs args)
        {
            // Assign values
            user = Player.Instance;
            badgeAssists = user.Assists;
            laughAssists = user.Assists;

            // Creating menu
            bmMenu = MainMenu.AddMenu("BMaster", "BMasterID", "BMaster - Tilt your enemies");

            bmMenu.AddGroupLabel("Have fun!");
            bmMenu.AddLabel("Be sure to give feedback on the forums!");

            generalMenu = bmMenu.AddSubMenu("General", "GeneralID", "General Menu");

            generalMenu.AddGroupLabel("Random Delay");
            generalMenu.AddLabel("Delay is randomly generated between these two values");
            generalMenu.AddLabel("Both sliders on 0 - No delay");
            generalMenu.Add("delay1", new Slider("From", 125, 0, 2000));
            generalMenu.Add("delay2", new Slider("To", 1000, 0, 2000));

            generalMenu.AddGroupLabel("Master Switch:");
            generalMenu.Add("useMastery", new CheckBox("Enable Mastery Emote"));
            generalMenu.Add("useLaugh", new CheckBox("Enable Laugh"));
            generalMenu.Add("useSpell", new CheckBox("Enable Spells"));
            generalMenu.Add("useOther", new CheckBox("Enable other"));
            

            masteryEmoteMenu = bmMenu.AddSubMenu("Mastery Badge","MasteryBadgeID","Mastery Badge settings");
            masteryEmoteMenu.AddGroupLabel("Use Mastery Emote:");
            masteryEmoteMenu.Add("emoteOnKill", new CheckBox("On kill", true));
            masteryEmoteMenu.Add("emoteOnDeath", new CheckBox("On death", false));
            masteryEmoteMenu.Add("emoteOnAssist", new CheckBox("On assist", true));
            masteryEmoteMenu.Add("emoteOnAce", new CheckBox("On ace", false));
            masteryEmoteMenu.Add("emoteNearDead", new CheckBox("Near dead enemy players", false));


            laughMenu = bmMenu.AddSubMenu("Laugh", "LaughID", "Laugh settings");
            laughMenu.AddGroupLabel("Use Laugh:");
            laughMenu.Add("laughOnKill", new CheckBox("On kill", true));
            laughMenu.Add("laughOnDeath", new CheckBox("On death", false));
            laughMenu.Add("laughOnAssist", new CheckBox("On assist", true));
            laughMenu.Add("laughOnAce", new CheckBox("On ace", false));

            spellsMenu = bmMenu.AddSubMenu("Spells", "SpellsID", "Spells settings");
            spellsMenu.AddGroupLabel("Spells:");
            DrawSpellMenu();

            otherMenu = bmMenu.AddSubMenu("Other", "OtherID", "Other settings");
            otherMenu.Add("pingOnAllyDeath", new CheckBox("Ping <?> on ally that dies on your screen", true));

            // Creating events
            Game.OnNotify += GameOnOnNotify;
            Game.OnUpdate += GameOnOnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Player.OnUpdateModel += delegate
            {
                Core.DelayAction(DrawSpellMenu, GenerateDelay());
            };
            Shop.OnBuyItem += delegate
            {
                Core.DelayAction(DrawSpellMenu, GenerateDelay());
            };

            Chat.Print("BMaster - Tilt your enemies: Successfully loaded!");
        }

        // Spell Menu
        private static void DrawSpellMenu()
        {
            foreach (var spell in Player.Spells)
            {
                
                if (spell != null && spell.Name != "Unknown" && !spellNames.Contains(spell.Name))
                {
                    spellsMenu.Add("BadgeOn" + spell.Name, new CheckBox("Mastery Badge on " + spell.Name, false));
                    spellsMenu.Add("LaughOn" + spell.Name, new CheckBox("Laugh on " + spell.Name, false));
                    spellNames.Add(spell.Name);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            // Emotes on spell casts
            if (sender.IsMe && generalMenu["useSpell"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var spell in Player.Spells)
                {
                    
                    if (spell != null && args != null && args.SData.Name.Equals(spell.SData.Name) && spellNames.Contains(spell.Name))
                    {
                        if (spellsMenu["BadgeOn" + spell.Name].Cast<CheckBox>().CurrentValue)
                        {
                            Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                        }
                        if (spellsMenu["LaughOn" + spell.Name].Cast<CheckBox>().CurrentValue)
                        {
                            Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
                        }
                        return;
                    }
                }
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            // Emotes near dead enemy heroes
            if (generalMenu["useMastery"].Cast<CheckBox>().CurrentValue && masteryEmoteMenu["emoteNearDead"].Cast<CheckBox>().CurrentValue)
            {
                var deadEnemy =
                EntityManager.Heroes.Enemies.OrderBy(a => a.Distance(user.Position))
                    .FirstOrDefault(b => b.Distance(user) <= 100 && b.IsDead);
                if (deadEnemy != null && masteryEmoteMenu["emoteNearDead"].Cast<CheckBox>().CurrentValue)
                {
                    Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                }
            }
        }

        private static void GameOnOnNotify(GameNotifyEventArgs args)
        {
            /*
             * 
             *  Mastery Emote
             * 
             */
            if (generalMenu["useMastery"].Cast<CheckBox>().CurrentValue)
            {
                // On champion kills
                if (masteryEmoteMenu["emoteOnKill"].Cast<CheckBox>().CurrentValue &&
                    args.EventId == GameEventId.OnChampionKill && args.NetworkId == user.NetworkId)
                {
                    Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                }

                // On death
                if (masteryEmoteMenu["emoteOnDeath"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnDie &&
                    args.NetworkId == user.NetworkId)
                {
                    Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                }

                // On assists
                if (masteryEmoteMenu["emoteOnAssist"].Cast<CheckBox>().CurrentValue &&
                    args.EventId == GameEventId.OnChampionKill &&
                    ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly && user.Assists > badgeAssists)
                {
                    badgeAssists++;
                    Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                }

                // On ace
                if (masteryEmoteMenu["emoteOnAce"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnAce &&
                    ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly)
                {
                    Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                }
            }
            /*
            * 
            *  Laugh
            * 
            */
            if (generalMenu["useLaugh"].Cast<CheckBox>().CurrentValue)
            {
                // On champion kills
                if (laughMenu["laughOnKill"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnChampionKill &&
                    args.NetworkId == user.NetworkId)
                {
                    Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
                }

                // On death
                if (laughMenu["laughOnDeath"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnDie &&
                    args.NetworkId == user.NetworkId)
                {
                    Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
                }

                // On assists
                if (laughMenu["laughOnAssist"].Cast<CheckBox>().CurrentValue &&
                    args.EventId == GameEventId.OnChampionKill &&
                    ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly && user.Assists > laughAssists)
                {
                    laughAssists++;
                    Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
                }

                // On ace
                if (laughMenu["laughOnAce"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnAce &&
                    ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly)
                {
                    Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
                }
            }
            /*
             * 
             * Ping
             * 
             */

            // On ally kill
            if (generalMenu["useOther"].Cast<CheckBox>().CurrentValue && otherMenu["pingOnAllyDeath"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnDie)
            {
                foreach (var ally in EntityManager.Heroes.Allies)
                {
                    if (ally.VisibleOnScreen && !ally.IsMe && args.NetworkId == ally.NetworkId)
                    {
                        Core.DelayAction(() => TacticalMap.SendPing(PingCategory.EnemyMissing, ally.Position), GenerateDelay());
                        return;
                    }
                }
            }
        }

        // Generate random delay
        private static int GenerateDelay()
        {
            if (generalMenu["delay1"].Cast<Slider>().CurrentValue < generalMenu["delay2"].Cast<Slider>().CurrentValue)
            {
                return delayRandom.Next(generalMenu["delay1"].Cast<Slider>().CurrentValue,
                    generalMenu["delay2"].Cast<Slider>().CurrentValue + 1);
            }
            return delayRandom.Next(generalMenu["delay2"].Cast<Slider>().CurrentValue,
                generalMenu["delay1"].Cast<Slider>().CurrentValue + 1);
        }
        
    }
}