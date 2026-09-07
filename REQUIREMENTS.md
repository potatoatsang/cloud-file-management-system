# 雲端檔案管理系統 — 需求規格

實作時以本檔與 `.cursorrules` 為準。範圍與開發順序只寫在這兩份文件（SDD）。必備功能的 Domain 必須先寫 xUnit 再實作；進階功能一旦列入實作，同樣必須先寫 xUnit 再實作（TDD）。

## 1. 目標與交付

作業重點是物件導向領域模型與核心邏輯。原文作業不強制 GUI；**Console 印出文字即可**。面試官驗證方式是修改 `Program.cs` 或 `SampleTreeFactory.cs` 後重新執行，**不是**互動選單。樹與檔案存在於記憶體物件圖，不是磁碟路徑。

工作順序（SA）：先依本規格推導並繪製**含功能四的目標** UML／ER，再實作功能一～四對齊該圖，然後寫 README，**Web UI 為最後加分項**（暴露已存在的 Domain，不是先做 UI 再補邏輯）。

繳交：

- 程式碼上傳 GitHub（repo：`cloud-file-management-system`）並回傳連結
- 概述設計與實作概念（README）
- UML 類別圖、ER Model（Mermaid，見第 8 節）

## 2. 技術選型

| 項目 | 選型 |
|---|---|
| 執行環境 / 語言 | .NET 8、C# 12 |
| 專案 | `FileSystem.Domain`、`FileSystem.Console`、`FileSystem.Web`、`FileSystem.Tests` |
| 資料 | 記憶體物件樹、`SampleTreeFactory` |
| 測試 | xUnit + FluentAssertions |
| API | ASP.NET Core Minimal API（最後加分階段） |
| 前端 | Vue 3（`<script setup>`）、TypeScript、Tailwind CSS、Axios（最後加分階段） |
| 方法 | SDD 決定範圍與順序；必備 Domain 與列入實作的進階功能皆 TDD |

## 3. 領域規則

來自需求訪談：

- 管理 Word文件、圖片、純文字檔；目錄可無限層巢狀（如同資料夾）
- 共同屬性：檔名、大小、建立時間
- Word文件：頁數；圖片：解析度寬與高；純文字：編碼（如 UTF-8）
- 目錄有名稱；所有檔案都必須放在某個目錄下。Root 本身是目錄，故根下的 `README.txt` 合法

模型要點：

- Composite：`Directory` 與 `File` 皆為 `FileSystemNode`
- `File` 子型別：`WordFile`、`ImageFile`、`TextFile`
- 大小內部存 `long` bytes；**1 KB = 1024 bytes**；2 MB = 2 × 1024 × 1024
- 顯示：500B、1KB、500KB、2MB
- 能力在 Domain：任一目錄可 `Accept` Visitor；總容量經 Visitor 得到單一結果 `TotalSize`（含單位的顯示字串）；任意副檔名以 `SearchVisitor` 搜尋。`PrintVisitor` 與 Console Demo 必須走真實 Visitor，禁止寫死預期字串。

## 4. 範例結構（Seed）

初始化必須建立下列物件（題目「三、客戶提供的範例結構圖」）。**樹狀顯示名稱**須能對照下列 plaintext（含 `├──`、`└──` 與 `[目錄]`／`[子目錄]`；icon 可忽略）：

```
根目錄 (Root)
├── 專案文件 (Project_Docs) [目錄]
│   ├── 需求規格書.docx [Word 檔案] (頁數: 15, 大小: 500KB)
│   └── 系統架構圖.png [圖片] (解析度: 1920x1080, 大小: 2MB)
├── 個人筆記 (Personal_Notes) [目錄]
│   ├── 待辦清單.txt [純文字檔] (編碼: UTF-8, 大小: 1KB)
│   └── 2025備份 (Archive_2025) [子目錄]
│       └── 舊會議記錄.docx [Word 檔案] (頁數: 5, 大小: 200KB)
└── README.txt [純文字檔] (編碼: ASCII, 大小: 500B)
```

節點「2025備份 (Archive_2025)」：

- **顯示名稱（樹狀 Print）**：中英並列 `2025備份 (Archive_2025)`，並標 `[子目錄]`
- **XML 目錄 tag**：仍為 `<Archive_2025>`（與題目 XML 範例一致）

