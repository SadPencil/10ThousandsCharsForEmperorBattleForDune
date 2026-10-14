# 万字精灵字符集

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
