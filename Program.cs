using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Text;

namespace TenThousandsCharsForSpriteFont;

internal static class Program {
    private static string DisplayCharacter(char character) {
        int codePoint = character;
        return $"{character} (U+{codePoint:X4})";
    }

    private static IReadOnlyList<char> GetUnicode1994HanCharacters() {
        // https://www.unicode.org/Public/1.1-Update/CJKXREF.TXT
        using StreamReader reader = new("CJKXREF.utf8.txt", new UTF8Encoding(false));
        List<int> codePoints = [];

        while (!reader.EndOfStream) {
            string? line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) {
                continue; // Skip empty lines and comments
            }
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && int.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out int codePoint)) {
                codePoints.Add(codePoint);
            }
        }

        return codePoints.Select(p => (char)p).ToList();
    }

    private static IReadOnlyList<char> GetUnicodeCharsInRange(int start, int end, IEnumerable<int>? excluded = null) {
        List<char> result = [];
        for (int codePoint = start; codePoint <= end; codePoint++) {
            char character = (char)codePoint;
            if (char.IsSurrogate(character) || char.IsControl(character)) {
                continue; // Skip surrogate and control characters
            }

            if (excluded != null && excluded.Contains(codePoint)) {
                continue; // Skip excluded characters
            }

            result.Add(character);
        }
        return result;
    }

    private static IReadOnlyList<char> GetCharactersFromTextFile(string filePath, Encoding encoding) {
        List<char> chars = File.ReadAllLines(filePath, encoding).SelectMany(line => line.ToCharArray()).Distinct().ToList();
        return chars;
    }

    private static IReadOnlyList<char> GetGB2312Chars() {
        // https://github.com/StarCompute/GB2312Font/raw/2c5ab2d891ccc62025b9928884d1b4657e12e9c9/gb2312%E5%AD%97%E7%AC%A6%E9%9B%86.txt

        IReadOnlyList<char> chars = GetCharactersFromTextFile("gb2312.utf8.txt", new UTF8Encoding(false));
        int charCount = chars.Count;
        Debug.Assert(charCount == 7445, "GB2312 character count should be 7445, but got " + charCount);
        return chars;
    }

    private static IReadOnlyList<char> GetTongYongGuiFanHanZiBiaoLevelOneChars() {
        // https://raw.githubusercontent.com/shengdoushi/common-standard-chinese-characters-table/9c450ebdc4680820010745a6f1078562127739bf/level-1.txt

        IReadOnlyList<char> chars = GetCharactersFromTextFile("level-1.utf8.txt", new UTF8Encoding(false));
        Debug.Assert(chars.Count == 3500, "Tong Yong Gui Fan Han Zi Biao level-1 character count should be 3500, but got " + chars.Count);
        return chars;
    }

    private static IReadOnlyList<char> GetTongYongGuiFanHanZiBiaoLevelTwoChars() {
        // https://raw.githubusercontent.com/shengdoushi/common-standard-chinese-characters-table/9c450ebdc4680820010745a6f1078562127739bf/level-2.txt
        IReadOnlyList<char> chars = GetCharactersFromTextFile("level-2.utf8.txt", new UTF8Encoding(false));

        // Debug.Assert(chars.Count == 3000, "Tong Yong Gui Fan Han Zi Biao level-2 character count should be 3000, but got " + chars.Count);
        // TODO: there are 3 more characters than expected. Investigate later.
        return chars;
    }

    private static IReadOnlyList<char> GetTongYongGuiFanHanZiBiaoLevelThreeChars() {
        // https://raw.githubusercontent.com/shengdoushi/common-standard-chinese-characters-table/d9b599a9c9cc0dd2d58cad829e285bc780cd4451/level-3.txt
        // Note: this URL has a different commit hash than the other two level files.

        IReadOnlyList<char> chars = GetCharactersFromTextFile("level-3.utf8.txt", new UTF8Encoding(false));

        // TODO: there are 3 more characters than expected. Investigate later.
        // Debug.Assert(chars.Count == 1605, "Tong Yong Gui Fan Han Zi Biao level-3 character count should be 1605, but got " + chars.Count);
        return chars;
    }

    private static IReadOnlyList<char> GetYiJianDuiDuoFanChars() {
        IReadOnlyList<char> chars = GetCharactersFromTextFile("YiJianDuiDuoFan.utf8.txt", new UTF8Encoding(false));
        return chars;
    }

    private static IReadOnlyList<char> GetHansChars() {
        List<char> chars = [];

        IReadOnlyList<char> unicodeHanChars = GetUnicode1994HanCharacters();

        // Add GB2312 characters
        chars.AddRange(unicodeHanChars.Intersect(GetGB2312Chars()));

        // Add 3500 level-1 characters in Tong Yong Gui Fan Han Zi Biao 通用规范汉字表.
        chars.AddRange(unicodeHanChars.Intersect(GetTongYongGuiFanHanZiBiaoLevelOneChars()));

        // Add 3000 level-2 characters in Tong Yong Gui Fan Han Zi Biao 通用规范汉字表
        chars.AddRange(unicodeHanChars.Intersect(GetTongYongGuiFanHanZiBiaoLevelTwoChars()));

        // Add 1605 level-3 characters in Tong Yong Gui Fan Han Zi Biao 通用规范汉字表
        chars.AddRange(unicodeHanChars.Intersect(GetTongYongGuiFanHanZiBiaoLevelThreeChars()));

        chars = chars.Distinct().ToList();

        return chars;
    }

    private static IReadOnlyList<char> GetHantChars() {
        IReadOnlyList<char> hansChars = GetHansChars();

        Dictionary<char, List<char>> stMap = [];
        {
            using StreamReader reader = new("STCharacters.utf8.txt", new UTF8Encoding(false));
            while (!reader.EndOfStream) {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('\t');
                if (parts.Length == 2) {
                    char hans = parts[0][0];
                    string[] hantStrings = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<char> hants = hantStrings.Select(s => s[0]).ToList();
                    stMap[hans] = hants;
                }
            }
        }

        List<char> hantChars = [];
        foreach (char c in hansChars) {
            // Get zh-Hant character from OpenCC
            if (stMap.TryGetValue(c, out var hants)) {
                hantChars.AddRange(hants);
            }

            // Get zh-Hant character from VB's StrConv
            {
                string hantStr = Strings.StrConv(c.ToString(), VbStrConv.TraditionalChinese, LocaleID: 2052);
                Debug.Assert(hantStr.Length == 1, "Converted Hant character length should be 1");
                char hantChar = hantStr[0];
                hantChars.Add(hantChar);
            }
        }

        hantChars.AddRange(GetYiJianDuiDuoFanChars());

        hantChars = hantChars.Distinct().ToList();

        {
            var exclusiveHansChars = hansChars.Where(c => !hantChars.Contains(c)).ToList();
            Debug.WriteLine($"{exclusiveHansChars.Count} zh-Hans exclusive Han characters: " + string.Join(string.Empty, exclusiveHansChars));
        }

        {
            var exclusiveHantChars = hantChars.Where(c => !hansChars.Contains(c)).ToList();
            Debug.WriteLine($"{exclusiveHantChars.Count} zh-Hant exclusive Han characters: " + string.Join(string.Empty, exclusiveHantChars));
        }

        {
            var sharedChars = hantChars.Intersect(hansChars).ToList();
            Debug.WriteLine($"{sharedChars.Count} shared Han characters between zh-Hans and zh-Hant: " + string.Join(string.Empty, hantChars.Intersect(hansChars)));
        }

        return hantChars;
    }

    private static void Main(string[] args) {
        bool zhHant = args.Length > 0 && args[0] == "--zh-Hant";
        if (zhHant) {
            Console.WriteLine("Generating characters for zh-Hant...");
        }
        else {
            Console.WriteLine("Generating characters for zh-Hans...");
        }

        List<char> chars = [];

        // U+0020 -> U+007F: Basic Latin.
        chars.AddRange(GetUnicodeCharsInRange(0x0020, 0x007F));

        // U+0080 -> U+00FF: Latin-1 Supplement.
        chars.AddRange(GetUnicodeCharsInRange(0x0080, 0x00FF));

        // U+0100 -> U+017F: Latin Extended-A.
        chars.AddRange(GetUnicodeCharsInRange(0x0100, 0x017F));

        // Latin Extended-B. 0180–024F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x0180, 0x024F));

        // U+1EA0 -> U+1EF9: Latin Extended Additional, additional letters for Vietnamese (skipped)

        // Greek and Coptic. Range: 0370–03FF. Excludes reserved characters 0378, 0379, 0380, 0381, 0382, 0383, 038B, 038D, 03A2
        chars.AddRange(GetUnicodeCharsInRange(0x0370, 0x03FF, excluded: [0x0378, 0x0379, 0x0380, 0x0381, 0x0382, 0x0383, 0x038B, 0x038D, 0x03A2]));

        // U+0400 -> U+04FF: Cyrillic. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x0400, 0x04FF));

        // U+FF01 -> U+FFEE: Halfwidth and Fullwidth Forms. Excludes reserved characters FF00, FFBF, FFC0, FFC1, FFC8, FFC9, FFD0, FFD1, FFD8, FFD9, FFDD, FFDE, FFDF, FFE7, FFEF
        // Also excludes: Halfwidth Hangul variants FFA0 -- FFDC
        chars.AddRange(
            GetUnicodeCharsInRange(0xFF01, 0xFFEE, excluded: [0xFF00, 0xFFBF, 0xFFC0, 0xFFC1, 0xFFC8, 0xFFC9, 0xFFD0, 0xFFD1, 0xFFD8, 0xFFD9, 0xFFDD, 0xFFDE, 0xFFDF, 0xFFE7, 0xFFEF])
            .Where(c => c is < (char)0xFFA0 or > (char)0xFFDC) // Exclude Halfwidth Hangul variants
        );

        // General Punctuation
        // 1. Spaces. 2000 -> 200A (skipped)
        // 2. Format characters (skipped)
        // 3. Dashes. 2010 -> 2015
        chars.AddRange(GetUnicodeCharsInRange(0x2010, 0x2015));
        // 4. General punctuation. 2016 -> 2017
        chars.AddRange(GetUnicodeCharsInRange(0x2016, 0x2017));
        // 5. Quotation marks and apostrophe. 2018 -> 201F
        chars.AddRange(GetUnicodeCharsInRange(0x2018, 0x201F));
        // 6. General punctuation. 2020 -> 2027
        chars.AddRange(GetUnicodeCharsInRange(0x2020, 0x2027));
        // 7. Separators (skipped)
        // 8. Format characters (skipped)
        // 9. Space. 202F (skipped)
        // 10.General punctuation. 2030-> 2038
        chars.AddRange(GetUnicodeCharsInRange(0x2030, 0x2038));
        // 11. Quotation marks. 2039 -> 203A (skipped)
        // 12. General punctuation. 203B
        chars.AddRange(GetUnicodeCharsInRange(0x203B, 0x203B));
        // 13. Double punctuation for vertical text. 203C (skipped)
        // 14. General punctuation. 203D -> 2044 (skipped)
        // 15. Brackets. 2045 -> 2046 (skipped)
        // 16. Double punctuation for vertical text. 2047 -> 2049 (skipped)
        // 17. General punctuation. 204A -> 2055 (skipped)
        // 18. Archaic punctuation. 2056. (skipped)
        // 19. General punctuation. 2057 (skipped)
        // 20. Archaic punctuation. 2058 -> 205E. (skipped)
        // 21. Space. 205F (skipped)
        // 22. Format character (skipped)
        // 23. Invisible operators (skipped)
        // 24. Format characters (skipped)
        // 25. Deprecated (skipped)

        //// Enclosed CJK Letters and Months. Only includes 3220 -- 325F and range 3280 -- 32FE, so Hangul characters are excluded.
        //chars.AddRange(GetUnicodeCharsInRange(0x3220, 0x325F));
        //chars.AddRange(GetUnicodeCharsInRange(0x3280, 0x32FE));

        // Enclosed CJK Letters and Months. Only includes GB2312 characters: ㈠ (U+3220), ㈡ (U+3221), ㈢ (U+3222), ㈣ (U+3223), ㈤ (U+3224), ㈥ (U+3225), ㈦ (U+3226), ㈧ (U+3227), ㈨ (U+3228), ㈩ (U+3229)
        chars.AddRange(GetUnicodeCharsInRange(0x3220, 0x3229));

        // Number Forms. 2150 -- 218B. Only includes 2160--216B and 2170--217F
        chars.AddRange(GetUnicodeCharsInRange(0x2160, 0x216B));
        chars.AddRange(GetUnicodeCharsInRange(0x2170, 0x217F));

        //// Miscellaneous Symbols. 2600–26FF. Includes all.
        //chars.AddRange(GetUnicodeCharsInRange(0x2600, 0x26FF));

        // Only introduce these miscellaneous symbols. (GB2312)
        chars.AddRange(GetUnicodeCharsInRange(0x2605, 0x2606));
        chars.AddRange(GetUnicodeCharsInRange(0x2640, 0x2640));
        chars.AddRange(GetUnicodeCharsInRange(0x2642, 0x2642));

        // Letterlike Symbols. 2100–214F. Includes all.
        // chars.AddRange(GetUnicodeCharsInRange(0x2100, 0x214F));

        // Letterlike Symbols. Only add ℃ (U+2103), № (U+2116) in GB2312. Also, add ℉(U+2109)
        chars.AddRange(GetUnicodeCharsInRange(0x2103, 0x2103));
        chars.AddRange(GetUnicodeCharsInRange(0x2109, 0x2109));
        chars.AddRange(GetUnicodeCharsInRange(0x2116, 0x2116));

        // CJK Symbols and Punctuation. 3000–303F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x3000, 0x303F));

        // U+3040 -> U+309F: Hiragana. Excludes reserved characters 3040, 3097, 3098. Excludes Voicing marks (3099-309C)
        chars.AddRange(GetUnicodeCharsInRange(0x3040, 0x309F, excluded: [0x3040, 0x3097, 0x3098, 0x3099, 0x309A, 0x309B, 0x309C]));

        // U+30A0 -> U+30FF: Katakana. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x30A0, 0x30FF));

        // Spacing Modifier Letters. 02B0–02FF. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x02B0, 0x02FF));

        // Mathematical Operators. 2200–22FF. Includes 2022-22F1.
        chars.AddRange(GetUnicodeCharsInRange(0x2200, 0x22F1));

        // Geometric Shapes. 25A0–25FF. Includes 25A0-25EF. (Unicode 2.0)
        chars.AddRange(GetUnicodeCharsInRange(0x25A0, 0x25EF));

        // Enclosed Alphanumerics. 2460–24FF. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x2460, 0x24FF));

        // Bopomofo. 3105 -- 312F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x3105, 0x312F));

        // Box Drawing. 2500–257F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x2500, 0x257F));

        // Arrows. 2190–21FF. Includes 2190--21EA. (Unicode 2.0)
        chars.AddRange(GetUnicodeCharsInRange(0x2190, 0x21EA));

        //// Miscellaneous Technical. 2300–23FF. Includes all.
        //chars.AddRange(GetUnicodeCharsInRange(0x2300, 0x23FF));

        // Miscellaneous Technical. Only add ⌒ (U+2312) in GB2312
        chars.AddRange(GetUnicodeCharsInRange(0x2312, 0x2312));

        // There should be no duplicate characters. Throw if there are any.
        {
            List<char> duplicateChars = chars
            .GroupBy(c => c)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

            if (duplicateChars.Count > 0) {
                throw new Exception($"There are {duplicateChars.Count} duplicate characters: {string.Join(", ", duplicateChars.Select(DisplayCharacter))}");
            }
        }

        // U+4E00 -> U+9FFF: CJK Unified Ideographs. Can't include all of them. Will include a selected subset.

        // All non-Han characters should already be included above.
        {
            IReadOnlyList<char> unicodeHans = GetUnicode1994HanCharacters();
            IReadOnlyList<char> gb2312NonHanChars = GetGB2312Chars().Where(c => !unicodeHans.Contains(c)).ToList();
            IReadOnlyList<char> notIncludedChars = gb2312NonHanChars.Where(c => !chars.Contains(c)).ToList();
            if (notIncludedChars.Count > 0) {
                throw new Exception($"There are {notIncludedChars.Count} non-Han characters in GB2312 that haven't been added: {string.Join(", ", notIncludedChars.Select(DisplayCharacter))}");
            }
        }

        if (!zhHant) {
            // Add Chinese (Simplified) characters
            Console.WriteLine("Before adding Hans characters, total characters: " + chars.Count);
            var hansChars = GetHansChars();
            chars.AddRange(hansChars);
            chars = chars.Distinct().ToList();
            Console.WriteLine("After adding Hans characters, total characters: " + chars.Count);
        }
        else {
            // Add Chinese (Traditional) characters
            Console.WriteLine("Before adding Hant characters, total characters: " + chars.Count);
            var hantChars = GetHantChars();
            chars.AddRange(hantChars);
            chars = chars.Distinct().ToList();
            Console.WriteLine("After adding Hant characters, total characters: " + chars.Count);
        }

        // Output the character count to console
        Console.WriteLine($"Total characters: {chars.Count}");

        // Output the characters to a txt file
        using (StreamWriter writer = new($"TenThousandsCharsForSpriteFont{chars.Count}.txt", false, new UTF8Encoding(false))) {
            foreach (char ch in chars) {
                writer.Write(ch);
            }
        }

        // Output the characters in a friendly format to a txt file
        using (StreamWriter writer = new($"TenThousandsCharsForSpriteFont{chars.Count}_Friendly.txt", false, new UTF8Encoding(false))) {
            foreach (char ch in chars) {
                writer.Write(DisplayCharacter(ch));
                writer.WriteLine();
            }
        }

    }
}
