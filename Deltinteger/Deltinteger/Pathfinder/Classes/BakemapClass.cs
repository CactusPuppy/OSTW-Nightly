using Deltin.Deltinteger.Parse;
using Deltin.Deltinteger.Elements;
using static Deltin.Deltinteger.Elements.Element;

namespace Deltin.Deltinteger.Pathfinder
{
    public class BakemapClass : ClassType
    {
        public ObjectVariable NodeBake { get; private set; }
        public ObjectVariable Pathmap { get; private set; }
        private readonly ITypeSupplier _types;

        public BakemapClass(ITypeSupplier types) : base("Bakemap")
        {
            _types = types;
        }

        public override void ResolveElements()
        {
            if (elementsResolved) return;
            base.ResolveElements();

            NodeBake = AddObjectVariable(new InternalVar("NodeBake"));
            Pathmap = AddObjectVariable(new InternalVar("Pathmap"));

            serveObjectScope.AddNativeMethod(Pathfind);
        }

        private FuncMethod Pathfind => new FuncMethodBuilder() {
            Name = "Pathfind",
            Documentation = "Pathfinds specified players to the destination.",
            Parameters = new CodeParameter[] {
                new CodeParameter("players", "The players to pathfind.", _types.Players()),
                new CodeParameter("destination", "The position to pathfind to.", _types.Vector())
            },
            Action = (actionSet, call) =>
            {
                // Get the ResolveInfoComponent.
                ResolveInfoComponent resolveInfo = actionSet.Translate.DeltinScript.GetComponent<ResolveInfoComponent>();

                // Get the Pathmap class.
                PathmapClass pathmapClass = actionSet.Translate.DeltinScript.Types.GetInstance<PathmapClass>();

                Element destination = call.Get(1);
                Element nodeArray = pathmapClass.Nodes.Get()[Pathmap.Get(actionSet)];

                // Get the node closest to the destination.
                Element targetNode = IndexOfArrayValue(
                    nodeArray,
                    FirstOf(Sort(
                        // Sort non-null nodes
                        /*Element.Part<V_FilteredArray>(nodeArray, new V_ArrayElement()),*/
                        nodeArray,
                        // Sort by distance to destination
                        DistanceBetween(ArrayElement(), destination)
                    ))
                );

                // For each of the players, get the current.
                resolveInfo.Pathfind(actionSet, call.Get(0), Pathmap.Get(actionSet), NodeBake.Get(actionSet)[targetNode], destination);
                return null;
            }
        };
    }
}