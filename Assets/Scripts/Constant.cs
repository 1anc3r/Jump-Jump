// 游戏状态
using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    // 游戏菜单
    Menu = 0,
    // 游戏初始化
    Init,
    // 生成平台
    Generate,
    // 平台坠落
    Fall,
    // 按压平台
    Fold,
    // 释放平台
    Free,
    // 玩家跳跃
    Jump,
    // 游戏结束
    Over
}

public static class Palette
{
    public static Color32 Red = new Color32(243, 68, 54, 255);
    public static Color32 Pink = new Color32(232, 30, 100, 255);
    public static Color32 Purple = new Color32(156, 38, 175, 255);
    public static Color32 DeepPurple = new Color32(104, 59, 182, 255);
    public static Color32 Indigo = new Color32(62, 82, 180, 255);
    public static Color32 Blue = new Color32(33, 150, 242, 255);
    public static Color32 LightBlue = new Color32(5, 168, 243, 255);
    public static Color32 Cyan = new Color32(6, 187, 211, 255);
    public static Color32 Teal = new Color32(5, 150, 135, 255);
    public static Color32 Green = new Color32(78, 174, 80, 255);
    public static Color32 LightGreen = new Color32(139, 194, 75, 255);
    public static Color32 Lime = new Color32(204, 219, 58, 255);
    public static Color32 Yellow = new Color32(254, 234, 60, 255);
    public static Color32 Amber = new Color32(254, 192, 11, 255);
    public static Color32 Orange = new Color32(254, 152, 1, 255);
    public static Color32 DeepOrange = new Color32(254, 87, 34, 255);
}