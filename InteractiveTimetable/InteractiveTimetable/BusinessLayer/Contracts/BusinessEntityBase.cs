using System.Collections.Generic;
using System.Reflection;
using SQLite;

namespace InteractiveTimetable.BusinessLayer.Contracts
{
    public abstract class BusinessEntityBase : IBusinessEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public override string ToString()
        {
            string businessEntityInfo = GetType().Name + ":\n";

            foreach (var property in GetType().GetRuntimeProperties())
            {
                businessEntityInfo += 
                    property.Name + 
                    ": " + 
                    property.GetValue(this) + 
                    "\n";
            }

            return businessEntityInfo;
        }

        #region HashCode

        protected static readonly int InitialHashValue = 17;
        protected static readonly int HashNumber = 23;

        [Ignore]
        protected int HashCode
        {
            get
            {
                return _isHashSet ? _hashCode : GetHashCode();
            }
        }

        protected int _hashCode;

        private bool _isHashSet = false;

        #endregion
    }
}
