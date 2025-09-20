# Come Eseguire ExcelViewer

## 🚀 Metodi per Avviare l'Applicazione

### 1. Da Terminale (Consigliato per Debug)
```bash
cd /data/repos/apps-desktop/excel-viewer
dotnet run --project src/ExcelViewer.UI.Avalonia/ExcelViewer.UI.Avalonia.csproj
```

### 2. Da VSCode

#### Metodo A: Configurazione launch.json
Crea il file `.vscode/launch.json`:
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Launch ExcelViewer",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/src/ExcelViewer.UI.Avalonia/bin/Debug/net8.0/ExcelViewer.UI.Avalonia.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole"
        }
    ]
}
```

E `.vscode/tasks.json`:
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/src/ExcelViewer.UI.Avalonia/ExcelViewer.UI.Avalonia.csproj"
            ],
            "problemMatcher": "$msCompile"
        }
    ]
}
```

Poi premi **F5** per avviare con debug.

#### Metodo B: Terminale Integrato VSCode
1. Apri il terminale in VSCode (`Ctrl+` `)
2. Esegui: `dotnet run --project src/ExcelViewer.UI.Avalonia/ExcelViewer.UI.Avalonia.csproj`

## 📁 Formati File Supportati

### ✅ Attualmente Supportati:
- **Excel 2007+** (.xlsx) - Formato moderno
- **Excel 97-2003** (.xls) - Formato legacy

### ❌ NON Supportati:
- **LibreOffice Calc** (.ods, .odf)
- **CSV** (.csv)
- **TSV** (.tsv)
- **Google Sheets** (export richiesto)

## 🆕 Come Aggiungere Supporto CSV

Per supportare file CSV, dovresti:

1. Modificare il file picker per accettare .csv:
```csharp
// In AvaloniaFilePickerService.cs
Patterns = new[] { "*.xlsx", "*.xls", "*.csv" }
```

2. Aggiungere logica di lettura CSV in ExcelReaderService:
```csharp
if (Path.GetExtension(filePath).ToLower() == ".csv")
{
    return ReadCsvFile(filePath);
}
```

3. Implementare un metodo ReadCsvFile che converte CSV in DataTable

## 📥 File Excel di Esempio

### Dove Trovare File di Test:

1. **Microsoft Office Templates**:
   - https://templates.office.com/ (scarica esempi gratuiti)

2. **Sample Excel Files**:
   - https://file-examples.com/format/xlsx
   - https://sample-videos.com/download-sample-xls.php

3. **Crea File di Test con LibreOffice**:
   - Apri LibreOffice Calc
   - Inserisci dati di esempio
   - **Salva come**: File → Salva con nome → Tipo: **Microsoft Excel 2007-365 (.xlsx)**

4. **Converti ODS in XLSX**:
   - Apri il tuo file .ods in LibreOffice
   - File → Salva con nome → Seleziona .xlsx come formato

## 🧪 File di Test Rapido

Crea un file Excel di test velocemente:

### Con LibreOffice:
1. Apri LibreOffice Calc
2. Inserisci:
   - A1: "Nome"
   - B1: "Cognome"
   - C1: "Età"
   - A2: "Mario"
   - B2: "Rossi"
   - C2: "30"
3. Salva come `test.xlsx`

### Con Python (se installato):
```python
import pandas as pd

df = pd.DataFrame({
    'Nome': ['Mario', 'Luigi', 'Peach'],
    'Cognome': ['Rossi', 'Verdi', 'Principessa'],
    'Età': [30, 28, 25]
})

df.to_excel('test.xlsx', index=False)
```

## 🐛 Troubleshooting

### Se l'app non parte:
1. Verifica che .NET 8 sia installato: `dotnet --version`
2. Pulisci e ricompila: `dotnet clean && dotnet build`
3. Controlla i log nella console per errori

### Se non riesce a leggere file:
1. Verifica che il file sia .xlsx o .xls
2. Il file non deve essere aperto in un altro programma
3. Controlla i permessi del file

### Per debug avanzato:
```bash
dotnet run --project src/ExcelViewer.UI.Avalonia/ExcelViewer.UI.Avalonia.csproj --verbosity detailed
```

## 💡 Suggerimento per Testing

Per testare rapidamente senza file Excel:
1. Scarica un esempio: `wget https://file-examples.com/storage/fe1170c2816762d3831be20/2017/02/file_example_XLSX_10.xlsx`
2. O converti un CSV in XLSX con LibreOffice
3. O esporta da Google Sheets come .xlsx

---

**Note**: L'applicazione è cross-platform e dovrebbe funzionare su Windows, Linux e macOS con .NET 8 installato.