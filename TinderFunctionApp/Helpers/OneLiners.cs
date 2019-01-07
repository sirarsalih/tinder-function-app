using System.Collections.Generic;

namespace TinderFunctionApp.Helpers
{
    public static class OneLiners
    {
        private static readonly Dictionary<Category, string> _dictionary = new Dictionary<Category, string>()
        {
            { Category.Charm, "Hei! Så fine øyner du har ;)" }
        };
        public static string GetOpener(Category category)
        {
            return _dictionary[category];
        }
    }

    public enum Category
    {
        Charm
    }
}
