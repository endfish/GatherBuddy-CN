using System.Linq;
using Dalamud.Interface.Textures;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.Interfaces;
using GatherBuddy.Time;

namespace GatherBuddy.Gui;

public partial class Interface
{
    public class ExtendedGatherable
    {
        public Gatherable              Data;
        public ISharedImmediateTexture Icon;
        public string                  Territories;
        public string                  Uptimes;
        public string                  Folklore;
        public string                  Level;
        public string                  NodeNames;
        public string                  Expansion;
        public string                  Aetherytes;

        public (ILocation, TimeInterval) Uptime
            => GatherBuddy.UptimeManager.BestLocation(Data);

        public ExtendedGatherable(Gatherable data)
        {
            Data = data;
            Icon = Icons.DefaultStorage.TextureProvider.GetFromGameIcon(new GameIconLookup(data.ItemData.Icon));

            Territories = string.Join("\n", data.NodeList.Select(n => n.Territory.Name).Distinct());
            if (!Territories.Contains('\n'))
                Territories = '\0' + Territories;

            Folklore = data.NodeList.Count == 0 || data.NodeList.Any(n => n.Folklore.Length == 0)
                ? string.Empty
                : data.NodeList.First().Folklore;
            Uptimes = data.NodeType switch
            {
                NodeType.Regular => Localize.Text("Always"),
                NodeType.Unknown => Localize.Text("Unknown"),
                _                => data.NodeList.Select(n => n.Times).Aggregate(BitfieldUptime.Combine).PrintHours(true),
            };
            Level     = Data.LevelString();
            NodeNames = string.Join("\n", data.NodeList.Select(n => n.Name).Distinct());
            if (!NodeNames.Contains('\n'))
                NodeNames = '\0' + NodeNames;

            Expansion = data.ExpansionIdx switch
            {
                0 => Localize.Text("ARR"),
                1 => Localize.Text("HW"),
                2 => Localize.Text("SB"),
                3 => Localize.Text("ShB"),
                4 => Localize.Text("EW"),
                5 => Localize.Text("DT"),
                _ => Localize.Text("Unk"),
            };
            Aetherytes = string.Join("\n",
                data.NodeList.Where(n => n.ClosestAetheryte != null).Select(n => n.ClosestAetheryte!.Name).Distinct());
            if (!Aetherytes.Contains('\n'))
                Aetherytes = '\0' + Aetherytes;
        }
    }
}
