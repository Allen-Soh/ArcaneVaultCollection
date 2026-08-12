//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultRP.Helpers
{
    public static class CategoryCodeHelper
    {
        public static string GenerateAlphabetPrefix(string categoryName)
        {
            var words = categoryName.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string prefix;

            if (words.Length == 1)
            {
                var word = words[0].ToUpper();
                if (word.Length < 3)
                    throw new ArgumentException("Category name must be at least 3 characters long.");
                if (word.Length == 3)
                    prefix = word;
                else
                    prefix = $"{word[0]}{word[2]}{word[word.Length - 1]}";
            }
            else if (words.Length == 2)
            {
                var w1 = words[0].ToUpper();
                var w2 = words[1].ToUpper();
                prefix = $"{w1[0]}{w2[0]}{w2[w2.Length - 1]}";
            }
            else if (words.Length == 3)
            {
                prefix = $"{words[0][0]}{words[1][0]}{words[2][0]}".ToUpper();
            }
            else // 4+ words
            {
                var w1 = words[0].ToUpper();
                var last = words[words.Length - 1].ToUpper();
                prefix = $"{w1[0]}{w1[2]}{last[last.Length - 1]}".ToUpper();
            }

            return prefix.ToUpper();
        }

        public static string GenerateSuffix(int number)
        {
            // Format number as 3-digit zero-padded string, max 999
            if (number > 999)
                throw new ArgumentException("Maximum category codes reached for this prefix.");
            return number.ToString("D3");
        }

        public static string GenerateCategoryCode(string categoryName, List<string> existingCodes)
        {
            var prefix = GenerateAlphabetPrefix(categoryName);

            // LINQ — find all existing codes with same prefix
            var matchingCodes = existingCodes
                .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingCodes.Count == 0)
            {
                return $"{prefix}-001";
            }

            // LINQ — extract existing suffix numbers for this prefix
            var existingNumbers = matchingCodes
                .Select(c =>
                {
                    var parts = c.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int num))
                        return num;
                    return 0;
                })
                .OrderBy(n => n)
                .ToList();

            // Find next available number
            int next = 1;
            while (existingNumbers.Contains(next))
                next++;

            return $"{prefix}-{GenerateSuffix(next)}";
        }
    }
}