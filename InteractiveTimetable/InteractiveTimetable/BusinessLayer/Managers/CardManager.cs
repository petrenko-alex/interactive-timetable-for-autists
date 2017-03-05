using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using SQLite;

namespace InteractiveTimetable.BusinessLayer.Managers
{
    class CardManager
    {
        private CardRepository _repository;

        public CardManager(SQLiteConnection connection)
        {
            _repository = new CardRepository(connection);
        }

        public Card GetCard(int cardId)
        {
            return _repository.GetCard(cardId);
        }

        public IEnumerable<Card> GetCards()
        {
            return _repository.GetCards();
        }

        public int SaveCard(Card card)
        {
            /* Data validation */
            Validate(card);

            return _repository.SaveCard(card);
        }

        public void DeleteCard(int cardId)
        {
            var card = _repository.GetCard(cardId);
            _repository.DeleteCardCascade(card);
        }

        public IEnumerable<Card> GetActivityCards()
        {
            return _repository.GetCards().Where(
                x => _repository.CardTypes.IsActivityCardType(x.CardTypeId));
        }

        public IEnumerable<Card> GetMotivationGoalCards()
        {
            return _repository.GetCards().Where(
                x => _repository.CardTypes.IsMotivationGoalCardType(x.CardTypeId));
        }

        private void Validate(Card card)
        {
            /* Path to photo is longer than 1024 symbols */
            if (card.PhotoPath.Length > 1024)
            {
                throw new ArgumentException("The length of the path " +
                                            "to the card's photo must be less " +
                                            "than 1024 symbols");
            }
        }
    }
}
