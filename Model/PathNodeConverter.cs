using Newtonsoft.Json.Serialization;
using uzSurfaceMapper.Utils.Serialization;

namespace uzSurfaceMapper.Model
{
    public class PathNodeConverter : LinkedListItemConverter<PathNode>
    {
        protected override bool IsNextItemProperty(JsonProperty member)
        {
            return member.UnderlyingName == "Neighbors"; //
                                                         // || member.UnderlyingName == "ParentNode";
        }
    }
}