
using Verse;
using System.Reflection;

namespace oreprocessing
{

        [StaticConstructorOnStartup]
    public static class NonPublicFields
    {

        public static FieldInfo ThingDef_allRecipesCached = typeof(ThingDef).GetField("allRecipesCached", BindingFlags.Instance | BindingFlags.NonPublic);

    }
}