兩者都必須滿足，不可只為 XML 把樹印成僅有 `Archive_2025`。

bytes 換算：500KB = 500 × 1024；2MB = 2 × 1024²；1KB = 1024；500B = 500。

## 5. 必備功能

### 功能一：目錄結構呈現

1. 依第 4 節建立物件實例
2. 以 `PrintVisitor` 顯示完整目錄結構與檔案詳細資訊（頁數、解析度、編碼、大小）
3. 輸出須能對照第 4 節 plaintext 的樹枝符號與目錄標示

### 功能二：核心邏輯

**遞迴計算總容量**  
可計算**任一目錄**及其下所有子目錄與檔案的總大小。對外 Demo 經 Visitor 執行，以便產生遍歷紀錄。結果為單一欄位 **`TotalSize`**（含單位的顯示字串，如 `500KB`、`2MB`、`500B`），**不得**再拆成並列的 `TotalBytes` 與 `DisplaySize`。檔案與走訪加總的內部單位仍為 bytes（1 KB = 1024）；格式化後才成為 `TotalSize`。

**副檔名搜尋**  
`SearchVisitor` 接受任意副檔名（`.` 可省略），列出該結構下所有符合條件的檔案路徑。範例樹搜尋 `.docx` 應得到 2 筆：需求規格書、舊會議記錄。

**XML 結構輸出**  
將目錄結構轉成 XML。預期格式：

```xml
<根目錄_Root>
    <專案文件_Project_Docs>
        <需求規格書_docx>頁數: 15, 大小: 500KB</需求規格書_docx>
        <系統架構圖_png>解析度: 1920x1080, 大小: 2MB</系統架構圖_png>
    </專案文件_Project_Docs>
    <個人筆記_Personal_Notes>
        <待辦清單_txt>編碼: UTF-8, 大小: 1KB</待辦清單_txt>
        <Archive_2025>
            <舊會議記錄_docx>頁數: 5, 大小: 200KB</舊會議記錄_docx>
        </Archive_2025>
    </個人筆記_Personal_Notes>
    <README_txt>編碼: ASCII, 大小: 500B</README_txt>
</根目錄_Root>
```

Tag 規則：目錄有中英並列且非上述 Archive 例外時用 `中文_英文`；檔案用 `主檔名_副檔名`。Archive 子目錄 XML tag 固定 `Archive_2025`。

### 功能三：功能進度追蹤

題目範例為單行路徑；本專案採用**逐步邊紀錄**：每 Visit 追加 `Visiting: {parentName} -> {node.Name}`。Root 可用 `(root) -> 根目錄`。節點名使用物件 `Name`（中文顯示名或 Seed 之 Archive 規則）。Console 在**計算大小與搜尋之後**印出 Trail。不要求做成互動 CLI。

### Console Demo 定位

`FileSystem.Console` 為**固定官方劇本**：啟動後依序走真實 Visitor，印出功能一～三（進階階段再加功能四劇本）。不提供互動選單。面試官可改 `Program.cs` 或 `SampleTreeFactory.cs` 後重跑以驗證 Domain。

### 必備功能驗收

執行 Console（或等價測試）須同時滿足：

1. 印出／斷言範例樹（含 `├──`／`└──`、`[目錄]`／`[子目錄]`、`2025備份 (Archive_2025)`）與 metadata
2. Root 的 `TotalSize` 正確（含 1024 進位與 B／KB／MB 顯示）
3. 搜尋 `.docx` 得到上述 2 條路徑
4. XML 含 `<根目錄_Root>`、`<Archive_2025>` 與範例節點內文
5. 計算大小與搜尋皆有完整逐步 Trail

`dotnet test` 必須全綠。

## 6. 進階功能 (Bonus)

題目「實作功能四」。類別與 ER 標籤已出現在目標圖中；於必備功能可經 Console／測試 Demo 之後、**README 與 Web 之前**實作並對齊該圖。驗收以 **Domain 測試 + Console 劇本 Demo** 為準（排序後印樹、刪除／貼上、標籤、undo／redo），不依賴 UI。

1. **排序**：依名稱、大小、副檔名，升冪或降冪（Strategy）
2. **編輯**：刪除、複製／貼上（Clone + Command）
3. **標籤**：Urgent（紅）、Work（藍）、Personal（綠），支援多重標籤；必須提供可呼叫的**貼標／移除** API，不能只有 enum 與私有集合
4. **Undo / Redo**：操作可復原與重做（CommandHistory）

