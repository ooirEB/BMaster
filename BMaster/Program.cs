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

        private static Menu bmMenu;

        private static readonly Random delayRandom = new Random();

        private static int assists;

        private static List<string> spellNames = new List<string>();

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += LoadingOnOnLoadingComplete;
        }

        private static void LoadingOnOnLoadingComplete(EventArgs args)
        {
            // Assign values
            user = Player.Instance;
            assists = user.Assists;

            // Creating menu
            bmMenu = MainMenu.AddMenu("BMaster", "BMasterID", "BMaster - Tilt your enemies");

            bmMenu.Add("mode", new ComboBox("Mode:", 0, "None", "Laugh", "Mastery Badge", "Both"));

            bmMenu.AddGroupLabel("Random Delay");
            bmMenu.AddLabel("Delay is randomly generated between these two values");
            bmMenu.AddLabel("Both sliders on 0 - No delay");
            bmMenu.Add("delay1", new Slider("From", 125, 0, 1000));
            bmMenu.Add("delay2", new Slider("To", 1000, 0, 2000));

            bmMenu.AddGroupLabel("Other stuff:");
            bmMenu.Add("pingOnAllyDeath", new CheckBox("Ping <?> on ally that dies near you", true));

            bmMenu.AddGroupLabel("Emote:");
            bmMenu.Add("emoteOnKill", new CheckBox("On kill", true));
            bmMenu.Add("emoteOnDeath", new CheckBox("On death", false));
            bmMenu.Add("emoteOnAssist", new CheckBox("On assist", true));
            bmMenu.Add("emoteOnAce", new CheckBox("On ace", false));
            bmMenu.Add("emoteNearDead", new CheckBox("Near dead enemy players", false));

            bmMenu.AddGroupLabel("Emote on spells:");
            DrawSpellMenu();

            // Creating events
            Game.OnNotify += GameOnOnNotify;
            Game.OnUpdate += GameOnOnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Chat.Print("BMaster - Tilt your enemies: Successfully loaded!");
        }

        // Spell Menu
        private static void DrawSpellMenu()
        {
            foreach (var spell in Player.Spells)
            {
                if (spell.Name != "Unknown" && !spellNames.Contains(spell.Name))
                {
                    bmMenu.Add("emoteOn" + spell.Name, new CheckBox("On " + spell.Name, false));
                    spellNames.Add(spell.Name);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            // Emotes on spell casts
            if (sender.IsMe)
            {
                foreach (var spell in Player.Spells)
                {
                    if (spell != null && spell.Slot == args.Slot && bmMenu["emoteOn" + spell.Name].Cast<CheckBox>().CurrentValue)
                    {
                        Core.DelayAction(DoEmotes, GenerateDelay());
                        return;
                    }
                }
                Core.DelayAction(DrawSpellMenu, GenerateDelay());
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            // Emotes near dead enemy heroes
            var deadEnemy =
                EntityManager.Heroes.Enemies.OrderBy(a => a.Distance(user.Position))
                    .FirstOrDefault(b => b.Distance(user) <= 100 && b.IsDead);
            if (deadEnemy != null && bmMenu["emoteNearDead"].Cast<CheckBox>().CurrentValue)
            {
                Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
            }

            
        }

        private static void GameOnOnNotify(GameNotifyEventArgs args)
        {
            // On champion kills
            if (bmMenu["emoteOnKill"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnChampionKill && args.NetworkId == user.NetworkId)
            {
                Core.DelayAction(DoEmotes, GenerateDelay());
                return;
            }

            // On death
            if (bmMenu["emoteOnDeath"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnDie && args.NetworkId == user.NetworkId)
            {
                Core.DelayAction(DoEmotes, GenerateDelay());
                return;
            }

            // On assists
            if (bmMenu["emoteOnAssist"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnChampionKill &&
                ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly && user.Assists > assists)
            {
                assists++;
                Core.DelayAction(DoEmotes, GenerateDelay());
                return;
            }

            // On ace
            if (bmMenu["emoteOnAce"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnAce &&
                ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly)
            {
                Core.DelayAction(DoEmotes, GenerateDelay());
                return;
            }

            // On ally kill
            if (bmMenu["pingOnAllyDeath"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnDie)
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
            if (bmMenu["delay1"].Cast<Slider>().CurrentValue < bmMenu["delay2"].Cast<Slider>().CurrentValue)
            {
                return delayRandom.Next(bmMenu["delay1"].Cast<Slider>().CurrentValue,
                    bmMenu["delay2"].Cast<Slider>().CurrentValue + 1);
            }
            return delayRandom.Next(bmMenu["delay2"].Cast<Slider>().CurrentValue,
                bmMenu["delay1"].Cast<Slider>().CurrentValue + 1);
        }

        // Cast selected emotes
        private static void DoEmotes()
        {
            switch (bmMenu["mode"].Cast<ComboBox>().SelectedText)
            {
                case "None":
                    break;
                case "Both":
                    Chat.Say("/masterybadge");
                    Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
                    break;
                case "Laugh":
                    Player.DoEmote(Emote.Laugh);
                    break;
                case "Mastery Badge":
                    Chat.Say("/masterybadge");
                    break;
            }
        }
    }
}