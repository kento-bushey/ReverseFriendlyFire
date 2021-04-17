using System.Collections.Generic;
using Exiled.Events.EventArgs;
using Exiled.API.Features;
using Exiled.API.Enums;

namespace FFREVERSE.Handlers
{
	class Player
	{
		public static Dictionary<string, PlayerInfo> PlayerInfoDict = new Dictionary<string, PlayerInfo>();
		private int rounds = 0;
		private bool enabled = true;
		private bool roundend = false;
		private void addPlayer(Exiled.API.Features.Player p)
		{
			if (!PlayerInfoDict.ContainsKey(p.UserId))
			{
				Exiled.API.Features.Log.Info("Player with userID:" + p.UserId + " added to dict.");
				PlayerInfoDict.Add(p.UserId, new PlayerInfo());
			}
		}
		private void updateDict()
		{
			IEnumerable<Exiled.API.Features.Player> PList = Exiled.API.Features.Player.List;
			foreach (Exiled.API.Features.Player p in PList)
			{
				addPlayer(p);
			}
		}
		public void OnRoundStart()
		{
			roundend = true;
			enabled = true;
			updateDict();
			rounds += 1;
			if (rounds> FFREVERSE.Instance.Config.FFRounds)
            {
				foreach (Exiled.API.Features.Player p in Exiled.API.Features.Player.List)
				{
					PlayerInfo pinfo = PlayerInfoDict[p.UserId];
					pinfo.teamDamage = 0;
					pinfo.teamKills = 0;
				}
			}
		}
		public void OnJoin(VerifiedEventArgs ev)
		{
			if (ev.Player != null)
            {
				addPlayer(ev.Player);
			}
		}
		public void OnKills(DyingEventArgs ev)
		{
			if (ev.HitInformation.GetDamageType() == DamageTypes.MicroHid && !FFREVERSE.Instance.Config.FFMicro)
			{
				return;
			}
			if (ev.Killer != null && ev.Killer.UserId != ev.Target.UserId && !roundend)
			{
				if (PlayerInfoDict.ContainsKey(ev.Killer.UserId))
				{
					addPlayer(ev.Killer);
				}
				PlayerInfo pinfo = PlayerInfoDict[ev.Killer.UserId];
				if (ev.Killer.Side == ev.Target.Side)
				{
					pinfo.teamKills += 1;
				}
			}

		}
		public void OnDamage(HurtingEventArgs ev)
		{
			if (!PlayerInfoDict.ContainsKey(ev.Attacker.UserId))
			{
				return;
			}
			if (ev.DamageType == DamageTypes.MicroHid && !FFREVERSE.Instance.Config.FFMicro)
            {
				return;
            }
			PlayerInfo pinfo = PlayerInfoDict[ev.Attacker.UserId];
			if (ev.Attacker.Side == ev.Target.Side && ev.Attacker.Id != ev.Target.Id && !roundend)
			{
				if (pinfo.teamKills >= FFREVERSE.Instance.Config.FFKills || pinfo.teamDamage >= FFREVERSE.Instance.Config.FFDamage)
				{
					ev.Attacker.ShowHint("<b><color=#F52929>Инверсия \"огня по своим\" включена</color></b>", 5);
					ev.Attacker.Hurt(ev.Amount,DamageTypes.E11StandardRifle,ev.Attacker.DisplayNickname);
					ev.IsAllowed = false;
                }
                else
                {
					ev.Attacker.ShowHint("<color=#F52929>Не наносите урон своим союзникам</color>", 3);
				}
				pinfo.teamDamage += ev.Amount;
			}
		}
		public void OnRoundEnding(RoundEndedEventArgs ev)
        {
			roundend = true;
        }
	}
}
