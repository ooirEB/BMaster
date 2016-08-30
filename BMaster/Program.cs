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

        private static AIHeroClient user = Player.Instance;

        private static Menu bmMenu;

        private static Random delayRandom = new Random();

        private static int assists;

        private static void LoadingOnOnLoadingComplete(EventArgs args)
        {
            // Creating menu
            bmMenu = MainMenu.AddMenu("BMaster", "BMasterID", "BMaster - Tilt your enemies");

            bmMenu.AddGroupLabel("Mastery Emote");
            bmMenu.Add("badgeOnKill", new CheckBox("Mastery Emote on kill", true));
            bmMenu.Add("badgeonDeath", new CheckBox("Mastery Emote on death", false));
            bmMenu.Add("badgeOnAssist", new CheckBox("Mastery Emote on assist", true));
            bmMenu.Add("badgeOnAce", new CheckBox("Mastery Emote on ace", false));

            bmMenu.AddGroupLabel("Laugh");
            bmMenu.Add("laughOnKill", new CheckBox("Laugh on kill", true));
            bmMenu.Add("laughOnAce", new CheckBox("Laugh on ace", true));

            bmMenu.AddGroupLabel("Random Delay");
            bmMenu.AddLabel("Delay is randomly generated between these two values");
            bmMenu.AddLabel("Both sliders on 0 - No delay");
            bmMenu.Add("delay1", new Slider("From", 125, 0, 1000));
            bmMenu.Add("delay2", new Slider("To", 1000, 0, 1000));

            // Assign values
            assists = user.Assists;

            // Creating events
            Game.OnNotify += GameOnOnNotify;

            Chat.Print("BMaster - Tilt your enemies: Successfully loaded!");
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
                Core.DelayAction(() => Chat.Say("/laugh"), GenerateDelay());
            }

            if (bmMenu["laughOnAce"].Cast<CheckBox>().CurrentValue && args.EventId == GameEventId.OnAce && ObjectManager.GetUnitByNetworkId<AIHeroClient>(args.NetworkId).IsAlly)
            {
                Core.DelayAction(() => Chat.Say("/laugh"), GenerateDelay());
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
