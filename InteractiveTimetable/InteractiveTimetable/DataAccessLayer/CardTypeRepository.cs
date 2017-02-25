using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class CardTypeRepository : BaseRepository
    {
        public CardTypeRepository(SQLiteConnection connection) : base(connection)
        {
            /* Adding default card types */
            CardType activivtyCardType = new CardType()
            {
                TypeName = "ACTIVITY_CARD"
            };

            CardType motivationGoalCardType = new CardType()
            {
                TypeName = "MOTIVATION_GOAL_CARD"
            };

            SaveCardType(activivtyCardType);
            SaveCardType(motivationGoalCardType);
        }

        public CardType GetCardType(int id)
        {
            return _database.GetItem<CardType>(id);
        }

        public IEnumerable<CardType> GetCardTypes()
        {
            return _database.GetItems<CardType>();
        }

        public int SaveCardType(CardType cardType)
        {
            return _database.SaveItem(cardType);
        }

        public int DeleteCardType(int id)
        {
            return _database.DeleteItem<CardType>(id);
        }
    }

}
