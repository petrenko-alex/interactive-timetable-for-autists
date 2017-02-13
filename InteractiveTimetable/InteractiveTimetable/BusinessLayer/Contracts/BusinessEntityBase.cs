using System.Collections.Generic;
using System.Reflection;
using SQLite;

namespace InteractiveTimetable.BusinessLayer.Contracts
{
    public class BusinessEntityBase : IBusinessEntity
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
    }
}
