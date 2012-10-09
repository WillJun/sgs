﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 仁德-出牌阶段，你可以将任意数量的手牌交给其他角色，若此阶段你给出的牌张数达到两张或更多时，你回复1点体力。
    /// </summary>
    public class RenDe : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            List<Card> cards = arg.Cards;
            if (cards != null)
            {
                foreach (Card card in cards)
                {
                    if (card.Owner != Owner || card.Place.DeckType != DeckType.Hand)
                    {
                        return VerifierResult.Fail;
                    }
                }
            }
            if (arg.Targets != null && arg.Targets.Count != 0 &&
                (arg.Targets.Count > 1 || arg.Targets[0] == Owner))
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            List<Card> cards = arg.Cards;
            Trace.Assert(cards.Count > 0 && arg.Targets.Count == 1);
            CardsMovement move;
            move.cards = new List<Card>(cards);
            move.to = new DeckPlace(arg.Targets[0], DeckType.Hand);
            int cardsHadGiven = Owner[RenDeNumberOfCardsGiven];
            Owner[RenDeNumberOfCardsGiven] += cards.Count;
            Game.CurrentGame.MoveCards(move, new CardUseLog() { Source = Owner, Targets = arg.Targets, Skill = this, Cards = null });
            if (Owner[RenDeNumberOfCardsGiven] >= 2 && cardsHadGiven < 2)
            {
                Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
            }
            return true;
        }

        public static string RenDeNumberOfCardsGiven = "RenDeNumberOfCardsGiven";

        public override Core.Players.Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                base.Owner = value;
                Owner.AutoResetAttributes.Add(RenDeNumberOfCardsGiven);
            }
        }
    }
}
