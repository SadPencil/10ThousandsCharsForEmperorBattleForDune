# TenThousandsCharsForEmperorBattleForDune

本仓库从 [SadPencil/10ThousandsCharsForSpriteFont](https://github.com/SadPencil/10ThousandsCharsForSpriteFont)
派生，用于生成 *Emperor: Battle for Dune*（帝王：沙丘之战）点阵字体所需的字符清单。

相对原版的改动：

1. **简繁合并输出**：原版用 `--zh-Hant` 在简体/繁体之间二选一，本版一次输出两者合并去重后的结果，
   只产出一个字符清单文件。
2. **排除游戏已有字符**：读取 `exclude.utf8.txt`，其中列出的字符是游戏内除了 Chinese 以外的其他字体所支持的全部字符。
3. **包含原 Chinese 字库的字符**：读取 `chineset.utf8.txt`，其中列出了游戏原本的繁体中文 `Chinese`
   字体覆盖的全部字符。本工具的输出是**替换**该字体而非追加，所以这些字必须一并生成，
   否则替换后会变成空白。程序末尾有覆盖自检，若有遗漏会直接报错中止。

## 用法

```
TenThousandsCharsForEmperorBattleForDune.exe
```

## 输出

| 文件 | 内容 |
|---|---|
| `<项目名><字符数>.txt` | 纯字符清单（UTF-8 无 BOM），每字符直接相连 |
| `<项目名><字符数>_Friendly.txt` | 每行一个字符 + 码位，形如 `永 (U+6C38)` |
| `<项目名><字符数>_Removed.txt` | 因游戏其他字体已提供而被移除的字符清单 |

## exclude.utf8.txt

`exclude.utf8.txt`（UTF-8 无 BOM，字符直接相连，2469 个码位）由 Emperor 的
`DATA/Ui0001.RFH` / `Ui0001.RFD` 归档中 `FONT/` 目录下**除 `Chinese*` 以外**
的所有 `.MAP` 预先提取而成：

14 个 `.MAP`（12 个有内容 + 2 个空表）：

| 来源 | 字节 | 提供码位 |
|---|---|---|
| `KOREAN0..8.MAP` | 514 ×9 | 2304（谚文音节） |
| `KOREAN9.MAP` | 119 | 0（全零空表） |
| `STANDARD.MAP` | 332 | 165（ASCII / Latin-1） |
| `SMALL.MAP` `SMALL0.MAP` | 312 ×2 | 155（ASCII / Latin-1 子集） |
| `SMALL1.MAP` | 34 | 0（全零空表） |
| **并集** | | **2469** |

`.MAP` 格式：UTF-16LE（可带 BOM），每码位 2 字节，无结尾 NUL，`U+0000` 表示空槽；
末尾若有多余的半个码元会被丢弃（`KOREAN9.MAP` 就是 119 字节的奇数长度）。

对简体 + 繁体清单实际生效的剔除量是 **165 个**（ASCII 字母数字、Latin-1 标点符号等）；
谚文音节与清单不重叠，所以不计入。

## chineset.utf8.txt

`chineset.utf8.txt`（UTF-8 无 BOM，字符直接相连，17270 个码位）由同一归档中的
`CHINESE0..67.MAP` 预先提取而成，即游戏原 `Chinese` 字体的全部字符。

因为本工具的输出是**替换**该字体，这里列出的每个字符都会先被加进生成清单。
程序在末尾会校验覆盖完整性，若有遗漏会直接报错中止。

## 平台

目标框架为 `net48`，**只能在 Windows 上运行** —— 简繁转换依赖
`Microsoft.VisualBasic.Strings.StrConv`（走 Windows NLS）。
非 Windows 平台运行时会自动退化为只使用 OpenCC 的 `STCharacters` 映射表，
结果会少一部分繁体字。

---

## 原版说明

本代码库精选了简体中文常用汉字及其他高频字符，旨在满足 XNA Reach 配置方案下约一万个字符的精灵字体限制的要求。

所选简体中文字符（其中，汉字约 8000 个）包括：
- GB2312-90 标准定义的字符
- 《通用规范汉字表》收录**且** Unicode 1.1 收录的汉字
- 通用标点（包含未分类的通用标点、破折号、引号和撇号，其余排除）
- CJK 符号和标点

同时包含以下字符：
- 基础拉丁字母
- 拉丁语补充-1
- 拉丁字母扩展-A
- 拉丁字母扩展-B
- 希腊字母和科普特字母
- 西里尔字母
- 半角及全角字符（排除韩文字母）
- 平假名
- 片假名
- 间距修饰字母
- 数学运算符
- 几何形状
- 带圈字母数字
- 制表符
- 箭头符号
- 汉语拼音

字符列表文件可在[发布页面](https://github.com/SadPencil/10ThousandsCharsForSpriteFont/releases)下载获取。

## 参考资料

### GB2312-90

一套具有影响力的前官方中文字符集。官方版本不可机读（扫描件），故使用第三方文本：https://github.com/StarCompute/GB2312Font

### 通用规范汉字表

由国务院发布的现行官方规范汉字表。官方版本不可机读（扫描件），故使用第三方文本：https://github.com/shengdoushi/common-standard-chinese-characters-table

### Unicode 汉字交叉参照表（1.1 版）

https://www.unicode.org/Public/1.1-Update/CJKXREF.TXT

### 七份非一一对应简繁字研究成果比较表

陈明然. 七份非一一对应简繁字研究成果比较表[C]// 第十二次汉字书同文学术研讨会, 中国, 秦皇岛, 2009年8月.

### OpenCC

OpenCC 的 STCharacters 数据库中包含了简体中文和繁体中文的字符映射关系。