列入實作時：先寫 xUnit，再寫 Domain，再於 Console 加固定劇本。

## 7. API 契約（最後加分：Web）

本節僅在 README 之後、最後加分階段適用。API 暴露**已存在**的 Domain（必備 + 已做的進階），不改演算法。Visitor 結果必須帶遍歷紀錄，例如：

```csharp
record VisitorResultDto<T>(T Data, IReadOnlyList<string> TraverseLog);
```

參考端點：

| 方法 | 路徑 | 說明 |
|---|---|---|
| GET | `/api/tree` | 整棵樹 |
| POST | `/api/visitors/size` | `TotalSize` + traverseLog |
| POST | `/api/visitors/search` | 副檔名搜尋 + traverseLog |
| POST | `/api/visitors/xml` | XML 字串 + traverseLog |

進階已做時再補 delete、paste、sort、tag、undo、redo。啟動時載入 Seed，Root 置於記憶體單例。

前端（最後加分）：`FileTree`、`VisitorPanel`、`TraverseConsole`，進階再加工具列。以 Axios 呼叫後端；運算在後端。

## 8. UML 與 ER

在實作功能一～四**之前**繪製**目標模型**（含功能四），實作必須對齊此圖。

- 類別圖：`docs/class-diagram.md`（Mermaid classDiagram）— 繼承、關聯／聚合；含 Visitor、Tag、Sort Strategy、Command／CommandHistory、Clone。計算總容量 Visitor 只暴露 `TotalSize`，不得並列 `TotalBytes` 與 `DisplaySize`
- ER：`docs/er-model.md`（Mermaid erDiagram）— 目錄自參照、檔案必須隸屬目錄、Word／Image／Text 子型別、標籤多對多。Visitor／Strategy／Command 為行為，不強制成為資料表

ER 為作業要求的 Schema 設計文件，與記憶體實作並存。程式對齊圖時若發現圖有誤，先改圖再改碼。

## 9. 開發生命週期

順序：

1. 規格（本檔與 `.cursorrules`）
2. 目標 UML／ER（`docs/class-diagram.md`、`docs/er-model.md`，含功能四）
3. 實作功能一～三：Domain TDD → `FileSystem.Console` 官方劇本（對齊圖）
4. 實作功能四（Bonus）：排序 → 刪除／複製貼上 → 標籤 → Undo／Redo；先測再寫；Console 加劇本（對齊圖）
5. `README.md`（還原、測試、Console 執行、設計與 pattern；對齊已實作）
6. `FileSystem.Web` Minimal API（不改 Domain 演算法）
7. Vue + Axios：FileTree → VisitorPanel → TraverseConsole → 進階工具列

硬性規則：

- 未完成步驟 2（目標圖）前，不開始步驟 3–4 的新開發。
- 未完成步驟 3–4 前，不定稿 README、不開始步驟 6–7。
- 未完成步驟 2–4 前，不建立 Vue。
- Web 是最後加分，用來暴露已存在的 Domain。

對應的 commit 主題（僅在明確要求時提交）：

1. `docs:` 規格與 Cursor 規則
2. `docs:` UML 類別圖與 ER
3. `feat(domain):`／`feat(console):` 必備功能
4. `feat:` 進階功能
5. `feat(web):` Minimal API
6. `feat(frontend):` Vue

README 於功能一～四完成後、Web 之前以 `docs:` 提交（可與進階同一階段之後、獨立 commit）。訊息格式：Conventional Commits，說明使用繁體中文。

## 10. 完成定義

- 必備功能：`dotnet test` 全綠；Console 劇本可 Demo 樹狀結構（對照 plaintext）、`TotalSize` 與 Trail、搜尋 `.docx` 與 Trail、XML（含 `<Archive_2025>`）
- 進階功能（若實作）：Domain 測試全綠；Console 劇本可 Demo 排序後印樹、刪除／貼上、貼標／移除、undo／redo
- 文件：目標 UML／ER 先於功能一～四存在；README 於實作 1–4 之後補齊執行與 pattern 說明
- Web／Vue：可選加分；若實作才驗：樹可見、Visitor 操作、畫面顯示後端 `traverseLog`、已做的進階工具列
