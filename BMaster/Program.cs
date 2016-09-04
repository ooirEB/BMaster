using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace BMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += LoadingOnOnLoadingComplete;
        }

        private static AIHeroClient user;

        private static Menu bmMenu;

        private static Random delayRandom = new Random();

        private static int assists;

        private static void LoadingOnOnLoadingComplete(EventArgs args)
        {

            // Assign values
            user = Player.Instance;
            assists = user.Assists;

            // Creating menu
            bmMenu = MainMenu.AddMenu("BMaster", "BMasterID", "BMaster - Tilt your enemies");

            bmMenu.AddGroupLabel("Mastery Emote");
            bmMenu.Add("badgeOnKill", new CheckBox("Mastery Emote on kill", true));
            bmMenu.Add("badgeOnDeath", new CheckBox("Mastery Emote on death", false));
            bmMenu.Add("badgeOnAssist", new CheckBox("Mastery Emote on assist", true));
            bmMenu.Add("badgeOnAce", new CheckBox("Mastery Emote on ace", false));
            bmMenu.Add("badgeNearDead", new CheckBox("Mastery Emote near dead enemy players", false));

            //Spell Menu
            bmMenu.AddGroupLabel("Mastery Emote on spells:");
            foreach (var spell in Player.Spells)
            {
                if (spell.Name != "Unknown")
                {
                    bmMenu.Add("badgeOn" + spell.Name, new CheckBox("On " + spell.Name, false));
                }
            }

            bmMenu.AddGroupLabel("Laugh");
            bmMenu.Add("laughOnKill", new CheckBox("Laugh on kill", true));
            bmMenu.Add("laughOnAce", new CheckBox("Laugh on ace", true));

            bmMenu.AddGroupLabel("Random Delay");
            bmMenu.AddLabel("Delay is randomly generated between these two values");
            bmMenu.AddLabel("Both sliders on 0 - No delay");
            bmMenu.Add("delay1", new Slider("From", 125, 0, 1000));
            bmMenu.Add("delay2", new Slider("To", 1000, 0, 1000));


            // Creating events
            Game.OnNotify += GameOnOnNotify;
            Game.OnUpdate += GameOnOnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Chat.Print("BMaster - Tilt your enemies: Successfully loaded!");
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            // Mastery Emotes on spell casts
            if (sender.IsMe)
            {
                foreach (var spell in Player.Spells)
                {
                    if (spell.Slot == args.Slot && bmMenu["badgeOn" + spell.Name].Cast<CheckBox>().CurrentValue)
                    {
                        Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                        return;
                    }
                }
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            // Mastery Emotes near dead enemy heroes
            var deadEnemy =EntityManager.Heroes.Enemies.OrderBy(a => a.Distance(user.Position)).FirstOrDefault(b => b.Distance(user) <= 100 && b.IsDead);
            if (deadEnemy != null && bmMenu["badgeNearDead"].Cast<CheckBox>().CurrentValue)
            {
                Chat.Say("/masterybadge");
            }
        }

        private static void GameOnOnNotify(GameNotifyEventArgs args)
        {
            // Casting Mastery Emotes
            if (bmMenu["badgeOnKill"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnChampionKill && args.NetworkId == user.NetworkId)
            {
                Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
                
            }

            if (bmMenu["badgeOnDeath"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnDie && args.NetworkId == user.NetworkId)
            {
                Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
            }

            if (bmMenu["badgeOnAssist"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnChampionKill && ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly && user.Assists > assists)
            {
                assists++;
                Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
            }

            if (bmMenu["badgeOnAce"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnAce && ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly)
            {
                Core.DelayAction(() => Chat.Say("/masterybadge"), GenerateDelay());
            }

            // Casting laugh

            if (bmMenu["laughOnKill"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnChampionKill && args.NetworkId == user.NetworkId)
            {
                Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
            }

            if (bmMenu["laughOnAce"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnAce && ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly)
            {
                Core.DelayAction(() => Player.DoEmote(Emote.Laugh), GenerateDelay());
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
            else
            {
                return delayRandom.Next(bmMenu["delay2"].Cast<Slider>().CurrentValue,
                   bmMenu["delay1"].Cast<Slider>().CurrentValue + 1);
            }
        }
    }
}
