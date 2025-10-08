using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Visit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.DataModel
{
    class Comparer
    {
    }

    public class DoorModelCompare : IEqualityComparer<DoorModel>
    {
        public bool Equals(DoorModel x, DoorModel y)
        {
            if (
                string.Equals(x.DoorId.ToString(), y.DoorId.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.AccessTimeId.ToString(), y.AccessTimeId.ToString(), StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(DoorModel obj)
        {
            return obj.DoorId.GetHashCode();
        }
    }
}
