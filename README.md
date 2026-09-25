# Gearsetter（繁中移植版 · TC13） / Traditional-Chinese Port

> 找出裝備升級。<br>
> Find gear upgrades.

**繁體中文**：這是 **[Gearsetter](https://git.carvel.li/liza/Gearsetter)** 的繁體中文客戶端移植版，對應 **FFXIV 7.20 / yanmucorp Dalamud API13（.NET 9）**。本專案僅做相容性移植，**非官方、非原作維護**；所有原始功能與設計著作權歸原作者 **Liza Carvelli**。

**English**: A Traditional-Chinese-client port of **[Gearsetter](https://git.carvel.li/liza/Gearsetter)** targeting **FFXIV 7.20 / yanmucorp Dalamud API13 (.NET 9)**. Compatibility port only — **unofficial and not maintained by the original author**. All original work © **Liza Carvelli**.

---

## 這是什麼 / About

瀏覽各職業／工作可用的所有裝備，並標示出目前裝備的可能升級選項。

Browse every gear piece available for each class/job and highlights possible upgrades over what you have equipped.

## 安裝 / Installation

**繁體中文**
1. 使用 **XIVTCLauncher** 啟動繁體中文客戶端。
2. 遊戲內輸入 `/xlsettings` → 切到 **Experimental** 分頁 → **Custom Plugin Repositories（自訂插件庫）**。
3. 貼上下列網址並按 **+** 儲存：
   ```
   https://raw.githubusercontent.com/lilasrepo/DalamudPlugins/main/pluginmaster.json
   ```
4. 輸入 `/xlplugins`，搜尋 **Gearsetter (TC13)** → 安裝 → 啟用。

**English**
1. Launch the Traditional-Chinese client with **XIVTCLauncher**.
2. In-game, type `/xlsettings` → **Experimental** tab → **Custom Plugin Repositories**.
3. Add this URL and save with **+**:
   ```
   https://raw.githubusercontent.com/lilasrepo/DalamudPlugins/main/pluginmaster.json
   ```
4. Type `/xlplugins`, search **Gearsetter (TC13)** → Install → Enable.

## 對應版本 / Compatibility

| 項目 / Item | 版本 / Version |
|---|---|
| 遊戲 / Game | FFXIV 7.20（繁中客戶端 / TC client） |
| Dalamud | yanmucorp API13（.NET 9） |
| 移植自上游 / Ported from upstream | v4.3 |

## 原作與授權 / Credits & License

本專案 fork 自 **[liza/Gearsetter](https://git.carvel.li/liza/Gearsetter)**，授權沿用上游；所有原始功能著作權歸 **Liza Carvelli**。<br>
Forked from **[liza/Gearsetter](https://git.carvel.li/liza/Gearsetter)**. License follows upstream; all original work © **Liza Carvelli**.

## 免責聲明 / Disclaimer

第三方插件，使用風險自負。**移植相關問題請回報到本 repo 的 Issues，請勿打擾上游原作者。**<br>
Third-party plugin — use at your own risk. **For port-specific issues please open an Issue here; do not contact the upstream author.**
