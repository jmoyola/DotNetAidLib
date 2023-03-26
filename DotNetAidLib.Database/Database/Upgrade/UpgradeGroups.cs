using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetAidLib.Database.Upgrade
{
    [CollectionDataContract(Name = "UpgradeGroups")]
    public class UpgradeGroups : List<UpgradeGroup>
    {
    }
}