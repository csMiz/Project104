﻿# 游戏数据记录格式

## 头

固定4位字符"PSAV"

50 53 41 56

对应4位工程代号"0104"

30 31 30 34

随机数4位

?? ?? ?? ??

保留用途16位

00 00 00 00

此文件有效内容长度4位，即文件总长度减去16

?? ?? ?? ??

## 内容

内容部分总长度4位

?? ?? ?? ??

用户名16位

00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 

权限组4位，使用字符缩写，如"Retail"->"RETA"

52 45 54 41

偏好语言4位代码，默认英文"ENGL"

45 4E 47 4C

窗口模式1位

00

分辨率4位

?? ?? ?? ??

FPS数1位，默认30

1E




## 系统数据

系统数据总长度4位

?? ?? ?? ??

成就状态8位，每一个bit指代一个Boolean

00 00 00 00 00 00 00 00

成就校验码1，8位，算法为"成就状态"8x8进行一次"Game of life"后的结果

00 00 00 00 00 00 00 00

成就校验码2，4位，算法为从字母大A开始与成就一一对应，计算总和，再乘以(64-已达成成就数)

00 00 00 00

游戏总时长4位，类型为Single，按分钟计

00 00 00 00

游戏总时长校验码，算法为"游戏总时长"与"成就校验码1"后四位计算Xor，与随机数求和，再进行一次8*4的"Game of life"

?? ?? ?? ??






