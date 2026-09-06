# 雲端檔案管理系統 — 需求規格

實作時以本檔與 `.cursorrules` 為準。

## 1. 目標與交付

作業重點是物件導向領域模型與核心邏輯。結果可用 Console 驗證；Web UI 為本專案預設的加分交付，但必須在 Console 已能 Demo 必備功能之後才開始。

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
| API | ASP.NET Core Minimal API |
| 前端 | Vue 3（`<script setup>`）、TypeScript、Tailwind CSS、Axios |
| 方法 | SDD 決定範圍與順序；必備功能的 Domain 採 TDD |

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

## 4. 範例結構（Seed）

初始化必須建立下列物件（題目「三、客戶提供的範例結構圖」）：

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

bytes 換算：500KB = 500 × 1024；2MB = 2 × 1024²；1KB = 1024；500B = 500。

## 5. 必備功能

### 功能一：目錄結構呈現

1. 依第 4 節建立物件實例
2. 顯示完整目錄結構與檔案詳細資訊（頁數、解析度、編碼、大小）

Console 輸出應可對照第 4 節樹狀文字（icon 可忽略）。

### 功能二：核心邏輯

**遞迴計算總容量**  
可計算任一目錄及其下所有子目錄與檔案的總大小。對外 Demo 經 Visitor 執行，以便產生遍歷紀錄。

**副檔名搜尋**  
輸入如 `.docx`（點可省略），列出該結構下所有符合條件的檔案路徑。範例樹應得到 2 筆：需求規格書、舊會議記錄。

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

Tag 規則：目錄有中英時用 `中文_英文`；檔案用 `主檔名_副檔名`。

### 功能三：功能進度追蹤

執行「計算大小」或「搜尋」時，必須提供當前訪問節點順序（Traverse Log），以證明走訪結構。範例形式：

`Visiting: Root -> Project_Docs -> 需求規格書.docx`

實作上將每步寫入 Visitor Trail，再由 Console 或 API 輸出；前端顯示 API 回傳的 log，不自行遍歷。

### 必備功能驗收

執行 Console（或等價測試）須同時滿足：

1. 印出／斷言範例樹與 metadata
2. Root 總容量正確（含單位換算）
3. 搜尋 `.docx` 得到上述 2 條路徑
4. XML 含 `<根目錄_Root>` 與範例節點內文
5. 計算大小與搜尋皆有完整 Traverse Log

`dotnet test` 必須全綠。

## 6. 進階功能 (Bonus)

題目「實作功能四」，於必備功能可 Demo 之後實作：

1. **排序**：依名稱、大小、副檔名，升冪或降冪（Strategy）
2. **編輯**：刪除、複製／貼上（Clone + Command）
3. **標籤**：Urgent（紅）、Work（藍）、Personal（綠），支援多重標籤
4. **Undo / Redo**：操作可復原與重做（CommandHistory）

Web UI 為預設加分交付：`FileTree`、`VisitorPanel`、`TraverseConsole`，進階功能再加工具列。運算在後端。

## 7. API 契約（Web 階段）

Visitor 結果必須帶遍歷紀錄，例如：

```csharp
record VisitorResultDto<T>(T Data, IReadOnlyList<string> TraverseLog);
```

參考端點：

| 方法 | 路徑 | 說明 |
|---|---|---|
| GET | `/api/tree` | 整棵樹 |
| POST | `/api/visitors/size` | 總容量 + traverseLog |
| POST | `/api/visitors/search` | 副檔名搜尋 + traverseLog |
| POST | `/api/visitors/xml` | XML 字串 + traverseLog |

進階功能再補 delete、paste、sort、tag、undo、redo。啟動時載入 Seed，Root 置於記憶體單例。

## 8. UML 與 ER

- 類別圖：`docs/class-diagram.md`（Mermaid classDiagram）— 繼承、關聯／聚合須可見
- ER：`docs/er-model.md`（Mermaid erDiagram）— 目錄自參照、檔案隸屬目錄且不可缺目錄、Word／Image／Text 子型別；標籤可畫於進階資料

ER 為作業要求的 Schema 設計文件，與記憶體實作並存。

## 9. 開發生命週期

順序：Domain TDD → Console Demo → docs／README → Minimal API → Vue + Axios → 進階功能。

對應的 commit 主題（僅在明確要求時提交）：

1. `docs:` 規格與 Cursor 規則
2. `feat(domain):`／`feat(console):` 必備功能（此時已符合作業核心）
3. `feat(web):` Minimal API
4. `feat(frontend):` Vue
5. `feat:` 進階功能

訊息格式：Conventional Commits，說明使用繁體中文。
