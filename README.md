# 雲端檔案管理系統

以物件導向領域模型管理記憶體中的目錄／檔案樹。驗證方式為 **Console 固定官方劇本**（非互動 CLI）：啟動後依序走真實 Visitor 與 Command 印出結果。可修改 `FileSystem.Console/Program.cs` 或 `FileSystem.Domain/SampleTreeFactory.cs` 後重跑以驗證 Domain。

規格見 [`REQUIREMENTS.md`](REQUIREMENTS.md)；開發規則見 [`.cursorrules`](.cursorrules)。

## 環境需求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## 還原與建置

於儲存庫根目錄：

```powershell
dotnet restore FileSystem.sln
dotnet build FileSystem.sln
```

## 測試

```powershell
dotnet test FileSystem.sln
```

測試專案為 `FileSystem.Tests`（xUnit + FluentAssertions），涵蓋必備功能與功能四（排序、刪除／複製貼上、標籤、Undo／Redo）。

## Console Demo

```powershell
dotnet run --project FileSystem.Console
```

固定劇本會依序印出（摘要）：

| 段落 | 內容 |
|---|---|
| 1) 目錄結構 | `PrintVisitor` 樹狀輸出（`├──`／`└──`、`[目錄]`／`[子目錄]`） |
| 2) Root 總容量 | `CalculateSizeVisitor` 的 `TotalSize` 與 Trail；並示範任一目錄（專案文件）容量 |
| 3) 搜尋 | `.docx`、`.png` 路徑與 Trail |
| 4) XML | `XmlExportVisitor`（含 `<Archive_2025>`） |
| 5) 排序 | 專案文件：名稱升冪、大小降冪、副檔名升冪後再印樹 |
| 6) 刪除與複製貼上 | 刪除前後樹、複製貼上後樹 |
| 7) 貼標與移除 | 節點標籤與顏色（Urgent／Work／Personal） |
| 8) Undo／Redo | `CommandHistory`：Execute → Undo → Redo |

不提供選單或互動輸入。

## 專案結構

```
cloud-file-management-system/
├── FileSystem.sln
├── FileSystem.Domain/     # 領域模型、Visitor、Strategy、Command（零 NuGet）
├── FileSystem.Console/    # 官方 Demo 劇本
├── FileSystem.Tests/      # Domain 單元測試
├── docs/
│   ├── class-diagram.md   # 目標類別圖（Mermaid）
│   └── er-model.md        # 目標 ER（Schema 文件；執行期為記憶體）
├── REQUIREMENTS.md
└── .cursorrules
```

依賴方向：`Console`／`Tests` → `Domain`。尚未建立 `FileSystem.Web` 或前端專案。

## 設計與 Pattern

實作對齊目標圖：

- [docs/class-diagram.md](docs/class-diagram.md) — 類別圖（含功能四）
- [docs/er-model.md](docs/er-model.md) — ER Schema 設計文件（非資料庫實作）

| Pattern | 用途 |
|---|---|
| **Composite** | `FileSystemNode` ← `Directory`／`File`（Word／Image／Text）；目錄聚合 `Children` |
| **Visitor** | `PrintVisitor`、`CalculateSizeVisitor`（對外單一 `TotalSize`）、`SearchVisitor`、`XmlExportVisitor`；每步 Trail：`Visiting: {parent} -> {node}` |
| **Strategy** | `ISortStrategy`：名稱／大小／副檔名，升冪與降冪；作用於目錄 `Children` |
| **Prototype** | `FileSystemNode.Clone()` 深拷貝子樹並產生新 `Id` |
| **Command** | `DeleteNodeCommand`、`CopyPasteNodeCommand`、`SortCommand`、`TagCommand` |
| **CommandHistory** | Undo／Redo 兩個堆疊；`CanUndo`／`CanRedo` |

容量：檔案以 bytes 儲存；走訪加總後格式化為 `TotalSize`（如 `500KB`、`2MB`、`2815476B`）。

## 領域重點

- **單位**：1 KB = 1024 bytes；1 MB = 1024² bytes。
- **Seed**：由 `SampleTreeFactory.Create()` 建立題目範例樹。
- **Archive 節點**：Print 顯示名為 `2025備份 (Archive_2025)`（並標 `[子目錄]`）；XML 目錄 tag 仍為 `<Archive_2025>`（兩者都要滿足）。
- **不變量**：Root 的 `Parent` 為 null；檔案入樹後必屬目錄；`Add` 拒絕成環；禁止刪除 Root。
- **標籤**：Urgent（紅）、Work（藍）、Personal（綠）；節點上可呼叫 `AddTag`／`RemoveTag`／`HasTag`。

## 後續加分

以下**尚未實作**，僅為規格中的加分階段：

- `FileSystem.Web`：ASP.NET Core Minimal API（暴露已存在的 Domain，含 `traverseLog`）
- Vue 3 + Axios 前端（`FileTree`、`VisitorPanel`、`TraverseConsole` 等）

目前請以 `dotnet test FileSystem.sln` 與 `dotnet run --project FileSystem.Console` 驗收功能一～四。
