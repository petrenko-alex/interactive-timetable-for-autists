using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class CardRepository : BaseRepository
    {
        public CardRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public Card GetCard(int id)
        {
            return _database.GetItem<Card>(id);
        }

        public IEnumerable<Card> GetCards()
        {
            return _database.GetItems<Card>();
        }

        public int SaveCard(Card card)
        {
            return _database.SaveItem(card);
        }

        public int DeleteCard(int id)
        {
            return _database.DeleteItem<Card>(id);
        }
    }
}
