# TenThousandsCharsForEmperorBattleForDune

Forked from [SadPencil/10ThousandsCharsForSpriteFont](https://github.com/SadPencil/10ThousandsCharsForSpriteFont),
this repo generates the character list needed by the bitmap fonts of
*Emperor: Battle for Dune*.

Three changes from the original:

1. **Simplified and Traditional are merged into one output.** The original chose
   between them with `--zh-Hant`; this version emits the deduplicated union in a
   single character list file.
2. **Characters the game already provides are excluded.** It reads the bundled
   `exclude.utf8.txt`, which lists every character supported by the game's fonts
   other than `Chinese`, so those characters are not generated again.
3. **The original `Chinese` font's characters are included.** It reads the bundled
   `chineset.utf8.txt`, which lists every character the game's original `Chinese`
   font already covers. The generated list **replaces** that font rather than
   extending it, so those characters must be generated too — otherwise they would
   turn into blanks. A self-check aborts the run if any of them is missing.

## Usage

```
TenThousandsCharsForEmperorBattleForDune.exe
```

## Output

| File | Contents |
|---|---|
| `<ProjectName><count>.txt` | Plain character list (UTF-8, no BOM), characters written back to back |
| `<ProjectName><count>_Friendly.txt` | One character per line with its codepoint, e.g. `永 (U+6C38)` |
| `<ProjectName><count>_Removed.txt` | Characters dropped because another game font already provides them |

## exclude.utf8.txt

`exclude.utf8.txt` (UTF-8, no BOM, characters back to back, 2469 codepoints) was
pre-extracted from every `.MAP` in the `FONT/` directory inside the game's
`DATA/Ui0001.RFH` / `Ui0001.RFD` archive, **except `Chinese*`**:

14 `.MAP` files (12 with content, 2 empty tables):

| Source | Bytes | Codepoints |
|---|---|---|
| `KOREAN0..8.MAP` | 514 x9 | 2304 (Hangul syllables) |
| `KOREAN9.MAP` | 119 | 0 (all-zero empty table) |
| `STANDARD.MAP` | 332 | 165 (ASCII / Latin-1) |
| `SMALL.MAP` `SMALL0.MAP` | 312 x2 | 155 (ASCII / Latin-1 subset) |
| `SMALL1.MAP` | 34 | 0 (all-zero empty table) |
| **union** | | **2469** |

`.MAP` parsing: UTF-16LE (optional BOM), 2 bytes per codepoint, no trailing NUL,
`U+0000` means an empty slot. A trailing odd byte is discarded
(`KOREAN9.MAP` is 119 bytes long).

For the merged Simplified + Traditional list this removes **165 characters**
(ASCII letters/digits, Latin-1 punctuation and symbols). The Hangul syllables do
not overlap the list, so they account for nothing.

## chineset.utf8.txt

`chineset.utf8.txt` (UTF-8, no BOM, characters back to back, 17270 codepoints) was
pre-extracted from the `CHINESE0..67.MAP` files inside the same archive — the
complete character set of the game's original `Chinese` font.

Since this tool's output **replaces** that font, every character listed here is
added to the generated list first. The program verifies this at the end and
aborts with an error if any of them would be missing.

## Platform

Target framework is `net48` and it **only runs on Windows** — the
Simplified/Traditional conversion relies on
`Microsoft.VisualBasic.Strings.StrConv` (which uses Windows NLS).
On non-Windows platforms it degrades to the OpenCC `STCharacters` mapping only,
producing fewer Traditional characters.

---

## Original description

This repo serves a selected area of Chinese (Simplified) characters as well as other characters frequently used by Chinese people, in order to fit the ~10000 character sprite font limit of the XNA Reach profile.

The following Chinese (Simplified) characters (of which there are approximately 8,000 Han characters) are selected:
- Characters defined in GB2312-90
- Characters defined in Tong Yong Gui Fan Han Zi Biao **AND** included in Unicode 1.1
- General Punctuation (unclassified general punctuation, dashes, quotation marks and apostrophe included, the rest excluded)
- CJK Symbols and Punctuation

It also contains the following characters:
- Basic Latin
- Latin-1 Supplement
- Latin Extended-A
- Latin Extended-B
- Greek and Coptic
- Cyrillic
- Halfwidth and Fullwidth Forms (Hangul excluded)
- Hiragana
- Katakana
- Spacing Modifier Letters
- Mathematical Operators
- Geometric Shapes
- Enclosed Alphanumerics
- Box Drawing
- Arrows
- Bopomofo

To get the list of characters, download the text file in the [Releases](https://github.com/SadPencil/10ThousandsCharsForSpriteFont/releases) page.

## References

### GB2312-90

An influential, former official Chinese character set. The official electronic document is in a non-machine-readable format (scanned image), so a third-party transcription is used: https://github.com/StarCompute/GB2312Font)

### Tong Yong Gui Fan Han Zi Biao

The current official Standard Chinese Characters List, issued by the State Council. The official electronic document is in a non-machine-readable format (scanned image), so a third-party transcription is used: https://github.com/shengdoushi/common-standard-chinese-characters-table

### The Unicode Han Character Cross-Reference, Version 1.1

https://www.unicode.org/Public/1.1-Update/CJKXREF.TXT

### Comparison Table of Seven Research Results on Non-One-to-One Correspondences between Simplified and Traditional Chinese Characters

CHEN, Mingran. *Comparison Table of Seven Research Results on Non-One-to-One Correspondences between Simplified and Traditional Chinese Characters*. Presented at the 12th Chinese Character Standardization Academic Seminar, Qinhuangdao, China, August 2009.

### OpenCC

OpenCC's STCharacters database contains the mapping relationship between Simplified Chinese and Traditional Chinese characters.