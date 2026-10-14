using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Text;

// 本程序依赖 Microsoft.VisualBasic.Strings.StrConv 做简繁转换，
// 该 API 只在 Windows 上实现（内部走 NLS），所以目标框架固定为 net48、
// 且只能在 Windows 上运行。

namespace TenThousandsCharsForEmperorBattleForDune;

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

    /// <summary>
    /// Microsoft.VisualBasic.Strings.StrConv 是否可用。
    /// 它只在 Windows 上实现（内部依赖 NLS），非 Windows 平台调用会抛
    /// PlatformNotSupportedException，因此先探测一次，不可用就退回只用
    /// OpenCC 的 STCharacters 映射表。
    /// </summary>
    private static readonly bool SStrConvAvailable = ProbeStrConv();

    private static bool ProbeStrConv() {
        try {
            _ = Strings.StrConv("国", VbStrConv.TraditionalChinese, LocaleID: 2052);
            return true;
        }
        catch (Exception) {
            Console.WriteLine("[提示] 当前平台不支持 Strings.StrConv（简繁转换走 NLS，仅 Windows 可用），"
                + "将只使用 OpenCC 的 STCharacters 映射表。");
            Console.WriteLine("       在 Windows 上运行可获得与原作者一致的完整结果。");
            return false;
        }
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

            // Get zh-Hant character from VB's StrConv（Windows 的 NLS 简繁转换）
            if (SStrConvAvailable) {
                string? hantStr = Strings.StrConv(c.ToString(), VbStrConv.TraditionalChinese, LocaleID: 2052);
                if (hantStr is null || hantStr.Length != 1) {
                    throw new InvalidOperationException(
                        $"Strings.StrConv 未能把 {DisplayCharacter(c)} 转成单个繁体字（返回 {hantStr?.Length.ToString() ?? "null"} 个字符）。");
                }
                hantChars.Add(hantStr[0]);
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

    /// <summary>
    /// 读取排除清单（固定为 exclude.utf8.txt）：文件里可写普通字符，也可写
    /// U+XXXX；空白与 BOM 忽略。内容是从 Emperor 的 MAP 文件预先提取出来的。
    /// </summary>
    private static ISet<int> ReadExcludeFile(string filePath) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException(
                $"找不到排除清单 {Path.GetFullPath(filePath)}。"
                + "该文件应随程序一起放在同一目录（csproj 会把它复制到输出目录）。");
        }
        string raw = File.ReadAllText(filePath, new UTF8Encoding(false));
        HashSet<int> result = [];
        foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(raw, @"[Uu]\+([0-9A-Fa-f]{1,6})")) {
            result.Add(Convert.ToInt32(m.Groups[1].Value, 16));
        }
        foreach (char c in raw) {
            if (!char.IsWhiteSpace(c) && c != '\uFEFF') {
                result.Add(c);
            }
        }
        Console.WriteLine($"从 {Path.GetFileName(filePath)} 读到 {result.Count} 个已由其他字体提供的码位。");
        return result;
    }

    /// <summary>读取字符清单文件（每行若干字符，或纯文本），丢掉空白与 BOM。</summary>
    private static IReadOnlyList<char> ReadCodePointListFile(string filePath) {
        string text = File.ReadAllText(filePath, new UTF8Encoding(false));
        return text.Where(c => !char.IsWhiteSpace(c) && c != '\uFEFF').Distinct().ToList();
    }

    /// <summary>
    /// 排除清单的文件名。固定从程序目录读取，不通过命令行指定。
    /// 内容是从 Ui0001.RFH/RFD 里 FONT/ 下除 Chinese* 以外的所有 .MAP
    /// 预先提取出来的码位清单。
    /// </summary>
    private const string ExcludeFileName = "exclude.utf8.txt";

    /// <summary>
    /// 补漏清单，同样固定从程序目录读取。
    /// 里面列出的字符会被**无条件加入**最终输出 —— 用于收录常用字表里没有、
    /// 但游戏文本实际会用到、且名单又漏掉的字（例如繁体异体字
    /// 偽/為/眾/妳/墬）。它是在"排除其他字体已有字符"**之后**并入的，
    /// 所以补漏字不会被排除逻辑删掉。
    /// </summary>
    private const string ExtraFileName = "extra.utf8.txt";

    private static void Main() {
        Console.WriteLine("生成合并的简体 + 繁体常用字符表。");
        Console.WriteLine();

        List<char> chars = [];

        // ---------------------------------------------------------------
        // 第一部分：非汉字字符（区间定义，与原版一致）
        // ---------------------------------------------------------------

        // U+0020 -> U+007F: Basic Latin.
        chars.AddRange(GetUnicodeCharsInRange(0x0020, 0x007F));

        // U+0080 -> U+00FF: Latin-1 Supplement.
        chars.AddRange(GetUnicodeCharsInRange(0x0080, 0x00FF));

        // U+0100 -> U+017F: Latin Extended-A.
        chars.AddRange(GetUnicodeCharsInRange(0x0100, 0x017F));

        // Latin Extended-B. 0180–024F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x0180, 0x024F));

        // Greek and Coptic. Range: 0370–03FF. Excludes reserved characters.
        chars.AddRange(GetUnicodeCharsInRange(0x0370, 0x03FF, excluded: [0x0378, 0x0379, 0x0380, 0x0381, 0x0382, 0x0383, 0x038B, 0x038D, 0x03A2]));

        // U+0400 -> U+04FF: Cyrillic. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x0400, 0x04FF));

        // U+FF01 -> U+FFEE: Halfwidth and Fullwidth Forms.
        chars.AddRange(
            GetUnicodeCharsInRange(0xFF01, 0xFFEE, excluded: [0xFF00, 0xFFBF, 0xFFC0, 0xFFC1, 0xFFC8, 0xFFC9, 0xFFD0, 0xFFD1, 0xFFD8, 0xFFD9, 0xFFDD, 0xFFDE, 0xFFDF, 0xFFE7, 0xFFEF])
            .Where(c => c is < (char)0xFFA0 or > (char)0xFFDC) // Exclude Halfwidth Hangul variants
        );

        // General Punctuation
        chars.AddRange(GetUnicodeCharsInRange(0x2010, 0x2015)); // Dashes
        chars.AddRange(GetUnicodeCharsInRange(0x2016, 0x2017)); // General punctuation
        chars.AddRange(GetUnicodeCharsInRange(0x2018, 0x201F)); // Quotation marks and apostrophe
        chars.AddRange(GetUnicodeCharsInRange(0x2020, 0x2027)); // General punctuation
        chars.AddRange(GetUnicodeCharsInRange(0x2030, 0x2038)); // General punctuation
        chars.AddRange(GetUnicodeCharsInRange(0x203B, 0x203B)); // General punctuation

        // Enclosed CJK Letters and Months. Only GB2312 characters ㈠..㈩.
        chars.AddRange(GetUnicodeCharsInRange(0x3220, 0x3229));

        // Number Forms. 2160--216B and 2170--217F
        chars.AddRange(GetUnicodeCharsInRange(0x2160, 0x216B));
        chars.AddRange(GetUnicodeCharsInRange(0x2170, 0x217F));

        // Miscellaneous Symbols. Only GB2312 ones.
        chars.AddRange(GetUnicodeCharsInRange(0x2605, 0x2606));
        chars.AddRange(GetUnicodeCharsInRange(0x2640, 0x2640));
        chars.AddRange(GetUnicodeCharsInRange(0x2642, 0x2642));

        // Letterlike Symbols. Only ℃ (U+2103), № (U+2116), plus ℉ (U+2109).
        chars.AddRange(GetUnicodeCharsInRange(0x2103, 0x2103));
        chars.AddRange(GetUnicodeCharsInRange(0x2109, 0x2109));
        chars.AddRange(GetUnicodeCharsInRange(0x2116, 0x2116));

        // CJK Symbols and Punctuation. 3000–303F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x3000, 0x303F));

        // Hiragana (excluding reserved and voicing marks)
        chars.AddRange(GetUnicodeCharsInRange(0x3040, 0x309F, excluded: [0x3040, 0x3097, 0x3098, 0x3099, 0x309A, 0x309B, 0x309C]));

        // Katakana. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x30A0, 0x30FF));

        // Spacing Modifier Letters. 02B0–02FF. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x02B0, 0x02FF));

        // Mathematical Operators. 2200–22F1.
        chars.AddRange(GetUnicodeCharsInRange(0x2200, 0x22F1));

        // Geometric Shapes. 25A0–25EF.
        chars.AddRange(GetUnicodeCharsInRange(0x25A0, 0x25EF));

        // Enclosed Alphanumerics. 2460–24FF. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x2460, 0x24FF));

        // Bopomofo. 3105 -- 312F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x3105, 0x312F));

        // Box Drawing. 2500–257F. Includes all.
        chars.AddRange(GetUnicodeCharsInRange(0x2500, 0x257F));

        // Arrows. 2190–21EA.
        chars.AddRange(GetUnicodeCharsInRange(0x2190, 0x21EA));

        // Miscellaneous Technical. Only ⌒ (U+2312) in GB2312.
        chars.AddRange(GetUnicodeCharsInRange(0x2312, 0x2312));

        // 非汉字区间定义之间不应有重复（这是原版的保护性检查）。
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

        // 说明：本版本**不**并入 chineset.utf8.txt。
        // 原因是引擎对 Chinese 字体有 68 页的硬上限（见 README），
        // 而"10k 清单 + 原字库"会超出容量。实测不并入时，放弃的 8793 个字
        // 全部是生僻异体字（GB2312/通用规范汉字表里一个都没有），
        // 译文实际只用到其中 2 个，已由 extra.utf8.txt 补回。

        // 检查 GB2312 里所有非汉字字符都已包含
        {
            IReadOnlyList<char> unicodeHans = GetUnicode1994HanCharacters();
            IReadOnlyList<char> gb2312NonHanChars = GetGB2312Chars().Where(c => !unicodeHans.Contains(c)).ToList();
            IReadOnlyList<char> notIncludedChars = gb2312NonHanChars.Where(c => !chars.Contains(c)).ToList();
            if (notIncludedChars.Count > 0) {
                throw new Exception($"There are {notIncludedChars.Count} non-Han characters in GB2312 that haven't been added: {string.Join(", ", notIncludedChars.Select(DisplayCharacter))}");
            }
        }

        Console.WriteLine($"非汉字区间字符：{chars.Count} 个");

        // ---------------------------------------------------------------
        // 第二部分：简体 + 繁体汉字（原本由 --zh-Hant 二选一，现在合并输出）
        // ---------------------------------------------------------------
        IReadOnlyList<char> hansChars = GetHansChars();
        IReadOnlyList<char> hantChars = GetHantChars();
        Console.WriteLine($"简体汉字：{hansChars.Count} 个");
        Console.WriteLine($"繁体汉字：{hantChars.Count} 个");

        chars.AddRange(hansChars);
        chars.AddRange(hantChars);
        chars = chars.Distinct().ToList();
        Console.WriteLine($"合并去重后（未过滤）：{chars.Count} 个");

        // ---------------------------------------------------------------
        // 第三部分：去掉游戏里"其他语言字体"已经提供的字符
        // ---------------------------------------------------------------
        ISet<int> providedByOtherFonts = ReadExcludeFile(ExcludeFileName);

        List<char> removedChars = chars.Where(c => providedByOtherFonts.Contains(c)).ToList();
        chars = chars.Where(c => !providedByOtherFonts.Contains(c)).ToList();

        // ---------------------------------------------------------------
        // 第四部分：并入 extra.utf8.txt 里的补漏字符
        // 放在排除之后，所以补漏字**不会被排除逻辑删掉**。
        // ---------------------------------------------------------------
        List<char> extraChars = ReadCodePointListFile(ExtraFileName).ToList();
        if (extraChars.Count > 0) {
            HashSet<char> have = [.. chars];
            List<char> added = extraChars.Where(c => !have.Contains(c)).ToList();
            chars.AddRange(added);
            Console.WriteLine();
            Console.WriteLine($"从 {ExtraFileName} 补漏：{added.Count} 个字符" +
                              (added.Count > 0 ? "  " + string.Join(string.Empty, added) : ""));
        }

        Console.WriteLine();
        Console.WriteLine($"已在其他字体中提供、因此被移除的字符：{removedChars.Count} 个");
        Console.WriteLine("  " + string.Join(string.Empty, removedChars));
        Console.WriteLine();
        Console.WriteLine($"最终字符数：{chars.Count}");


        // ---------------------------------------------------------------
        // 输出
        // ---------------------------------------------------------------
        const string baseName = "TenThousandsCharsForEmperorBattleForDune";

        string plainPath = $"{baseName}{chars.Count}.txt";
        using (StreamWriter writer = new(plainPath, false, new UTF8Encoding(false))) {
            foreach (char ch in chars) {
                writer.Write(ch);
            }
        }

        string friendlyPath = $"{baseName}{chars.Count}_Friendly.txt";
        using (StreamWriter writer = new(friendlyPath, false, new UTF8Encoding(false))) {
            foreach (char ch in chars) {
                writer.Write(DisplayCharacter(ch));
                writer.WriteLine();
            }
        }

        string removedPath = $"{baseName}{chars.Count}_Removed.txt";
        using (StreamWriter writer = new(removedPath, false, new UTF8Encoding(false))) {
            writer.WriteLine($"# 以下 {removedChars.Count} 个字符已由其他字体（Korean / Standard / Small）提供，故未收录。");
            foreach (char ch in removedChars) {
                writer.Write(DisplayCharacter(ch));
                writer.WriteLine();
            }
        }

        Console.WriteLine();
        Console.WriteLine($"已写出：{plainPath}");
        Console.WriteLine($"        {friendlyPath}");
        Console.WriteLine($"        {removedPath}");
    }

}
