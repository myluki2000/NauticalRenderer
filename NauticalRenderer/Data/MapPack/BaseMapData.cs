using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NauticalRenderer.Data.Map;
using OsmSharp.Complete;
using OsmSharp.Streams;

namespace NauticalRenderer.Data.MapPack
{
    class BaseMapData
    {
        public List<RestrictedArea> RestrictedAreas { get; private set; }

        public void Load(MapPack mapPack)
        {
            bool loadRestrictedAreas = false;
            if (RestrictedAreas == null)
            {
                loadRestrictedAreas = true;
                RestrictedAreas = new List<RestrictedArea>();
            }

            foreach (ICompleteOsmGeo geo in new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete())
            {
                if (loadRestrictedAreas)
                {
                    if (geo.Tags.Contains("seamark:type", "restricted_area") && geo is CompleteWay way)
                    {
                        RestrictedAreas.Add(new RestrictedArea
                        (
                            way.Tags.GetValue("name"),
                            way.Nodes.Select(y => new Vector2((float)y.Longitude, -(float)y.Latitude)).ToArray(),
                            way.Tags.TryGetValue("seamark:restricted_area:category", out string value)
                                ? Enum.TryParse(value, true, out RestrictedArea.RestrictedAreaCategory cat) ? cat :
                                RestrictedArea.RestrictedAreaCategory.UNKNOWN
                                : RestrictedArea.RestrictedAreaCategory.UNKNOWN,
                            way.Tags.TryGetValue("seamark:restricted_area:restriction", out value)
                                ? Enum.TryParse(value, true, out RestrictedArea.RestrictedAreaRestriction r) ? r :
                                RestrictedArea.RestrictedAreaRestriction.UNKNOWN
                                : RestrictedArea.RestrictedAreaRestriction.UNKNOWN
                        ));
                    }
                }
            }
        }
    }
}
