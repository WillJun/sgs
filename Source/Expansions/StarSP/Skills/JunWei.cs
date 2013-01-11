﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;
using System.Threading;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 军威–回合结束阶段开始时，若“锦”的数量达到3或更多，你可以弃置三张“锦”，并选择一名角色，该角色须选择一项：1、展示一张【闪】，然后交给一名由你指定的其他角色；2、失去1点体力，然后令你将其装备区内的一张牌移出游戏，该角色的回合结束后，将移除游戏的牌置入其装备区。
    /// </summary>
    public class JunWei : TriggerSkill
    {
        class JunWeiGiveShanVerifier : CardsAndTargetsVerifier
        {
            public JunWeiGiveShanVerifier()
            {
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }
        }

        class JunWeiVerifier : CardUsageVerifier
        {
            public JunWeiVerifier()
            {
                Helper.OtherDecksUsed.Add(YinLing.JinDeck);
            }
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (players != null && players.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 3)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Any(c => c.Place.DeckType != YinLing.JinDeck))
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count < 3)
                    return VerifierResult.Partial;
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        class JunWeiShowCardVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                if (!(cards[0].Type is Shan))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            List<List<Card>> answer;
            if (!Owner.AskForCardUsage(new CardUsagePrompt("JunWei"), new JunWeiVerifier(), out skill, out cards, out players))
            {
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
                Player target = players[0];
                if (target.AskForCardUsage(new CardUsagePrompt("JunWeiShowCard"), new JunWeiShowCardVerifier(), out skill, out cards, out players))
                {
                    Card temp = cards[0];
                    Game.CurrentGame.NotificationProxy.NotifyShowCard(target, temp);
                    if (!Owner.AskForCardUsage(new CardUsagePrompt("JunWeiGiveShan"), new JunWeiGiveShanVerifier(), out skill, out cards, out players))
                    {
                        players = new List<Player>() { Owner };
                    }
                    Game.CurrentGame.SyncCardAll(ref temp);
                    Game.CurrentGame.HandleCardTransferToHand(target, players[0], new List<Card>() { temp });
                }
                else
                {
                    Game.CurrentGame.LoseHealth(target, 1);
                    if (target.Equipments().Count == 0) return;
                    Thread.Sleep(380);
                    List<DeckPlace> sourceDecks = new List<DeckPlace>();
                    sourceDecks.Add(new DeckPlace(target, DeckType.Equipment));
                    if (!Owner.AskForCardChoice(new CardChoicePrompt("JunWeiChoice", target, Owner),
                        sourceDecks,
                        new List<string>() { "JunWei" },
                        new List<int>() { 1 },
                        new RequireOneCardChoiceVerifier(),
                        out answer))
                    {
                        answer[0].Clear();
                        answer[0].Add(target.Equipments().First());
                    }
                    Game.CurrentGame.HandleCardTransfer(target, target, TempDeck, answer[0]);
                }
            }
        }
        public JunWei()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[p, YinLing.JinDeck].Count >= 3; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
            IsAutoInvoked = null;
            Helper.OtherDecksUsed.Add(YinLing.JinDeck);
        }

        public static PrivateDeckType TempDeck = new PrivateDeckType("Temp", true);
    }
}