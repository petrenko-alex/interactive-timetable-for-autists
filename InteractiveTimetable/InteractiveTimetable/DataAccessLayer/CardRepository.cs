using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class CardRepository : BaseRepository
    {
        internal readonly CardTypeRepository CardTypes;

        public CardRepository(SQLiteConnection connection) : base(connection)
        {
            CardTypes = new CardTypeRepository(connection);
        }

        public Card GetCard(int cardId)
        {
            return _database.GetItemCascade<Card>(cardId);
        }

        public IEnumerable<Card> GetCards()
        {
            return _database.GetItemsCascade<Card>();
        }

        public int SaveCard(Card card)
        {
            return _database.SaveItemCascade(card);
        }

        public int DeleteCard(int cardId)
        {
            return _database.DeleteItem<Card>(cardId);
        }

        public void DeleteCardCascade(Card card)
        {
            _database.DeleteItemCascade(card);
        }
    }
}
